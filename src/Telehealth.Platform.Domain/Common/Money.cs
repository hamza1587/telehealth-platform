namespace Telehealth.Platform.Domain.Common;

public sealed record Money(long MinorUnits, string Currency)
{
    public static Money Zero(string currency) => new(0, currency);
}
