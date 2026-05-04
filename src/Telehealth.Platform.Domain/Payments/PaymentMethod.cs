using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Payments;

public enum PaymentMethodType
{
    CreditCard = 1,
    DebitCard = 2,
    DigitalWallet = 3,
    BankTransfer = 4,
    PayPal = 5,
    ApplePay = 6,
    GooglePay = 7
}

public enum CardBrand
{
    Visa,
    Mastercard,
    AmericanExpress,
    Discover,
    Other
}

public sealed class PaymentMethod : Entity<Guid>
{
    private PaymentMethod(
        Guid id,
        Guid patientAccountId,
        PaymentMethodType type,
        string provider,
        string token,
        string lastFour,
        string brand,
        DateTimeOffset expiryMonth,
        bool isDefault,
        bool isVerified) : base(id)
    {
        PatientAccountId = patientAccountId;
        Type = type;
        Provider = provider;
        Token = token;
        LastFour = lastFour;
        Brand = brand;
        ExpiryMonth = expiryMonth;
        IsDefault = isDefault;
        IsVerified = isVerified;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid PatientAccountId { get; private set; }
    public PaymentMethodType Type { get; private set; }
    public string Provider { get; private set; } = string.Empty;
    public string Token { get; private set; } = string.Empty;
    public string LastFour { get; private set; } = string.Empty;
    public string Brand { get; private set; } = string.Empty;
    public DateTimeOffset ExpiryMonth { get; private set; }
    public bool IsDefault { get; private set; }
    public bool IsVerified { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public static PaymentMethod CreateCard(
        Guid patientAccountId,
        PaymentMethodType type,
        string provider,
        string token,
        string lastFour,
        string brand,
        DateTimeOffset expiryMonth,
        bool isDefault = false)
    {
        return new PaymentMethod(
            Guid.NewGuid(),
            patientAccountId,
            type,
            provider,
            token,
            lastFour,
            brand,
            expiryMonth,
            isDefault,
            false);
    }

    public static PaymentMethod CreateWallet(
        Guid patientAccountId,
        PaymentMethodType type,
        string provider,
        string token)
    {
        return new PaymentMethod(
            Guid.NewGuid(),
            patientAccountId,
            type,
            provider,
            token,
            string.Empty,
            string.Empty,
            DateTimeOffset.MinValue,
            false,
            true);
    }

    public static PaymentMethod CreateBankTransfer(
        Guid patientAccountId,
        string provider,
        string token,
        string lastFour)
    {
        return new PaymentMethod(
            Guid.NewGuid(),
            patientAccountId,
            PaymentMethodType.BankTransfer,
            provider,
            token,
            lastFour,
            string.Empty,
            DateTimeOffset.MinValue,
            false,
            false);
    }

    public void SetAsDefault()
    {
        IsDefault = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Verify()
    {
        IsVerified = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Delete()
    {
        DeletedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}