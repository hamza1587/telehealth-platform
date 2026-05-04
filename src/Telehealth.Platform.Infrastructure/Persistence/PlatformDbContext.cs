using Microsoft.EntityFrameworkCore;
using Telehealth.Platform.Domain.Analytics;
using Telehealth.Platform.Domain.Auditing;
using Telehealth.Platform.Domain.Billing;
using Telehealth.Platform.Domain.Clinical;
using Telehealth.Platform.Domain.Common;
using Telehealth.Platform.Domain.Compliance;
using Telehealth.Platform.Domain.Consultations;
using Telehealth.Platform.Domain.Doctors;
using Telehealth.Platform.Domain.Financial;
using Telehealth.Platform.Domain.Identity;
using Telehealth.Platform.Domain.InstantConsultation;
using Telehealth.Platform.Domain.Notifications;
using Telehealth.Platform.Domain.Patients;
using Telehealth.Platform.Domain.Reviews;
using Telehealth.Platform.Domain.Research;
using Telehealth.Platform.Domain.Tenancy;

namespace Telehealth.Platform.Infrastructure.Persistence;

public sealed partial class PlatformDbContext : DbContext
{
    public PlatformDbContext(DbContextOptions<PlatformDbContext> options)
        : base(options)
    {
    }

    public DbSet<PatientAccount> PatientAccounts => Set<PatientAccount>();

    public DbSet<PatientConsentRecord> PatientConsentRecords => Set<PatientConsentRecord>();

    public DbSet<PatientMedicalProfile> PatientMedicalProfiles => Set<PatientMedicalProfile>();

    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();

    public DbSet<DoctorProfile> DoctorProfiles => Set<DoctorProfile>();

    public DbSet<DoctorOnboardingRecord> DoctorOnboardingRecords => Set<DoctorOnboardingRecord>();

    public DbSet<DoctorLanguage> DoctorLanguages => Set<DoctorLanguage>();

    public DbSet<DoctorAvailabilityWindow> DoctorAvailabilityWindows => Set<DoctorAvailabilityWindow>();

    // Identity
    public DbSet<PlatformUser> PlatformUsers => Set<PlatformUser>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<LoginAttempt> LoginAttempts => Set<LoginAttempt>();
    public DbSet<DeviceAuthorization> DeviceAuthorizations => Set<DeviceAuthorization>();
    public DbSet<UserDevice> UserDevices => Set<UserDevice>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();

    // Financial
    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<WalletLedgerEntry> WalletLedgerEntries => Set<WalletLedgerEntry>();
    public DbSet<Payment> Payments => Set<Payment>();

    // Billing
    public DbSet<BillingSession> BillingSessions => Set<BillingSession>();

    // Consultations
    public DbSet<VideoRoom> VideoRooms => Set<VideoRoom>();
    public DbSet<ConsultationSession> ConsultationSessions => Set<ConsultationSession>();
    public DbSet<ParticipantEvent> ParticipantEvents => Set<ParticipantEvent>();

    // Notifications
    public DbSet<Notification> Notifications => Set<Notification>();

    // Clinical
    public DbSet<ClinicalRecord> ClinicalRecords => Set<ClinicalRecord>();

    // Billing
    public DbSet<InsuranceProvider> InsuranceProviders => Set<InsuranceProvider>();
    public DbSet<PatientInsurance> PatientInsurances => Set<PatientInsurance>();

    // Analytics
    public DbSet<DashboardMetrics> DashboardMetrics => Set<DashboardMetrics>();
    public DbSet<AnalyticsReport> Reports => Set<AnalyticsReport>();
    public DbSet<ConsultationAnalytics> ConsultationAnalytics => Set<ConsultationAnalytics>();

    // Consultation Requests
    public DbSet<ConsultationRequest> ConsultationRequests => Set<ConsultationRequest>();

    // Instant Queue
    public DbSet<InstantQueueEntry> InstantQueueEntries => Set<InstantQueueEntry>();

    // Research
    public DbSet<ResearchExport> ResearchExports => Set<ResearchExport>();

    // Reviews
    public DbSet<Review> Reviews => Set<Review>();

    // Tenancy
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<TenantMembership> TenantMemberships => Set<TenantMembership>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PatientAccount>(entity =>
        {
            entity.ToTable("patient_accounts");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.DisplayName).HasColumnName("display_name");
            entity.Property(x => x.Email).HasColumnName("email");
            entity.Property(x => x.MedplumPatientId).HasColumnName("medplum_patient_id");
            entity.Property(x => x.CountryCode).HasColumnName("country_code");
            entity.Property(x => x.PreferredLanguage).HasColumnName("preferred_language");
            entity.Property(x => x.Status).HasColumnName("status").HasConversion<string>();
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<PatientConsentRecord>(entity =>
        {
            entity.ToTable("patient_consent_records");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.PatientAccountId).HasColumnName("patient_account_id");
            entity.Property(x => x.ConsentType).HasColumnName("consent_type");
            entity.Property(x => x.Version).HasColumnName("version");
            entity.Property(x => x.TextSnapshot).HasColumnName("text_snapshot");
            entity.Property(x => x.TextHash).HasColumnName("text_hash");
            entity.Property(x => x.Language).HasColumnName("language");
            entity.Property(x => x.LegalBasis).HasColumnName("legal_basis");
            entity.Property(x => x.IsAccepted).HasColumnName("is_accepted");
            entity.Property(x => x.IpAddress).HasColumnName("ip_address");
            entity.Property(x => x.UserAgent).HasColumnName("user_agent");
            entity.Property(x => x.CapturedAt).HasColumnName("captured_at");
            entity.Property(x => x.WithdrawnAt).HasColumnName("withdrawn_at");
        });

