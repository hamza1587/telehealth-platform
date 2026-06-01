using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Identity;

/// <summary>
/// Role entity for RBAC (Role-Based Access Control).
/// </summary>
public sealed class Role : Entity<Guid>
{
    public Role(Guid id, string name, string? description = null, int? priority = null)
        : base(id)
    {
        Name = name;
        NormalizedName = name.ToUpperInvariant();
        Description = description;
        Priority = priority ?? 0;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public string Name { get; private set; }
    public string NormalizedName { get; private set; }
    public string? Description { get; private set; }
    public int Priority { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public ICollection<UserRole> UserRoles { get; private set; } = [];
    public ICollection<RolePermission> RolePermissions { get; private set; } = [];
}

/// <summary>
/// Join entity for User-Role many-to-many relationship.
/// </summary>
public sealed class UserRole
{
    public Guid UserId { get; init; }
    public Guid RoleId { get; init; }
    public DateTimeOffset AssignedAt { get; init; }
    public string? AssignedBy { get; init; }

    public PlatformUser User { get; private set; } = null!;
    public Role Role { get; private set; } = null!;
}
