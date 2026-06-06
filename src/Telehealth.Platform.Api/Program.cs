using Hangfire;
using Microsoft.AspNetCore.RateLimiting;
using Telehealth.Platform.Api.Admin;
using Telehealth.Platform.Api.Analytics;
using Telehealth.Platform.Api.Appointments;
using Telehealth.Platform.Api.Payouts;
using Telehealth.Platform.Api.PMS;
using Telehealth.Platform.Api.Billing;
using Telehealth.Platform.Api.Clinical;
using Telehealth.Platform.Api.Compliance;
using Telehealth.Platform.Api.Consent;
using Telehealth.Platform.Api.Consultations;
using Telehealth.Platform.Api.Discovery;
using Telehealth.Platform.Api.Doctors;
using Telehealth.Platform.Api.Identity;
using Telehealth.Platform.Api.InstantConsultation;
using Telehealth.Platform.Api.Middleware;
using Telehealth.Platform.Api.Notifications;
using Telehealth.Platform.Api.Patients;
using Telehealth.Platform.Api.Prescriptions;
using Telehealth.Platform.Api.Reviews;
using Telehealth.Platform.Api.Research;
using Telehealth.Platform.Api.Support;
using Telehealth.Platform.Api.Wallets;
using Telehealth.Platform.Application;
using Telehealth.Platform.Infrastructure;
using Telehealth.Platform.Infrastructure.BackgroundJobs;
using Telehealth.Platform.Infrastructure.Health;
using Telehealth.Platform.Infrastructure.Identity;
using Telehealth.Platform.Infrastructure.Persistence;
using Telehealth.Platform.Integrations.Medplum;
using Telehealth.Platform.Integrations.Medplum.Health;
using Telehealth.Platform.Shared;

var DocsHtml = ApiDocs.GetHtml("Telehealth Platform API");

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();
// CORS — origins from config; falls back to localhost for development
var allowedOrigins = builder.Configuration
    .GetSection("Security:AllowedOrigins")
    .Get<string[]>() ?? ["http://localhost:5173"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("WebClient", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .WithHeaders("Content-Type", "Authorization", "X-Correlation-ID", "Accept", "Origin")
            .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS")
            .AllowCredentials();
    });
});

builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database", tags: new[] { "ready", "database" })
    .AddCheck<MedplumHealthCheck>("medplum", tags: new[] { "ready", "external" })
    .AddCheck<RedisHealthCheck>("redis", tags: new[] { "ready", "cache" });

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddMedplumIntegration(builder.Configuration);

// Payouts + PMS (stub repos — replace with EF Core implementations in Infrastructure)
builder.Services.AddSingleton<StripeConnectService>();
builder.Services.AddSingleton<IPayoutRepository, InMemoryPayoutRepository>();
builder.Services.AddSingleton<IPmsRepository, InMemoryPmsRepository>();

// Rate limiting — fixed windows: 100 req/min general, 10 req/min for auth routes
var rlSection = builder.Configuration.GetSection("RateLimit");
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("general", limiter =>
    {
        limiter.PermitLimit = rlSection.GetValue("GeneralLimit", 100);
        limiter.Window = TimeSpan.FromSeconds(rlSection.GetValue("WindowSeconds", 60));
        limiter.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
        limiter.QueueLimit = 5;
    });
    options.AddFixedWindowLimiter("auth", limiter =>
    {
        limiter.PermitLimit = rlSection.GetValue("AuthLimit", 10);
        limiter.Window = TimeSpan.FromSeconds(rlSection.GetValue("WindowSeconds", 60));
        limiter.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
        limiter.QueueLimit = 0;
    });
    options.RejectionStatusCode = 429;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi("/openapi/{name}/document.json");
    app.MapGet("/docs", () => Results.Content(DocsHtml, "text/html"));
}

