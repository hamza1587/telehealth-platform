using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Telehealth.Platform.Infrastructure.Persistence;
using DbContextAttribute = Microsoft.EntityFrameworkCore.Infrastructure.DbContextAttribute;

#nullable disable

namespace Telehealth.Platform.Infrastructure.Migrations
{
    /// <inheritdoc />
    [DbContextAttribute(typeof(PlatformDbContext))]
    [Migration("20260601190000_AddMissingEntities")]
    public partial class AddMissingEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                -- Tenants
                create table if not exists tenants (
                    id uuid primary key,
                    name varchar(200) not null,
                    subdomain varchar(100) not null unique,
                    settings_logo_url varchar(500),
                    settings_primary_color varchar(20) not null default '#007bff',
                    settings_secondary_color varchar(20) not null default '#6c757d',
                    settings_max_users int not null default 100,
                    settings_max_storage_mb int not null default 1000,
                    settings_enable_ai_features boolean not null default true,
                    settings_enable_ehr_integration boolean not null default true,
                    settings_allowed_specialties text[] not null default '{}',
                    created_at timestamp with time zone not null,
                    updated_at timestamp with time zone not null,
                    is_active boolean not null default true
                );
                create unique index if not exists ix_tenants_subdomain on tenants(subdomain);

                -- Tenant Memberships
                create table if not exists tenant_memberships (
                    id uuid primary key,
                    tenant_id uuid not null,
                    user_id uuid not null,
                    role varchar(50) not null,
                    permissions text[] not null default '{}',
                    assigned_at timestamp with time zone not null,
                    is_active boolean not null default true,
                    foreign key (tenant_id) references tenants(id) on delete cascade
                );
                create unique index if not exists ix_tenant_memberships_tenant_user on tenant_memberships(tenant_id, user_id);

                -- Notifications
                create table if not exists notifications (
                    id uuid primary key,
                    user_id uuid not null,
                    type varchar(100) not null,
                    title varchar(200) not null,
                    message text not null,
                    is_read boolean not null default false,
                    read_at timestamp with time zone,
                    action_url varchar(500),
                    metadata jsonb,
                    created_at timestamp with time zone not null
                );
                create index if not exists ix_notifications_user_id on notifications(user_id);
                create index if not exists ix_notifications_is_read on notifications(is_read);
                create index if not exists ix_notifications_created_at on notifications(created_at);

                -- Payments
                create table if not exists payments (
                    id uuid primary key,
                    patient_account_id uuid not null,
                    amount_minor bigint not null,
                    currency varchar(3) not null,
                    status varchar(20) not null,
                    payment_method_type varchar(50) not null,
                    external_payment_id varchar(100),
                    description varchar(500),
                    failure_reason varchar(500),
                    metadata jsonb,
                    created_at timestamp with time zone not null,
                    completed_at timestamp with time zone,
                    updated_at timestamp with time zone not null
                );
                create index if not exists ix_payments_patient_id on payments(patient_account_id);
                create index if not exists ix_payments_status on payments(status);
                create index if not exists ix_payments_external_id on payments(external_payment_id);
                create index if not exists ix_payments_created_at on payments(created_at);

                -- Payment Methods
                create table if not exists payment_methods (
                    id uuid primary key,
                    patient_account_id uuid not null,
                    type varchar(50) not null,
                    token varchar(500) not null,
                    last_four varchar(4),
                    expiry_month int,
                    expiry_year int,
                    card_holder_name varchar(200),
                    is_default boolean not null default false,
                    provider varchar(50),
                    metadata jsonb,
                    created_at timestamp with time zone not null,
                    updated_at timestamp with time zone not null
                );
                create index if not exists ix_payment_methods_patient_id on payment_methods(patient_account_id);
                create index if not exists ix_payment_methods_token on payment_methods(token);
                create index if not exists ix_payment_methods_default on payment_methods(is_default);

                -- Disputes
                create table if not exists disputes (
                    id uuid primary key,
                    payment_id uuid not null,
                    reason varchar(500) not null,
                    status varchar(20) not null,
                    external_dispute_id varchar(100),
                    amount_minor bigint not null,
                    currency varchar(3) not null,
                    evidence_due_by timestamp with time zone,
                    resolved_at timestamp with time zone,
                    created_at timestamp with time zone not null,
                    updated_at timestamp with time zone not null,
                    foreign key (payment_id) references payments(id) on delete cascade
                );
                create index if not exists ix_disputes_payment_id on disputes(payment_id);
                create index if not exists ix_disputes_status on disputes(status);

                -- Insurance Providers
                create table if not exists "InsuranceProviders" (
                    "Id" uuid primary key,
                    "Name" varchar(200) not null,
                    "Code" varchar(50) not null unique,
                    "CountryCode" varchar(2) not null,
                    "ApiEndpoint" varchar(500),
                    "ApiKey" varchar(500),
                    "IsActive" boolean not null default true,
                    "CreatedAt" timestamp with time zone not null,
                    "UpdatedAt" timestamp with time zone not null
                );

