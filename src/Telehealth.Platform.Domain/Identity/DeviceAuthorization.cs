using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Identity;

/// <summary>
/// Tracks authorized devices for each user.
/// Used for device-based security controls and suspicious activity detection.
/// </summary>
public sealed class DeviceAuthorization : Entity<Guid>
{
    public DeviceAuthorization(
        Guid id,
        Guid userId,
        string deviceId,
        string? deviceName,
        string? deviceType,
        string? os,
        string? browser,
        string ipAddress,
        string? userAgent,
        string? publicKey = null)
        : base(id)
    {
        UserId = userId;
        DeviceId = deviceId;
        DeviceName = deviceName;
        DeviceType = deviceType;
        OperatingSystem = os;
        Browser = browser;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        PublicKey = publicKey;
        Status = DeviceStatus.Active;
        FirstSeenAt = DateTimeOffset.UtcNow;
        LastSeenAt = DateTimeOffset.UtcNow;
        TrustLevel = DeviceTrustLevel.Medium;
    }

    public Guid UserId { get; private set; }
    public string DeviceId { get; private set; }
    public string? DeviceName { get; private set; }
    public string? DeviceType { get; private set; }
    public string? OperatingSystem { get; private set; }
    public string? Browser { get; private set; }
    public string? PublicKey { get; private set; }

    // Network info
    public string IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public string? CountryCode { get; private set; }
    public string? City { get; private set; }

    // Status
    public DeviceStatus Status { get; private set; }
    public DeviceTrustLevel TrustLevel { get; private set; }

    // Timestamps
    public DateTimeOffset FirstSeenAt { get; private set; }
    public DateTimeOffset LastSeenAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public string? RevokedReason { get; private set; }

    // MFA on device
    public bool MfaVerified { get; private set; }
    public DateTimeOffset? MfaVerifiedAt { get; private set; }

    // Navigation
    public PlatformUser User { get; private set; } = null!;

    public void UpdateLastSeen(string ipAddress, string? userAgent)
    {
        IpAddress = ipAddress;
        if (userAgent != null)
            UserAgent = userAgent;
        LastSeenAt = DateTimeOffset.UtcNow;
    }

    public void SetGeoInfo(string? countryCode, string? city)
    {
        CountryCode = countryCode;
        City = city;
    }

    public void VerifyMfa()
    {
        MfaVerified = true;
        MfaVerifiedAt = DateTimeOffset.UtcNow;
        TrustLevel = DeviceTrustLevel.High;
    }

    public void Revoke(string reason)
    {
        Status = DeviceStatus.Revoked;
        RevokedAt = DateTimeOffset.UtcNow;
        RevokedReason = reason;
    }

    public void Suspend(string reason)
    {
        Status = DeviceStatus.Suspended;
        RevokedReason = reason;
    }

    public void SetTrustLevel(DeviceTrustLevel level)
    {
        TrustLevel = level;
    }
}

public enum DeviceStatus
{
    Active,
    Suspended,
    Revoked,
    Expired
}

public enum DeviceTrustLevel
{
    Low,      // New or unverified device
    Medium,   // Device seen before but not MFA verified
    High      // MFA verified device
}
