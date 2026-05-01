using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Identity;

/// <summary>
/// Permission entity for fine-grained access control.
/// Supports both RBAC and ABAC (Attribute-Based Access Control).
/// </summary>
public sealed class Permission : Entity<Guid>
{
    public Permission(Guid id, string name, string category, string? description = null)
        : base(id)
    {
        Name = name;
        Category = category;
        Description = description;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public string Name { get; private set; }
    public string Category { get; private set; }
    public string? Description { get; private set; }
    public DateTimeOffset CreatedAt { get; }

    public ICollection<RolePermission> RolePermissions { get; private set; } = [];
}

/// <summary>
/// Join entity for Role-Permission many-to-many relationship.
/// </summary>
public sealed class RolePermission
{
    public Guid RoleId { get; init; }
    public Guid PermissionId { get; init; }
    public DateTimeOffset AssignedAt { get; init; }

    public Role Role { get; private set; } = null!;
    public Permission Permission { get; private set; } = null!;
}

/// <summary>
/// Predefined permission constants for the telehealth platform.
/// </summary>
public static class Permissions
{
    // Patient Permissions
    public const string PatientRead = "patient:read";
    public const string PatientWrite = "patient:write";
    public const string PatientDelete = "patient:delete";
    public const string PatientMedicalHistoryRead = "patient:medical_history:read";
    public const string PatientMedicalHistoryWrite = "patient:medical_history:write";
    public const string PatientConsultationRead = "patient:consultation:read";
    public const string PatientConsultationCreate = "patient:consultation:create";
    public const string PatientPrescriptionRead = "patient:prescription:read";
    public const string PatientWalletRead = "patient:wallet:read";
    public const string PatientWalletWrite = "patient:wallet:write";

    // Doctor Permissions
    public const string DoctorRead = "doctor:read";
    public const string DoctorWrite = "doctor:write";
    public const string DoctorProfileManage = "doctor:profile:manage";
    public const string DoctorAvailabilityManage = "doctor:availability:manage";
    public const string DoctorConsultationAccept = "doctor:consultation:accept";
    public const string DoctorClinicalNotesWrite = "doctor:clinical_notes:write";
    public const string DoctorPrescriptionWrite = "doctor:prescription:write";
    public const string DoctorPatientDataRead = "doctor:patient_data:read";
    public const string DoctorEarningsRead = "doctor:earnings:read";

    // Admin Permissions
    public const string AdminUsersRead = "admin:users:read";
    public const string AdminUsersManage = "admin:users:manage";
    public const string AdminDoctorsVerify = "admin:doctors:verify";
    public const string AdminDoctorsSuspend = "admin:doctors:suspend";
    public const string AdminConsultationsRead = "admin:consultations:read";
    public const string AdminBillingManage = "admin:billing:manage";
    public const string AdminRefundsProcess = "admin:refunds:process";
    public const string AdminSupportTicketsManage = "admin:support_tickets:manage";
    public const string AdminConfigurationManage = "admin:configuration:manage";

    // Compliance Permissions
    public const string ComplianceAuditRead = "compliance:audit:read";
    public const string ComplianceGdprRequestsManage = "compliance:gdpr:manage";
    public const string ComplianceConsentVersionsManage = "compliance:consent:manage";
    public const string ComplianceBreachManage = "compliance:breach:manage";
    public const string ComplianceResearchExportsApprove = "compliance:research:approve";

    // Support Permissions
    public const string SupportTicketsRead = "support:tickets:read";
    public const string SupportTicketsManage = "support:tickets:manage";
    public const string SupportUsersRead = "support:users:read";
    public const string SupportBillingRead = "support:billing:read";

    // Finance Permissions
    public const string FinancePaymentsRead = "finance:payments:read";
    public const string FinancePayoutsManage = "finance:payouts:manage";
    public const string FinanceReportsRead = "finance:reports:read";
    public const string FinanceWalletAdjust = "finance:wallet:adjust";

    // System Permissions
    public const string SystemAuditRead = "system:audit:read";
    public const string SystemLogsRead = "system:logs:read";
    public const string SystemHealthRead = "system:health:read";

    // Break-Glass Emergency Access
    public const string BreakGlassAccess = "break_glass:access";
}

/// <summary>
/// Predefined role constants.
/// </summary>
public static class Roles
{
    public const string Patient = "Patient";
    public const string Doctor = "Doctor";
    public const string Admin = "Admin";
    public const string SupportAgent = "SupportAgent";
    public const string ComplianceOfficer = "ComplianceOfficer";
    public const string FinanceOperator = "FinanceOperator";
    public const string ResearchReviewer = "ResearchReviewer";
}
