using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Telehealth.Platform.Application.Abstractions.ClinicalRecords;
using Telehealth.Platform.Integrations.Medplum.Options;

namespace Telehealth.Platform.Integrations.Medplum;

public static class DependencyInjection
{
    public static IServiceCollection AddMedplumIntegration(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<MedplumOptions>()
            .Bind(configuration.GetSection(MedplumOptions.SectionName))
            .Validate(options => Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out _), "Medplum BaseUrl must be an absolute URL.")
            .Validate(options => options.TimeoutSeconds > 0, "Medplum TimeoutSeconds must be greater than zero.")
            .ValidateOnStart();

        services.AddHttpClient<IClinicalRecordGateway, MedplumClinicalRecordGateway>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<MedplumOptions>>().Value;

            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        });

        return services;
    }
}
