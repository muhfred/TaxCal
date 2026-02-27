namespace TaxCal.Domain.Services;

using TaxCal.Domain.Entities;
using TaxCal.Domain.Results;
using TaxCal.Domain.ValueObjects;

/// <summary>Pure tax calculation: taxable base = gross minus fixed taxes; flat-rate and progressive applied to base. No I/O; deterministic (NFR-V1, NFR-V2).</summary>
public static class TaxCalculator
{
    private const decimal PercentFactor = 100m;

    /// <summary>Computes taxable base, total taxes, per-item breakdown, and net salary from gross and a country rule. Gross must be non-negative.</summary>
    public static TaxCalculationResult Calculate(decimal gross, CountryTaxRule rule)
    {
        ArgumentNullException.ThrowIfNull(rule);
        if (gross < 0)
            throw new ArgumentOutOfRangeException(nameof(gross), gross, "Gross salary must be non-negative.");

        var breakdown = new List<TaxBreakdownEntry>();

        // Fixed: sum and subtract from gross to get taxable base (non-negative)
        var fixedSum = 0m;
        foreach (var item in rule.TaxItems)
        {
            if (item.Type != TaxItemType.Fixed)
                continue;
            var amount = item.Amount ?? 0;
            fixedSum += amount;
            breakdown.Add(new TaxBreakdownEntry(item.Name, amount));
        }

        var taxableBase = Math.Max(0, gross - fixedSum);

        // Flat-rate: apply each to taxable base
        foreach (var item in rule.TaxItems)
        {
            if (item.Type != TaxItemType.FlatRate)
                continue;
            var rate = item.RatePercent ?? 0;
            var amount = taxableBase * (rate / PercentFactor);
            breakdown.Add(new TaxBreakdownEntry(item.Name, amount));
        }

        // Progressive: single item, apply brackets (sorted by threshold) to taxable base
        foreach (var item in rule.TaxItems)
        {
            if (item.Type != TaxItemType.Progressive || item.Brackets == null || item.Brackets.Count == 0)
                continue;
            var progressiveTax = CalculateProgressiveTax(taxableBase, item.Brackets);
            breakdown.Add(new TaxBreakdownEntry(item.Name, progressiveTax));
            break; // at most one progressive
        }

        var totalTaxes = breakdown.Sum(b => b.Amount);
        var netSalary = gross - totalTaxes;

        return new TaxCalculationResult(
            Gross: gross,
            TaxableBase: taxableBase,
            TotalTaxes: totalTaxes,
            Breakdown: [..breakdown],
            NetSalary: netSalary);
    }

    /// <summary>Progressive tax: brackets define (threshold, rate). Amount in each band is taxed at that rate. Brackets assumed ascending by threshold.</summary>
    private static decimal CalculateProgressiveTax(decimal taxableBase, IReadOnlyList<ProgressiveBracket> brackets)
    {
        if (taxableBase <= 0 || brackets.Count == 0)
            return 0;

        var sorted = brackets.OrderBy(b => b.Threshold).ToList();
        decimal tax = 0;
        for (var i = 0; i < sorted.Count; i++)
        {
            var (threshold, ratePercent) = (sorted[i].Threshold, sorted[i].RatePercent);
            var nextThreshold = i < sorted.Count - 1 ? sorted[i + 1].Threshold : taxableBase;
            var amountInBand = Math.Max(0, Math.Min(taxableBase, nextThreshold) - threshold);
            tax += amountInBand * (ratePercent / PercentFactor);
        }
        return tax;
    }
}
