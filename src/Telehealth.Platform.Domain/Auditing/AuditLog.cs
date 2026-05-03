using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Auditing;

public class AuditLog : Entity<Guid>
{
    public DateTimeOffset Timestamp { get; private set; }

    public string EventType { get; private set; } = string.Empty;

    public string UserId { get; private set; } = string.Empty;

    public string? UserType { get; private set; }

    public string ResourceType { get; private set; } = string.Empty;

    public Guid? ResourceId { get; private set; }

    public string Action { get; private set; } = string.Empty;

    public Dictionary<string, object>? Changes { get; private set; }

    public string? IpAddress { get; private set; }

    public string? UserAgent { get; private set; }

    public string CorrelationId { get; private set; } = string.Empty;

    private AuditLog(
        Guid id,
        string eventType,
        string resourceType,
        string action) : base(id)
    {
        Timestamp = DateTimeOffset.UtcNow;
        EventType = eventType;
        ResourceType = resourceType;
        Action = action;
    }

    public static AuditLog Create(
        string eventType,
        string resourceType,
        string action,
        Guid? resourceId = null,
        string? userId = null,
        string? userType = null,
        Dictionary<string, object>? changes = null,
        string? ipAddress = null,
        string? userAgent = null,
        string? correlationId = null)
    {
        var log = new AuditLog(Guid.NewGuid(), eventType, resourceType, action)
        {
            ResourceId = resourceId,
            UserId = userId ?? string.Empty,
            UserType = userType,
            Changes = changes,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            CorrelationId = correlationId ?? string.Empty
        };

        return log;
    }

    public void SetChanges(Dictionary<string, object> changes)
    {
        Changes = changes;
    }
}