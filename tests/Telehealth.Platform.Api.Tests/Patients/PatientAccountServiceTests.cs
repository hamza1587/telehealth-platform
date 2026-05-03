using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Telehealth.Platform.Domain.Patients;
using Telehealth.Platform.Infrastructure.Patients;
using Telehealth.Platform.Infrastructure.Persistence;

namespace Telehealth.Platform.Api.Tests.Patients;

public class PatientAccountServiceTests
{
    [Fact]
    public async Task CreateAsync_ShouldCreatePatientAccount_WhenValidRequest()
    {
        var options = new DbContextOptionsBuilder<PlatformDbContext>()
            .UseInMemoryDatabase($"PatientAccountTests_{Guid.NewGuid()}")
            .Options;

        using var dbContext = new PlatformDbContext(options);
        var service = new PatientAccountService(dbContext);

        var medplumPatientId = Guid.NewGuid();
        var patient = await service.CreateAsync(
            medplumPatientId,
            "John Doe",
            "john@example.com",
            "US",
            "en");

        patient.Should().NotBeNull();
        patient.DisplayName.Should().Be("John Doe");
        patient.Email.Should().Be("john@example.com");
        patient.CountryCode.Should().Be("US");
        patient.PreferredLanguage.Should().Be("en");
    }

    [Fact]
    public async Task GetByMedplumIdAsync_ShouldReturnPatient_WhenExists()
    {
        var options = new DbContextOptionsBuilder<PlatformDbContext>()
            .UseInMemoryDatabase($"PatientAccountTests_{Guid.NewGuid()}")
            .Options;

        using var dbContext = new PlatformDbContext(options);
        var service = new PatientAccountService(dbContext);

        var medplumPatientId = Guid.NewGuid();
        await service.CreateAsync(medplumPatientId, "John Doe", "john@example.com", "US", "en");

        var patient = await service.GetByMedplumIdAsync(medplumPatientId);

        patient.Should().NotBeNull();
        patient!.MedplumPatientId.Should().Be(medplumPatientId);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnPatient_WhenExists()
    {
        var options = new DbContextOptionsBuilder<PlatformDbContext>()
            .UseInMemoryDatabase($"PatientAccountTests_{Guid.NewGuid()}")
            .Options;

        using var dbContext = new PlatformDbContext(options);
        var service = new PatientAccountService(dbContext);

        var patient = await service.CreateAsync(Guid.NewGuid(), "John Doe", "john@example.com", "US", "en");

        var result = await service.GetByIdAsync(patient.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(patient.Id);
    }

    [Fact]
    public async Task UpdateProfileAsync_ShouldUpdatePatient_WhenValidRequest()
    {
        var options = new DbContextOptionsBuilder<PlatformDbContext>()
            .UseInMemoryDatabase($"PatientAccountTests_{Guid.NewGuid()}")
            .Options;

        using var dbContext = new PlatformDbContext(options);
        var service = new PatientAccountService(dbContext);

        var patient = await service.CreateAsync(Guid.NewGuid(), "John Doe", "john@example.com", "US", "en");

        await service.UpdateProfileAsync(patient.Id, "Jane Doe", "jane@example.com", "CA", "fr");

        var updated = await service.GetByIdAsync(patient.Id);
        updated!.DisplayName.Should().Be("Jane Doe");
        updated.Email.Should().Be("jane@example.com");
        updated.CountryCode.Should().Be("CA");
        updated.PreferredLanguage.Should().Be("fr");
    }

    [Fact]
    public async Task DeactivateAsync_ShouldDeactivatePatient()
    {
        var options = new DbContextOptionsBuilder<PlatformDbContext>()
            .UseInMemoryDatabase($"PatientAccountTests_{Guid.NewGuid()}")
            .Options;

        using var dbContext = new PlatformDbContext(options);
        var service = new PatientAccountService(dbContext);

        var patient = await service.CreateAsync(Guid.NewGuid(), "John Doe", "john@example.com", "US", "en");

        await service.DeactivateAsync(patient.Id);

        var deactivated = await service.GetByIdAsync(patient.Id);
        deactivated!.Status.Should().Be(PatientAccountStatus.Deactivated);
    }

    [Fact]
    public async Task ActivateAsync_ShouldActivatePatient()
    {
        var options = new DbContextOptionsBuilder<PlatformDbContext>()
            .UseInMemoryDatabase($"PatientAccountTests_{Guid.NewGuid()}")
            .Options;

        using var dbContext = new PlatformDbContext(options);
        var service = new PatientAccountService(dbContext);

        var patient = await service.CreateAsync(Guid.NewGuid(), "John Doe", "john@example.com", "US", "en");
        await service.DeactivateAsync(patient.Id);

        await service.ActivateAsync(patient.Id);

        var activated = await service.GetByIdAsync(patient.Id);
        activated!.Status.Should().Be(PatientAccountStatus.Active);
    }
}