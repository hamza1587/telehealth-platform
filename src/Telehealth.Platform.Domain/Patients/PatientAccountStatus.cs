namespace Telehealth.Platform.Domain.Patients;

public enum PatientAccountStatus
{
    PendingOnboarding = 1,
    Active = 2,
    Suspended = 3,
    ErasureRequested = 4,
    Closed = 5,
}
