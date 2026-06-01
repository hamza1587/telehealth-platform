namespace Telehealth.Platform.Prescription.Domain.Models;

public enum PrescriptionStatus
{
    Draft,
    Pending,
    Sent,
    Dispensed,
    Cancelled,
    Expired
}

public class Prescription
{
    public Guid Id { get; private set; }
    public Guid ConsultationId { get; private set; }
    public Guid DoctorId { get; private set; }
    public Guid PatientId { get; private set; }
    public List<PrescriptionItem> Items { get; private set; } = new List<PrescriptionItem>();
    public string CountryCode { get; private set; } = string.Empty;
    public PrescriptionStatus Status { get; private set; }
    public string DigitalSignature { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public DateTimeOffset? SentAt { get; private set; }
    public DateTimeOffset? DispensedAt { get; private set; }
    public string? NationalPrescriptionId { get; private set; }
    public string? PharmacyId { get; private set; }
    public bool IsControlledSubstance { get; private set; }
    public string? DeaNumber { get; private set; }
    public string RoomName { get; private set; } = string.Empty;

    public Prescription()
    {
    }

    public Prescription(Guid consultationId, Guid doctorId, Guid patientId, string countryCode, string roomName = "")
    {
        Id = Guid.NewGuid();
        ConsultationId = consultationId;
        DoctorId = doctorId;
        PatientId = patientId;
        CountryCode = countryCode;
        RoomName = roomName;
        Items = new List<PrescriptionItem>();
        Status = PrescriptionStatus.Draft;
        CreatedAt = DateTimeOffset.UtcNow;
        DigitalSignature = string.Empty;
    }

    public void AddItem(PrescriptionItem item)
    {
        Items.Add(item);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Sign(string digitalSignature)
    {
        DigitalSignature = digitalSignature;
        Status = PrescriptionStatus.Pending;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Send(string nationalPrescriptionId, string? pharmacyId = null)
    {
        NationalPrescriptionId = nationalPrescriptionId;
        PharmacyId = pharmacyId;
        Status = PrescriptionStatus.Sent;
        SentAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Dispense()
    {
        Status = PrescriptionStatus.Dispensed;
        DispensedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Cancel()
    {
        Status = PrescriptionStatus.Cancelled;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkAsControlledSubstance(string deaNumber)
    {
        IsControlledSubstance = true;
        DeaNumber = deaNumber;
    }
}
