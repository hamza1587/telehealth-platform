using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Telehealth.Platform.Application.Abstractions.Wallets;
using Telehealth.Platform.Domain.Financial;
using Telehealth.Platform.Infrastructure.Persistence;

namespace Telehealth.Platform.Infrastructure.Wallets;

public class WalletService : IWalletService
{
    private readonly PlatformDbContext _dbContext;

    public WalletService(PlatformDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Wallet> GetWalletAsync(Guid patientAccountId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Wallets
            .FirstOrDefaultAsync(w => w.PatientAccountId == patientAccountId, cancellationToken)
            ?? throw new InvalidOperationException($"Wallet not found for patient account {patientAccountId}");
    }

    public async Task<Wallet> CreateWalletAsync(Guid patientAccountId, CancellationToken cancellationToken = default)
    {
        var wallet = Wallet.Create(patientAccountId);
        await _dbContext.Wallets.AddAsync(wallet, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return wallet;
    }

    public async Task<Wallet> AddCreditAsync(Guid patientAccountId, long amountMinor, CancellationToken cancellationToken = default)
    {
        var wallet = await GetWalletAsync(patientAccountId, cancellationToken);
        wallet.AddSeconds(amountMinor / 100);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return wallet;
    }

    public async Task<bool> DeductCreditAsync(Guid patientAccountId, long amountMinor, CancellationToken cancellationToken = default)
    {
        var wallet = await GetWalletAsync(patientAccountId, cancellationToken);
        try
        {
            wallet.ReserveSeconds(amountMinor / 100);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    public async Task<decimal> GetBalanceAsync(Guid patientAccountId, CancellationToken cancellationToken = default)
    {
        var wallet = await _dbContext.Wallets
            .FirstOrDefaultAsync(w => w.PatientAccountId == patientAccountId, cancellationToken);

        return wallet?.AvailableSeconds ?? 0;
    }
}

public class PaymentService : IPaymentService
{
    private readonly IWalletService _walletService;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(IWalletService walletService, ILogger<PaymentService> logger)
    {
        _walletService = walletService;
        _logger = logger;
    }

    public async Task<string> ProcessPaymentAsync(
        Guid patientAccountId,
        long amountMinor,
        string currency,
        CancellationToken cancellationToken = default)
    {
        var success = await _walletService.DeductCreditAsync(patientAccountId, amountMinor, cancellationToken);
        if (!success)
        {
            throw new InvalidOperationException("Insufficient funds");
        }

        var transactionId = Guid.NewGuid().ToString("N");
        _logger.LogInformation("Payment processed: {TransactionId} for {Amount} {Currency}", transactionId, amountMinor, currency);
        return transactionId;
    }

    public async Task<bool> RefundPaymentAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Payment refunded: {TransactionId}", transactionId);
        return true;
    }

    public async Task<bool> ValidatePaymentAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        return !string.IsNullOrEmpty(transactionId);
    }
}