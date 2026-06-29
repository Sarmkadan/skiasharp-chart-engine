using SkiaSharpChartEngine;
using SkiaSharpChartEngine.Models;

// This example demonstrates the most basic usage: creating a chart,
// adding data, and rendering it to a PNG file.

// 1. Create the engine instance
var engine = ChartEngine.Create();

// 2. Define the chart (LineChart type)
var chart = new Chart(ChartType.LineChart)
{
    Configuration = new ChartConfiguration
    {
        Title = "Basic Usage Example",
        Width = 800,
        Height = 600
    }
};

// 3. Add a data series
var series = new ChartSeries("Series 1", "#FF6B6B");
series.AddDataPoint(1, 10);
series.AddDataPoint(2, 20);
series.AddDataPoint(3, 15);
chart.AddSeries(series);

// 4. Render and save
var result = engine.RenderChart(chart);
if (result.IsSuccessful)
{
    File.WriteAllBytes("basic-chart.png", (byte[])result.Data);
    Console.WriteLine("Chart saved to basic-chart.png");
}
else
{
    Console.WriteLine($"Error: {result.ErrorMessage}");
}
