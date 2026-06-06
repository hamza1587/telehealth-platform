using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Telehealth.Platform.Infrastructure.Persistence;

#nullable disable

namespace Telehealth.Platform.Infrastructure.Migrations
{
    [DbContext(typeof(PlatformDbContext))]
    [Migration("20260606000001_AddSupportTickets")]
    public partial class AddSupportTickets : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                create table if not exists support_tickets (
                    id uuid primary key,
                    ticket_number varchar(30) not null unique,
                    user_id uuid not null,
                    user_type varchar(50) not null,
                    category varchar(100) not null,
                    subject varchar(300) not null,
                    description text not null,
                    priority varchar(20) not null,
                    status varchar(30) not null default 'Open',
                    assigned_to uuid,
                    assigned_to_name varchar(200),
                    related_consultation_id uuid,
                    related_billing_id uuid,
                    resolution text,
                    created_at timestamp with time zone not null,
                    updated_at timestamp with time zone not null,
                    assigned_at timestamp with time zone,
                    resolved_at timestamp with time zone,
                    closed_at timestamp with time zone
                );
                create index if not exists ix_support_tickets_user_id on support_tickets(user_id);
                create index if not exists ix_support_tickets_status on support_tickets(status);
                create index if not exists ix_support_tickets_number on support_tickets(ticket_number);

                create table if not exists ticket_comments (
                    id uuid primary key,
                    ticket_id uuid not null,
                    author_id uuid not null,
                    author_type varchar(50) not null,
                    author_name varchar(200) not null,
                    content text not null,
                    is_internal boolean not null default false,
                    attachments text[] not null default '{}',
                    created_at timestamp with time zone not null,
                    foreign key (ticket_id) references support_tickets(id) on delete cascade
                );
                create index if not exists ix_ticket_comments_ticket_id on ticket_comments(ticket_id);
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                drop table if exists ticket_comments;
                drop table if exists support_tickets;
                """);
        }
    }
}
