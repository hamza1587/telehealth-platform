using Telehealth.Platform.Domain.Identity;

namespace Telehealth.Platform.Application.Abstractions.Identity;

/// <summary>
/// Service for managing Multi-Factor Authentication (MFA) with TOTP.
/// </summary>
public interface IMfaService
{
    /// <summary>
    /// Generates a new MFA setup for a user including secret key and QR code URI.
    /// </summary>
    /// <param name="user">The user for whom to generate MFA setup.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>MFA setup result containing secret key and QR code URI.</returns>
    Task<MfaSetupResult> GenerateSetupAsync(PlatformUser user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies a TOTP code for the specified user.
    /// </summary>
    /// <param name="user">The user to verify the code for.</param>
    /// <param name="code">The TOTP code to verify.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the code is valid, false otherwise.</returns>
    Task<bool> VerifyCodeAsync(PlatformUser user, string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Disables MFA for the specified user.
    /// </summary>
    /// <param name="user">The user to disable MFA for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task DisableMfaAsync(PlatformUser user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the QR code URI for manual MFA setup.
    /// </summary>
    /// <param name="user">The user for whom to get the QR code URI.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The QR code URI for scanning with an authenticator app.</returns>
    Task<string> GetQrCodeUriAsync(PlatformUser user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a TOTP code using the provided secret key.
    /// </summary>
    /// <param name="secretKey">The MFA secret key.</param>
    /// <param name="code">The TOTP code to validate.</param>
    /// <returns>True if the code is valid, false otherwise.</returns>
    bool ValidateCode(string secretKey, string code);

    /// <summary>
    /// Generates recovery codes for MFA backup.
    /// </summary>
    /// <param name="count">Number of recovery codes to generate.</param>
    /// <returns>Array of recovery codes.</returns>
    string[] GenerateRecoveryCodes(int count = 10);

    /// <summary>
    /// Validates a recovery code against the stored codes.
    /// </summary>
    /// <param name="recoveryCodes">Comma-separated or newline-separated recovery codes.</param>
    /// <param name="code">The recovery code to validate.</param>
    /// <returns>True if the code is valid, false otherwise.</returns>
    bool ValidateRecoveryCode(string recoveryCodes, string code);

    /// <summary>
    /// Removes a used recovery code from the stored codes.
    /// </summary>
    /// <param name="recoveryCodes">Current recovery codes.</param>
    /// <param name="code">The used recovery code.</param>
    /// <returns>Updated recovery codes string.</returns>
    string RemoveUsedRecoveryCode(string recoveryCodes, string code);

    /// <summary>
    /// Generates a new secret key for MFA.
    /// </summary>
    /// <returns>A new base32-encoded secret key.</returns>
    string GenerateSecretKey();

    /// <summary>
    /// Generates a QR code URI for the specified secret key and email.
    /// </summary>
    /// <param name="secretKey">The MFA secret key.</param>
    /// <param name="email">The user's email address.</param>
    /// <returns>The QR code URI for scanning with an authenticator app.</returns>
    string GenerateQrCodeUri(string secretKey, string email);
}