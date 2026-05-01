# Migration Notes

The initial migration is intentionally designed but not applied.

Migration file:

- `src/Telehealth.Platform.Infrastructure/Persistence/Migrations/202604290001_InitialPlatformSchema.cs`

Design-time context:

- `src/Telehealth.Platform.Infrastructure/Persistence/PlatformDbContext.cs`
- `src/Telehealth.Platform.Infrastructure/Persistence/PlatformDbContextFactory.cs`

## Do Not Run Yet

Do not run this until the database name, production connection handling, and deployment environment are confirmed:

```powershell
dotnet ef database update
```

## Generate SQL Without Applying

When you want to inspect the SQL script later:

```powershell
dotnet ef migrations script --project .\src\Telehealth.Platform.Infrastructure\Telehealth.Platform.Infrastructure.csproj --context PlatformDbContext --output .\artifacts\sql\initial-platform-schema.sql
```

## Apply Later

When the team intentionally decides to create/update the database:

```powershell
dotnet ef database update --project .\src\Telehealth.Platform.Infrastructure\Telehealth.Platform.Infrastructure.csproj --context PlatformDbContext
```

## Important

The current migration is hand-authored from the database design document. It creates the complete first business schema for the .NET platform database and does not store clinical payloads owned by Medplum.

