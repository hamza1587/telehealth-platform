using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Billing;

public sealed class BillingSession : Entity<Guid>
{
    public BillingSession(
        Guid id,
        Guid consultationSessionId,
        Guid walletId,
        Guid doctorProfileId,
        long billableSeconds,
        Money grossAmount,
        Money platformFee,
        Money doctorEarning)
        : base(id)
    {
        ConsultationSessionId = consultationSessionId;
        WalletId = walletId;
        DoctorProfileId = doctorProfileId;
        BillableSeconds = billableSeconds;
        GrossAmount = grossAmount;
        PlatformFee = platformFee;
        DoctorEarning = doctorEarning;
        Status = BillingSessionStatus.Pending;
    }

    public Guid ConsultationSessionId { get; }

    public Guid WalletId { get; }

    public Guid DoctorProfileId { get; }

    public long BillableSeconds { get; }

    public Money GrossAmount { get; }

    public Money PlatformFee { get; }

    public Money DoctorEarning { get; }

    public BillingSessionStatus Status { get; private set; }
}
