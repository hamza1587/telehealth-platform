using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Payments;

public sealed class Refund : Entity<Guid>
{
    private Refund(
        Guid id,
        Guid paymentId,
        long amountMinor,
        string currency,
        string reason,
        RefundStatus status) : base(id)
    {
        PaymentId = paymentId;
        AmountMinor = amountMinor;
        Currency = currency;
        Reason = reason;
        Status = status;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid PaymentId { get; private set; }
    public long AmountMinor { get; private set; }
    public string Currency { get; private set; } = "USD";
    public string Reason { get; private set; } = string.Empty;
    public RefundStatus Status { get; private set; } = RefundStatus.Initiated;
    public string ExternalRefundId { get; private set; } = string.Empty;
    public string StatusDetails { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public static Refund Create(Guid paymentId, long amountMinor, string currency, string reason)
    {
        return new Refund(Guid.NewGuid(), paymentId, amountMinor, currency, reason, RefundStatus.Initiated);
    }

    public void SetStatus(RefundStatus status, string details = "")
    {
        Status = status;
        StatusDetails = details;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkProcessing()
    {
        Status = RefundStatus.Processing;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkCompleted(string externalRefundId)
    {
        Status = RefundStatus.Completed;
        ExternalRefundId = externalRefundId;
        CompletedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkFailed(string details)
    {
        Status = RefundStatus.Failed;
        StatusDetails = details;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkCancelled()
    {
        Status = RefundStatus.Cancelled;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}