using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Telehealth.Platform.Domain.Billing;

namespace Telehealth.Platform.Api.Billing;

/// <summary>
/// Consultation billing API endpoints.
/// </summary>
public static class BillingEndpoints
{
    public static IEndpointRouteBuilder MapBillingEndpoints(this IEndpointRouteBuilder app)
    {
        var billing = app.MapGroup("/billing").WithTags("Billing");

        // Patient endpoints
        billing.MapGet("/my-bills", GetMyBillsAsync).RequireAuthorization("RequirePatient");
        billing.MapGet("/{billingId}", GetBillDetailsAsync).RequireAuthorization();
        billing.MapGet("/{billingId}/receipt", GetReceiptAsync).RequireAuthorization();

        // Doctor endpoints
        billing.MapGet("/doctor/earnings", GetDoctorEarningsAsync).RequireAuthorization("RequireDoctor");
        billing.MapGet("/doctor/payouts", GetDoctorPayoutsAsync).RequireAuthorization("RequireDoctor");

        // Admin endpoints
        billing.MapGet("/admin/consultations", GetAllBillsAsync).RequireAuthorization("RequireAdmin");
        billing.MapPost("/admin/{billingId}/refund", ProcessRefundAsync).RequireAuthorization("RequireAdmin");
        billing.MapGet("/admin/fee-structure", GetFeeStructureAsync).RequireAuthorization("RequireAdmin");
        billing.MapPut("/admin/fee-structure", UpdateFeeStructureAsync).RequireAuthorization("RequireAdmin");

        return app;
    }

    private static async Task<IResult> GetMyBillsAsync(
        ClaimsPrincipal user,
        string? status,
        DateTimeOffset? from,
        DateTimeOffset? to)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var bills = new List<BillSummaryDto>
        {
            new(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Dr. Jane Smith",
                "Cardiology",
                new MoneyDto(450m, "EUR"),
                BillingStatus.Paid,
                DateTimeOffset.UtcNow.AddDays(-2),
                DateTimeOffset.UtcNow.AddDays(-2),
                1800)
        };

