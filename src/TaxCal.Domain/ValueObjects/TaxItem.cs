namespace TaxCal.Domain.ValueObjects;

/// <summary>One tax item: fixed amount, flat-rate percentage, or progressive brackets.</summary>
public sealed record TaxItem(
    TaxItemType Type,
    string Name,
    decimal? Amount,
    decimal? RatePercent,
    IReadOnlyList<ProgressiveBracket>? Brackets);
