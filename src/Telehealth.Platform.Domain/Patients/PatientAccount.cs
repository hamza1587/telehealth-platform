using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Patients;

public sealed class PatientAccount : Entity<Guid>
{
    public PatientAccount(
        Guid id,
        string displayName,
        string email,
        string medplumPatientId,
        string countryCode,
        string preferredLanguage,
        PatientAccountStatus status,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
        : base(id)
    {
        DisplayName = displayName;
        Email = email;
        MedplumPatientId = medplumPatientId;
        CountryCode = countryCode;
        PreferredLanguage = preferredLanguage;
        Status = status;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public string DisplayName { get; private set; }

    public string Email { get; private set; }

    public string MedplumPatientId { get; }

    public string CountryCode { get; private set; }

    public string PreferredLanguage { get; private set; }

    public PatientAccountStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public void MarkProfileCompleted(
        string displayName,
        string countryCode,
        string preferredLanguage,
        DateTimeOffset updatedAt)
    {
        DisplayName = displayName;
        CountryCode = countryCode;
        PreferredLanguage = preferredLanguage;
        Status = PatientAccountStatus.Active;
        UpdatedAt = updatedAt;
    }
}
