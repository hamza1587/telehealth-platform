using Microsoft.EntityFrameworkCore;
using Telehealth.Platform.Analytics.Domain.Models;

namespace Telehealth.Platform.Analytics.Data;

public class AnalyticsDbContext : DbContext
{
    public AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options) : base(options)
    {
    }

    public DbSet<DashboardMetrics> DashboardMetrics { get; set; }
    public DbSet<AnalyticsReport> AnalyticsReports { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure DashboardMetrics entity
        modelBuilder.Entity<DashboardMetrics>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MetricName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Category).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Dimensions).HasMaxLength(500);
            entity.Property(e => e.Timestamp).IsRequired();
        });

        // Configure AnalyticsReport entity
        modelBuilder.Entity<AnalyticsReport>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ReportName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.QueryDefinition).IsRequired();
            entity.Property(e => e.DownloadUrl).IsRequired().HasMaxLength(500);
            entity.Property(e => e.GeneratedAt).IsRequired();
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.ErrorMessage).HasMaxLength(1000);
        });
    }
}
