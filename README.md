# SkiaSharp Chart Engine

High-performance chart rendering with SkiaSharp - support for line, bar, pie, heatmap charts with export to PNG/SVG and more.

## Features

- **Multiple Chart Types**: Line, Bar, Pie, Heatmap, Area, Scatter, Column charts
- **High Performance**: Efficient rendering with configurable caching
- **Multiple Export Formats**: PNG, SVG, JPEG, WebP, PDF
- **Rich Configuration**: Extensive customization options for appearance and behavior
- **Data Validation**: Comprehensive validation of chart data
- **Async Support**: Full async/await support for rendering and export operations
- **In-Memory Repository**: Built-in chart storage and retrieval
- **Template System**: Pre-configured templates for quick chart creation
- **Metrics Collection**: Detailed rendering metrics and statistics

## Quick Start

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
        Title = "Sample Line Chart",
        Width = 800,
        Height = 600
    }
};

// Add data series
var series = new ChartSeries("Series 1", "#FF6B6B");
series.AddDataPoint(1, 10);
series.AddDataPoint(2, 20);
series.AddDataPoint(3, 15);
chart.AddSeries(series);

// Render to bytes
var result = engine.RenderChart(chart);

// Or export to file
var exportOptions = new ExportOptions("my_chart", ExportFormat.PNG);
var exportResult = engine.ExportChart(chart, exportOptions);
```

## Supported Chart Types

- **LineChart**: Standard line charts for trend visualization
- **BarChart**: Horizontal bar charts for categorical comparisons
- **ColumnChart**: Vertical bar charts for categorical data
- **PieChart**: Circular charts for composition visualization
- **HeatmapChart**: Color-mapped grids for pattern analysis
- **AreaChart**: Line charts with filled areas
- **ScatterChart**: Point-based charts for correlation analysis

## Export Formats

- PNG (default)
- JPEG
- WebP
- SVG
- PDF

## Architecture

The chart engine follows a layered architecture:

- **Models**: Domain entities (Chart, ChartSeries, DataPoint, etc.)
- **Services**: Business logic (rendering, data validation, caching, export)
- **Repository**: Data persistence (in-memory storage)
- **Configuration**: Dependency injection and engine configuration

## Installation

Add the SkiaSharp Chart Engine NuGet package:

```bash
dotnet add package SkiaSharpChartEngine
```

## Contributing

Contributions are welcome! Please feel free to submit pull requests or open issues for bugs and feature requests.

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Author

Vladyslav Zaiets - https://sarmkadan.com
