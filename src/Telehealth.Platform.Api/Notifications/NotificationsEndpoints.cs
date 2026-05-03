using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Telehealth.Platform.Domain.Notifications;

namespace Telehealth.Platform.Api.Notifications;

/// <summary>
/// Notification management API endpoints.
/// </summary>
public static class NotificationsEndpoints
{
    public static IEndpointRouteBuilder MapNotificationsEndpoints(this IEndpointRouteBuilder app)
    {
        var notifications = app.MapGroup("/notifications").WithTags("Notifications");

        // User endpoints
        notifications.MapGet("/my-notifications", GetMyNotificationsAsync).RequireAuthorization();
        notifications.MapGet("/my-preferences", GetMyPreferencesAsync).RequireAuthorization();
        notifications.MapPut("/my-preferences", UpdateMyPreferencesAsync).RequireAuthorization();
        notifications.MapPost("/{notificationId}/read", MarkAsReadAsync).RequireAuthorization();
        notifications.MapPost("/read-all", MarkAllAsReadAsync).RequireAuthorization();

        // Admin endpoints
        notifications.MapGet("/admin/notifications", GetAllNotificationsAsync).RequireAuthorization("RequireAdmin");
        notifications.MapPost("/admin/send", SendNotificationAsync).RequireAuthorization("RequireAdmin");

        return app;
    }

    private static async Task<IResult> GetMyNotificationsAsync(
        ClaimsPrincipal user,
        string? status,
        int page = 1,
        int pageSize = 20)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var notifications = new List<NotificationDto>
        {
            new(
                Guid.NewGuid(),
                userId.Value,
                "patient",
                "appointment_reminder",
                "Appointment Reminder",
                "Your consultation with Dr. Jane Smith starts in 30 minutes",
                "/consultations/123",
                null,
                NotificationType.Info,
                NotificationStatus.Pending,
                new List<NotificationChannel> { NotificationChannel.InApp, NotificationChannel.Email },
                DateTimeOffset.UtcNow.AddHours(-1),
                null,
                null,
                null),
            new(
                Guid.NewGuid(),
                userId.Value,
                "patient",
                "consultation_completed",
                "Consultation Completed",
                "Your consultation has ended. Total duration: 30 minutes",
                "/billing/456",
                new { duration = 1800, amount = 450 },
                NotificationType.Success,
                NotificationStatus.Sent,
                new List<NotificationChannel> { NotificationChannel.InApp },
                DateTimeOffset.UtcNow.AddDays(-1),
                DateTimeOffset.UtcNow.AddDays(-1),
                DateTimeOffset.UtcNow.AddDays(-1),
                null)
        };

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<NotificationStatus>(status, out var parsedStatus))
        {
            notifications = notifications.Where(n => n.Status == parsedStatus).ToList();
        }

        var totalCount = notifications.Count;
        var pagedNotifications = notifications.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return Results.Ok(new
        {
            Items = pagedNotifications,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            UnreadCount = notifications.Count(n => n.Status != NotificationStatus.Read)
        });
    }

    private static async Task<IResult> GetMyPreferencesAsync(ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var preferences = new NotificationPreferencesDto(
            true,
            true,
            true,
            true,
            new Dictionary<string, bool>
            {
                ["appointment_reminder"] = true,
                ["appointment_confirmation"] = true,
                ["consultation_completed"] = true,
                ["payment_success"] = true,
                ["payment_failure"] = false,
                ["prescription_available"] = true,
                ["low_credit"] = true
            });

        return Results.Ok(preferences);
    }

    private static async Task<IResult> UpdateMyPreferencesAsync(
        UpdateNotificationPreferencesRequestDto request,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var preferences = new NotificationPreferencesDto(
            request.Email,
            request.Push,
            request.Sms,
            request.InApp,
            request.Types);

        return Results.Ok(new { Message = "Preferences updated", Preferences = preferences });
    }

    private static async Task<IResult> MarkAsReadAsync(
        Guid notificationId,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(new
        {
            Message = "Notification marked as read",
            NotificationId = notificationId,
            ReadAt = DateTimeOffset.UtcNow
        });
    }

    private static async Task<IResult> MarkAllAsReadAsync(ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(new
        {
            Message = "All notifications marked as read",
            ReadAt = DateTimeOffset.UtcNow
        });
    }

    private static async Task<IResult> GetAllNotificationsAsync(
        ClaimsPrincipal user,
        string? userId,
        string? type,
        string? status,
        int page = 1,
        int pageSize = 50)
    {
        var adminId = GetUserId(user);
        if (!adminId.HasValue)
        {
            return Results.Unauthorized();
        }

        var notifications = new List<NotificationDto>
        {
            new(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "patient",
                "appointment_reminder",
                "Appointment Reminder",
                "Your consultation starts in 30 minutes",
                null,
                null,
                NotificationType.Info,
                NotificationStatus.Sent,
                new List<NotificationChannel> { NotificationChannel.InApp, NotificationChannel.Email },
                DateTimeOffset.UtcNow.AddHours(-1),
                DateTimeOffset.UtcNow.AddHours(-1),
                null,
                null)
        };

        return Results.Ok(new
        {
            Items = notifications,
            TotalCount = notifications.Count,
            Page = page,
            PageSize = pageSize
        });
    }

    private static async Task<IResult> SendNotificationAsync(
        SendNotificationRequestDto request,
        ClaimsPrincipal user)
    {
        var adminId = GetUserId(user);
        if (!adminId.HasValue)
        {
            return Results.Unauthorized();
        }

        var notification = new NotificationDto(
            Guid.NewGuid(),
            request.UserId,
            "user",
            request.Type.ToString(),
            request.Title,
            request.Message,
            request.ActionUrl,
            request.Data,
            NotificationType.Info,
            NotificationStatus.Pending,
            request.Channels,
            DateTimeOffset.UtcNow,
            null,
            null,
            null);

        return Results.Ok(new
        {
            Message = "Notification sent",
            Notification = notification
        });
    }

    private static Guid? GetUserId(ClaimsPrincipal user)
    {
        var userIdString = user.FindFirst("sub")?.Value ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdString, out var userId) ? userId : null;
    }
}

// DTOs
public record NotificationDto(
    Guid Id,
    Guid? UserId,
    string UserType,
    string Type,
    string Title,
    string Message,
    string? ActionUrl,
    object? Data,
    NotificationType NotificationType,
    NotificationStatus Status,
    List<NotificationChannel> Channels,
    DateTimeOffset CreatedAt,
    DateTimeOffset? SentAt,
    DateTimeOffset? DeliveredAt,
    DateTimeOffset? ReadAt);

public record NotificationPreferencesDto(
    bool Email,
    bool Push,
    bool Sms,
    bool InApp,
    Dictionary<string, bool> Types);

public record UpdateNotificationPreferencesRequestDto(
    bool Email,
    bool Push,
    bool Sms,
    bool InApp,
    Dictionary<string, bool> Types);

public record SendNotificationRequestDto(
    Guid? UserId,
    string NotificationType,
    string Title,
    string Message,
    string? ActionUrl,
    object? Data,
    NotificationType Type,
    List<NotificationChannel> Channels);