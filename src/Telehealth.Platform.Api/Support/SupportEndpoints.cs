using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Telehealth.Platform.Domain.Support;
using Telehealth.Platform.Infrastructure.Persistence;

namespace Telehealth.Platform.Api.Support;

/// <summary>
/// Support ticket API endpoints — wired to real DB (support_tickets table).
/// </summary>
public static class SupportEndpoints
{
    public static IEndpointRouteBuilder MapSupportEndpoints(this IEndpointRouteBuilder app)
    {
        var support = app.MapGroup("/support").WithTags("Support and Disputes");

        support.MapGet("/my-tickets", GetMyTicketsAsync).RequireAuthorization();
        support.MapPost("/tickets", CreateTicketAsync).RequireAuthorization();
        support.MapGet("/tickets/{ticketId}", GetTicketAsync).RequireAuthorization();
        support.MapPost("/tickets/{ticketId}/reply", ReplyToTicketAsync).RequireAuthorization();
        support.MapPost("/tickets/{ticketId}/close", CloseTicketAsync).RequireAuthorization();

        support.MapGet("/agent/tickets", GetAllTicketsAsync).RequireAuthorization("RequireSupportAgent");
        support.MapPost("/agent/tickets/{ticketId}/assign", AssignTicketAsync).RequireAuthorization("RequireSupportAgent");
        support.MapPost("/agent/tickets/{ticketId}/resolve", ResolveTicketAsync).RequireAuthorization("RequireSupportAgent");
        support.MapPost("/agent/tickets/{ticketId}/escalate", EscalateTicketAsync).RequireAuthorization("RequireSupportAgent");

        return app;
    }

