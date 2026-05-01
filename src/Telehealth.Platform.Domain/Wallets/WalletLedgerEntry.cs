using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Wallets;

public sealed class WalletLedgerEntry : Entity<Guid>
{
    public WalletLedgerEntry(
        Guid id,
        Guid walletId,
        WalletLedgerEntryType entryType,
        long secondsDelta,
        long balanceAfterSeconds,
        string referenceType,
        string referenceId,
        string reason,
        DateTimeOffset createdAt)
        : base(id)
    {
        WalletId = walletId;
        EntryType = entryType;
        SecondsDelta = secondsDelta;
        BalanceAfterSeconds = balanceAfterSeconds;
        ReferenceType = referenceType;
        ReferenceId = referenceId;
        Reason = reason;
        CreatedAt = createdAt;
    }

    public Guid WalletId { get; }

    public WalletLedgerEntryType EntryType { get; }

    public long SecondsDelta { get; }

    public long BalanceAfterSeconds { get; }

    public string ReferenceType { get; }

    public string ReferenceId { get; }

    public string Reason { get; }

    public DateTimeOffset CreatedAt { get; }
}
