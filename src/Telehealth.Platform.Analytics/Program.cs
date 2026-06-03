using Microsoft.EntityFrameworkCore;
using Telehealth.Platform.Analytics.Data;
using Telehealth.Platform.Analytics.Services;
using Telehealth.Platform.Shared;

var DocsHtml = ApiDocs.GetHtml("Telehealth Platform - Analytics Service API");

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Register DbContext
builder.Services.AddDbContext<AnalyticsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AnalyticsDatabase")));

// Register analytics services
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddSingleton<IDataWarehouseService, DataWarehouseService>();
builder.Services.AddSingleton<IAggregationEngine, AggregationEngine>();
builder.Services.AddSingleton<IPredictiveModelingService, PredictiveModelingService>();
builder.Services.AddSingleton<IAlertingService, AlertingService>();

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
