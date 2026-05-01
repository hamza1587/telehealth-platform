using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Billing;

/// <summary>
/// Billing record for a consultation session.
/// </summary>
public sealed class ConsultationBilling : Entity<Guid>
{
    public ConsultationBilling(
        Guid id,
        Guid consultationSessionId,
        Guid patientAccountId,
        Guid doctorProfileId,
        Money pricePerSecond,
        long actualDurationSeconds,
        Money baseAmount,
        Money platformFee,
        Money doctorPayout,
        Money totalCharged,
        string currency,
        DateTimeOffset createdAt)
        : base(id)
    {
        ConsultationSessionId = consultationSessionId;
        PatientAccountId = patientAccountId;
        DoctorProfileId = doctorProfileId;
        PricePerSecond = pricePerSecond;
        ActualDurationSeconds = actualDurationSeconds;
        BaseAmount = baseAmount;
        PlatformFee = platformFee;
        DoctorPayout = doctorPayout;
        TotalCharged = totalCharged;
        Currency = currency;
        Status = BillingStatus.Pending;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public Guid ConsultationSessionId { get; }
    public Guid PatientAccountId { get; }
    public Guid DoctorProfileId { get; }
    public Money PricePerSecond { get; }
    public long ActualDurationSeconds { get; }
    public Money BaseAmount { get; }
    public Money PlatformFee { get; }
    public Money DoctorPayout { get; }
    public Money TotalCharged { get; private set; }
    public string Currency { get; }
    public BillingStatus Status { get; private set; }
    public string? PaymentMethod { get; private set; }
    public string? TransactionReference { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? PaidAt { get; private set; }
    public DateTimeOffset? RefundedAt { get; private set; }
    public Money? RefundAmount { get; private set; }
    public string? RefundReason { get; private set; }

    public void MarkAsPaid(string paymentMethod, string transactionReference, DateTimeOffset paidAt)
    {
        Status = BillingStatus.Paid;
        PaymentMethod = paymentMethod;
        TransactionReference = transactionReference;
        PaidAt = paidAt;
        UpdatedAt = paidAt;
    }

    public void MarkAsFailed(string reason, DateTimeOffset failedAt)
    {
        Status = BillingStatus.Failed;
        UpdatedAt = failedAt;
    }

    public void ProcessRefund(Money refundAmount, string reason, DateTimeOffset refundedAt)
    {
        RefundAmount = refundAmount;
        RefundReason = reason;
        RefundedAt = refundedAt;
        Status = BillingStatus.Refunded;
        UpdatedAt = refundedAt;
    }

    public void ApplyDiscount(Money discountAmount, string discountCode, DateTimeOffset updatedAt)
    {
        TotalCharged = new Money(TotalCharged.Amount - discountAmount.Amount, Currency);
        UpdatedAt = updatedAt;
    }

    public Money CalculatePlatformFeePercentage()
    {
        if (BaseAmount.Amount == 0) return new Money(0, Currency);
        var percentage = PlatformFee.Amount / BaseAmount.Amount * 100;
        return new Money(percentage, Currency);
    }
}

public enum BillingStatus
{
    Pending,
    Processing,
    Paid,
    Failed,
    Disputed,
    Refunded,
    PartiallyRefunded
}
