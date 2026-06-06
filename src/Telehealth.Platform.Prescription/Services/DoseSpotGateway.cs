using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Telehealth.Platform.Prescription.Services;

/// <summary>
/// DoseSpot eRx gateway — wraps the DoseSpot v2 API for SSO session creation,
/// write prescription (CPOE), and prescription status retrieval.
/// Docs: https://docs.dosespot.com/
/// </summary>
public class DoseSpotGateway
{
    private readonly HttpClient _http;
    private readonly IConfiguration _cfg;
    private readonly string _clinicId;
    private readonly string _clinicKey;

    public DoseSpotGateway(HttpClient http, IConfiguration cfg)
    {
        _http = http;
        _cfg = cfg;
        _clinicId = cfg["DoseSpot:ClinicId"] ?? throw new InvalidOperationException("DoseSpot:ClinicId not configured");
        _clinicKey = cfg["DoseSpot:ClinicKey"] ?? throw new InvalidOperationException("DoseSpot:ClinicKey not configured");
        _http.BaseAddress = new Uri(cfg["DoseSpot:BaseUrl"] ?? "https://my.staging.dosespot.com");
    }

    /// <summary>
    /// Creates a DoseSpot SSO session and returns the iframe URL for the prescriber UI.
    /// </summary>
    public async Task<DoseSpotSsoResult> CreateSsoSessionAsync(string prescriberId, string patientId, CancellationToken ct = default)
    {
        var token = GenerateSsoToken(prescriberId);
        var response = await _http.PostAsJsonAsync("/webapi/api/sso", new
        {
            SingleSignOnClinicId = _clinicId,
            SingleSignOnCode = token,
            SingleSignOnUserId = prescriberId,
            SingleSignOnUserIdVerify = GenerateVerifyHash(prescriberId),
            PatientId = patientId,
        }, ct);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<DoseSpotSsoResponse>(cancellationToken: ct);
        return new DoseSpotSsoResult(result!.IFrameUrl, result.SingleSignOnCode);
    }

    /// <summary>
    /// Retrieves the status of a prescription from DoseSpot by its external ID.
    /// </summary>
    public async Task<DoseSpotPrescriptionStatus> GetPrescriptionStatusAsync(string doseSpotPrescriptionId, CancellationToken ct = default)
    {
        var response = await _http.GetAsync($"/webapi/api/prescriptions/{doseSpotPrescriptionId}", ct);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<DoseSpotPrescriptionResponse>(cancellationToken: ct);
        return new DoseSpotPrescriptionStatus(
            result!.PrescriptionId,
            result.Status,
            result.PharmacyName,
            result.DateWritten,
            result.RxNumber
        );
    }

    /// <summary>
    /// Checks drug interactions for a list of drug identifiers using the DoseSpot API.
    /// </summary>
    public async Task<IReadOnlyList<DrugInteraction>> CheckDrugInteractionsAsync(
        IEnumerable<string> drugIds, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("/webapi/api/drug-interactions", new
        {
            DrugIds = drugIds,
        }, ct);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<DrugInteractionResponse>(cancellationToken: ct);
        return result?.Interactions ?? [];
    }

    private string GenerateSsoToken(string userId)
    {
        // DoseSpot SSO: MD5(clinicKey + clinicId + userId)
        var raw = $"{_clinicKey}{_clinicId}{userId}";
        var hash = MD5.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private string GenerateVerifyHash(string userId)
    {
        var raw = $"{_clinicKey}{_clinicId}{userId}{_clinicKey}";
        var hash = MD5.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}

// ── Response / result types ──────────────────────────────────────────────────

public record DoseSpotSsoResult(string IFrameUrl, string SsoCode);

public record DoseSpotPrescriptionStatus(
    string PrescriptionId,
    string Status,
    string PharmacyName,
    DateTimeOffset DateWritten,
    string? RxNumber);

public record DrugInteraction(
    string Drug1Name,
    string Drug2Name,
    string Severity,
    string Description);

internal record DoseSpotSsoResponse(string IFrameUrl, string SingleSignOnCode);

internal record DoseSpotPrescriptionResponse(
    string PrescriptionId,
    string Status,
    string PharmacyName,
    DateTimeOffset DateWritten,
    string? RxNumber);

internal record DrugInteractionResponse(IReadOnlyList<DrugInteraction> Interactions);
