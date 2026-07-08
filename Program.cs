using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SkiaSharpChartEngine;
using SkiaSharpChartEngine.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddSkiaSharpChartEngine(options =>
{
    options.CacheEnabled = true;
    options.CacheDurationSeconds = builder.Configuration.GetValue<int?>("Cache:DurationSeconds") ?? 300;
    options.MaxConcurrentRenders = builder.Configuration.GetValue<int?>("Performance:MaxConcurrentRenders") ?? 10;
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register HealthCheckService for health monitoring
builder.Services.AddSingleton<SkiaSharpChartEngine.Diagnostics.HealthCheckService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();