namespace TaxCal.Application.Interfaces;

/// <summary>
/// Extension point for external tax rule resolution. When a country has no manual configuration, the application layer may call this to obtain a rule from an external source.
/// Not used in MVP; no implementation required. When Domain defines the rule type (e.g. CountryTaxRule), implementations may return that type; callers will use it to run calculation.
/// </summary>
public interface IExternalTaxRuleProvider
{
    /// <summary>
    /// Try to resolve a tax rule for the given country from an external source.
    /// </summary>
    /// <param name="countryCode">Country code (e.g. DE, ES).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A rule representation if available; otherwise null. When Domain.CountryTaxRule exists, implementations may return that type; until then, callers may receive a placeholder or null.</returns>
    Task<object?> TryGetRuleAsync(string countryCode, CancellationToken cancellationToken = default);
}
