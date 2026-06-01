using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Doctors;

/// <summary>
/// Documents submitted by doctors during verification process.
/// </summary>
public sealed class DoctorVerificationDocument : Entity<Guid>
{
    public DoctorVerificationDocument(
        Guid id,
        Guid doctorProfileId,
        string documentType,
        string fileName,
        string fileUrl,
        string? mimeType,
        long fileSize,
        DateTimeOffset uploadedAt)
        : base(id)
    {
        DoctorProfileId = doctorProfileId;
        DocumentType = documentType;
        FileName = fileName;
        FileUrl = fileUrl;
        MimeType = mimeType;
        FileSize = fileSize;
        Status = DocumentVerificationStatus.Pending;
        UploadedAt = uploadedAt;
    }

    public Guid DoctorProfileId { get; private set; }
    public string DocumentType { get; private set; }
    public string FileName { get; private set; }
    public string FileUrl { get; private set; }
    public string? MimeType { get; private set; }
    public long FileSize { get; private set; }
    public DocumentVerificationStatus Status { get; private set; }
    public string? RejectionReason { get; private set; }
    public DateTimeOffset UploadedAt { get; private set; }
    public DateTimeOffset? ReviewedAt { get; private set; }
    public string? ReviewedBy { get; private set; }

    public void Approve(string reviewerId, DateTimeOffset reviewedAt)
    {
        Status = DocumentVerificationStatus.Approved;
        ReviewedBy = reviewerId;
        ReviewedAt = reviewedAt;
    }

    public void Reject(string reviewerId, string reason, DateTimeOffset reviewedAt)
    {
        Status = DocumentVerificationStatus.Rejected;
        RejectionReason = reason;
        ReviewedBy = reviewerId;
        ReviewedAt = reviewedAt;
    }

    public void Replace(string newFileUrl, DateTimeOffset uploadedAt)
    {
        FileUrl = newFileUrl;
        Status = DocumentVerificationStatus.Pending;
        RejectionReason = null;
        ReviewedBy = null;
        ReviewedAt = null;
    }
}

public enum DocumentVerificationStatus
{
    Pending,
    Approved,
    Rejected,
    Expired
}

public enum DocumentType
{
    MedicalLicense,
    BoardCertification,
    IdentityDocument,
    ProofOfAddress,
    MalpracticeInsurance,
    CurriculumVitae,
    ProfessionalPhoto,
    SpecialtyCertification,
    TrainingCertificate
}
