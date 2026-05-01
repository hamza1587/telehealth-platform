using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Telehealth.Platform.Domain.Identity;
using Telehealth.Platform.Infrastructure.Persistence;

namespace Telehealth.Platform.Infrastructure.Identity;

/// <summary>
/// Seeds initial roles and permissions into the database.
/// </summary>
public sealed class IdentityDataSeeder
{
    private readonly PlatformDbContext _dbContext;
    private readonly ILogger<IdentityDataSeeder> _logger;

    public IdentityDataSeeder(
        PlatformDbContext dbContext,
        ILogger<IdentityDataSeeder> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        _logger.LogInformation("Starting identity data seeding...");

        await SeedPermissionsAsync();
        await SeedRolesAsync();
        await SeedRolePermissionsAsync();

        _logger.LogInformation("Identity data seeding completed.");
    }

    private async Task SeedPermissionsAsync()
    {
        var existingPermissions = await _dbContext.Permissions
            .Select(p => p.Name)
            .ToListAsync();

        var permissionsToSeed = GetAllPermissions();
        var newPermissions = permissionsToSeed
            .Where(p => !existingPermissions.Contains(p.Name))
            .ToList();

        if (newPermissions.Any())
        {
            _logger.LogInformation("Seeding {Count} new permissions", newPermissions.Count);
            _dbContext.Permissions.AddRange(newPermissions);
            await _dbContext.SaveChangesAsync();
        }
    }

    private async Task SeedRolesAsync()
    {
        var existingRoles = await _dbContext.Roles
            .Select(r => r.NormalizedName)
            .ToListAsync();

        var rolesToSeed = new List<Role>
        {
            new(Guid.NewGuid(), Roles.Patient, "Patient users who can book consultations and manage their health data", 100),
            new(Guid.NewGuid(), Roles.Doctor, "Verified medical professionals who can provide consultations", 200),
            new(Guid.NewGuid(), Roles.Admin, "Platform administrators with broad operational access", 300),
            new(Guid.NewGuid(), Roles.SupportAgent, "Support staff for handling user issues", 150),
            new(Guid.NewGuid(), Roles.ComplianceOfficer, "Data protection and compliance officers", 250),
            new(Guid.NewGuid(), Roles.FinanceOperator, "Finance staff for managing payments and payouts", 180),
            new(Guid.NewGuid(), Roles.ResearchReviewer, "Researchers who can approve data export requests", 220)
        };

        var newRoles = rolesToSeed
            .Where(r => !existingRoles.Contains(r.NormalizedName))
            .ToList();

        if (newRoles.Any())
        {
            _logger.LogInformation("Seeding {Count} new roles", newRoles.Count);
            _dbContext.Roles.AddRange(newRoles);
            await _dbContext.SaveChangesAsync();
        }
    }

    private async Task SeedRolePermissionsAsync()
    {
        var roles = await _dbContext.Roles.ToListAsync();
        var permissions = await _dbContext.Permissions.ToListAsync();
        var existingRolePermissions = await _dbContext.RolePermissions.ToListAsync();

        var rolePermissionMappings = GetRolePermissionMappings();

        foreach (var mapping in rolePermissionMappings)
        {
            var role = roles.FirstOrDefault(r => r.Name == mapping.RoleName);
            var permission = permissions.FirstOrDefault(p => p.Name == mapping.PermissionName);

            if (role == null || permission == null)
            {
                continue;
            }

            var exists = existingRolePermissions.Any(rp =>
                rp.RoleId == role.Id && rp.PermissionId == permission.Id);

            if (!exists)
            {
                _dbContext.RolePermissions.Add(new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = permission.Id,
                    AssignedAt = DateTimeOffset.UtcNow
                });
            }
        }

