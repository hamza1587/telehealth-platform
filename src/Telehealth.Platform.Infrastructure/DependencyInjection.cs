using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Text;
using Telehealth.Platform.Application.Abstractions.AI;
using Telehealth.Platform.Application.Abstractions.Analytics;
using Telehealth.Platform.Application.Abstractions.Consent;
using Telehealth.Platform.Application.Abstractions.Billing;
using Telehealth.Platform.Application.Abstractions.Clinical;
using Telehealth.Platform.Application.Abstractions.Consultations;
using Telehealth.Platform.Application.Abstractions.Doctors;
using Telehealth.Platform.Application.Abstractions.EHR;
using Telehealth.Platform.Application.Abstractions.Identity;
using Telehealth.Platform.Application.Abstractions.Notifications;
using Telehealth.Platform.Application.Abstractions.Patients;
using Telehealth.Platform.Application.Abstractions.Pharmacy;
using Telehealth.Platform.Application.Abstractions.Reviews;
using Telehealth.Platform.Application.Abstractions.Tenancy;
using Telehealth.Platform.Application.Abstractions.Time;
using Telehealth.Platform.Application.Abstractions.Wallets;
using Telehealth.Platform.Infrastructure.Analytics;
using Telehealth.Platform.Infrastructure.AI;
using Telehealth.Platform.Infrastructure.Billing;
using Telehealth.Platform.Infrastructure.Clinical;
using Telehealth.Platform.Infrastructure.Consultations;
using Telehealth.Platform.Infrastructure.Doctors;
using Telehealth.Platform.Infrastructure.EHR;
using Telehealth.Platform.Infrastructure.Health;
using Telehealth.Platform.Infrastructure.Identity;
using Telehealth.Platform.Infrastructure.Patients;
using Telehealth.Platform.Infrastructure.Pharmacy;
using Telehealth.Platform.Infrastructure.Persistence;
using Telehealth.Platform.Infrastructure.Performance;
using Telehealth.Platform.Infrastructure.Reviews;
using Telehealth.Platform.Infrastructure.Tenancy;
using Telehealth.Platform.Infrastructure.Time;
using Telehealth.Platform.Infrastructure.Wallets;
using Telehealth.Platform.Infrastructure.Notifications;

namespace Telehealth.Platform.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PlatformDatabase")
            ?? "Host=localhost;Port=5432;Database=telehealth_platform;Username=telehealth;Password=telehealth";

        services.AddDbContext<PlatformDbContext>(options => options.UseNpgsql(connectionString));
        services.AddSingleton<IClock, SystemClock>();

        services.AddSingleton<DatabaseHealthCheck>();
        services.Configure<RedisOptions>(configuration.GetSection(RedisOptions.SectionName));

        // Register Redis if enabled
        var redisOptions = configuration.GetSection(RedisOptions.SectionName).Get<RedisOptions>();
        if (redisOptions?.Enabled ?? false)
        {
            services.AddSingleton<IConnectionMultiplexer>(sp =>
                ConnectionMultiplexer.Connect(redisOptions.Configuration));
        }

        services.AddScoped<IMfaService, MfaService>();
        services.AddScoped<IDeviceManagementService, DeviceManagementService>();
        services.AddScoped<IConsentService, ConsentService>();

        // Configure JWT settings
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>() ?? new JwtSettings();

        // Add Authentication
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = !string.IsNullOrEmpty(jwtSettings.SecretKey)
                        ? new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
                        : null,
                    ClockSkew = TimeSpan.FromSeconds(jwtSettings.ClockSkewSeconds)
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers.Add("Token-Expired", "true");
                        }
                        return Task.CompletedTask;
                    }
                };
            });

        // Add Authorization
        services.AddAuthorization(options =>
        {
            // Define policies based on user types
            options.AddPolicy("RequirePatient", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim("user_type", "Patient"));

            options.AddPolicy("RequireDoctor", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim("user_type", "Doctor"));

            options.AddPolicy("RequireAdmin", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim("user_type", "Admin"));

            options.AddPolicy("RequireCompliance", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim("user_type", "ComplianceOfficer"));

            // MFA-required policy for sensitive operations
            options.AddPolicy("RequireMfa", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim("mfa_enabled", "True"));

            // Break-glass emergency access policy
            options.AddPolicy("BreakGlassAccess", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireRole("BreakGlass"));
        });

        // Register Identity Services
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IMfaService, MfaService>();
        services.AddScoped<ILoginAttemptLogger, LoginAttemptLogger>();
        services.AddScoped<IUserDeviceService, UserDeviceService>();
        services.AddScoped<IUserSessionService, UserSessionService>();

        // Register Patient Services
        services.AddScoped<IPatientOnboardingService, PatientOnboardingService>();
        services.AddScoped<IPatientAccountService, PatientAccountService>();

        // Register Doctor Services
        services.AddScoped<IDoctorProfileService, DoctorProfileService>();

        // Register Consultation Services
        services.AddScoped<IVideoRoomService, VideoRoomService>();
        services.AddScoped<ITeleconsultationService, TeleconsultationService>();
        services.AddScoped<IVideoIntegrationService, JitsiVideoIntegrationService>();

        // Register IOptions for Jitsi configuration
        services.Configure<JitsiOptions>(configuration.GetSection(JitsiOptions.SectionName));

        // Register Clinical Services
        services.AddScoped<IClinicalRecordService, ClinicalRecordService>();

        // Register Billing Services
        services.AddScoped<IInsuranceProviderService, InsuranceProviderService>();
        services.AddScoped<IPatientInsuranceService, PatientInsuranceService>();

        // Register Analytics Services
        services.AddScoped<IAnalyticsService, AnalyticsService>();

        // Register Wallet Services
        services.AddScoped<IWalletService, WalletService>();
        services.AddScoped<IPaymentService, PaymentService>();

        // Register Review Services
        services.AddScoped<IReviewService, ReviewService>();

        // Register AI Services
        services.AddScoped<IDiagnosticService, DiagnosticService>();

        // Register EHR Services
        services.AddScoped<IFhirService, FhirService>();

        // Register Pharmacy Services
        services.AddScoped<IDrugInteractionService, DrugInteractionService>();

        // Register Tenancy Services
        services.AddScoped<ITenantService, TenantService>();

        // Register Notification Services
        services.AddScoped<INotificationService, NotificationService>();

        // Register Caching Services
        services.AddSingleton<ICachingService, CachingService>();

        // Configure Session Options
        services.Configure<SessionOptions>(options =>
        {
            options.DefaultTtl = TimeSpan.FromHours(8);
            options.MaxConcurrentSessions = 5;
            options.InactivityTimeout = TimeSpan.FromHours(2);
        });

        return services;
    }
}
