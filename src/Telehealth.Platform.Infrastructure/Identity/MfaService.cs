using OtpNet;
using System.Security.Cryptography;
using System.Text;
using Telehealth.Platform.Application.Abstractions.Identity;

namespace Telehealth.Platform.Infrastructure.Identity;

/// <summary>
/// TOTP-based Multi-Factor Authentication service.
/// Implements RFC 6238 (TOTP) and generates recovery codes.
/// </summary>
public sealed class MfaService : IMfaService
{
    // TOTP settings
    private const int TotpStep = 30; // 30-second time step
    private const int TotpDigits = 6;  // 6-digit codes
    private const OtpHashMode TotpMode = OtpHashMode.Sha512;

    public string GenerateSecretKey()
    {
        // Generate a 256-bit (32 byte) random secret
        var key = KeyGeneration.GenerateRandomKey(32);
        return Base32Encoding.ToString(key);
    }

    public string GenerateQrCodeUri(string secretKey, string email, string issuer = "Telehealth Platform")
    {
        // Generate the otpauth URI for QR code generation
        // Format: otpauth://totp/{issuer}:{email}?secret={secret}&issuer={issuer}&algorithm=SHA512&digits=6&period=30
        var encodedIssuer = Uri.EscapeDataString(issuer);
        var encodedEmail = Uri.EscapeDataString(email);

        return $"otpauth://totp/{encodedIssuer}:{encodedEmail}?secret={secretKey}&issuer={encodedIssuer}&algorithm=SHA512&digits={TotpDigits}&period={TotpStep}";
    }

    public bool ValidateCode(string secretKey, string code)
    {
        if (string.IsNullOrWhiteSpace(secretKey) || string.IsNullOrWhiteSpace(code))
        {
            return false;
        }

        try
        {
            var secretBytes = Base32Encoding.ToBytes(secretKey);
            var totp = new Totp(secretBytes, step: TotpStep, mode: TotpMode, totpSize: TotpDigits);

            // Verify with a window of 1 step before and after (tolerance for clock skew)
            // Window parameter: 1 means 1 step before AND 1 step after current time
            return totp.VerifyTotp(code, out long timeStepMatched, new VerificationWindow(previous: 1, future: 1));
        }
        catch
        {
            return false;
        }
    }

    public string[] GenerateRecoveryCodes(int count = 10)
    {
        var codes = new List<string>(count);

        for (int i = 0; i < count; i++)
        {
            // Generate a 10-character alphanumeric code
            var bytes = RandomNumberGenerator.GetBytes(8);
            var code = Convert.ToBase64String(bytes)
                .Replace("+", "")
                .Replace("/", "")
                .Replace("=", "")
                .Substring(0, 10)
                .ToUpperInvariant();

            // Format as XXXXX-XXXXX for readability
            var formatted = $"{code.Substring(0, 5)}-{code.Substring(5, 5)}";
            codes.Add(formatted);
        }

        return codes.ToArray();
    }

    public bool ValidateRecoveryCode(string storedCodes, string code)
    {
        if (string.IsNullOrWhiteSpace(storedCodes) || string.IsNullOrWhiteSpace(code))
        {
            return false;
        }

        try
        {
            // In production, recovery codes should be stored as individual hashed codes
            // This is a simplified implementation
            var codes = storedCodes.Split(',');
            return codes.Any(c => c.Equals(code, StringComparison.OrdinalIgnoreCase));
        }
        catch
        {
            return false;
        }
    }

    public string RemoveUsedRecoveryCode(string storedCodes, string usedCode)
    {
        if (string.IsNullOrWhiteSpace(storedCodes) || string.IsNullOrWhiteSpace(usedCode))
        {
            return storedCodes ?? string.Empty;
        }

        var codes = storedCodes.Split(',').ToList();
        codes.RemoveAll(c => c.Equals(usedCode, StringComparison.OrdinalIgnoreCase));

        return string.Join(",", codes);
    }
}
