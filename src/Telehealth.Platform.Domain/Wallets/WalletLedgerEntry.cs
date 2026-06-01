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

    public Guid WalletId { get; private set; }

    public WalletLedgerEntryType EntryType { get; private set; }

    public long SecondsDelta { get; private set; }

    public long BalanceAfterSeconds { get; private set; }

    public string ReferenceType { get; private set; }

    public string ReferenceId { get; private set; }

    public string Reason { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
}
