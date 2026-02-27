namespace TaxCal.Application.Interfaces;

using TaxCal.Domain.Entities;

/// <summary>Validates and persists a country tax rule (at least one item, at most one progressive).</summary>
public interface IConfigureTaxRuleService
{
    /// <summary>Validates the rule and saves or replaces it for the country. Returns null on success, or an error message on validation failure.</summary>
    Task<string?> ConfigureAsync(CountryTaxRule rule, CancellationToken cancellationToken = default);
}
