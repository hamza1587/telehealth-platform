namespace Telehealth.Platform.Domain.Consultations;

public enum ConsultationBookingStatus
{
    Draft = 1,
    Confirmed = 2,
    PatientWaiting = 3,
    DoctorWaiting = 4,
    InProgress = 5,
    Completed = 6,
    CancelledByPatient = 7,
    CancelledByDoctor = 8,
    NoShowPatient = 9,
    NoShowDoctor = 10,
    FailedTechnical = 11,
    Refunded = 12,
    Rejected = 13,
    Cancelled = 14,
}
