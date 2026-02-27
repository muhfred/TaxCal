namespace TaxCal.Application.Interfaces;

using TaxCal.Domain.Entities;

/// <summary>Repository for storing and retrieving tax rules per country (one rule per country; save overwrites).</summary>
public interface ITaxRuleRepository
{
    /// <summary>Gets the tax rule for the given country code, or null if none is configured.</summary>
    Task<CountryTaxRule?> GetByCountryCodeAsync(string countryCode, CancellationToken cancellationToken = default);

    /// <summary>Saves or replaces the tax rule for the given country code. Process-scoped only; no persistence.</summary>
    Task SaveOrReplaceAsync(string countryCode, CountryTaxRule rule, CancellationToken cancellationToken = default);
}
