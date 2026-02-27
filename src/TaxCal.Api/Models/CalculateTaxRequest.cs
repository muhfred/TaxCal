namespace TaxCal.Api.Models;

/// <summary>Request to calculate tax for a country and gross salary.</summary>
public class CalculateTaxRequest
{
    /// <summary>Country code (e.g. DE, ES, US).</summary>
    public string CountryCode { get; set; } = string.Empty;

    /// <summary>Gross salary (annual).</summary>
    public decimal GrossSalary { get; set; }
}
