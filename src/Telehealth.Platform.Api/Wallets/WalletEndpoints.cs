using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Telehealth.Platform.Application.Abstractions.Wallets;
using Telehealth.Platform.Domain.Financial;
using Telehealth.Platform.Infrastructure.Persistence;

namespace Telehealth.Platform.Api.Wallets;

/// <summary>
/// Credit wallet and day pass API endpoints.
/// The wallet stores consultation time in seconds; 60 seconds = 1 credit minute (displayed as EUR equivalent).
/// </summary>
public static class WalletEndpoints
{
    // 1 EUR = 60 consultation seconds (1 minute per EUR)
    private const decimal SecondsPerEur = 60m;

    public static IEndpointRouteBuilder MapWalletEndpoints(this IEndpointRouteBuilder app)
    {
        var wallets = app.MapGroup("/wallets").WithTags("Wallets");

        wallets.MapGet("/my-wallet", GetMyWalletAsync).RequireAuthorization("RequirePatient");
        wallets.MapGet("/my-wallet/transactions", GetMyTransactionsAsync).RequireAuthorization("RequirePatient");
        wallets.MapPost("/my-wallet/deposit", DepositAsync).RequireAuthorization("RequirePatient");

        wallets.MapGet("/day-passes/available", GetAvailableDayPassesAsync).RequireAuthorization();
        wallets.MapGet("/my-day-passes", GetMyDayPassesAsync).RequireAuthorization("RequirePatient");
        wallets.MapPost("/day-passes/purchase", PurchaseDayPassAsync).RequireAuthorization("RequirePatient");
        wallets.MapGet("/my-day-passes/{dayPassId}", GetDayPassDetailsAsync).RequireAuthorization("RequirePatient");

        return app;
    }

