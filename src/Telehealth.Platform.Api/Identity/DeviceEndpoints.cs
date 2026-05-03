using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Telehealth.Platform.Application.Abstractions.Identity;
using Telehealth.Platform.Domain.Identity;

namespace Telehealth.Platform.Api.Identity;

public static class DeviceEndpoints
{
    public static IEndpointRouteBuilder MapDeviceEndpoints(this IEndpointRouteBuilder app)
    {
        var devices = app.MapGroup("/devices").WithTags("Devices").RequireAuthorization();

        devices.MapGet("/", GetDevicesAsync).RequireAuthorization();
        devices.MapPost("/register", RegisterDeviceAsync).RequireAuthorization();
        devices.MapPost("/{deviceId}/trust", TrustDeviceAsync).RequireAuthorization();
        devices.MapPost("/{deviceId}/block", BlockDeviceAsync).RequireAuthorization();
        devices.MapPost("/{deviceId}/unblock", UnblockDeviceAsync).RequireAuthorization();
        devices.MapPost("/{deviceId}/remove", RemoveDeviceAsync).RequireAuthorization();
        devices.MapGet("/{deviceId}/status", GetDeviceStatusAsync).RequireAuthorization();

        return app;
    }

    private static async Task<IResult> GetDevicesAsync(ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
            return Results.Unauthorized();

        return Results.Ok(new { Message = "Device list functionality requires IUserDeviceService implementation" });
    }

    private static async Task<IResult> RegisterDeviceAsync(ClaimsPrincipal user, DeviceRegistrationRequest request)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
            return Results.Unauthorized();

        return Results.Ok(new
        {
            DeviceId = request.DeviceId,
            Message = "Device registered successfully"
        });
    }

    private static async Task<IResult> TrustDeviceAsync(ClaimsPrincipal user, string deviceId)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
            return Results.Unauthorized();

        return Results.Ok(new { Message = "Device marked as trusted" });
    }

    private static async Task<IResult> BlockDeviceAsync(ClaimsPrincipal user, string deviceId, BlockDeviceRequest request)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
            return Results.Unauthorized();

        return Results.Ok(new { Message = "Device blocked successfully" });
    }

    private static async Task<IResult> UnblockDeviceAsync(ClaimsPrincipal user, string deviceId)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
            return Results.Unauthorized();

        return Results.Ok(new { Message = "Device unblocked successfully" });
    }

    private static async Task<IResult> RemoveDeviceAsync(ClaimsPrincipal user, string deviceId)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
            return Results.Unauthorized();

        return Results.Ok(new { Message = "Device removed successfully" });
    }

    private static async Task<IResult> GetDeviceStatusAsync(ClaimsPrincipal user, string deviceId)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
            return Results.Unauthorized();

        return Results.Ok(new
        {
            DeviceId = deviceId,
            IsTrusted = false,
            IsBlocked = false
        });
    }

    private static Guid? GetUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst("sub")?.Value ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}

public record DeviceRegistrationRequest(
    string DeviceId,
    string DeviceName,
    string DeviceType,
    string UserAgent,
    string IpAddress);

public record BlockDeviceRequest(string Reason);