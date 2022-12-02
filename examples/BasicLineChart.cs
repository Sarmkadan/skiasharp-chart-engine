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
/// Basic example: Create a simple line chart with one data series
/// </summary>
public class BasicLineChartExample
{
    public static void Main()
    {
        Console.WriteLine("Creating basic line chart...");

        var engine = ChartEngine.Create();

        var chart = new Chart(ChartType.LineChart)
        {
            Configuration = new ChartConfiguration
            {
                Title = "Website Traffic - Q1 2026",
                Width = 800,
                Height = 600,
                XAxisLabel = "Week",
                YAxisLabel = "Visitors",
                ShowGrid = true,
                ShowLegend = true,
                BackgroundColor = "#FFFFFF",
                AxisColor = "#333333",
                GridColor = "#E0E0E0",
                TextColor = "#000000"
            }
        };

        var series = new ChartSeries("Weekly Visitors", "#FF6B6B");

        var weeklyData = new[] { 1500, 2100, 1900, 2800, 3200, 2900, 3500 };

        for (int week = 1; week <= weeklyData.Length; week++)
        {
            series.AddDataPoint(week, weeklyData[week - 1]);
        }

        chart.AddSeries(series);

        var result = engine.RenderChart(chart);

        if (result.IsSuccessful)
        {
            var outputPath = Path.Combine("output", "basic-line-chart.png");
            Directory.CreateDirectory("output");
            File.WriteAllBytes(outputPath, result.Data as byte[]);
            Console.WriteLine($"✓ Chart saved to {outputPath}");
            Console.WriteLine($"  Render time: {result.RenderTimeMs}ms");
            Console.WriteLine($"  Chart ID: {result.ChartId}");
        }
        else
        {
            Console.WriteLine($"✗ Error: {result.ErrorMessage}");
        }
    }
}
