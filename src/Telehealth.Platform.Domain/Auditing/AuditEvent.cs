using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Auditing;

public sealed class AuditEvent : Entity<Guid>
{
    public AuditEvent(
        Guid id,
        string actorId,
        string actorType,
        string action,
        string targetType,
        string targetId,
        DateTimeOffset occurredAt)
        : base(id)
    {
        ActorId = actorId;
        ActorType = actorType;
        Action = action;
        TargetType = targetType;
        TargetId = targetId;
        OccurredAt = occurredAt;
    }

    public string ActorId { get; }

    public string ActorType { get; }

    public string Action { get; }

    public string TargetType { get; }

    public string TargetId { get; }

    public DateTimeOffset OccurredAt { get; }
}
