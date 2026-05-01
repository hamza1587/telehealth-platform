using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Discovery;

/// <summary>
/// Medical specialty catalog entry.
/// </summary>
public sealed class SpecialtyCatalog : Entity<Guid>
{
    public SpecialtyCatalog(
        Guid id,
        string code,
        string name,
        string? description,
        string? parentCode,
        bool isActive,
        int displayOrder,
        DateTimeOffset createdAt)
        : base(id)
    {
        Code = code;
        Name = name;
        Description = description;
        ParentCode = parentCode;
        IsActive = isActive;
        DisplayOrder = displayOrder;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public string Code { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public string? ParentCode { get; private set; }
    public bool IsActive { get; private set; }
    public int DisplayOrder { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public void Update(string name, string? description, string? parentCode, int displayOrder, DateTimeOffset updatedAt)
    {
        Name = name;
        Description = description;
        ParentCode = parentCode;
        DisplayOrder = displayOrder;
        UpdatedAt = updatedAt;
    }

    public void Activate(DateTimeOffset updatedAt)
    {
        IsActive = true;
        UpdatedAt = updatedAt;
    }

    public void Deactivate(DateTimeOffset updatedAt)
    {
        IsActive = false;
        UpdatedAt = updatedAt;
    }
}
