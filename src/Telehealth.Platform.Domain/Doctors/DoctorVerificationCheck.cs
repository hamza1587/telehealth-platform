using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Doctors;

/// <summary>
/// Individual verification checks performed during doctor onboarding.
/// </summary>
public sealed class DoctorVerificationCheck : Entity<Guid>
{
    public DoctorVerificationCheck(
        Guid id,
        Guid doctorProfileId,
        string checkType,
        VerificationCheckStatus status,
        string? notes,
        DateTimeOffset createdAt)
        : base(id)
    {
        DoctorProfileId = doctorProfileId;
        CheckType = checkType;
        Status = status;
        Notes = notes;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public Guid DoctorProfileId { get; private set; }
    public string CheckType { get; private set; }
    public VerificationCheckStatus Status { get; private set; }
    public string? Notes { get; private set; }
    public string? VerifiedBy { get; private set; }
    public string? ExternalReference { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    public void Complete(VerificationCheckStatus status, string? notes, string? verifiedBy, string? externalReference, DateTimeOffset updatedAt)
    {
        Status = status;
        Notes = notes;
        VerifiedBy = verifiedBy;
        ExternalReference = externalReference;
        CompletedAt = updatedAt;
        UpdatedAt = updatedAt;
    }

    public void UpdateNotes(string? notes, DateTimeOffset updatedAt)
    {
        Notes = notes;
        UpdatedAt = updatedAt;
    }
}

public enum VerificationCheckStatus
{
    Pending,
    InProgress,
    Passed,
    Failed,
    RequiresReview,
    Waived
}

public enum VerificationCheckType
{
    IdentityVerification,
    LicenseVerification,
    BackgroundCheck,
    ReferenceCheck,
    EducationVerification,
    MalpracticeHistory,
    SanctionsCheck,
    WorkHistoryVerification,
    InterviewCompleted,
    PlatformTrainingCompleted
}
