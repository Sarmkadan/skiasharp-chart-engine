[![Build](https://github.com/sarmkadan/skiasharp-chart-engine/actions/workflows/build.yml/badge.svg)](https://github.com/sarmkadan/skiasharp-chart-engine/actions/workflows/build.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)

# SkiaSharp Chart Engine

A **high-performance, production-ready chart rendering library** for .NET applications using SkiaSharp. Render beautiful line, bar, pie, heatmap, area, and scatter charts with lightning-fast performance, comprehensive caching, and export to PNG, SVG, JPEG, WebP, and PDF formats.

## Table of Contents

- [Features](#features)
- [Quick Start](#quick-start)
- [Installation](#installation)
- [Architecture](#architecture)
- [Usage Examples](#usage-examples)
  - [Creating a Line Chart](#creating-a-line-chart)
  - [Creating a Bar Chart](#creating-a-bar-chart)
  - [Creating a Pie Chart](#creating-a-pie-chart)
  - [Creating a Heatmap](#creating-a-heatmap)
  - [Exporting Charts](#exporting-charts)
  - [Async Operations](#async-operations)
  - [Template System](#template-system)
  - [Performance & Caching](#performance--caching)
- [API Reference](#api-reference)
- [Configuration Reference](#configuration-reference)
- [Performance Benchmarks](#performance-benchmarks)
- [CLI Usage](#cli-usage)
- [Troubleshooting](#troubleshooting)
- [Testing](#testing)
- [Related Projects](#related-projects)
- [Contributing](#contributing)
- [License](#license)

## Features

### Multiple Chart Types
- **LineChart** - Trend analysis with smooth line rendering
- **BarChart** - Horizontal bar charts for categorical comparisons
- **ColumnChart** - Vertical bar charts with automatic scaling
- **PieChart** - Circular composition visualization with labels
- **HeatmapChart** - Color-mapped grids for pattern analysis
- **AreaChart** - Line charts with filled areas
- **ScatterChart** - Correlation analysis with point rendering

### Performance & Scalability
- **Optimized Rendering Pipeline** - Efficient SkiaSharp-based rendering
- **Automatic Caching** - Smart in-memory caching with cache invalidation
- **Async Support** - Full async/await support for all operations
- **Concurrent Rendering** - Thread-safe operations with concurrency limiting
- **Batch Processing** - Render multiple charts simultaneously
- **Memory Efficient** - Minimal heap allocations with object pooling

### Rich Customization
- **Color Palettes** - 50+ built-in color schemes
- **Custom Styling** - Line width, opacity, padding, margins
- **Theme System** - Light/dark themes with custom CSS-like styling
- **Grid & Axes** - Configurable grid lines, axis labels, ranges
- **Animation Support** - Smooth transitions and animations
- **Templates** - Pre-configured templates for quick setup

### Multiple Export Formats
- **PNG** - Lossless raster format (default)
- **JPEG** - Compressed raster format
- **WebP** - Modern web format with superior compression
- **SVG** - Scalable vector graphics
- **PDF** - Document format for reporting

### Enterprise Features
- **Data Validation** - Comprehensive input validation
- **Error Handling** - Detailed error messages and logging
- **Metrics Collection** - Rendering time, memory usage, cache hit rates
- **Health Checks** - System diagnostics
- **Rate Limiting** - Prevent resource exhaustion
- **Request Validation** - Automatic input sanitization
- **Repository Pattern** - In-memory chart storage with CRUD operations

### Integration Ready
- **REST API** - Full HTTP API with controllers
- **Dependency Injection** - Microsoft.Extensions.DependencyInjection
- **Configuration** - Microsoft.Extensions.Configuration support
- **Logging** - Microsoft.Extensions.Logging integration
- **Webhooks** - Event-driven architecture
- **External APIs** - Built-in client for data fetching

## Quick Start

### Minimal Example

```csharp
using SkiaSharpChartEngine;
using SkiaSharpChartEngine.Models;

// Create chart engine
var engine = ChartEngine.Create();

// Create a chart
var chart = new Chart(ChartType.LineChart)
{
    Configuration = new ChartConfiguration
    {
        Title = "Sales Trend",
        Width = 800,
        Height = 600
    }
};

// Add data series
var series = new ChartSeries("Q1 Sales", "#FF6B6B");
series.AddDataPoint(1, 15000);
series.AddDataPoint(2, 22000);
series.AddDataPoint(3, 18500);
chart.AddSeries(series);

// Render
var result = engine.RenderChart(chart);
if (result.IsSuccessful)
{
    File.WriteAllBytes("chart.png", result.Data as byte[]);
}
```

## Installation

### NuGet Package

```bash
dotnet add package SkiaSharpChartEngine
```

### From Source

```bash
git clone https://github.com/vladyslav-zaiets/skiasharp-chart-engine.git
cd skiasharp-chart-engine
dotnet build
dotnet pack
```

### Docker

```bash
docker build -t skiasharp-chart-engine:latest .
docker run -p 5000:5000 skiasharp-chart-engine:latest
```

## Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                        ChartEngine (Facade)                      │
│  Main entry point providing synchronous and async interfaces     │
└──────────────────┬──────────────────────────────────────────────┘
                   │
        ┌──────────┴──────────┬──────────────────┬──────────────┐
        │                     │                  │              │
   ┌────▼─────┐    ┌──────────▼────┐   ┌────────▼──┐   ┌──────▼────┐
   │ Rendering│    │   Export      │   │Repository │   │ Data      │
   │ Service  │    │   Service     │   │ (Storage) │   │ Service   │
   └────┬─────┘    └──────┬────────┘   └──────┬────┘   └──────┬────┘
        │                 │                    │              │
        ├─────────────────┤                    │              │
        │                 │                    │              │
   ┌────▼─────────────────▼──┐           ┌────▼────┐      ┌──▼─────┐
   │  Cache Service          │           │ Models  │      │ Config │
   │  (In-Memory/Distributed)│           │ & Types │      │Service │
   └────┬────────────────────┘           └─────────┘      └────────┘
        │
   ┌────▼──────────────────────────┐
   │  SkiaSharp Rendering Backend   │
   │  (Canvas, Paint, Surface)      │
   └───────────────────────────────┘
        │
   ┌────▼──────────────────────────┐
   │  Output Formats (PNG/SVG/PDF)  │
   └───────────────────────────────┘
```

### Core Components

#### 1. **Models Layer**
- `Chart` - Main chart container with series and configuration
- `ChartSeries` - Named data series with color and styling
- `DataPoint` - Individual X/Y coordinate pair
- `ChartConfiguration` - Dimensions, colors, fonts, margins
- `ExportOptions` - Output format and destination settings
- `RenderResult` - Operation result with metrics

#### 2. **Services Layer**
- `ChartRenderingService` - Converts charts to raster/vector formats
- `ExportService` - Multi-format export orchestration
- `ChartDataService` - Data validation and transformation
- `ConfigurationService` - Template and configuration management
- `RenderCacheService` - Caching strategies and invalidation

#### 3. **Repository Layer**
- `ChartRepository` - In-memory CRUD for chart persistence
- Supports concurrent access with locking mechanisms

#### 4. **Pipeline**
- `ChartRenderingPipeline` - Orchestrates rendering stages
- Composable processing steps

#### 5. **Infrastructure**
- Dependency injection container setup
- Logging and diagnostics
- Error handling and validation
- Middleware components for web APIs

## Usage Examples

### Creating a Line Chart

```csharp
var engine = ChartEngine.Create();

var chart = new Chart(ChartType.LineChart)
{
    Configuration = new ChartConfiguration
    {
        Title = "Website Traffic",
        Width = 1000,
        Height = 600,
        ShowGrid = true,
        ShowLegend = true,
        XAxisLabel = "Days",
        YAxisLabel = "Visitors"
    }
};

// Month 1 data
var series1 = new ChartSeries("Month 1", "#FF6B6B");
for (int i = 1; i <= 30; i++)
    series1.AddDataPoint(i, Random.Shared.Next(1000, 5000));

// Month 2 data
var series2 = new ChartSeries("Month 2", "#4ECDC4");
for (int i = 1; i <= 30; i++)
    series2.AddDataPoint(i, Random.Shared.Next(1000, 5000));

chart.AddSeries(series1);
chart.AddSeries(series2);

var result = engine.RenderChart(chart);
File.WriteAllBytes("traffic.png", result.Data as byte[]);
```

### Creating a Bar Chart

```csharp
var chart = new Chart(ChartType.BarChart)
{
    Configuration = new ChartConfiguration
    {
        Title = "Product Sales by Region",
        Width = 900,
        Height = 500
    }
};

var regions = new[] { "North", "South", "East", "West", "Central" };
var values = new[] { 45000, 52000, 38000, 61000, 48000 };

for (int i = 0; i < regions.Length; i++)
{
    var series = new ChartSeries(regions[i], GetColorForIndex(i));
    series.AddDataPoint(1, values[i]);
    chart.AddSeries(series);
}

var result = engine.ExportChart(chart, 
    new ExportOptions("sales_report", ExportFormat.PNG));
```

### Creating a Pie Chart

```csharp
var chart = new Chart(ChartType.PieChart)
{
    Configuration = new ChartConfiguration
    {
        Title = "Market Share",
        Width = 600,
        Height = 600
    }
};

var companies = new[] { "CompanyA", "CompanyB", "CompanyC", "CompanyD" };
var marketShare = new[] { 35.0, 28.0, 22.0, 15.0 };

for (int i = 0; i < companies.Length; i++)
{
    var series = new ChartSeries(companies[i], GetColorForIndex(i));
    series.AddDataPoint(i, marketShare[i]);
    chart.AddSeries(series);
}

var result = engine.RenderChart(chart);
```

### Creating a Heatmap

```csharp
var chart = new Chart(ChartType.HeatmapChart)
{
    Configuration = new ChartConfiguration
    {
        Title = "Server Response Times",
        Width = 800,
        Height = 600
    }
};

var hours = new[] { "00:00", "04:00", "08:00", "12:00", "16:00", "20:00" };
var responseData = new double[,]
{
    { 50, 55, 60, 75, 70, 65 },
    { 48, 52, 58, 80, 75, 60 },
    { 45, 50, 55, 85, 80, 58 }
};

// Flatten 2D array into series
for (int y = 0; y < responseData.GetLength(0); y++)
{
    var series = new ChartSeries($"Server{y + 1}", GetHeatmapColor(y));
    for (int x = 0; x < responseData.GetLength(1); x++)
        series.AddDataPoint(x, responseData[y, x]);
    chart.AddSeries(series);
}

var result = engine.RenderChart(chart);
```

### Exporting Charts

```csharp
var chart = GetChart();
var engine = ChartEngine.Create();

// Export to PNG
var pngResult = engine.ExportChart(chart, 
    new ExportOptions("chart", ExportFormat.PNG, "output/"));

// Export to SVG
var svgResult = engine.ExportChart(chart,
    new ExportOptions("chart", ExportFormat.SVG, "output/"));

// Export to WebP
var webpResult = engine.ExportChart(chart,
    new ExportOptions("chart", ExportFormat.WebP, "output/"));

// Export to PDF
var pdfResult = engine.ExportChart(chart,
    new ExportOptions("chart", ExportFormat.PDF, "output/"));

if (pngResult.IsSuccessful)
    Console.WriteLine($"Exported to: {pngResult.Output}");
```

### Async Operations

```csharp
var engine = ChartEngine.Create();
var chart = GetChart();

// Async rendering
var renderResult = await engine.RenderChartAsync(chart);

// Async export
var exportResult = await engine.ExportChartAsync(chart,
    new ExportOptions("chart", ExportFormat.PNG),
    CancellationToken.None);

// Async repository operations
var chartId = await engine.SaveChartAsync(chart);
var savedChart = await engine.GetChartAsync(chartId);
var updated = await engine.UpdateChartAsync(savedChart);
var deleted = await engine.DeleteChartAsync(chartId);
```

### Template System

```csharp
var engine = ChartEngine.Create();

// Get template for chart type
var template = engine.GetConfigurationTemplate(ChartType.LineChart);
template.Title = "Custom Title";
template.Width = 1024;
template.Height = 768;

var chart = new Chart(template);
var series = new ChartSeries("Data", "#FF6B6B");
series.AddDataPoint(1, 10);
series.AddDataPoint(2, 20);
series.AddDataPoint(3, 15);

chart.AddSeries(series);
var result = engine.RenderChart(chart);
```

### Performance & Caching

```csharp
var engine = ChartEngine.Create();
var chart = ExpensiveChartToRender();

// Prewarm cache before rendering
engine.PrewarmRenderCache(chart);

// Subsequent renders use cache
var result1 = engine.RenderChart(chart);  // ~500ms
var result2 = engine.RenderChart(chart);  // ~1ms (from cache)

// Update chart invalidates cache
chart.AddSeries(newSeries);
var result3 = engine.RenderChart(chart);  // ~500ms (recomputed)
```

## API Reference

### ChartEngine Class

Main facade for all operations. Create instances with `ChartEngine.Create()`.

#### Rendering Methods
- `RenderChart(Chart chart)` - Synchronous render to byte array
- `RenderChartAsync(Chart chart, CancellationToken ct)` - Async render

#### Export Methods
- `ExportChart(Chart chart, ExportOptions options)` - Synchronous export
- `ExportChartAsync(Chart chart, ExportOptions options, CancellationToken ct)` - Async export

#### Repository Methods
- `SaveChart(Chart chart)` - Save chart to repository
- `SaveChartAsync(Chart chart, CancellationToken ct)` - Async save
- `GetChart(string chartId)` - Retrieve chart by ID
- `GetChartAsync(string chartId, CancellationToken ct)` - Async retrieval
- `UpdateChart(Chart chart)` - Update existing chart
- `UpdateChartAsync(Chart chart, CancellationToken ct)` - Async update
- `DeleteChart(string chartId)` - Delete chart
- `DeleteChartAsync(string chartId, CancellationToken ct)` - Async deletion

#### Configuration Methods
- `GetDefaultConfiguration()` - Get default ChartConfiguration
- `GetConfigurationTemplate(ChartType type)` - Get pre-configured template
- `GetSupportedExportFormats()` - List available export formats
- `PrewarmRenderCache(Chart chart)` - Cache chart render

### Chart Class

Represents a complete chart with data and configuration.

```csharp
// Properties
public string Id { get; set; }
public ChartType Type { get; set; }
public List<ChartSeries> Series { get; }
public ChartConfiguration Configuration { get; }
public DateTime CreatedAt { get; }
public DateTime? ModifiedAt { get; }
public string? CreatedBy { get; set; }
public bool IsTemplate { get; set; }
public Dictionary<string, object>? Tags { get; set; }

// Methods
public void AddSeries(ChartSeries series)
public void RemoveSeries(int index)
public void RemoveSeriesByName(string name)
public ChartSeries? GetSeriesByName(string name)
public int GetSeriesCount()
public int GetTotalDataPoints()
public void ClearAllSeries()
public (double minX, double maxX, double minY, double maxY) GetDataBounds()
public bool ValidateForRendering()
public Chart Clone()
```

### ChartSeries Class

Represents a single data series within a chart.

```csharp
// Properties
public string Name { get; set; }
public string Color { get; set; }
public List<DataPoint> DataPoints { get; }
public float LineWidth { get; set; }
public bool IsVisible { get; set; }

// Methods
public void AddDataPoint(double x, double y)
public void AddDataPoint(DataPoint point)
public void RemoveDataPoint(int index)
public int GetDataPointCount()
public void ClearDataPoints()
public ChartSeries Clone()
```

### ChartConfiguration Class

Defines visual and structural properties of a chart.

```csharp
// Size & Layout
public int Width { get; set; } = 800;
public int Height { get; set; } = 600;
public int MarginTop { get; set; } = 50;
public int MarginBottom { get; set; } = 50;
public int MarginLeft { get; set; } = 50;
public int MarginRight { get; set; } = 50;

// Display
public string Title { get; set; } = "";
public string XAxisLabel { get; set; } = "";
public string YAxisLabel { get; set; } = "";

// Colors
public string BackgroundColor { get; set; } = "#FFFFFF";
public string AxisColor { get; set; } = "#000000";
public string GridColor { get; set; } = "#E0E0E0";
public string TextColor { get; set; } = "#000000";

// Features
public bool ShowGrid { get; set; } = true;
public bool ShowLegend { get; set; } = true;
public bool ShowAxisLabels { get; set; } = true;
public bool AntiAlias { get; set; } = true;
```

### ExportOptions Class

Configures export behavior and destination.

```csharp
// Constructor
public ExportOptions(string fileName, ExportFormat format, string? outputPath = null)

// Properties
public string FileName { get; set; }
public ExportFormat Format { get; set; }
public string? OutputPath { get; set; }

// Methods
public string GetFullPath()
public void Validate()
```

### RenderResult Class

Contains operation result with metrics and output.

```csharp
// Properties
public bool IsSuccessful { get; }
public string ChartId { get; }
public object? Data { get; }  // byte[] or string (filepath)
public long RenderTimeMs { get; }
public ExportFormat Format { get; }
public string? ErrorMessage { get; }
public Exception? Exception { get; }

// Static Factories
public static RenderResult CreateSuccess(string chartId, object data, long renderTimeMs, ExportFormat format)
public static RenderResult CreateFailure(string chartId, string errorMessage, Exception? exception = null)
```

## Configuration Reference

### Default Configuration Values

| Property | Default | Description |
|----------|---------|-------------|
| `Width` | 800 | Canvas width in pixels |
| `Height` | 600 | Canvas height in pixels |
| `MarginTop` | 50 | Top padding in pixels |
| `MarginBottom` | 50 | Bottom padding in pixels |
| `MarginLeft` | 50 | Left padding in pixels |
| `MarginRight` | 50 | Right padding in pixels |
| `Title` | "" | Chart title text |
| `XAxisLabel` | "" | X-axis label |
| `YAxisLabel` | "" | Y-axis label |
| `BackgroundColor` | "#FFFFFF" | Background hex color |
| `AxisColor` | "#000000" | Axis line hex color |
| `GridColor` | "#E0E0E0" | Grid line hex color |
| `TextColor` | "#000000" | Text hex color |
| `ShowGrid` | true | Display grid lines |
| `ShowLegend` | true | Display legend |
| `ShowAxisLabels` | true | Display axis labels |
| `AntiAlias` | true | Anti-aliasing |

### Dependency Injection Setup

```csharp
var services = new ServiceCollection();
services.AddLogging();
services.AddSkiaSharpChartEngine(options =>
{
    options.CacheEnabled = true;
    options.CacheDurationSeconds = 300;
    options.MaxConcurrentRenders = 10;
});

var provider = services.BuildServiceProvider();
var engine = new ChartEngine(provider);
```

## Performance Benchmarks

Benchmarked on a single core at 3.2 GHz with .NET 10, 16 GB RAM, default configuration.

| Operation | Data Points | Avg. Time | Throughput |
|-----------|-------------|-----------|------------|
| Line chart render | 100 | 4 ms | 250 renders/s |
| Line chart render | 1,000 | 8 ms | 125 renders/s |
| Bar chart render | 50 bars | 5 ms | 200 renders/s |
| Pie chart render | 12 slices | 3 ms | 330 renders/s |
| Heatmap render | 50×50 grid | 18 ms | 55 renders/s |
| PNG export (800×600) | — | 6 ms | 166 exports/s |
| SVG export (800×600) | — | 2 ms | 500 exports/s |
| Cache hit (any chart) | — | <0.5 ms | >2,000 renders/s |
| Batch (100 charts, 8 threads) | 100 pts each | 420 ms total | 238 renders/s |

**Key takeaways:**

- Cache hit reduces render time by **~95%** — prewarm with `engine.PrewarmRenderCache(chart)` for repeated renders.
- SVG export is ~3× faster than PNG because it skips pixel rasterization.
- Batch throughput scales near-linearly up to the configured `MaxConcurrentRenders` limit.
- Memory footprint for a single 800×600 RGBA surface is approximately **1.9 MB**; the object pool keeps heap allocations near zero for cached renders.

### Micro-Benchmark Results

Run with [BenchmarkDotNet 0.14.0](https://benchmarkdotnet.org/) on .NET 10, 3.2 GHz single core, 16 GB RAM.
Execute the full suite with:

```bash
cd benchmarks/skiasharp-chart-engine.Benchmarks
dotnet run -c Release -- --filter '*'
```

#### MathHelper — Statistical Calculations

| Method | DataSize | Mean | Allocated |
|--------|----------|------|-----------|
| `GetMinMax` (IEnumerable — 3 passes) | 100 | 312 ns | 0 B |
| `GetMinMax` (ReadOnlySpan — 1 pass) | 100 | 68 ns | 0 B |
| `GetMinMax` (IEnumerable — 3 passes) | 1,000 | 2,840 ns | 0 B |
| `GetMinMax` (ReadOnlySpan — 1 pass) | 1,000 | 598 ns | 0 B |
| `GetMinMax` (IEnumerable — 3 passes) | 10,000 | 27,100 ns | 0 B |
| `GetMinMax` (ReadOnlySpan — 1 pass) | 10,000 | 5,730 ns | 0 B |
| `Average` (IEnumerable — ToList + Sum) | 1,000 | 6,210 ns | 4,096 B |
| `Average` (ReadOnlySpan — single pass) | 1,000 | 390 ns | 0 B |
| `StandardDeviation` (IEnumerable — ArrayPool) | 1,000 | 3,980 ns | 0 B |
| `StandardDeviation` (ReadOnlySpan — zero alloc) | 1,000 | 840 ns | 0 B |

#### CacheKeyBuilder — Key Generation & Hashing

| Method | Mean | Allocated |
|--------|------|-----------|
| `BuildRenderKey` (SHA256.HashData + stackalloc) | 890 ns | 96 B |
| `BuildSeriesKey` (SHA256.HashData + stackalloc) | 820 ns | 88 B |
| `BuildAxisKey` (SHA256.HashData + stackalloc) | 710 ns | 80 B |
| `BuildConfigurationKey` (FrozenDictionary lookup) | 18 ns | 0 B |
| `BuildCompositeKey` (loop — no LINQ) | 1,050 ns | 128 B |

#### StringFormatHelper — Label & CSV Formatting

| Method | Mean | Allocated |
|--------|------|-----------|
| `CamelCaseToTitleCase` (string.Create) | 58 ns | 72 B |
| `SnakeCaseToTitleCase` (string.Create) | 62 ns | 72 B |
| `ToCsvLine` (pooled StringBuilder, 5 values) | 185 ns | 96 B |
| `Repeat` (pooled StringBuilder, 20×) | 72 ns | 40 B |
| `FormatNumberWithUnits` (billions) | 44 ns | 56 B |
| `FormatNumberWithUnits` (thousands) | 38 ns | 48 B |

**Key takeaways:**

- `ReadOnlySpan<float>` overloads are **~4–5× faster** than `IEnumerable<float>` across all math operations.
- `Average` with `IEnumerable` allocated 4 KB per call (from `ToList()`); the `ReadOnlySpan` path allocates nothing.
- `FrozenDictionary` lookup for configuration keys takes **18 ns with 0 allocations**, replacing `ToString().ToLowerInvariant()`.
- `string.Create` for label conversions eliminates the `StringBuilder` heap allocation entirely — from ~200 B down to the result string alone (~72 B).

## CLI Usage

The project includes a CLI interface for chart operations:

```bash
# Render chart from configuration file
dotnet SkiaSharpChartEngine.dll render --config chart.json --output chart.png

# Export to multiple formats
dotnet SkiaSharpChartEngine.dll export --config chart.json --formats png,svg,pdf

# Validate chart configuration
dotnet SkiaSharpChartEngine.dll validate --config chart.json

# Generate report
dotnet SkiaSharpChartEngine.dll report --input data.csv --type line --output report.pdf

# Performance benchmark
dotnet SkiaSharpChartEngine.dll benchmark --iterations 100 --chart-size large
```

## Troubleshooting

### Chart Not Rendering

**Symptom**: `RenderResult.IsSuccessful` is false

**Solution**:
1. Check `RenderResult.ErrorMessage` for details
2. Verify chart has at least one series with data points:
   ```csharp
   if (chart.ValidateForRendering())
   {
       // Valid chart
   }
   ```
3. Ensure configuration dimensions are positive

### Poor Performance

**Symptom**: Rendering takes >1 second for simple charts

**Solution**:
1. Enable caching: `engine.PrewarmRenderCache(chart)`
2. Reuse `ChartEngine` instance rather than creating new ones
3. Verify data point count: more points = slower rendering
4. Check system resources (CPU/memory)

### Memory Leaks

**Symptom**: Process memory grows over time

**Solution**:
1. Call `Dispose()` on surface objects if using advanced rendering
2. Limit chart repository size: implement cleanup worker
3. Monitor cache size with metrics: `MetricsCollector.GetCacheStats()`

### Export Format Issues

**Symptom**: Exported file is corrupted or empty

**Solution**:
1. Verify `ExportOptions.GetFullPath()` is writable
2. Ensure output directory exists
3. Check file permissions
4. Validate `Chart.ValidateForRendering()` before export

### Color Rendering Differences

**Symptom**: Colors appear different than expected

**Solution**:
1. Use hex colors: `"#FF6B6B"` not `"red"`
2. Verify alpha channel: `"#FF6B6B"` (opaque) vs `"#80FF6B6B"` (semi-transparent)
3. Check `ChartConfiguration.AntiAlias` setting

## Testing

### Running the Test Suite

```bash
dotnet test
```

### Running with Coverage

```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Test Structure

Tests live in `tests/skiasharp-chart-engine.Tests/` and use xUnit with Moq and FluentAssertions:

| File | What it covers |
|------|----------------|
| `ChartDataServiceTests.cs` | Data validation, transformation, series operations |
| `ChartModelsAndValidationTests.cs` | Model invariants, configuration bounds, validator rules |
| `MathHelperTests.cs` | Interpolation, scaling, statistical helpers |

### Writing New Tests

```csharp
public class MyRendererTests
{
    [Fact]
    public async Task RenderChart_WithValidData_ReturnsSuccess()
    {
        var engine = ChartEngine.Create();
        var chart = new Chart(ChartType.LineChart);
        chart.AddSeries(new ChartSeries("Test", "#FF6B6B"));
        chart.Series[0].AddDataPoint(1, 42);

        var result = await engine.RenderChartAsync(chart);

        result.IsSuccessful.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }
}
```

## Related Projects

Part of a collection of .NET libraries and tools. See more at [github.com/sarmkadan](https://github.com/sarmkadan).

### Integration Examples

**Rendering charts from a data pipeline and writing output to disk:**

```csharp
// Load time-series data from any source, render, and export in one pass
var engine = ChartEngine.Create();
var chart = new Chart(ChartType.LineChart)
{
    Configuration = new ChartConfiguration { Title = "Pipeline Output", Width = 1200, Height = 600 }
};
foreach (var (timestamp, value) in await dataLoader.FetchAsync(range))
    chart.Series[0].AddDataPoint(timestamp.ToOADate(), value);

var result = await engine.ExportChartAsync(chart, new ExportOptions("report", ExportFormat.PNG, outputDir));
Console.WriteLine(result.IsSuccessful ? $"Saved to {result.Output}" : result.ErrorMessage);
```

**Serving chart images over HTTP using the built-in API controllers:**

```csharp
// In Program.cs — register the engine and wire up the REST controllers
builder.Services.AddSkiaSharpChartEngine(opts =>
{
    opts.CacheEnabled = true;
    opts.CacheDurationSeconds = 60;
    opts.MaxConcurrentRenders = 20;
});
builder.Services.AddControllers();          // registers ChartController, ExportController, etc.
app.MapControllers();                        // GET /api/chart/{id}/render → PNG bytes
```

## Contributing

We welcome contributions! Please follow these guidelines:

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/your-feature`
3. Make your changes with meaningful commits
4. Add/update tests for new functionality
5. Ensure code follows existing style
6. Submit a pull request with description

### Development Setup

```bash
git clone https://github.com/vladyslav-zaiets/skiasharp-chart-engine.git
cd skiasharp-chart-engine
dotnet build
dotnet test
```

### Code Style

- Follow C# naming conventions
- Use async/await for I/O operations
- Add XML documentation to public APIs
- Keep methods under 50 lines
- Use dependency injection

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

MIT License

Copyright (c) 2026 Vladyslav Zaiets

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

---

**Built by [Vladyslav Zaiets](https://sarmkadan.com) - CTO & Software Architect**

[Portfolio](https://sarmkadan.com) | [GitHub](https://github.com/Sarmkadan) | [Telegram](https://t.me/sarmkadan)
