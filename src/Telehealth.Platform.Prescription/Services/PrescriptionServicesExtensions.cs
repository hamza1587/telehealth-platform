namespace Telehealth.Platform.Prescription.Services;

public static class PrescriptionServicesExtensions
{
    /// <summary>
    /// Registers all prescription gateway services.
    /// Call from your host's DI setup: builder.Services.AddPrescriptionGateways(builder.Configuration)
    /// </summary>
    public static IServiceCollection AddPrescriptionGateways(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpClient<DoseSpotGateway>(client =>
        {
            var baseUrl = configuration["DoseSpot:BaseUrl"] ?? "https://my.staging.dosespot.com";
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddHttpClient<HealthGorillaGateway>(client =>
        {
            var baseUrl = configuration["HealthGorilla:BaseUrl"] ?? "https://sandbox.healthgorilla.com";
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        return services;
    }
}
