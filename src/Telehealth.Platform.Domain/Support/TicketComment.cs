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

    public Guid TicketId { get; private set; }
    public Guid AuthorId { get; private set; }
    public string AuthorType { get; private set; }
    public string AuthorName { get; private set; }
    public string Content { get; private set; }
    public bool IsInternal { get; private set; }
    public List<string> Attachments { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
}
