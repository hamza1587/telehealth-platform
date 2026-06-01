using Telehealth.Platform.Api.Admin;
using Telehealth.Platform.Api.Analytics;
using Telehealth.Platform.Api.Appointments;
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
using Telehealth.Platform.Api.Payments;
using Telehealth.Platform.Api.Prescriptions;
using Telehealth.Platform.Api.Reviews;
using Telehealth.Platform.Api.Research;
using Telehealth.Platform.Api.Support;
using Telehealth.Platform.Api.Wallets;
using Telehealth.Platform.Infrastructure.Performance;
using Telehealth.Platform.Application;
using Telehealth.Platform.Infrastructure;
using Telehealth.Platform.Infrastructure.Health;
using Telehealth.Platform.Infrastructure.Identity;
using Telehealth.Platform.Infrastructure.Persistence;
using Telehealth.Platform.Integrations.Medplum;
using Telehealth.Platform.Integrations.Medplum.Health;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("WebClient", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database", tags: new[] { "ready", "database" })
    .AddCheck<MedplumHealthCheck>("medplum", tags: new[] { "ready", "external" })
    .AddCheck<RedisHealthCheck>("redis", tags: new[] { "ready", "cache" });

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddMedplumIntegration(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Seed identity data
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<IdentityDataSeeder>>();
    var seeder = new IdentityDataSeeder(dbContext, logger);
    await seeder.SeedAsync();
}

app.UseHttpsRedirection();
app.UseCors("WebClient");

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ResponseCachingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

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
