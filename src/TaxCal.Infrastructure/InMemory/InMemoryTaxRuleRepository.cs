namespace TaxCal.Infrastructure.InMemory;

using System.Collections.Concurrent;
using TaxCal.Application.Interfaces;
using TaxCal.Domain.Entities;

/// <summary>In-memory tax rule store: one rule per country; save overwrites. Process-scoped only; no persistence (NFR-S2).</summary>
public sealed class InMemoryTaxRuleRepository : ITaxRuleRepository
{
    private readonly ConcurrentDictionary<string, CountryTaxRule> _store = new(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public Task<CountryTaxRule?> GetByCountryCodeAsync(string countryCode, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var key = NormalizeCountryCode(countryCode);
        _store.TryGetValue(key, out var rule);
        return Task.FromResult(rule);
    }

    /// <inheritdoc />
    public Task SaveOrReplaceAsync(string countryCode, CountryTaxRule rule, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(rule);
        cancellationToken.ThrowIfCancellationRequested();
        var key = NormalizeCountryCode(countryCode);
        var ruleKey = NormalizeCountryCode(rule.CountryCode);
        if (key != ruleKey)
            throw new ArgumentException($"Country code '{countryCode}' does not match rule country code '{rule.CountryCode}'.", nameof(countryCode));
        _store[key] = rule;
        return Task.CompletedTask;
    }

    private static string NormalizeCountryCode(string countryCode) =>
        (countryCode ?? string.Empty).Trim().ToUpperInvariant();
}