                -- Patient Insurances
                create table if not exists "PatientInsurances" (
                    "Id" uuid primary key,
                    "PatientAccountId" uuid not null,
                    "InsuranceProviderId" uuid not null,
                    "PolicyNumber" varchar(100) not null,
                    "GroupNumber" varchar(100),
                    "HolderName" varchar(200) not null,
                    "HolderRelationship" varchar(50) not null,
                    "EffectiveDate" timestamp with time zone not null,
                    "ExpirationDate" timestamp with time zone,
                    "IsPrimary" boolean not null default true,
                    "IsActive" boolean not null default true,
                    "CreatedAt" timestamp with time zone not null,
                    "UpdatedAt" timestamp with time zone not null,
                    foreign key ("InsuranceProviderId") references "InsuranceProviders"("Id") on delete restrict
                );
                create index if not exists "IX_PatientInsurances_InsuranceProviderId" on "PatientInsurances"("InsuranceProviderId");
                create index if not exists "IX_PatientInsurances_PatientAccountId" on "PatientInsurances"("PatientAccountId");

                -- Clinical Records
                create table if not exists "ClinicalRecords" (
                    "Id" uuid primary key,
                    "PatientAccountId" uuid not null,
                    "DoctorProfileId" uuid,
                    "RecordType" varchar(50) not null,
                    "Title" varchar(200) not null,
                    "Content" text not null,
                    "Diagnosis" text,
                    "Treatment" text,
                    "Status" varchar(20) not null,
                    "Attachments" jsonb,
                    "CreatedAt" timestamp with time zone not null,
                    "UpdatedAt" timestamp with time zone not null
                );

                -- Consent Templates
                create table if not exists "ConsentTemplates" (
                    "Id" uuid primary key,
                    "Name" varchar(200) not null,
                    "Type" varchar(50) not null,
                    "Version" varchar(20) not null,
                    "Content" text not null,
                    "IsActive" boolean not null default true,
                    "CreatedAt" timestamp with time zone not null,
                    "UpdatedAt" timestamp with time zone not null
                );

                -- Patient Consents
                create table if not exists "PatientConsents" (
                    "Id" uuid primary key,
                    "PatientAccountId" uuid not null,
                    "ConsentTemplateId" uuid not null,
                    "SignedAt" timestamp with time zone,
                    "IsGranted" boolean not null default false,
                    "IpAddress" varchar(50),
                    "UserAgent" varchar(500),
                    "ExpiresAt" timestamp with time zone,
                    "CreatedAt" timestamp with time zone not null,
                    foreign key ("ConsentTemplateId") references "ConsentTemplates"("Id") on delete restrict
                );

                -- Consultation Analytics
                create table if not exists consultation_analytics (
                    id uuid primary key,
                    patient_account_id uuid not null,
                    doctor_profile_id uuid not null,
                    date date not null,
                    total_consultations int not null default 0,
                    completed_consultations int not null default 0,
                    cancelled_consultations int not null default 0,
                    no_show_consultations int not null default 0,
                    average_duration_minutes int,
                    total_revenue_minor bigint not null default 0,
                    average_rating decimal(3,2),
                    created_at timestamp with time zone not null,
                    updated_at timestamp with time zone not null
                );
                create index if not exists ix_consultation_analytics_date on consultation_analytics(date);
                create index if not exists ix_consultation_analytics_doctor_profile_id on consultation_analytics(doctor_profile_id);
                create index if not exists ix_consultation_analytics_patient_account_id on consultation_analytics(patient_account_id);

                -- Consultation Requests
                create table if not exists consultation_requests (
                    id uuid primary key,
                    patient_account_id uuid not null,
                    doctor_profile_id uuid not null,
                    requested_at timestamp with time zone not null,
                    scheduled_at timestamp with time zone,
                    chief_concern text not null,
                    symptoms text,
                    urgency_level varchar(20) not null default 'normal',
                    preferred_language varchar(10) not null default 'en',
                    status varchar(20) not null default 'pending',
                    matched_at timestamp with time zone,
                    created_at timestamp with time zone not null,
                    updated_at timestamp with time zone not null
                );
                create index if not exists ix_requests_patient_id on consultation_requests(patient_account_id);
                create index if not exists ix_requests_doctor_id on consultation_requests(doctor_profile_id);
                create index if not exists ix_requests_status on consultation_requests(status);

                -- Dashboard Metrics
                create table if not exists "DashboardMetrics" (
                    "Id" uuid primary key,
                    "MetricType" varchar(50) not null,
                    "Value" decimal(18,4) not null,
                    "Metadata" jsonb,
                    "PeriodStart" timestamp with time zone not null,
                    "PeriodEnd" timestamp with time zone not null,
                    "CreatedAt" timestamp with time zone not null
                );

