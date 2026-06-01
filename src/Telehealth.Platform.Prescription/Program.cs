using Microsoft.EntityFrameworkCore;
using Telehealth.Platform.Prescription.Data;
using Telehealth.Platform.Prescription.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();
