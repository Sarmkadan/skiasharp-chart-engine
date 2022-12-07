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
/// Template system example: Use built-in templates for quick chart creation
/// </summary>
public class TemplateSystemExample
{
    public static void Main()
    {
        Console.WriteLine("Demonstrating template system...\n");

        var engine = ChartEngine.Create();
        Directory.CreateDirectory("output");

        Console.WriteLine("=== AVAILABLE TEMPLATES ===\n");

        var chartTypes = new[]
        {
            ChartType.LineChart,
            ChartType.BarChart,
            ChartType.ColumnChart,
            ChartType.PieChart
        };

        foreach (var chartType in chartTypes)
        {
            Console.WriteLine($"Creating {chartType} from template...");

            var template = engine.GetConfigurationTemplate(chartType);
            template.Title = $"{chartType} - Template Example";
            template.Width = 800;
            template.Height = 600;

            var chart = new Chart(template);

            if (chartType == ChartType.PieChart)
            {
                CreatePieChartData(chart);
            }
            else
            {
                CreateLineChartData(chart);
            }

            var result = engine.RenderChart(chart);

            if (result.IsSuccessful)
            {
                var outputPath = Path.Combine("output", $"template-{chartType.ToString().ToLower()}.png");
                File.WriteAllBytes(outputPath, result.Data as byte[]);
                Console.WriteLine($"  ✓ Saved to {outputPath}");
                Console.WriteLine($"    Render time: {result.RenderTimeMs}ms\n");
            }
            else
            {
                Console.WriteLine($"  ✗ Error: {result.ErrorMessage}\n");
            }
        }

        Console.WriteLine("=== CUSTOM TEMPLATE ===\n");

        var customTemplate = new ChartConfiguration
        {
            Title = "Custom Configuration",
            Width = 1200,
            Height = 600,
            MarginTop = 80,
            MarginBottom = 80,
            MarginLeft = 100,
            MarginRight = 100,
            BackgroundColor = "#F0F0F0",
            AxisColor = "#003366",
            GridColor = "#CCCCCC",
            TextColor = "#000000",
            ShowGrid = true,
            ShowLegend = true,
            ShowAxisLabels = true,
            AntiAlias = true
        };

        var customChart = new Chart(customTemplate);

        var customSeries = new ChartSeries("Custom Data", "#0066CC");
        for (int i = 1; i <= 12; i++)
        {
            customSeries.AddDataPoint(i, i * 1000 + (i % 3) * 500);
        }

        customChart.AddSeries(customSeries);

        var customResult = engine.RenderChart(customChart);

        if (customResult.IsSuccessful)
        {
            var outputPath = Path.Combine("output", "template-custom.png");
            File.WriteAllBytes(outputPath, customResult.Data as byte[]);
            Console.WriteLine($"✓ Custom template chart saved to {outputPath}\n");
        }
    }

    private static void CreateLineChartData(Chart chart)
    {
        var series = new ChartSeries("Sample Data", "#FF6B6B");
        var values = new[] { 100, 150, 120, 200, 180, 220, 190, 250, 240, 280, 270, 320 };

        for (int i = 1; i <= values.Length; i++)
        {
            series.AddDataPoint(i, values[i - 1]);
        }

        chart.AddSeries(series);
    }

    private static void CreatePieChartData(Chart chart)
    {
        var items = new[] { ("Item A", 40.0), ("Item B", 30.0), ("Item C", 20.0), ("Item D", 10.0) };
        var colors = new[] { "#FF6B6B", "#4ECDC4", "#F7DC6F", "#95E1D3" };

        for (int i = 0; i < items.Length; i++)
        {
            var series = new ChartSeries(items[i].Item1, colors[i]);
            series.AddDataPoint(1, items[i].Item2);
            chart.AddSeries(series);
        }
    }
}
