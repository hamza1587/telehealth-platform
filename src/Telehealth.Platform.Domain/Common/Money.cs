namespace Telehealth.Platform.Domain.Common;

public sealed record Money
{
    public long MinorUnits { get; init; }
    public string Currency { get; init; }
    public decimal Amount => MinorUnits / 100m;

    public Money(long minorUnits, string currency)
    {
        MinorUnits = minorUnits;
        Currency = currency;
    }

    public static Money Zero(string currency) => new(0, currency);

    public static Money FromAmount(decimal amount, string currency) => new((long)(amount * 100), currency);
}
