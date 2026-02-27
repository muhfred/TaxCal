namespace TaxCal.Application.Services;

using TaxCal.Application.Interfaces;
using TaxCal.Domain.Entities;
using TaxCal.Domain.ValueObjects;

/// <summary>Validates tax rule (at least one item, at most one progressive) and saves via repository.</summary>
public sealed class ConfigureTaxRuleService : IConfigureTaxRuleService
{
    private readonly ITaxRuleRepository _repository;

    public ConfigureTaxRuleService(ITaxRuleRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <inheritdoc />
    public async Task<string?> ConfigureAsync(CountryTaxRule rule, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(rule);

        if (rule.TaxItems.Count == 0)
            return "At least one tax item is required.";

        var progressiveCount = rule.TaxItems.Count(i => i.Type == TaxItemType.Progressive);
        if (progressiveCount > 1)
            return "At most one progressive tax item is allowed per country.";

        var typeError = ValidateTypeSpecificParameters(rule.TaxItems);
        if (typeError != null)
            return typeError;

        await _repository.SaveOrReplaceAsync(rule.CountryCode, rule, cancellationToken).ConfigureAwait(false);
        return null;
    }

    private static string? ValidateTypeSpecificParameters(IReadOnlyList<TaxItem> items)
    {
        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];
            switch (item.Type)
            {
                case TaxItemType.Fixed:
                    if (!item.Amount.HasValue)
                        return $"Fixed tax item '{item.Name}' must have an Amount.";
                    if (item.Amount.Value < 0)
                        return $"Fixed tax item '{item.Name}' must have a non-negative Amount.";
                    break;
                case TaxItemType.FlatRate:
                    if (!item.RatePercent.HasValue)
                        return $"Flat-rate tax item '{item.Name}' must have a RatePercent.";
                    if (item.RatePercent.Value < 0 || item.RatePercent.Value > 100)
                        return $"Flat-rate tax item '{item.Name}' must have RatePercent between 0 and 100.";
                    break;
                case TaxItemType.Progressive:
                    if (item.Brackets == null || item.Brackets.Count == 0)
                        return $"Progressive tax item '{item.Name}' must have at least one bracket.";
                    break;
            }
        }
        return null;
    }
}
