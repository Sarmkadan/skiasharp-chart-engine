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
/// Multi-series example: Compare multiple data series in one chart
/// </summary>
public class MultiSeriesComparisonExample
{
    public static void Main()
    {
        Console.WriteLine("Creating multi-series comparison chart...");

        var engine = ChartEngine.Create();

        var chart = new Chart(ChartType.LineChart)
        {
            Configuration = new ChartConfiguration
            {
                Title = "Sales Performance - 2025 vs 2026",
                Width = 1000,
                Height = 600,
                XAxisLabel = "Month",
                YAxisLabel = "Revenue ($)",
                ShowGrid = true,
                ShowLegend = true,
                MarginTop = 60,
                MarginBottom = 60,
                MarginLeft = 70,
                MarginRight = 70
            }
        };

        var data2025 = new[] { 45000, 48000, 52000, 50000, 55000, 58000,
                              61000, 59000, 63000, 65000, 68000, 72000 };

        var data2026 = new[] { 52000, 55000, 58000, 62000, 65000, 70000,
                              72000, 75000, 78000, 82000, 85000, 90000 };

        var series2025 = new ChartSeries("2025", "#FF6B6B");
        for (int month = 1; month <= data2025.Length; month++)
        {
            series2025.AddDataPoint(month, data2025[month - 1]);
        }

        var series2026 = new ChartSeries("2026", "#4ECDC4");
        for (int month = 1; month <= data2026.Length; month++)
        {
            series2026.AddDataPoint(month, data2026[month - 1]);
        }

        chart.AddSeries(series2025);
        chart.AddSeries(series2026);

        var result = engine.RenderChart(chart);

        if (result.IsSuccessful)
        {
            var outputPath = Path.Combine("output", "multi-series-chart.png");
            Directory.CreateDirectory("output");
            File.WriteAllBytes(outputPath, result.Data as byte[]);
            Console.WriteLine($"✓ Chart saved to {outputPath}");
            Console.WriteLine($"  Render time: {result.RenderTimeMs}ms");
            Console.WriteLine($"  Series count: {chart.Series.Count}");
            Console.WriteLine($"  Total data points: {chart.GetTotalDataPoints()}");
        }
        else
        {
            Console.WriteLine($"✗ Error: {result.ErrorMessage}");
        }
    }
}
