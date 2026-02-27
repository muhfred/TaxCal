namespace TaxCal.Application.Services;

using TaxCal.Application;
using TaxCal.Application.Interfaces;
using TaxCal.Domain.Results;
using TaxCal.Domain.Services;

/// <summary>Resolves rule by country, runs TaxCalculator, returns result or structured error (not configured vs validation).</summary>
public sealed class CalculateTaxService : ICalculateTaxService
{
    private readonly ITaxRuleRepository _repository;

    public CalculateTaxService(ITaxRuleRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <inheritdoc />
    public async Task<(TaxCalculationResult? Result, CalculateTaxErrorKind? ErrorKind, string? ErrorMessage)> CalculateAsync(string countryCode, decimal grossSalary, CancellationToken cancellationToken = default)
    {
        var code = (countryCode ?? string.Empty).Trim();
        if (code.Length == 0)
            return (null, CalculateTaxErrorKind.Validation, "Country code is required.");

        var rule = await _repository.GetByCountryCodeAsync(code, cancellationToken).ConfigureAwait(false);
        if (rule == null)
            return (null, CalculateTaxErrorKind.CountryNotConfigured, $"No tax configuration for country {code}.");

        try
        {
            var result = TaxCalculator.Calculate(grossSalary, rule);
            return (result, null, null);
        }
        catch (ArgumentOutOfRangeException ex) when (ex.ParamName == "gross")
        {
            return (null, CalculateTaxErrorKind.Validation, ex.Message);
        }
    }
}
