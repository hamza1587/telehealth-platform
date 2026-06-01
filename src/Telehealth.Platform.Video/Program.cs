using Microsoft.EntityFrameworkCore;
using Telehealth.Platform.Video.Data;
using Telehealth.Platform.Video.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register DbContext
builder.Services.AddDbContext<VideoDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("VideoDatabase")));

// Register video services
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
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();
