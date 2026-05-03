using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.InstantConsultation;

public enum QueueEntryStatus
{
    Waiting,
    Matched,
    Started,
    Completed,
    Expired,
    Cancelled
}

public class InstantQueueEntry : Entity<Guid>
{
    public Guid PatientAccountId { get; private set; }

    public Guid? DoctorProfileId { get; private set; }

    public DateTimeOffset JoinedAt { get; private set; }

    public DateTimeOffset? MatchedAt { get; private set; }

    public DateTimeOffset? StartedAt { get; private set; }

    public DateTimeOffset ExpiresAt { get; private set; }

    public QueueEntryStatus Status { get; private set; } = QueueEntryStatus.Waiting;

    public int Priority { get; private set; } = 1;

    public string? Reason { get; private set; }

    private InstantQueueEntry(Guid id, Guid patientAccountId) : base(id)
    {
        PatientAccountId = patientAccountId;
        JoinedAt = DateTimeOffset.UtcNow;
        ExpiresAt = JoinedAt.AddMinutes(10);
    }

    public static InstantQueueEntry Create(Guid patientAccountId, string? reason = null)
    {
        var entry = new InstantQueueEntry(Guid.NewGuid(), patientAccountId)
        {
            Reason = reason
        };

        return entry;
    }

    public void Match(Guid doctorProfileId)
    {
        DoctorProfileId = doctorProfileId;
        Status = QueueEntryStatus.Matched;
        MatchedAt = DateTimeOffset.UtcNow;
    }

    public void Start()
    {
        Status = QueueEntryStatus.Started;
        StartedAt = DateTimeOffset.UtcNow;
    }

    public void Complete()
    {
        Status = QueueEntryStatus.Completed;
    }

    public void Expire()
    {
        Status = QueueEntryStatus.Expired;
    }

    public void Cancel()
    {
        Status = QueueEntryStatus.Cancelled;
    }

    public bool IsExpired => DateTimeOffset.UtcNow > ExpiresAt;
}