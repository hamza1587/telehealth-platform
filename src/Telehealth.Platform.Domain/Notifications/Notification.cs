using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Notifications;

/// <summary>
/// Notification to be sent to users across multiple channels.
/// </summary>
public sealed class Notification : Entity<Guid>
{
    public Notification(
        Guid id,
        Guid? userId,
        string userType,
        string notificationType,
        string title,
        string message,
        string? actionUrl,
        object? data,
        NotificationPriority priority,
        DateTimeOffset createdAt)
        : base(id)
    {
        UserId = userId;
        UserType = userType;
        NotificationType = notificationType;
        Title = title;
        Message = message;
        ActionUrl = actionUrl;
        Data = data;
        Priority = priority;
        Status = NotificationStatus.Pending;
        Channels = new List<NotificationChannel>();
        CreatedAt = createdAt;
        ScheduledFor = createdAt;
    }

    public Guid? UserId { get; }
    public string UserType { get; }
    public string NotificationType { get; }
    public string Title { get; }
    public string Message { get; }
    public string? ActionUrl { get; }
    public object? Data { get; }
    public NotificationPriority Priority { get; }
    public NotificationStatus Status { get; private set; }
    public List<NotificationChannel> Channels { get; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset ScheduledFor { get; private set; }
    public DateTimeOffset? SentAt { get; private set; }
    public DateTimeOffset? ReadAt { get; private set; }

    public void ScheduleFor(DateTimeOffset scheduledFor)
    {
        ScheduledFor = scheduledFor;
    }

    public void MarkAsSent(DateTimeOffset sentAt)
    {
        Status = NotificationStatus.Sent;
        SentAt = sentAt;
    }

    public void MarkAsFailed(string reason, DateTimeOffset failedAt)
    {
        Status = NotificationStatus.Failed;
    }

    public void MarkAsRead(DateTimeOffset readAt)
    {
        ReadAt = readAt;
    }

    public void AddChannel(NotificationChannel channel)
    {
        Channels.Add(channel);
    }

    public bool ShouldSendNow(DateTimeOffset now)
    {
        return Status == NotificationStatus.Pending && ScheduledFor <= now;
    }
}

public enum NotificationChannel
{
    InApp,
    Email,
    Push,
    Sms,
    WebSocket
}

public enum NotificationStatus
{
    Pending,
    Sending,
    Sent,
    Delivered,
    Read,
    Failed,
    Cancelled
}

public enum NotificationPriority
{
    Low,
    Normal,
    High,
    Urgent
}

public enum NotificationType
{
    AppointmentReminder,
    AppointmentConfirmed,
    AppointmentCancelled,
    ConsultationStarting,
    ConsultationEnded,
    PrescriptionReady,
    PaymentReceived,
    PaymentFailed,
    RefundProcessed,
    DayPassExpiring,
    DayPassExpired,
    WalletLowBalance,
    NewMessage,
    VerificationApproved,
    VerificationRejected,
    SystemMaintenance,
    PasswordChanged,
    NewDeviceLogin,
    Welcome
}
