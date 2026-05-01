using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Doctors;

public sealed class DoctorProfile : Entity<Guid>
{
    public DoctorProfile(
        Guid id,
        string medplumPractitionerId,
        string displayName,
        string countryCode,
        string primarySpecialty,
        Money pricePerSecond,
        DoctorVerificationStatus verificationStatus,
        DoctorMarketplaceStatus marketplaceStatus,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
        : base(id)
    {
        MedplumPractitionerId = medplumPractitionerId;
        DisplayName = displayName;
        CountryCode = countryCode;
        PrimarySpecialty = primarySpecialty;
        PricePerSecond = pricePerSecond;
        VerificationStatus = verificationStatus;
        MarketplaceStatus = marketplaceStatus;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public string MedplumPractitionerId { get; }

    public string DisplayName { get; private set; }

    public string CountryCode { get; private set; }

    public string PrimarySpecialty { get; private set; }

    public Money PricePerSecond { get; private set; }

    public DoctorVerificationStatus VerificationStatus { get; private set; }

    public DoctorMarketplaceStatus MarketplaceStatus { get; private set; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public void UpdateProfile(
        string displayName,
        string countryCode,
        string primarySpecialty,
        Money pricePerSecond,
        DateTimeOffset updatedAt)
    {
        DisplayName = displayName;
        CountryCode = countryCode;
        PrimarySpecialty = primarySpecialty;
        PricePerSecond = pricePerSecond;
        UpdatedAt = updatedAt;
    }

    public void UpdateVerificationStatus(
        DoctorVerificationStatus verificationStatus,
        DoctorMarketplaceStatus marketplaceStatus,
        DateTimeOffset updatedAt)
    {
        VerificationStatus = verificationStatus;
        MarketplaceStatus = marketplaceStatus;
        UpdatedAt = updatedAt;
    }
}
