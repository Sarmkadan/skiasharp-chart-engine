using Microsoft.Extensions.DependencyInjection;
using SkiaSharpChartEngine;
using SkiaSharpChartEngine.Models;

// This example shows how to register the chart engine in an ASP.NET Core DI container.

var services = new ServiceCollection();

// Register the engine and necessary services
services.AddSkiaSharpChartEngine(options =>
{
    options.CacheEnabled = true;
    options.CacheDurationSeconds = 300;
});

var provider = services.BuildServiceProvider();

// Resolve the engine for use
var engine = provider.GetRequiredService<ChartEngine>();

// Now you can use 'engine' to render charts within your services or controllers
var chart = new Chart(ChartType.LineChart) { /* ... */ };
var result = engine.RenderChart(chart);
