using Microsoft.EntityFrameworkCore;
using Telehealth.Platform.Prescription.Data;
using Telehealth.Platform.Prescription.Services;
using Telehealth.Platform.Shared;

var DocsHtml = ApiDocs.GetHtml("Telehealth Platform - Prescription Service API");

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Register HttpClient
builder.Services.AddHttpClient();

// Register DbContext
builder.Services.AddDbContext<PrescriptionDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("PrescriptionDatabase")));

// Register prescription services
builder.Services.AddScoped<IPrescriptionService, PrescriptionService>();
builder.Services.AddScoped<INationalPrescriptionGateway, NationalPrescriptionGateway>();
builder.Services.AddScoped<IDrugInteractionService, DrugInteractionService>();
builder.Services.AddScoped<IPharmacyService, PharmacyService>();

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
