using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Notifications;

public class Notification : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public NotificationType Type { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public bool IsRead { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? ReadAt { get; private set; }
    public string? ActionUrl { get; private set; }
    public Dictionary<string, object>? Metadata { get; private set; }

    private Notification(
        Guid id,
        Guid userId,
        NotificationType type,
        string title,
        string message) : base(id)
    {
        UserId = userId;
        Type = type;
        Title = title;
        Message = message;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public static Notification Create(
        Guid userId,
        NotificationType type,
        string title,
        string message,
        string? actionUrl = null,
        Dictionary<string, object>? metadata = null)
    {
        var notification = new Notification(Guid.NewGuid(), userId, type, title, message)
        {
            ActionUrl = actionUrl,
            Metadata = metadata
        };
        return notification;
    }

    public void MarkAsRead()
    {
        IsRead = true;
        ReadAt = DateTimeOffset.UtcNow;
    }

    public void UpdateMetadata(Dictionary<string, object> metadata)
    {
        Metadata = metadata;
    }
}

public enum NotificationType
{
    Info = 1,
    Warning = 2,
    Error = 3,
    Success = 4,
    Appointment = 5,
    Message = 6,
    Prescription = 7
}

public enum NotificationStatus
{
    Pending = 1,
    Sent = 2,
    Delivered = 3,
    Read = 4,
    Failed = 5
}