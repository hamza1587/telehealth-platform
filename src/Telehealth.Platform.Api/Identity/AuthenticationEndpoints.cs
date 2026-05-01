using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Telehealth.Platform.Application.Abstractions.Identity;
using Telehealth.Platform.Domain.Identity;

namespace Telehealth.Platform.Api.Identity;

/// <summary>
/// Authentication and authorization API endpoints.
/// </summary>
public static class AuthenticationEndpoints
{
    public static IEndpointRouteBuilder MapAuthenticationEndpoints(this IEndpointRouteBuilder app)
    {
        var auth = app.MapGroup("/auth").WithTags("Authentication");

        // Public endpoints
        auth.MapPost("/register", RegisterAsync);
        auth.MapPost("/login", LoginAsync);
        auth.MapPost("/refresh", RefreshTokenAsync);
        auth.MapPost("/verify-email", VerifyEmailAsync);
        auth.MapPost("/resend-verification", ResendVerificationAsync);
        auth.MapPost("/forgot-password", ForgotPasswordAsync);
        auth.MapPost("/reset-password", ResetPasswordAsync);

        // MFA endpoints
        auth.MapPost("/mfa/verify", VerifyMfaAsync);
        auth.MapPost("/mfa/setup", SetupMfaAsync).RequireAuthorization();
        auth.MapPost("/mfa/confirm", ConfirmMfaSetupAsync).RequireAuthorization();
        auth.MapPost("/mfa/disable", DisableMfaAsync).RequireAuthorization();
        auth.MapPost("/mfa/recovery-codes", GenerateRecoveryCodesAsync).RequireAuthorization();

        // Phone verification
        auth.MapPost("/verify-phone", VerifyPhoneAsync).RequireAuthorization();

        // Protected endpoints
        auth.MapPost("/logout", LogoutAsync).RequireAuthorization();
        auth.MapPost("/change-password", ChangePasswordAsync).RequireAuthorization();
        auth.MapGet("/me", GetCurrentUserAsync).RequireAuthorization();
        auth.MapPost("/revoke-token", RevokeTokenAsync).RequireAuthorization();

        return app;
    }

    private static async Task<IResult> RegisterAsync(
        RegisterRequestDto request,
        IAuthenticationService authService,
        CancellationToken cancellationToken)
    {
        var result = await authService.RegisterAsync(new RegisterRequest(
            request.Email,
            request.Password,
            request.UserType,
            request.FirstName,
            request.LastName,
            request.PhoneNumber,
            request.CountryCode,
            request.PreferredLanguage), cancellationToken);

        if (!result.Success)
        {
            return Results.BadRequest(new { Error = result.Error, ErrorCode = result.ErrorCode });
        }

        return Results.Ok(new
        {
            result.UserId,
            result.Email,
            result.UserType,
            result.Roles,
            result.EmailConfirmed,
            Message = "Registration successful. Please verify your email."
        });
    }

    private static async Task<IResult> LoginAsync(
        LoginRequestDto request,
        IAuthenticationService authService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = httpContext.Request.Headers.UserAgent.ToString();

        var result = await authService.LoginAsync(new LoginRequest(
            request.Email,
            request.Password,
            request.DeviceId,
            request.DeviceName,
            userAgent,
            ipAddress), cancellationToken);

        if (!result.Success)
        {
            return Results.Unauthorized();
        }

        if (result.MfaRequired)
        {
            return Results.Ok(new
            {
                MfaRequired = true,
                result.MfaToken,
                Message = "MFA verification required"
            });
        }

        return Results.Ok(new
        {
            result.UserId,
            result.Email,
            result.UserType,
            result.Roles,
            result.AccessToken,
            result.RefreshToken,
            result.ExpiresAt,
            result.EmailConfirmed,
            result.PhoneConfirmed
        });
    }

    private static async Task<IResult> VerifyMfaAsync(
        MfaVerificationRequestDto request,
        IAuthenticationService authService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = httpContext.Request.Headers.UserAgent.ToString();

        var result = await authService.VerifyMfaAndCompleteLoginAsync(new MfaVerificationRequest(
            request.MfaToken,
            request.Code,
            request.DeviceId,
            request.DeviceName,
            userAgent,
            ipAddress), cancellationToken);

        if (!result.Success)
        {
            return Results.BadRequest(new { Error = result.Error, ErrorCode = result.ErrorCode });
        }

        return Results.Ok(new
        {
            result.UserId,
            result.Email,
            result.UserType,
            result.Roles,
            result.AccessToken,
            result.RefreshToken,
            result.ExpiresAt
        });
    }

    private static async Task<IResult> RefreshTokenAsync(
        RefreshTokenRequestDto request,
        IAuthenticationService authService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        var result = await authService.RefreshTokenAsync(new RefreshTokenRequest(
            request.RefreshToken,
            request.DeviceId,
            ipAddress), cancellationToken);

        if (!result.Success)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(new
        {
            result.AccessToken,
            result.RefreshToken,
            result.ExpiresAt
        });
    }

    private static async Task<IResult> LogoutAsync(
        IAuthenticationService authService,
        ClaimsPrincipal user,
        LogoutRequestDto? request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        await authService.LogoutAsync(userId.Value, request?.DeviceId, cancellationToken);
        return Results.Ok(new { Message = "Logged out successfully" });
    }

    private static async Task<IResult> GetCurrentUserAsync(
        IAuthenticationService authService,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var result = await authService.GetCurrentUserAsync(userId.Value, cancellationToken);
        if (result == null)
        {
            return Results.NotFound();
        }

        return Results.Ok(result);
    }

