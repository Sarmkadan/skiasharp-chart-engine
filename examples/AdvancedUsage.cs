using SkiaSharpChartEngine;
using SkiaSharpChartEngine.Models;

// This example shows advanced configuration, custom options, and error handling.

var engine = ChartEngine.Create();

// Configure the chart with specific styling
var chart = new Chart(ChartType.BarChart)
{
    Configuration = new ChartConfiguration
    {
        Title = "Advanced Usage",
        Width = 1000,
        Height = 600,
        BackgroundColor = "#F0F0F0",
        ShowGrid = true,
        ShowLegend = true
    }
};

// Add data series
var series = new ChartSeries("Sales", "#4ECDC4")
{
    LineWidth = 4
};
series.AddDataPoint(1, 100);
series.AddDataPoint(2, 250);
series.AddDataPoint(3, 180);
chart.AddSeries(series);

// Export with options
var options = new ExportOptions("advanced-report", ExportFormat.PNG, "./output");

try
{
    var result = engine.ExportChart(chart, options);
    
    if (result.IsSuccessful)
    {
        Console.WriteLine($"Chart successfully exported to {result.Data}");
    }
    else
    {
        // Handle rendering/export errors
        Console.WriteLine($"Export failed: {result.ErrorMessage}");
    }
}
catch (Exception ex)
{
    // Handle unexpected exceptions
    Console.WriteLine($"An unexpected error occurred: {ex.Message}");
}
