using Microsoft.EntityFrameworkCore;
using Telehealth.Platform.Integrations.Healthcare.Services;
using Telehealth.Platform.Integrations.Healthcare;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();
