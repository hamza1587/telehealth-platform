using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Telehealth.Platform.Infrastructure.Payments;
using Telehealth.Platform.Application.Abstractions.Payments;

namespace Telehealth.Platform.Api.Tests.Payments;

public class PciComplianceServiceTests
{
    private readonly PciComplianceService _service;
    private readonly PciComplianceServiceOptions _options;

    public PciComplianceServiceTests()
    {
        _options = new PciComplianceServiceOptions
        {
            EncryptionKey = Convert.ToBase64String(new byte[32]),
            CvvStorageAllowed = false
        };
        
        var options = Options.Create(_options);
        _service = new PciComplianceService(options, null);
    }

    [Fact]
    public async Task MaskCardNumber_ReturnsMaskedNumber()
    {
        var cardNumber = "4111111111111111";

        var masked = await _service.MaskCardNumberAsync(cardNumber);

        masked.Should().Be("****-****-****-1111");
    }

    [Fact]
    public async Task MaskCardNumber_WithShortNumber_ReturnsStars()
    {
        var cardNumber = "123";

        var masked = await _service.MaskCardNumberAsync(cardNumber);

        masked.Should().Be("****");
    }

    [Fact]
    public async Task EncryptDecrypt_SymmetricEncryption()
    {
        var data = "sensitive-data-123";

        var encrypted = await _service.EncryptSensitiveDataAsync(data);
        var decrypted = await _service.DecryptSensitiveDataAsync(encrypted);

        decrypted.Should().Be(data);
    }

    [Fact]
    public async Task ValidatePciCompliance_WithValidData_ReturnsTrue()
    {
        var request = new PciComplianceCheckRequest
        {
            CardNumber = "4111111111111111",
            ExpiryMonth = "12",
            ExpiryYear = "2025",
            Cvv = "",
            StoreCvv = false
        };

        var isValid = await _service.ValidatePciComplianceAsync(request);

        isValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidatePciCompliance_WithCvvStorageNotAllowed_ReturnsFalse()
    {
        _options.CvvStorageAllowed = false;

        var request = new PciComplianceCheckRequest
        {
            CardNumber = "4111111111111111",
            ExpiryMonth = "12",
            ExpiryYear = "2025",
            Cvv = "123",
            StoreCvv = false
        };

        var isValid = await _service.ValidatePciComplianceAsync(request);

        isValid.Should().BeFalse();
    }

    [Fact]
    public async Task GenerateComplianceReport_ReturnsReport()
    {
        var report = await _service.GenerateComplianceReportAsync();

        report.Should().NotBeNull();
        report.GeneratedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        report.IsCompliant.Should().BeTrue();
        report.Recommendations.Should().NotBeEmpty();
    }

    [Fact]
    public async Task LogDataAccessKey_AddsLogEntry()
    {
        await _service.LogDataAccessKeyAsync("Payment", "pay-123", "user-456");

        var report = await _service.GenerateComplianceReportAsync();
        report.AccessLogs.Should().BeGreaterThan(0);
    }
}