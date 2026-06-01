using Telehealth.Platform.Prescription.Domain.Models;

namespace Telehealth.Platform.Prescription.Services;

public interface INationalPrescriptionGateway
{
    Task<string> SendPrescriptionAsync(Domain.Models.Prescription prescription, string? pharmacyId = null);
    Task<bool> ValidateSignatureAsync(string digitalSignature);
    Task<Domain.Models.Prescription?> GetPrescriptionStatusAsync(string nationalPrescriptionId);
}
