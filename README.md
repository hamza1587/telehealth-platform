# Telehealth Platform API

Professional .NET backend foundation for the custom business services around Medplum.

Medplum remains the clinical/FHIR system of record. This solution owns product-specific workflows such as wallet, credit billing, payments, consultation orchestration, EU compliance workflows, and research export controls.

## Solution Structure

- `Telehealth.Platform.Api` - HTTP API boundary.
- `Telehealth.Platform.Application` - use cases, contracts, orchestration.
- `Telehealth.Platform.Domain` - domain models and business rules.
- `Telehealth.Platform.Infrastructure` - persistence, background jobs, operational services.
- `Telehealth.Platform.Integrations.Medplum` - Medplum/FHIR API client integration.

## Local Commands

```powershell
dotnet restore
dotnet build
dotnet run --project .\src\Telehealth.Platform.Api\Telehealth.Platform.Api.csproj
```

If Windows blocks the compiler server in this environment, use:

```powershell
dotnet build .\Telehealth.Platform.slnx -p:UseSharedCompilation=false
```

## Local URLs

- API: `https://localhost:7271` or `http://localhost:5131`
- OpenAPI document in development: `/openapi/v1.json`
- Liveness: `/health/live`
- Readiness: `/health/ready`
- Platform info: `/platform/info`

## Architecture Rule

Do not put wallet, billing, payments, video-session orchestration, GDPR workflows, or research anonymization inside Medplum. Keep those in this solution and integrate with Medplum only for clinical/FHIR records.
