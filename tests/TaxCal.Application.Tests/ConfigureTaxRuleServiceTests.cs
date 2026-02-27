namespace TaxCal.Application.Tests;

using Xunit;
using TaxCal.Application.Interfaces;
using TaxCal.Application.Services;
using TaxCal.Domain.Entities;
using TaxCal.Domain.ValueObjects;

public class ConfigureTaxRuleServiceTests
{
    [Fact]
    public async Task ConfigureAsync_WhenZeroItems_ReturnsError()
    {
        var repo = new FakeTaxRuleRepository();
        var service = new ConfigureTaxRuleService(repo);
        var rule = new CountryTaxRule("DE", []);

        var error = await service.ConfigureAsync(rule, TestContext.Current.CancellationToken);

        Assert.NotNull(error);
        Assert.Contains("at least one", error, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0, repo.SaveCallCount);
    }

    [Fact]
    public async Task ConfigureAsync_WhenTwoProgressive_ReturnsError()
    {
        var repo = new FakeTaxRuleRepository();
        var service = new ConfigureTaxRuleService(repo);
        var items = new List<TaxItem>
        {
            new(TaxItemType.Progressive, "Income", null, null, [new ProgressiveBracket(0, 10)]),
            new(TaxItemType.Progressive, "Other", null, null, [new ProgressiveBracket(1000, 20)])
        };
        var rule = new CountryTaxRule("DE", items);

        var error = await service.ConfigureAsync(rule, TestContext.Current.CancellationToken);

        Assert.NotNull(error);
        Assert.Contains("at most one progressive", error, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0, repo.SaveCallCount);
    }

    [Fact]
    public async Task ConfigureAsync_WhenValid_SavesAndReturnsNull()
    {
        var repo = new FakeTaxRuleRepository();
        var service = new ConfigureTaxRuleService(repo);
        var items = new List<TaxItem>
        {
            new(TaxItemType.Fixed, "Solidarity", 100, null, null),
            new(TaxItemType.FlatRate, "Community", null, 5, null)
        };
        var rule = new CountryTaxRule("DE", items);

        var error = await service.ConfigureAsync(rule, TestContext.Current.CancellationToken);

        Assert.Null(error);
        Assert.Equal(1, repo.SaveCallCount);
        Assert.Same(rule, repo.LastSavedRule);
        Assert.Equal("DE", repo.LastCountryCode);
    }

    [Fact]
    public async Task ConfigureAsync_WhenOneProgressive_Saves()
    {
        var repo = new FakeTaxRuleRepository();
        var service = new ConfigureTaxRuleService(repo);
        var items = new List<TaxItem>
        {
            new(TaxItemType.Progressive, "Income", null, null, [new ProgressiveBracket(0, 10), new ProgressiveBracket(10000, 20)])
        };
        var rule = new CountryTaxRule("ES", items);

        var error = await service.ConfigureAsync(rule, TestContext.Current.CancellationToken);

        Assert.Null(error);
        Assert.Equal(1, repo.SaveCallCount);
    }

    [Fact]
    public async Task ConfigureAsync_WhenFixedWithoutAmount_ReturnsError()
    {
        var repo = new FakeTaxRuleRepository();
        var service = new ConfigureTaxRuleService(repo);
        var items = new List<TaxItem> { new(TaxItemType.Fixed, "Fee", null, null, null) };
        var rule = new CountryTaxRule("DE", items);

        var error = await service.ConfigureAsync(rule, TestContext.Current.CancellationToken);

        Assert.NotNull(error);
        Assert.Contains("Amount", error, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0, repo.SaveCallCount);
    }

    [Fact]
    public async Task ConfigureAsync_WhenFlatRateOutOfRange_ReturnsError()
    {
        var repo = new FakeTaxRuleRepository();
        var service = new ConfigureTaxRuleService(repo);
        var items = new List<TaxItem> { new(TaxItemType.FlatRate, "VAT", null, 150, null) };
        var rule = new CountryTaxRule("DE", items);

        var error = await service.ConfigureAsync(rule, TestContext.Current.CancellationToken);

        Assert.NotNull(error);
        Assert.Contains("0 and 100", error);
        Assert.Equal(0, repo.SaveCallCount);
    }

    [Fact]
    public async Task ConfigureAsync_WhenProgressiveWithoutBrackets_ReturnsError()
    {
        var repo = new FakeTaxRuleRepository();
        var service = new ConfigureTaxRuleService(repo);
        var items = new List<TaxItem> { new(TaxItemType.Progressive, "Income", null, null, null) };
        var rule = new CountryTaxRule("DE", items);

        var error = await service.ConfigureAsync(rule, TestContext.Current.CancellationToken);

        Assert.NotNull(error);
        Assert.Contains("at least one bracket", error, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0, repo.SaveCallCount);
    }

    private sealed class FakeTaxRuleRepository : ITaxRuleRepository
    {
        public int SaveCallCount { get; private set; }
        public string? LastCountryCode { get; private set; }
        public CountryTaxRule? LastSavedRule { get; private set; }

        public Task<CountryTaxRule?> GetByCountryCodeAsync(string countryCode, CancellationToken cancellationToken = default) =>
            Task.FromResult<CountryTaxRule?>(null);

        public Task SaveOrReplaceAsync(string countryCode, CountryTaxRule rule, CancellationToken cancellationToken = default)
        {
            SaveCallCount++;
            LastCountryCode = countryCode;
            LastSavedRule = rule;
            return Task.CompletedTask;
        }
    }
}
