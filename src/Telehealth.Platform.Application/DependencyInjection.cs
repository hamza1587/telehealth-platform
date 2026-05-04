using Microsoft.Extensions.DependencyInjection;
using Telehealth.Platform.Application.Abstractions.Consent;
using Telehealth.Platform.Application.Abstractions.Identity;

namespace Telehealth.Platform.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IDeviceManagementService>();
        services.AddScoped<IConsentService>();
        return services;
    }
}