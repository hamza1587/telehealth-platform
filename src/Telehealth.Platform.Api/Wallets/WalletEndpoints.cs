using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Telehealth.Platform.Domain.Wallets;

namespace Telehealth.Platform.Api.Wallets;

/// <summary>
/// Credit wallet and day pass API endpoints.
/// </summary>
public static class WalletEndpoints
{
    public static IEndpointRouteBuilder MapWalletEndpoints(this IEndpointRouteBuilder app)
    {
        var wallets = app.MapGroup("/wallets").WithTags("Wallets");

        // Wallet management
        wallets.MapGet("/my-wallet", GetMyWalletAsync).RequireAuthorization("RequirePatient");
        wallets.MapGet("/my-wallet/transactions", GetMyTransactionsAsync).RequireAuthorization("RequirePatient");
        wallets.MapPost("/my-wallet/deposit", DepositAsync).RequireAuthorization("RequirePatient");

        // Day pass management
        wallets.MapGet("/day-passes/available", GetAvailableDayPassesAsync).RequireAuthorization();
        wallets.MapGet("/my-day-passes", GetMyDayPassesAsync).RequireAuthorization("RequirePatient");
        wallets.MapPost("/day-passes/purchase", PurchaseDayPassAsync).RequireAuthorization("RequirePatient");
        wallets.MapGet("/my-day-passes/{dayPassId}", GetDayPassDetailsAsync).RequireAuthorization("RequirePatient");

        return app;
    }

    private static async Task<IResult> GetMyWalletAsync(
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var wallet = new WalletDto(
            Guid.NewGuid(),
            userId.Value,
            250.00m,
            "EUR",
            1250.00m,
            1000.00m,
            true,
            DateTimeOffset.UtcNow.AddMonths(-6));

        return Results.Ok(wallet);
    }

    private static async Task<IResult> GetMyTransactionsAsync(
        ClaimsPrincipal user,
        string? type,
        DateTimeOffset? from,
        DateTimeOffset? to,
        int? page,
        int? pageSize)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var transactions = new List<WalletTransactionDto>
        {
            new(
                Guid.NewGuid(),
                TransactionType.Deposit,
                100.00m,
                "EUR",
                350.00m,
                "Credit card deposit",
                "txn_12345",
                TransactionStatus.Completed,
                DateTimeOffset.UtcNow.AddDays(-5)),
            new(
                Guid.NewGuid(),
                TransactionType.Spend,
                -45.00m,
                "EUR",
                250.00m,
                "Consultation with Dr. Smith",
                "cons_67890",
                TransactionStatus.Completed,
                DateTimeOffset.UtcNow.AddDays(-2))
        };

