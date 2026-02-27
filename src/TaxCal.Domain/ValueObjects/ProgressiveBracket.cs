namespace TaxCal.Domain.ValueObjects;

/// <summary>One bracket for progressive tax: apply rate to amount above threshold.</summary>
public sealed record ProgressiveBracket(decimal Threshold, decimal RatePercent);
