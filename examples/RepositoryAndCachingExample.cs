// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Diagnostics;
using System.IO;
using SkiaSharpChartEngine;
using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.Examples;

/// <summary>
/// Repository and caching example: Store charts and demonstrate cache benefits
/// </summary>
public class RepositoryAndCachingExample
{
    public static void Main()
    {
        Console.WriteLine("Demonstrating repository and caching functionality...\n");

        var engine = ChartEngine.Create();

        var chart = CreateSampleChart();

        Console.WriteLine("=== REPOSITORY OPERATIONS ===\n");

        var chartId = engine.SaveChart(chart);
        Console.WriteLine($"✓ Chart saved with ID: {chartId}\n");

        var retrievedChart = engine.GetChart(chartId);
        Console.WriteLine($"✓ Chart retrieved: {retrievedChart?.Id}");
        Console.WriteLine($"  Series: {retrievedChart?.Series.Count}");
        Console.WriteLine($"  Data points: {retrievedChart?.GetTotalDataPoints()}\n");

        retrievedChart?.Series[0].AddDataPoint(13, 3100);
        var updated = engine.UpdateChart(retrievedChart);
        Console.WriteLine($"✓ Chart updated: {updated}\n");

        Console.WriteLine("=== CACHING PERFORMANCE ===\n");

        var stopwatch = Stopwatch.StartNew();
        var result1 = engine.RenderChart(chart);
        stopwatch.Stop();
        Console.WriteLine($"First render (cache miss): {stopwatch.ElapsedMilliseconds}ms");

        stopwatch.Restart();
        var result2 = engine.RenderChart(chart);
        stopwatch.Stop();
        Console.WriteLine($"Second render (cache hit): {stopwatch.ElapsedMilliseconds}ms");

        var speedup = (double)result1.RenderTimeMs / (result2.RenderTimeMs > 0 ? result2.RenderTimeMs : 1);
        Console.WriteLine($"Speed improvement: {speedup:F1}x faster from cache\n");

        Console.WriteLine("=== CACHE INVALIDATION ===\n");

        chart.Series[0].AddDataPoint(13, 3200);
        Console.WriteLine("✓ Chart modified (cache invalidated)");

        stopwatch.Restart();
        var result3 = engine.RenderChart(chart);
        stopwatch.Stop();
        Console.WriteLine($"Third render (cache miss): {stopwatch.ElapsedMilliseconds}ms\n");

        Console.WriteLine("=== CLEANUP ===\n");

        var deleted = engine.DeleteChart(chartId);
        Console.WriteLine($"✓ Chart deleted: {deleted}\n");

        if (result1.IsSuccessful)
        {
            var outputPath = Path.Combine("output", "caching-example.png");
            Directory.CreateDirectory("output");
            File.WriteAllBytes(outputPath, result1.Data as byte[]);
            Console.WriteLine($"✓ Sample chart saved to {outputPath}");
        }
    }

    private static Chart CreateSampleChart()
    {
        var chart = new Chart(ChartType.LineChart)
        {
            Configuration = new ChartConfiguration
            {
                Title = "Performance Metrics",
                Width = 800,
                Height = 600,
                XAxisLabel = "Hour",
                YAxisLabel = "Requests/sec"
            }
        };

        var series = new ChartSeries("API Requests", "#FF6B6B");
        var hourlyData = new[] { 500, 620, 580, 750, 920, 1100, 980, 1250, 1400, 1200, 1350, 1500 };

        for (int hour = 1; hour <= hourlyData.Length; hour++)
        {
            series.AddDataPoint(hour, hourlyData[hour - 1]);
        }

        chart.AddSeries(series);
        return chart;
    }
}
