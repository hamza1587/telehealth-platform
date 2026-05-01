using Telehealth.Platform.Domain.Common;
using Telehealth.Platform.Domain.Consultations;

namespace Telehealth.Platform.Domain.Doctors;

public sealed class DoctorAvailabilityWindow : Entity<Guid>
{
    public DoctorAvailabilityWindow(
        Guid id,
        Guid doctorProfileId,
        DateTimeOffset startsAt,
        DateTimeOffset endsAt,
        ConsultationMode consultationMode,
        bool isInstantEnabled,
        DateTimeOffset createdAt)
        : base(id)
    {
        DoctorProfileId = doctorProfileId;
        StartsAt = startsAt;
        EndsAt = endsAt;
        ConsultationMode = consultationMode;
        IsInstantEnabled = isInstantEnabled;
        CreatedAt = createdAt;
    }

    public Guid DoctorProfileId { get; }

    public DateTimeOffset StartsAt { get; }

    public DateTimeOffset EndsAt { get; }

    public ConsultationMode ConsultationMode { get; }

    public bool IsInstantEnabled { get; }

    public DateTimeOffset CreatedAt { get; }
}
