using Microsoft.AspNetCore.Mvc;

namespace TaxCal.Api;

/// <summary>Builds consistent 4xx error responses (RFC 7807–style problem details) for all API endpoints.</summary>
public static class ApiProblemDetailsFactory
{
    private const string BaseType = "https://api.taxcal/errors/";

    /// <summary>Creates a problem details instance for validation errors (400).</summary>
    public static ProblemDetails Validation(int statusCode, string detail)
    {
        return new ProblemDetails
        {
            Type = BaseType + "validation",
            Title = "Validation Error",
            Status = statusCode,
            Detail = detail
        };
    }

    /// <summary>Creates a problem details instance for resource not found (404), e.g. country not configured.</summary>
    public static ProblemDetails NotFound(string detail)
    {
        return new ProblemDetails
        {
            Type = BaseType + "not-found",
            Title = "Not Found",
            Status = StatusCodes.Status404NotFound,
            Detail = detail
        };
    }
}
