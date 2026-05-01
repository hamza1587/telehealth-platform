using Telehealth.Platform.Domain.Identity;

namespace Telehealth.Platform.Application.Abstractions.ClinicalRecords;

public interface IClinicalRecordGateway
{
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Creates a Patient resource in Medplum when a new patient registers.
    /// </summary>
    Task<ClinicalRecordResult<string>> CreatePatientAsync(
        PlatformUser user,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Creates a Practitioner resource in Medplum when a new doctor registers.
    /// </summary>
    Task<ClinicalRecordResult<string>> CreatePractitionerAsync(
        PlatformUser user,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a Patient resource by ID.
    /// </summary>
    Task<ClinicalRecordResult<PatientRecord>> GetPatientAsync(
        string patientId,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates a Patient resource.
    /// </summary>
    Task<ClinicalRecordResult<bool>> UpdatePatientAsync(
        string patientId,
        PatientUpdateData data,
        CancellationToken cancellationToken = default);
}

public record ClinicalRecordResult<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public string? Error { get; init; }
    public string? ErrorCode { get; init; }
}

public record PatientRecord
{
    public string Id { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? PhoneNumber { get; init; }
    public DateTime? BirthDate { get; init; }
    public string? Gender { get; init; }
    public string? Address { get; init; }
}

public record PatientUpdateData
{
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? PhoneNumber { get; init; }
    public DateTime? BirthDate { get; init; }
    public string? Gender { get; init; }
}

public record PractitionerRecord
{
    public string Id { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? PhoneNumber { get; init; }
    public string? Qualification { get; init; }
    public string? Specialty { get; init; }
    public string? LicenseNumber { get; init; }
}