        return Results.Ok(new { Items = transactions, TotalCount = transactions.Count });
    }

    private static async Task<IResult> DepositAsync(
        DepositRequestDto request,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var transaction = new WalletTransactionDto(
            Guid.NewGuid(),
            TransactionType.Deposit,
            request.Amount,
            request.Currency,
            350.00m,
            $"Deposit via {request.PaymentMethod}",
            Guid.NewGuid().ToString(),
            TransactionStatus.Completed,
            DateTimeOffset.UtcNow);

        return Results.Ok(new
        {
            Message = "Deposit processed",
            Transaction = transaction,
            NewBalance = 350.00m
        });
    }

    private static async Task<IResult> GetAvailableDayPassesAsync()
    {
        var passes = new List<DayPassOptionDto>
        {
            new(
                "basic_24h",
                "Basic 24-Hour Pass",
                "Unlimited consultations for 24 hours",
                24,
                49.99m,
                "EUR",
                5,
                false,
                new List<string> { "General Practice", "Mental Health" }),
            new(
                "premium_24h",
                "Premium 24-Hour Pass",
                "Unlimited consultations including specialists",
                24,
                79.99m,
                "EUR",
                10,
                false,
                new List<string> { "General Practice", "Mental Health", "Cardiology", "Dermatology", "Pediatrics" }),
            new(
                "unlimited_day",
                "Unlimited Day Pass",
                "True unlimited consultations for 24 hours",
                24,
                149.99m,
                "EUR",
                int.MaxValue,
                true,
                new List<string> { "All Specialties" })
        };

        return Results.Ok(passes);
    }

    private static async Task<IResult> GetMyDayPassesAsync(
        ClaimsPrincipal user,
        string? status)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var passes = new List<PatientDayPassDto>
        {
            new(
                Guid.NewGuid(),
                "Premium 24-Hour Pass",
                DayPassStatus.Active,
                DateTimeOffset.UtcNow.AddHours(-2),
                DateTimeOffset.UtcNow.AddHours(22),
                10,
                2,
                true,
                22 * 60)
        };

        return Results.Ok(new { Items = passes, TotalCount = passes.Count });
    }

    private static async Task<IResult> PurchaseDayPassAsync(
        PurchaseDayPassRequestDto request,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var dayPass = new PatientDayPassDto(
            Guid.NewGuid(),
            request.PassType,
            DayPassStatus.Active,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddHours(request.DurationHours),
            request.MaxConsultations,
            0,
            true,
            request.DurationHours * 60);

        return Results.Ok(new
        {
            Message = "Day pass purchased successfully",
            DayPass = dayPass,
            Charged = new MoneyDto(request.Price, request.Currency)
        });
    }

    private static async Task<IResult> GetDayPassDetailsAsync(
        Guid dayPassId,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var dayPass = new DayPassDetailDto(
            dayPassId,
            "Premium 24-Hour Pass",
            DayPassStatus.Active,
            DateTimeOffset.UtcNow.AddHours(-2),
            DateTimeOffset.UtcNow.AddHours(22),
            79.99m,
            "EUR",
            10,
            false,
            2,
            new List<string> { "General Practice", "Mental Health", "Cardiology", "Dermatology", "Pediatrics" },
            new List<DayPassUsageDto>
            {
                new(Guid.NewGuid(), "Dr. Jane Smith", "Cardiology", DateTimeOffset.UtcNow.AddHours(-1), 30)
            });

        return Results.Ok(dayPass);
    }

    private static Guid? GetUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user.FindFirst("sub")?.Value;

        if (Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }

        return null;
    }
}

// DTOs
public record MoneyDto(decimal Amount, string Currency);

public record WalletDto(
    Guid Id,
    Guid PatientAccountId,
    decimal Balance,
    string Currency,
    decimal TotalDeposited,
    decimal TotalSpent,
    bool IsActive,
    DateTimeOffset CreatedAt);

public record WalletTransactionDto(
    Guid Id,
    TransactionType Type,
    decimal Amount,
    string Currency,
    decimal BalanceAfter,
    string? Description,
    string? ReferenceId,
    TransactionStatus Status,
    DateTimeOffset CreatedAt);

public record DepositRequestDto(
    decimal Amount,
    string Currency,
    string PaymentMethod);

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

public record PatientDayPassDto(
    Guid Id,
    string PassType,
    DayPassStatus Status,
    DateTimeOffset ActivatedAt,
    DateTimeOffset ExpiresAt,
    int MaxConsultations,
    int ConsultationsUsed,
    bool IsValid,
    int MinutesRemaining);

public record PurchaseDayPassRequestDto(
    string PassType,
    int DurationHours,
    decimal Price,
    string Currency,
    int MaxConsultations,
    string PaymentMethod);

public record DayPassDetailDto(
    Guid Id,
    string PassType,
    DayPassStatus Status,
    DateTimeOffset ActivatedAt,
    DateTimeOffset ExpiresAt,
    decimal Price,
    string Currency,
    int MaxConsultations,
    bool IsUnlimited,
    int ConsultationsUsed,
    List<string> IncludedSpecialties,
    List<DayPassUsageDto> UsageHistory);

public record DayPassUsageDto(
    Guid ConsultationId,
    string DoctorName,
    string Specialty,
    DateTimeOffset UsedAt,
    int DurationMinutes);
