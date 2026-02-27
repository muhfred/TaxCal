using Microsoft.AspNetCore.Mvc;
using TaxCal.Api.Mapping;
using TaxCal.Api.Models;
using TaxCal.Api.Validation;
using TaxCal.Application.Interfaces;

namespace TaxCal.Api.Controllers;

/// <summary>Configure or replace the tax rule for a country.</summary>
[ApiController]
[Route("api/taxes/rules")]
public class TaxRulesController : ControllerBase
{
    private readonly IConfigureTaxRuleService _configureService;

    public TaxRulesController(IConfigureTaxRuleService configureService)
    {
        _configureService = configureService ?? throw new ArgumentNullException(nameof(configureService));
    }

    /// <summary>Configure or replace the tax rule for a country. At least one tax item required; at most one progressive.</summary>
    /// <param name="request">Country code and list of tax items (fixed, flat-rate, progressive).</param>
    /// <remarks>
    /// Sample request:
    ///{
    ///  "countryCode": "DE",
    ///  "taxItems": [
    ///    {
    ///      "type": "Fixed",
    ///      "name": "CommunityTax",
    ///      "amount": 1500
    ///    },
    ///    {
    ///      "type": "Fixed",
    ///      "name": "RadioTax",
    ///      "amount": 500
    ///    },
    ///    {
    ///    "type": "FlatRate",
    ///      "name": "PensionTax",
    ///      "ratePercent": 20
    ///    },
    ///    {
    ///    "type": "FlatRate",
    ///      "name": "HealthInsurance",
    ///      "ratePercent": 7.3
    ///    },
    ///    {
    ///    "type": "Progressive",
    ///      "name": "IncomeTax",
    ///      "brackets": [
    ///        { "threshold": 0, "ratePercent": 0 },
    ///        { "threshold": 11604, "ratePercent": 14 },
    ///        { "threshold": 17005, "ratePercent": 23.97 },
    ///        { "threshold": 66760, "ratePercent": 42 },
    ///        { "threshold": 277825, "ratePercent": 45 }
    ///      ]
    ///    }
    ///  ]
    /// }
    /// </remarks>
    /// <response code="200">Rule configured or replaced.</response>
    /// <response code="400">Validation error (e.g. no tax items, more than one progressive, invalid type).</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfigureRule([FromBody] ConfigureRuleRequest? request, CancellationToken cancellationToken)
    {
        if (request == null)
            return BadRequest(ApiProblemDetailsFactory.Validation(StatusCodes.Status400BadRequest, "Request body is required."));

        var countryCodeError = CountryCodeValidation.Validate(request.CountryCode);
        if (countryCodeError != null)
            return BadRequest(ApiProblemDetailsFactory.Validation(StatusCodes.Status400BadRequest, countryCodeError));

        var (rule, mapperError) = ConfigureRuleMapper.TryToDomain(request);
        if (rule == null)
            return BadRequest(ApiProblemDetailsFactory.Validation(StatusCodes.Status400BadRequest, mapperError ?? "Invalid request: country code and at least one valid tax item (Fixed, FlatRate, or Progressive) are required."));

        var error = await _configureService.ConfigureAsync(rule, cancellationToken).ConfigureAwait(false);
        if (error != null)
            return BadRequest(ApiProblemDetailsFactory.Validation(StatusCodes.Status400BadRequest, error));

        return Ok();
    }
}
