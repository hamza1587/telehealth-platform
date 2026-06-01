using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Telehealth.Platform.Infrastructure.Persistence;

#nullable disable

namespace Telehealth.Platform.Infrastructure.Migrations;

[DbContextAttribute(typeof(PlatformDbContext))]
[Migration("202605010001_AddPatientOnboardingSchema")]
public partial class AddPatientOnboardingSchema : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            create table patient_consent_records (
                id uuid primary key,
                patient_account_id uuid not null references patient_accounts(id) on delete cascade,
                consent_type text not null,
                version text not null,
                text_snapshot text not null,
                text_hash text not null,
                language text not null,
                legal_basis text not null,
                is_accepted boolean not null,
                ip_address text null,
                user_agent text null,
                captured_at timestamptz not null,
                withdrawn_at timestamptz null
            );

            create table patient_medical_profiles (
                id uuid primary key,
                patient_account_id uuid not null unique references patient_accounts(id) on delete cascade,
                date_of_birth date not null,
                sex_at_birth text not null,
                phone_number text not null,
                country_code char(2) not null,
                city text not null,
                time_zone text not null,
                emergency_contact_name text not null,
                emergency_contact_phone text not null,
                emergency_contact_relationship text not null,
                chief_concern text not null,
                symptoms text not null,
                symptom_duration text not null,
                current_medications text not null,
                allergies text not null,
                known_conditions text not null,
                past_surgeries text not null,
                pregnancy_status text not null,
                lifestyle_factors text not null,
                preferred_consultation_language text not null,
                urgency_level text not null,
                emergency_symptoms boolean not null,
                medical_disclaimer_accepted boolean not null,
                created_at timestamptz not null,
                updated_at timestamptz not null
            );

            create index ix_patient_consent_records_patient_captured_at
                on patient_consent_records(patient_account_id, captured_at desc);

            create index ix_patient_consent_records_patient_type
                on patient_consent_records(patient_account_id, consent_type);
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            drop table if exists patient_medical_profiles;
            drop table if exists patient_consent_records;
            """);
    }
}

