using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Notifications;

/// <summary>
/// User preferences for notification channels and types.
/// </summary>
public sealed class NotificationPreference : Entity<Guid>
{
    public NotificationPreference(
        Guid id,
        Guid userId,
        string notificationType,
        bool emailEnabled,
        bool pushEnabled,
        bool smsEnabled,
        bool inAppEnabled,
        DateTimeOffset createdAt)
        : base(id)
    {
        UserId = userId;
        NotificationType = notificationType;
        EmailEnabled = emailEnabled;
        PushEnabled = pushEnabled;
        SmsEnabled = smsEnabled;
        InAppEnabled = inAppEnabled;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public Guid UserId { get; private set; }
    public string NotificationType { get; private set; }
    public bool EmailEnabled { get; private set; }
    public bool PushEnabled { get; private set; }
    public bool SmsEnabled { get; private set; }
    public bool InAppEnabled { get; private set; }
    public bool IsMuted { get; private set; }
    public TimeOnly? QuietHoursStart { get; private set; }
    public TimeOnly? QuietHoursEnd { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public void Update(
        bool emailEnabled,
        bool pushEnabled,
        bool smsEnabled,
        bool inAppEnabled,
        DateTimeOffset updatedAt)
    {
        EmailEnabled = emailEnabled;
        PushEnabled = pushEnabled;
        SmsEnabled = smsEnabled;
        InAppEnabled = inAppEnabled;
        UpdatedAt = updatedAt;
    }

    public void SetQuietHours(TimeOnly? start, TimeOnly? end, DateTimeOffset updatedAt)
    {
        QuietHoursStart = start;
        QuietHoursEnd = end;
        UpdatedAt = updatedAt;
    }

    public void Mute(DateTimeOffset updatedAt)
    {
        IsMuted = true;
        UpdatedAt = updatedAt;
    }

    public void Unmute(DateTimeOffset updatedAt)
    {
        IsMuted = false;
        UpdatedAt = updatedAt;
    }

    public bool ShouldSendToChannel(NotificationChannel channel, DateTimeOffset now)
    {
        if (IsMuted)
            return false;

        // Check quiet hours
        if (QuietHoursStart.HasValue && QuietHoursEnd.HasValue)
        {
            var currentTime = TimeOnly.FromDateTime(now.DateTime);
            if (QuietHoursStart.Value <= QuietHoursEnd.Value)
            {
                if (currentTime >= QuietHoursStart.Value && currentTime <= QuietHoursEnd.Value)
                    return false;
            }
            else
            {
                if (currentTime >= QuietHoursStart.Value || currentTime <= QuietHoursEnd.Value)
                    return false;
            }
        }

        return channel switch
        {
            NotificationChannel.Email => EmailEnabled,
            NotificationChannel.Push => PushEnabled,
            NotificationChannel.Sms => SmsEnabled,
            NotificationChannel.InApp => InAppEnabled,
            _ => false
        };
    }
}
