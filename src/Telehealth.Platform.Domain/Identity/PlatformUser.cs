using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Identity;

/// <summary>
/// Core user entity for authentication and authorization.
/// Supports multiple user types: Patient, Doctor, Admin, Support, Compliance, Finance.
/// </summary>
public sealed class PlatformUser : Entity<Guid>, IAuditableEntity
{
    public PlatformUser(
        Guid id,
        string email,
        string passwordHash,
        UserType userType,
        string? phoneNumber = null,
        string? firstName = null,
        string? lastName = null,
        string? displayName = null,
        string? countryCode = null,
        string? preferredLanguage = null)
        : base(id)
    {
        Email = email;
        NormalizedEmail = email.ToUpperInvariant();
        PasswordHash = passwordHash;
        UserType = userType;
        PhoneNumber = phoneNumber;
        FirstName = firstName;
        LastName = lastName;
        DisplayName = displayName;
        CountryCode = countryCode;
        PreferredLanguage = preferredLanguage ?? "en";
        Status = UserStatus.PendingVerification;
        SecurityStamp = Guid.NewGuid().ToString("N");
        ConcurrencyStamp = Guid.NewGuid().ToString("N");
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
        EmailConfirmed = false;
        PhoneNumberConfirmed = false;
        TwoFactorEnabled = userType is UserType.Doctor or UserType.Admin or UserType.ComplianceOfficer;
        LockoutEnabled = true;
        AccessFailedCount = 0;
    }

    // Authentication
    public string Email { get; private set; }
    public string NormalizedEmail { get; private set; }
    public string PasswordHash { get; private set; }
    public string SecurityStamp { get; private set; }
    public string ConcurrencyStamp { get; private set; }

    // Profile
    public string? PhoneNumber { get; private set; }
    public string? FirstName { get; private set; }
    public string? LastName { get; private set; }
    public string? DisplayName { get; private set; }
    public string? CountryCode { get; private set; }
    public string PreferredLanguage { get; private set; }

    // User Type & Status
    public UserType UserType { get; private set; }
    public UserStatus Status { get; private set; }

    // Verification
    public bool EmailConfirmed { get; private set; }
    public bool PhoneNumberConfirmed { get; private set; }
    public string? EmailVerificationToken { get; private set; }
    public DateTimeOffset? EmailVerificationTokenExpiresAt { get; private set; }
    public string? PhoneVerificationCode { get; private set; }
    public DateTimeOffset? PhoneVerificationCodeExpiresAt { get; private set; }

    // Security
    public bool TwoFactorEnabled { get; private set; }
    public string? TwoFactorSecret { get; private set; }
    public bool LockoutEnabled { get; private set; }
    public DateTimeOffset? LockoutEndAt { get; private set; }
    public int AccessFailedCount { get; private set; }

    // MFA Recovery Codes (encrypted)
    public string? TwoFactorRecoveryCodes { get; set; }

    // External Authentication
    public string? ExternalProvider { get; private set; }
    public string? ExternalProviderId { get; private set; }

    // GDPR Consent
    public bool TermsAccepted { get; private set; }
    public DateTimeOffset? TermsAcceptedAt { get; private set; }
    public string? TermsVersion { get; private set; }
    public bool PrivacyPolicyAccepted { get; private set; }
    public DateTimeOffset? PrivacyPolicyAcceptedAt { get; private set; }
    public string? PrivacyPolicyVersion { get; private set; }

    // Audit
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? LastLoginAt { get; private set; }
    public string? LastLoginIp { get; private set; }

    // Navigation
    public ICollection<UserRole> UserRoles { get; private set; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; private set; } = [];
    public ICollection<LoginAttempt> LoginAttempts { get; private set; } = [];
    public ICollection<DeviceAuthorization> DeviceAuthorizations { get; private set; } = [];

    public void ConfirmEmail()
    {
        EmailConfirmed = true;
        EmailVerificationToken = null;
        EmailVerificationTokenExpiresAt = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetEmailVerificationToken(string token, DateTimeOffset expiresAt)
    {
        EmailVerificationToken = token;
        EmailVerificationTokenExpiresAt = expiresAt;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ConfirmPhone()
    {
        PhoneNumberConfirmed = true;
        PhoneVerificationCode = null;
        PhoneVerificationCodeExpiresAt = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetPhoneVerificationCode(string code, DateTimeOffset expiresAt)
    {
        PhoneVerificationCode = code;
        PhoneVerificationCodeExpiresAt = expiresAt;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void EnableTwoFactor(string secret, string recoveryCodes)
    {
        TwoFactorEnabled = true;
        TwoFactorSecret = secret;
        TwoFactorRecoveryCodes = recoveryCodes;
        SecurityStamp = Guid.NewGuid().ToString("N");
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void DisableTwoFactor()
    {
        TwoFactorEnabled = false;
        TwoFactorSecret = null;
        TwoFactorRecoveryCodes = null;
        SecurityStamp = Guid.NewGuid().ToString("N");
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ChangePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        SecurityStamp = Guid.NewGuid().ToString("N");
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RecordLogin(string ipAddress)
    {
        LastLoginAt = DateTimeOffset.UtcNow;
        LastLoginIp = ipAddress;
        AccessFailedCount = 0;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RecordFailedLogin()
    {
        AccessFailedCount++;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Lockout(TimeSpan duration)
    {
        LockoutEndAt = DateTimeOffset.UtcNow.Add(duration);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Unlock()
    {
        LockoutEndAt = null;
        AccessFailedCount = 0;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void AcceptTerms(string version)
    {
        TermsAccepted = true;
        TermsAcceptedAt = DateTimeOffset.UtcNow;
        TermsVersion = version;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void AcceptPrivacyPolicy(string version)
    {
        PrivacyPolicyAccepted = true;
        PrivacyPolicyAcceptedAt = DateTimeOffset.UtcNow;
        PrivacyPolicyVersion = version;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Activate()
    {
        Status = UserStatus.Active;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Suspend(string reason)
    {
        Status = UserStatus.Suspended;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public bool IsLockedOut()
    {
        return LockoutEnabled && LockoutEndAt.HasValue && LockoutEndAt.Value > DateTimeOffset.UtcNow;
    }

    public string GetFullName() => $"{FirstName} {LastName}".Trim();
}

public enum UserType
{
    Patient,
    Doctor,
    Admin,
    SupportAgent,
    ComplianceOfficer,
    FinanceOperator,
    ResearchReviewer
}

public enum UserStatus
{
    PendingVerification,
    Active,
    Suspended,
    Deactivated,
    PendingDeletion
}
