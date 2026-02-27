using Microsoft.AspNetCore.Mvc;
using TaxCal.Api.Mapping;
using TaxCal.Api.Models;
using TaxCal.Api.Validation;
using TaxCal.Application;
using TaxCal.Application.Interfaces;

namespace TaxCal.Api.Controllers;

/// <summary>Calculate tax for a country and gross salary.</summary>
[ApiController]
[Route("api/taxes")]
public class TaxCalculationController : ControllerBase
{
    private readonly ICalculateTaxService _calculateService;

    public TaxCalculationController(ICalculateTaxService calculateService)
    {
        _calculateService = calculateService ?? throw new ArgumentNullException(nameof(calculateService));
    }

    /// <summary>Calculate tax for the given country and gross salary.</summary>
    /// <param name="request">Country code and gross salary (annual).</param>
    /// <response code="200">Calculation result (gross, taxableBase, totalTaxes, breakdown, netSalary).</response>
    /// <response code="400">Validation error (e.g. negative gross, empty country code).</response>
    /// <response code="404">No tax configuration for the country.</response>
    [HttpPost]
    [ProducesResponseType(typeof(CalculationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CalculationResult>> Calculate([FromBody] CalculateTaxRequest? request, CancellationToken cancellationToken)
    {
        if (request == null)
            return BadRequest(ApiProblemDetailsFactory.Validation(StatusCodes.Status400BadRequest, "Request body is required."));

        var countryCodeError = CountryCodeValidation.Validate(request.CountryCode);
        if (countryCodeError != null)
            return BadRequest(ApiProblemDetailsFactory.Validation(StatusCodes.Status400BadRequest, countryCodeError));

        var countryCode = (request.CountryCode ?? string.Empty).Trim();
        var (result, errorKind, errorMessage) = await _calculateService.CalculateAsync(countryCode, request.GrossSalary, cancellationToken).ConfigureAwait(false);

        if (errorKind != null)
        {
            var detail = errorMessage ?? "Calculation failed.";
            return errorKind == CalculateTaxErrorKind.CountryNotConfigured
                ? NotFound(ApiProblemDetailsFactory.NotFound(detail))
                : BadRequest(ApiProblemDetailsFactory.Validation(StatusCodes.Status400BadRequest, detail));
        }

        return Ok(CalculationResultMapper.ToApi(result!));
    }
}
