// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.IO;
using SkiaSharpChartEngine;
using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.Examples;

/// <summary>
/// Pie chart example: Visualize composition/distribution
/// </summary>
public class PieChartExample
{
    public static void Main()
    {
        Console.WriteLine("Creating pie chart...");

        var engine = ChartEngine.Create();

        var chart = new Chart(ChartType.PieChart)
        {
            Configuration = new ChartConfiguration
            {
                Title = "Market Share Distribution",
                Width = 700,
                Height = 700,
                BackgroundColor = "#F5F5F5"
            }
        };

        var companies = new[]
        {
            ("CompanyA", 35.0, "#FF6B6B"),
            ("CompanyB", 28.0, "#4ECDC4"),
            ("CompanyC", 22.0, "#F7DC6F"),
            ("CompanyD", 15.0, "#95E1D3")
        };

        foreach (var (name, percentage, color) in companies)
        {
            var series = new ChartSeries(name, color);
            series.AddDataPoint(1, percentage);
            chart.AddSeries(series);
        }

        var result = engine.RenderChart(chart);

        if (result.IsSuccessful)
        {
            var outputPath = Path.Combine("output", "pie-chart.png");
            Directory.CreateDirectory("output");
            File.WriteAllBytes(outputPath, result.Data as byte[]);
            Console.WriteLine($"✓ Pie chart saved to {outputPath}");
            Console.WriteLine($"  Render time: {result.RenderTimeMs}ms");
            Console.WriteLine($"  Segments: {chart.Series.Count}");
        }
        else
        {
            Console.WriteLine($"✗ Error: {result.ErrorMessage}");
        }
    }
}
