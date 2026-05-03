using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Wallets;

public class Wallet : Entity<Guid>
{
    public Guid PatientAccountId { get; private set; }

    public long BalanceMinor { get; private set; }

    public string Currency { get; private set; } = "USD";

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    private Wallet(Guid id, Guid patientAccountId) : base(id)
    {
        PatientAccountId = patientAccountId;
        BalanceMinor = 0;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public static Wallet Create(Guid patientAccountId)
    {
        return new Wallet(Guid.NewGuid(), patientAccountId);
    }

    public void AddCredit(long amountMinor)
    {
        BalanceMinor += amountMinor;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public bool HasSufficientFunds(long amountMinor)
    {
        return BalanceMinor >= amountMinor;
    }

    public bool Deduct(long amountMinor)
    {
        if (!HasSufficientFunds(amountMinor))
            return false;

        BalanceMinor -= amountMinor;
        UpdatedAt = DateTimeOffset.UtcNow;
        return true;
    }
}