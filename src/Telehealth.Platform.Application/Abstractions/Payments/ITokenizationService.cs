namespace Telehealth.Platform.Application.Abstractions.Payments;

public interface ITokenizationService
{
    Task<string> TokenizeCardNumberAsync(string cardNumber, CancellationToken cancellationToken = default);

    Task<string> TokenizeBankAccountAsync(string accountNumber, CancellationToken cancellationToken = default);

    Task<string> DetokenizeAsync(string token, CancellationToken cancellationToken = default);

    Task<string> GeneratePaymentMethodTokenAsync(
        string provider,
        string cardNumber,
        string expiryMonth,
        string expiryYear,
        string cvv,
        CancellationToken cancellationToken = default);

    Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);

    Task<bool> RevokeTokenAsync(string token, CancellationToken cancellationToken = default);
}