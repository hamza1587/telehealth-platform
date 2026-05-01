using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Wallets;

public sealed class Wallet : Entity<Guid>
{
    public Wallet(Guid id, Guid patientAccountId, string currency)
        : base(id)
    {
        PatientAccountId = patientAccountId;
        Currency = currency;
        Status = WalletStatus.Active;
    }

    public Guid PatientAccountId { get; }

    public string Currency { get; }

    public long AvailableSeconds { get; private set; }

    public long ReservedSeconds { get; private set; }

    public WalletStatus Status { get; private set; }

    public bool CanReserve(long seconds) => Status == WalletStatus.Active && seconds > 0 && AvailableSeconds >= seconds;
}
