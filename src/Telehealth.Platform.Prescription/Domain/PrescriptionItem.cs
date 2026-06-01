namespace Telehealth.Platform.Prescription.Domain.Models;

public class PrescriptionItem
{
    public Guid Id { get; private set; }
    public string MedicationCode { get; private set; } = string.Empty;
    public string MedicationName { get; private set; } = string.Empty;
    public string Dosage { get; private set; } = string.Empty;
    public string Frequency { get; private set; } = string.Empty;
    public int DurationDays { get; private set; }
    public string Instructions { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public string? Refills { get; private set; }
    public bool IsControlledSubstance { get; private set; }
    public Guid RoomId { get; private set; }

    public PrescriptionItem()
    {
    }

    public PrescriptionItem(
        string medicationCode,
        string medicationName,
        string dosage,
        string frequency,
        int durationDays,
        string instructions,
        int quantity,
        Guid roomId,
        string? refills = null,
        bool isControlledSubstance = false)
    {
        Id = Guid.NewGuid();
        MedicationCode = medicationCode;
        MedicationName = medicationName;
        Dosage = dosage;
        Frequency = frequency;
        DurationDays = durationDays;
        Instructions = instructions;
        Quantity = quantity;
        RoomId = roomId;
        Refills = refills;
        IsControlledSubstance = isControlledSubstance;
    }
}
