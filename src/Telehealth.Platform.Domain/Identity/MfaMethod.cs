using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Identity;

public enum MfaMethodType
{
    Totp,
    Sms,
    Email,
    WebAuthn,
    Biometric
}

public sealed class MfaMethod : Entity<Guid>
{
    public MfaMethod(
        Guid id,
        Guid userId,
        MfaMethodType methodType,
        string provider,
        bool isVerified = false,
        string? deviceName = null,
        string? secretKey = null,
        string? publicKey = null,
        DateTimeOffset? lastUsedAt = null) : base(id)
    {
        UserId = userId;
        MethodType = methodType;
        Provider = provider;
        IsVerified = isVerified;
        DeviceName = deviceName;
        SecretKey = secretKey;
        PublicKey = publicKey;
        LastUsedAt = lastUsedAt;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
        IsActive = true;
    }

    public Guid UserId { get; private set; }
    public MfaMethodType MethodType { get; private set; }
    public string Provider { get; private set; }
    public bool IsVerified { get; private set; }
    public string? DeviceName { get; private set; }
    public string? SecretKey { get; private set; }
    public string? PublicKey { get; private set; }
    public DateTimeOffset? LastUsedAt { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public void UpdateLastUsed()
    {
        LastUsedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetVerified(bool verified = true)
    {
        IsVerified = verified;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateDeviceName(string? deviceName)
    {
        DeviceName = deviceName;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}