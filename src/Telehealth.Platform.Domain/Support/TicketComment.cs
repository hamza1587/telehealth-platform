using Telehealth.Platform.Domain.Common;

namespace Telehealth.Platform.Domain.Support;

/// <summary>
/// Comment or reply on a support ticket.
/// </summary>
public sealed class TicketComment : Entity<Guid>
{
    public TicketComment(
        Guid id,
        Guid ticketId,
        Guid authorId,
        string authorType,
        string authorName,
        string content,
        bool isInternal,
        List<string>? attachments,
        DateTimeOffset createdAt)
        : base(id)
    {
        TicketId = ticketId;
        AuthorId = authorId;
        AuthorType = authorType;
        AuthorName = authorName;
        Content = content;
        IsInternal = isInternal;
        Attachments = attachments ?? new List<string>();
        CreatedAt = createdAt;
    }

    public Guid TicketId { get; }
    public Guid AuthorId { get; }
    public string AuthorType { get; }
    public string AuthorName { get; }
    public string Content { get; }
    public bool IsInternal { get; }
    public List<string> Attachments { get; }
    public DateTimeOffset CreatedAt { get; }
}