    private static async Task<IResult> ChangePasswordAsync(
        ChangePasswordRequestDto request,
        IAuthenticationService authService,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        try
        {
            await authService.ChangePasswordAsync(userId.Value, new ChangePasswordRequest(
                request.CurrentPassword,
                request.NewPassword), cancellationToken);

            return Results.Ok(new { Message = "Password changed successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { Error = ex.Message });
        }
    }

    private static async Task<IResult> ForgotPasswordAsync(
        ForgotPasswordRequestDto request,
        IAuthenticationService authService,
        CancellationToken cancellationToken)
    {
        var result = await authService.RequestPasswordResetAsync(request.Email, cancellationToken);
        // Always return success to prevent email enumeration
        return Results.Ok(new { Message = "If the email exists, a password reset link has been sent." });
    }

    private static async Task<IResult> ResetPasswordAsync(
        ResetPasswordRequestDto request,
        IAuthenticationService authService,
        CancellationToken cancellationToken)
    {
        var result = await authService.ResetPasswordAsync(new ResetPasswordRequest(
            request.Token,
            request.NewPassword), cancellationToken);

        if (!result.Success)
        {
            return Results.BadRequest(new { Error = result.Error });
        }

        return Results.Ok(new { Message = result.Message });
    }

    private static async Task<IResult> VerifyEmailAsync(
        VerifyEmailRequestDto request,
        IAuthenticationService authService,
        CancellationToken cancellationToken)
    {
        var result = await authService.VerifyEmailAsync(request.Token, cancellationToken);
        if (!result)
        {
            return Results.BadRequest(new { Error = "Invalid or expired verification token" });
        }

        return Results.Ok(new { Message = "Email verified successfully" });
    }

    private static async Task<IResult> ResendVerificationAsync(
        ResendVerificationRequestDto request,
        IAuthenticationService authService,
        CancellationToken cancellationToken)
    {
        await authService.ResendVerificationEmailAsync(request.Email, cancellationToken);
        return Results.Ok(new { Message = "If the email exists and is not verified, a new verification email has been sent." });
    }

    private static async Task<IResult> VerifyPhoneAsync(
        VerifyPhoneRequestDto request,
        IAuthenticationService authService,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var result = await authService.VerifyPhoneAsync(userId.Value, request.Code, cancellationToken);
        if (!result)
        {
            return Results.BadRequest(new { Error = "Invalid or expired verification code" });
        }

        return Results.Ok(new { Message = "Phone number verified successfully" });
    }

    private static async Task<IResult> SetupMfaAsync(
        IAuthenticationService authService,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var result = await authService.SetupMfaAsync(userId.Value, cancellationToken);
        if (!result.Success)
        {
            return Results.BadRequest(new { Error = result.Error });
        }

        return Results.Ok(new
        {
            result.SecretKey,
            result.QrCodeUri,
            result.BackupCodes,
            Message = "Scan the QR code with your authenticator app and verify to complete setup."
        });
    }

    private static async Task<IResult> ConfirmMfaSetupAsync(
        ConfirmMfaSetupRequestDto request,
        IAuthenticationService authService,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var result = await authService.VerifyMfaSetupAsync(userId.Value, request.Code, cancellationToken);
        if (!result)
        {
            return Results.BadRequest(new { Error = "Invalid verification code" });
        }

        return Results.Ok(new { Message = "MFA enabled successfully" });
    }

    private static async Task<IResult> DisableMfaAsync(
        DisableMfaRequestDto request,
        IAuthenticationService authService,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        try
        {
            await authService.DisableMfaAsync(userId.Value, request.Password, cancellationToken);
            return Results.Ok(new { Message = "MFA disabled successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { Error = ex.Message });
        }
    }

    private static async Task<IResult> GenerateRecoveryCodesAsync(
        IAuthenticationService authService,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var codes = await authService.GenerateMfaRecoveryCodesAsync(userId.Value, cancellationToken);
        return Results.Ok(new { RecoveryCodes = codes });
    }

    private static async Task<IResult> RevokeTokenAsync(
        RevokeTokenRequestDto request,
        IAuthenticationService authService,
        CancellationToken cancellationToken)
    {
        await authService.RevokeTokenAsync(request.RefreshToken, cancellationToken);
        return Results.Ok(new { Message = "Token revoked successfully" });
    }

    private static Guid? GetUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user.FindFirst("sub")?.Value;

        if (Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }

        return null;
    }
}

// DTOs
public record RegisterRequestDto(
    string Email,
    string Password,
    UserType UserType,
    string? FirstName = null,
    string? LastName = null,
    string? PhoneNumber = null,
    string? CountryCode = null,
    string? PreferredLanguage = null);

public record LoginRequestDto(
    string Email,
    string Password,
    string? DeviceId = null,
    string? DeviceName = null);

public record MfaVerificationRequestDto(
    string MfaToken,
    string Code,
    string? DeviceId = null,
    string? DeviceName = null);

public record RefreshTokenRequestDto(
    string RefreshToken,
    string? DeviceId = null);

public record LogoutRequestDto(string? DeviceId = null);

public record ChangePasswordRequestDto(
    string CurrentPassword,
    string NewPassword);

public record ForgotPasswordRequestDto(string Email);

public record ResetPasswordRequestDto(
    string Token,
    string NewPassword);

public record VerifyEmailRequestDto(string Token);

public record ResendVerificationRequestDto(string Email);

public record VerifyPhoneRequestDto(string Code);

public record ConfirmMfaSetupRequestDto(string Code);

public record DisableMfaRequestDto(string Password);

public record RevokeTokenRequestDto(string RefreshToken);
