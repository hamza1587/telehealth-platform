using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Doctors;

/// <summary>
/// Doctor's education and training history.
/// </summary>
public sealed class DoctorEducation : Entity<Guid>
{
    public DoctorEducation(
        Guid id,
        Guid doctorProfileId,
        string institution,
        string degree,
        string fieldOfStudy,
        DateOnly? startDate,
        DateOnly? endDate,
        bool isVerified,
        DateTimeOffset createdAt)
        : base(id)
    {
        DoctorProfileId = doctorProfileId;
        Institution = institution;
        Degree = degree;
        FieldOfStudy = fieldOfStudy;
        StartDate = startDate;
        EndDate = endDate;
        IsVerified = isVerified;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public Guid DoctorProfileId { get; private set; }
    public string Institution { get; private set; }
    public string Degree { get; private set; }
    public string FieldOfStudy { get; private set; }
    public DateOnly? StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }
    public bool IsVerified { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public void Update(
        string institution,
        string degree,
        string fieldOfStudy,
        DateOnly? startDate,
        DateOnly? endDate,
        DateTimeOffset updatedAt)
    {
        Institution = institution;
        Degree = degree;
        FieldOfStudy = fieldOfStudy;
        StartDate = startDate;
        EndDate = endDate;
        UpdatedAt = updatedAt;
    }

    public void MarkAsVerified(DateTimeOffset updatedAt)
    {
        IsVerified = true;
        UpdatedAt = updatedAt;
    }
}
