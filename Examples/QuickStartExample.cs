// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using SkiaSharpChartEngine;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Utilities;

namespace SkiaSharpChartEngine.Examples;

/// <summary>
/// Quick start examples demonstrating common usage patterns
/// </summary>
public static class QuickStartExample
{
    public static void CreateSimpleLineChart()
    {
        var engine = ChartEngine.Create();

        var chart = new Chart(ChartType.LineChart)
        {
            Configuration = new ChartConfiguration { Title = "Sample Line Chart" }
        };

        var series = new ChartSeries("Series 1", "#1F77B4");
        for (int i = 0; i < 10; i++)
        {
            series.AddDataPoint(i, Math.Sin(i) * 10 + 20);
        }
        chart.AddSeries(series);

        var result = engine.RenderChart(chart);
        if (result.Success)
        {
            Console.WriteLine($"Chart rendered: {result.FileSizeBytes} bytes");
        }
    }

    public static void CreateMultiSeriesChart()
    {
        var engine = ChartEngine.Create();

        var chart = new ChartBuilder(ChartType.LineChart)
            .WithTitle("Multi-Series Chart")
            .WithSize(1000, 600)
            .WithAxisLabels("Time", "Value")
            .ShowGrid(true)
            .ShowLegend(true)
            .Build();

        var data1 = DataPointGenerator.GenerateSinusoidalData(50, 10, 1);
        chart.AddSeries(new ChartSeries("Sin Wave", "#FF6B6B", ChartType.LineChart)
        {
            DataPoints = { data1 }
        });

        var data2 = DataPointGenerator.GenerateSinusoidalData(50, 15, 1.5);
        chart.AddSeries(new ChartSeries("Offset Sin Wave", "#4ECDC4", ChartType.LineChart)
        {
            DataPoints = { data2 }
        });

        var result = engine.RenderChart(chart);
        Console.WriteLine($"Render result: Success={result.Success}, Time={result.RenderTimeMs}ms");
    }

    public static void ExportChartToFile()
    {
        var engine = ChartEngine.Create();

        var chart = new Chart(ChartType.BarChart)
        {
            Configuration = new ChartConfiguration
            {
                Title = "Monthly Sales",
                XAxisLabel = "Month",
                YAxisLabel = "Sales ($)"
            }
        };

        var series = new ChartSeries("Sales", "#2CA02C");
        series.AddDataPoint(0, 5000, "Jan");
        series.AddDataPoint(1, 7500, "Feb");
        series.AddDataPoint(2, 6000, "Mar");
        series.AddDataPoint(3, 8500, "Apr");
        chart.AddSeries(series);

        var exportOptions = new ExportOptions("monthly_sales", ExportFormat.PNG)
        {
            OutputDirectory = Environment.GetTempPath(),
            DPI = 150
        };

        var result = engine.ExportChart(chart, exportOptions);

        if (result.Success)
        {
            Console.WriteLine($"Chart exported to: {result.OutputPath}");
        }
        else
        {
            Console.WriteLine($"Export failed: {result.ErrorMessage}");
        }
    }

    public static void ValidateChartBeforeRendering()
    {
        var chart = new Chart(ChartType.LineChart)
        {
            Configuration = new ChartConfiguration { Title = "Test Chart" }
        };

        var validation = ChartValidator.ValidateChart(chart);

        if (!validation.IsValid)
        {
            Console.WriteLine("Chart validation failed:");
            Console.WriteLine(validation);
            return;
        }

        var series = new ChartSeries("Data");
        series.AddDataPoint(1, 10);
        series.AddDataPoint(2, 20);
        chart.AddSeries(series);

        validation = ChartValidator.ValidateChart(chart);
        Console.WriteLine($"Chart is valid: {validation.IsValid}");
    }

    public static void GenerateAndVisualizePaletteColors()
    {
        var palettes = new[]
        {
            ColorPalette.CreateDefaultPalette(),
            ColorPalette.CreateVibrantPalette(),
            ColorPalette.CreatePastelPalette(),
            ColorPalette.CreateOceanPalette()
        };

        var engine = ChartEngine.Create();

        foreach (var palette in palettes)
        {
            var chart = new Chart(ChartType.BarChart)
            {
                Configuration = new ChartConfiguration
                {
                    Title = $"{palette.Name} Color Palette",
                    Height = 300
                }
            };

            for (int i = 0; i < palette.GetColorCount(); i++)
            {
                var series = new ChartSeries($"Color {i + 1}", palette.GetColorAtIndex(i));
                series.AddDataPoint(i, i + 1);
                chart.AddSeries(series);
            }

            var result = engine.RenderChart(chart);
            Console.WriteLine($"Rendered {palette.Name} palette: {result.Success}");
        }
    }

    public static async Task RenderChartAsync()
    {
        var engine = ChartEngine.Create();

        var chart = new Chart(ChartType.LineChart)
        {
            Configuration = new ChartConfiguration { Title = "Async Rendering Example" }
        };

        var series = new ChartSeries("Data", "#1F77B4");
        var data = DataPointGenerator.GenerateRandomData(100, 0, 100);
        series.AddDataPoints(data);
        chart.AddSeries(series);

        var cts = new CancellationTokenSource();
        var result = await engine.RenderChartAsync(chart, cts.Token);

        Console.WriteLine($"Async render completed: {result.RenderTimeMs}ms");
    }
}
