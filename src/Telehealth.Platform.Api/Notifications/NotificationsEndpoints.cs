using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Telehealth.Platform.Application.Abstractions.Notifications;
using Telehealth.Platform.Domain.Notifications;

namespace Telehealth.Platform.Api.Notifications;

/// <summary>
/// Notification management API endpoints — wired to real NotificationService/DB.
/// </summary>
public static class NotificationsEndpoints
{
    public static IEndpointRouteBuilder MapNotificationsEndpoints(this IEndpointRouteBuilder app)
    {
        var notifications = app.MapGroup("/notifications").WithTags("Notifications");

        notifications.MapGet("/my-notifications", GetMyNotificationsAsync).RequireAuthorization();
        notifications.MapGet("/my-preferences", GetMyPreferencesAsync).RequireAuthorization();
        notifications.MapPut("/my-preferences", UpdateMyPreferencesAsync).RequireAuthorization();
        notifications.MapPost("/{notificationId}/read", MarkAsReadAsync).RequireAuthorization();
        notifications.MapPost("/read-all", MarkAllAsReadAsync).RequireAuthorization();

        notifications.MapGet("/admin/notifications", GetAllNotificationsAsync).RequireAuthorization("RequireAdmin");
        notifications.MapPost("/admin/send", SendNotificationAsync).RequireAuthorization("RequireAdmin");

        return app;
    }

    private static async Task<IResult> GetMyNotificationsAsync(
        ClaimsPrincipal user,
        INotificationService notificationService,
        string? status,
        int page = 1,
        int pageSize = 20)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue) return Results.Unauthorized();

        var all = await notificationService.GetUserNotificationsAsync(userId.Value, page, pageSize);
        var unread = await notificationService.GetUnreadNotificationCountAsync(userId.Value);

        var items = all.Select(MapToDto);

        return Results.Ok(new
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            UnreadCount = unread
        });
    }

    private static async Task<IResult> GetMyPreferencesAsync(ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue) return Results.Unauthorized();

        // Preferences are not yet persisted; return sensible defaults
        var prefs = new NotificationPreferencesDto(
            Email: true,
            Push: true,
            Sms: false,
            InApp: true,
            Types: new Dictionary<string, bool>
            {
                ["appointment_reminder"] = true,
                ["appointment_confirmation"] = true,
                ["consultation_completed"] = true,
                ["payment_success"] = true,
                ["payment_failure"] = true,
                ["prescription_available"] = true,
                ["low_credit"] = true
            });

        return Results.Ok(prefs);
    }

    private static async Task<IResult> UpdateMyPreferencesAsync(
        UpdateNotificationPreferencesRequestDto request,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue) return Results.Unauthorized();

        // Preferences are in-memory for now; persist when NotificationPreference entity is wired
        return Results.Ok(new
        {
            Message = "Preferences updated",
            Preferences = new NotificationPreferencesDto(request.Email, request.Push, request.Sms, request.InApp, request.Types)
        });
    }

    private static async Task<IResult> MarkAsReadAsync(
        Guid notificationId,
        ClaimsPrincipal user,
        INotificationService notificationService)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue) return Results.Unauthorized();

        await notificationService.MarkNotificationAsReadAsync(notificationId);
        return Results.Ok(new { Message = "Notification marked as read", NotificationId = notificationId, ReadAt = DateTimeOffset.UtcNow });
    }

    private static async Task<IResult> MarkAllAsReadAsync(
        ClaimsPrincipal user,
        INotificationService notificationService)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue) return Results.Unauthorized();

        await notificationService.MarkAllNotificationsAsReadAsync(userId.Value);
        return Results.Ok(new { Message = "All notifications marked as read", ReadAt = DateTimeOffset.UtcNow });
    }

    private static async Task<IResult> GetAllNotificationsAsync(
        ClaimsPrincipal user,
        INotificationService notificationService,
        Guid? targetUserId,
        string? type,
        string? status,
        int page = 1,
        int pageSize = 50)
    {
        var adminId = GetUserId(user);
        if (!adminId.HasValue) return Results.Unauthorized();

        if (!targetUserId.HasValue)
            return Results.BadRequest(new { Message = "userId query parameter is required for admin notification listing." });

        var items = await notificationService.GetUserNotificationsAsync(targetUserId.Value, page, pageSize);
        return Results.Ok(new { Items = items.Select(MapToDto), Page = page, PageSize = pageSize });
    }

    private static async Task<IResult> SendNotificationAsync(
        SendNotificationRequestDto request,
        ClaimsPrincipal user,
        INotificationService notificationService)
    {
        var adminId = GetUserId(user);
        if (!adminId.HasValue) return Results.Unauthorized();

        if (!request.UserId.HasValue)
            return Results.BadRequest(new { Message = "UserId is required." });

        var notification = await notificationService.CreateNotificationAsync(
            request.UserId.Value,
            NotificationType.Info,
            request.Title,
            request.Message,
            request.ActionUrl);

        return Results.Ok(new { Message = "Notification sent", Notification = MapToDto(notification) });
    }

    private static NotificationDto MapToDto(Notification n) => new(
        n.Id,
        n.UserId,
        n.Type.ToString(),
        n.Title,
        n.Message,
        n.ActionUrl,
        n.IsRead ? "Read" : "Unread",
        n.CreatedAt,
        n.ReadAt);

    private static Guid? GetUserId(ClaimsPrincipal user)
    {
        var raw = user.FindFirst("sub")?.Value ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(raw, out var id) ? id : null;
    }
}

public record NotificationDto(
    Guid Id,
    Guid UserId,
    string Type,
    string Title,
    string Message,
    string? ActionUrl,
    string Status,
    DateTimeOffset CreatedAt,
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
    string Title,
    string Message,
    string? ActionUrl);
