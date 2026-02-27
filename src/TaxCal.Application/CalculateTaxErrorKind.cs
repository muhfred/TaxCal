namespace TaxCal.Application;

/// <summary>Kind of error returned when tax calculation cannot be performed (used to map to HTTP 404 vs 400).</summary>
public enum CalculateTaxErrorKind
{
    /// <summary>Country has no tax configuration.</summary>
    CountryNotConfigured,

    /// <summary>Validation failed (e.g. empty country code, negative gross).</summary>
    Validation
}
