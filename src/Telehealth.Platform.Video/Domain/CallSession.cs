namespace Telehealth.Platform.Video.Domain;

public enum CallSessionStatus
{
    Waiting,
    Active,
    Ended,
    Failed
}

public class CallSession
{
    public Guid Id { get; private set; }

    /// <summary>References the platform consultation booking that spawned this call.</summary>
    public Guid ConsultationId { get; private set; }

    public Guid RoomId { get; private set; }
    public VideoRoom Room { get; private set; } = null!;

    /// <summary>Identity of the participant who initiated the call (doctor or patient user ID).</summary>
    public string InitiatedBy { get; private set; } = string.Empty;

    public CallSessionStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? StartedAt { get; private set; }
    public DateTimeOffset? EndedAt { get; private set; }

    public CallSession() { }

    public CallSession(Guid consultationId, Guid roomId, string initiatedBy)
    {
        Id = Guid.NewGuid();
        ConsultationId = consultationId;
        RoomId = roomId;
        InitiatedBy = initiatedBy;
        Status = CallSessionStatus.Waiting;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void Start()
    {
        Status = CallSessionStatus.Active;
        StartedAt = DateTimeOffset.UtcNow;
    }

    public void End()
    {
        Status = CallSessionStatus.Ended;
        EndedAt = DateTimeOffset.UtcNow;
    }

    public void Fail()
    {
        Status = CallSessionStatus.Failed;
        EndedAt = DateTimeOffset.UtcNow;
    }
}
