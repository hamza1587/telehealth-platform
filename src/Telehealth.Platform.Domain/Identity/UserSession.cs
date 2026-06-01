using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Identity;

public class UserSession : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public string SessionId { get; private set; } = string.Empty;
    public string DeviceId { get; private set; } = string.Empty;
    public string IpAddress { get; private set; } = string.Empty;
    public string UserAgent { get; private set; } = string.Empty;
    public DateTimeOffset StartedAt { get; private set; }
    public DateTimeOffset LastActivityAt { get; private set; }
    public DateTimeOffset? ExpiresAt { get; private set; }
    public DateTimeOffset? EndedAt { get; private set; }
    public SessionStatus Status { get; private set; } = SessionStatus.Active;
    public string? EndReason { get; private set; }

    private UserSession(
        Guid id,
        Guid userId,
        string sessionId,
        string deviceId,
        string ipAddress,
        string userAgent,
        DateTimeOffset? expiresAt) : base(id)
    {
        UserId = userId;
        SessionId = sessionId;
        DeviceId = deviceId;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        StartedAt = DateTimeOffset.UtcNow;
        LastActivityAt = DateTimeOffset.UtcNow;
        ExpiresAt = expiresAt;
    }

    public void UpdateActivity()
    {
        LastActivityAt = DateTimeOffset.UtcNow;
    }

    public void End(string reason)
    {
        Status = SessionStatus.Ended;
        EndedAt = DateTimeOffset.UtcNow;
        EndReason = reason;
    }

    public bool IsExpired => DateTimeOffset.UtcNow > ExpiresAt;
    public bool IsActive => Status == SessionStatus.Active && !IsExpired;

    public static UserSession Create(
        Guid userId,
        string sessionId,
        string deviceId,
        string ipAddress,
        string userAgent,
        DateTimeOffset? expiresAt)
    {
        return new UserSession(Guid.NewGuid(), userId, sessionId, deviceId, ipAddress, userAgent, expiresAt);
    }

    public PlatformUser? User { get; private set; }
}

public enum SessionStatus
{
    Active = 1,
    Ended = 2,
    Expired = 3
}