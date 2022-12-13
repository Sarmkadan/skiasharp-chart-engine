# Getting Started with SkiaSharp Chart Engine

This guide will walk you through installing and using the SkiaSharp Chart Engine in your .NET application.

## Prerequisites

- .NET 10.0 or later
- Visual Studio 2022, VS Code, or Rider
- NuGet package manager

## Installation

### Option 1: NuGet Package Manager UI

1. Right-click your project in Solution Explorer
2. Select **Manage NuGet Packages**
3. Search for `SkiaSharpChartEngine`
4. Click **Install**

### Option 2: Package Manager Console

```powershell
Install-Package SkiaSharpChartEngine
```

### Option 3: .NET CLI

```bash
dotnet add package SkiaSharpChartEngine
```

### Option 4: Direct Project File Edit

Add to your `.csproj`:

```xml
<ItemGroup>
    <PackageReference Include="SkiaSharpChartEngine" Version="1.2.0" />
</ItemGroup>
```

## Your First Chart

### 1. Create a Console Application

```bash
dotnet new console -n MyChartApp
cd MyChartApp
dotnet add package SkiaSharpChartEngine
```

### 2. Write the Code

Edit `Program.cs`:

```csharp
using SkiaSharpChartEngine;
using SkiaSharpChartEngine.Models;

// Create engine
var engine = ChartEngine.Create();

// Create chart
var chart = new Chart(ChartType.LineChart)
{
    Configuration = new ChartConfiguration
    {
        Title = "Monthly Revenue",
        Width = 800,
        Height = 600,
        XAxisLabel = "Month",
        YAxisLabel = "Revenue ($)"
    }
};

// Add data series
var series = new ChartSeries("2026", "#FF6B6B");
series.AddDataPoint(1, 45000);  // January
series.AddDataPoint(2, 52000);  // February
series.AddDataPoint(3, 48000);  // March
series.AddDataPoint(4, 61000);  // April
series.AddDataPoint(5, 58000);  // May

chart.AddSeries(series);

// Render
var result = engine.RenderChart(chart);

if (result.IsSuccessful)
{
    var outputPath = "revenue_chart.png";
    File.WriteAllBytes(outputPath, result.Data as byte[]);
    Console.WriteLine($"Chart saved to {outputPath}");
}
else
{
    Console.WriteLine($"Error: {result.ErrorMessage}");
}
```

### 3. Run It

```bash
dotnet run
```

You'll now have a `revenue_chart.png` file showing your chart!

## Common Scenarios

### Scenario 1: Multiple Series (Comparing Data)

```csharp
var chart = new Chart(ChartType.LineChart)
{
    Configuration = new ChartConfiguration
    {
        Title = "Sales Comparison",
        ShowLegend = true,
        ShowGrid = true
    }
};

// 2025 data
var series2025 = new ChartSeries("2025", "#FF6B6B");
series2025.AddDataPoint(1, 30000);
series2025.AddDataPoint(2, 35000);
series2025.AddDataPoint(3, 38000);
chart.AddSeries(series2025);

// 2026 data
var series2026 = new ChartSeries("2026", "#4ECDC4");
series2026.AddDataPoint(1, 45000);
series2026.AddDataPoint(2, 52000);
series2026.AddDataPoint(3, 48000);
chart.AddSeries(series2026);

var result = engine.RenderChart(chart);
```

### Scenario 2: Export to Multiple Formats

```csharp
var chart = GetYourChart();
var engine = ChartEngine.Create();

var formats = new[] 
{ 
    ExportFormat.PNG, 
    ExportFormat.SVG, 
    ExportFormat.PDF 
};

foreach (var format in formats)
{
    var options = new ExportOptions($"chart", format, "output/");
    var result = engine.ExportChart(chart, options);
    
    if (result.IsSuccessful)
        Console.WriteLine($"Exported {format} to {result.Output}");
}
```

### Scenario 3: Async Operations (Web Application)

```csharp
using SkiaSharpChartEngine;
using SkiaSharpChartEngine.Models;

// In an ASP.NET Core controller
[ApiController]
[Route("api/charts")]
public class ChartController : ControllerBase
{
    private readonly ChartEngine _engine;

    public ChartController(IServiceProvider serviceProvider)
    {
        _engine = new ChartEngine(serviceProvider);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetChart(string id, CancellationToken ct)
    {
        var chart = await _engine.GetChartAsync(id, ct);
        if (chart == null)
            return NotFound();

        var result = await _engine.RenderChartAsync(chart, ct);
        
        if (!result.IsSuccessful)
            return BadRequest(result.ErrorMessage);

        var data = result.Data as byte[];
        return File(data, "image/png", $"{id}.png");
    }
}
```

### Scenario 4: Template System

