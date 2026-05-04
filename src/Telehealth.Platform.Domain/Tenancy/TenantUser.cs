using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Tenancy;

public class TenantMembership : Entity<Guid>
{
    public Guid TenantId { get; private set; }
    public Guid UserId { get; private set; }
    public TenantUserRole Role { get; private set; }
    public List<string> Permissions { get; private set; } = new();
    public DateTimeOffset AssignedAt { get; private set; }
    public bool IsActive { get; private set; } = true;

    private TenantMembership(Guid id, Guid tenantId, Guid userId, TenantUserRole role) : base(id)
    {
        TenantId = tenantId;
        UserId = userId;
        Role = role;
        AssignedAt = DateTimeOffset.UtcNow;
    }

    public static TenantMembership Create(Guid tenantId, Guid userId, TenantUserRole role)
    {
        return new TenantMembership(Guid.NewGuid(), tenantId, userId, role);
    }

    public void UpdatePermissions(List<string> permissions)
    {
        Permissions = permissions;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
    }
}

public enum TenantUserRole
{
    Administrator = 1,
    Manager = 2,
    Doctor = 3,
    Nurse = 4,
    Patient = 5,
    Billing = 6,
    Compliance = 7
}