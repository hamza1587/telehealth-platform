using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Doctors;

public class DoctorAvailabilityWindow : Entity<Guid>
{
    public Guid DoctorProfileId { get; private set; }
    public DateTimeOffset StartsAt { get; private set; }
    public DateTimeOffset EndsAt { get; private set; }
    public string ConsultationMode { get; private set; } = string.Empty;
    public bool IsInstantEnabled { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private DoctorAvailabilityWindow(
        Guid id,
        Guid doctorProfileId,
        DateTimeOffset startsAt,
        DateTimeOffset endsAt,
        string consultationMode,
        bool isInstantEnabled) : base(id)
    {
        DoctorProfileId = doctorProfileId;
        StartsAt = startsAt;
        EndsAt = endsAt;
        ConsultationMode = consultationMode;
        IsInstantEnabled = isInstantEnabled;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public static DoctorAvailabilityWindow Create(
        Guid doctorProfileId,
        DateTimeOffset startsAt,
        DateTimeOffset endsAt,
        string consultationMode,
        bool isInstantEnabled = false)
    {
        return new DoctorAvailabilityWindow(
            Guid.NewGuid(),
            doctorProfileId,
            startsAt,
            endsAt,
            consultationMode,
            isInstantEnabled);
    }

    public bool IsWithinWindow(DateTimeOffset dateTime)
    {
        return dateTime >= StartsAt && dateTime <= EndsAt;
    }
}