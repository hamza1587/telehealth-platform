using Microsoft.EntityFrameworkCore;
using Telehealth.Platform.Application.Abstractions.Notifications;
using Telehealth.Platform.Domain.Notifications;
using Telehealth.Platform.Infrastructure.Persistence;

namespace Telehealth.Platform.Infrastructure.Notifications;

public class NotificationService : INotificationService
{
    private readonly PlatformDbContext _context;

    public NotificationService(PlatformDbContext context)
    {
        _context = context;
    }

    public async Task<Notification> CreateNotificationAsync(
        Guid userId,
        NotificationType type,
        string title,
        string message,
        string? actionUrl = null,
        Dictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        var notification = Notification.Create(userId, type, title, message, actionUrl, metadata);
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync(cancellationToken);
        return notification;
    }

    public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(
        Guid userId,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<Notification> GetNotificationByIdAsync(
        Guid notificationId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Notifications.FindAsync([notificationId], cancellationToken)
            ?? throw new InvalidOperationException("Notification not found");
    }

    public async Task MarkNotificationAsReadAsync(
        Guid notificationId,
        CancellationToken cancellationToken = default)
    {
        var notification = await _context.Notifications.FindAsync([notificationId], cancellationToken);
        if (notification != null)
        {
            notification.MarkAsRead();
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task MarkAllNotificationsAsReadAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var notifications = _context.Notifications.Where(n => n.UserId == userId && !n.IsRead);
        foreach (var notification in notifications)
        {
            notification.MarkAsRead();
        }
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> GetUnreadNotificationCountAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Notifications.CountAsync(
            n => n.UserId == userId && !n.IsRead, cancellationToken);
    }

    public async Task DeleteNotificationAsync(
        Guid notificationId,
        CancellationToken cancellationToken = default)
    {
        var notification = await _context.Notifications.FindAsync([notificationId], cancellationToken);
        if (notification != null)
        {
            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}