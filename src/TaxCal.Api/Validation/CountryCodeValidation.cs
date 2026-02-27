namespace TaxCal.Api.Validation;

/// <summary>Validates country code for API requests (ISO 3166-1 alpha-2 style: two ASCII letters).</summary>
public static class CountryCodeValidation
{
    /// <summary>Validates country code. Returns an error message if invalid, or null if valid.</summary>
    /// <param name="countryCode">Raw country code (will be trimmed).</param>
    /// <returns>Null if valid; otherwise a clear error message for 400 response.</returns>
    public static string? Validate(string? countryCode)
    {
        var trimmed = (countryCode ?? string.Empty).Trim();
        if (trimmed.Length == 0)
            return "Country code is required.";
        if (trimmed.Length != 2)
            return "Country code must be two letters (e.g. DE, ES).";
        foreach (var c in trimmed)
        {
            if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))
                continue;
            return "Country code must be two letters (e.g. DE, ES).";
        }
        return null;
    }
}
