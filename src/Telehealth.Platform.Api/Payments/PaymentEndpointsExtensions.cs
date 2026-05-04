using Telehealth.Platform.Api.Payments;

namespace Telehealth.Platform.Api;

public static class PaymentEndpointsExtensions
{
    public static IEndpointRouteBuilder MapPaymentEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapControllers();
        return endpoints;
    }
}