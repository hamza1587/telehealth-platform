using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Telehealth.Platform.Prescription.Services;

/// <summary>
/// Health Gorilla gateway — wraps the Health Gorilla FHIR-based API for lab
/// order submission, result retrieval, and pharmacy-level medication history.
/// Docs: https://developer.healthgorilla.com/
/// </summary>
public class HealthGorillaGateway
{
    private readonly HttpClient _http;
    private readonly IConfiguration _cfg;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _baseUrl;
    private string? _cachedAccessToken;
    private DateTimeOffset _tokenExpiry = DateTimeOffset.MinValue;

    public HealthGorillaGateway(HttpClient http, IConfiguration cfg)
    {
        _http = http;
        _cfg = cfg;
        _clientId = cfg["HealthGorilla:ClientId"] ?? throw new InvalidOperationException("HealthGorilla:ClientId not configured");
        _clientSecret = cfg["HealthGorilla:ClientSecret"] ?? throw new InvalidOperationException("HealthGorilla:ClientSecret not configured");
        _baseUrl = cfg["HealthGorilla:BaseUrl"] ?? "https://sandbox.healthgorilla.com";
        _http.BaseAddress = new Uri(_baseUrl);
    }

    /// <summary>
    /// Submits a lab order as a FHIR ServiceRequest. Returns the order ID.
    /// </summary>
    public async Task<string> SubmitLabOrderAsync(LabOrderRequest request, CancellationToken ct = default)
    {
        await EnsureAccessTokenAsync(ct);

        var fhirServiceRequest = new
        {
            resourceType = "ServiceRequest",
            status = "active",
            intent = "order",
            subject = new { reference = $"Patient/{request.PatientFhirId}" },
            requester = new { reference = $"Practitioner/{request.PractitionerFhirId}" },
            code = new
            {
                coding = request.TestCodes.Select(t => new
                {
                    system = "http://loinc.org",
                    code = t.LoincCode,
                    display = t.DisplayName,
                }),
            },
            reasonCode = request.DiagnosisCodes.Select(d => new
            {
                coding = new[] { new { system = "http://hl7.org/fhir/sid/icd-10-cm", code = d } },
            }).ToList(),
        };

        var response = await _http.PostAsJsonAsync("/fhir/v3/ServiceRequest", fhirServiceRequest, ct);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<FhirCreatedResponse>(cancellationToken: ct);
        return result?.Id ?? throw new InvalidOperationException("Lab order submission returned no ID");
    }

    /// <summary>
    /// Retrieves lab results for a patient as FHIR DiagnosticReport resources.
    /// </summary>
    public async Task<IReadOnlyList<LabResult>> GetLabResultsAsync(string patientFhirId, CancellationToken ct = default)
    {
        await EnsureAccessTokenAsync(ct);
        var response = await _http.GetAsync($"/fhir/v3/DiagnosticReport?subject=Patient/{patientFhirId}&_sort=-date&_count=20", ct);
        response.EnsureSuccessStatusCode();
        var bundle = await response.Content.ReadFromJsonAsync<FhirBundle<FhirDiagnosticReport>>(cancellationToken: ct);

        return bundle?.Entry?.Select(e => new LabResult(
            e.Resource.Id,
            e.Resource.Code?.Coding?.FirstOrDefault()?.Display ?? "Unknown",
            e.Resource.Status,
            e.Resource.Issued,
            e.Resource.Conclusion
        )).ToList() ?? [];
    }

    /// <summary>
    /// Retrieves medication history for a patient from Health Gorilla's
    /// pharmacy network aggregation (SureScripts).
    /// </summary>
    public async Task<IReadOnlyList<MedicationHistoryItem>> GetMedicationHistoryAsync(string patientFhirId, CancellationToken ct = default)
    {
        await EnsureAccessTokenAsync(ct);
        var response = await _http.GetAsync($"/fhir/v3/MedicationRequest?subject=Patient/{patientFhirId}&_sort=-_lastUpdated&_count=50", ct);
        response.EnsureSuccessStatusCode();
        var bundle = await response.Content.ReadFromJsonAsync<FhirBundle<FhirMedicationRequest>>(cancellationToken: ct);

        return bundle?.Entry?.Select(e => new MedicationHistoryItem(
            e.Resource.Id,
            e.Resource.MedicationCodeableConcept?.Coding?.FirstOrDefault()?.Display ?? "Unknown",
            e.Resource.Status,
            e.Resource.AuthoredOn,
            e.Resource.DosageInstruction?.FirstOrDefault()?.Text
        )).ToList() ?? [];
    }

    private async Task EnsureAccessTokenAsync(CancellationToken ct)
    {
        if (_cachedAccessToken is not null && DateTimeOffset.UtcNow < _tokenExpiry.AddMinutes(-1))
            return;

        var form = new FormUrlEncodedContent([
            new("grant_type", "client_credentials"),
            new("client_id", _clientId),
            new("client_secret", _clientSecret),
        ]);

        var response = await _http.PostAsync("/oauth2/token", form, ct);
        response.EnsureSuccessStatusCode();
        var token = await response.Content.ReadFromJsonAsync<OAuthTokenResponse>(cancellationToken: ct);
        _cachedAccessToken = token!.AccessToken;
        _tokenExpiry = DateTimeOffset.UtcNow.AddSeconds(token.ExpiresIn);

        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _cachedAccessToken);
    }
}

// ── Request / result types ───────────────────────────────────────────────────

public record LabOrderRequest(
    string PatientFhirId,
    string PractitionerFhirId,
    IReadOnlyList<LabTestCode> TestCodes,
    IReadOnlyList<string> DiagnosisCodes);

public record LabTestCode(string LoincCode, string DisplayName);

public record LabResult(
    string Id,
    string TestName,
    string Status,
    DateTimeOffset? Issued,
    string? Conclusion);

public record MedicationHistoryItem(
    string Id,
    string MedicationName,
    string Status,
    DateTimeOffset? AuthoredOn,
    string? DosageText);

internal record OAuthTokenResponse(
    string AccessToken,
    int ExpiresIn,
    string TokenType);

internal record FhirCreatedResponse(string Id, string ResourceType);

internal record FhirBundle<T>(
    string ResourceType,
    int Total,
    IReadOnlyList<FhirBundleEntry<T>>? Entry);

internal record FhirBundleEntry<T>(T Resource);

internal record FhirDiagnosticReport(
    string Id,
    FhirCoding? Code,
    string Status,
    DateTimeOffset? Issued,
    string? Conclusion);

internal record FhirMedicationRequest(
    string Id,
    FhirCoding? MedicationCodeableConcept,
    string Status,
    DateTimeOffset? AuthoredOn,
    IReadOnlyList<FhirDosage>? DosageInstruction);

internal record FhirCoding(IReadOnlyList<FhirCodingItem>? Coding);

internal record FhirCodingItem(string System, string Code, string Display);

internal record FhirDosage(string? Text);
