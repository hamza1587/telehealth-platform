using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Payments;

public enum PaymentStatus
{
    Pending = 1,
    Processing = 2,
    Authorized = 3,
    Completed = 4,
    Failed = 5,
    Cancelled = 6,
    Refunded = 7,
    Chargeback = 8
}

public enum CurrencyCode
{
    USD = 840,
    EUR = 978,
    GBP = 826,
    CAD = 124,
    AUD = 036
}

public sealed class Payment : Entity<Guid>
{
    private Payment(
        Guid id,
        Guid patientAccountId,
        Guid? paymentMethodId,
        long amountMinor,
        CurrencyCode currency,
        string description,
        string externalPaymentId,
        PaymentStatus status) : base(id)
    {
        PatientAccountId = patientAccountId;
        PaymentMethodId = paymentMethodId;
        AmountMinor = amountMinor;
        Currency = currency;
        Description = description;
        ExternalPaymentId = externalPaymentId;
        Status = status;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid PatientAccountId { get; private set; }
    public Guid? PaymentMethodId { get; private set; }
    public long AmountMinor { get; private set; }
    public CurrencyCode Currency { get; private set; }
    public string Description { get; private set; }
    public string ExternalPaymentId { get; private set; }
    public PaymentStatus Status { get; private set; }
    public string StatusDetails { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public DateTimeOffset? FailedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public static Payment Create(
        Guid patientAccountId,
        Guid? paymentMethodId,
        long amountMinor,
        CurrencyCode currency,
        string description,
        string externalPaymentId = "")
    {
        return new Payment(
            Guid.NewGuid(),
            patientAccountId,
            paymentMethodId,
            amountMinor,
            currency,
            description,
            string.IsNullOrEmpty(externalPaymentId) ? Guid.NewGuid().ToString("N") : externalPaymentId,
            PaymentStatus.Pending);
    }

    public void SetProcessing()
    {
        Status = PaymentStatus.Processing;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetAuthorized()
    {
        Status = PaymentStatus.Authorized;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetCompleted(string externalPaymentId)
    {
        Status = PaymentStatus.Completed;
        ExternalPaymentId = externalPaymentId;
        CompletedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetFailed(string reason)
    {
        Status = PaymentStatus.Failed;
        StatusDetails = reason;
        FailedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetCancelled(string reason)
    {
        Status = PaymentStatus.Cancelled;
        StatusDetails = reason;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetRefunded()
    {
        Status = PaymentStatus.Refunded;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetChargeback()
    {
        Status = PaymentStatus.Chargeback;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateStatusDetails(string details)
    {
        StatusDetails = details;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}