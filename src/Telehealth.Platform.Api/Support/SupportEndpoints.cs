using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Telehealth.Platform.Domain.Support;

namespace Telehealth.Platform.Api.Support;

/// <summary>
/// Support and disputes API endpoints.
/// </summary>
public static class SupportEndpoints
{
    public static IEndpointRouteBuilder MapSupportEndpoints(this IEndpointRouteBuilder app)
    {
        var support = app.MapGroup("/support").WithTags("Support and Disputes");

        // User endpoints
        support.MapGet("/my-tickets", GetMyTicketsAsync).RequireAuthorization();
        support.MapPost("/tickets", CreateTicketAsync).RequireAuthorization();
        support.MapGet("/tickets/{ticketId}", GetTicketAsync).RequireAuthorization();
        support.MapPost("/tickets/{ticketId}/reply", ReplyToTicketAsync).RequireAuthorization();
        support.MapPost("/tickets/{ticketId}/close", CloseTicketAsync).RequireAuthorization();

        // Support agent endpoints
        support.MapGet("/agent/tickets", GetAllTicketsAsync).RequireAuthorization("RequireSupportAgent");
        support.MapPost("/agent/tickets/{ticketId}/assign", AssignTicketAsync).RequireAuthorization("RequireSupportAgent");
        support.MapPost("/agent/tickets/{ticketId}/resolve", ResolveTicketAsync).RequireAuthorization("RequireSupportAgent");
        support.MapPost("/agent/tickets/{ticketId}/escalate", EscalateTicketAsync).RequireAuthorization("RequireSupportAgent");

        return app;
    }

    private static async Task<IResult> GetMyTicketsAsync(
        ClaimsPrincipal user,
        string? status)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var tickets = new List<TicketSummaryDto>
        {
            new(
                Guid.NewGuid(),
                "TIC-20240115-A1B2C3",
                "Billing Dispute",
                "Incorrect charge on my last consultation",
                TicketStatus.InProgress,
                TicketPriority.High,
                DateTimeOffset.UtcNow.AddDays(-3),
                "Support Agent")
        };

