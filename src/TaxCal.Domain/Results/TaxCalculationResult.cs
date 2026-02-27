namespace TaxCal.Domain.Results;

/// <summary>Result of a pure tax calculation: gross, taxable base, total taxes, per-item breakdown, net salary.</summary>
public sealed record TaxCalculationResult(
    decimal Gross,
    decimal TaxableBase,
    decimal TotalTaxes,
    IReadOnlyList<TaxBreakdownEntry> Breakdown,
    decimal NetSalary);

/// <summary>One line in the tax breakdown (name and amount).</summary>
public sealed record TaxBreakdownEntry(string Name, decimal Amount);
