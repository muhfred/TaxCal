namespace TaxCal.Api.Tests;

using System.Net;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using TaxCal.Api.Models;
using TaxCal.Application.Interfaces;

/// <summary>Integration tests for POST /api/tax/rules (configure endpoint).</summary>
public class TaxRulesControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;

    public TaxRulesControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ConfigureRule_ValidRequest_Returns200_AndStoresRule()
    {
        var request = new ConfigureRuleRequest
        {
            CountryCode = "DE",
            TaxItems = new List<TaxItemRequest>
            {
                new() { Type = "Fixed", Name = "Solidarity", Amount = 100 },
                new() { Type = "FlatRate", Name = "Community", RatePercent = 5 }
            }
        };

        var response = await _client.PostAsJsonAsync("/api/taxes/rules", request, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = _factory.Server.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ITaxRuleRepository>();
        var rule = await repo.GetByCountryCodeAsync("DE", TestContext.Current.CancellationToken);
        Assert.NotNull(rule);
        Assert.Equal("DE", rule.CountryCode);
        Assert.Equal(2, rule.TaxItems.Count);
    }

    /// <summary>Story 4.2: null body returns 400 with clear message (no 500).</summary>
    [Fact]
    public async Task ConfigureRule_WhenNullBody_Returns400WithClearMessage()
    {
        var content = new StringContent("null", Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/taxes/rules", content, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(problem?.Detail);
        Assert.Contains("Request body is required", problem.Detail, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Story 4.2 optional: empty POST body returns 400 (no content).</summary>
    [Fact]
    public async Task ConfigureRule_WhenEmptyBody_Returns400()
    {
        var content = new StringContent("", Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/taxes/rules", content, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(problem?.Detail);
    }

    /// <summary>Story 4.2 optional: invalid tax item structure (Fixed without Amount) returns 400 with clear message.</summary>
    [Fact]
    public async Task ConfigureRule_WhenFixedWithoutAmount_Returns400WithClearMessage()
    {
        var request = new ConfigureRuleRequest
        {
            CountryCode = "DE",
            TaxItems = new List<TaxItemRequest> { new() { Type = "Fixed", Name = "Fee" } }
        };
        var response = await _client.PostAsJsonAsync("/api/taxes/rules", request, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(problem?.Detail);
        Assert.Contains("Fixed", problem.Detail, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Amount", problem.Detail, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Story 4.2: invalid country code format returns 400 with clear message.</summary>
    [Fact]
    public async Task ConfigureRule_WhenInvalidCountryCodeFormat_Returns400WithClearMessage()
    {
        var request = new ConfigureRuleRequest
        {
            CountryCode = "X",
            TaxItems = new List<TaxItemRequest> { new() { Type = "Fixed", Name = "F", Amount = 0 } }
        };
        var response = await _client.PostAsJsonAsync("/api/taxes/rules", request, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(problem?.Detail);
        Assert.Contains("two letters", problem.Detail, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Story 4.1: configure endpoint 400 responses use consistent problem-details shape (type, title, status, detail).</summary>
    [Fact]
    public async Task ConfigureRule_WhenInvalidRequest_Returns400_WithConsistentProblemDetailsShape()
    {
        var request = new ConfigureRuleRequest { CountryCode = "DE", TaxItems = new List<TaxItemRequest>() };
        var response = await _client.PostAsJsonAsync("/api/taxes/rules", request, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(problem);
        Assert.NotNull(problem.Type);
        Assert.Contains("validation", problem.Type, StringComparison.OrdinalIgnoreCase);
        Assert.NotNull(problem.Title);
        Assert.Equal(400, problem.Status);
        Assert.NotNull(problem.Detail);
    }
}
