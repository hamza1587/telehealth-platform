# Telehealth Platform Database Design

This document defines the database owned by `telehealth-platform`, the .NET business/product API.

Medplum remains the clinical/FHIR source of truth. This database stores business, financial, operational, compliance workflow, and integration state.

## Source Of Truth Rules

| Data | Source of truth |
|---|---|
| Patient clinical identity and FHIR `Patient` | Medplum |
| Doctor clinical/practitioner identity and FHIR `Practitioner` | Medplum |
| Medical history, allergies, conditions, medications | Medplum |
| Clinical notes, prescriptions, encounters | Medplum |
| Wallet balance, credit ledger, day passes | .NET database |
| Payments, refunds, chargebacks | .NET database |
| Per-second billing and doctor earnings | .NET database |
| Video rooms and consultation runtime state | .NET database/Redis |
| Doctor marketplace profile, pricing, availability projection | .NET database |
| GDPR request workflow orchestration | .NET database |
| Research export jobs and anonymization runs | .NET database |
| Audit logs for product/admin/financial actions | .NET database |

## Database Technology

Recommended primary database: PostgreSQL.

Recommended runtime support:

- PostgreSQL for transactional records.
- Redis for queues, timers, locks, and short-lived session state.
- Append-only audit/event tables for financial and compliance actions.

## Naming Conventions

- Tables use `snake_case`.
- Primary keys use `id`.
- External Medplum references use `medplum_*_id`.
- Money amounts use minor units where possible, for example cents.
- Credit time is stored as seconds.
- Timestamps are stored in UTC as `timestamptz`.
- Ledger tables are append-only.

## Core Tables

## 1. patient_accounts

Business projection of a Medplum patient.

Columns:

- `id uuid primary key`
- `medplum_patient_id text not null unique`
- `display_name text null`
- `email text null`
- `country_code char(2) not null`
- `preferred_language text not null`
- `status text not null`
- `created_at timestamptz not null`
- `updated_at timestamptz not null`

Notes:

- Do not store full clinical profile here.
- Keep only product-facing/account fields needed for wallet, booking, and support.

## 2. doctor_profiles

Marketplace and business projection of a Medplum practitioner.

Columns:

- `id uuid primary key`
- `medplum_practitioner_id text not null unique`
- `display_name text not null`
- `country_code char(2) not null`
- `primary_specialty text not null`
- `verification_status text not null`
- `marketplace_status text not null`
- `default_price_per_second_minor int not null`
- `currency char(3) not null`
- `created_at timestamptz not null`
- `updated_at timestamptz not null`

Notes:

- Clinical qualification documents may live in Medplum as `DocumentReference`; this table stores review/business status.
- Pricing belongs here, not in Medplum.

## 3. doctor_languages

Columns:

- `doctor_profile_id uuid not null`
- `language_code text not null`

Primary key:

- `(doctor_profile_id, language_code)`

## 4. doctor_specialties

Columns:

- `doctor_profile_id uuid not null`
- `specialty_code text not null`
- `is_primary boolean not null`

Primary key:

- `(doctor_profile_id, specialty_code)`

## 5. doctor_availability_windows

Business availability for booking and marketplace search.

Columns:

- `id uuid primary key`
- `doctor_profile_id uuid not null`
- `starts_at timestamptz not null`
- `ends_at timestamptz not null`
- `consultation_mode text not null`
- `is_instant_enabled boolean not null`
- `created_at timestamptz not null`

## 6. wallets

One wallet per patient account.

Columns:

- `id uuid primary key`
- `patient_account_id uuid not null unique`
- `currency char(3) not null`
- `available_seconds bigint not null`
- `reserved_seconds bigint not null`
- `status text not null`
- `created_at timestamptz not null`
- `updated_at timestamptz not null`
- `row_version bigint not null`

Rules:

- `available_seconds` must never be negative.
- `reserved_seconds` must never be negative.
- Updates must use optimistic concurrency.
- Ledger remains the audit source; wallet row is current balance snapshot.

## 7. wallet_ledger_entries

Append-only wallet ledger.

Columns:

- `id uuid primary key`
- `wallet_id uuid not null`
- `entry_type text not null`
- `seconds_delta bigint not null`
- `balance_after_seconds bigint not null`
- `reference_type text not null`
- `reference_id text not null`
- `reason text not null`
- `created_by text not null`
- `created_at timestamptz not null`

Entry types:

- `CreditPurchased`
- `CreditReserved`
- `CreditReservationReleased`
- `ConsultationCharged`
- `RefundGranted`
- `PromotionalCreditGranted`
- `AdminAdjustment`
- `Expiry`

Rules:

