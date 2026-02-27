namespace TaxCal.Api.Tests;

using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using TaxCal.Api.Models;

/// <summary>Integration tests for POST /api/tax/calculate.
/// Note: Tests share the same in-memory rule store (single WebApplicationFactory instance). Each test that needs a specific configuration sets it up (e.g. configure DE then calculate). The "not configured" test uses country "XX" which is not configured by other tests.</summary>
public class TaxCalculationControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public TaxCalculationControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Calculate_WhenEmptyCountryCode_Returns400()
    {
        var request = new CalculateTaxRequest { CountryCode = "", GrossSalary = 50_000 };
        var response = await _client.PostAsJsonAsync("/api/taxes", request, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(problem?.Detail);
        Assert.Contains("Country code", problem.Detail, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Story 4.2: negative gross returns 400 with clear message (no 500).</summary>
    [Fact]
    public async Task Calculate_WhenNegativeGross_Returns400WithClearMessage()
    {
        await _client.PostAsJsonAsync("/api/taxes/rules", new ConfigureRuleRequest
        {
            CountryCode = "DE",
            TaxItems = new List<TaxItemRequest> { new() { Type = "Fixed", Name = "F", Amount = 0 } }
        }, cancellationToken: TestContext.Current.CancellationToken);
        var request = new CalculateTaxRequest { CountryCode = "DE", GrossSalary = -1000 };
        var response = await _client.PostAsJsonAsync("/api/taxes", request, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(problem?.Detail);
        Assert.Contains("Gross", problem.Detail, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("non-negative", problem.Detail, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Story 4.2: invalid country code format returns 400 with clear message.</summary>
    [Fact]
    public async Task Calculate_WhenInvalidCountryCodeFormat_Returns400WithClearMessage()
    {
        var request = new CalculateTaxRequest { CountryCode = "D", GrossSalary = 50_000 };
        var response = await _client.PostAsJsonAsync("/api/taxes", request, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(problem?.Detail);
        Assert.Contains("two letters", problem.Detail, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Calculate_WhenCountryNotConfigured_Returns404()
    {
        var request = new CalculateTaxRequest { CountryCode = "XX", GrossSalary = 50_000 };
        var response = await _client.PostAsJsonAsync("/api/taxes", request, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(problem?.Detail);
        Assert.Contains("No tax configuration", problem.Detail, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Story 4.1: 4xx responses use consistent structure (type, title, status, detail).</summary>
    [Fact]
    public async Task ErrorResponses_400_ReturnConsistentProblemDetailsShape_TypeTitleStatusDetail()
    {
        var request = new CalculateTaxRequest { CountryCode = "", GrossSalary = 50_000 };
        var response = await _client.PostAsJsonAsync("/api/taxes", request, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(problem);
        Assert.NotNull(problem.Type);
        Assert.Contains("validation", problem.Type, StringComparison.OrdinalIgnoreCase);
        Assert.NotNull(problem.Title);
        Assert.Equal(400, problem.Status);
        Assert.NotNull(problem.Detail);
    }

    /// <summary>Story 4.1: 404 responses use same consistent structure (type, title, status, detail).</summary>
    [Fact]
    public async Task ErrorResponses_404_ReturnConsistentProblemDetailsShape_TypeTitleStatusDetail()
    {
        var request = new CalculateTaxRequest { CountryCode = "XX", GrossSalary = 50_000 };
        var response = await _client.PostAsJsonAsync("/api/taxes", request, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(problem);
        Assert.NotNull(problem.Type);
        Assert.Contains("not-found", problem.Type, StringComparison.OrdinalIgnoreCase);
        Assert.NotNull(problem.Title);
        Assert.Equal(404, problem.Status);
        Assert.NotNull(problem.Detail);
    }

    /// <summary>Story 3.2: configured country returns 200 with result. Kept alongside FullFlow_* (3.3) for traceability; both configure DE then calculate with slightly different gross.</summary>
    [Fact]
    public async Task Calculate_WhenConfigured_Returns200WithResult()
    {
        await _client.PostAsJsonAsync("/api/taxes/rules", new ConfigureRuleRequest
        {
            CountryCode = "DE",
            TaxItems = new List<TaxItemRequest>
            {
                new() { Type = "Fixed", Name = "Solidarity", Amount = 100 },
                new() { Type = "FlatRate", Name = "Community", RatePercent = 5 }
            }
        }, cancellationToken: TestContext.Current.CancellationToken);

        var request = new CalculateTaxRequest { CountryCode = "DE", GrossSalary = 60_000 };
        var response = await _client.PostAsJsonAsync("/api/taxes", request, cancellationToken: TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<CalculationResult>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(result);
        Assert.Equal(60_000, result.Gross);
        Assert.Equal(59_900, result.TaxableBase);
        Assert.Equal(100 + 59_900 * 0.05m, result.TotalTaxes);
        Assert.Equal(2, result.Breakdown.Count);
        Assert.Equal(60_000 - result.TotalTaxes, result.NetSalary);
    }

    /// <summary>Full flow per Story 3.3: configure a country with tax items, then calculate; assert expected taxable base, total taxes, and net.</summary>
    [Fact]
    public async Task FullFlow_ConfigureDE_ThenCalculate_AssertsExpectedTaxableBaseTotalTaxesAndNet()
    {
        await _client.PostAsJsonAsync("/api/taxes/rules", new ConfigureRuleRequest
        {
            CountryCode = "DE",
            TaxItems = new List<TaxItemRequest>
            {
                new() { Type = "Fixed", Name = "Solidarity", Amount = 100 },
                new() { Type = "FlatRate", Name = "Community", RatePercent = 5 }
            }
        }, cancellationToken: TestContext.Current.CancellationToken);

        var request = new CalculateTaxRequest { CountryCode = "DE", GrossSalary = 62_000 };
        var response = await _client.PostAsJsonAsync("/api/taxes", request, cancellationToken: TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<CalculationResult>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(result);
        const decimal expectedTaxableBase = 61_900m;  // 62000 - 100
        const decimal expectedTotalTaxes = 100m + 61_900m * 0.05m;  // 3195
        const decimal expectedNet = 62_000m - expectedTotalTaxes;    // 58805

        Assert.Equal(62_000, result.Gross);
        Assert.Equal(expectedTaxableBase, result.TaxableBase);
        Assert.Equal(expectedTotalTaxes, result.TotalTaxes);
        Assert.Equal(expectedNet, result.NetSalary);
        Assert.Equal(2, result.Breakdown.Count);
        Assert.Contains(result.Breakdown, b => b.Name == "Solidarity" && b.Amount == 100);
        Assert.Contains(result.Breakdown, b => b.Name == "Community" && b.Amount == 3095);
    }

    /// <summary>Story 3.3 optional: full flow with progressive tax item; asserts 200 and breakdown includes progressive.</summary>
    [Fact]
    public async Task FullFlow_ConfigureWithProgressive_ThenCalculate_Returns200WithExpectedBreakdown()
    {
        await _client.PostAsJsonAsync("/api/taxes/rules", new ConfigureRuleRequest
        {
            CountryCode = "ES",
            TaxItems = new List<TaxItemRequest>
            {
                new() { Type = "Fixed", Name = "Fee", Amount = 100 },
                new() { Type = "Progressive", Name = "Income", Brackets = new List<ProgressiveBracketRequest> { new() { Threshold = 0, RatePercent = 10 }, new() { Threshold = 10_000, RatePercent = 20 } } }
            }
        }, cancellationToken: TestContext.Current.CancellationToken);

        var request = new CalculateTaxRequest { CountryCode = "ES", GrossSalary = 15_000 };
        var response = await _client.PostAsJsonAsync("/api/taxes", request, cancellationToken: TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<CalculationResult>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(result);
        Assert.Equal(15_000, result.Gross);
        Assert.Equal(14_900, result.TaxableBase);
        Assert.Equal(2, result.Breakdown.Count);
        Assert.Contains(result.Breakdown, b => b.Name == "Fee" && b.Amount == 100);
        Assert.Contains(result.Breakdown, b => b.Name == "Income");
        Assert.Equal(15_000 - result.TotalTaxes, result.NetSalary);
    }
}
