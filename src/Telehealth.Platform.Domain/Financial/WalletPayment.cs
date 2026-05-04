using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Financial;

public class WalletPayment : Entity<Guid>
{
    public Guid WalletId { get; private set; }
    public long AmountMinor { get; private set; }
    public string Currency { get; private set; } = "USD";
    public string PaymentMethod { get; private set; } = string.Empty;
    public string ExternalPaymentId { get; private set; } = string.Empty;
    public WalletPaymentStatus Status { get; private set; } = WalletPaymentStatus.Pending;
    public string StatusDetails { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    private WalletPayment(
        Guid id,
        Guid walletId,
        long amountMinor,
        string currency,
        string paymentMethod,
        string externalPaymentId) : base(id)
    {
        WalletId = walletId;
        AmountMinor = amountMinor;
        Currency = currency;
        PaymentMethod = paymentMethod;
        ExternalPaymentId = externalPaymentId;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public static WalletPayment Create(
        Guid walletId,
        long amountMinor,
        string currency,
        string paymentMethod,
        string externalPaymentId)
    {
        return new WalletPayment(Guid.NewGuid(), walletId, amountMinor, currency, paymentMethod, externalPaymentId);
    }

    public void MarkCompleted()
    {
        Status = WalletPaymentStatus.Completed;
        CompletedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkFailed(string reason)
    {
        Status = WalletPaymentStatus.Failed;
        StatusDetails = reason;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkPending(string details)
    {
        Status = WalletPaymentStatus.Pending;
        StatusDetails = details;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;
}

public enum WalletPaymentStatus
{
    Pending = 1,
    Processing = 2,
    Completed = 3,
    Failed = 4,
    Cancelled = 5,
    Refunded = 6
}