- Never update or delete ledger entries.
- Reversals are new entries.

## 8. credit_purchases

Columns:

- `id uuid primary key`
- `patient_account_id uuid not null`
- `wallet_id uuid not null`
- `payment_transaction_id uuid null`
- `bundle_code text not null`
- `seconds_purchased bigint not null`
- `amount_minor bigint not null`
- `currency char(3) not null`
- `status text not null`
- `created_at timestamptz not null`
- `completed_at timestamptz null`

## 9. day_passes

Columns:

- `id uuid primary key`
- `patient_account_id uuid not null`
- `payment_transaction_id uuid null`
- `pass_code text not null`
- `starts_at timestamptz not null`
- `ends_at timestamptz not null`
- `fair_use_seconds bigint not null`
- `used_seconds bigint not null`
- `status text not null`
- `created_at timestamptz not null`

## 10. payment_transactions

Columns:

- `id uuid primary key`
- `patient_account_id uuid not null`
- `provider text not null`
- `provider_transaction_id text not null unique`
- `amount_minor bigint not null`
- `currency char(3) not null`
- `status text not null`
- `failure_code text null`
- `failure_message text null`
- `created_at timestamptz not null`
- `completed_at timestamptz null`

Rules:

- Never store raw card data.
- Store provider IDs/tokens only.

## 11. refunds

Columns:

- `id uuid primary key`
- `payment_transaction_id uuid not null`
- `provider_refund_id text null`
- `amount_minor bigint not null`
- `currency char(3) not null`
- `reason text not null`
- `status text not null`
- `created_at timestamptz not null`
- `completed_at timestamptz null`

## 12. consultation_bookings

Business booking record linked to Medplum Appointment.

Columns:

- `id uuid primary key`
- `patient_account_id uuid not null`
- `doctor_profile_id uuid not null`
- `medplum_appointment_id text null`
- `specialty_code text not null`
- `consultation_mode text not null`
- `booking_type text not null`
- `status text not null`
- `scheduled_starts_at timestamptz null`
- `scheduled_ends_at timestamptz null`
- `price_per_second_minor int not null`
- `currency char(3) not null`
- `reserved_seconds bigint not null`
- `created_at timestamptz not null`
- `updated_at timestamptz not null`

Booking types:

- `Scheduled`
- `Instant`

Statuses:

- `Draft`
- `Confirmed`
- `PatientWaiting`
- `DoctorWaiting`
- `InProgress`
- `Completed`
- `CancelledByPatient`
- `CancelledByDoctor`
- `NoShowPatient`
- `NoShowDoctor`
- `FailedTechnical`
- `Refunded`

## 13. consultation_sessions

Runtime session record linked to a booking and Medplum Encounter.

Columns:

- `id uuid primary key`
- `consultation_booking_id uuid not null unique`
- `medplum_encounter_id text null`
- `video_provider text not null`
- `video_room_id text not null`
- `status text not null`
- `started_at timestamptz null`
- `ended_at timestamptz null`
- `billable_started_at timestamptz null`
- `billable_ended_at timestamptz null`
- `billable_seconds bigint not null`
- `created_at timestamptz not null`
- `updated_at timestamptz not null`

Rules:

- Billing timestamps come from server-side events.
- Medplum Encounter can be created after billing starts or completed asynchronously.

## 14. consultation_participant_events

Append-only join/leave/network event log.

Columns:

- `id uuid primary key`
- `consultation_session_id uuid not null`
- `participant_type text not null`
- `participant_id uuid not null`
- `event_type text not null`
- `occurred_at timestamptz not null`
- `metadata_json jsonb null`

Event types:

- `Joined`
- `Left`
- `Disconnected`
- `Reconnected`
- `DeviceFailure`
- `NetworkFailure`

## 15. billing_sessions

Financial finalization for a consultation.

Columns:

- `id uuid primary key`
- `consultation_session_id uuid not null unique`
- `wallet_id uuid not null`
- `doctor_profile_id uuid not null`
- `billable_seconds bigint not null`
- `price_per_second_minor int not null`
- `gross_amount_minor bigint not null`
- `platform_fee_minor bigint not null`
- `doctor_earning_minor bigint not null`
- `currency char(3) not null`
- `status text not null`
- `finalized_at timestamptz null`
- `created_at timestamptz not null`

Rules:

- Finalization must be idempotent.
- Billing failure must be recoverable by reconciliation jobs.

## 16. doctor_earning_entries

Append-only doctor earnings ledger.

Columns:

- `id uuid primary key`
- `doctor_profile_id uuid not null`
- `billing_session_id uuid null`
- `entry_type text not null`
- `amount_minor bigint not null`
- `currency char(3) not null`
- `status text not null`
- `created_at timestamptz not null`

