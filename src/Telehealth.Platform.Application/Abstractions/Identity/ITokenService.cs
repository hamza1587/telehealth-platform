using Telehealth.Platform.Domain.Identity;

namespace Telehealth.Platform.Application.Abstractions.Identity;

/// <summary>
/// Service for JWT token generation and validation.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generates a JWT access token for the user.
    /// </summary>
    TokenData GenerateAccessToken(PlatformUser user, IEnumerable<string> roles, IEnumerable<string> permissions);

    /// <summary>
    /// Generates a secure refresh token.
    /// </summary>
    string GenerateRefreshToken();

    /// <summary>
    /// Validates an access token and returns the principal.
    /// </summary>
    TokenValidationResult ValidateAccessToken(string token);

    /// <summary>
    /// Generates a temporary token for MFA verification step.
    /// </summary>
    string GenerateMfaToken(Guid userId, string email);

    /// <summary>
    /// Validates an MFA token.
    /// </summary>
    MfaTokenValidationResult ValidateMfaToken(string token);

    /// <summary>
    /// Generates a password reset token.
    /// </summary>
    string GeneratePasswordResetToken(Guid userId);

    /// <summary>
    /// Validates a password reset token.
    /// </summary>
    TokenValidationResult ValidatePasswordResetToken(string token);

    /// <summary>
    /// Generates an email verification token.
    /// </summary>
    string GenerateEmailVerificationToken(Guid userId);

    /// <summary>
    /// Generates a secure random code for SMS/email verification.
    /// </summary>
    string GenerateVerificationCode(int length = 6);
}

public record TokenData
{
    public string AccessToken { get; init; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; init; }
    public string TokenId { get; init; } = string.Empty;
}

public record TokenValidationResult
{
    public bool IsValid { get; init; }
    public string? Error { get; init; }
    public Guid? UserId { get; init; }
    public string? Email { get; init; }
    public UserType? UserType { get; init; }
    public string[] Roles { get; init; } = [];
    public string[] Permissions { get; init; } = [];
    public DateTimeOffset? ExpiresAt { get; init; }
}

public record MfaTokenValidationResult
{
    public bool IsValid { get; init; }
    public string? Error { get; init; }
    public Guid? UserId { get; init; }
    public string? Email { get; init; }
}