        modelBuilder.Entity<PatientMedicalProfile>(entity =>
        {
            entity.ToTable("patient_medical_profiles");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.PatientAccountId).HasColumnName("patient_account_id");
            entity.Property(x => x.DateOfBirth).HasColumnName("date_of_birth");
            entity.Property(x => x.SexAtBirth).HasColumnName("sex_at_birth");
            entity.Property(x => x.PhoneNumber).HasColumnName("phone_number");
            entity.Property(x => x.CountryCode).HasColumnName("country_code");
            entity.Property(x => x.City).HasColumnName("city");
            entity.Property(x => x.TimeZone).HasColumnName("time_zone");
            entity.Property(x => x.EmergencyContactName).HasColumnName("emergency_contact_name");
            entity.Property(x => x.EmergencyContactPhone).HasColumnName("emergency_contact_phone");
            entity.Property(x => x.EmergencyContactRelationship).HasColumnName("emergency_contact_relationship");
            entity.Property(x => x.ChiefConcern).HasColumnName("chief_concern");
            entity.Property(x => x.Symptoms).HasColumnName("symptoms");
            entity.Property(x => x.SymptomDuration).HasColumnName("symptom_duration");
            entity.Property(x => x.CurrentMedications).HasColumnName("current_medications");
            entity.Property(x => x.Allergies).HasColumnName("allergies");
            entity.Property(x => x.KnownConditions).HasColumnName("known_conditions");
            entity.Property(x => x.PastSurgeries).HasColumnName("past_surgeries");
            entity.Property(x => x.PregnancyStatus).HasColumnName("pregnancy_status");
            entity.Property(x => x.LifestyleFactors).HasColumnName("lifestyle_factors");
            entity.Property(x => x.PreferredConsultationLanguage).HasColumnName("preferred_consultation_language");
            entity.Property(x => x.UrgencyLevel).HasColumnName("urgency_level");
            entity.Property(x => x.EmergencySymptoms).HasColumnName("emergency_symptoms");
            entity.Property(x => x.MedicalDisclaimerAccepted).HasColumnName("medical_disclaimer_accepted");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<AuditEvent>(entity =>
        {
            entity.ToTable("audit_events");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.ActorId).HasColumnName("actor_id");
            entity.Property(x => x.ActorType).HasColumnName("actor_type");
            entity.Property(x => x.Action).HasColumnName("action");
            entity.Property(x => x.TargetType).HasColumnName("target_type");
            entity.Property(x => x.TargetId).HasColumnName("target_id");
            entity.Property(x => x.OccurredAt).HasColumnName("occurred_at");
        });

