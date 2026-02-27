namespace TaxCal.Domain.Tests;

using TaxCal.Domain.Entities;
using TaxCal.Domain.Services;
using TaxCal.Domain.ValueObjects;

public class TaxCalculatorTests
{
    [Fact]
    public void Calculate_FixedOnly_ComputesTaxableBaseAndNet()
    {
        var rule = new CountryTaxRule("DE", new List<TaxItem>
        {
            new(TaxItemType.Fixed, "Solidarity", 100, null, null)
        });
        var result = TaxCalculator.Calculate(60_000, rule);

        Assert.Equal(60_000, result.Gross);
        Assert.Equal(59_900, result.TaxableBase);
        Assert.Equal(100, result.TotalTaxes);
        Assert.Single(result.Breakdown);
        Assert.Equal("Solidarity", result.Breakdown[0].Name);
        Assert.Equal(100, result.Breakdown[0].Amount);
        Assert.Equal(59_900, result.NetSalary);
    }

    [Fact]
    public void Calculate_FlatRateOnly_AppliesRateToGross()
    {
        var rule = new CountryTaxRule("DE", new List<TaxItem>
        {
            new(TaxItemType.FlatRate, "VAT", null, 10, null)
        });
        var result = TaxCalculator.Calculate(1_000, rule);

        Assert.Equal(1_000, result.Gross);
        Assert.Equal(1_000, result.TaxableBase);
        Assert.Equal(100, result.TotalTaxes);
        Assert.Single(result.Breakdown);
        Assert.Equal(900, result.NetSalary);
    }

    [Fact]
    public void Calculate_ProgressiveOnly_AppliesBrackets()
    {
        var rule = new CountryTaxRule("DE", new List<TaxItem>
        {
            new(TaxItemType.Progressive, "Income", null, null, new[]
            {
                new ProgressiveBracket(0, 10),
                new ProgressiveBracket(10_000, 20)
            })
        });
        var result = TaxCalculator.Calculate(15_000, rule);

        Assert.Equal(15_000, result.TaxableBase);
        // 0–10k at 10% = 1000; 10k–15k at 20% = 1000; total 2000
        Assert.Equal(2_000, result.TotalTaxes);
        Assert.Equal(13_000, result.NetSalary);
    }

    [Fact]
    public void Calculate_MixedRule_ComputesCorrectTotalAndBreakdown()
    {
        var rule = new CountryTaxRule("DE", new List<TaxItem>
        {
            new(TaxItemType.Fixed, "Solidarity", 200, null, null),
            new(TaxItemType.FlatRate, "Community", null, 5, null),
            new(TaxItemType.Progressive, "Income", null, null, new[] { new ProgressiveBracket(0, 10) })
        });
        var gross = 10_000m;
        var result = TaxCalculator.Calculate(gross, rule);

        Assert.Equal(10_000, result.Gross);
        Assert.Equal(9_800, result.TaxableBase); // 10000 - 200
        var flatAmount = 9_800m * 0.05m;
        var progressiveAmount = 9_800m * 0.10m;
        Assert.Equal(200 + flatAmount + progressiveAmount, result.TotalTaxes);
        Assert.Equal(3, result.Breakdown.Count);
        Assert.Equal("Solidarity", result.Breakdown[0].Name);
        Assert.Equal(200, result.Breakdown[0].Amount);
        Assert.Equal("Community", result.Breakdown[1].Name);
        Assert.Equal(490, result.Breakdown[1].Amount); // 9800 * 5%
        Assert.Equal("Income", result.Breakdown[2].Name);
        Assert.Equal(980, result.Breakdown[2].Amount); // 9800 * 10%
        Assert.Equal(gross - result.TotalTaxes, result.NetSalary);
    }

    [Fact]
    public void Calculate_SameInputs_ProducesSameOutputs_Deterministic()
    {
        var rule = new CountryTaxRule("ES", new List<TaxItem>
        {
            new(TaxItemType.FlatRate, "VAT", null, 21, null)
        });
        var a = TaxCalculator.Calculate(50_000, rule);
        var b = TaxCalculator.Calculate(50_000, rule);

        Assert.Equal(a.Gross, b.Gross);
        Assert.Equal(a.TaxableBase, b.TaxableBase);
        Assert.Equal(a.TotalTaxes, b.TotalTaxes);
        Assert.Equal(a.NetSalary, b.NetSalary);
    }

    [Fact]
    public void Calculate_WhenNegativeGross_ThrowsArgumentOutOfRangeException()
    {
        var rule = new CountryTaxRule("DE", new List<TaxItem>
        {
            new(TaxItemType.FlatRate, "VAT", null, 10, null)
        });
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => TaxCalculator.Calculate(-1000, rule));
        Assert.Equal("gross", ex.ParamName);
    }

    [Fact]
    public void Calculate_WhenFixedExceedsGross_TaxableBaseIsZero()
    {
        var rule = new CountryTaxRule("DE", new List<TaxItem>
        {
            new(TaxItemType.Fixed, "Fee", 5_000, null, null)
        });
        var result = TaxCalculator.Calculate(3_000, rule);

        Assert.Equal(3_000, result.Gross);
        Assert.Equal(0, result.TaxableBase);
        Assert.Equal(5_000, result.TotalTaxes);
        Assert.Equal(-2_000, result.NetSalary);
    }
}
