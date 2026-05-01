namespace Telehealth.Platform.Application.Abstractions.Patients;

/// <summary>
/// Service for patient onboarding and profile management.
/// </summary>
public interface IPatientOnboardingService
{
    /// <summary>
    /// Creates or updates the patient medical profile.
    /// </summary>
    Task<CreateMedicalProfileResult> CreateMedicalProfileAsync(
        Guid patientAccountId,
        CreateMedicalProfileRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the patient medical profile.
    /// </summary>
    Task<MedicalProfileDto?> GetMedicalProfileAsync(
        Guid patientAccountId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records consent acceptance.
    /// </summary>
    Task RecordConsentAsync(
        Guid patientAccountId,
        RecordConsentRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all consent records for a patient.
    /// </summary>
    Task<List<ConsentRecordDto>> GetConsentRecordsAsync(
        Guid patientAccountId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Withdraws a consent.
    /// </summary>
    Task WithdrawConsentAsync(
        Guid patientAccountId,
        string consentType,
        CancellationToken cancellationToken = default);
}

public record CreateMedicalProfileRequest
{
    public DateOnly DateOfBirth { get; init; }
    public string SexAtBirth { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string CountryCode { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string TimeZone { get; init; } = string.Empty;
    public string EmergencyContactName { get; init; } = string.Empty;
    public string EmergencyContactPhone { get; init; } = string.Empty;
    public string EmergencyContactRelationship { get; init; } = string.Empty;
    public string ChiefConcern { get; init; } = string.Empty;
    public string Symptoms { get; init; } = string.Empty;
    public string SymptomDuration { get; init; } = string.Empty;
    public string CurrentMedications { get; init; } = string.Empty;
    public string Allergies { get; init; } = string.Empty;
    public string KnownConditions { get; init; } = string.Empty;
    public string PastSurgeries { get; init; } = string.Empty;
    public string PregnancyStatus { get; init; } = string.Empty;
    public string LifestyleFactors { get; init; } = string.Empty;
    public string PreferredConsultationLanguage { get; init; } = string.Empty;
    public string UrgencyLevel { get; init; } = string.Empty;
    public bool EmergencySymptoms { get; init; }
    public bool MedicalDisclaimerAccepted { get; init; }
}

public record CreateMedicalProfileResult
{
    public bool Success { get; init; }
    public string? Error { get; init; }
    public MedicalProfileDto? Profile { get; init; }
}

public record MedicalProfileDto
{
    public Guid Id { get; init; }
    public Guid PatientAccountId { get; init; }
    public DateOnly DateOfBirth { get; init; }
    public string SexAtBirth { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string CountryCode { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string TimeZone { get; init; } = string.Empty;
    public string EmergencyContactName { get; init; } = string.Empty;
    public string EmergencyContactPhone { get; init; } = string.Empty;
    public string EmergencyContactRelationship { get; init; } = string.Empty;
    public string ChiefConcern { get; init; } = string.Empty;
    public string Symptoms { get; init; } = string.Empty;
    public string SymptomDuration { get; init; } = string.Empty;
    public string CurrentMedications { get; init; } = string.Empty;
    public string Allergies { get; init; } = string.Empty;
    public string KnownConditions { get; init; } = string.Empty;
    public string PastSurgeries { get; init; } = string.Empty;
    public string PregnancyStatus { get; init; } = string.Empty;
    public string LifestyleFactors { get; init; } = string.Empty;
    public string PreferredConsultationLanguage { get; init; } = string.Empty;
    public string UrgencyLevel { get; init; } = string.Empty;
    public bool EmergencySymptoms { get; init; }
    public bool MedicalDisclaimerAccepted { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public record RecordConsentRequest
{
    public string ConsentType { get; init; } = string.Empty;
    public string Version { get; init; } = string.Empty;
    public string TextSnapshot { get; init; } = string.Empty;
    public string TextHash { get; init; } = string.Empty;
    public string Language { get; init; } = string.Empty;
    public string LegalBasis { get; init; } = string.Empty;
    public bool IsAccepted { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
}

public record ConsentRecordDto
{
    public Guid Id { get; init; }
    public Guid PatientAccountId { get; init; }
    public string ConsentType { get; init; } = string.Empty;
    public string Version { get; init; } = string.Empty;
    public string TextSnapshot { get; init; } = string.Empty;
    public string TextHash { get; init; } = string.Empty;
    public string Language { get; init; } = string.Empty;
    public string LegalBasis { get; init; } = string.Empty;
    public bool IsAccepted { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
    public DateTimeOffset CapturedAt { get; init; }
    public DateTimeOffset? WithdrawnAt { get; init; }
}
