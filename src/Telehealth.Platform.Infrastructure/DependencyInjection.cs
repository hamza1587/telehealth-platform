using Hangfire;
using Hangfire.PostgreSql;
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
using Telehealth.Platform.Application.Abstractions.Payments;
using Telehealth.Platform.Infrastructure.Analytics;
using Telehealth.Platform.Infrastructure.AI;
using Telehealth.Platform.Infrastructure.BackgroundJobs;
using Telehealth.Platform.Infrastructure.Billing;
using Telehealth.Platform.Infrastructure.Clinical;
using Telehealth.Platform.Infrastructure.Consultations;
using Telehealth.Platform.Infrastructure.Doctors;
using Telehealth.Platform.Infrastructure.EHR;
using Telehealth.Platform.Infrastructure.Health;
using Telehealth.Platform.Infrastructure.Identity;
using Telehealth.Platform.Infrastructure.Notifications;
using Telehealth.Platform.Infrastructure.Patients;
using Telehealth.Platform.Infrastructure.Pharmacy;
using Telehealth.Platform.Infrastructure.Persistence;
using Telehealth.Platform.Infrastructure.Performance;
using Telehealth.Platform.Infrastructure.Reviews;
using Telehealth.Platform.Infrastructure.Tenancy;
using Telehealth.Platform.Infrastructure.Time;
using Telehealth.Platform.Infrastructure.Wallets;
using Telehealth.Platform.Infrastructure.Payments;

namespace Telehealth.Platform.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PlatformDatabase")
            ?? "Host=localhost;Port=5432;Database=telehealth_platform;Username=telehealth;Password=telehealth";

        services.AddDbContext<PlatformDbContext>(options => options.UseNpgsql(connectionString));
        services.AddSingleton<IClock, SystemClock>();

        services.AddDistributedMemoryCache();

        services.AddScoped<DatabaseHealthCheck>();
        services.Configure<RedisOptions>(configuration.GetSection(RedisOptions.SectionName));

        var redisOptions = configuration.GetSection(RedisOptions.SectionName).Get<RedisOptions>();
        if (redisOptions?.Enabled ?? false)
        {
            services.AddSingleton<IConnectionMultiplexer>(sp =>
                ConnectionMultiplexer.Connect(redisOptions.Configuration));
        }

        services.AddScoped<IMfaService, MfaService>();
        services.AddScoped<IDeviceManagementService, DeviceManagementService>();
        services.AddScoped<IConsentService, Identity.ConsentService>();

        // JWT / Authentication
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>() ?? new JwtSettings();

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
                            context.Response.Headers["Token-Expired"] = "true";
                        return Task.CompletedTask;
                    }
                };
            });

        // Authorization policies
        services.AddAuthorization(options =>
        {
            options.AddPolicy("RequirePatient", policy =>
                policy.RequireAuthenticatedUser().RequireClaim("user_type", "Patient"));

            options.AddPolicy("RequireDoctor", policy =>
                policy.RequireAuthenticatedUser().RequireClaim("user_type", "Doctor"));

            options.AddPolicy("RequireAdmin", policy =>
                policy.RequireAuthenticatedUser().RequireClaim("user_type", "Admin"));

            options.AddPolicy("RequireCompliance", policy =>
                policy.RequireAuthenticatedUser().RequireClaim("user_type", "ComplianceOfficer"));

            options.AddPolicy("RequireSupportAgent", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim("user_type", "SupportAgent", "Admin"));

            options.AddPolicy("RequireMfa", policy =>
                policy.RequireAuthenticatedUser().RequireClaim("mfa_enabled", "True"));

            options.AddPolicy("BreakGlassAccess", policy =>
                policy.RequireAuthenticatedUser().RequireRole("BreakGlass"));
        });

        // Identity Services
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IMfaService, MfaService>();
        services.AddScoped<ILoginAttemptLogger, LoginAttemptLogger>();
        services.AddScoped<IUserDeviceService, UserDeviceService>();
        services.AddScoped<IUserSessionService, UserSessionService>();

        // Patient & Doctor Services
        services.AddScoped<IPatientOnboardingService, PatientOnboardingService>();
        services.AddScoped<IPatientAccountService, PatientAccountService>();
        services.AddScoped<IDoctorProfileService, DoctorProfileService>();

        // Consultation Services
        services.AddScoped<IVideoRoomService, VideoRoomService>();
        services.AddScoped<ITeleconsultationService, TeleconsultationService>();
        services.AddScoped<IVideoIntegrationService, JitsiVideoIntegrationService>();
        services.Configure<JitsiOptions>(configuration.GetSection(JitsiOptions.SectionName));

        // Clinical, Billing, Analytics, Wallet
        services.AddScoped<IClinicalRecordService, ClinicalRecordService>();
        services.AddScoped<IInsuranceProviderService, InsuranceProviderService>();
        services.AddScoped<IPatientInsuranceService, PatientInsuranceService>();
        services.AddScoped<IAnalyticsService, AnalyticsService>();
        services.AddScoped<IWalletService, WalletService>();

        // Stripe: use AddHttpClient so StripePaymentGateway receives a managed HttpClient
        services.AddHttpClient<StripePaymentGateway>(client =>
        {
            client.BaseAddress = new Uri("https://api.stripe.com/");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });
        services.AddScoped<Application.Abstractions.Payments.IPaymentGateway>(
            sp => sp.GetRequiredService<StripePaymentGateway>());
        services.AddScoped<Application.Abstractions.Payments.ITokenizationService, Payments.TokenizationService>();
        services.AddScoped<Application.Abstractions.Payments.IPciComplianceService, Payments.PciComplianceService>();
        services.Configure<Payments.StripePaymentGatewayOptions>(configuration.GetSection("PaymentGateways:Stripe"));
        services.Configure<Payments.TokenizationServiceOptions>(configuration.GetSection("Tokenization"));
        services.Configure<Payments.PciComplianceServiceOptions>(configuration.GetSection("PciCompliance"));

        // Review, AI, EHR, Pharmacy, Tenancy
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<IDiagnosticService, DiagnosticService>();
        services.AddScoped<IFhirService, FhirService>();
        services.AddScoped<IDrugInteractionService, DrugInteractionService>();
        services.AddScoped<ITenantService, TenantService>();

        // Notifications (in-app) + Email sender (SMTP)
        services.AddScoped<INotificationService, NotificationService>();
        services.Configure<SmtpEmailSenderOptions>(configuration.GetSection("Email"));
        services.AddScoped<IEmailSender, SmtpEmailSender>();

        // Caching
        services.AddScoped<ICachingService, CachingService>();

        // Session options
        services.Configure<SessionOptions>(options =>
        {
            options.DefaultTtl = TimeSpan.FromHours(8);
            options.MaxConcurrentSessions = 5;
            options.InactivityTimeout = TimeSpan.FromHours(2);
        });

        // Hangfire — background job processing via PostgreSQL
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(c => c.UseNpgsqlConnection(connectionString)));
        services.AddHangfireServer();

        // Background job classes — transient so Hangfire creates a fresh instance per invocation
        services.AddTransient<AppointmentReminderJob>();
        services.AddTransient<GdprCleanupJob>();

        return services;
    }
}