                -- Instant Queue Entries
                create table if not exists instant_queue_entries (
                    id uuid primary key,
                    patient_account_id uuid not null,
                    requested_at timestamp with time zone not null,
                    matched_at timestamp with time zone,
                    doctor_profile_id uuid,
                    chief_concern text not null,
                    symptoms text,
                    urgency_level varchar(20) not null default 'normal',
                    status varchar(20) not null default 'waiting',
                    position int,
                    estimated_wait_seconds int,
                    expires_at timestamp with time zone not null,
                    created_at timestamp with time zone not null,
                    updated_at timestamp with time zone not null
                );
                create index if not exists ix_queue_patient_id on instant_queue_entries(patient_account_id);
                create index if not exists ix_queue_status on instant_queue_entries(status);
                create index if not exists ix_queue_expires_at on instant_queue_entries(expires_at);

                -- Reviews
                create table if not exists reviews (
                    id uuid primary key,
                    patient_account_id uuid not null,
                    doctor_profile_id uuid not null,
                    consultation_booking_id uuid,
                    rating int not null,
                    comment varchar(2000),
                    status varchar(20) not null default 'pending',
                    created_at timestamp with time zone not null,
                    reviewed_at timestamp with time zone,
                    reviewed_by varchar(100)
                );
                create index if not exists ix_reviews_patient_id on reviews(patient_account_id);
                create index if not exists ix_reviews_doctor_id on reviews(doctor_profile_id);
                create index if not exists ix_reviews_status on reviews(status);

                -- Analytics Reports
                create table if not exists analytics_reports (
                    id uuid primary key,
                    report_type varchar(100) not null,
                    title varchar(500) not null,
                    description varchar(1000) not null,
                    format varchar(50) not null,
                    generated_at timestamp with time zone not null,
                    started_at timestamp with time zone,
                    completed_at timestamp with time zone,
                    record_count bigint,
                    status varchar(50) not null,
                    created_by varchar(100) not null,
                    parameters jsonb not null default '{}'::jsonb
                );
                create index if not exists ix_analytics_reports_report_type on analytics_reports(report_type);
                create index if not exists ix_analytics_reports_status on analytics_reports(status);
                create index if not exists ix_analytics_reports_generated_at on analytics_reports(generated_at);

                -- Video Rooms
                create table if not exists "VideoRooms" (
                    "Id" uuid primary key,
                    "BookingId" uuid not null,
                    "RoomId" text not null,
                    "SessionId" text not null,
                    "Status" int not null,
                    "StartedAt" timestamp with time zone not null,
                    "JoinedAt" timestamp with time zone,
                    "EndedAt" timestamp with time zone,
                    "CreatedBy" text not null,
                    "CreatedAt" timestamp with time zone not null,
                    "UpdatedAt" timestamp with time zone not null
                );

                -- Wallet Payments
                create table if not exists "WalletPayments" (
                    "Id" uuid primary key,
                    "WalletId" uuid not null,
                    "AmountMinor" bigint not null,
                    "Currency" text not null,
                    "PaymentMethod" text not null,
                    "ExternalPaymentId" text not null,
                    "Status" int not null,
                    "StatusDetails" text not null,
                    "CreatedAt" timestamp with time zone not null,
                    "CompletedAt" timestamp with time zone,
                    "UpdatedAt" timestamp with time zone not null
                );

                -- Audit Logs
                create table if not exists audit_logs (
                    id uuid primary key,
                    timestamp timestamp with time zone not null,
                    user_id varchar(100),
                    action varchar(100) not null,
                    entity_type varchar(100) not null,
                    entity_id varchar(100),
                    correlation_id varchar(100),
                    ip_address varchar(50),
                    details jsonb
                );
                create index if not exists ix_audit_logs_user_id on audit_logs(user_id);
                create index if not exists ix_audit_logs_timestamp on audit_logs(timestamp);
                create index if not exists ix_audit_logs_correlation_id on audit_logs(correlation_id);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                drop table if exists audit_logs;
                drop table if exists "WalletPayments";
                drop table if exists "VideoRooms";
                drop table if exists analytics_reports;
                drop table if exists reviews;
                drop table if exists instant_queue_entries;
                drop table if exists "DashboardMetrics";
                drop table if exists consultation_requests;
                drop table if exists consultation_analytics;
                drop table if exists "PatientConsents";
                drop table if exists "ConsentTemplates";
                drop table if exists "ClinicalRecords";
                drop table if exists "PatientInsurances";
                drop table if exists "InsuranceProviders";
                drop table if exists disputes;
                drop table if exists payment_methods;
                drop table if exists payments;
                drop table if exists notifications;
                drop table if exists tenant_memberships;
                drop table if exists tenants;
                """);
        }
    }
}
