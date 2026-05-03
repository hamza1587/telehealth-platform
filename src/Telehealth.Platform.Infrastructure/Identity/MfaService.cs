using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using OtpNet;
using Telehealth.Platform.Application.Abstractions.Identity;
using Telehealth.Platform.Domain.Identity;

namespace Telehealth.Platform.Infrastructure.Identity;

/// <summary>
/// Implementation of MFA service using TOTP (Time-based One-Time Password).
/// </summary>
public class MfaService : IMfaService
{
    private readonly ILogger<MfaService> _logger;

    /// <summary>
    /// Initializes a new instance of the MfaService.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    public MfaService(ILogger<MfaService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public Task<string> GetQrCodeUriAsync(PlatformUser user, CancellationToken cancellationToken = default)
    {
        var secretKey = user.MfaSecretKey ?? string.Empty;
        var issuer = "Telehealth Platform";
        var label = $"{issuer}:{user.Email}";
        var uri = $"otpauth://totp/{Uri.EscapeDataString(label)}?secret={secretKey}&issuer={Uri.EscapeDataString(issuer)}";
        return Task.FromResult(uri);
    }

    /// <inheritdoc/>
    public string GenerateSecretKey()
    {
        var bytes = new byte[20];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Base32Encoding.ToString(bytes);
    }

    /// <inheritdoc/>
    public string GenerateQrCodeUri(string secretKey, string email)
    {
        var issuer = "Telehealth Platform";
        var label = $"{issuer}:{email}";
        return $"otpauth://totp/{Uri.EscapeDataString(label)}?secret={secretKey}&issuer={Uri.EscapeDataString(issuer)}";
    }

    /// <inheritdoc/>
    public async Task<MfaSetupResult> GenerateSetupAsync(
        PlatformUser user,
        CancellationToken cancellationToken = default)
    {
        var secretKey = GenerateSecretKey();
        var issuer = "Telehealth Platform";
        var label = $"{issuer}:{user.Email}";
        var uri = $"otpauth://totp/{Uri.EscapeDataString(label)}?secret={secretKey}&issuer={Uri.EscapeDataString(issuer)}";

        user.SetMfaSecretKey(secretKey);
        user.EnableMfa();

        _logger.LogInformation("Generated MFA setup for user {UserId}", user.Id);

        return await Task.FromResult(new MfaSetupResult
        {
            Success = true,
            SecretKey = secretKey,
            QrCodeUri = uri
        });
    }

    /// <inheritdoc/>
    public async Task<bool> VerifyCodeAsync(
        PlatformUser user,
        string code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(user.MfaSecretKey))
        {
            _logger.LogWarning("MFA not enabled for user {UserId}", user.Id);
            return false;
        }

        if (string.IsNullOrEmpty(code))
        {
            _logger.LogWarning("Empty code provided for user {UserId}", user.Id);
            return false;
        }

        try
        {
            var totp = new Totp(Base32Encoding.ToBytes(user.MfaSecretKey));
            var window = 2;

            var isValid = totp.VerifyTotp(code, out _, new VerificationWindow(window, window));

            if (isValid)
            {
                _logger.LogInformation("MFA code verified for user {UserId}", user.Id);
            }
            else
            {
                _logger.LogWarning("Invalid MFA code for user {UserId}", user.Id);
            }

            return await Task.FromResult(isValid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying MFA code for user {UserId}", user.Id);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task DisableMfaAsync(PlatformUser user, CancellationToken cancellationToken = default)
    {
        user.DisableMfa();
        _logger.LogInformation("Disabled MFA for user {UserId}", user.Id);
        await Task.CompletedTask;
    }

    public bool ValidateCode(string secretKey, string code)
    {
        if (string.IsNullOrEmpty(secretKey) || string.IsNullOrEmpty(code))
            return false;

        try
        {
            var totp = new Totp(Base32Encoding.ToBytes(secretKey));
            var window = 2;
            return totp.VerifyTotp(code, out _, new VerificationWindow(window, window));
        }
        catch
        {
            return false;
        }
    }

    public string[] GenerateRecoveryCodes(int count = 10)
    {
        var codes = new string[count];
        using var rng = RandomNumberGenerator.Create();
        
        for (int i = 0; i < count; i++)
        {
            var bytes = new byte[8];
            rng.GetBytes(bytes);
            codes[i] = Convert.ToBase64String(bytes).Substring(0, 8).Replace("=", "");
        }
        
        return codes;
    }

    public bool ValidateRecoveryCode(string recoveryCodes, string code)
    {
        if (string.IsNullOrEmpty(recoveryCodes) || string.IsNullOrEmpty(code))
            return false;

        var codes = recoveryCodes.Split(new[] { ',', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        return codes.Contains(code, StringComparer.OrdinalIgnoreCase);
    }

    public string RemoveUsedRecoveryCode(string recoveryCodes, string code)
    {
        if (string.IsNullOrEmpty(recoveryCodes) || string.IsNullOrEmpty(code))
            return recoveryCodes;

        var codes = recoveryCodes.Split(new[] { ',', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        var filtered = codes.Where(c => !c.Equals(code, StringComparison.OrdinalIgnoreCase)).ToArray();
        return string.Join(",", filtered);
    }
}