    private static async Task<IResult> GetMyWalletAsync(
        ClaimsPrincipal user,
        IWalletService walletService)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue) return Results.Unauthorized();

        Wallet wallet;
        try
        {
            wallet = await walletService.GetWalletAsync(userId.Value);
        }
        catch (InvalidOperationException)
        {
            // Create wallet on first access
            wallet = await walletService.CreateWalletAsync(userId.Value);
        }

        var balanceEur = wallet.AvailableSeconds / SecondsPerEur;
        var dto = new WalletDto(
            wallet.Id,
            wallet.PatientAccountId,
            balanceEur,
            "EUR",
            wallet.AvailableSeconds,
            wallet.Status == WalletStatus.Active,
            wallet.CreatedAt);

        return Results.Ok(dto);
    }

    private static async Task<IResult> GetMyTransactionsAsync(
        ClaimsPrincipal user,
        PlatformDbContext db,
        string? type,
        DateTimeOffset? from,
        DateTimeOffset? to,
        int page = 1,
        int pageSize = 20)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue) return Results.Unauthorized();

        var wallet = await db.Wallets.FirstOrDefaultAsync(w => w.PatientAccountId == userId.Value);
        if (wallet is null) return Results.Ok(new { Items = Array.Empty<object>(), TotalCount = 0 });

        var query = db.WalletLedgerEntries
            .Where(e => e.WalletId == wallet.Id);

        if (from.HasValue) query = query.Where(e => e.CreatedAt >= from.Value);
        if (to.HasValue) query = query.Where(e => e.CreatedAt <= to.Value);
        if (!string.IsNullOrEmpty(type)) query = query.Where(e => e.EntryType == type);

        var totalCount = await query.CountAsync();
        var entries = await query
            .OrderByDescending(e => e.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = entries.Select(e => new WalletTransactionDto(
            e.Id,
            e.EntryType,
            e.SecondsDelta / SecondsPerEur,
            "EUR",
            e.BalanceAfterSeconds / SecondsPerEur,
            e.Reason,
            e.ReferenceId,
            e.CreatedAt));

        return Results.Ok(new { Items = items, TotalCount = totalCount, Page = page, PageSize = pageSize });
    }

    private static async Task<IResult> DepositAsync(
        DepositRequestDto request,
        ClaimsPrincipal user,
        IWalletService walletService)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue) return Results.Unauthorized();

        if (request.Amount <= 0) return Results.BadRequest("Amount must be positive.");

        // Ensure wallet exists
        try { await walletService.GetWalletAsync(userId.Value); }
        catch (InvalidOperationException) { await walletService.CreateWalletAsync(userId.Value); }

        // 1 EUR = 60 seconds; AddCreditAsync expects amountMinor where it internally does /100 to get seconds
        var secondsToAdd = (long)(request.Amount * SecondsPerEur);
        var wallet = await walletService.AddCreditAsync(userId.Value, secondsToAdd * 100);

        return Results.Ok(new
        {
            Message = "Deposit processed",
            NewBalance = wallet.AvailableSeconds / SecondsPerEur,
            Currency = "EUR",
            CreditsAdded = secondsToAdd
        });
    }

    private static IResult GetAvailableDayPassesAsync()
    {
        var passes = new[]
        {
            new DayPassOptionDto("basic_24h", "Basic 24-Hour Pass", "Unlimited consultations for 24 hours",
                24, 49.99m, "EUR", 5, false, ["General Practice", "Mental Health"]),
            new DayPassOptionDto("premium_24h", "Premium 24-Hour Pass", "Unlimited consultations including specialists",
                24, 79.99m, "EUR", 10, false, ["General Practice", "Mental Health", "Cardiology", "Dermatology", "Pediatrics"]),
            new DayPassOptionDto("unlimited_day", "Unlimited Day Pass", "True unlimited consultations for 24 hours",
                24, 149.99m, "EUR", int.MaxValue, true, ["All Specialties"])
        };
        return Results.Ok(passes);
    }

    private static async Task<IResult> GetMyDayPassesAsync(
        ClaimsPrincipal user,
        PlatformDbContext db,
        string? status)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue) return Results.Unauthorized();

        // Day passes are stored as WalletPayments referencing day pass products
        var wallet = await db.Wallets.FirstOrDefaultAsync(w => w.PatientAccountId == userId.Value);
        if (wallet is null) return Results.Ok(new { Items = Array.Empty<object>(), TotalCount = 0 });

        var payments = await db.WalletPayments
            .Where(p => p.WalletId == wallet.Id)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return Results.Ok(new { Items = payments, TotalCount = payments.Count });
    }

    private static async Task<IResult> PurchaseDayPassAsync(
        PurchaseDayPassRequestDto request,
        ClaimsPrincipal user,
        IWalletService walletService,
        PlatformDbContext db)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue) return Results.Unauthorized();

        // Deduct from wallet
        var priceSeconds = (long)(request.Price * SecondsPerEur);
        var deducted = await walletService.DeductCreditAsync(userId.Value, priceSeconds * 100);
        if (!deducted)
            return Results.UnprocessableEntity(new { Message = "Insufficient wallet balance to purchase day pass." });

        return Results.Ok(new
        {
            Message = "Day pass purchased successfully",
            PassType = request.PassType,
            ActivatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(request.DurationHours),
            Charged = new MoneyDto(request.Price, request.Currency)
        });
    }

    private static async Task<IResult> GetDayPassDetailsAsync(
        Guid dayPassId,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue) return Results.Unauthorized();

        return Results.NotFound(new { Message = "Day pass not found." });
    }

    private static Guid? GetUserId(ClaimsPrincipal user)
    {
        var raw = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;
        return Guid.TryParse(raw, out var id) ? id : null;
    }
}

// DTOs
public record MoneyDto(decimal Amount, string Currency);

public record WalletDto(
    Guid Id,
    Guid PatientAccountId,
    decimal BalanceEur,
    string Currency,
    long AvailableSeconds,
    bool IsActive,
    DateTimeOffset CreatedAt);

public record WalletTransactionDto(
    Guid Id,
    string EntryType,
    decimal AmountEur,
    string Currency,
    decimal BalanceAfterEur,
    string? Description,
    string? ReferenceId,
    DateTimeOffset CreatedAt);

public record DepositRequestDto(decimal Amount, string Currency, string PaymentMethod);

public record DayPassOptionDto(
    string Id,
    string Name,
    string Description,
    int DurationHours,
    decimal Price,
    string Currency,
    int MaxConsultations,
    bool IsUnlimited,
    List<string> IncludedSpecialties);

public record PurchaseDayPassRequestDto(
    string PassType,
    int DurationHours,
    decimal Price,
    string Currency,
    int MaxConsultations,
    string PaymentMethod);
