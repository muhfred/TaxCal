namespace TaxCal.Domain.Entities;

using TaxCal.Domain.ValueObjects;

/// <summary>Tax rule for a country: ordered list of tax items (fixed, flat-rate, progressive).</summary>
public sealed class CountryTaxRule
{
    public string CountryCode { get; }
    public IReadOnlyList<TaxItem> TaxItems { get; }

    public CountryTaxRule(string countryCode, IReadOnlyList<TaxItem> taxItems)
    {
        CountryCode = countryCode ?? throw new ArgumentNullException(nameof(countryCode));
        TaxItems = taxItems ?? throw new ArgumentNullException(nameof(taxItems));
    }
}
