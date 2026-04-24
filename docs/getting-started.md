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

---

## Chart Type Reference

Each section below shows the minimal code required to produce a chart plus the key options you can customise.

---

### Line Chart

**Required options**: `ChartType.LineChart`, at least one `ChartSeries` with ≥ 2 `DataPoint`s.

**Optional highlights**: `ShowGrid`, `ShowLegend`, `XAxisLabel`, `YAxisLabel`, multiple series for comparisons.

```csharp
var chart = new Chart(ChartType.LineChart)
{
    Configuration = new ChartConfiguration
    {
        Title  = "Weekly Active Users",
        Width  = 900,
        Height = 500,
        XAxisLabel = "Day",
        YAxisLabel = "Users",
        ShowGrid   = true,
        ShowLegend = true
    }
};

var series = new ChartSeries("This week", "#4ECDC4");
series.AddDataPoint(1, 1200);
series.AddDataPoint(2, 1800);
series.AddDataPoint(3, 1500);
series.AddDataPoint(4, 2200);
series.AddDataPoint(5, 2000);
chart.AddSeries(series);

var result = engine.RenderChart(chart);
File.WriteAllBytes("line.png", result.Data as byte[]);
```

---

### Bar Chart

**Required options**: `ChartType.BarChart`, one series per category (or one series with one `DataPoint` each).

**Optional highlights**: custom `Width`/`Height`, `BackgroundColor`, `AxisColor`.

```csharp
var chart = new Chart(ChartType.BarChart)
{
    Configuration = new ChartConfiguration
    {
        Title  = "Quarterly Revenue by Region",
        Width  = 900,
        Height = 500
    }
};

var labels  = new[] { "North", "South", "East", "West" };
var revenue = new[] { 82_000, 74_000, 95_000, 61_000 };

for (int i = 0; i < labels.Length; i++)
{
    var s = new ChartSeries(labels[i], $"#{(0x44 + i * 0x30):X2}AAFF");
    s.AddDataPoint(i + 1, revenue[i]);
    chart.AddSeries(s);
}

var result = engine.RenderChart(chart);
File.WriteAllBytes("bar.png", result.Data as byte[]);
```

---

### Pie Chart

**Required options**: `ChartType.PieChart`, one series per slice, each series containing a single `DataPoint` whose `Value` is the slice magnitude.

**Optional highlights**: square `Width`/`Height` for a circular chart, labels are rendered automatically as percentages.

```csharp
var chart = new Chart(ChartType.PieChart)
{
    Configuration = new ChartConfiguration
    {
        Title  = "Browser Market Share",
        Width  = 600,
        Height = 600
    }
};

var slices = new[]
{
    ("Chrome",  65.5),
    ("Safari",  19.1),
    ("Firefox",  4.0),
    ("Edge",     4.2),
    ("Other",    7.2)
};

for (int i = 0; i < slices.Length; i++)
{
    var s = new ChartSeries(slices[i].Item1, $"#{(0x20 + i * 0x30):X2}9FE8");
    s.AddDataPoint(i, slices[i].Item2);
    chart.AddSeries(s);
}

var result = engine.RenderChart(chart);
File.WriteAllBytes("pie.png", result.Data as byte[]);
```

---

### Heatmap Chart

**Required options**: `ChartType.HeatmapChart`, one series whose `DataPoint` list is a row-major flattened 2-D matrix (values are laid out row by row, left to right).

**Optional highlights**: `HeatmapColorScale` — set it on `ChartConfiguration` to choose between `Linear` (default), `Logarithmic` (wide dynamic range / outliers), or `Quantile` (best contrast for skewed distributions).

