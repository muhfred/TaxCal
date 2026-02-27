namespace TaxCal.Api.Mapping;

using TaxCal.Api.Models;
using TaxCal.Domain.Entities;
using TaxCal.Domain.ValueObjects;

/// <summary>Maps configure-rule API request to domain model. Returns null if request is invalid.</summary>
public static class ConfigureRuleMapper
{

    /// <summary>Maps request to domain model with a specific error message when invalid (for 400 detail).</summary>
    public static (CountryTaxRule? Rule, string? ErrorMessage) TryToDomain(ConfigureRuleRequest? request)
    {
        if (request == null)
            return (null, "Request body is required.");

        var countryCode = (request.CountryCode ?? string.Empty).Trim();
        if (countryCode.Length == 0)
            return (null, "Country code is required.");

        if (request.TaxItems == null || request.TaxItems.Count == 0)
            return (null, "At least one tax item is required.");

        var items = new List<TaxItem>();
        for (var i = 0; i < request.TaxItems.Count; i++)
        {
            var item = ToTaxItem(request.TaxItems[i]);
            if (item == null)
                return (null, $"Tax item at index {i} has invalid type or missing parameters. Use Fixed, FlatRate, or Progressive with the required fields.");
            items.Add(item);
        }

        return (new CountryTaxRule(countryCode, items), null);
    }

    /// <summary>Maps request to CountryTaxRule. Returns null if country code is missing or any tax item type is invalid.</summary>
    public static CountryTaxRule? ToDomain(ConfigureRuleRequest? request)
    {
        var (rule, _) = TryToDomain(request);
        return rule;
    }

    private static TaxItem? ToTaxItem(TaxItemRequest? req)
    {
        if (req == null)
            return null;

        var type = ParseType(req.Type ?? string.Empty);
        if (type == null)
            return null;

        var name = (req.Name ?? string.Empty).Trim();
        if (name.Length == 0)
            name = type.Value.ToString();

        IReadOnlyList<ProgressiveBracket>? brackets = null;
        if (type == TaxItemType.Progressive && req.Brackets != null && req.Brackets.Count > 0)
        {
            brackets = req.Brackets
                .Select(b => new ProgressiveBracket(b.Threshold, b.RatePercent))
                .ToList();
        }

        return new TaxItem(
            type.Value,
            name,
            req.Amount,
            req.RatePercent,
            brackets);
    }

    private static TaxItemType? ParseType(string type)
    {
        return type.Trim().ToUpperInvariant() switch
        {
            "FIXED" => TaxItemType.Fixed,
            "FLATRATE" or "FLAT RATE" => TaxItemType.FlatRate,
            "PROGRESSIVE" => TaxItemType.Progressive,
            _ => null
        };
    }
}