// Seed identity data
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<IdentityDataSeeder>>();
    var seeder = new IdentityDataSeeder(dbContext, logger);
    await seeder.SeedAsync();
}

// Global exception handler must be outermost middleware so it catches all errors
app.UseMiddleware<GlobalExceptionMiddleware>();

// Security headers on every response (before CORS so they cover preflight too)
app.UseMiddleware<SecurityHeadersMiddleware>();

// Input sanitization — reject clearly malicious query-string values early
app.UseMiddleware<InputSanitizationMiddleware>();

// HSTS in production (also set via SecurityHeadersMiddleware but UseHsts adds the ASP.NET defaults)
if (!app.Environment.IsDevelopment())
    app.UseHsts();

app.UseHttpsRedirection();
app.UseCors("WebClient");
app.UseRateLimiter();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ResponseCachingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

// Audit middleware runs after auth so ActorId is available from the JWT claims
app.UseMiddleware<AuditMiddleware>();

// Hangfire dashboard — admin-only in production; open in development
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = app.Environment.IsDevelopment()
        ? [new Hangfire.Dashboard.LocalRequestsOnlyAuthorizationFilter()]
        : [new Hangfire.Dashboard.LocalRequestsOnlyAuthorizationFilter()]
});

// Register recurring background jobs
RecurringJob.AddOrUpdate<AppointmentReminderJob>(
    "appointment-reminders",
    job => job.ExecuteAsync(CancellationToken.None),
    "*/15 * * * *"); // every 15 minutes

RecurringJob.AddOrUpdate<GdprCleanupJob>(
    "gdpr-cleanup",
    job => job.ExecuteAsync(CancellationToken.None),
    Cron.Daily(2)); // 02:00 UTC daily

var platform = app.MapGroup("/platform").WithTags("Platform");

platform.MapGet("/info", () => Results.Ok(new
{
    name = "Telehealth Platform API",
    service = "telehealth-platform-api",
    version = "0.1.0",
    environment = app.Environment.EnvironmentName
}));

var health = app.MapGroup("/health").WithTags("Health");

health.MapGet("/live", () => Results.Ok(new
{
    status = "Healthy",
    checkedAt = DateTimeOffset.UtcNow
}));

health.MapHealthChecks("/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions());

app.MapAuthenticationEndpoints();
app.MapPatientEndpoints();
app.MapDoctorEndpoints();
app.MapDoctorOnboardingEndpoints();
app.MapDoctorProfileEndpoints();
app.MapDiscoveryEndpoints();
app.MapConsentEndpoints();
app.MapAppointmentEndpoints();
app.MapInstantConsultationEndpoints();
app.MapConsultationEndpoints();
app.MapBillingEndpoints();
app.MapWalletEndpoints();
app.MapClinicalNotesEndpoints();
app.MapPrescriptionEndpoints();
app.MapSupportEndpoints();
app.MapNotificationsEndpoints();
app.MapAdminEndpoints();
app.MapGdprEndpoints();
app.MapReviewEndpoints();
app.MapGroup("/api").MapAnalyticsEndpoints();
app.MapGroup("/api").MapTeleconsultationEndpoints();
app.MapResearchExportEndpoints();
app.MapDeviceEndpoints();
app.MapSessionEndpoints();

app.MapControllers();

// Protected endpoint examples using authorization policies
var admin = app.MapGroup("/admin").RequireAuthorization("RequireAdmin").WithTags("Admin");
admin.MapGet("/dashboard", () => Results.Ok(new { Message = "Admin dashboard" }));

var doctor = app.MapGroup("/doctor").RequireAuthorization("RequireDoctor").WithTags("Doctor");
doctor.MapGet("/schedule", () => Results.Ok(new { Message = "Doctor schedule" }));

var patient = app.MapGroup("/patient").RequireAuthorization("RequirePatient").WithTags("Patient");
patient.MapGet("/profile", () => Results.Ok(new { Message = "Patient profile" }));

app.Run();
