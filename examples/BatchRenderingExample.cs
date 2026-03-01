// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SkiaSharpChartEngine;
using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.Examples;

/// <summary>
/// Batch rendering example: Efficiently render multiple charts in parallel
/// </summary>
public class BatchRenderingExample
{
    public static async Task Main()
    {
        Console.WriteLine("Rendering batch of charts in parallel...\n");

        var engine = ChartEngine.Create();
        Directory.CreateDirectory("output");

        var charts = GenerateSampleCharts(5);
        Console.WriteLine($"Generated {charts.Count} charts for rendering\n");

        var stopwatch = Stopwatch.StartNew();

        var renderTasks = charts.Select(async chart =>
        {
            var result = await engine.RenderChartAsync(chart);
            return (Chart: chart, Result: result);
        }).ToList();

        var results = await Task.WhenAll(renderTasks);
        stopwatch.Stop();

        Console.WriteLine("=== BATCH RENDER RESULTS ===\n");

        int successful = 0;
        long totalRenderTime = 0;

        foreach (var (chart, result) in results)
        {
            if (result.IsSuccessful)
            {
                var outputPath = Path.Combine("output", $"{chart.Id}-batch.png");
                File.WriteAllBytes(outputPath, result.Data as byte[]);
                Console.WriteLine($"✓ {chart.Configuration.Title} ({result.RenderTimeMs}ms)");
                successful++;
                totalRenderTime += result.RenderTimeMs;
            }
            else
            {
                Console.WriteLine($"✗ {chart.Configuration.Title}: {result.ErrorMessage}");
            }
        }

        Console.WriteLine($"\n=== SUMMARY ===");
        Console.WriteLine($"Total time: {stopwatch.ElapsedMilliseconds}ms");
        Console.WriteLine($"Successful: {successful}/{charts.Count}");
        Console.WriteLine($"Average render time: {totalRenderTime / Math.Max(successful, 1)}ms");
        Console.WriteLine($"Charts saved to output/ directory\n");
    }

    private static List<Chart> GenerateSampleCharts(int count)
    {
        var charts = new List<Chart>();
        var random = new Random(42);

        for (int i = 0; i < count; i++)
        {
            var chart = new Chart(ChartType.LineChart)
            {
                Configuration = new ChartConfiguration
                {
                    Title = $"Report {i + 1} - Daily Performance",
                    Width = 800,
                    Height = 500,
                    XAxisLabel = "Day",
                    YAxisLabel = "Value"
                }
            };

            var seriesCount = random.Next(1, 4);

            for (int s = 0; s < seriesCount; s++)
            {
                var series = new ChartSeries($"Series {s + 1}", GetRandomColor(s));

                for (int day = 1; day <= 30; day++)
                {
                    series.AddDataPoint(day, random.Next(1000, 10000));
                }

                chart.AddSeries(series);
            }

            charts.Add(chart);
        }

        return charts;
    }

    private static string GetRandomColor(int index)
    {
        var colors = new[]
        {
            "#FF6B6B",
            "#4ECDC4",
            "#F7DC6F",
            "#95E1D3",
            "#C8B8FF"
        };

        return colors[index % colors.Length];
    }
}
