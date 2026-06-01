using Microsoft.EntityFrameworkCore;
using Telehealth.Platform.Prescription.Domain.Models;

namespace Telehealth.Platform.Prescription.Data;

public class PrescriptionDbContext : DbContext
{
    public PrescriptionDbContext(DbContextOptions<PrescriptionDbContext> options) : base(options)
    {
    }

    public DbSet<Domain.Models.Prescription> Prescriptions { get; set; }
    public DbSet<Domain.Models.PrescriptionItem> PrescriptionItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Prescription entity
        modelBuilder.Entity<Domain.Models.Prescription>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RoomName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.CountryCode).IsRequired().HasMaxLength(10);
            entity.Property(e => e.DigitalSignature).HasMaxLength(500);
            entity.Property(e => e.NationalPrescriptionId).HasMaxLength(100);
            entity.Property(e => e.PharmacyId).HasMaxLength(100);
            entity.Property(e => e.DeaNumber).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.Status).IsRequired();
        });

        // Configure PrescriptionItem entity
        modelBuilder.Entity<Domain.Models.PrescriptionItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MedicationCode).IsRequired().HasMaxLength(50);
            entity.Property(e => e.MedicationName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Dosage).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Frequency).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Instructions).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Refills).HasMaxLength(50);
            
            entity.HasOne<Domain.Models.Prescription>()
                .WithMany(p => p.Items)
                .HasForeignKey(e => e.RoomId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
