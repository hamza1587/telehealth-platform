using Microsoft.EntityFrameworkCore;
using Telehealth.Platform.Application.Abstractions.Tenancy;
using Telehealth.Platform.Domain.Tenancy;
using Telehealth.Platform.Infrastructure.Persistence;

namespace Telehealth.Platform.Infrastructure.Tenancy;

public class TenantService : ITenantService
{
    private readonly PlatformDbContext _context;

    public TenantService(PlatformDbContext context)
    {
        _context = context;
    }

    public async Task<TenantDto?> GetTenantBySubdomainAsync(string subdomain, CancellationToken cancellationToken = default)
    {
        var tenant = await _context.Tenants
            .FirstOrDefaultAsync(t => t.Subdomain == subdomain && t.IsActive, cancellationToken);

        return tenant == null ? null : MapToDto(tenant);
    }

    public async Task<TenantDto?> GetTenantByIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var tenant = await _context.Tenants
            .FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);

        return tenant == null ? null : MapToDto(tenant);
    }

    public async Task<IEnumerable<TenantDto>> GetAllTenantsAsync(CancellationToken cancellationToken = default)
    {
        var tenants = await _context.Tenants
            .ToListAsync(cancellationToken);

        return tenants.Select(MapToDto);
    }

    public async Task<TenantDto> CreateTenantAsync(CreateTenantRequest request, CancellationToken cancellationToken = default)
    {
        var tenant = Tenant.Create(request.Name, request.Subdomain);
        tenant.UpdateSettings(new TenantSettings
        {
            LogoUrl = request.Settings.LogoUrl,
            PrimaryColor = request.Settings.PrimaryColor,
            SecondaryColor = request.Settings.SecondaryColor,
            MaxUsers = request.Settings.MaxUsers,
            MaxStorageMb = request.Settings.MaxStorageMb,
            EnableAiFeatures = request.Settings.EnableAiFeatures,
            EnableEhrIntegration = request.Settings.EnableEhrIntegration,
            AllowedSpecialties = request.Settings.AllowedSpecialties
        });

        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(tenant);
    }

    public async Task<TenantDto> UpdateTenantAsync(Guid tenantId, UpdateTenantRequest request, CancellationToken cancellationToken = default)
    {
        var tenant = await _context.Tenants
            .FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);

        if (tenant == null)
            throw new InvalidOperationException("Tenant not found");

        if (!string.IsNullOrEmpty(request.Name))
            tenant.GetType().GetProperty("Name")?.SetValue(tenant, request.Name);

        if (request.Settings != null)
        {
            tenant.UpdateSettings(new TenantSettings
            {
                LogoUrl = request.Settings.LogoUrl,
                PrimaryColor = request.Settings.PrimaryColor,
                SecondaryColor = request.Settings.SecondaryColor,
                MaxUsers = request.Settings.MaxUsers,
                MaxStorageMb = request.Settings.MaxStorageMb,
                EnableAiFeatures = request.Settings.EnableAiFeatures,
                EnableEhrIntegration = request.Settings.EnableEhrIntegration,
                AllowedSpecialties = request.Settings.AllowedSpecialties
            });
        }

        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(tenant);
    }

    public async Task<bool> DeactivateTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var tenant = await _context.Tenants
            .FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);

        if (tenant == null)
            return false;

        tenant.SetActive(false);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> ActivateTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var tenant = await _context.Tenants
            .FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);

        if (tenant == null)
            return false;

        tenant.SetActive(true);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static TenantDto MapToDto(Tenant tenant)
    {
        return new TenantDto
        {
            Id = tenant.Id,
            Name = tenant.Name,
            Subdomain = tenant.Subdomain,
            Settings = new TenantSettingsDto
            {
                LogoUrl = tenant.Settings.LogoUrl,
                PrimaryColor = tenant.Settings.PrimaryColor,
                SecondaryColor = tenant.Settings.SecondaryColor,
                MaxUsers = tenant.Settings.MaxUsers,
                MaxStorageMb = tenant.Settings.MaxStorageMb,
                EnableAiFeatures = tenant.Settings.EnableAiFeatures,
                EnableEhrIntegration = tenant.Settings.EnableEhrIntegration,
                AllowedSpecialties = tenant.Settings.AllowedSpecialties
            },
            CreatedAt = tenant.CreatedAt,
            UpdatedAt = tenant.UpdatedAt,
            IsActive = tenant.IsActive
        };
    }
}