using Microsoft.EntityFrameworkCore;
using Telehealth.Platform.Integrations.Healthcare.Services;
using Telehealth.Platform.Integrations.Healthcare;
using Telehealth.Platform.Shared;

var DocsHtml = ApiDocs.GetHtml("Telehealth Platform - Healthcare Integration API");

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Register DbContext
builder.Services.AddDbContext<HealthcareIntegrationsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("HealthcareIntegrationsDatabase")));

// Register healthcare integration services
builder.Services.AddScoped<ILabIntegrationService, LabIntegrationService>();
builder.Services.AddScoped<IInsuranceIntegrationService, InsuranceIntegrationService>();

// Add HttpClient
builder.Services.AddHttpClient();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi("/openapi/{name}/document.json");
    app.MapGet("/docs", () => Results.Content(DocsHtml, "text/html"));
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();