        modelBuilder.Entity<DoctorProfile>(entity =>
        {
            entity.ToTable("doctor_profiles");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.MedplumPractitionerId).HasColumnName("medplum_practitioner_id");
            entity.Property(x => x.DisplayName).HasColumnName("display_name");
            entity.Property(x => x.CountryCode).HasColumnName("country_code");
            entity.Property(x => x.PrimarySpecialty).HasColumnName("primary_specialty");
            entity.Property(x => x.VerificationStatus).HasColumnName("verification_status").HasConversion<string>();
            entity.Property(x => x.MarketplaceStatus).HasColumnName("marketplace_status").HasConversion<string>();
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<DoctorOnboardingRecord>(entity =>
        {
            entity.ToTable("doctor_onboarding_records");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.DoctorProfileId).HasColumnName("doctor_profile_id");
            entity.Property(x => x.LegalName).HasColumnName("legal_name");
            entity.Property(x => x.Email).HasColumnName("email");
            entity.Property(x => x.PhoneNumber).HasColumnName("phone_number");
            entity.Property(x => x.CountryOfPractice).HasColumnName("country_of_practice");
            entity.Property(x => x.LicenseNumber).HasColumnName("license_number");
            entity.Property(x => x.LicensingAuthority).HasColumnName("licensing_authority");
            entity.Property(x => x.Qualifications).HasColumnName("qualifications");
            entity.Property(x => x.YearsOfExperience).HasColumnName("years_of_experience");
            entity.Property(x => x.Biography).HasColumnName("biography");
            entity.Property(x => x.InsuranceProvider).HasColumnName("insurance_provider");
            entity.Property(x => x.InsurancePolicyNumber).HasColumnName("insurance_policy_number");
            entity.Property(x => x.LicenseExpiryDate).HasColumnName("license_expiry_date");
            entity.Property(x => x.ReviewerId).HasColumnName("reviewer_id");
            entity.Property(x => x.ReviewNotes).HasColumnName("review_notes");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<DoctorLanguage>(entity =>
        {
            entity.ToTable("doctor_languages");
            entity.HasKey(x => new { x.DoctorProfileId, x.LanguageCode });
            entity.Property(x => x.DoctorProfileId).HasColumnName("doctor_profile_id");
            entity.Property(x => x.LanguageCode).HasColumnName("language_code");
        });

        modelBuilder.Entity<DoctorAvailabilityWindow>(entity =>
        {
            entity.ToTable("doctor_availability_windows");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.DoctorProfileId).HasColumnName("doctor_profile_id");
            entity.Property(x => x.StartsAt).HasColumnName("starts_at");
            entity.Property(x => x.EndsAt).HasColumnName("ends_at");
            entity.Property(x => x.ConsultationMode).HasColumnName("consultation_mode").HasConversion<string>();
            entity.Property(x => x.IsInstantEnabled).HasColumnName("is_instant_enabled");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<ConsultationBooking>(entity =>
        {
            entity.ToTable("consultation_bookings");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.PatientAccountId).HasColumnName("patient_account_id");
            entity.Property(x => x.DoctorProfileId).HasColumnName("doctor_profile_id");
            entity.Property(x => x.MedplumAppointmentId).HasColumnName("medplum_appointment_id");
            entity.Property(x => x.SpecialtyCode).HasColumnName("specialty_code");
            entity.Property(x => x.ConsultationMode).HasColumnName("consultation_mode").HasConversion<string>();
            entity.Property(x => x.BookingType).HasColumnName("booking_type").HasConversion<string>();
            entity.Property(x => x.Status).HasColumnName("status").HasConversion<string>();
            entity.Property(x => x.ScheduledStartsAt).HasColumnName("scheduled_starts_at");
            entity.Property(x => x.ScheduledEndsAt).HasColumnName("scheduled_ends_at");
            entity.OwnsOne(x => x.PricePerSecond, money =>
            {
                money.Property(x => x.MinorUnits).HasColumnName("price_per_second_minor");
                money.Property(x => x.Currency).HasColumnName("currency");
            });
            entity.Property(x => x.ReservedSeconds).HasColumnName("reserved_seconds");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        });

        // Identity Configuration
        ConfigureIdentityEntities(modelBuilder);
    }

    private void ConfigureIdentityEntities(ModelBuilder modelBuilder)
    {
        // PlatformUser
        modelBuilder.Entity<PlatformUser>(entity =>
        {
            entity.ToTable("platform_users");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Email).HasColumnName("email").HasMaxLength(256);
            entity.Property(x => x.NormalizedEmail).HasColumnName("normalized_email").HasMaxLength(256);
            entity.Property(x => x.PasswordHash).HasColumnName("password_hash");
            entity.Property(x => x.SecurityStamp).HasColumnName("security_stamp").HasMaxLength(64);
            entity.Property(x => x.ConcurrencyStamp).HasColumnName("concurrency_stamp").HasMaxLength(64);
            entity.Property(x => x.PhoneNumber).HasColumnName("phone_number").HasMaxLength(50);
            entity.Property(x => x.FirstName).HasColumnName("first_name").HasMaxLength(100);
            entity.Property(x => x.LastName).HasColumnName("last_name").HasMaxLength(100);
            entity.Property(x => x.DisplayName).HasColumnName("display_name").HasMaxLength(200);
            entity.Property(x => x.CountryCode).HasColumnName("country_code").HasMaxLength(2);
            entity.Property(x => x.PreferredLanguage).HasColumnName("preferred_language").HasMaxLength(10);
            entity.Property(x => x.UserType).HasColumnName("user_type").HasConversion<string>().HasMaxLength(50);
            entity.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(50);
            entity.Property(x => x.EmailConfirmed).HasColumnName("email_confirmed");
            entity.Property(x => x.PhoneNumberConfirmed).HasColumnName("phone_number_confirmed");
            entity.Property(x => x.EmailVerificationToken).HasColumnName("email_verification_token");
            entity.Property(x => x.EmailVerificationTokenExpiresAt).HasColumnName("email_verification_token_expires_at");
            entity.Property(x => x.PhoneVerificationCode).HasColumnName("phone_verification_code").HasMaxLength(10);
            entity.Property(x => x.PhoneVerificationCodeExpiresAt).HasColumnName("phone_verification_code_expires_at");
            entity.Property(x => x.TwoFactorEnabled).HasColumnName("two_factor_enabled");
            entity.Property(x => x.TwoFactorSecret).HasColumnName("two_factor_secret");
            entity.Property(x => x.TwoFactorRecoveryCodes).HasColumnName("two_factor_recovery_codes");
            entity.Property(x => x.LockoutEnabled).HasColumnName("lockout_enabled");
            entity.Property(x => x.LockoutEndAt).HasColumnName("lockout_end_at");
            entity.Property(x => x.AccessFailedCount).HasColumnName("access_failed_count");
            entity.Property(x => x.ExternalProvider).HasColumnName("external_provider").HasMaxLength(50);
            entity.Property(x => x.ExternalProviderId).HasColumnName("external_provider_id").HasMaxLength(256);
            entity.Property(x => x.TermsAccepted).HasColumnName("terms_accepted");
            entity.Property(x => x.TermsAcceptedAt).HasColumnName("terms_accepted_at");
            entity.Property(x => x.TermsVersion).HasColumnName("terms_version").HasMaxLength(20);
            entity.Property(x => x.PrivacyPolicyAccepted).HasColumnName("privacy_policy_accepted");
            entity.Property(x => x.PrivacyPolicyAcceptedAt).HasColumnName("privacy_policy_accepted_at");
            entity.Property(x => x.PrivacyPolicyVersion).HasColumnName("privacy_policy_version").HasMaxLength(20);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.Property(x => x.LastLoginAt).HasColumnName("last_login_at");
            entity.Property(x => x.LastLoginIp).HasColumnName("last_login_ip").HasMaxLength(50);

            entity.HasIndex(x => x.NormalizedEmail).IsUnique().HasDatabaseName("ix_platform_users_normalized_email");
            entity.HasIndex(x => x.UserType).HasDatabaseName("ix_platform_users_user_type");
            entity.HasIndex(x => x.Status).HasDatabaseName("ix_platform_users_status");
        });

        // Role
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("roles");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(100);
            entity.Property(x => x.NormalizedName).HasColumnName("normalized_name").HasMaxLength(100);
            entity.Property(x => x.Description).HasColumnName("description").HasMaxLength(500);
            entity.Property(x => x.Priority).HasColumnName("priority");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");

            entity.HasIndex(x => x.NormalizedName).IsUnique().HasDatabaseName("ix_roles_normalized_name");
        });

        // UserRole
        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.ToTable("user_roles");
            entity.HasKey(x => new { x.UserId, x.RoleId });
            entity.Property(x => x.UserId).HasColumnName("user_id");
            entity.Property(x => x.RoleId).HasColumnName("role_id");
            entity.Property(x => x.AssignedAt).HasColumnName("assigned_at");
            entity.Property(x => x.AssignedBy).HasColumnName("assigned_by").HasMaxLength(100);

            entity.HasOne(x => x.User).WithMany(x => x.UserRoles).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Role).WithMany(x => x.UserRoles).HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Cascade);
        });

        // Permission
        modelBuilder.Entity<Permission>(entity =>
        {
            entity.ToTable("permissions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(100);
            entity.Property(x => x.Category).HasColumnName("category").HasMaxLength(50);
            entity.Property(x => x.Description).HasColumnName("description").HasMaxLength(500);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");

            entity.HasIndex(x => x.Name).IsUnique().HasDatabaseName("ix_permissions_name");
        });

        // RolePermission
        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.ToTable("role_permissions");
            entity.HasKey(x => new { x.RoleId, x.PermissionId });
            entity.Property(x => x.RoleId).HasColumnName("role_id");
            entity.Property(x => x.PermissionId).HasColumnName("permission_id");
            entity.Property(x => x.AssignedAt).HasColumnName("assigned_at");

            entity.HasOne(x => x.Role).WithMany(x => x.RolePermissions).HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Permission).WithMany(x => x.RolePermissions).HasForeignKey(x => x.PermissionId).OnDelete(DeleteBehavior.Cascade);
        });

        // RefreshToken
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("refresh_tokens");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.UserId).HasColumnName("user_id");
            entity.Property(x => x.TokenHash).HasColumnName("token_hash");
            entity.Property(x => x.DeviceId).HasColumnName("device_id").HasMaxLength(100);
            entity.Property(x => x.DeviceName).HasColumnName("device_name").HasMaxLength(200);
            entity.Property(x => x.IpAddress).HasColumnName("ip_address").HasMaxLength(50);
            entity.Property(x => x.UserAgent).HasColumnName("user_agent").HasMaxLength(500);
            entity.Property(x => x.ExpiresAt).HasColumnName("expires_at");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.RevokedAt).HasColumnName("revoked_at");
            entity.Property(x => x.RevokedReason).HasColumnName("revoked_reason").HasMaxLength(200);
            entity.Property(x => x.IsRevoked).HasColumnName("is_revoked");
            entity.Property(x => x.IsUsed).HasColumnName("is_used");
            entity.Property(x => x.UsedAt).HasColumnName("used_at");
            entity.Property(x => x.ReplacedByTokenId).HasColumnName("replaced_by_token_id").HasMaxLength(100);

            entity.HasIndex(x => x.UserId).HasDatabaseName("ix_refresh_tokens_user_id");
            entity.HasIndex(x => x.TokenHash).HasDatabaseName("ix_refresh_tokens_token_hash");
            entity.HasIndex(x => x.ExpiresAt).HasDatabaseName("ix_refresh_tokens_expires_at");

            entity.HasOne(x => x.User).WithMany(x => x.RefreshTokens).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        // LoginAttempt
        modelBuilder.Entity<LoginAttempt>(entity =>
        {
            entity.ToTable("login_attempts");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.UserId).HasColumnName("user_id").HasMaxLength(100);
            entity.Property(x => x.Email).HasColumnName("email").HasMaxLength(256);
            entity.Property(x => x.IpAddress).HasColumnName("ip_address").HasMaxLength(50);
            entity.Property(x => x.UserAgent).HasColumnName("user_agent").HasMaxLength(500);
            entity.Property(x => x.DeviceId).HasColumnName("device_id").HasMaxLength(100);
            entity.Property(x => x.Result).HasColumnName("result").HasConversion<string>().HasMaxLength(50);
            entity.Property(x => x.FailureReason).HasColumnName("failure_reason").HasMaxLength(200);
            entity.Property(x => x.MfaMethodUsed).HasColumnName("mfa_method_used").HasMaxLength(50);
            entity.Property(x => x.MfaSuccessful).HasColumnName("mfa_successful");
            entity.Property(x => x.CountryCode).HasColumnName("country_code").HasMaxLength(2);
            entity.Property(x => x.City).HasColumnName("city").HasMaxLength(100);
            entity.Property(x => x.IsKnownDevice).HasColumnName("is_known_device");
            entity.Property(x => x.IsSuspicious).HasColumnName("is_suspicious");
            entity.Property(x => x.AttemptedAt).HasColumnName("attempted_at");

            entity.HasIndex(x => x.UserId).HasDatabaseName("ix_login_attempts_user_id");
            entity.HasIndex(x => x.Email).HasDatabaseName("ix_login_attempts_email");
            entity.HasIndex(x => x.IpAddress).HasDatabaseName("ix_login_attempts_ip_address");
            entity.HasIndex(x => x.AttemptedAt).HasDatabaseName("ix_login_attempts_attempted_at");
        });

        // DeviceAuthorization
        modelBuilder.Entity<DeviceAuthorization>(entity =>
        {
            entity.ToTable("device_authorizations");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.UserId).HasColumnName("user_id");
            entity.Property(x => x.DeviceId).HasColumnName("device_id").HasMaxLength(100);
            entity.Property(x => x.DeviceName).HasColumnName("device_name").HasMaxLength(200);
            entity.Property(x => x.DeviceType).HasColumnName("device_type").HasMaxLength(50);
            entity.Property(x => x.OperatingSystem).HasColumnName("operating_system").HasMaxLength(100);
            entity.Property(x => x.Browser).HasColumnName("browser").HasMaxLength(100);
            entity.Property(x => x.PublicKey).HasColumnName("public_key");
            entity.Property(x => x.IpAddress).HasColumnName("ip_address").HasMaxLength(50);
            entity.Property(x => x.UserAgent).HasColumnName("user_agent").HasMaxLength(500);
            entity.Property(x => x.CountryCode).HasColumnName("country_code").HasMaxLength(2);
            entity.Property(x => x.City).HasColumnName("city").HasMaxLength(100);
            entity.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(50);
            entity.Property(x => x.TrustLevel).HasColumnName("trust_level").HasConversion<string>().HasMaxLength(50);
            entity.Property(x => x.FirstSeenAt).HasColumnName("first_seen_at");
            entity.Property(x => x.LastSeenAt).HasColumnName("last_seen_at");
            entity.Property(x => x.RevokedAt).HasColumnName("revoked_at");
            entity.Property(x => x.RevokedReason).HasColumnName("revoked_reason").HasMaxLength(200);
            entity.Property(x => x.MfaVerified).HasColumnName("mfa_verified");
            entity.Property(x => x.MfaVerifiedAt).HasColumnName("mfa_verified_at");

            entity.HasIndex(x => x.UserId).HasDatabaseName("ix_device_authorizations_user_id");
            entity.HasIndex(x => new { x.UserId, x.DeviceId }).IsUnique().HasDatabaseName("ix_device_authorizations_user_device");
            entity.HasIndex(x => x.LastSeenAt).HasDatabaseName("ix_device_authorizations_last_seen_at");

            entity.HasOne(x => x.User).WithMany(x => x.DeviceAuthorizations).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserDevice>(entity =>
        {
            entity.ToTable("user_devices");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.UserId).HasColumnName("user_id");
            entity.Property(x => x.DeviceId).HasColumnName("device_id").HasMaxLength(100);
            entity.Property(x => x.DeviceName).HasColumnName("device_name").HasMaxLength(200);
            entity.Property(x => x.DeviceType).HasColumnName("device_type").HasMaxLength(50);
            entity.Property(x => x.UserAgent).HasColumnName("user_agent").HasMaxLength(500);
            entity.Property(x => x.IpAddress).HasColumnName("ip_address").HasMaxLength(50);
            entity.Property(x => x.DeviceFingerprint).HasColumnName("device_fingerprint").HasMaxLength(500);
            entity.Property(x => x.FirstSeenAt).HasColumnName("first_seen_at");
            entity.Property(x => x.LastSeenAt).HasColumnName("last_seen_at");
            entity.Property(x => x.LastUsedAt).HasColumnName("last_used_at");
            entity.Property(x => x.IsTrusted).HasColumnName("is_trusted");
            entity.Property(x => x.IsBlocked).HasColumnName("is_blocked");
            entity.Property(x => x.BlockedAt).HasColumnName("blocked_at");
            entity.Property(x => x.BlockedReason).HasColumnName("blocked_reason").HasMaxLength(200);
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");

            entity.HasIndex(x => new { x.UserId, x.DeviceId }).IsUnique().HasDatabaseName("ix_user_devices_user_device");
            entity.HasIndex(x => x.DeviceFingerprint).HasDatabaseName("ix_user_devices_fingerprint");
            entity.HasIndex(x => x.LastSeenAt).HasDatabaseName("ix_user_devices_last_seen_at");

            entity.HasOne(x => x.User).WithMany(x => x.UserDevices).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserSession>(entity =>
        {
            entity.ToTable("user_sessions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.UserId).HasColumnName("user_id");
            entity.Property(x => x.SessionId).HasColumnName("session_id").HasMaxLength(100);
            entity.Property(x => x.DeviceId).HasColumnName("device_id").HasMaxLength(100);
            entity.Property(x => x.IpAddress).HasColumnName("ip_address").HasMaxLength(50);
            entity.Property(x => x.UserAgent).HasColumnName("user_agent").HasMaxLength(500);
            entity.Property(x => x.StartedAt).HasColumnName("started_at");
            entity.Property(x => x.LastActivityAt).HasColumnName("last_activity_at");
            entity.Property(x => x.ExpiresAt).HasColumnName("expires_at");
            entity.Property(x => x.EndedAt).HasColumnName("ended_at");
            entity.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(50);
            entity.Property(x => x.EndReason).HasColumnName("end_reason").HasMaxLength(200);

            entity.HasIndex(x => x.SessionId).IsUnique().HasDatabaseName("ix_user_sessions_session_id");
            entity.HasIndex(x => x.UserId).HasDatabaseName("ix_user_sessions_user_id");
            entity.HasIndex(x => x.DeviceId).HasDatabaseName("ix_user_sessions_device_id");
            entity.HasIndex(x => x.ExpiresAt).HasDatabaseName("ix_user_sessions_expires_at");
            entity.HasIndex(x => x.LastActivityAt).HasDatabaseName("ix_user_sessions_last_activity_at");

            entity.HasOne(x => x.User).WithMany(x => x.UserSessions).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        // Analytics Configuration
        modelBuilder.Entity<AnalyticsReport>(entity =>
        {
            entity.ToTable("analytics_reports");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.ReportType).HasColumnName("report_type").HasMaxLength(100);
            entity.Property(x => x.Title).HasColumnName("title").HasMaxLength(500);
            entity.Property(x => x.Description).HasColumnName("description").HasMaxLength(1000);
            entity.Property(x => x.Format).HasColumnName("format").HasMaxLength(50);
            entity.Property(x => x.GeneratedAt).HasColumnName("generated_at");
            entity.Property(x => x.StartedAt).HasColumnName("started_at");
            entity.Property(x => x.CompletedAt).HasColumnName("completed_at");
            entity.Property(x => x.RecordCount).HasColumnName("record_count");
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(50);
            entity.Property(x => x.CreatedBy).HasColumnName("created_by").HasMaxLength(100);
            entity.Property(x => x.Parameters).HasColumnName("parameters").HasColumnType("jsonb");
            entity.Property(x => x.ResultLocation).HasColumnName("result_location").HasMaxLength(500);

            entity.HasIndex(x => x.ReportType).HasDatabaseName("ix_analytics_reports_report_type");
            entity.HasIndex(x => x.Status).HasDatabaseName("ix_analytics_reports_status");
            entity.HasIndex(x => x.GeneratedAt).HasDatabaseName("ix_analytics_reports_generated_at");
        });

        modelBuilder.Entity<ConsultationAnalytics>(entity =>
        {
            entity.ToTable("consultation_analytics");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Date).HasColumnName("date");
            entity.Property(x => x.ConsultationMode).HasColumnName("consultation_mode").HasMaxLength(50);
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(50);
            entity.Property(x => x.Count).HasColumnName("count");
            entity.Property(x => x.CompletedCount).HasColumnName("completed_count");
            entity.Property(x => x.CancelledCount).HasColumnName("cancelled_count");
            entity.Property(x => x.NoShowCount).HasColumnName("no_show_count");
            entity.Property(x => x.TotalBillableSeconds).HasColumnName("total_billable_seconds");
            entity.Property(x => x.TotalRevenueMinor).HasColumnName("total_revenue_minor");
            entity.Property(x => x.AverageDurationSeconds).HasColumnName("average_duration_seconds");
            entity.Property(x => x.DoctorProfileId).HasColumnName("doctor_profile_id");
            entity.Property(x => x.PatientAccountId).HasColumnName("patient_account_id");

            entity.HasIndex(x => x.Date).HasDatabaseName("ix_consultation_analytics_date");
            entity.HasIndex(x => x.DoctorProfileId).HasDatabaseName("ix_consultation_analytics_doctor_profile_id");
            entity.HasIndex(x => x.PatientAccountId).HasDatabaseName("ix_consultation_analytics_patient_account_id");
        });

        // Review Configuration
        modelBuilder.Entity<Review>(entity =>
        {
            entity.ToTable("reviews");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.PatientAccountId).HasColumnName("patient_account_id");
            entity.Property(x => x.DoctorProfileId).HasColumnName("doctor_profile_id");
            entity.Property(x => x.ConsultationBookingId).HasColumnName("consultation_booking_id");
            entity.Property(x => x.Rating).HasColumnName("rating");
            entity.Property(x => x.Comment).HasColumnName("comment").HasMaxLength(2000);
            entity.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.ReviewedAt).HasColumnName("reviewed_at");
            entity.Property(x => x.ReviewedBy).HasColumnName("reviewed_by").HasMaxLength(100);

            entity.HasIndex(x => x.DoctorProfileId).HasDatabaseName("ix_reviews_doctor_id");
            entity.HasIndex(x => x.PatientAccountId).HasDatabaseName("ix_reviews_patient_id");
            entity.HasIndex(x => x.Status).HasDatabaseName("ix_reviews_status");
        });

        // Notification Configuration
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("notifications");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.UserId).HasColumnName("user_id");
            entity.Property(x => x.Type).HasColumnName("type").HasConversion<string>().HasMaxLength(20);
            entity.Property(x => x.Title).HasColumnName("title").HasMaxLength(200);
            entity.Property(x => x.Message).HasColumnName("message").HasMaxLength(2000);
            entity.Property(x => x.IsRead).HasColumnName("is_read");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.ReadAt).HasColumnName("read_at");
            entity.Property(x => x.ActionUrl).HasColumnName("action_url").HasMaxLength(500);
            entity.Property(x => x.Metadata).HasColumnName("metadata").HasColumnType("jsonb");

            entity.HasIndex(x => x.UserId).HasDatabaseName("ix_notifications_user_id");
            entity.HasIndex(x => x.IsRead).HasDatabaseName("ix_notifications_is_read");
            entity.HasIndex(x => x.CreatedAt).HasDatabaseName("ix_notifications_created_at");
        });

        // ResearchExport Configuration
        modelBuilder.Entity<ResearchExport>(entity =>
        {
            entity.ToTable("research_exports");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.RequesterId).HasColumnName("requester_id");
            entity.Property(x => x.RequesterType).HasColumnName("requester_type").HasMaxLength(50);
            entity.Property(x => x.Title).HasColumnName("title").HasMaxLength(200);
            entity.Property(x => x.Description).HasColumnName("description").HasMaxLength(1000);
            entity.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20);
            entity.Property(x => x.RequestedAt).HasColumnName("requested_at");
            entity.Property(x => x.StartedAt).HasColumnName("started_at");
            entity.Property(x => x.CompletedAt).HasColumnName("completed_at");
            entity.Property(x => x.RecordCount).HasColumnName("record_count");
            entity.Property(x => x.DownloadUrl).HasColumnName("download_url").HasMaxLength(500);
            entity.Property(x => x.FailureReason).HasColumnName("failure_reason").HasMaxLength(500);
            entity.Property(x => x.Parameters).HasColumnName("parameters").HasColumnType("jsonb");

            entity.HasIndex(x => x.RequesterId).HasDatabaseName("ix_research_exports_requester_id");
            entity.HasIndex(x => x.Status).HasDatabaseName("ix_research_exports_status");
        });

        // ConsultationRequest Configuration
        modelBuilder.Entity<ConsultationRequest>(entity =>
        {
            entity.ToTable("consultation_requests");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.PatientAccountId).HasColumnName("patient_account_id");
            entity.Property(x => x.DoctorProfileId).HasColumnName("doctor_profile_id");
            entity.Property(x => x.RequestedAt).HasColumnName("requested_at");
            entity.Property(x => x.ScheduledAt).HasColumnName("scheduled_at");
            entity.Property(x => x.CompletedAt).HasColumnName("completed_at");
            entity.Property(x => x.Mode).HasColumnName("mode").HasConversion<string>().HasMaxLength(20);
            entity.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20);
            entity.Property(x => x.Reason).HasColumnName("reason").HasMaxLength(500);
            entity.Property(x => x.RejectionReason).HasColumnName("rejection_reason").HasMaxLength(500);
            entity.Property(x => x.MedplumEncounterId).HasColumnName("medplum_encounter_id").HasMaxLength(100);

            entity.HasIndex(x => x.PatientAccountId).HasDatabaseName("ix_requests_patient_id");
            entity.HasIndex(x => x.DoctorProfileId).HasDatabaseName("ix_requests_doctor_id");
            entity.HasIndex(x => x.Status).HasDatabaseName("ix_requests_status");
        });

        // GdprRequest Configuration
        modelBuilder.Entity<GdprRequest>(entity =>
        {
            entity.ToTable("gdpr_requests");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.PatientAccountId).HasColumnName("patient_account_id");
            entity.Property(x => x.RequestType).HasColumnName("request_type").HasConversion<string>().HasMaxLength(20);
            entity.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20);
            entity.Property(x => x.SubmittedAt).HasColumnName("submitted_at");
            entity.Property(x => x.ProcessedAt).HasColumnName("processed_at");
            entity.Property(x => x.Details).HasColumnName("details").HasMaxLength(1000);
            entity.Property(x => x.RejectionReason).HasColumnName("rejection_reason").HasMaxLength(500);
            entity.Property(x => x.DownloadUrl).HasColumnName("download_url").HasMaxLength(500);

            entity.HasIndex(x => x.PatientAccountId).HasDatabaseName("ix_gdpr_requests_patient_id");
            entity.HasIndex(x => x.Status).HasDatabaseName("ix_gdpr_requests_status");
        });

        // AuditLog Configuration
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("audit_logs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Timestamp).HasColumnName("timestamp");
            entity.Property(x => x.EventType).HasColumnName("event_type").HasMaxLength(100);
            entity.Property(x => x.UserId).HasColumnName("user_id").HasMaxLength(100);
            entity.Property(x => x.UserType).HasColumnName("user_type").HasMaxLength(50);
            entity.Property(x => x.ResourceType).HasColumnName("resource_type").HasMaxLength(100);
            entity.Property(x => x.ResourceId).HasColumnName("resource_id");
            entity.Property(x => x.Action).HasColumnName("action").HasMaxLength(100);
            entity.Property(x => x.Changes).HasColumnName("changes").HasColumnType("jsonb");
            entity.Property(x => x.IpAddress).HasColumnName("ip_address").HasMaxLength(45);
            entity.Property(x => x.UserAgent).HasColumnName("user_agent").HasMaxLength(500);
            entity.Property(x => x.CorrelationId).HasColumnName("correlation_id").HasMaxLength(100);

            entity.HasIndex(x => x.Timestamp).HasDatabaseName("ix_audit_logs_timestamp");
            entity.HasIndex(x => x.UserId).HasDatabaseName("ix_audit_logs_user_id");
            entity.HasIndex(x => x.CorrelationId).HasDatabaseName("ix_audit_logs_correlation_id");
        });

        // ConsultationSession Configuration
        modelBuilder.Entity<InstantQueueEntry>(entity =>
        {
            entity.ToTable("instant_queue_entries");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.PatientAccountId).HasColumnName("patient_account_id");
            entity.Property(x => x.DoctorProfileId).HasColumnName("doctor_profile_id");
            entity.Property(x => x.JoinedAt).HasColumnName("joined_at");
            entity.Property(x => x.MatchedAt).HasColumnName("matched_at");
            entity.Property(x => x.StartedAt).HasColumnName("started_at");
            entity.Property(x => x.ExpiresAt).HasColumnName("expires_at");
            entity.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20);
            entity.Property(x => x.Priority).HasColumnName("priority");
            entity.Property(x => x.Reason).HasColumnName("reason").HasMaxLength(500);

            entity.HasIndex(x => x.PatientAccountId).HasDatabaseName("ix_queue_patient_id");
            entity.HasIndex(x => x.Status).HasDatabaseName("ix_queue_status");
            entity.HasIndex(x => x.ExpiresAt).HasDatabaseName("ix_queue_expires_at");
        });

        // ConsultationSession Configuration
        modelBuilder.Entity<ConsultationSession>(entity =>
        {
            entity.ToTable("consultation_sessions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.ConsultationBookingId).HasColumnName("consultation_booking_id");
            entity.Property(x => x.MedplumEncounterId).HasColumnName("medplum_encounter_id");
            entity.Property(x => x.VideoProvider).HasColumnName("video_provider").HasMaxLength(50);
            entity.Property(x => x.VideoRoomId).HasColumnName("video_room_id").HasMaxLength(100);
            entity.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(50);
            entity.Property(x => x.BillableSeconds).HasColumnName("billable_seconds");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");

            entity.HasIndex(x => x.ConsultationBookingId).IsUnique().HasDatabaseName("ix_consultation_sessions_booking_id");
            entity.HasIndex(x => x.Status).HasDatabaseName("ix_consultation_sessions_status");
        });

        // ParticipantEvent Configuration
        modelBuilder.Entity<ParticipantEvent>(entity =>
        {
            entity.ToTable("participant_events");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.ConsultationSessionId).HasColumnName("consultation_session_id");
            entity.Property(x => x.EventType).HasColumnName("event_type").HasConversion<string>().HasMaxLength(50);
            entity.Property(x => x.OccurredAt).HasColumnName("occurred_at");
            entity.Property(x => x.ParticipantType).HasColumnName("participant_type").HasMaxLength(50);
            entity.Property(x => x.ParticipantId).HasColumnName("participant_id").HasMaxLength(100);
            entity.Property(x => x.Metadata).HasColumnName("metadata").HasColumnType("jsonb");

            entity.HasIndex(x => x.ConsultationSessionId).HasDatabaseName("ix_participant_events_session_id");
            entity.HasIndex(x => x.OccurredAt).HasDatabaseName("ix_participant_events_occurred_at");

            entity.HasOne(x => x.Session)
                .WithMany(x => x.ParticipantEvents)
                .HasForeignKey(x => x.ConsultationSessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
