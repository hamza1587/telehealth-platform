using Microsoft.EntityFrameworkCore;
using Telehealth.Platform.Domain.Support;

namespace Telehealth.Platform.Infrastructure.Persistence;

public partial class PlatformDbContext
{
    public DbSet<SupportTicket> SupportTickets => Set<SupportTicket>();
    public DbSet<TicketComment> TicketComments => Set<TicketComment>();
}
