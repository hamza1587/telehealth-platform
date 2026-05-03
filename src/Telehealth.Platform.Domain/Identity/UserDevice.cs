using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Identity;

public class UserDevice : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public string DeviceId { get; private set; } = string.Empty;
    public string DeviceName { get; private set; } = string.Empty;
    public string DeviceType { get; private set; } = string.Empty;
    public string UserAgent { get; private set; } = string.Empty;
    public string IpAddress { get; private set; } = string.Empty;
    public string DeviceFingerprint { get; private set; } = string.Empty;
    public DateTimeOffset FirstSeenAt { get; private set; }
    public DateTimeOffset LastSeenAt { get; private set; }
    public DateTimeOffset? LastUsedAt { get; private set; }
    public bool IsTrusted { get; private set; }
    public bool IsBlocked { get; private set; }
    public DateTimeOffset? BlockedAt { get; private set; }
    public string? BlockedReason { get; private set; }

    private UserDevice(
        Guid id,
        Guid userId,
        string deviceId,
        string deviceName,
        string deviceType,
        string userAgent,
        string ipAddress) : base(id)
    {
        UserId = userId;
        DeviceId = deviceId;
        DeviceName = deviceName;
        DeviceType = deviceType;
        UserAgent = userAgent;
        IpAddress = ipAddress;
        DeviceFingerprint = GenerateFingerprint(deviceId, userAgent, ipAddress);
        FirstSeenAt = DateTimeOffset.UtcNow;
        LastSeenAt = DateTimeOffset.UtcNow;
    }

    public void UpdateLastSeen(string userAgent, string ipAddress)
    {
        LastSeenAt = DateTimeOffset.UtcNow;
        if (UserAgent != userAgent)
        {
            UserAgent = userAgent;
        }
        if (IpAddress != ipAddress)
        {
            IpAddress = ipAddress;
        }
    }

    public void MarkAsUsed()
    {
        LastUsedAt = DateTimeOffset.UtcNow;
    }

    public void SetTrusted(bool isTrusted = true)
    {
        IsTrusted = isTrusted;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Block(string reason)
    {
        IsBlocked = true;
        BlockedAt = DateTimeOffset.UtcNow;
        BlockedReason = reason;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Unblock()
    {
        IsBlocked = false;
        BlockedAt = null;
        BlockedReason = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static string GenerateFingerprint(string deviceId, string userAgent, string ipAddress)
    {
        return $"{deviceId}|{userAgent}|{ipAddress}";
    }

    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public static UserDevice Create(
        Guid userId,
        string deviceId,
        string deviceName,
        string deviceType,
        string userAgent,
        string ipAddress)
    {
        return new UserDevice(Guid.NewGuid(), userId, deviceId, deviceName, deviceType, userAgent, ipAddress);
    }

    public PlatformUser? User { get; private set; }
}