Entry types:

- `ConsultationEarning`
- `RefundReversal`
- `AdminAdjustment`
- `Payout`

## 17. support_tickets

Columns:

- `id uuid primary key`
- `created_by_account_id uuid not null`
- `assigned_to_user_id uuid null`
- `category text not null`
- `priority text not null`
- `status text not null`
- `subject text not null`
- `related_booking_id uuid null`
- `created_at timestamptz not null`
- `updated_at timestamptz not null`

## 18. gdpr_requests

Columns:

- `id uuid primary key`
- `requester_account_id uuid not null`
- `request_type text not null`
- `status text not null`
- `identity_verification_status text not null`
- `decision text null`
- `decision_reason text null`
- `due_at timestamptz not null`
- `created_at timestamptz not null`
- `completed_at timestamptz null`

Request types:

- `Access`
- `Rectification`
- `Erasure`
- `Restriction`
- `Portability`
- `Objection`
- `ConsentWithdrawal`

## 19. research_export_jobs

Columns:

- `id uuid primary key`
- `requested_by_user_id uuid not null`
- `purpose text not null`
- `legal_basis text not null`
- `status text not null`
- `k_anonymity_threshold int not null`
- `date_shift_days_min int not null`
- `date_shift_days_max int not null`
- `output_location text null`
- `created_at timestamptz not null`
- `approved_at timestamptz null`
- `completed_at timestamptz null`

Rules:

- Export jobs store process metadata only.
- Exported datasets must not be stored in the transactional DB.

## 20. audit_events

Append-only audit log for product/business actions.

Columns:

- `id uuid primary key`
- `actor_id text not null`
- `actor_type text not null`
- `action text not null`
- `target_type text not null`
- `target_id text not null`
- `reason text null`
- `ip_address text null`
- `user_agent text null`
- `metadata_json jsonb null`
- `occurred_at timestamptz not null`

Rules:

- Append-only.
- Sensitive admin actions require a reason.
- Use correlation IDs in metadata.

## Key Relationships

- `patient_accounts` has one `wallet`.
- `patient_accounts` has many `credit_purchases`, `day_passes`, `payment_transactions`, `consultation_bookings`, `gdpr_requests`.
- `doctor_profiles` has many `consultation_bookings`, `billing_sessions`, `doctor_earning_entries`.
- `consultation_bookings` has one `consultation_session`.
- `consultation_sessions` has one `billing_session`.
- `wallets` has many `wallet_ledger_entries`.
- `payment_transactions` has many `refunds`.

## Indexes

Recommended indexes:

- `patient_accounts(medplum_patient_id)`
- `doctor_profiles(medplum_practitioner_id)`
- `doctor_profiles(country_code, primary_specialty, marketplace_status)`
- `doctor_languages(language_code)`
- `doctor_specialties(specialty_code)`
- `wallets(patient_account_id)`
- `wallet_ledger_entries(wallet_id, created_at desc)`
- `consultation_bookings(patient_account_id, created_at desc)`
- `consultation_bookings(doctor_profile_id, scheduled_starts_at)`
- `consultation_bookings(medplum_appointment_id)`
- `consultation_sessions(medplum_encounter_id)`
- `payment_transactions(provider, provider_transaction_id)`
- `gdpr_requests(requester_account_id, created_at desc)`
- `audit_events(target_type, target_id, occurred_at desc)`

## Consistency Rules

### Booking Creation

1. Validate patient account.
2. Validate doctor profile and marketplace status.
3. Validate jurisdiction.
4. Reserve credits in wallet.
5. Create local `consultation_booking`.
6. Create Medplum `Appointment`.
7. Store `medplum_appointment_id`.

If Medplum appointment creation fails, release reserved credits.

### Consultation Start

1. Create or activate `consultation_session`.
2. Start video room.
3. Start server-side billable timer.
4. Create Medplum `Encounter` synchronously or asynchronously.

### Consultation End

1. End billable timer.
2. Calculate charge.
3. Finalize wallet debit.
4. Create `billing_session`.
5. Create doctor earning ledger entry.
6. Close/update Medplum `Encounter`.

### Payment Success

1. Receive idempotent payment webhook.
2. Mark payment transaction completed.
3. Create credit purchase.
4. Append wallet ledger credit.
5. Update wallet balance snapshot.

## Data That Must Not Be Stored Here

- Full clinical notes.
- Prescriptions.
- Allergies.
- Conditions.
- Lab results.
- Medical document contents.
- Raw card numbers.
- Session recordings.
- Research export datasets.

