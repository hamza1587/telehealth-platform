using System.Security.Claims;
using Telehealth.Platform.Domain.Auditing;
using Telehealth.Platform.Infrastructure.Persistence;

namespace Telehealth.Platform.Api.Middleware;

public class AuditMiddleware
{
    private static readonly HashSet<string> AuditedMethods =
        new(StringComparer.OrdinalIgnoreCase) { "POST", "PUT", "PATCH", "DELETE" };

    private readonly RequestDelegate _next;

    public AuditMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, PlatformDbContext db)
    {
        await _next(context);

        // Only audit successful state-changing requests
        if (!AuditedMethods.Contains(context.Request.Method)) return;
        if (context.Response.StatusCode is < 200 or >= 400) return;

        var correlationId = context.Items["CorrelationId"]?.ToString() ?? string.Empty;
        var userId = context.User.FindFirst("sub")?.Value
                     ?? context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? "anonymous";
        var userType = context.User.FindFirst("user_type")?.Value ?? "unknown";
        var ip = context.Connection.RemoteIpAddress?.ToString();
        var ua = context.Request.Headers.UserAgent.ToString();

        var path = context.Request.Path.Value ?? "/";
        var segments = path.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        var resourceType = segments.Length > 0 ? segments[0] : "unknown";
        var action = $"{context.Request.Method} {path}";

        var auditEvent = new AuditEvent(
            Guid.NewGuid(),
            userId,
            userType,
            action,
            resourceType,
            segments.Length > 1 ? segments[1] : path,
            DateTimeOffset.UtcNow);

        db.AuditEvents.Add(auditEvent);

        try
        {
            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Audit failure must never break the HTTP response
            var logger = context.RequestServices.GetService<ILogger<AuditMiddleware>>();
            logger?.LogWarning(ex, "Audit event save failed for {Action}", action);
        }
    }
}
