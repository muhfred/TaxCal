namespace TaxCal.Application.Interfaces;

using TaxCal.Application;
using TaxCal.Domain.Results;

/// <summary>Calculates tax for a country and gross salary; returns result or a structured error (not configured vs validation).</summary>
public interface ICalculateTaxService
{
    /// <summary>Gets the rule for the country, calculates tax, and returns the result or (ErrorKind, message) when failing.</summary>
    Task<(TaxCalculationResult? Result, CalculateTaxErrorKind? ErrorKind, string? ErrorMessage)> CalculateAsync(string countryCode, decimal grossSalary, CancellationToken cancellationToken = default);
}
