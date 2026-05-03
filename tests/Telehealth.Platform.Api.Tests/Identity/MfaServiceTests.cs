using Microsoft.Extensions.Logging;
using Telehealth.Platform.Domain.Identity;
using Telehealth.Platform.Infrastructure.Identity;

namespace Telehealth.Platform.Api.Tests.Identity;

public class MfaServiceTests
{
    private readonly ILogger<MfaService> _logger;
    private readonly MfaService _service;
    private readonly PlatformUser _user;

    public MfaServiceTests()
    {
        var logger = new LoggerFactory();
        _logger = logger.CreateLogger<MfaService>();
        _service = new MfaService(_logger);
        _user = new PlatformUser(
            Guid.NewGuid(),
            "test@example.com",
            "hashedpassword",
            UserType.Patient);
    }

    [Fact]
    public async Task GenerateSetupAsync_ShouldGenerateSecretKeyAndUri()
    {
        var result = await _service.GenerateSetupAsync(_user);

        Assert.NotNull(result.SecretKey);
        Assert.NotNull(result.QrCodeUri);
        Assert.NotEmpty(result.SecretKey);
        Assert.True(_user.IsMfaEnabled);
        Assert.NotNull(_user.MfaSecretKey);
    }

    [Fact]
    public async Task VerifyCodeAsync_ShouldReturnFalse_WhenMfaNotEnabled()
    {
        var result = await _service.VerifyCodeAsync(_user, "123456");

        Assert.False(result);
    }

    [Fact]
    public async Task VerifyCodeAsync_ShouldReturnFalse_WhenCodeIsEmpty()
    {
        await _service.GenerateSetupAsync(_user);

        var result = await _service.VerifyCodeAsync(_user, "");

        Assert.False(result);
    }

    [Fact]
    public async Task VerifyCodeAsync_ShouldReturnFalse_WhenCodeIsNull()
    {
        await _service.GenerateSetupAsync(_user);

        var result = await _service.VerifyCodeAsync(_user, null);

        Assert.False(result);
    }

    [Fact]
    public async Task DisableMfaAsync_ShouldDisableMfa()
    {
        await _service.GenerateSetupAsync(_user);
        Assert.True(_user.IsMfaEnabled);

        await _service.DisableMfaAsync(_user);

        Assert.False(_user.IsMfaEnabled);
        Assert.Null(_user.MfaSecretKey);
    }

    [Fact]
    public async Task GetQrCodeUriAsync_ShouldReturnUri_WhenMfaEnabled()
    {
        await _service.GenerateSetupAsync(_user);

        var uri = await _service.GetQrCodeUriAsync(_user);

        Assert.NotNull(uri);
        Assert.Contains("otpauth://", uri);
        Assert.Contains("Telehealth Platform", uri);
    }

    [Fact]
    public async Task GetQrCodeUriAsync_ShouldReturnUri_WhenSecretKeyExists()
    {
        await _service.GenerateSetupAsync(_user);
        var expectedUri = await _service.GetQrCodeUriAsync(_user);

        var actualUri = await _service.GetQrCodeUriAsync(_user);

        Assert.Equal(expectedUri, actualUri);
    }

    [Fact]
    public void GenerateSecretKey_ShouldGenerateValidBase32Key()
    {
        var secretKey = _service.GenerateSecretKey();

        Assert.NotNull(secretKey);
        Assert.NotEmpty(secretKey);
        Assert.All(secretKey.ToCharArray(), c => Assert.True("ABCDEFGHIJKLMNOPQRSTUVWXYZ234567".Contains(c)));
    }

    [Fact]
    public void GenerateSecretKey_ShouldGenerateUniqueKeys()
    {
        var key1 = _service.GenerateSecretKey();
        var key2 = _service.GenerateSecretKey();

        Assert.NotEqual(key1, key2);
    }

