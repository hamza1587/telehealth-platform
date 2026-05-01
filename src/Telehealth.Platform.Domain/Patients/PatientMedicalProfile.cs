using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Patients;

public sealed class PatientMedicalProfile : Entity<Guid>
{
    public PatientMedicalProfile(
        Guid id,
        Guid patientAccountId,
        DateOnly dateOfBirth,
        string sexAtBirth,
        string phoneNumber,
        string countryCode,
        string city,
        string timeZone,
        string emergencyContactName,
        string emergencyContactPhone,
        string emergencyContactRelationship,
        string chiefConcern,
        string symptoms,
        string symptomDuration,
        string currentMedications,
        string allergies,
        string knownConditions,
        string pastSurgeries,
        string pregnancyStatus,
        string lifestyleFactors,
        string preferredConsultationLanguage,
        string urgencyLevel,
        bool emergencySymptoms,
        bool medicalDisclaimerAccepted,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
        : base(id)
    {
        PatientAccountId = patientAccountId;
        DateOfBirth = dateOfBirth;
        SexAtBirth = sexAtBirth;
        PhoneNumber = phoneNumber;
        CountryCode = countryCode;
        City = city;
        TimeZone = timeZone;
        EmergencyContactName = emergencyContactName;
        EmergencyContactPhone = emergencyContactPhone;
        EmergencyContactRelationship = emergencyContactRelationship;
        ChiefConcern = chiefConcern;
        Symptoms = symptoms;
        SymptomDuration = symptomDuration;
        CurrentMedications = currentMedications;
        Allergies = allergies;
        KnownConditions = knownConditions;
        PastSurgeries = pastSurgeries;
        PregnancyStatus = pregnancyStatus;
        LifestyleFactors = lifestyleFactors;
        PreferredConsultationLanguage = preferredConsultationLanguage;
        UrgencyLevel = urgencyLevel;
        EmergencySymptoms = emergencySymptoms;
        MedicalDisclaimerAccepted = medicalDisclaimerAccepted;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public Guid PatientAccountId { get; }

    public DateOnly DateOfBirth { get; private set; }

    public string SexAtBirth { get; private set; }

    public string PhoneNumber { get; private set; }

    public string CountryCode { get; private set; }

    public string City { get; private set; }

    public string TimeZone { get; private set; }

    public string EmergencyContactName { get; private set; }

    public string EmergencyContactPhone { get; private set; }

    public string EmergencyContactRelationship { get; private set; }

    public string ChiefConcern { get; private set; }

    public string Symptoms { get; private set; }

    public string SymptomDuration { get; private set; }

    public string CurrentMedications { get; private set; }

    public string Allergies { get; private set; }

    public string KnownConditions { get; private set; }

    public string PastSurgeries { get; private set; }

    public string PregnancyStatus { get; private set; }

    public string LifestyleFactors { get; private set; }

    public string PreferredConsultationLanguage { get; private set; }

    public string UrgencyLevel { get; private set; }

    public bool EmergencySymptoms { get; private set; }

    public bool MedicalDisclaimerAccepted { get; private set; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset UpdatedAt { get; private set; }
}
