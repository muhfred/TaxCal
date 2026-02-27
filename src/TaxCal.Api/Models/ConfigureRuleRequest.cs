namespace TaxCal.Api.Models;

/// <summary>Request to configure or replace the tax rule for a country.</summary>
public class ConfigureRuleRequest
{
    /// <summary>Country code (e.g. ISO 3166-1 alpha-2: DE, ES, US).</summary>
    public string CountryCode { get; set; } = string.Empty;

    /// <summary>At least one tax item is required. At most one item may be progressive.</summary>
    public List<TaxItemRequest> TaxItems { get; set; } = new();
}

/// <summary>One tax item: fixed amount, flat-rate percentage, or progressive brackets.</summary>
public class TaxItemRequest
{
    /// <summary>Type of tax: Fixed, FlatRate, or Progressive.</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>Display name for the tax (e.g. "CommunityTax").</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Fixed amount (for Type=Fixed).</summary>
    public decimal? Amount { get; set; }

    /// <summary>Percentage 0–100 (for Type=FlatRate).</summary>
    public decimal? RatePercent { get; set; }

    /// <summary>Progressive brackets: threshold and rate (for Type=Progressive).</summary>
    public List<ProgressiveBracketRequest>? Brackets { get; set; }
}

/// <summary>One bracket for progressive tax: apply rate to amount above threshold.</summary>
public class ProgressiveBracketRequest
{
    public decimal Threshold { get; set; }
    public decimal RatePercent { get; set; }
}
