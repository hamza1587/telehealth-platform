using Microsoft.EntityFrameworkCore;
using Telehealth.Platform.Video.Domain;

namespace Telehealth.Platform.Video.Data;

public class VideoDbContext : DbContext
{
    public VideoDbContext(DbContextOptions<VideoDbContext> options) : base(options)
    {
    }

    public DbSet<VideoRoom> VideoRooms { get; set; }
    public DbSet<RecordingSession> RecordingSessions { get; set; }
    public DbSet<CallSession> CallSessions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure VideoRoom entity
        modelBuilder.Entity<VideoRoom>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RoomName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.TwilioRoomSid).HasMaxLength(100);
            entity.Property(e => e.CountryCode).HasMaxLength(10);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.Status).IsRequired();
        });

        // Configure RecordingSession entity
        modelBuilder.Entity<RecordingSession>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RoomId).IsRequired();
            entity.Property(e => e.RecordingUrl).IsRequired().HasMaxLength(500);
            entity.Property(e => e.StartedAt).IsRequired();
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.TwilioRecordingSid).HasMaxLength(100);
            entity.Property(e => e.TranscriptionUrl).HasMaxLength(500);
            
            entity.HasOne<VideoRoom>()
                .WithMany()
                .HasForeignKey(e => e.RoomId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CallSession>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ConsultationId).IsRequired();
            entity.Property(e => e.RoomId).IsRequired();
            entity.Property(e => e.InitiatedBy).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();

            entity.HasOne(e => e.Room)
                .WithMany()
                .HasForeignKey(e => e.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.ConsultationId);
        });
    }
}
