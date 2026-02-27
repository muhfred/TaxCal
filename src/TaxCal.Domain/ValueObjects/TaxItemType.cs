namespace TaxCal.Domain.ValueObjects;

/// <summary>Type of tax item: fixed amount, flat-rate percentage, or progressive brackets.</summary>
public enum TaxItemType
{
    Fixed,
    FlatRate,
    Progressive
}
