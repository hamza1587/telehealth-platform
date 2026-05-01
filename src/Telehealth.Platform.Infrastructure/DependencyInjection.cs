using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Telehealth.Platform.Application.Abstractions.Identity;
using Telehealth.Platform.Application.Abstractions.Patients;
using Telehealth.Platform.Application.Abstractions.Time;
using Telehealth.Platform.Infrastructure.Identity;
using Telehealth.Platform.Infrastructure.Patients;
using Telehealth.Platform.Infrastructure.Persistence;
using Telehealth.Platform.Infrastructure.Time;

namespace Telehealth.Platform.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PlatformDatabase")
            ?? "Host=localhost;Port=5432;Database=telehealth_platform;Username=telehealth;Password=telehealth";

        services.AddDbContext<PlatformDbContext>(options => options.UseNpgsql(connectionString));
        services.AddSingleton<IClock, SystemClock>();

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

        // Register Patient Services
        services.AddScoped<IPatientOnboardingService, PatientOnboardingService>();

        return services;
    }
}
