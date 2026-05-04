using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using Telehealth.Platform.Application.Abstractions.Payments;

namespace Telehealth.Platform.Infrastructure.Payments;

public class PciComplianceServiceOptions
{
    public string EncryptionKey { get; set; } = Convert.ToBase64String(new byte[32]);
    public int KeySize { get; set; } = 256;
    public bool CvvStorageAllowed { get; set; } = false;
}

public class PciComplianceService : IPciComplianceService
{
    private readonly IOptions<PciComplianceServiceOptions> _options;
    private readonly ILogger<PciComplianceService> _logger;
    private readonly List<DataAccessKeyLog> _accessLogs = new();

    public PciComplianceService(
        IOptions<PciComplianceServiceOptions> options,
        ILogger<PciComplianceService> logger)
    {
        _options = options;
        _logger = logger;
    }

    public async Task<string> MaskCardNumberAsync(string cardNumber, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            if (string.IsNullOrEmpty(cardNumber) || cardNumber.Length < 4)
            {
                return "****";
            }

            var lastFour = cardNumber.Substring(cardNumber.Length - 4);
            return $"****-****-****-{lastFour}";
        }, cancellationToken);
    }

    public async Task<string> EncryptSensitiveDataAsync(string data, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            if (string.IsNullOrEmpty(data))
            {
                return string.Empty;
            }

            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(_options.Value.EncryptionKey.Substring(0, 32));
            aes.IV = new byte[16];

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            var dataBytes = Encoding.UTF8.GetBytes(data);
            var encrypted = encryptor.TransformFinalBlock(dataBytes, 0, dataBytes.Length);
            return Convert.ToBase64String(encrypted);
        }, cancellationToken);
    }

    public async Task<string> DecryptSensitiveDataAsync(string encryptedData, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            if (string.IsNullOrEmpty(encryptedData))
            {
                return string.Empty;
            }

            try
            {
                using var aes = Aes.Create();
                aes.Key = Encoding.UTF8.GetBytes(_options.Value.EncryptionKey.Substring(0, 32));
                aes.IV = new byte[16];

                using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                var encryptedBytes = Convert.FromBase64String(encryptedData);
                var decrypted = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
                return Encoding.UTF8.GetString(decrypted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to decrypt sensitive data");
                return string.Empty;
            }
        }, cancellationToken);
    }

    public async Task<bool> ValidatePciComplianceAsync(PciComplianceCheckRequest request, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            if (string.IsNullOrEmpty(request.CardNumber))
            {
                _logger.LogWarning("PCI Compliance Check Failed: Card number is empty");
                return false;
            }

            if (!_options.Value.CvvStorageAllowed && !string.IsNullOrEmpty(request.Cvv))
            {
                _logger.LogWarning("PCI Compliance Check Failed: CVV storage is not allowed");
                return false;
            }

            if (string.IsNullOrEmpty(request.ExpiryMonth) || string.IsNullOrEmpty(request.ExpiryYear))
            {
                _logger.LogWarning("PCI Compliance Check Failed: Expiry date is incomplete");
                return false;
            }

            return true;
        }, cancellationToken);
    }

    public async Task<PciComplianceReport> GenerateComplianceReportAsync(CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            return new PciComplianceReport
            {
                GeneratedAt = DateTime.UtcNow,
                IsCompliant = true,
                CardDataStores = 0,
                EncryptedFields = _accessLogs.Count,
                AccessLogs = _accessLogs.Count,
                Violations = [],
                Recommendations = new List<string>
                {
                    "Ensure all card data is encrypted at rest",
                    "Implement regular PCI DSS audits",
                    "Use tokenization for card storage"
                }
            };
        }, cancellationToken);
    }

    public async Task LogDataAccessKeyAsync(string resourceType, string resourceId, string userId, CancellationToken cancellationToken = default)
    {
        await Task.Run(() =>
        {
            _accessLogs.Add(new DataAccessKeyLog
            {
                ResourceType = resourceType,
                ResourceId = resourceId,
                UserId = userId,
                AccessedAt = DateTime.UtcNow
            });
        }, cancellationToken);
    }
}

internal class DataAccessKeyLog
{
    public string ResourceType { get; set; } = string.Empty;
    public string ResourceId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime AccessedAt { get; set; }
}