        return Results.Ok(new { Items = tickets, TotalCount = tickets.Count });
    }

    private static async Task<IResult> CreateTicketAsync(
        CreateTicketRequestDto request,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var ticket = new TicketDetailDto(
            Guid.NewGuid(),
            "TIC-20240115-" + Guid.NewGuid().ToString()[..6].ToUpper(),
            request.Category,
            request.Subject,
            request.Description,
            TicketStatus.Open,
            request.Priority,
            null,
            null,
            request.RelatedConsultationId,
            request.RelatedBillingId,
            DateTimeOffset.UtcNow,
            new List<TicketCommentDto>());

        return Results.Created($"/support/tickets/{ticket.Id}", ticket);
    }

    private static async Task<IResult> GetTicketAsync(
        Guid ticketId,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var ticket = new TicketDetailDto(
            ticketId,
            "TIC-20240115-A1B2C3",
            "Billing Dispute",
            "Incorrect charge on my last consultation",
            "I was charged 50 EUR for a consultation that only lasted 5 minutes instead of 30.",
            TicketStatus.InProgress,
            TicketPriority.High,
            Guid.NewGuid(),
            "Support Agent Sarah",
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddDays(-3),
            new List<TicketCommentDto>
            {
                new(
                    Guid.NewGuid(),
                    "Patient",
                    "I was charged 50 EUR for a consultation that only lasted 5 minutes instead of 30.",
                    false,
                    DateTimeOffset.UtcNow.AddDays(-3)),
                new(
                    Guid.NewGuid(),
                    "Support Agent",
                    "Thank you for reporting this. I'm investigating your billing issue and will get back to you within 24 hours.",
                    false,
                    DateTimeOffset.UtcNow.AddDays(-2))
            });

        return Results.Ok(ticket);
    }

    private static async Task<IResult> ReplyToTicketAsync(
        Guid ticketId,
        ReplyToTicketRequestDto request,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(new
        {
            TicketId = ticketId,
            Message = "Reply added successfully",
            RepliedAt = DateTimeOffset.UtcNow
        });
    }

    private static async Task<IResult> CloseTicketAsync(
        Guid ticketId,
        CloseTicketRequestDto request,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(new
        {
            TicketId = ticketId,
            Status = TicketStatus.Closed,
            Reason = request.Reason,
            ClosedAt = DateTimeOffset.UtcNow
        });
    }

    private static async Task<IResult> GetAllTicketsAsync(
        string? status,
        string? priority,
        Guid? assignedTo,
        int? page,
        int? pageSize)
    {
        var tickets = new List<AgentTicketDto>
        {
            new(
                Guid.NewGuid(),
                "TIC-20240115-A1B2C3",
                "john.doe@example.com",
                "Billing Dispute",
                "Incorrect charge",
                TicketStatus.Open,
                TicketPriority.High,
                null,
                DateTimeOffset.UtcNow.AddHours(-2))
        };

        return Results.Ok(new { Items = tickets, TotalCount = tickets.Count });
    }

    private static async Task<IResult> AssignTicketAsync(
        Guid ticketId,
        AssignTicketRequestDto request)
    {
        return Results.Ok(new
        {
            TicketId = ticketId,
            AssignedTo = request.AgentId,
            AssignedToName = request.AgentName,
            AssignedAt = DateTimeOffset.UtcNow,
            Status = TicketStatus.InProgress
        });
    }

    private static async Task<IResult> ResolveTicketAsync(
        Guid ticketId,
        ResolveTicketRequestDto request)
    {
        return Results.Ok(new
        {
            TicketId = ticketId,
            Status = TicketStatus.Resolved,
            Resolution = request.Resolution,
            RefundAmount = request.RefundAmount,
            ResolvedAt = DateTimeOffset.UtcNow
        });
    }

    private static async Task<IResult> EscalateTicketAsync(
        Guid ticketId,
        EscalateTicketRequestDto request)
    {
        return Results.Ok(new
        {
            TicketId = ticketId,
            NewPriority = request.NewPriority,
            Reason = request.Reason,
            EscalatedAt = DateTimeOffset.UtcNow
        });
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
public record TicketSummaryDto(
    Guid Id,
    string TicketNumber,
    string Category,
    string Subject,
    TicketStatus Status,
    TicketPriority Priority,
    DateTimeOffset CreatedAt,
    string? AssignedTo);

public record TicketDetailDto(
    Guid Id,
    string TicketNumber,
    string Category,
    string Subject,
    string Description,
    TicketStatus Status,
    TicketPriority Priority,
    Guid? AssignedTo,
    string? AssignedToName,
    Guid? RelatedConsultationId,
    Guid? RelatedBillingId,
    DateTimeOffset CreatedAt,
    List<TicketCommentDto> Comments);

public record TicketCommentDto(
    Guid Id,
    string AuthorType,
    string Content,
    bool IsInternal,
    DateTimeOffset CreatedAt);

public record CreateTicketRequestDto(
    string Category,
    string Subject,
    string Description,
    TicketPriority Priority,
    Guid? RelatedConsultationId,
    Guid? RelatedBillingId);

public record ReplyToTicketRequestDto(string Content, List<string>? Attachments);

public record CloseTicketRequestDto(string? Reason);

public record AgentTicketDto(
    Guid Id,
    string TicketNumber,
    string UserEmail,
    string Category,
    string Subject,
    TicketStatus Status,
    TicketPriority Priority,
    Guid? AssignedTo,
    DateTimeOffset CreatedAt);

public record AssignTicketRequestDto(Guid AgentId, string AgentName);

public record ResolveTicketRequestDto(
    string Resolution,
    decimal? RefundAmount,
    string? RefundReason);

public record EscalateTicketRequestDto(TicketPriority NewPriority, string Reason);
