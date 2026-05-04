using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Telehealth.Platform.Application.Abstractions.Payments;

namespace Telehealth.Platform.Api.Payments;

[ApiController]
[Route("api/v1/webhooks/payments")]
public class WebhookEndpoints : ControllerBase
{
    private readonly IPaymentGateway _paymentGateway;
    private readonly ILogger<WebhookEndpoints> _logger;

    public WebhookEndpoints(
        IPaymentGateway paymentGateway,
        ILogger<WebhookEndpoints> logger)
    {
        _paymentGateway = paymentGateway;
        _logger = logger;
    }

    [HttpPost("stripe")]
    public async Task<IActionResult> HandleStripeWebhook()
    {
        var signature = Request.Headers["Stripe-Signature"].FirstOrDefault() ?? string.Empty;
        var payload = await new StreamReader(Request.Body).ReadToEndAsync();

        var isValid = await _paymentGateway.ValidateWebhookAsync(signature, payload, HttpContext.RequestAborted);
        if (!isValid)
        {
            _logger.LogWarning("Invalid webhook signature received");
            return Unauthorized();
        }

        var webhookEvent = await _paymentGateway.ProcessWebhookAsync(signature, payload, HttpContext.RequestAborted);

        _logger.LogInformation(
            "Webhook processed: {EventType} for payment {PaymentId}",
            webhookEvent.EventType,
            webhookEvent.PaymentId);

        return Ok(new { received = true });
    }
}