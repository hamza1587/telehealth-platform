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
        BookingType bookingType,
        Money pricePerSecond,
        long reservedSeconds,
        DateTimeOffset? scheduledStartsAt,
        DateTimeOffset? scheduledEndsAt,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
        : base(id)
    {
        PatientAccountId = patientAccountId;
        DoctorProfileId = doctorProfileId;
        SpecialtyCode = specialtyCode;
        ConsultationMode = consultationMode;
        BookingType = bookingType;
        PricePerSecond = pricePerSecond;
        ReservedSeconds = reservedSeconds;
        ScheduledStartsAt = scheduledStartsAt;
        ScheduledEndsAt = scheduledEndsAt;
        Status = ConsultationBookingStatus.Draft;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public Guid PatientAccountId { get; }

    public Guid DoctorProfileId { get; }

    public string? MedplumAppointmentId { get; private set; }

    public string SpecialtyCode { get; }

    public ConsultationMode ConsultationMode { get; }

    public BookingType BookingType { get; }

    public ConsultationBookingStatus Status { get; private set; }

    public Money PricePerSecond { get; }

    public long ReservedSeconds { get; }

    public DateTimeOffset? ScheduledStartsAt { get; private set; }

    public DateTimeOffset? ScheduledEndsAt { get; private set; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public void LinkMedplumAppointment(string medplumAppointmentId)
    {
        MedplumAppointmentId = medplumAppointmentId;
    }

    public void Confirm(DateTimeOffset updatedAt)
    {
        Status = ConsultationBookingStatus.Confirmed;
        UpdatedAt = updatedAt;
    }
}
