using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Telehealth.Platform.Domain.Doctors;
using Telehealth.Platform.Infrastructure.Doctors;
using Telehealth.Platform.Infrastructure.Persistence;

namespace Telehealth.Platform.Api.Tests.Doctors;

public class DoctorProfileServiceTests
{
    [Fact]
    public async Task CreateAsync_ShouldCreateDoctorProfile_WhenValidRequest()
    {
        var options = new DbContextOptionsBuilder<PlatformDbContext>()
            .UseInMemoryDatabase($"DoctorProfileTests_{Guid.NewGuid()}")
            .Options;

        using var dbContext = new PlatformDbContext(options);
        var service = new DoctorProfileService(dbContext);

        var doctor = await service.CreateAsync(
            Guid.NewGuid(),
            "Dr. John Smith",
            "US",
            "Cardiology",
            50000,
            "USD");

        doctor.Should().NotBeNull();
        doctor.DisplayName.Should().Be("Dr. John Smith");
        doctor.CountryCode.Should().Be("US");
        doctor.PrimarySpecialty.Should().Be("Cardiology");
        doctor.DefaultPricePerSecondMinor.Should().Be(50000);
        doctor.Currency.Should().Be("USD");
    }

    [Fact]
    public async Task GetByMedplumIdAsync_ShouldReturnDoctor_WhenExists()
    {
        var options = new DbContextOptionsBuilder<PlatformDbContext>()
            .UseInMemoryDatabase($"DoctorProfileTests_{Guid.NewGuid()}")
            .Options;

        using var dbContext = new PlatformDbContext(options);
        var service = new DoctorProfileService(dbContext);

        var medplumId = Guid.NewGuid();
        await service.CreateAsync(medplumId, "Dr. John Smith", "US", "Cardiology", 50000, "USD");

        var doctor = await service.GetByMedplumIdAsync(medplumId);

        doctor.Should().NotBeNull();
        doctor!.MedplumPractitionerId.Should().Be(medplumId);
    }

    [Fact]
    public async Task UpdateProfileAsync_ShouldUpdateDoctor_WhenValidRequest()
    {
        var options = new DbContextOptionsBuilder<PlatformDbContext>()
            .UseInMemoryDatabase($"DoctorProfileTests_{Guid.NewGuid()}")
            .Options;

        using var dbContext = new PlatformDbContext(options);
        var service = new DoctorProfileService(dbContext);

        var doctor = await service.CreateAsync(Guid.NewGuid(), "Dr. John Smith", "US", "Cardiology", 50000, "USD");

        await service.UpdateProfileAsync(doctor.Id, "Dr. Jane Smith", "CA", "Neurology", 60000, "CAD");

        var updated = await service.GetByIdAsync(doctor.Id);
        updated!.DisplayName.Should().Be("Dr. Jane Smith");
        updated.CountryCode.Should().Be("CA");
        updated.PrimarySpecialty.Should().Be("Neurology");
        updated.DefaultPricePerSecondMinor.Should().Be(60000);
        updated.Currency.Should().Be("CAD");
    }

    [Fact]
    public async Task SubmitForVerificationAsync_ShouldUpdateStatus()
    {
        var options = new DbContextOptionsBuilder<PlatformDbContext>()
            .UseInMemoryDatabase($"DoctorProfileTests_{Guid.NewGuid()}")
            .Options;

        using var dbContext = new PlatformDbContext(options);
        var service = new DoctorProfileService(dbContext);

        var doctor = await service.CreateAsync(Guid.NewGuid(), "Dr. John Smith", "US", "Cardiology", 50000, "USD");

        await service.SubmitForVerificationAsync(doctor.Id);

        var updated = await service.GetByIdAsync(doctor.Id);
        updated!.VerificationStatus.Should().Be(DoctorVerificationStatus.Submitted);
    }

    [Fact]
    public async Task ApproveVerificationAsync_ShouldUpdateStatus()
    {
        var options = new DbContextOptionsBuilder<PlatformDbContext>()
            .UseInMemoryDatabase($"DoctorProfileTests_{Guid.NewGuid()}")
            .Options;

        using var dbContext = new PlatformDbContext(options);
        var service = new DoctorProfileService(dbContext);

        var doctor = await service.CreateAsync(Guid.NewGuid(), "Dr. John Smith", "US", "Cardiology", 50000, "USD");

        await service.ApproveVerificationAsync(doctor.Id);

        var updated = await service.GetByIdAsync(doctor.Id);
        updated!.VerificationStatus.Should().Be(DoctorVerificationStatus.Verified);
        updated.MarketplaceStatus.Should().Be(DoctorMarketplaceStatus.Available);
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnMatchingDoctors()
    {
        var options = new DbContextOptionsBuilder<PlatformDbContext>()
            .UseInMemoryDatabase($"DoctorProfileTests_{Guid.NewGuid()}")
            .Options;

        using var dbContext = new PlatformDbContext(options);
        var service = new DoctorProfileService(dbContext);

        await service.CreateAsync(Guid.NewGuid(), "Dr. Cardiology", "US", "Cardiology", 50000, "USD");
        await service.CreateAsync(Guid.NewGuid(), "Dr. Neurology", "US", "Neurology", 60000, "USD");

        await service.ApproveVerificationAsync(await GetDoctorId(dbContext, "Dr. Cardiology"));
        await service.ApproveVerificationAsync(await GetDoctorId(dbContext, "Dr. Neurology"));

        var results = await service.SearchAsync("Cardiology", null, DoctorMarketplaceStatus.Available);

        results.Should().HaveCount(1);
        results.First().PrimarySpecialty.Should().Be("Cardiology");
    }

    private async Task<Guid> GetDoctorId(PlatformDbContext dbContext, string displayName)
    {
        return (await dbContext.DoctorProfiles.FirstOrDefaultAsync(d => d.DisplayName == displayName))?.Id 
            ?? throw new InvalidOperationException("Doctor not found");
    }
}