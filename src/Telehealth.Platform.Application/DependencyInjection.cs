using Microsoft.Extensions.DependencyInjection;
using Telehealth.Platform.Application.Abstractions.Consent;
using Telehealth.Platform.Application.Abstractions.Identity;
using Telehealth.Platform.Application.Abstractions.Payments;
using Telehealth.Platform.Application.Payments;

namespace Telehealth.Platform.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IDeviceManagementService>();
        services.AddScoped<IConsentService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IFraudDetectionService, FraudDetectionService>();
        return services;
    }
}