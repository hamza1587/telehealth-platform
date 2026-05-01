using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Support;

/// <summary>
/// Support ticket for user issues and disputes.
/// </summary>
public sealed class SupportTicket : Entity<Guid>
{
    public SupportTicket(
        Guid id,
        Guid userId,
        string userType,
        string category,
        string subject,
        string description,
        TicketPriority priority,
        Guid? relatedConsultationId,
        Guid? relatedBillingId,
        DateTimeOffset createdAt)
        : base(id)
    {
        UserId = userId;
        UserType = userType;
        Category = category;
        Subject = subject;
        Description = description;
        Priority = priority;
        Status = TicketStatus.Open;
        RelatedConsultationId = relatedConsultationId;
        RelatedBillingId = relatedBillingId;
        TicketNumber = GenerateTicketNumber();
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public string TicketNumber { get; }
    public Guid UserId { get; }
    public string UserType { get; }
    public string Category { get; }
    public string Subject { get; private set; }
    public string Description { get; private set; }
    public TicketPriority Priority { get; private set; }
    public TicketStatus Status { get; private set; }
    public Guid? AssignedTo { get; private set; }
    public string? AssignedToName { get; private set; }
    public Guid? RelatedConsultationId { get; }
    public Guid? RelatedBillingId { get; }
    public string? Resolution { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? AssignedAt { get; private set; }
    public DateTimeOffset? ResolvedAt { get; private set; }
    public DateTimeOffset? ClosedAt { get; private set; }

    public void Update(string subject, string description, TicketPriority priority, DateTimeOffset updatedAt)
    {
        Subject = subject;
        Description = description;
        Priority = priority;
        UpdatedAt = updatedAt;
    }

    public void Assign(Guid agentId, string agentName, DateTimeOffset assignedAt)
    {
        AssignedTo = agentId;
        AssignedToName = agentName;
        Status = TicketStatus.InProgress;
        AssignedAt = assignedAt;
        UpdatedAt = assignedAt;
    }

    public void Resolve(string resolution, DateTimeOffset resolvedAt)
    {
        Resolution = resolution;
        Status = TicketStatus.Resolved;
        ResolvedAt = resolvedAt;
        UpdatedAt = resolvedAt;
    }

    public void Close(string? reason, DateTimeOffset closedAt)
    {
        Status = TicketStatus.Closed;
        ClosedAt = closedAt;
        UpdatedAt = closedAt;
    }

    public void Reopen(string reason, DateTimeOffset reopenedAt)
    {
        Status = TicketStatus.Reopened;
        Resolution = null;
        ResolvedAt = null;
        ClosedAt = null;
        UpdatedAt = reopenedAt;
    }

    public void Escalate(TicketPriority newPriority, string reason, DateTimeOffset escalatedAt)
    {
        Priority = newPriority;
        UpdatedAt = escalatedAt;
    }

    private static string GenerateTicketNumber()
    {
        return $"TIC-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";
    }
}

public enum TicketStatus
{
    Open,
    InProgress,
    WaitingForUser,
    WaitingForThirdParty,
    Resolved,
    Closed,
    Reopened,
    Escalated
}

public enum TicketPriority
{
    Low,
    Medium,
    High,
    Critical
}

public enum TicketCategory
{
    TechnicalIssue,
    BillingDispute,
    ConsultationIssue,
    AccountAccess,
    RefundRequest,
    GeneralInquiry,
    FeatureRequest,
    Complaint,
    DataPrivacy
}
