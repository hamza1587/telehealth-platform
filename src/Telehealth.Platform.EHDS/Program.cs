using Microsoft.EntityFrameworkCore;
using Telehealth.Platform.EHDS.Services;
using Telehealth.Platform.EHDS;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register DbContext
builder.Services.AddDbContext<EhdsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("EhdsDatabase")));

// Register EHDS services
builder.Services.AddScoped<IPatientHealthRecordService, PatientHealthRecordService>();
builder.Services.AddScoped<IResearchExportService, ResearchExportService>();
builder.Services.AddScoped<IDeIdentificationService, DeIdentificationService>();

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
