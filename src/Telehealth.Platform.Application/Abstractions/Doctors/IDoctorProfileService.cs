using Telehealth.Platform.Domain.Doctors;

namespace Telehealth.Platform.Application.Abstractions.Doctors;

public interface IDoctorProfileService
{
    Task<DoctorProfile?> GetByMedplumIdAsync(Guid medplumPractitionerId, CancellationToken cancellationToken = default);
    Task<DoctorProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DoctorProfile> CreateAsync(Guid medplumPractitionerId, string displayName, string countryCode, string primarySpecialty, int defaultPricePerSecondMinor, string currency, CancellationToken cancellationToken = default);
    Task UpdateProfileAsync(Guid id, string displayName, string countryCode, string primarySpecialty, int defaultPricePerSecondMinor, string currency, CancellationToken cancellationToken = default);
    Task SubmitForVerificationAsync(Guid id, CancellationToken cancellationToken = default);
    Task ApproveVerificationAsync(Guid id, CancellationToken cancellationToken = default);
    Task RejectVerificationAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdateMarketplaceStatusAsync(Guid id, DoctorMarketplaceStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<DoctorProfile>> SearchAsync(string? specialty, string? countryCode, DoctorMarketplaceStatus status, CancellationToken cancellationToken = default);
}