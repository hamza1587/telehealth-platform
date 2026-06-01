using Telehealth.Platform.Prescription.Domain.Models;

namespace Telehealth.Platform.Prescription.Services;

public interface IPrescriptionService
{
    Task<Domain.Models.Prescription> CreatePrescriptionAsync(Guid consultationId, Guid doctorId, Guid patientId, string countryCode);
    Task<Domain.Models.Prescription?> GetPrescriptionAsync(Guid prescriptionId);
    Task<Domain.Models.Prescription> AddItemAsync(Guid prescriptionId, Domain.Models.PrescriptionItem item);
    Task<Domain.Models.Prescription> SignPrescriptionAsync(Guid prescriptionId, string digitalSignature);
    Task<Domain.Models.Prescription> SendPrescriptionAsync(Guid prescriptionId, string? pharmacyId = null);
    Task<Domain.Models.Prescription> DispensePrescriptionAsync(Guid prescriptionId);
    Task<Domain.Models.Prescription> CancelPrescriptionAsync(Guid prescriptionId);
    Task<List<Domain.Models.Prescription>> GetPatientPrescriptionsAsync(Guid patientId);
}
