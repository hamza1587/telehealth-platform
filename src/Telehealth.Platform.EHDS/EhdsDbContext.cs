using Microsoft.EntityFrameworkCore;
using Telehealth.Platform.Domain.Entities;

namespace Telehealth.Platform.EHDS;

public class EhdsDbContext : DbContext
{
    public EhdsDbContext(DbContextOptions<EhdsDbContext> options)
        : base(options)
    {
    }

    public DbSet<PatientHealthRecord> PatientHealthRecords { get; set; }
    public DbSet<ResearchExportRequest> ResearchExportRequests { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PatientHealthRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PatientId).IsRequired();
            entity.Property(e => e.DataOrigin).IsRequired().HasMaxLength(200);
            entity.Property(e => e.LastUpdated).IsRequired();
            entity.Property(e => e.IsCrossBorderAccessible).IsRequired();
            
            entity.OwnsMany(e => e.Conditions, c =>
            {
                c.HasKey(e => e.Id);
                c.Property(e => e.Code).IsRequired().HasMaxLength(50);
                c.Property(e => e.System).IsRequired().HasMaxLength(200);
                c.Property(e => e.Display).IsRequired();
                c.Property(e => e.ClinicalStatus).IsRequired().HasMaxLength(50);
                c.Property(e => e.VerificationStatus).IsRequired().HasMaxLength(50);
                c.Property(e => e.OnsetDateTime).IsRequired();
            });

            entity.OwnsMany(e => e.Medications, m =>
            {
                m.HasKey(e => e.Id);
                m.Property(e => e.Code).IsRequired().HasMaxLength(50);
                m.Property(e => e.System).IsRequired().HasMaxLength(200);
                m.Property(e => e.Display).IsRequired();
                m.Property(e => e.Dosage).IsRequired().HasMaxLength(100);
                m.Property(e => e.Frequency).IsRequired().HasMaxLength(100);
                m.Property(e => e.StartDate).IsRequired();
            });

            entity.OwnsMany(e => e.Allergies, a =>
            {
                a.HasKey(e => e.Id);
                a.Property(e => e.Code).IsRequired().HasMaxLength(50);
                a.Property(e => e.System).IsRequired().HasMaxLength(200);
                a.Property(e => e.Display).IsRequired();
                a.Property(e => e.ClinicalStatus).IsRequired().HasMaxLength(50);
                a.Property(e => e.Criticality).IsRequired().HasMaxLength(50);
            });

            entity.OwnsMany(e => e.Immunizations, i =>
            {
                i.HasKey(e => e.Id);
                i.Property(e => e.VaccineCode).IsRequired().HasMaxLength(50);
                i.Property(e => e.System).IsRequired().HasMaxLength(200);
                i.Property(e => e.Display).IsRequired();
                i.Property(e => e.AdministrationDate).IsRequired();
            });

            entity.OwnsMany(e => e.Procedures, p =>
            {
                p.HasKey(e => e.Id);
                p.Property(e => e.Code).IsRequired().HasMaxLength(50);
                p.Property(e => e.System).IsRequired().HasMaxLength(200);
                p.Property(e => e.Display).IsRequired();
                p.Property(e => e.PerformedDate).IsRequired();
            });

            entity.OwnsMany(e => e.Observations, o =>
            {
                o.HasKey(e => e.Id);
                o.Property(e => e.Code).IsRequired().HasMaxLength(50);
                o.Property(e => e.System).IsRequired().HasMaxLength(200);
                o.Property(e => e.Display).IsRequired();
                o.Property(e => e.Value).IsRequired();
                o.Property(e => e.Unit).IsRequired().HasMaxLength(50);
                o.Property(e => e.EffectiveDateTime).IsRequired();
            });
        });

        modelBuilder.Entity<ResearchExportRequest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RequesterId).IsRequired();
            entity.Property(e => e.ResearchPurpose).IsRequired().HasMaxLength(500);
            entity.Property(e => e.RequestedAt).IsRequired();
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.DeidentificationMethod).IsRequired().HasMaxLength(100);
            entity.Property(e => e.KAnonymityLevel).IsRequired();
            entity.Property(e => e.Epsilon).IsRequired();
            entity.Property(e => e.ApprovedBy).HasMaxLength(200);
            entity.Property(e => e.ExportUrl).HasMaxLength(500);
        });
    }
}
