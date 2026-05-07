using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Consultations;

public sealed class ConsultationBooking : Entity<Guid>
{
    public ConsultationBooking(
        Guid id,
        Guid patientAccountId,
        Guid doctorProfileId,
        string specialtyCode,
        ConsultationMode consultationMode,
        string specialty,
        long priceMinor,
        string currency) : base(id)
    {
        PatientAccountId = patientAccountId;
        DoctorProfileId = doctorProfileId;
        SpecialtyCode = specialtyCode;
        Specialty = specialty;
        ConsultationMode = consultationMode;
        PriceMinor = priceMinor;
        Currency = currency;
        Status = ConsultationBookingStatus.Draft;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public Guid PatientAccountId { get; private set; }
    public Guid DoctorProfileId { get; private set; }
    public string SpecialtyCode { get; private set; }
    public string Specialty { get; private set; }
    public string PatientName { get; private set; } = string.Empty;
    public string DoctorName { get; private set; } = string.Empty;
    public ConsultationMode ConsultationMode { get; private set; }
    public ConsultationBookingStatus Status { get; private set; }
    public long PriceMinor { get; private set; }
    public string Currency { get; private set; } = "EUR";
    public DateTimeOffset? ScheduledStartsAt { get; private set; }
    public DateTimeOffset? ScheduledEndsAt { get; private set; }
    public string Notes { get; private set; } = string.Empty;
    public string RejectionReason { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public void SetScheduledAt(DateTimeOffset startsAt, DateTimeOffset endsAt)
    {
        ScheduledStartsAt = startsAt;
        ScheduledEndsAt = endsAt;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Confirm()
    {
        Status = ConsultationBookingStatus.Confirmed;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Cancel(string reason)
    {
        Status = ConsultationBookingStatus.Cancelled;
        RejectionReason = reason;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Reject(string reason)
    {
        Status = ConsultationBookingStatus.Rejected;
        RejectionReason = reason;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Reschedule(DateTimeOffset newStartsAt, DateTimeOffset newEndsAt)
    {
        ScheduledStartsAt = newStartsAt;
        ScheduledEndsAt = newEndsAt;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetPatientInfo(string patientName)
    {
        PatientName = patientName;
    }

    public void SetDoctorInfo(string doctorName)
    {
        DoctorName = doctorName;
    }

    public void SetNotes(string notes)
    {
        Notes = notes;
    }
}