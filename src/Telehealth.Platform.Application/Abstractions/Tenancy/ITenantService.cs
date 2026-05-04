namespace Telehealth.Platform.Application.Abstractions.Tenancy;

public interface ITenantService
{
    Task<TenantDto?> GetTenantBySubdomainAsync(string subdomain, CancellationToken cancellationToken = default);
    Task<TenantDto?> GetTenantByIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TenantDto>> GetAllTenantsAsync(CancellationToken cancellationToken = default);
    Task<TenantDto> CreateTenantAsync(CreateTenantRequest request, CancellationToken cancellationToken = default);
    Task<TenantDto> UpdateTenantAsync(Guid tenantId, UpdateTenantRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeactivateTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<bool> ActivateTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
}

public class TenantDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Subdomain { get; set; } = null!;
    public TenantSettingsDto Settings { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public bool IsActive { get; set; }
}

public class TenantSettingsDto
{
    public string? LogoUrl { get; set; }
    public string PrimaryColor { get; set; } = null!;
    public string SecondaryColor { get; set; } = null!;
    public int MaxUsers { get; set; }
    public int MaxStorageMb { get; set; }
    public bool EnableAiFeatures { get; set; }
    public bool EnableEhrIntegration { get; set; }
    public List<string> AllowedSpecialties { get; set; } = new();
}

public class CreateTenantRequest
{
    public string Name { get; set; } = null!;
    public string Subdomain { get; set; } = null!;
    public TenantSettingsDto Settings { get; set; } = null!;
}

public class UpdateTenantRequest
{
    public string? Name { get; set; }
    public TenantSettingsDto? Settings { get; set; }
}