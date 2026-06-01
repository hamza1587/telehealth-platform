using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Payments;

public sealed class PaymentTransaction : Entity<Guid>
{
    private PaymentTransaction(
        Guid id,
        Guid paymentId,
        Guid patientAccountId,
        string provider,
        string providerTransactionId,
        long amountMinor,
        PaymentTransactionStatus status) : base(id)
    {
        PaymentId = paymentId;
        PatientAccountId = patientAccountId;
        Provider = provider;
        ProviderTransactionId = providerTransactionId;
        AmountMinor = amountMinor;
        Status = status;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid PaymentId { get; private set; }
    public Guid PatientAccountId { get; private set; }
    public string Provider { get; private set; }
    public string ProviderTransactionId { get; private set; }
    public long AmountMinor { get; private set; }
    public PaymentTransactionStatus Status { get; private set; }
    public string StatusDetails { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public static PaymentTransaction Create(
        Guid paymentId,
        Guid patientAccountId,
        string provider,
        string providerTransactionId,
        long amountMinor)
    {
        return new PaymentTransaction(
            Guid.NewGuid(),
            paymentId,
            patientAccountId,
            provider,
            providerTransactionId,
            amountMinor,
            PaymentTransactionStatus.Pending);
    }

    public void SetStatus(PaymentTransactionStatus status, string details = "")
    {
        Status = status;
        StatusDetails = details;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Complete()
    {
        Status = PaymentTransactionStatus.Completed;
        CompletedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Fail(string details)
    {
        Status = PaymentTransactionStatus.Failed;
        StatusDetails = details;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}