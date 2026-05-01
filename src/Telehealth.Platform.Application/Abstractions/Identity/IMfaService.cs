namespace Telehealth.Platform.Application.Abstractions.Identity;

/// <summary>
/// Service for Multi-Factor Authentication using TOTP (Time-based One-Time Password).
/// </summary>
public interface IMfaService
{
    /// <summary>
    /// Generates a new TOTP secret key for MFA setup.
    /// </summary>
    string GenerateSecretKey();

    /// <summary>
    /// Generates a QR code URI for authenticator app setup.
    /// </summary>
    string GenerateQrCodeUri(string secretKey, string email, string issuer = "Telehealth Platform");

    /// <summary>
    /// Validates a TOTP code against the secret key.
    /// </summary>
    bool ValidateCode(string secretKey, string code);

    /// <summary>
    /// Generates recovery codes for account recovery.
    /// </summary>
    string[] GenerateRecoveryCodes(int count = 10);

    /// <summary>
    /// Validates a recovery code.
    /// </summary>
    bool ValidateRecoveryCode(string storedCodes, string code);

    /// <summary>
    /// Removes a used recovery code from the stored codes.
    /// </summary>
    string RemoveUsedRecoveryCode(string storedCodes, string usedCode);
}
