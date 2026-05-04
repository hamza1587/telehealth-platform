namespace Telehealth.Platform.Application.Abstractions.Payments;

public interface IPciComplianceService
{
    Task<string> MaskCardNumberAsync(string cardNumber, CancellationToken cancellationToken = default);

    Task<string> EncryptSensitiveDataAsync(string data, CancellationToken cancellationToken = default);

    Task<string> DecryptSensitiveDataAsync(string encryptedData, CancellationToken cancellationToken = default);

    Task<bool> ValidatePciComplianceAsync(PciComplianceCheckRequest request, CancellationToken cancellationToken = default);

    Task<PciComplianceReport> GenerateComplianceReportAsync(CancellationToken cancellationToken = default);

    Task LogDataAccessKeyAsync(string resourceType, string resourceId, string userId, CancellationToken cancellationToken = default);
}

public class PciComplianceCheckRequest
{
    public string CardNumber { get; set; } = string.Empty;
    public string ExpiryMonth { get; set; } = string.Empty;
    public string ExpiryYear { get; set; } = string.Empty;
    public string Cvv { get; set; } = string.Empty;
    public bool StoreCvv { get; set; }
}

public class PciComplianceReport
{
    public DateTime GeneratedAt { get; set; }
    public bool IsCompliant { get; set; }
    public int CardDataStores { get; set; }
    public int EncryptedFields { get; set; }
    public int AccessLogs { get; set; }
    public List<string> Violations { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
}