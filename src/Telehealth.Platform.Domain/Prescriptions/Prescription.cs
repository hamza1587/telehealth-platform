using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Prescriptions;

/// <summary>
/// Prescription for medications or treatments.
/// </summary>
public sealed class Prescription : Entity<Guid>
{
    public Prescription(
        Guid id,
        Guid consultationSessionId,
        Guid patientAccountId,
        Guid doctorProfileId,
        string prescriptionType,
        string? notes,
        int? durationDays,
        DateOnly? startDate,
        DateOnly? endDate,
        int? refillsAllowed,
        PrescriptionStatus status,
        DateTimeOffset createdAt)
        : base(id)
    {
        ConsultationSessionId = consultationSessionId;
        PatientAccountId = patientAccountId;
        DoctorProfileId = doctorProfileId;
        PrescriptionType = prescriptionType;
        Notes = notes;
        DurationDays = durationDays;
        StartDate = startDate;
        EndDate = endDate;
        RefillsAllowed = refillsAllowed;
        RefillsUsed = 0;
        Status = status;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public Guid ConsultationSessionId { get; private set; }
    public Guid PatientAccountId { get; private set; }
    public Guid DoctorProfileId { get; private set; }
    public string PrescriptionType { get; private set; }
    public string? Notes { get; private set; }
    public int? DurationDays { get; private set; }
    public DateOnly? StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }
    public int? RefillsAllowed { get; private set; }
    public int RefillsUsed { get; private set; }
    public PrescriptionStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? DispensedAt { get; private set; }
    public string? PharmacyId { get; private set; }

    public void AddRefill(DateTimeOffset dispensedAt, string pharmacyId)
    {
        if (!RefillsAllowed.HasValue || RefillsUsed >= RefillsAllowed.Value)
            throw new InvalidOperationException("No refills remaining");

        RefillsUsed++;
        DispensedAt = dispensedAt;
        PharmacyId = pharmacyId;
        UpdatedAt = dispensedAt;
    }

    public void Cancel(string reason, DateTimeOffset cancelledAt)
    {
        if (Status == PrescriptionStatus.Dispensed)
            throw new InvalidOperationException("Cannot cancel a dispensed prescription");

        Status = PrescriptionStatus.Cancelled;
        UpdatedAt = cancelledAt;
    }

    public void MarkAsDispensed(DateTimeOffset dispensedAt)
    {
        Status = PrescriptionStatus.Dispensed;
        DispensedAt = dispensedAt;
        UpdatedAt = dispensedAt;
    }

    public bool HasRefillsRemaining()
    {
        return RefillsAllowed.HasValue && RefillsUsed < RefillsAllowed.Value;
    }
}

public enum PrescriptionStatus
{
    Draft,
    Issued,
    Dispensed,
    PartiallyDispensed,
    Cancelled,
    Expired
}

public enum PrescriptionType
{
    Medication,
    MedicalDevice,
    Therapy,
    LaboratoryTest,
    Imaging,
    Referral
}
