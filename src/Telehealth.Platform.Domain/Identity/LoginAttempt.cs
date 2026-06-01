using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Identity;

/// <summary>
/// Records all login attempts for security auditing and brute-force protection.
/// This entity is append-only and tamper-resistant.
/// </summary>
public sealed class LoginAttempt : Entity<Guid>
{
    public LoginAttempt(
        Guid id,
        string? userId,
        string? email,
        string? ipAddress,
        string? userAgent,
        string? deviceId,
        LoginAttemptResult result,
        string? failureReason = null,
        string? mfaMethodUsed = null)
        : base(id)
    {
        UserId = userId;
        Email = email;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        DeviceId = deviceId;
        Result = result;
        FailureReason = failureReason;
        MfaMethodUsed = mfaMethodUsed;
        AttemptedAt = DateTimeOffset.UtcNow;
    }

    public string? UserId { get; private set; }
    public string? Email { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public string? DeviceId { get; private set; }

    public LoginAttemptResult Result { get; private set; }
    public string? FailureReason { get; private set; }

    // MFA tracking
    public string? MfaMethodUsed { get; private set; }
    public bool MfaSuccessful { get; set; }

    // Geo/Network info (if available)
    public string? CountryCode { get; private set; }
    public string? City { get; private set; }
    public bool? IsKnownDevice { get; private set; }
    public bool? IsSuspicious { get; private set; }

    // Audit timestamp - immutable
    public DateTimeOffset AttemptedAt { get; }

    public void MarkMfaSuccess(string method)
    {
        MfaSuccessful = true;
        MfaMethodUsed = method;
    }

    public void SetGeoInfo(string? countryCode, string? city)
    {
        CountryCode = countryCode;
        City = city;
    }

    public void SetRiskAssessment(bool isKnownDevice, bool isSuspicious)
    {
        IsKnownDevice = isKnownDevice;
        IsSuspicious = isSuspicious;
    }
}

public enum LoginAttemptResult
{
    Success,
    InvalidCredentials,
    UserNotFound,
    AccountLocked,
    AccountDisabled,
    EmailNotVerified,
    MfaRequired,
    MfaFailed,
    SessionExpired,
    TokenInvalid,
    TokenExpired,
    TokenRevoked,
    IpBlocked,
    RateLimited,
    SuspiciousActivity
}
