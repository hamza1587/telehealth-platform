using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Patients;

public class PatientAccount : Entity<Guid>
{
    public Guid MedplumPatientId { get; private set; }
    public string DisplayName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string CountryCode { get; private set; } = string.Empty;
    public string PreferredLanguage { get; private set; } = "en";
    public PatientAccountStatus Status { get; private set; } = PatientAccountStatus.Active;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private PatientAccount(
        Guid id,
        Guid medplumPatientId,
        string displayName,
        string email,
        string countryCode,
        string preferredLanguage) : base(id)
    {
        MedplumPatientId = medplumPatientId;
        DisplayName = displayName;
        Email = email;
        CountryCode = countryCode;
        PreferredLanguage = preferredLanguage;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public static PatientAccount Create(
        Guid medplumPatientId,
        string displayName,
        string email,
        string countryCode,
        string preferredLanguage = "en")
    {
        return new PatientAccount(
            Guid.NewGuid(),
            medplumPatientId,
            displayName,
            email,
            countryCode,
            preferredLanguage);
    }

    public void UpdateProfile(string displayName, string email, string countryCode, string preferredLanguage)
    {
        DisplayName = displayName;
        Email = email;
        CountryCode = countryCode;
        PreferredLanguage = preferredLanguage;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Deactivate()
    {
        Status = PatientAccountStatus.Deactivated;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Activate()
    {
        Status = PatientAccountStatus.Active;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkProfileCompleted()
    {
        Status = PatientAccountStatus.Active;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}

public enum PatientAccountStatus
{
    Active = 1,
    PendingOnboarding = 2,
    Deactivated = 3,
    Suspended = 4
}