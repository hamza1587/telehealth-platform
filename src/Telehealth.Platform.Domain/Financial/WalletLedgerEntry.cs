using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Financial;

public class WalletLedgerEntry : Entity<Guid>
{
    public Guid WalletId { get; private set; }
    public string EntryType { get; private set; } = string.Empty;
    public long SecondsDelta { get; private set; }
    public long BalanceAfterSeconds { get; private set; }
    public string ReferenceType { get; private set; } = string.Empty;
    public string ReferenceId { get; private set; } = string.Empty;
    public string Reason { get; private set; } = string.Empty;
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }

    private WalletLedgerEntry(
        Guid id,
        Guid walletId,
        string entryType,
        long secondsDelta,
        long balanceAfterSeconds,
        string referenceType,
        string referenceId,
        string reason,
        string createdBy) : base(id)
    {
        WalletId = walletId;
        EntryType = entryType;
        SecondsDelta = secondsDelta;
        BalanceAfterSeconds = balanceAfterSeconds;
        ReferenceType = referenceType;
        ReferenceId = referenceId;
        Reason = reason;
        CreatedBy = createdBy;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public static WalletLedgerEntry Create(
        Guid walletId,
        string entryType,
        long secondsDelta,
        long balanceAfterSeconds,
        string referenceType,
        string referenceId,
        string reason,
        string createdBy)
    {
        return new WalletLedgerEntry(
            Guid.NewGuid(),
            walletId,
            entryType,
            secondsDelta,
            balanceAfterSeconds,
            referenceType,
            referenceId,
            reason,
            createdBy);
    }
}

public static class LedgerEntryTypes
{
    public const string CreditPurchased = "CreditPurchased";
    public const string CreditReserved = "CreditReserved";
    public const string CreditReservationReleased = "CreditReservationReleased";
    public const string ConsultationCharged = "ConsultationCharged";
    public const string RefundGranted = "RefundGranted";
    public const string PromotionalCreditGranted = "PromotionalCreditGranted";
    public const string AdminAdjustment = "AdminAdjustment";
    public const string Expiry = "Expiry";
}