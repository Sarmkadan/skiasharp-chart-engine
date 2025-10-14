// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SkiaSharpChartEngine;
using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.Examples;

/// <summary>
/// Async operations example: Asynchronous rendering and export to multiple formats
/// </summary>
public class AsyncExportExample
{
    public static async Task Main()
    {
        Console.WriteLine("Creating chart and exporting to multiple formats...\n");

        var engine = ChartEngine.Create();

        var chart = new Chart(ChartType.ColumnChart)
        {
            Configuration = new ChartConfiguration
            {
                Title = "Product Sales by Region",
                Width = 900,
                Height = 500,
                XAxisLabel = "Region",
                YAxisLabel = "Sales ($)",
                ShowGrid = true
            }
        };

        var regions = new[] { "North", "South", "East", "West", "Central" };
        var sales = new[] { 45000, 52000, 38000, 61000, 48000 };

        for (int i = 0; i < regions.Length; i++)
        {
            var series = new ChartSeries(regions[i], GetColorForRegion(i));
            series.AddDataPoint(i + 1, sales[i]);
            chart.AddSeries(series);
        }

        Directory.CreateDirectory("output");

        var exportFormats = new[] { ExportFormat.PNG, ExportFormat.SVG, ExportFormat.PDF };
        var tasks = new List<Task>();

        foreach (var format in exportFormats)
        {
            var task = ExportChartAsync(engine, chart, format);
            tasks.Add(task);
        }

        await Task.WhenAll(tasks);

        Console.WriteLine("\n✓ All exports completed!");
    }

    private static async Task ExportChartAsync(ChartEngine engine, Chart chart, ExportFormat format)
    {
        Console.WriteLine($"Exporting to {format}...");

        var options = new ExportOptions($"sales_{format.ToString().ToLower()}", format, "output/");
        var result = await engine.ExportChartAsync(chart, options);

        if (result.IsSuccessful)
        {
            Console.WriteLine($"  ✓ Saved to {result.Output}");
            Console.WriteLine($"    Render time: {result.RenderTimeMs}ms\n");
        }
        else
        {
            Console.WriteLine($"  ✗ Error: {result.ErrorMessage}\n");
        }
    }

    private static string GetColorForRegion(int index)
    {
        return index switch
        {
            0 => "#FF6B6B",   // Red
            1 => "#4ECDC4",   // Teal
            2 => "#F7DC6F",   // Yellow
            3 => "#95E1D3",   // Mint
            _ => "#C8B8FF"    // Purple
        };
    }
}
