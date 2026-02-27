namespace TaxCal.Application.Tests;

using Xunit;
using TaxCal.Application;
using TaxCal.Application.Interfaces;
using TaxCal.Application.Services;
using TaxCal.Domain.Entities;
using TaxCal.Domain.ValueObjects;

public class CalculateTaxServiceTests
{
    [Fact]
    public async Task CalculateAsync_WhenNoRule_ReturnsCountryNotConfigured()
    {
        var repo = new FakeTaxRuleRepository();
        var service = new CalculateTaxService(repo);
        var (result, errorKind, errorMessage) = await service.CalculateAsync("XX", 50_000, TestContext.Current.CancellationToken);

        Assert.Null(result);
        Assert.Equal(CalculateTaxErrorKind.CountryNotConfigured, errorKind);
        Assert.NotNull(errorMessage);
        Assert.Contains("No tax configuration", errorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("XX", errorMessage);
    }

    [Fact]
    public async Task CalculateAsync_WhenRuleExists_ReturnsResult()
    {
        var rule = new CountryTaxRule("DE", new List<TaxItem>
        {
            new(TaxItemType.FlatRate, "VAT", null, 10, null)
        });
        var repo = new FakeTaxRuleRepository();
        repo.SetRule("DE", rule);
        var service = new CalculateTaxService(repo);

        var (result, errorKind, errorMessage) = await service.CalculateAsync("DE", 10_000, TestContext.Current.CancellationToken);

        Assert.Null(errorKind);
        Assert.NotNull(result);
        Assert.Equal(10_000, result.Gross);
        Assert.Equal(10_000, result.TaxableBase);
        Assert.Equal(1_000, result.TotalTaxes);
        Assert.Equal(9_000, result.NetSalary);
    }

    [Fact]
    public async Task CalculateAsync_WhenNegativeGross_ReturnsError()
    {
        var rule = new CountryTaxRule("DE", new List<TaxItem>
        {
            new(TaxItemType.Fixed, "Fee", 100, null, null)
        });
        var repo = new FakeTaxRuleRepository();
        repo.SetRule("DE", rule);
        var service = new CalculateTaxService(repo);

        var (result, errorKind, errorMessage) = await service.CalculateAsync("DE", -1000, TestContext.Current.CancellationToken);

        Assert.Null(result);
        Assert.Equal(CalculateTaxErrorKind.Validation, errorKind);
        Assert.NotNull(errorMessage);
    }

    [Fact]
    public async Task CalculateAsync_WhenEmptyCountryCode_ReturnsValidationError()
    {
        var repo = new FakeTaxRuleRepository();
        var service = new CalculateTaxService(repo);
        var (result, errorKind, errorMessage) = await service.CalculateAsync("  ", 50_000, TestContext.Current.CancellationToken);

        Assert.Null(result);
        Assert.Equal(CalculateTaxErrorKind.Validation, errorKind);
        Assert.Contains("Country code", errorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class FakeTaxRuleRepository : ITaxRuleRepository
    {
        private CountryTaxRule? _rule;
        private string? _countryCode;

        public void SetRule(string countryCode, CountryTaxRule rule)
        {
            _countryCode = countryCode;
            _rule = rule;
        }

        public Task<CountryTaxRule?> GetByCountryCodeAsync(string countryCode, CancellationToken cancellationToken = default)
        {
            if (_rule != null && string.Equals(_countryCode, countryCode, StringComparison.OrdinalIgnoreCase))
                return Task.FromResult<CountryTaxRule?>(_rule);
            return Task.FromResult<CountryTaxRule?>(null);
        }

        public Task SaveOrReplaceAsync(string countryCode, CountryTaxRule rule, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }
}
