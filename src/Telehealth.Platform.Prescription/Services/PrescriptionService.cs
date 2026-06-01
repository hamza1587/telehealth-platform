using Microsoft.EntityFrameworkCore;
using Telehealth.Platform.Prescription.Data;
using Telehealth.Platform.Prescription.Domain.Models;

namespace Telehealth.Platform.Prescription.Services;

public class PrescriptionService : IPrescriptionService
{
    private readonly PrescriptionDbContext _context;
    private readonly INationalPrescriptionGateway _nationalGateway;
    private readonly IDrugInteractionService _drugInteractionService;

    public PrescriptionService(
        PrescriptionDbContext context,
        INationalPrescriptionGateway nationalGateway,
        IDrugInteractionService drugInteractionService)
    {
        _context = context;
        _nationalGateway = nationalGateway;
        _drugInteractionService = drugInteractionService;
    }

    public async Task<Domain.Models.Prescription> CreatePrescriptionAsync(Guid consultationId, Guid doctorId, Guid patientId, string countryCode)
    {
        var prescription = new Domain.Models.Prescription(consultationId, doctorId, patientId, countryCode);
        _context.Prescriptions.Add(prescription);
        await _context.SaveChangesAsync();
        return prescription;
    }

    public async Task<Domain.Models.Prescription?> GetPrescriptionAsync(Guid prescriptionId)
    {
        return await _context.Prescriptions.Include(p => p.Items).FirstOrDefaultAsync(p => p.Id == prescriptionId);
    }

    public async Task<Domain.Models.Prescription> AddItemAsync(Guid prescriptionId, Domain.Models.PrescriptionItem item)
    {
        var prescription = await _context.Prescriptions.Include(p => p.Items).FirstOrDefaultAsync(p => p.Id == prescriptionId);
        if (prescription == null)
        {
            throw new ArgumentException("Prescription not found", nameof(prescriptionId));
        }

        // Check for drug interactions
        var interactions = await _drugInteractionService.CheckInteractionsAsync(
            prescription.Items.Select(i => i.MedicationCode).ToList(),
            new List<string> { item.MedicationCode }
        );

        if (interactions.Any(i => i.Severity == InteractionSeverity.High))
        {
            throw new InvalidOperationException("High-severity drug interaction detected");
        }

        prescription.AddItem(item);
        await _context.SaveChangesAsync();
        return prescription;
    }

    public async Task<Domain.Models.Prescription> SignPrescriptionAsync(Guid prescriptionId, string digitalSignature)
    {
        var prescription = await _context.Prescriptions.FindAsync(prescriptionId);
        if (prescription == null)
        {
            throw new ArgumentException("Prescription not found", nameof(prescriptionId));
        }
        
        // Validate digital signature
        var isValid = await _nationalGateway.ValidateSignatureAsync(digitalSignature);
        if (!isValid)
        {
            throw new InvalidOperationException("Invalid digital signature");
        }

        prescription.Sign(digitalSignature);
        await _context.SaveChangesAsync();
        return prescription;
    }

    public async Task<Domain.Models.Prescription> SendPrescriptionAsync(Guid prescriptionId, string? pharmacyId = null)
    {
        var prescription = await _context.Prescriptions.FindAsync(prescriptionId);
        if (prescription == null)
        {
            throw new ArgumentException("Prescription not found", nameof(prescriptionId));
        }

        if (prescription.Status != PrescriptionStatus.Pending)
        {
            throw new InvalidOperationException("Prescription must be signed before sending");
        }

        // Send to national e-prescription system
        var nationalId = await _nationalGateway.SendPrescriptionAsync(prescription, pharmacyId);
        prescription.Send(nationalId, pharmacyId);
        await _context.SaveChangesAsync();
        return prescription;
    }

    public async Task<Domain.Models.Prescription> DispensePrescriptionAsync(Guid prescriptionId)
    {
        var prescription = await _context.Prescriptions.FindAsync(prescriptionId);
        if (prescription == null)
        {
            throw new ArgumentException("Prescription not found", nameof(prescriptionId));
        }

        prescription.Dispense();
        await _context.SaveChangesAsync();
        return prescription;
    }

    public async Task<Domain.Models.Prescription> CancelPrescriptionAsync(Guid prescriptionId)
    {
        var prescription = await _context.Prescriptions.FindAsync(prescriptionId);
        if (prescription == null)
        {
            throw new ArgumentException("Prescription not found", nameof(prescriptionId));
        }

        prescription.Cancel();
        await _context.SaveChangesAsync();
        return prescription;
    }

    public async Task<List<Domain.Models.Prescription>> GetPatientPrescriptionsAsync(Guid patientId)
    {
        var prescriptions = await _context.Prescriptions
            .Where(p => p.PatientId == patientId)
            .Include(p => p.Items)
            .ToListAsync();
        return prescriptions;
    }
}