```csharp
var engine = ChartEngine.Create();

// Get pre-configured template
var template = engine.GetConfigurationTemplate(ChartType.BarChart);
template.Title = "Employee Salaries";
template.Width = 1024;
template.Height = 512;

var chart = new Chart(template);

var series = new ChartSeries("Salaries", "#3498DB");
series.AddDataPoint(1, 50000);  // Employee 1
series.AddDataPoint(2, 65000);  // Employee 2
series.AddDataPoint(3, 55000);  // Employee 3
series.AddDataPoint(4, 75000);  // Employee 4

chart.AddSeries(series);

var result = engine.RenderChart(chart);
```

### Scenario 5: Caching for Performance

```csharp
var engine = ChartEngine.Create();
var expensiveChart = BuildComplexChart();

// Pre-render and cache
engine.PrewarmRenderCache(expensiveChart);

// First render (uses cache)
var result1 = engine.RenderChart(expensiveChart);

// Update data - cache is invalidated automatically
expensiveChart.Series[0].AddDataPoint(100, 200);

// Re-render (recalculated)
var result2 = engine.RenderChart(expensiveChart);
```

## Configuration

### Custom ChartConfiguration

```csharp
var config = new ChartConfiguration
{
    // Dimensions
    Width = 1024,
    Height = 768,
    
    // Margins
    MarginTop = 60,
    MarginBottom = 60,
    MarginLeft = 60,
    MarginRight = 60,
    
    // Text
    Title = "Performance Metrics",
    XAxisLabel = "Time (seconds)",
    YAxisLabel = "Response Time (ms)",
    
    // Colors
    BackgroundColor = "#FFFFFF",
    AxisColor = "#333333",
    GridColor = "#CCCCCC",
    TextColor = "#000000",
    
    // Features
    ShowGrid = true,
    ShowLegend = true,
    ShowAxisLabels = true,
    AntiAlias = true
};

var chart = new Chart(config);
```

## Data Point Addition Methods

### Simple Values

```csharp
var series = new ChartSeries("Data", "#FF6B6B");
series.AddDataPoint(1, 10);    // X=1, Y=10
series.AddDataPoint(2, 20);    // X=2, Y=20
series.AddDataPoint(3, 15);    // X=3, Y=15
```

### From Arrays

```csharp
var months = new[] { 1, 2, 3, 4, 5, 6 };
var values = new[] { 100, 150, 120, 200, 180, 220 };

var series = new ChartSeries("Sales", "#4ECDC4");
for (int i = 0; i < months.Length; i++)
    series.AddDataPoint(months[i], values[i]);
```

### From Collections

```csharp
var data = new List<(DateTime Date, decimal Amount)>
{
    (DateTime.Now.AddDays(-5), 1000m),
    (DateTime.Now.AddDays(-4), 1500m),
    (DateTime.Now.AddDays(-3), 1200m),
    (DateTime.Now.AddDays(-2), 1800m),
    (DateTime.Now.AddDays(-1), 1600m)
};

var series = new ChartSeries("Daily Revenue", "#F7DC6F");
for (int i = 0; i < data.Count; i++)
    series.AddDataPoint(i, (double)data[i].Amount);
```

## Handling Results

Every render/export operation returns a `RenderResult`:

```csharp
var result = engine.RenderChart(chart);

// Check success
if (result.IsSuccessful)
{
    // Get the data (byte array for renders, filepath for exports)
    var data = result.Data;
    
    // Get performance metrics
    var renderTime = result.RenderTimeMs;
    Console.WriteLine($"Rendered in {renderTime}ms");
    
    // Get format information
    var format = result.Format;  // ExportFormat.PNG
}
else
{
    // Handle error
    var message = result.ErrorMessage;
    var exception = result.Exception;
    Console.WriteLine($"Error: {message}");
}
```

## Validation

Always validate charts before rendering in production:

```csharp
try
{
    if (chart.ValidateForRendering())
    {
        var result = engine.RenderChart(chart);
    }
}
catch (InvalidChartDataException ex)
{
    Console.WriteLine($"Invalid chart: {ex.Message}");
}
```

## Supported Chart Types

| Type | Use Case |
|------|----------|
| `LineChart` | Trends over time |
| `BarChart` | Categorical comparisons (horizontal) |
| `ColumnChart` | Categorical comparisons (vertical) |
| `PieChart` | Composition/percentages |
| `AreaChart` | Filled line charts |
| `HeatmapChart` | 2D patterns/correlations |
| `ScatterChart` | Relationship analysis |

## Supported Export Formats

- **PNG** - Best for web and presentations
- **JPEG** - Best for compression
- **WebP** - Modern format with superior compression
- **SVG** - Scalable vector format
- **PDF** - Print-ready documents

## Next Steps

- Read [Architecture](architecture.md) for deep dive into design
- Check [API Reference](api-reference.md) for complete method documentation
- Explore [Deployment](deployment.md) for production setup
- Review [FAQ](faq.md) for common questions

## Need Help?

- 🐛 Found a bug? [Open an issue](https://github.com/vladyslav-zaiets/skiasharp-chart-engine/issues)
- 💡 Have a question? Check the [FAQ](faq.md)
- 📖 Want to contribute? See [Contributing Guidelines](../CONTRIBUTING.md)
