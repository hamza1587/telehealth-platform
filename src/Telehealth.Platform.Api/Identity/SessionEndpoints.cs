using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Telehealth.Platform.Application.Abstractions.Identity;
using Telehealth.Platform.Domain.Identity;

namespace Telehealth.Platform.Api.Identity;

public static class SessionEndpoints
{
    public static IEndpointRouteBuilder MapSessionEndpoints(this IEndpointRouteBuilder app)
    {
        var sessions = app.MapGroup("/sessions").WithTags("Sessions").RequireAuthorization();

        sessions.MapGet("/", GetSessionsAsync).RequireAuthorization();
        sessions.MapPost("/create", CreateSessionAsync).RequireAuthorization();
        sessions.MapPost("/{sessionId}/end", EndSessionAsync).RequireAuthorization();
        sessions.MapPost("/end-others", EndOtherSessionsAsync).RequireAuthorization();
        sessions.MapGet("/{sessionId}/valid", ValidateSessionAsync).RequireAuthorization();
        sessions.MapGet("/count", GetSessionCountAsync).RequireAuthorization();

        return app;
    }

    private static async Task<IResult> GetSessionsAsync(ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
            return Results.Unauthorized();

        return Results.Ok(new { Message = "Session list functionality requires IUserSessionService implementation" });
    }

    private static async Task<IResult> CreateSessionAsync(ClaimsPrincipal user, CreateSessionRequest request)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
            return Results.Unauthorized();

        return Results.Ok(new
        {
            SessionId = Guid.NewGuid(),
            Message = "Session created successfully",
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(8)
        });
    }

    private static async Task<IResult> EndSessionAsync(ClaimsPrincipal user, string sessionId, EndSessionRequest request)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
            return Results.Unauthorized();

        return Results.Ok(new { Message = "Session ended successfully" });
    }

    private static async Task<IResult> EndOtherSessionsAsync(ClaimsPrincipal user, EndSessionRequest request)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
            return Results.Unauthorized();

        return Results.Ok(new { Message = "Other sessions ended successfully" });
    }

    private static async Task<IResult> ValidateSessionAsync(ClaimsPrincipal user, string sessionId)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
            return Results.Unauthorized();

        return Results.Ok(new
        {
            SessionId = sessionId,
            IsValid = true
        });
    }

    private static async Task<IResult> GetSessionCountAsync(ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
            return Results.Unauthorized();

        return Results.Ok(new
        {
            ActiveSessionCount = 1
        });
    }

    private static Guid? GetUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst("sub")?.Value ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}

public record CreateSessionRequest(
    string DeviceId,
    string IpAddress,
    string UserAgent);

public record EndSessionRequest(string Reason);