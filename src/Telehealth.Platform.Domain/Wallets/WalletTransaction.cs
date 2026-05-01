using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Wallets;

/// <summary>
/// Transaction record for wallet operations.
/// </summary>
public sealed class WalletTransaction : Entity<Guid>
{
    public WalletTransaction(
        Guid id,
        Guid walletId,
        Guid patientAccountId,
        TransactionType type,
        decimal amount,
        string currency,
        decimal balanceAfter,
        string? description,
        string? referenceId,
        string? paymentMethod,
        DateTimeOffset createdAt)
        : base(id)
    {
        WalletId = walletId;
        PatientAccountId = patientAccountId;
        Type = type;
        Amount = amount;
        Currency = currency;
        BalanceAfter = balanceAfter;
        Description = description;
        ReferenceId = referenceId;
        PaymentMethod = paymentMethod;
        Status = TransactionStatus.Completed;
        CreatedAt = createdAt;
    }

    public Guid WalletId { get; }
    public Guid PatientAccountId { get; }
    public TransactionType Type { get; }
    public decimal Amount { get; }
    public string Currency { get; }
    public decimal BalanceAfter { get; }
    public TransactionStatus Status { get; private set; }
    public string? Description { get; }
    public string? ReferenceId { get; }
    public string? PaymentMethod { get; }
    public string? FailureReason { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset? ProcessedAt { get; private set; }

    public void MarkAsFailed(string reason, DateTimeOffset processedAt)
    {
        Status = TransactionStatus.Failed;
        FailureReason = reason;
        ProcessedAt = processedAt;
    }

    public void MarkAsPending()
    {
        Status = TransactionStatus.Pending;
    }

    public void Complete(DateTimeOffset processedAt)
    {
        Status = TransactionStatus.Completed;
        ProcessedAt = processedAt;
    }

    public bool IsCredit()
    {
        return Type is TransactionType.Deposit or TransactionType.Refund or TransactionType.Bonus;
    }

    public bool IsDebit()
    {
        return Type is TransactionType.Spend or TransactionType.Withdrawal or TransactionType.Fee;
    }
}

public enum TransactionType
{
    Deposit,
    Spend,
    Refund,
    Withdrawal,
    Bonus,
    Fee,
    Adjustment,
    DayPassPurchase,
    DayPassRefund
}

public enum TransactionStatus
{
    Pending,
    Completed,
    Failed,
    Cancelled,
    Reversed
}
