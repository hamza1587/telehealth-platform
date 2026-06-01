using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Doctors;

/// <summary>
/// Exceptions to the doctor's regular schedule (time off, holidays, etc).
/// </summary>
public sealed class DoctorScheduleException : Entity<Guid>
{
    public DoctorScheduleException(
        Guid id,
        Guid doctorProfileId,
        DateTimeOffset startsAt,
        DateTimeOffset endsAt,
        string reason,
        bool isAllDay,
        DateTimeOffset createdAt)
        : base(id)
    {
        DoctorProfileId = doctorProfileId;
        StartsAt = startsAt;
        EndsAt = endsAt;
        Reason = reason;
        IsAllDay = isAllDay;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public Guid DoctorProfileId { get; private set; }
    public DateTimeOffset StartsAt { get; private set; }
    public DateTimeOffset EndsAt { get; private set; }
    public string Reason { get; private set; }
    public bool IsAllDay { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public void Update(
        DateTimeOffset startsAt,
        DateTimeOffset endsAt,
        string reason,
        bool isAllDay,
        DateTimeOffset updatedAt)
    {
        StartsAt = startsAt;
        EndsAt = endsAt;
        Reason = reason;
        IsAllDay = isAllDay;
        UpdatedAt = updatedAt;
    }
}