        return Results.Ok(new { Items = bills, TotalCount = bills.Count });
    }

    private static async Task<IResult> GetBillDetailsAsync(
        Guid billingId,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var bill = new BillDetailDto(
            billingId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Dr. Jane Smith",
            "John Doe",
            "Cardiology",
            new MoneyDto(0.50m, "EUR"),
            1800,
            new MoneyDto(900m, "EUR"),
            new MoneyDto(180m, "EUR"),
            new MoneyDto(20m, "EUR"),
            new MoneyDto(700m, "EUR"),
            new MoneyDto(900m, "EUR"),
            BillingStatus.Paid,
            "Credit Card",
            "txn_123456",
            DateTimeOffset.UtcNow.AddDays(-2),
            DateTimeOffset.UtcNow.AddDays(-2),
            null,
            null,
            null);

        return Results.Ok(bill);
    }

    private static async Task<IResult> GetReceiptAsync(
        Guid billingId,
        ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var receipt = new ReceiptDto(
            billingId,
            "RCP-2024-001",
            DateTimeOffset.UtcNow.AddDays(-2),
            "John Doe",
            "Dr. Jane Smith",
            "Cardiology Consultation",
            new MoneyDto(900m, "EUR"),
            new MoneyDto(180m, "EUR"),
            new MoneyDto(20m, "EUR"),
            new MoneyDto(900m, "EUR"),
            "Credit Card",
            "Paid",
            "https://api.example.com/receipts/RCP-2024-001.pdf");

        return Results.Ok(receipt);
    }

    private static async Task<IResult> GetDoctorEarningsAsync(
        ClaimsPrincipal user,
        DateTimeOffset? from,
        DateTimeOffset? to)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var earnings = new DoctorEarningsDto(
            new MoneyDto(12500m, "EUR"),
            new MoneyDto(3500m, "EUR"),
            new MoneyDto(9000m, "EUR"),
            45,
            new MoneyDto(200m, "EUR"),
            new MoneyDto(500m, "EUR"),
            from ?? DateTimeOffset.UtcNow.AddDays(-30),
            to ?? DateTimeOffset.UtcNow);

        return Results.Ok(earnings);
    }

    private static async Task<IResult> GetDoctorPayoutsAsync(
        ClaimsPrincipal user,
        int? page,
        int? pageSize)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var payouts = new List<PayoutDto>
        {
            new(
                Guid.NewGuid(),
                new MoneyDto(3000m, "EUR"),
                PayoutStatus.Completed,
                DateTimeOffset.UtcNow.AddDays(-7),
                DateTimeOffset.UtcNow.AddDays(-5),
                "Bank Transfer",
                "TXN-789012")
        };

        return Results.Ok(new { Items = payouts, TotalCount = payouts.Count });
    }

    private static async Task<IResult> GetAllBillsAsync(
        string? status,
        DateTimeOffset? from,
        DateTimeOffset? to,
        int? page,
        int? pageSize)
    {
        var bills = new List<AdminBillDto>
        {
            new(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "John Doe",
                Guid.NewGuid(),
                "Dr. Jane Smith",
                new MoneyDto(900m, "EUR"),
                BillingStatus.Paid,
                DateTimeOffset.UtcNow.AddDays(-2),
                "Credit Card")
        };

        return Results.Ok(new { Items = bills, TotalCount = bills.Count });
    }

    private static async Task<IResult> ProcessRefundAsync(
        Guid billingId,
        ProcessRefundRequestDto request)
    {
        return Results.Ok(new
        {
            BillingId = billingId,
            RefundAmount = request.Amount,
            Reason = request.Reason,
            ProcessedAt = DateTimeOffset.UtcNow,
            Status = "Refunded"
        });
    }

    private static async Task<IResult> GetFeeStructureAsync()
    {
        var structure = new FeeStructureDto(
            Guid.NewGuid(),
            "Standard EU Fee Structure",
            20m,
            2.9m,
            new MoneyDto(0.50m, "EUR"),
            "EUR",
            null,
            true,
            DateTimeOffset.UtcNow.AddYears(-1));

        return Results.Ok(structure);
    }

    private static async Task<IResult> UpdateFeeStructureAsync(
        UpdateFeeStructureRequestDto request)
    {
        return Results.Ok(new { Message = "Fee structure updated successfully" });
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

public record BillSummaryDto(
    Guid Id,
    Guid ConsultationId,
    string DoctorName,
    string Specialty,
    MoneyDto TotalAmount,
    BillingStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? PaidAt,
    long DurationSeconds);

public record BillDetailDto(
    Guid Id,
    Guid ConsultationId,
    Guid PatientId,
    string DoctorName,
    string PatientName,
    string Specialty,
    MoneyDto PricePerSecond,
    long DurationSeconds,
    MoneyDto BaseAmount,
    MoneyDto PlatformFee,
    MoneyDto ProcessingFee,
    MoneyDto DoctorPayout,
    MoneyDto TotalCharged,
    BillingStatus Status,
    string? PaymentMethod,
    string? TransactionReference,
    DateTimeOffset CreatedAt,
    DateTimeOffset? PaidAt,
    DateTimeOffset? RefundedAt,
    MoneyDto? RefundAmount,
    string? RefundReason);

public record ReceiptDto(
    Guid BillingId,
    string ReceiptNumber,
    DateTimeOffset IssueDate,
    string PatientName,
    string DoctorName,
    string ServiceDescription,
    MoneyDto Subtotal,
    MoneyDto PlatformFee,
    MoneyDto Tax,
    MoneyDto Total,
    string PaymentMethod,
    string PaymentStatus,
    string PdfUrl);

public record DoctorEarningsDto(
    MoneyDto TotalRevenue,
    MoneyDto PlatformFees,
    MoneyDto NetEarnings,
    int ConsultationsCount,
    MoneyDto AveragePerConsultation,
    MoneyDto HighestSingleConsultation,
    DateTimeOffset PeriodStart,
    DateTimeOffset PeriodEnd);

public enum PayoutStatus
{
    Pending,
    Processing,
    Completed,
    Failed,
    Cancelled
}

public record PayoutDto(
    Guid Id,
    MoneyDto Amount,
    PayoutStatus Status,
    DateTimeOffset RequestedAt,
    DateTimeOffset? CompletedAt,
    string Method,
    string? TransactionReference);

public record AdminBillDto(
    Guid Id,
    Guid PatientId,
    string PatientName,
    Guid DoctorId,
    string DoctorName,
    MoneyDto Amount,
    BillingStatus Status,
    DateTimeOffset CreatedAt,
    string? PaymentMethod);

public record ProcessRefundRequestDto(
    decimal Amount,
    string Reason,
    bool NotifyPatient);

public record FeeStructureDto(
    Guid Id,
    string Name,
    decimal BasePlatformFeePercent,
    decimal? PaymentProcessingFeePercent,
    MoneyDto? FixedFeePerConsultation,
    string? CountryCode,
    string? DoctorTier,
    bool IsActive,
    DateTimeOffset EffectiveFrom);

public record UpdateFeeStructureRequestDto(
    string Name,
    decimal BasePlatformFeePercent,
    decimal? PaymentProcessingFeePercent,
    MoneyDto? FixedFeePerConsultation,
    bool IsActive);
