using Microsoft.Extensions.DependencyInjection;
using Telehealth.Platform.Application.Abstractions.Payments;

namespace Telehealth.Platform.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Payment services in Application layer (not Infrastructure)
        services.AddScoped<IPaymentService, Payments.PaymentService>();
        services.AddScoped<IFraudDetectionService, Payments.FraudDetectionService>();
        
        return services;
    }
}