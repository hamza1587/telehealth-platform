using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Identity;

/// <summary>
/// Refresh token entity for secure token rotation.
/// Implements secure token storage with hashing.
/// </summary>
public sealed class RefreshToken : Entity<Guid>
{
    public RefreshToken(
        Guid id,
        Guid userId,
        string tokenHash,
        string deviceId,
        string? deviceName,
        string? ipAddress,
        string? userAgent,
        DateTimeOffset expiresAt)
        : base(id)
    {
        UserId = userId;
        TokenHash = tokenHash;
        DeviceId = deviceId;
        DeviceName = deviceName;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        ExpiresAt = expiresAt;
        CreatedAt = DateTimeOffset.UtcNow;
        IsRevoked = false;
        IsUsed = false;
    }

    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; }

    // Device information for tracking
    public string DeviceId { get; private set; }
    public string? DeviceName { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }

    // Token lifecycle
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public string? RevokedReason { get; private set; }
    public bool IsRevoked { get; private set; }
    public bool IsUsed { get; private set; }
    public DateTimeOffset? UsedAt { get; private set; }
    public string? ReplacedByTokenId { get; private set; }

    // Navigation
    public PlatformUser User { get; private set; } = null!;

    public void MarkAsUsed()
    {
        IsUsed = true;
        UsedAt = DateTimeOffset.UtcNow;
    }

    public void Revoke(string reason)
    {
        IsRevoked = true;
        RevokedAt = DateTimeOffset.UtcNow;
        RevokedReason = reason;
    }

    public void ReplaceWith(string newTokenId)
    {
        ReplacedByTokenId = newTokenId;
        Revoke("Replaced by new token");
    }

    public bool IsValid()
    {
        return !IsRevoked && !IsUsed && ExpiresAt > DateTimeOffset.UtcNow;
    }
}
