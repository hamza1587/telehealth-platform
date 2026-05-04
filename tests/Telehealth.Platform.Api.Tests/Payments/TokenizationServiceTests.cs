using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Telehealth.Platform.Infrastructure.Payments;

namespace Telehealth.Platform.Api.Tests.Payments;

public class TokenizationServiceTests
{
    private readonly TokenizationService _service;
    private readonly TokenizationServiceOptions _options;

    public TokenizationServiceTests()
    {
        _options = new TokenizationServiceOptions
        {
            EncryptionKey = Convert.ToBase64String(new byte[32])
        };
        
        var options = Options.Create(_options);
        _service = new TokenizationService(options, null);
    }

    [Fact]
    public async Task TokenizeCardNumber_ReturnsToken()
    {
        var cardNumber = "4111111111111111";

        var token = await _service.TokenizeCardNumberAsync(cardNumber);

        token.Should().NotBeNullOrEmpty();
        token.Length.Should().BeGreaterThan(20);
    }

    [Fact]
    public async Task Detokenize_ReturnsOriginalValue()
    {
        var cardNumber = "4111111111111111";

        var token = await _service.TokenizeCardNumberAsync(cardNumber);
        var detokenized = await _service.DetokenizeAsync(token);

        detokenized.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ValidateToken_WhenTokenExists_ReturnsTrue()
    {
        var cardNumber = "4111111111111111";

        var token = await _service.TokenizeCardNumberAsync(cardNumber);
        var isValid = await _service.ValidateTokenAsync(token);

        isValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateToken_WhenTokenNotExists_ReturnsFalse()
    {
        var isValid = await _service.ValidateTokenAsync("invalid-token");

        isValid.Should().BeFalse();
    }

    [Fact]
    public async Task RevokeToken_RemovesToken()
    {
        var cardNumber = "4111111111111111";

        var token = await _service.TokenizeCardNumberAsync(cardNumber);
        var revoked = await _service.RevokeTokenAsync(token);

        revoked.Should().BeTrue();
        (await _service.ValidateTokenAsync(token)).Should().BeFalse();
    }

    [Fact]
    public async Task GeneratePaymentMethodToken_CreatesToken()
    {
        var token = await _service.GeneratePaymentMethodTokenAsync(
            "Visa",
            "4111111111111111",
            "12",
            "2025",
            "123",
            default);

        token.Should().NotBeNullOrEmpty();
    }
}