using Telehealth.Platform.Domain.Identity;

namespace Telehealth.Platform.Application.Abstractions.Identity;

/// <summary>
/// Service for handling authentication operations including login, registration,
/// token management, MFA, and password operations.
/// </summary>
public interface IAuthenticationService
{
    Task<AuthenticationResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<AuthenticationResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<AuthenticationResult> VerifyMfaAndCompleteLoginAsync(MfaVerificationRequest request, CancellationToken cancellationToken = default);
    Task<TokenResult> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);
    Task RevokeTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task LogoutAsync(Guid userId, string? deviceId = null, CancellationToken cancellationToken = default);

    Task<PasswordResetResult> RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default);
    Task<PasswordResetResult> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default);
    Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default);

    Task<MfaSetupResult> SetupMfaAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> VerifyMfaSetupAsync(Guid userId, string code, CancellationToken cancellationToken = default);
    Task DisableMfaAsync(Guid userId, string password, CancellationToken cancellationToken = default);
    Task<string[]> GenerateMfaRecoveryCodesAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<bool> VerifyEmailAsync(string token, CancellationToken cancellationToken = default);
    Task ResendVerificationEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> VerifyPhoneAsync(Guid userId, string code, CancellationToken cancellationToken = default);

    Task<CurrentUserInfo?> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> IsInRoleAsync(Guid userId, string role, CancellationToken cancellationToken = default);
    Task<bool> HasPermissionAsync(Guid userId, string permission, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default);
}

public record RegisterRequest(
    string Email,
    string Password,
    UserType UserType,
    string? FirstName = null,
    string? LastName = null,
    string? PhoneNumber = null,
    string? CountryCode = null,
    string? PreferredLanguage = null);

public record LoginRequest(
    string Email,
    string Password,
    string? DeviceId = null,
    string? DeviceName = null,
    string? UserAgent = null,
    string? IpAddress = null);

public record MfaVerificationRequest(
    string MfaToken,
    string Code,
    string? DeviceId = null,
    string? DeviceName = null,
    string? UserAgent = null,
    string? IpAddress = null);

public record RefreshTokenRequest(
    string RefreshToken,
    string? DeviceId = null,
    string? IpAddress = null);

public record ResetPasswordRequest(
    string Token,
    string NewPassword);

public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword);

public record AuthenticationResult
{
    public bool Success { get; init; }
    public string? Error { get; init; }
    public string? ErrorCode { get; init; }
    public Guid? UserId { get; init; }
    public string? Email { get; init; }
    public UserType? UserType { get; init; }
    public string[]? Roles { get; init; }
    public string? AccessToken { get; init; }
    public string? RefreshToken { get; init; }
    public DateTimeOffset? ExpiresAt { get; init; }
    public bool MfaRequired { get; init; }
    public string? MfaToken { get; init; }
    public bool EmailConfirmed { get; init; }
    public bool PhoneConfirmed { get; init; }
    public bool MustChangePassword { get; init; }
}

public record TokenResult
{
    public bool Success { get; init; }
    public string? Error { get; init; }
    public string? AccessToken { get; init; }
    public string? RefreshToken { get; init; }
    public DateTimeOffset? ExpiresAt { get; init; }
}

public record PasswordResetResult
{
    public bool Success { get; init; }
    public string? Error { get; init; }
    public string? Message { get; init; }
}

public record MfaSetupResult
{
    public bool Success { get; init; }
    public string? Error { get; init; }
    public string? SecretKey { get; init; }
    public string? QrCodeUri { get; init; }
    public string[]? BackupCodes { get; init; }
}

public record CurrentUserInfo
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string? DisplayName { get; init; }
    public UserType UserType { get; init; }
    public string[] Roles { get; init; } = [];
    public bool TwoFactorEnabled { get; init; }
    public bool EmailConfirmed { get; init; }
    public bool PhoneConfirmed { get; init; }
    public string? CountryCode { get; init; }
    public string? PreferredLanguage { get; init; }
    public DateTimeOffset? LastLoginAt { get; init; }
}