    private static async Task<IResult> GetMyTicketsAsync(
        ClaimsPrincipal user,
        PlatformDbContext db,
        string? status)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue) return Results.Unauthorized();

        var query = db.SupportTickets.Where(t => t.UserId == userId.Value);

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<TicketStatus>(status, out var parsedStatus))
            query = query.Where(t => t.Status == parsedStatus);

        var tickets = await query
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TicketSummaryDto(
                t.Id, t.TicketNumber, t.Subject, t.Description,
                t.Status, t.Priority, t.CreatedAt, t.AssignedToName))
            .ToListAsync();

        return Results.Ok(new { Items = tickets, TotalCount = tickets.Count });
    }

    private static async Task<IResult> CreateTicketAsync(
        CreateTicketRequestDto request,
        ClaimsPrincipal user,
        PlatformDbContext db)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue) return Results.Unauthorized();

        var userType = user.FindFirst("user_type")?.Value ?? "Patient";
        var now = DateTimeOffset.UtcNow;

        var ticket = new SupportTicket(
            Guid.NewGuid(),
            userId.Value,
            userType,
            request.Category,
            request.Subject,
            request.Description,
            request.Priority,
            request.RelatedConsultationId,
            request.RelatedBillingId,
            now);

        db.SupportTickets.Add(ticket);
        await db.SaveChangesAsync();

        return Results.Created($"/support/tickets/{ticket.Id}", new
        {
            Message = "Ticket created",
            TicketId = ticket.Id,
            TicketNumber = ticket.TicketNumber,
            Status = ticket.Status.ToString()
        });
    }

    private static async Task<IResult> GetTicketAsync(
        Guid ticketId,
        ClaimsPrincipal user,
        PlatformDbContext db)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue) return Results.Unauthorized();

        var ticket = await db.SupportTickets.FindAsync(ticketId);
        if (ticket is null) return Results.NotFound(new { Message = "Ticket not found." });
        if (ticket.UserId != userId.Value) return Results.Forbid();

        var comments = await db.TicketComments
            .Where(c => c.TicketId == ticketId && !c.IsInternal)
            .OrderBy(c => c.CreatedAt)
            .Select(c => new TicketCommentDto(c.Id, c.AuthorId, c.AuthorType, c.AuthorName, c.Content, c.CreatedAt))
            .ToListAsync();

        return Results.Ok(new TicketDetailDto(
            ticket.Id, ticket.TicketNumber, ticket.Subject, ticket.Description,
            ticket.Category, ticket.Status, ticket.Priority,
            ticket.AssignedToName, ticket.Resolution,
            ticket.CreatedAt, ticket.UpdatedAt, comments));
    }

    private static async Task<IResult> ReplyToTicketAsync(
        Guid ticketId,
        ReplyToTicketRequestDto request,
        ClaimsPrincipal user,
        PlatformDbContext db)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue) return Results.Unauthorized();

        var ticket = await db.SupportTickets.FindAsync(ticketId);
        if (ticket is null) return Results.NotFound(new { Message = "Ticket not found." });
        if (ticket.UserId != userId.Value) return Results.Forbid();

        var userType = user.FindFirst("user_type")?.Value ?? "Patient";
        var displayName = user.FindFirst("name")?.Value ?? userId.Value.ToString();
        var now = DateTimeOffset.UtcNow;

        var comment = new TicketComment(
            Guid.NewGuid(), ticketId, userId.Value, userType, displayName,
            request.Message, isInternal: false, null, now);

        db.TicketComments.Add(comment);
        await db.SaveChangesAsync();

        return Results.Ok(new { Message = "Reply added", CommentId = comment.Id });
    }

    private static async Task<IResult> CloseTicketAsync(
        Guid ticketId,
        ClaimsPrincipal user,
        PlatformDbContext db)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue) return Results.Unauthorized();

        var ticket = await db.SupportTickets.FindAsync(ticketId);
        if (ticket is null) return Results.NotFound(new { Message = "Ticket not found." });
        if (ticket.UserId != userId.Value) return Results.Forbid();

        ticket.Close(null, DateTimeOffset.UtcNow);
        await db.SaveChangesAsync();

        return Results.Ok(new { Message = "Ticket closed", TicketId = ticketId });
    }

    // Agent endpoints
    private static async Task<IResult> GetAllTicketsAsync(
        PlatformDbContext db,
        string? status,
        string? priority,
        int page = 1,
        int pageSize = 50)
    {
        var query = db.SupportTickets.AsQueryable();

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<TicketStatus>(status, out var s))
            query = query.Where(t => t.Status == s);
        if (!string.IsNullOrEmpty(priority) && Enum.TryParse<TicketPriority>(priority, out var p))
            query = query.Where(t => t.Priority == p);

        var total = await query.CountAsync();
        var tickets = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TicketSummaryDto(
                t.Id, t.TicketNumber, t.Subject, t.Description,
                t.Status, t.Priority, t.CreatedAt, t.AssignedToName))
            .ToListAsync();

        return Results.Ok(new { Items = tickets, TotalCount = total, Page = page, PageSize = pageSize });
    }

    private static async Task<IResult> AssignTicketAsync(
        Guid ticketId,
        AssignTicketRequestDto request,
        ClaimsPrincipal user,
        PlatformDbContext db)
    {
        var ticket = await db.SupportTickets.FindAsync(ticketId);
        if (ticket is null) return Results.NotFound(new { Message = "Ticket not found." });

        var agentId = GetUserId(user) ?? Guid.Empty;
        var agentName = user.FindFirst("name")?.Value ?? agentId.ToString();
        ticket.Assign(agentId, agentName, DateTimeOffset.UtcNow);
        await db.SaveChangesAsync();

        return Results.Ok(new { Message = "Ticket assigned", TicketId = ticketId, AgentName = agentName });
    }

    private static async Task<IResult> ResolveTicketAsync(
        Guid ticketId,
        ResolveTicketRequestDto request,
        PlatformDbContext db)
    {
        var ticket = await db.SupportTickets.FindAsync(ticketId);
        if (ticket is null) return Results.NotFound(new { Message = "Ticket not found." });

        ticket.Resolve(request.Resolution, DateTimeOffset.UtcNow);
        await db.SaveChangesAsync();

        return Results.Ok(new { Message = "Ticket resolved", TicketId = ticketId });
    }

    private static async Task<IResult> EscalateTicketAsync(
        Guid ticketId,
        EscalateTicketRequestDto request,
        PlatformDbContext db)
    {
        var ticket = await db.SupportTickets.FindAsync(ticketId);
        if (ticket is null) return Results.NotFound(new { Message = "Ticket not found." });

        ticket.Escalate(request.NewPriority, request.Reason, DateTimeOffset.UtcNow);
        await db.SaveChangesAsync();

        return Results.Ok(new { Message = "Ticket escalated", TicketId = ticketId, NewPriority = request.NewPriority.ToString() });
    }

    private static Guid? GetUserId(ClaimsPrincipal user)
    {
        var raw = user.FindFirst("sub")?.Value ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(raw, out var id) ? id : null;
    }
}

// DTOs
public record TicketSummaryDto(
    Guid Id, string TicketNumber, string Subject, string Description,
    TicketStatus Status, TicketPriority Priority,
    DateTimeOffset CreatedAt, string? AssignedTo);

public record TicketDetailDto(
    Guid Id, string TicketNumber, string Subject, string Description,
    string Category, TicketStatus Status, TicketPriority Priority,
    string? AssignedTo, string? Resolution,
    DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt,
    List<TicketCommentDto> Comments);

public record TicketCommentDto(
    Guid Id, Guid AuthorId, string AuthorType, string AuthorName,
    string Content, DateTimeOffset CreatedAt);

public record CreateTicketRequestDto(
    string Category, string Subject, string Description,
    TicketPriority Priority,
    Guid? RelatedConsultationId, Guid? RelatedBillingId);

public record ReplyToTicketRequestDto(string Message);
public record AssignTicketRequestDto(Guid? AgentId);
public record ResolveTicketRequestDto(string Resolution);
public record EscalateTicketRequestDto(TicketPriority NewPriority, string Reason);
