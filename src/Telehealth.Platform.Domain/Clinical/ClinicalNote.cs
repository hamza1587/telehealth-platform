using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Clinical;

/// <summary>
/// Clinical notes created during or after consultations.
/// </summary>
public sealed class ClinicalNote : Entity<Guid>
{
    public ClinicalNote(
        Guid id,
        Guid consultationSessionId,
        Guid patientAccountId,
        Guid doctorProfileId,
        string noteType,
        string title,
        string content,
        bool isDraft,
        DateTimeOffset createdAt)
        : base(id)
    {
        ConsultationSessionId = consultationSessionId;
        PatientAccountId = patientAccountId;
        DoctorProfileId = doctorProfileId;
        NoteType = noteType;
        Title = title;
        Content = content;
        IsDraft = isDraft;
        IsLocked = false;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public Guid ConsultationSessionId { get; private set; }
    public Guid PatientAccountId { get; private set; }
    public Guid DoctorProfileId { get; private set; }
    public string NoteType { get; private set; }
    public string Title { get; private set; }
    public string Content { get; private set; }
    public bool IsDraft { get; private set; }
    public bool IsLocked { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? LockedAt { get; private set; }
    public string? LockedBy { get; private set; }
    public string? LockReason { get; private set; }

    public void Update(string title, string content, DateTimeOffset updatedAt)
    {
        if (IsLocked)
            throw new InvalidOperationException("Cannot modify a locked clinical note");

        Title = title;
        Content = content;
        UpdatedAt = updatedAt;
    }

    public void Finalize(DateTimeOffset finalizedAt)
    {
        IsDraft = false;
        UpdatedAt = finalizedAt;
    }

    public void Lock(string lockedBy, string reason, DateTimeOffset lockedAt)
    {
        IsLocked = true;
        LockedBy = lockedBy;
        LockReason = reason;
        LockedAt = lockedAt;
    }

    public void Unlock(string unlockedBy, DateTimeOffset unlockedAt)
    {
        IsLocked = false;
        LockedBy = null;
        LockReason = null;
        LockedAt = null;
        UpdatedAt = unlockedAt;
    }
}

public enum ClinicalNoteType
{
    ConsultationSummary,
    FollowUpNotes,
    ProgressNotes,
    ReferralNotes,
    DischargeSummary,
    LabReview,
    ImagingReview,
    PrescriptionReview
}
