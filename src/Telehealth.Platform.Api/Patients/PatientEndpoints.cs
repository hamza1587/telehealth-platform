using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Telehealth.Platform.Domain.Patients;
using Telehealth.Platform.Infrastructure.Persistence;

namespace Telehealth.Platform.Api.Patients;

internal static class PatientEndpoints
{
    public static IEndpointRouteBuilder MapPatientEndpoints(this IEndpointRouteBuilder app)
    {
        var patients = app.MapGroup("/platform/patients").WithTags("Patients");

        patients.MapPost("/register", RegisterPatientAsync);
        patients.MapGet("/{patientId:guid}", GetPatientAsync);

        return app;
    }

    private static async Task<Results<Ok<PatientResponse>, ValidationProblem>> RegisterPatientAsync(
        RegisterPatientRequest request,
        PlatformDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.DisplayName))
        {
            errors["displayName"] = ["Display name is required."];
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            errors["email"] = ["Email is required."];
        }

        if (string.IsNullOrWhiteSpace(request.CountryCode) || request.CountryCode.Trim().Length != 2)
        {
            errors["countryCode"] = ["Country code must be a 2-letter ISO code."];
        }

        if (string.IsNullOrWhiteSpace(request.PreferredLanguage))
        {
            errors["preferredLanguage"] = ["Preferred language is required."];
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var emailTaken = await dbContext.PatientAccounts.AnyAsync(
            patient => patient.Email == normalizedEmail,
            cancellationToken);

        if (emailTaken)
        {
            errors["email"] = ["An account with this email already exists."];
        }

        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors);
        }

        var patient = PatientAccount.Create(
            Guid.NewGuid(),
            request.DisplayName.Trim(),
            normalizedEmail,
            request.CountryCode.Trim().ToUpperInvariant(),
            request.PreferredLanguage.Trim());

        dbContext.PatientAccounts.Add(patient);
        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok(ToResponse(patient));
    }

    private static async Task<Results<Ok<PatientResponse>, NotFound>> GetPatientAsync(
        Guid patientId,
        PlatformDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var patient = await dbContext.PatientAccounts.SingleOrDefaultAsync(
            x => x.Id == patientId,
            cancellationToken);

        if (patient is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(ToResponse(patient));
    }

    private static PatientResponse ToResponse(PatientAccount patient)
    {
        return new PatientResponse(
            patient.Id,
            patient.DisplayName,
            patient.Email,
            patient.CountryCode,
            patient.PreferredLanguage,
            patient.Status.ToString(),
            patient.CreatedAt,
            patient.UpdatedAt);
    }
}

public record RegisterPatientRequest(
    string DisplayName,
    string Email,
    string CountryCode,
    string PreferredLanguage);

public record PatientResponse(
    Guid Id,
    string DisplayName,
    string Email,
    string CountryCode,
    string PreferredLanguage,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);