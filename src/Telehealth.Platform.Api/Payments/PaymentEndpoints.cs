using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Telehealth.Platform.Application.Abstractions.Payments;
using Telehealth.Platform.Domain.Common;
using Telehealth.Platform.Domain.Payments;

namespace Telehealth.Platform.Api.Payments;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class PaymentEndpoints : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentEndpoints(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost]
    public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequest request)
    {
        var patientAccountId = User.FindFirst("patient_account_id")?.Value;
        if (string.IsNullOrEmpty(patientAccountId) || !Guid.TryParse(patientAccountId, out var patientId))
        {
            return Unauthorized();
        }

        var payment = await _paymentService.CreatePaymentAsync(
            patientId,
            request.PaymentMethodId,
            request.AmountMinor,
            request.Currency,
            request.Description,
            HttpContext.RequestAborted);

        return CreatedAtAction(nameof(GetPayment), new { id = payment.Id }, payment);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPayment(Guid id)
    {
        var payment = await _paymentService.GetPaymentAsync(id, HttpContext.RequestAborted);
        if (payment == null)
        {
            return NotFound();
        }
        return Ok(payment);
    }

    [HttpGet]
    public async Task<IActionResult> GetPayments([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var patientAccountId = User.FindFirst("patient_account_id")?.Value;
        if (string.IsNullOrEmpty(patientAccountId) || !Guid.TryParse(patientAccountId, out var patientId))
        {
            return Unauthorized();
        }

        var payments = await _paymentService.GetPaymentsByPatientAsync(patientId, HttpContext.RequestAborted);
        return Ok(payments.Skip((page - 1) * pageSize).Take(pageSize));
    }

    [HttpPost("{id}/process")]
    public async Task<IActionResult> ProcessPayment(Guid id)
    {
        var result = await _paymentService.ProcessPaymentAsync(id, HttpContext.RequestAborted);
        return Ok(result);
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelPayment(Guid id, [FromBody] CancelPaymentRequest request)
    {
        var success = await _paymentService.CancelPaymentAsync(id, request.Reason, HttpContext.RequestAborted);
        if (!success)
        {
            return BadRequest();
        }
        return Ok();
    }

    [HttpPost("{id}/refund")]
    public async Task<IActionResult> CreateRefund(Guid id, [FromBody] CreateRefundRequest request)
    {
        var refund = await _paymentService.CreateRefundAsync(
            id,
            request.AmountMinor,
            request.Currency,
            request.Reason,
            HttpContext.RequestAborted);

        return CreatedAtAction(nameof(CreateRefund), new { id = refund.Id }, refund);
    }

    [HttpPost("{id}/dispute")]
    public async Task<IActionResult> CreateDispute(Guid id, [FromBody] CreateDisputeRequest request)
    {
        var dispute = await _paymentService.CreateDisputeAsync(
            id,
            request.Reason,
            request.ReasonDescription,
            request.Evidence,
            HttpContext.RequestAborted);

        return CreatedAtAction(nameof(CreateDispute), new { id = dispute.Id }, dispute);
    }

    [HttpPost("methods")]
    public async Task<IActionResult> AddPaymentMethod([FromBody] AddPaymentMethodRequest request)
    {
        var patientAccountId = User.FindFirst("patient_account_id")?.Value;
        if (string.IsNullOrEmpty(patientAccountId) || !Guid.TryParse(patientAccountId, out var patientId))
        {
            return Unauthorized();
        }

        var paymentMethod = await _paymentService.AddPaymentMethodAsync(
            patientId,
            request.Type,
            request.Provider,
            request.Token,
            request.LastFour,
            request.Brand,
            request.ExpiryMonth,
            request.IsDefault,
            HttpContext.RequestAborted);

        return CreatedAtAction(nameof(GetPaymentMethods), paymentMethod);
    }

    [HttpGet("methods")]
    public async Task<IActionResult> GetPaymentMethods()
    {
        var patientAccountId = User.FindFirst("patient_account_id")?.Value;
        if (string.IsNullOrEmpty(patientAccountId) || !Guid.TryParse(patientAccountId, out var patientId))
        {
            return Unauthorized();
        }

        var methods = await _paymentService.GetPaymentMethodsAsync(patientId, HttpContext.RequestAborted);
        return Ok(methods);
    }

    [HttpGet("balance")]
    public async Task<IActionResult> GetBalance()
    {
        var patientAccountId = User.FindFirst("patient_account_id")?.Value;
        if (string.IsNullOrEmpty(patientAccountId) || !Guid.TryParse(patientAccountId, out var patientId))
        {
            return Unauthorized();
        }

        var balance = await _paymentService.GetBalanceAsync(patientId, HttpContext.RequestAborted);
        return Ok(new { balance = balance });
    }
}

public class CreatePaymentRequest
{
    public Guid? PaymentMethodId { get; set; }
    public long AmountMinor { get; set; }
    public CurrencyCode Currency { get; set; } = CurrencyCode.USD;
    public string Description { get; set; } = string.Empty;
}

public class CancelPaymentRequest
{
    public string Reason { get; set; } = string.Empty;
}

public class CreateRefundRequest
{
    public long AmountMinor { get; set; }
    public string Currency { get; set; } = "USD";
    public string Reason { get; set; } = string.Empty;
}

public class CreateDisputeRequest
{
    public DisputeReason Reason { get; set; }
    public string ReasonDescription { get; set; } = string.Empty;
    public string Evidence { get; set; } = string.Empty;
}

public class AddPaymentMethodRequest
{
    public PaymentMethodType Type { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string LastFour { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public DateTimeOffset ExpiryMonth { get; set; }
    public bool IsDefault { get; set; }
}