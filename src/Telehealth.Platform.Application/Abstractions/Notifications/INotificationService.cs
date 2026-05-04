using Telehealth.Platform.Domain.Notifications;

namespace Telehealth.Platform.Application.Abstractions.Notifications;

public interface INotificationService
{
    Task<Notification> CreateNotificationAsync(
        Guid userId,
        NotificationType type,
        string title,
        string message,
        string? actionUrl = null,
        Dictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<Notification>> GetUserNotificationsAsync(
        Guid userId,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    Task<Notification> GetNotificationByIdAsync(
        Guid notificationId,
        CancellationToken cancellationToken = default);

    Task MarkNotificationAsReadAsync(
        Guid notificationId,
        CancellationToken cancellationToken = default);

    Task MarkAllNotificationsAsReadAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<int> GetUnreadNotificationCountAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task DeleteNotificationAsync(
        Guid notificationId,
        CancellationToken cancellationToken = default);
}