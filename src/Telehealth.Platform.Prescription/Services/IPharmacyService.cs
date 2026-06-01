namespace Telehealth.Platform.Prescription.Services;

public record Pharmacy(
    string Id,
    string Name,
    string Address,
    string City,
    string CountryCode,
    bool IsOnline,
    bool AcceptsEPrescriptions
);

public interface IPharmacyService
{
    Task<List<Pharmacy>> GetNearbyPharmaciesAsync(string countryCode, string? city = null);
    Task<Pharmacy?> GetPharmacyAsync(string pharmacyId);
    Task<bool> SendToPharmacyAsync(string prescriptionId, string pharmacyId);
}
