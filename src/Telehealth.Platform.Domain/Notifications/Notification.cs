using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Notifications;

public enum NotificationType
{
    Info,
    Warning,
    Error,
    Success
}

public enum NotificationStatus
{
    Pending,
    Sent,
    Failed,
    Read
}

public class Notification : Entity<Guid>
{
    public Guid RecipientId { get; private set; }

    public string RecipientType { get; private set; } = string.Empty;

    public NotificationType Type { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string Message { get; private set; } = string.Empty;

    public NotificationStatus Status { get; private set; } = NotificationStatus.Pending;

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? SentAt { get; private set; }

    public DateTimeOffset? ReadAt { get; private set; }

    public string? RelatedEntity { get; private set; }

    public Guid? RelatedEntityId { get; private set; }

    public Dictionary<string, object>? Metadata { get; private set; }

    private Notification(
        Guid id,
        Guid recipientId,
        NotificationType type,
        string title,
        string message) : base(id)
    {
        RecipientId = recipientId;
        Type = type;
        Title = title;
        Message = message;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public static Notification Create(
        Guid recipientId,
        NotificationType type,
        string title,
        string message,
        string? recipientType = null)
    {
        var notification = new Notification(Guid.NewGuid(), recipientId, type, title, message)
        {
            RecipientType = recipientType ?? string.Empty
        };

        return notification;
    }

    public void MarkAsSent()
    {
        Status = NotificationStatus.Sent;
        SentAt = DateTimeOffset.UtcNow;
    }

    public void MarkAsRead()
    {
        Status = NotificationStatus.Read;
        ReadAt = DateTimeOffset.UtcNow;
    }

    public void MarkAsFailed()
    {
        Status = NotificationStatus.Failed;
    }

    public void SetRelatedEntity(string entity, Guid entityId)
    {
        RelatedEntity = entity;
        RelatedEntityId = entityId;
    }

    public void SetMetadata(Dictionary<string, object> metadata)
    {
        Metadata = metadata;
    }
}