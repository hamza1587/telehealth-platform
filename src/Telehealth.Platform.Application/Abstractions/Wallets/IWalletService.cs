using Telehealth.Platform.Domain.Financial;

namespace Telehealth.Platform.Application.Abstractions.Wallets;

public interface IWalletService
{
    Task<Wallet> GetWalletAsync(Guid patientAccountId, CancellationToken cancellationToken = default);
    Task<Wallet> CreateWalletAsync(Guid patientAccountId, CancellationToken cancellationToken = default);
    Task<Wallet> AddCreditAsync(Guid patientAccountId, long amountMinor, CancellationToken cancellationToken = default);
    Task<bool> DeductCreditAsync(Guid patientAccountId, long amountMinor, CancellationToken cancellationToken = default);
    Task<decimal> GetBalanceAsync(Guid patientAccountId, CancellationToken cancellationToken = default);
}

public interface IPaymentService
{
    Task<string> ProcessPaymentAsync(
        Guid patientAccountId,
        long amountMinor,
        string currency,
        CancellationToken cancellationToken = default);

    Task<bool> RefundPaymentAsync(string transactionId, CancellationToken cancellationToken = default);
    Task<bool> ValidatePaymentAsync(string transactionId, CancellationToken cancellationToken = default);
}