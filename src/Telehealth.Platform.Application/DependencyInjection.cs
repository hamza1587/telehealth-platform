using Microsoft.Extensions.DependencyInjection;

namespace Telehealth.Platform.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services;
    }
}
