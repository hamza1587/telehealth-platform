namespace Telehealth.Platform.Domain.Doctors;

public enum DoctorVerificationStatus
{
    Draft = 1,
    Submitted = 2,
    InReview = 3,
    MoreInformationRequired = 4,
    Verified = 5,
    Rejected = 6,
    Expired = 7,
    Suspended = 8,
}
