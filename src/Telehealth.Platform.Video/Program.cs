using Microsoft.EntityFrameworkCore;
using Telehealth.Platform.Video.Data;
using Telehealth.Platform.Video.Services;
using Telehealth.Platform.Shared;

var DocsHtml = ApiDocs.GetHtml("Telehealth Platform - Video Service API");

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Register DbContext
builder.Services.AddDbContext<VideoDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("VideoDatabase")));

// Register video services
builder.Services.AddSingleton<ILiveKitTokenService, LiveKitTokenService>();
builder.Services.AddScoped<IVideoService, VideoService>();
builder.Services.AddScoped<IRecordingService, RecordingService>();
builder.Services.AddSingleton<INetworkQualityService, NetworkQualityService>();

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

// Ensure schema is up to date on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<VideoDbContext>();
    db.Database.EnsureCreated();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();