    [Fact]
    public void GenerateQrCodeUri_ShouldReturnValidUri()
    {
        var secretKey = _service.GenerateSecretKey();
        var email = "test@example.com";

        var uri = _service.GenerateQrCodeUri(secretKey, email);

        Assert.NotNull(uri);
        Assert.Contains("otpauth://", uri);
        Assert.Contains(Uri.EscapeDataString("Telehealth Platform"), uri);
        Assert.Contains(Uri.EscapeDataString(email), uri);
        Assert.Contains(secretKey, uri);
    }

    [Fact]
    public void GenerateQrCodeUri_ShouldIncludeSecretKeyInUri()
    {
        var secretKey = _service.GenerateSecretKey();
        var email = "doctor@hospital.com";

        var uri = _service.GenerateQrCodeUri(secretKey, email);

        Assert.Contains($"secret={secretKey}", uri);
    }

    [Fact]
    public void ValidateCode_ShouldReturnFalse_WhenSecretKeyIsNull()
    {
        var result = _service.ValidateCode(null, "123456");

        Assert.False(result);
    }

    [Fact]
    public void ValidateCode_ShouldReturnFalse_WhenCodeIsNull()
    {
        var secretKey = _service.GenerateSecretKey();
        var result = _service.ValidateCode(secretKey, null);

        Assert.False(result);
    }

    [Fact]
    public void ValidateCode_ShouldReturnFalse_WhenBothNull()
    {
        var result = _service.ValidateCode(null, null);

        Assert.False(result);
    }

    [Fact]
    public void ValidateCode_ShouldReturnFalse_ForInvalidCode()
    {
        var secretKey = _service.GenerateSecretKey();
        var result = _service.ValidateCode(secretKey, "000000");

        Assert.False(result);
    }

    [Fact]
    public void GenerateRecoveryCodes_ShouldGenerateCorrectCount()
    {
        var codes = _service.GenerateRecoveryCodes(5);

        Assert.Equal(5, codes.Length);
    }

    [Fact]
    public void GenerateRecoveryCodes_ShouldGenerateUniqueCodes()
    {
        var codes = _service.GenerateRecoveryCodes(10);

        Assert.Equal(10, codes.Length);
        Assert.Equal(10, codes.Distinct().Count());
    }

    [Fact]
    public void GenerateRecoveryCodes_ShouldGenerate8CharacterCodes()
    {
        var codes = _service.GenerateRecoveryCodes(1);

        Assert.Equal(8, codes[0].Length);
    }

    [Fact]
    public void ValidateRecoveryCode_ShouldReturnTrue_ForValidCode()
    {
        var codes = _service.GenerateRecoveryCodes(5);
        var codesString = string.Join(",", codes);

        var result = _service.ValidateRecoveryCode(codesString, codes[0]);

        Assert.True(result);
    }

    [Fact]
    public void ValidateRecoveryCode_ShouldReturnFalse_ForInvalidCode()
    {
        var codes = _service.GenerateRecoveryCodes(5);
        var codesString = string.Join(",", codes);

        var result = _service.ValidateRecoveryCode(codesString, "INVALID");

        Assert.False(result);
    }

    [Fact]
    public void ValidateRecoveryCode_ShouldReturnFalse_WhenCodesAreNull()
    {
        var result = _service.ValidateRecoveryCode(null, "123456");

        Assert.False(result);
    }

    [Fact]
    public void RemoveUsedRecoveryCode_ShouldRemoveCode()
    {
        var codes = _service.GenerateRecoveryCodes(5);
        var codesString = string.Join(",", codes);

        var updatedCodes = _service.RemoveUsedRecoveryCode(codesString, codes[0]);

        Assert.DoesNotContain(codes[0], updatedCodes);
        Assert.Equal(4, updatedCodes.Split(',').Length);
    }

    [Fact]
    public void RemoveUsedRecoveryCode_ShouldHandleNewlineSeparatedCodes()
    {
        var codes = _service.GenerateRecoveryCodes(5);
        var codesString = string.Join(Environment.NewLine, codes);

        var updatedCodes = _service.RemoveUsedRecoveryCode(codesString, codes[0]);

        Assert.DoesNotContain(codes[0], updatedCodes);
    }
}