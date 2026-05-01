using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Telehealth.Platform.Application.Abstractions.ClinicalRecords;
using Telehealth.Platform.Domain.Identity;

namespace Telehealth.Platform.Integrations.Medplum;

/// <summary>
/// Medplum FHIR gateway for creating and managing clinical records.
/// Implements user provisioning when patients and doctors register.
/// </summary>
internal sealed class MedplumClinicalRecordGateway : IClinicalRecordGateway
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<MedplumClinicalRecordGateway> _logger;

    public MedplumClinicalRecordGateway(
        HttpClient httpClient,
        ILogger<MedplumClinicalRecordGateway> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.GetAsync("healthcheck", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<ClinicalRecordResult<string>> CreatePatientAsync(
        PlatformUser user,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Create FHIR Patient resource
            var patientResource = new
            {
                resourceType = "Patient",
                identifier = new[]
                {
                    new
                    {
                        system = "https://telehealth.platform/user-id",
                        value = user.Id.ToString()
                    }
                },
                name = new[]
                {
                    new
                    {
                        use = "official",
                        family = user.LastName ?? "Unknown",
                        given = new[] { user.FirstName ?? "Unknown" }
                    }
                },
                telecom = new List<object>()
            };

            // Add email
            if (!string.IsNullOrEmpty(user.Email))
            {
                ((List<object>)patientResource.telecom).Add(new
                {
                    system = "email",
                    value = user.Email,
                    use = "work"
                });
            }

            // Add phone
            if (!string.IsNullOrEmpty(user.PhoneNumber))
            {
                ((List<object>)patientResource.telecom).Add(new
                {
                    system = "phone",
                    value = user.PhoneNumber,
                    use = "mobile"
                });
            }

            var json = JsonSerializer.Serialize(patientResource);
            var content = new StringContent(json, Encoding.UTF8, "application/fhir+json");

            var response = await _httpClient.PostAsync("fhir/R4/Patient", content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to create Patient in Medplum: {Error}", error);
                return new ClinicalRecordResult<string>
                {
                    Success = false,
                    Error = $"Medplum API error: {response.StatusCode}",
                    ErrorCode = "MEDPLUM_PATIENT_CREATE_FAILED"
                };
            }

            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(responseJson);
            var patientId = doc.RootElement.GetProperty("id").GetString();

            _logger.LogInformation("Created Patient in Medplum with ID: {PatientId}", patientId);

            return new ClinicalRecordResult<string>
            {
                Success = true,
                Data = patientId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception creating Patient in Medplum");
            return new ClinicalRecordResult<string>
            {
                Success = false,
                Error = ex.Message,
                ErrorCode = "MEDPLUM_EXCEPTION"
            };
        }
    }

    public async Task<ClinicalRecordResult<string>> CreatePractitionerAsync(
        PlatformUser user,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Create FHIR Practitioner resource
            var practitionerResource = new
            {
                resourceType = "Practitioner",
                identifier = new[]
                {
                    new
                    {
                        system = "https://telehealth.platform/user-id",
                        value = user.Id.ToString()
                    }
                },
                name = new[]
                {
                    new
                    {
                        use = "official",
                        family = user.LastName ?? "Unknown",
                        given = new[] { user.FirstName ?? "Unknown" }
                    }
                },
                telecom = new List<object>()
            };

            // Add email
            if (!string.IsNullOrEmpty(user.Email))
            {
                ((List<object>)practitionerResource.telecom).Add(new
                {
                    system = "email",
                    value = user.Email,
                    use = "work"
                });
            }

            // Add phone
            if (!string.IsNullOrEmpty(user.PhoneNumber))
            {
                ((List<object>)practitionerResource.telecom).Add(new
                {
                    system = "phone",
                    value = user.PhoneNumber,
                    use = "mobile"
                });
            }

            var json = JsonSerializer.Serialize(practitionerResource);
            var content = new StringContent(json, Encoding.UTF8, "application/fhir+json");

            var response = await _httpClient.PostAsync("fhir/R4/Practitioner", content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to create Practitioner in Medplum: {Error}", error);
                return new ClinicalRecordResult<string>
                {
                    Success = false,
                    Error = $"Medplum API error: {response.StatusCode}",
                    ErrorCode = "MEDPLUM_PRACTITIONER_CREATE_FAILED"
                };
            }

            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(responseJson);
            var practitionerId = doc.RootElement.GetProperty("id").GetString();

            _logger.LogInformation("Created Practitioner in Medplum with ID: {PractitionerId}", practitionerId);

            return new ClinicalRecordResult<string>
            {
                Success = true,
                Data = practitionerId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception creating Practitioner in Medplum");
            return new ClinicalRecordResult<string>
            {
                Success = false,
                Error = ex.Message,
                ErrorCode = "MEDPLUM_EXCEPTION"
            };
        }
    }

    public async Task<ClinicalRecordResult<PatientRecord>> GetPatientAsync(
        string patientId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"fhir/R4/Patient/{patientId}", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return new ClinicalRecordResult<PatientRecord>
                {
                    Success = false,
                    Error = $"Failed to get patient: {response.StatusCode}"
                };
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var record = new PatientRecord
            {
                Id = root.GetProperty("id").GetString() ?? string.Empty,
            };

            // Extract name
            if (root.TryGetProperty("name", out var nameArray) && nameArray.GetArrayLength() > 0)
            {
                var name = nameArray[0];
                if (name.TryGetProperty("family", out var family))
                    record = record with { LastName = family.GetString() };
                if (name.TryGetProperty("given", out var givenArray) && givenArray.GetArrayLength() > 0)
                    record = record with { FirstName = givenArray[0].GetString() };
            }

            // Extract telecom
            if (root.TryGetProperty("telecom", out var telecomArray))
            {
                foreach (var telecom in telecomArray.EnumerateArray())
                {
                    var system = telecom.GetProperty("system").GetString();
                    var value = telecom.GetProperty("value").GetString();
                    
                    if (system == "email")
                        record = record with { Email = value ?? string.Empty };
                    else if (system == "phone")
                        record = record with { PhoneNumber = value };
                }
            }

            return new ClinicalRecordResult<PatientRecord>
            {
                Success = true,
                Data = record
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception getting Patient from Medplum");
            return new ClinicalRecordResult<PatientRecord>
            {
                Success = false,
                Error = ex.Message
            };
        }
    }

    public async Task<ClinicalRecordResult<bool>> UpdatePatientAsync(
        string patientId,
        PatientUpdateData data,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // First get the existing patient
            var getResult = await GetPatientAsync(patientId, cancellationToken);
            if (!getResult.Success)
            {
                return new ClinicalRecordResult<bool>
                {
                    Success = false,
                    Error = "Failed to retrieve existing patient data"
                };
            }

            // Build update resource
            var updateResource = new
            {
                resourceType = "Patient",
                id = patientId,
                name = new[]
                {
                    new
                    {
                        use = "official",
                        family = data.LastName ?? getResult.Data?.LastName ?? "Unknown",
                        given = new[] { data.FirstName ?? getResult.Data?.FirstName ?? "Unknown" }
                    }
                }
            };

            var json = JsonSerializer.Serialize(updateResource);
            var content = new StringContent(json, Encoding.UTF8, "application/fhir+json");

            var response = await _httpClient.PutAsync($"fhir/R4/Patient/{patientId}", content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to update Patient in Medplum: {Error}", error);
                return new ClinicalRecordResult<bool>
                {
                    Success = false,
                    Error = $"Medplum API error: {response.StatusCode}"
                };
            }

            return new ClinicalRecordResult<bool>
            {
                Success = true,
                Data = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception updating Patient in Medplum");
            return new ClinicalRecordResult<bool>
            {
                Success = false,
                Error = ex.Message
            };
        }
    }
}
