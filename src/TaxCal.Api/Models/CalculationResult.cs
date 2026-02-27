namespace TaxCal.Api.Models;

/// <summary>Result of a tax calculation.</summary>
public class CalculationResult
{
    /// <summary>Gross salary (input).</summary>
    public decimal Gross { get; set; }

    /// <summary>Taxable base after fixed deductions.</summary>
    public decimal TaxableBase { get; set; }

    /// <summary>Total taxes (sum of all tax items).</summary>
    public decimal TotalTaxes { get; set; }

    /// <summary>Breakdown per tax item.</summary>
    public List<TaxBreakdownItem> Breakdown { get; set; } = new();

    /// <summary>Net salary (gross minus total taxes).</summary>
    public decimal NetSalary { get; set; }
}

/// <summary>One line in the tax breakdown.</summary>
public class TaxBreakdownItem
{
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
