using Microsoft.EntityFrameworkCore;

namespace Telehealth.Platform.Integrations.Healthcare;

public class HealthcareIntegrationsDbContext : DbContext
{
    public HealthcareIntegrationsDbContext(DbContextOptions<HealthcareIntegrationsDbContext> options)
        : base(options)
    {
    }

    public DbSet<LabResult> LabResults { get; set; }
    public DbSet<InsuranceClaim> InsuranceClaims { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<LabResult>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PatientId).IsRequired();
            entity.Property(e => e.OrderingProviderId).IsRequired();
            entity.Property(e => e.LabSystemId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.TestCode).IsRequired().HasMaxLength(50);
            entity.Property(e => e.TestName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Value).IsRequired();
            entity.Property(e => e.Unit).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ReferenceRange).HasMaxLength(100);
            entity.Property(e => e.ResultDate).IsRequired();
            entity.Property(e => e.SampleDate).IsRequired();
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
        });

        modelBuilder.Entity<InsuranceClaim>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PatientId).IsRequired();
            entity.Property(e => e.ConsultationId).IsRequired();
            entity.Property(e => e.InsuranceProviderId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PolicyNumber).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Amount).IsRequired();
            entity.Property(e => e.Currency).IsRequired().HasMaxLength(3);
            entity.Property(e => e.ServiceType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.SubmittedDate).IsRequired();
            entity.Property(e => e.ProcessedDate);
            entity.Property(e => e.ClaimNumber).HasMaxLength(100);
            entity.Property(e => e.RejectionReason).HasMaxLength(500);
        });
    }
}