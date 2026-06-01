using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Telehealth.Platform.Infrastructure.Persistence;

#nullable disable

namespace Telehealth.Platform.Infrastructure.Migrations;

[DbContextAttribute(typeof(PlatformDbContext))]
[Migration("202605010002_AddDoctorOnboardingSchema")]
public partial class AddDoctorOnboardingSchema : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            create table doctor_onboarding_records (
                id uuid primary key,
                doctor_profile_id uuid not null unique references doctor_profiles(id) on delete cascade,
                legal_name text not null,
                email text not null,
                phone_number text not null,
                country_of_practice char(2) not null,
                license_number text not null,
                licensing_authority text not null,
                qualifications text not null,
                years_of_experience integer not null,
                biography text not null,
                insurance_provider text not null,
                insurance_policy_number text not null,
                license_expiry_date date null,
                reviewer_id text null,
                review_notes text null,
                created_at timestamptz not null,
                updated_at timestamptz not null,
                constraint ck_doctor_onboarding_records_years_non_negative check (years_of_experience >= 0)
            );

            alter table doctor_profiles
                add column if not exists created_at timestamptz not null default now();

            alter table doctor_profiles
                add column if not exists updated_at timestamptz not null default now();

            create index ix_doctor_onboarding_records_country_license
                on doctor_onboarding_records(country_of_practice, license_number);
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            drop table if exists doctor_onboarding_records;
            """);
    }
}

