using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Consultations;

public enum ParticipantEventType
{
    Joined,
    Left,
    Disconnected,
    Reconnected,
    DeviceFailure,
    NetworkFailure
}

public sealed class ParticipantEvent : Entity<Guid>
{
    public ParticipantEvent(
        Guid id,
        Guid consultationSessionId,
        ParticipantEventType eventType,
        DateTimeOffset occurredAt)
        : base(id)
    {
        ConsultationSessionId = consultationSessionId;
        EventType = eventType;
        OccurredAt = occurredAt;
    }

    public Guid ConsultationSessionId { get; }

    public ParticipantEventType EventType { get; private set; }

    public DateTimeOffset OccurredAt { get; private set; }

    public string? ParticipantType { get; private set; }

    public string? ParticipantId { get; private set; }

    public Dictionary<string, object>? Metadata { get; private set; }

    public ConsultationSession? Session { get; private set; }

    public void SetParticipant(string? participantType, string? participantId)
    {
        ParticipantType = participantType;
        ParticipantId = participantId;
    }

    public void SetMetadata(Dictionary<string, object> metadata)
    {
        Metadata = metadata;
    }
}