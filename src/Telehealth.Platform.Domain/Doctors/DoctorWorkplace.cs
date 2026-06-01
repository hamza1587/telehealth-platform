using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Doctors;

/// <summary>
/// Doctor's workplace/hospital affiliations.
/// </summary>
public sealed class DoctorWorkplace : Entity<Guid>
{
    public DoctorWorkplace(
        Guid id,
        Guid doctorProfileId,
        string name,
        string? department,
        string? address,
        string? city,
        string? country,
        bool isPrimary,
        DateTimeOffset createdAt)
        : base(id)
    {
        DoctorProfileId = doctorProfileId;
        Name = name;
        Department = department;
        Address = address;
        City = city;
        Country = country;
        IsPrimary = isPrimary;
        IsActive = true;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public Guid DoctorProfileId { get; private set; }
    public string Name { get; private set; }
    public string? Department { get; private set; }
    public string? Address { get; private set; }
    public string? City { get; private set; }
    public string? Country { get; private set; }
    public bool IsPrimary { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public void Update(
        string name,
        string? department,
        string? address,
        string? city,
        string? country,
        bool isPrimary,
        DateTimeOffset updatedAt)
    {
        Name = name;
        Department = department;
        Address = address;
        City = city;
        Country = country;
        IsPrimary = isPrimary;
        UpdatedAt = updatedAt;
    }

    public void Deactivate(DateTimeOffset updatedAt)
    {
        IsActive = false;
        IsPrimary = false;
        UpdatedAt = updatedAt;
    }

    public void Activate(DateTimeOffset updatedAt)
    {
        IsActive = true;
        UpdatedAt = updatedAt;
    }
}
