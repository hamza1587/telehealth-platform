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

    public string ActorId { get; private set; }

    public string ActorType { get; private set; }

    public string Action { get; private set; }

    public string TargetType { get; private set; }

    public string TargetId { get; private set; }

    public DateTimeOffset OccurredAt { get; private set; }
}
