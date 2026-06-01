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

    private BillingSession(Guid id) : base(id) { }

    public Guid ConsultationSessionId { get; private set; } = Guid.Empty;

    public Guid WalletId { get; private set; } = Guid.Empty;

    public Guid DoctorProfileId { get; private set; } = Guid.Empty;

    public long BillableSeconds { get; private set; }

    public Money? GrossAmount { get; private set; }

    public Money? PlatformFee { get; private set; }

    public Money? DoctorEarning { get; private set; }

    public BillingSessionStatus Status { get; private set; } = BillingSessionStatus.Pending;
}
