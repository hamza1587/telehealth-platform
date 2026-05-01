using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Telehealth.Platform.Infrastructure.Persistence.Migrations;

/// <summary>
/// Migration to add comprehensive identity and authentication schema.
/// Includes users, roles, permissions, MFA, device management, and audit logging.
/// </summary>
public partial class AddIdentitySchema : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            -- =============================================
            -- IDENTITY AND ACCESS MANAGEMENT SCHEMA
            -- =============================================

            -- Platform Users table
            create table platform_users (
                id uuid primary key,
                email text not null,
                normalized_email text not null,
                password_hash text not null,
                security_stamp text not null,
                concurrency_stamp text not null,
                phone_number text null,
                first_name text null,
                last_name text null,
                display_name text null,
                country_code char(2) null,
                preferred_language text not null default 'en',
                user_type text not null,
                status text not null default 'PendingVerification',
                email_confirmed boolean not null default false,
                phone_number_confirmed boolean not null default false,
                email_verification_token text null,
                email_verification_token_expires_at timestamptz null,
                phone_verification_code text null,
                phone_verification_code_expires_at timestamptz null,
                two_factor_enabled boolean not null default false,
                two_factor_secret text null,
                two_factor_recovery_codes text null,
                lockout_enabled boolean not null default true,
                lockout_end_at timestamptz null,
                access_failed_count integer not null default 0,
                external_provider text null,
                external_provider_id text null,
                terms_accepted boolean not null default false,
                terms_accepted_at timestamptz null,
                terms_version text null,
                privacy_policy_accepted boolean not null default false,
                privacy_policy_accepted_at timestamptz null,
                privacy_policy_version text null,
                created_at timestamptz not null default now(),
                updated_at timestamptz not null default now(),
                last_login_at timestamptz null,
                last_login_ip text null
            );

            create unique index ix_platform_users_normalized_email on platform_users(normalized_email);
            create index ix_platform_users_user_type on platform_users(user_type);
            create index ix_platform_users_status on platform_users(status);
            create index ix_platform_users_last_login_at on platform_users(last_login_at desc);

            -- Roles table
            create table roles (
                id uuid primary key,
                name text not null,
                normalized_name text not null,
                description text null,
                priority integer not null default 0,
                created_at timestamptz not null default now()
            );

            create unique index ix_roles_normalized_name on roles(normalized_name);

            -- User Roles join table
            create table user_roles (
                user_id uuid not null references platform_users(id) on delete cascade,
                role_id uuid not null references roles(id) on delete cascade,
                assigned_at timestamptz not null default now(),
                assigned_by text null,
                primary key (user_id, role_id)
            );

            create index ix_user_roles_role_id on user_roles(role_id);

            -- Permissions table
            create table permissions (
                id uuid primary key,
                name text not null,
                category text not null,
                description text null,
                created_at timestamptz not null default now()
            );

            create unique index ix_permissions_name on permissions(name);
            create index ix_permissions_category on permissions(category);

            -- Role Permissions join table
            create table role_permissions (
                role_id uuid not null references roles(id) on delete cascade,
                permission_id uuid not null references permissions(id) on delete cascade,
                assigned_at timestamptz not null default now(),
                primary key (role_id, permission_id)
            );

            create index ix_role_permissions_permission_id on role_permissions(permission_id);

            -- Refresh Tokens table (secure token rotation)
            create table refresh_tokens (
                id uuid primary key,
                user_id uuid not null references platform_users(id) on delete cascade,
                token_hash text not null,
                device_id text not null,
                device_name text null,
                ip_address text null,
                user_agent text null,
                expires_at timestamptz not null,
                created_at timestamptz not null default now(),
                revoked_at timestamptz null,
                revoked_reason text null,
                is_revoked boolean not null default false,
                is_used boolean not null default false,
                used_at timestamptz null,
                replaced_by_token_id text null
            );

            create index ix_refresh_tokens_user_id on refresh_tokens(user_id);
            create index ix_refresh_tokens_token_hash on refresh_tokens(token_hash);
            create index ix_refresh_tokens_expires_at on refresh_tokens(expires_at);
            create index ix_refresh_tokens_is_revoked on refresh_tokens(is_revoked, is_used);

            -- Login Attempts table (security audit - append only)
            create table login_attempts (
                id uuid primary key,
                user_id text null,
                email text null,
                ip_address text not null,
                user_agent text null,
                device_id text null,
                result text not null,
                failure_reason text null,
                mfa_method_used text null,
                mfa_successful boolean null,
                country_code char(2) null,
                city text null,
                is_known_device boolean null,
                is_suspicious boolean null,
                attempted_at timestamptz not null default now()
            );

            create index ix_login_attempts_user_id on login_attempts(user_id);
            create index ix_login_attempts_email on login_attempts(email);
            create index ix_login_attempts_ip_address on login_attempts(ip_address);
            create index ix_login_attempts_attempted_at on login_attempts(attempted_at desc);
            create index ix_login_attempts_result on login_attempts(result);

            -- Device Authorizations table
            create table device_authorizations (
                id uuid primary key,
                user_id uuid not null references platform_users(id) on delete cascade,
                device_id text not null,
                device_name text null,
                device_type text null,
                operating_system text null,
                browser text null,
                public_key text null,
                ip_address text not null,
                user_agent text null,
                country_code char(2) null,
                city text null,
                status text not null default 'Active',
                trust_level text not null default 'Low',
                first_seen_at timestamptz not null default now(),
                last_seen_at timestamptz not null default now(),
                revoked_at timestamptz null,
                revoked_reason text null,
                mfa_verified boolean not null default false,
                mfa_verified_at timestamptz null
            );

            create index ix_device_authorizations_user_id on device_authorizations(user_id);
            create unique index ix_device_authorizations_user_device on device_authorizations(user_id, device_id);
            create index ix_device_authorizations_last_seen_at on device_authorizations(last_seen_at desc);
            create index ix_device_authorizations_status on device_authorizations(status);

            -- Comments for documentation
            comment on table platform_users is 'Core user accounts for authentication and authorization';
            comment on table refresh_tokens is 'Secure refresh tokens for JWT token rotation - tokens are hashed';
            comment on table login_attempts is 'Security audit log for all authentication attempts - append only';
            comment on table device_authorizations is 'Authorized devices per user for device-based security';
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            drop table if exists device_authorizations;
            drop table if exists login_attempts;
            drop table if exists refresh_tokens;
            drop table if exists role_permissions;
            drop table if exists permissions;
            drop table if exists user_roles;
            drop table if exists roles;
            drop table if exists platform_users;
            """);
    }
}
