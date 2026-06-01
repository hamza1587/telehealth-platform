using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Doctors;

public sealed class DoctorOnboardingRecord : Entity<Guid>
{
    public DoctorOnboardingRecord(
        Guid id,
        Guid doctorProfileId,
        string legalName,
        string email,
        string phoneNumber,
        string countryOfPractice,
        string licenseNumber,
        string licensingAuthority,
        string qualifications,
        int yearsOfExperience,
        string biography,
        string insuranceProvider,
        string insurancePolicyNumber,
        DateOnly? licenseExpiryDate,
        string? reviewerId,
        string? reviewNotes,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
        : base(id)
    {
        DoctorProfileId = doctorProfileId;
        LegalName = legalName;
        Email = email;
        PhoneNumber = phoneNumber;
        CountryOfPractice = countryOfPractice;
        LicenseNumber = licenseNumber;
        LicensingAuthority = licensingAuthority;
        Qualifications = qualifications;
        YearsOfExperience = yearsOfExperience;
        Biography = biography;
        InsuranceProvider = insuranceProvider;
        InsurancePolicyNumber = insurancePolicyNumber;
        LicenseExpiryDate = licenseExpiryDate;
        ReviewerId = reviewerId;
        ReviewNotes = reviewNotes;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public Guid DoctorProfileId { get; private set; }

    public string LegalName { get; private set; }

    public string Email { get; private set; }

    public string PhoneNumber { get; private set; }

    public string CountryOfPractice { get; private set; }

    public string LicenseNumber { get; private set; }

    public string LicensingAuthority { get; private set; }

    public string Qualifications { get; private set; }

    public int YearsOfExperience { get; private set; }

    public string Biography { get; private set; }

    public string InsuranceProvider { get; private set; }

    public string InsurancePolicyNumber { get; private set; }

    public DateOnly? LicenseExpiryDate { get; private set; }

    public string? ReviewerId { get; private set; }

    public string? ReviewNotes { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public void UpdateDetails(
        string legalName,
        string email,
        string phoneNumber,
        string countryOfPractice,
        string licenseNumber,
        string licensingAuthority,
        string qualifications,
        int yearsOfExperience,
        string biography,
        string insuranceProvider,
        string insurancePolicyNumber,
        DateOnly? licenseExpiryDate,
        DateTimeOffset updatedAt)
    {
        LegalName = legalName;
        Email = email;
        PhoneNumber = phoneNumber;
        CountryOfPractice = countryOfPractice;
        LicenseNumber = licenseNumber;
        LicensingAuthority = licensingAuthority;
        Qualifications = qualifications;
        YearsOfExperience = yearsOfExperience;
        Biography = biography;
        InsuranceProvider = insuranceProvider;
        InsurancePolicyNumber = insurancePolicyNumber;
        LicenseExpiryDate = licenseExpiryDate;
        UpdatedAt = updatedAt;
    }

    public void UpdateReview(
        string? reviewerId,
        string? reviewNotes,
        DateTimeOffset updatedAt)
    {
        ReviewerId = reviewerId;
        ReviewNotes = reviewNotes;
        UpdatedAt = updatedAt;
    }
}
