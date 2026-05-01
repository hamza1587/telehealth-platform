using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Doctors;

/// <summary>
/// Doctor specialty with certification details.
/// </summary>
public sealed class DoctorSpecialty : Entity<Guid>
{
    public DoctorSpecialty(
        Guid id,
        Guid doctorProfileId,
        string specialtyCode,
        string specialtyName,
        bool isPrimary,
        string? certificationNumber,
        DateOnly? certifiedDate,
        DateOnly? expiryDate,
        DateTimeOffset createdAt)
        : base(id)
    {
        DoctorProfileId = doctorProfileId;
        SpecialtyCode = specialtyCode;
        SpecialtyName = specialtyName;
        IsPrimary = isPrimary;
        CertificationNumber = certificationNumber;
        CertifiedDate = certifiedDate;
        ExpiryDate = expiryDate;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public Guid DoctorProfileId { get; }
    public string SpecialtyCode { get; private set; }
    public string SpecialtyName { get; private set; }
    public bool IsPrimary { get; private set; }
    public string? CertificationNumber { get; private set; }
    public DateOnly? CertifiedDate { get; private set; }
    public DateOnly? ExpiryDate { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public void Update(
        string specialtyCode,
        string specialtyName,
        bool isPrimary,
        string? certificationNumber,
        DateOnly? certifiedDate,
        DateOnly? expiryDate,
        DateTimeOffset updatedAt)
    {
        SpecialtyCode = specialtyCode;
        SpecialtyName = specialtyName;
        IsPrimary = isPrimary;
        CertificationNumber = certificationNumber;
        CertifiedDate = certifiedDate;
        ExpiryDate = expiryDate;
        UpdatedAt = updatedAt;
    }

    public bool IsExpired()
    {
        return ExpiryDate.HasValue && ExpiryDate.Value < DateOnly.FromDateTime(DateTime.UtcNow);
    }
}
