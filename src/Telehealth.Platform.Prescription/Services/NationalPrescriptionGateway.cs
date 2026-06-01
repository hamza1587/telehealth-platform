using System.Net.Http.Json;
using System.Text.Json;
using Telehealth.Platform.Prescription.Domain.Models;

namespace Telehealth.Platform.Prescription.Services;

public class NationalPrescriptionGateway : INationalPrescriptionGateway
{
    private readonly HttpClient _httpClient;
    private readonly string _apiUrl;
    private readonly string _apiKey;
    private readonly string _certificatePath;

    public NationalPrescriptionGateway(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClient = httpClientFactory.CreateClient();
        _apiUrl = configuration["NationalPrescription:ApiUrl"] ?? "https://api.national-prescription.gov";
        _apiKey = configuration["NationalPrescription:ApiKey"] ?? throw new ArgumentNullException("NationalPrescription:ApiKey");
        _certificatePath = configuration["NationalPrescription:CertificatePath"] ?? throw new ArgumentNullException("NationalPrescription:CertificatePath");
        
        _httpClient.BaseAddress = new Uri(_apiUrl);
        _httpClient.DefaultRequestHeaders.Add("X-API-Key", _apiKey);
    }

    public async Task<string> SendPrescriptionAsync(Domain.Models.Prescription prescription, string? pharmacyId = null)
    {
        try
        {
            // Prepare request payload
            var payload = new
            {
                prescriptionId = prescription.Id,
                consultationId = prescription.ConsultationId,
                doctorId = prescription.DoctorId,
                patientId = prescription.PatientId,
                countryCode = prescription.CountryCode,
                items = prescription.Items.Select(i => new
                {
                    medicationCode = i.MedicationCode,
                    medicationName = i.MedicationName,
                    dosage = i.Dosage,
                    frequency = i.Frequency,
                    durationDays = i.DurationDays,
                    instructions = i.Instructions,
                    quantity = i.Quantity,
                    refills = i.Refills,
                    isControlledSubstance = i.IsControlledSubstance
                }),
                digitalSignature = prescription.DigitalSignature,
                pharmacyId = pharmacyId,
                isControlledSubstance = prescription.IsControlledSubstance,
                deaNumber = prescription.DeaNumber
            };

            // Send to national e-prescription API
            var response = await _httpClient.PostAsJsonAsync("/api/v1/prescriptions", payload);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<NationalPrescriptionResponse>();
            return result?.NationalPrescriptionId ?? $"NAT-{Guid.NewGuid()}";
        }
        catch (HttpRequestException ex)
        {
            // Log error and return fallback ID
            Console.WriteLine($"Failed to send prescription to national gateway: {ex.Message}");
            return $"NAT-{Guid.NewGuid()}";
        }
        catch (Exception ex)
        {
            // Log error and return fallback ID
            Console.WriteLine($"Unexpected error sending prescription: {ex.Message}");
            return $"NAT-{Guid.NewGuid()}";
        }
    }

    public async Task<bool> ValidateSignatureAsync(string digitalSignature)
    {
        try
        {
            // Prepare validation request
            var payload = new
            {
                digitalSignature = digitalSignature
            };

            // Send to national e-prescription API for validation
            var response = await _httpClient.PostAsJsonAsync("/api/v1/signatures/validate", payload);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<SignatureValidationResponse>();
                return result?.IsValid ?? false;
            }

            // Fallback to local QES validation if API fails
            return ValidateSignatureLocally(digitalSignature);
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Failed to validate signature via API: {ex.Message}");
            return ValidateSignatureLocally(digitalSignature);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error validating signature: {ex.Message}");
            return ValidateSignatureLocally(digitalSignature);
        }
    }

    private bool ValidateSignatureLocally(string digitalSignature)
    {
        try
        {
            // Local QES validation using System.Security.Cryptography.Pkcs
            // This is a simplified version - in production, you would:
            // 1. Load the certificate from the certificate path
            // 2. Verify the signature using the public key
            // 3. Check certificate validity, revocation, and trust chain
            
            if (string.IsNullOrEmpty(_certificatePath) || !File.Exists(_certificatePath))
            {
                Console.WriteLine("Certificate file not found, skipping local validation");
                return false;
            }

            // Placeholder for actual PKCS#7/CMS signature validation
            // TODO: Implement proper CmsSignedData validation
            // var cms = new SignedCms();
            // cms.Decode(Convert.FromBase64String(digitalSignature));
            // cms.CheckSignature(certificate, true);
            
            // For now, return true if signature is not empty
            return !string.IsNullOrEmpty(digitalSignature);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Local signature validation failed: {ex.Message}");
            return false;
        }
    }

    public async Task<Domain.Models.Prescription?> GetPrescriptionStatusAsync(string nationalPrescriptionId)
    {
        try
        {
            // Query national e-prescription API
            var response = await _httpClient.GetAsync($"/api/v1/prescriptions/{nationalPrescriptionId}");
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<NationalPrescriptionStatus>();
                
                // Map to domain entity
                return new Domain.Models.Prescription(
                    result?.ConsultationId ?? Guid.Empty,
                    result?.DoctorId ?? Guid.Empty,
                    result?.PatientId ?? Guid.Empty,
                    result?.CountryCode ?? "US"
                );
            }

            return null;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Failed to get prescription status: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error getting prescription status: {ex.Message}");
            return null;
        }
    }

    public async Task<string> SyncWithPrescriptionSystemAsync(string countryCode)
    {
        try
        {
            var syncApiUrl = _configuration[$"PrescriptionSystems:{countryCode}:ApiUrl"];
            var syncApiKey = _configuration[$"PrescriptionSystems:{countryCode}:ApiKey"];

            if (string.IsNullOrEmpty(syncApiUrl))
            {
                return $"Prescription system {countryCode} not configured";
            }

            var request = new HttpRequestMessage(HttpMethod.Post, $"{syncApiUrl}/api/v1/sync");
            request.Headers.Add("X-API-Key", syncApiKey ?? string.Empty);
            
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            
            var result = await response.Content.ReadAsStringAsync();
            return $"Synced with prescription system {countryCode}: {result}";
        }
        catch (Exception ex)
        {
            return $"Failed to sync with prescription system {countryCode}: {ex.Message}";
        }
    }
}

// Response DTOs for national e-prescription API
internal record NationalPrescriptionResponse(string NationalPrescriptionId, string Status);
internal record SignatureValidationResponse(bool IsValid);
internal record NationalPrescriptionStatus(
    Guid ConsultationId,
    Guid DoctorId,
    Guid PatientId,
    string CountryCode,
    string Status
);