```csharp
var chart = new Chart(ChartType.HeatmapChart)
{
    Configuration = new ChartConfiguration
    {
        Title = "Server Response Time (ms) by Hour & Day",
        Width = 800,
        Height = 500,
        // Use Logarithmic scale to reveal detail across a wide value range:
        HeatmapColorScale = HeatmapColorScale.Logarithmic
    }
};

// 4 rows (servers) × 6 columns (time slots)
double[,] matrix =
{
    {  50,  55, 120, 400, 200,  80 },
    {  48,  60, 130, 450, 210,  75 },
    {  52,  58, 115, 380, 195,  82 },
    {  45,  50, 105, 360, 185,  70 }
};

var series = new ChartSeries("Response times", "#FF6B6B");
for (int row = 0; row < matrix.GetLength(0); row++)
    for (int col = 0; col < matrix.GetLength(1); col++)
        series.AddDataPoint(row * matrix.GetLength(1) + col, matrix[row, col]);

chart.AddSeries(series);

var result = engine.RenderChart(chart);
File.WriteAllBytes("heatmap.png", result.Data as byte[]);
```

---

### Scatter Chart

**Required options**: `ChartType.ScatterChart`, at least one `ChartSeries` with data points where both `X` and `Y` carry meaningful values.

**Optional highlights**: multiple series to compare two populations, `ShowGrid = true` helps read coordinates, set a `Subtitle` to describe the axes relationship.

```csharp
var chart = new Chart(ChartType.ScatterChart)
{
    Configuration = new ChartConfiguration
    {
        Title    = "Height vs Weight",
        Subtitle = "Sample of 200 adults",
        Width    = 800,
        Height   = 600,
        XAxisLabel = "Height (cm)",
        YAxisLabel = "Weight (kg)",
        ShowGrid   = true,
        ShowLegend = true
    }
};

var rng = new Random(42);
var males = new ChartSeries("Male", "#4A90D9");
for (int i = 0; i < 100; i++)
    males.AddDataPoint(160 + rng.NextDouble() * 30, 65 + rng.NextDouble() * 40);
chart.AddSeries(males);

var females = new ChartSeries("Female", "#E74C8B");
for (int i = 0; i < 100; i++)
    females.AddDataPoint(150 + rng.NextDouble() * 25, 50 + rng.NextDouble() * 35);
chart.AddSeries(females);

var result = engine.RenderChart(chart);
File.WriteAllBytes("scatter.png", result.Data as byte[]);
```

---

### Area Chart

**Required options**: `ChartType.AreaChart`, at least one `ChartSeries` with ≥ 2 `DataPoint`s.

**Optional highlights**: layering multiple semi-transparent series creates a stacked-area feel; use `Subtitle` to add a time range or data source note.

```csharp
var chart = new Chart(ChartType.AreaChart)
{
    Configuration = new ChartConfiguration
    {
        Title      = "Monthly Active Users",
        Subtitle   = "Jan – Jun 2026",
        Width      = 900,
        Height     = 500,
        XAxisLabel = "Month",
        YAxisLabel = "Users (thousands)",
        ShowGrid   = true,
        ShowLegend = true
    }
};

var mobile = new ChartSeries("Mobile", "#4ECDC4");
mobile.AddDataPoint(1, 38);
mobile.AddDataPoint(2, 42);
mobile.AddDataPoint(3, 47);
mobile.AddDataPoint(4, 51);
mobile.AddDataPoint(5, 56);
mobile.AddDataPoint(6, 61);
chart.AddSeries(mobile);

var desktop = new ChartSeries("Desktop", "#FF6B6B");
desktop.AddDataPoint(1, 22);
desktop.AddDataPoint(2, 24);
desktop.AddDataPoint(3, 23);
desktop.AddDataPoint(4, 25);
desktop.AddDataPoint(5, 24);
desktop.AddDataPoint(6, 26);
chart.AddSeries(desktop);

var result = engine.RenderChart(chart);
File.WriteAllBytes("area.png", result.Data as byte[]);
```

---

### Title and Subtitle

Every chart type supports a `Title` (bold, larger font) and optional `Subtitle` (smaller font below the title). The plot area is automatically shifted down to make room for the text.

```csharp
var config = new ChartConfiguration
{
    Title    = "Q2 2026 Revenue",
    Subtitle = "All figures in USD thousands — unaudited",
    Width    = 900,
    Height   = 500
};
```

---


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
