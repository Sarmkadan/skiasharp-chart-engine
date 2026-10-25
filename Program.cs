// <summary>
// Entry point for the SkiaSharpChartEngine ASP.NET Core Web API host.
// Startup flow (top-level statements):
// 1. Create the WebApplicationBuilder from args, which loads configuration and environment settings.
// 2. Register the chart engine via AddSkiaSharpChartEngine. Caching is always enabled. Cache duration is read from
//    Cache:DurationSeconds (default 300) and render concurrency from Performance:MaxConcurrentRenders (default 10).
// 3. Register MVC controllers, endpoint API exploration and Swagger generation.
// 4. Register HealthCheckService as a singleton for health monitoring.
// 5. Build the application and configure the HTTP pipeline: Swagger UI only in the Development environment,
//    then HTTPS redirection, authorization and controller route mapping.
// 6. Run the host until it is shut down.
// </summary>
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