        await _dbContext.SaveChangesAsync();
    }

    private static List<Permission> GetAllPermissions()
    {
        var permissions = new List<Permission>();
        var permissionType = typeof(Permissions);
        var fields = permissionType.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

        foreach (var field in fields)
        {
            if (field.IsLiteral && !field.IsInitOnly)
            {
                var name = (string)field.GetValue(null)!;
                var category = name.Split(':')[0];
                permissions.Add(new Permission(Guid.NewGuid(), name, category));
            }
        }

        return permissions;
    }

    private static List<(string RoleName, string PermissionName)> GetRolePermissionMappings()
    {
        return new List<(string, string)>
        {
            // Patient permissions
            (Roles.Patient, Permissions.PatientRead),
            (Roles.Patient, Permissions.PatientWrite),
            (Roles.Patient, Permissions.PatientMedicalHistoryRead),
            (Roles.Patient, Permissions.PatientMedicalHistoryWrite),
            (Roles.Patient, Permissions.PatientConsultationRead),
            (Roles.Patient, Permissions.PatientConsultationCreate),
            (Roles.Patient, Permissions.PatientPrescriptionRead),
            (Roles.Patient, Permissions.PatientWalletRead),
            (Roles.Patient, Permissions.PatientWalletWrite),

            // Doctor permissions
            (Roles.Doctor, Permissions.DoctorRead),
            (Roles.Doctor, Permissions.DoctorWrite),
            (Roles.Doctor, Permissions.DoctorProfileManage),
            (Roles.Doctor, Permissions.DoctorAvailabilityManage),
            (Roles.Doctor, Permissions.DoctorConsultationAccept),
            (Roles.Doctor, Permissions.DoctorClinicalNotesWrite),
            (Roles.Doctor, Permissions.DoctorPrescriptionWrite),
            (Roles.Doctor, Permissions.DoctorPatientDataRead),
            (Roles.Doctor, Permissions.DoctorEarningsRead),
            (Roles.Doctor, Permissions.PatientMedicalHistoryRead), // Doctors need access to patient history
            (Roles.Doctor, Permissions.PatientConsultationRead),
            (Roles.Doctor, Permissions.PatientPrescriptionRead),

            // Admin permissions
            (Roles.Admin, Permissions.AdminUsersRead),
            (Roles.Admin, Permissions.AdminUsersManage),
            (Roles.Admin, Permissions.AdminDoctorsVerify),
            (Roles.Admin, Permissions.AdminDoctorsSuspend),
            (Roles.Admin, Permissions.AdminConsultationsRead),
            (Roles.Admin, Permissions.AdminBillingManage),
            (Roles.Admin, Permissions.AdminRefundsProcess),
            (Roles.Admin, Permissions.AdminSupportTicketsManage),
            (Roles.Admin, Permissions.AdminConfigurationManage),
            (Roles.Admin, Permissions.SystemAuditRead),
            (Roles.Admin, Permissions.SystemLogsRead),
            (Roles.Admin, Permissions.SystemHealthRead),

            // Compliance permissions
            (Roles.ComplianceOfficer, Permissions.ComplianceAuditRead),
            (Roles.ComplianceOfficer, Permissions.ComplianceGdprRequestsManage),
            (Roles.ComplianceOfficer, Permissions.ComplianceConsentVersionsManage),
            (Roles.ComplianceOfficer, Permissions.ComplianceBreachManage),
            (Roles.ComplianceOfficer, Permissions.ComplianceResearchExportsApprove),
            (Roles.ComplianceOfficer, Permissions.SystemAuditRead),
            (Roles.ComplianceOfficer, Permissions.AdminConsultationsRead),

            // Support permissions
            (Roles.SupportAgent, Permissions.SupportTicketsRead),
            (Roles.SupportAgent, Permissions.SupportTicketsManage),
            (Roles.SupportAgent, Permissions.SupportUsersRead),
            (Roles.SupportAgent, Permissions.SupportBillingRead),
            (Roles.SupportAgent, Permissions.AdminSupportTicketsManage),

            // Finance permissions
            (Roles.FinanceOperator, Permissions.FinancePaymentsRead),
            (Roles.FinanceOperator, Permissions.FinancePayoutsManage),
            (Roles.FinanceOperator, Permissions.FinanceReportsRead),
            (Roles.FinanceOperator, Permissions.FinanceWalletAdjust),
            (Roles.FinanceOperator, Permissions.AdminBillingManage),
            (Roles.FinanceOperator, Permissions.AdminRefundsProcess),
            (Roles.FinanceOperator, Permissions.SupportBillingRead),

            // Research reviewer permissions
            (Roles.ResearchReviewer, Permissions.ComplianceResearchExportsApprove),
            (Roles.ResearchReviewer, Permissions.ComplianceAuditRead)
        };
    }
}
