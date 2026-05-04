using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using Telehealth.Platform.Application.Abstractions.Payments;

namespace Telehealth.Platform.Infrastructure.Payments;

public class TokenizationServiceOptions
{
    public string EncryptionKey { get; set; } = string.Empty;
    public int TokenLength { get; set; } = 32;
}

public class TokenizationService : ITokenizationService
{
    private readonly IOptions<TokenizationServiceOptions> _options;
    private readonly ILogger<TokenizationService> _logger;
    private readonly Dictionary<string, string> _tokenStore = new();

    public TokenizationService(
        IOptions<TokenizationServiceOptions> options,
        ILogger<TokenizationService> logger)
    {
        _options = options;
        _logger = logger;
    }

    public async Task<string> TokenizeCardNumberAsync(string cardNumber, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(_options.Value.TokenLength));
            _tokenStore[token] = HashCardNumber(cardNumber);
            return token;
        }, cancellationToken);
    }

    public async Task<string> TokenizeBankAccountAsync(string accountNumber, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(_options.Value.TokenLength));
            _tokenStore[token] = HashAccountNumber(accountNumber);
            return token;
        }, cancellationToken);
    }

    public async Task<string> DetokenizeAsync(string token, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            if (_tokenStore.TryGetValue(token, out var value))
            {
                return value;
            }
            _logger.LogWarning("Token not found: {Token}", token);
            return string.Empty;
        }, cancellationToken);
    }

    public async Task<string> GeneratePaymentMethodTokenAsync(
        string provider,
        string cardNumber,
        string expiryMonth,
        string expiryYear,
        string cvv,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            var dataToTokenize = $"{provider}|{cardNumber}|{expiryMonth}|{expiryYear}";
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(_options.Value.TokenLength));
            _tokenStore[token] = Encrypt(dataToTokenize);
            return token;
        }, cancellationToken);
    }

    public async Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(_tokenStore.ContainsKey(token));
    }

    public async Task<bool> RevokeTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            if (_tokenStore.ContainsKey(token))
            {
                _tokenStore.Remove(token);
                return true;
            }
            return false;
        }, cancellationToken);
    }

    private string HashCardNumber(string cardNumber)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(cardNumber));
        return Convert.ToBase64String(hash);
    }

    private string HashAccountNumber(string accountNumber)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(accountNumber));
        return Convert.ToBase64String(hash);
    }

    private string Encrypt(string data)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(data));
    }
}