using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Telehealth.Platform.Infrastructure.Persistence.Migrations;

public partial class InitialPlatformSchema : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            create table patient_accounts (
                id uuid primary key,
                medplum_patient_id text not null unique,
                display_name text null,
                email text null,
                country_code char(2) not null,
                preferred_language text not null,
                status text not null,
                created_at timestamptz not null,
                updated_at timestamptz not null
            );

            create table doctor_profiles (
                id uuid primary key,
                medplum_practitioner_id text not null unique,
                display_name text not null,
                country_code char(2) not null,
                primary_specialty text not null,
                verification_status text not null,
                marketplace_status text not null,
                default_price_per_second_minor integer not null,
                currency char(3) not null,
                created_at timestamptz not null,
                updated_at timestamptz not null,
                constraint ck_doctor_profiles_default_price_non_negative check (default_price_per_second_minor >= 0)
            );

            create table doctor_languages (
                doctor_profile_id uuid not null references doctor_profiles(id) on delete cascade,
                language_code text not null,
                primary key (doctor_profile_id, language_code)
            );

            create table doctor_specialties (
                doctor_profile_id uuid not null references doctor_profiles(id) on delete cascade,
                specialty_code text not null,
                is_primary boolean not null,
                primary key (doctor_profile_id, specialty_code)
            );

            create table doctor_availability_windows (
                id uuid primary key,
                doctor_profile_id uuid not null references doctor_profiles(id) on delete cascade,
                starts_at timestamptz not null,
                ends_at timestamptz not null,
                consultation_mode text not null,
                is_instant_enabled boolean not null,
                created_at timestamptz not null,
                constraint ck_doctor_availability_windows_time_range check (ends_at > starts_at)
            );

            create table wallets (
                id uuid primary key,
                patient_account_id uuid not null unique references patient_accounts(id) on delete restrict,
                currency char(3) not null,
                available_seconds bigint not null,
                reserved_seconds bigint not null,
                status text not null,
                created_at timestamptz not null,
                updated_at timestamptz not null,
                row_version bigint not null,
                constraint ck_wallets_available_seconds_non_negative check (available_seconds >= 0),
                constraint ck_wallets_reserved_seconds_non_negative check (reserved_seconds >= 0),
                constraint ck_wallets_row_version_positive check (row_version >= 0)
            );

            create table payment_transactions (
                id uuid primary key,
                patient_account_id uuid not null references patient_accounts(id) on delete restrict,
                provider text not null,
                provider_transaction_id text not null unique,
                amount_minor bigint not null,
                currency char(3) not null,
                status text not null,
                failure_code text null,
                failure_message text null,
                created_at timestamptz not null,
                completed_at timestamptz null,
                constraint ck_payment_transactions_amount_non_negative check (amount_minor >= 0)
            );

            create table credit_purchases (
                id uuid primary key,
                patient_account_id uuid not null references patient_accounts(id) on delete restrict,
                wallet_id uuid not null references wallets(id) on delete restrict,
                payment_transaction_id uuid null references payment_transactions(id) on delete restrict,
                bundle_code text not null,
                seconds_purchased bigint not null,
                amount_minor bigint not null,
                currency char(3) not null,
                status text not null,
                created_at timestamptz not null,
                completed_at timestamptz null,
                constraint ck_credit_purchases_seconds_positive check (seconds_purchased > 0),
                constraint ck_credit_purchases_amount_non_negative check (amount_minor >= 0)
            );

            create table day_passes (
                id uuid primary key,
                patient_account_id uuid not null references patient_accounts(id) on delete restrict,
                payment_transaction_id uuid null references payment_transactions(id) on delete restrict,
                pass_code text not null,
                starts_at timestamptz not null,
                ends_at timestamptz not null,
                fair_use_seconds bigint not null,
                used_seconds bigint not null,
                status text not null,
                created_at timestamptz not null,
                constraint ck_day_passes_time_range check (ends_at > starts_at),
                constraint ck_day_passes_fair_use_positive check (fair_use_seconds > 0),
                constraint ck_day_passes_used_seconds_non_negative check (used_seconds >= 0),
                constraint ck_day_passes_used_within_fair_use check (used_seconds <= fair_use_seconds)
            );

            create table refunds (
                id uuid primary key,
                payment_transaction_id uuid not null references payment_transactions(id) on delete restrict,
                provider_refund_id text null,
                amount_minor bigint not null,
                currency char(3) not null,
                reason text not null,
                status text not null,
                created_at timestamptz not null,
                completed_at timestamptz null,
                constraint ck_refunds_amount_positive check (amount_minor > 0)
            );

            create table wallet_ledger_entries (
                id uuid primary key,
                wallet_id uuid not null references wallets(id) on delete restrict,
                entry_type text not null,
                seconds_delta bigint not null,
                balance_after_seconds bigint not null,
                reference_type text not null,
                reference_id text not null,
                reason text not null,
                created_by text not null,
                created_at timestamptz not null,
                constraint ck_wallet_ledger_entries_balance_after_non_negative check (balance_after_seconds >= 0)
            );

            create table consultation_bookings (
                id uuid primary key,
                patient_account_id uuid not null references patient_accounts(id) on delete restrict,
                doctor_profile_id uuid not null references doctor_profiles(id) on delete restrict,
                medplum_appointment_id text null,
                specialty_code text not null,
                consultation_mode text not null,
                booking_type text not null,
                status text not null,
                scheduled_starts_at timestamptz null,
                scheduled_ends_at timestamptz null,
                price_per_second_minor integer not null,
                currency char(3) not null,
                reserved_seconds bigint not null,
                created_at timestamptz not null,
                updated_at timestamptz not null,
                constraint ck_consultation_bookings_price_non_negative check (price_per_second_minor >= 0),
                constraint ck_consultation_bookings_reserved_seconds_non_negative check (reserved_seconds >= 0),
                constraint ck_consultation_bookings_schedule_range check (
                    scheduled_starts_at is null
                    or scheduled_ends_at is null
                    or scheduled_ends_at > scheduled_starts_at
                )
            );

            create table consultation_sessions (
                id uuid primary key,
                consultation_booking_id uuid not null unique references consultation_bookings(id) on delete restrict,
                medplum_encounter_id text null,
                video_provider text not null,
                video_room_id text not null,
                status text not null,
                started_at timestamptz null,
                ended_at timestamptz null,
                billable_started_at timestamptz null,
                billable_ended_at timestamptz null,
                billable_seconds bigint not null,
                created_at timestamptz not null,
                updated_at timestamptz not null,
                constraint ck_consultation_sessions_billable_seconds_non_negative check (billable_seconds >= 0),
                constraint ck_consultation_sessions_time_range check (started_at is null or ended_at is null or ended_at >= started_at),
                constraint ck_consultation_sessions_billable_range check (
                    billable_started_at is null
                    or billable_ended_at is null
                    or billable_ended_at >= billable_started_at
                )
            );

            create table consultation_participant_events (
                id uuid primary key,
                consultation_session_id uuid not null references consultation_sessions(id) on delete cascade,
                participant_type text not null,
                participant_id uuid not null,
                event_type text not null,
                occurred_at timestamptz not null,
                metadata_json jsonb null
            );

            create table billing_sessions (
                id uuid primary key,
                consultation_session_id uuid not null unique references consultation_sessions(id) on delete restrict,
                wallet_id uuid not null references wallets(id) on delete restrict,
                doctor_profile_id uuid not null references doctor_profiles(id) on delete restrict,
                billable_seconds bigint not null,
                price_per_second_minor integer not null,
                gross_amount_minor bigint not null,
                platform_fee_minor bigint not null,
                doctor_earning_minor bigint not null,
                currency char(3) not null,
                status text not null,
                finalized_at timestamptz null,
                created_at timestamptz not null,
                constraint ck_billing_sessions_billable_seconds_non_negative check (billable_seconds >= 0),
                constraint ck_billing_sessions_price_non_negative check (price_per_second_minor >= 0),
                constraint ck_billing_sessions_gross_non_negative check (gross_amount_minor >= 0),
                constraint ck_billing_sessions_platform_fee_non_negative check (platform_fee_minor >= 0),
                constraint ck_billing_sessions_doctor_earning_non_negative check (doctor_earning_minor >= 0),
                constraint ck_billing_sessions_amounts_balance check (gross_amount_minor = platform_fee_minor + doctor_earning_minor)
            );

            create table doctor_earning_entries (
                id uuid primary key,
                doctor_profile_id uuid not null references doctor_profiles(id) on delete restrict,
                billing_session_id uuid null references billing_sessions(id) on delete restrict,
                entry_type text not null,
                amount_minor bigint not null,
                currency char(3) not null,
                status text not null,
                created_at timestamptz not null
            );

            create table support_tickets (
                id uuid primary key,
                created_by_account_id uuid not null references patient_accounts(id) on delete restrict,
                assigned_to_user_id uuid null,
                category text not null,
                priority text not null,
                status text not null,
                subject text not null,
                related_booking_id uuid null references consultation_bookings(id) on delete set null,
                created_at timestamptz not null,
                updated_at timestamptz not null
            );

            create table gdpr_requests (
                id uuid primary key,
                requester_account_id uuid not null references patient_accounts(id) on delete restrict,
                request_type text not null,
                status text not null,
                identity_verification_status text not null,
                decision text null,
                decision_reason text null,
                due_at timestamptz not null,
                created_at timestamptz not null,
                completed_at timestamptz null
            );

            create table research_export_jobs (
                id uuid primary key,
                requested_by_user_id uuid not null,
                purpose text not null,
                legal_basis text not null,
                status text not null,
                k_anonymity_threshold integer not null,
                date_shift_days_min integer not null,
                date_shift_days_max integer not null,
                output_location text null,
                created_at timestamptz not null,
                approved_at timestamptz null,
                completed_at timestamptz null,
                constraint ck_research_export_jobs_k_threshold check (k_anonymity_threshold >= 2),
                constraint ck_research_export_jobs_date_shift_range check (date_shift_days_max >= date_shift_days_min)
            );

            create table audit_events (
                id uuid primary key,
                actor_id text not null,
                actor_type text not null,
                action text not null,
                target_type text not null,
                target_id text not null,
                reason text null,
                ip_address text null,
                user_agent text null,
                metadata_json jsonb null,
                occurred_at timestamptz not null
            );

            create index ix_patient_accounts_medplum_patient_id on patient_accounts(medplum_patient_id);
            create index ix_doctor_profiles_medplum_practitioner_id on doctor_profiles(medplum_practitioner_id);
            create index ix_doctor_profiles_marketplace_search on doctor_profiles(country_code, primary_specialty, marketplace_status);
            create index ix_doctor_languages_language_code on doctor_languages(language_code);
            create index ix_doctor_specialties_specialty_code on doctor_specialties(specialty_code);
            create index ix_doctor_availability_windows_doctor_starts_at on doctor_availability_windows(doctor_profile_id, starts_at);
            create index ix_wallets_patient_account_id on wallets(patient_account_id);
            create index ix_wallet_ledger_entries_wallet_created_at on wallet_ledger_entries(wallet_id, created_at desc);
            create index ix_payment_transactions_patient_created_at on payment_transactions(patient_account_id, created_at desc);
            create index ix_payment_transactions_provider_transaction on payment_transactions(provider, provider_transaction_id);
            create index ix_credit_purchases_patient_created_at on credit_purchases(patient_account_id, created_at desc);
            create index ix_day_passes_patient_status on day_passes(patient_account_id, status, starts_at, ends_at);
            create index ix_consultation_bookings_patient_created_at on consultation_bookings(patient_account_id, created_at desc);
            create index ix_consultation_bookings_doctor_schedule on consultation_bookings(doctor_profile_id, scheduled_starts_at);
            create index ix_consultation_bookings_medplum_appointment_id on consultation_bookings(medplum_appointment_id);
            create index ix_consultation_sessions_medplum_encounter_id on consultation_sessions(medplum_encounter_id);
            create index ix_consultation_participant_events_session_occurred_at on consultation_participant_events(consultation_session_id, occurred_at);
            create index ix_billing_sessions_doctor_created_at on billing_sessions(doctor_profile_id, created_at desc);
            create index ix_doctor_earning_entries_doctor_created_at on doctor_earning_entries(doctor_profile_id, created_at desc);
            create index ix_support_tickets_status_priority on support_tickets(status, priority, created_at);
            create index ix_gdpr_requests_requester_created_at on gdpr_requests(requester_account_id, created_at desc);
            create index ix_gdpr_requests_status_due_at on gdpr_requests(status, due_at);
            create index ix_research_export_jobs_status_created_at on research_export_jobs(status, created_at);
            create index ix_audit_events_target_occurred_at on audit_events(target_type, target_id, occurred_at desc);
            create index ix_audit_events_actor_occurred_at on audit_events(actor_type, actor_id, occurred_at desc);
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            drop table if exists audit_events;
            drop table if exists research_export_jobs;
            drop table if exists gdpr_requests;
            drop table if exists support_tickets;
            drop table if exists doctor_earning_entries;
            drop table if exists billing_sessions;
            drop table if exists consultation_participant_events;
            drop table if exists consultation_sessions;
            drop table if exists consultation_bookings;
            drop table if exists wallet_ledger_entries;
            drop table if exists refunds;
            drop table if exists day_passes;
            drop table if exists credit_purchases;
            drop table if exists payment_transactions;
            drop table if exists wallets;
            drop table if exists doctor_availability_windows;
            drop table if exists doctor_specialties;
            drop table if exists doctor_languages;
            drop table if exists doctor_profiles;
            drop table if exists patient_accounts;
            """);
    }
}
