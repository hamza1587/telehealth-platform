using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Tenancy;

public class Tenant : Entity<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public string Subdomain { get; private set; } = string.Empty;
    public TenantSettings Settings { get; private set; } = new();
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Tenant(
        Guid id,
        string name,
        string subdomain) : base(id)
    {
        Name = name;
        Subdomain = subdomain;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public static Tenant Create(string name, string subdomain)
    {
        return new Tenant(Guid.NewGuid(), name, subdomain);
    }

    public void UpdateSettings(TenantSettings settings)
    {
        Settings = settings;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}

public class TenantSettings
{
    public string? LogoUrl { get; set; }
    public string PrimaryColor { get; set; } = "#007bff";
    public string SecondaryColor { get; set; } = "#6c757d";
    public int MaxUsers { get; set; } = 100;
    public int MaxStorageMb { get; set; } = 1000;
    public bool EnableAiFeatures { get; set; } = true;
    public bool EnableEhrIntegration { get; set; } = true;
    public List<string> AllowedSpecialties { get; set; } = new();
}