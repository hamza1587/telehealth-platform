using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Financial;

public class Wallet : Entity<Guid>
{
    public Guid PatientAccountId { get; private set; }
    public string Currency { get; private set; } = "USD";
    public long AvailableSeconds { get; private set; }
    public long ReservedSeconds { get; private set; }
    public WalletStatus Status { get; private set; } = WalletStatus.Active;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public long RowVersion { get; private set; }

    private Wallet(
        Guid id,
        Guid patientAccountId,
        string currency) : base(id)
    {
        PatientAccountId = patientAccountId;
        Currency = currency;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public static Wallet Create(Guid patientAccountId, string currency = "USD")
    {
        return new Wallet(Guid.NewGuid(), patientAccountId, currency);
    }

    public void AddSeconds(long seconds)
    {
        AvailableSeconds += seconds;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ReserveSeconds(long seconds)
    {
        if (AvailableSeconds < seconds)
            throw new InvalidOperationException("Insufficient balance");
        
        AvailableSeconds -= seconds;
        ReservedSeconds += seconds;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ReleaseReservation(long seconds)
    {
        ReservedSeconds -= seconds;
        AvailableSeconds += seconds;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ChargeSeconds(long seconds)
    {
        ReservedSeconds -= seconds;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetStatus(WalletStatus status)
    {
        Status = status;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}

public enum WalletStatus
{
    Active = 1,
    Suspended = 2,
    Closed = 3
}