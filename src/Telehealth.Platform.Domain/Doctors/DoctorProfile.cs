using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Doctors;

public class DoctorProfile : Entity<Guid>
{
    public Guid MedplumPractitionerId { get; private set; }
    public string DisplayName { get; private set; } = string.Empty;
    public string CountryCode { get; private set; } = string.Empty;
    public string PrimarySpecialty { get; private set; } = string.Empty;
    public DoctorVerificationStatus VerificationStatus { get; private set; } = DoctorVerificationStatus.Draft;
    public DoctorMarketplaceStatus MarketplaceStatus { get; private set; } = DoctorMarketplaceStatus.Hidden;
    public int DefaultPricePerSecondMinor { get; private set; }
    public string Currency { get; private set; } = "USD";
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public long PricePerSecond => DefaultPricePerSecondMinor;

    private DoctorProfile(
        Guid id,
        Guid medplumPractitionerId,
        string displayName,
        string countryCode,
        string primarySpecialty,
        int defaultPricePerSecondMinor,
        string currency) : base(id)
    {
        MedplumPractitionerId = medplumPractitionerId;
        DisplayName = displayName;
        CountryCode = countryCode;
        PrimarySpecialty = primarySpecialty;
        DefaultPricePerSecondMinor = defaultPricePerSecondMinor;
        Currency = currency;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public static DoctorProfile Create(
        Guid medplumPractitionerId,
        string displayName,
        string countryCode,
        string primarySpecialty,
        int defaultPricePerSecondMinor,
        string currency = "USD")
    {
        return new DoctorProfile(
            Guid.NewGuid(),
            medplumPractitionerId,
            displayName,
            countryCode,
            primarySpecialty,
            defaultPricePerSecondMinor,
            currency);
    }

    public void UpdateProfile(string displayName, string countryCode, string primarySpecialty, int defaultPricePerSecondMinor, string currency)
    {
        DisplayName = displayName;
        CountryCode = countryCode;
        PrimarySpecialty = primarySpecialty;
        DefaultPricePerSecondMinor = defaultPricePerSecondMinor;
        Currency = currency;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SubmitForVerification()
    {
        VerificationStatus = DoctorVerificationStatus.Submitted;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ApproveVerification()
    {
        VerificationStatus = DoctorVerificationStatus.Verified;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RejectVerification()
    {
        VerificationStatus = DoctorVerificationStatus.Rejected;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateMarketplaceStatus(DoctorMarketplaceStatus status)
    {
        MarketplaceStatus = status;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateVerificationStatus(DoctorVerificationStatus status)
    {
        VerificationStatus = status;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}