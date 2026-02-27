namespace TaxCal.Api.Mapping;

using TaxCal.Api.Models;
using TaxCal.Domain.Results;

/// <summary>Maps domain TaxCalculationResult to API CalculationResult (camelCase in JSON).</summary>
public static class CalculationResultMapper
{
    public static CalculationResult ToApi(TaxCalculationResult domain)
    {
        if (domain == null)
            throw new ArgumentNullException(nameof(domain));

        return new CalculationResult
        {
            Gross = domain.Gross,
            TaxableBase = domain.TaxableBase,
            TotalTaxes = domain.TotalTaxes,
            Breakdown = domain.Breakdown
                .Select(b => new TaxBreakdownItem { Name = b.Name, Amount = b.Amount })
                .ToList(),
            NetSalary = domain.NetSalary
        };
    }
}
