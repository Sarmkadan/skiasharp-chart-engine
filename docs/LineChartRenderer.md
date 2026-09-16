# LineChartRenderer

Specialized renderer for line charts that renders smooth or straight lines connecting data points with optional markers.

## Overview

The `LineChartRenderer` class implements the `IChartRenderer` interface to draw line charts using SkiaSharp. It handles rendering of multiple data series, axes, titles, subtitles, and legends.

## Key Features

- Renders multiple data series as lines with configurable colors
- Optional data point markers
- Automatic axes rendering with dynamic label formatting
- Support for chart titles and subtitles
- Legend generation for series with names
- Batched rendering paths for performance optimization

## Usage

The renderer is typically used through the chart engine's rendering pipeline:

```csharp
var renderer = new LineChartRenderer(logger);
renderer.Render(canvas, chart, bounds);
```

## Implementation Details

### Rendering Process

1. **Parameter Validation**: Checks for null canvas, chart, or empty series
2. **Layout Calculation**: Computes plot area accounting for titles, subtitles, and padding
3. **Value Range Determination**: Calculates min/max values across all series for scaling
4. **Title/Subtitle Rendering**: Draws chart title and subtitle if present
5. **Series Rendering**: Draws each data series as a line with markers
6. **Axes Rendering**: Draws X and Y axes with value labels
7. **Legend Rendering**: Draws legend if series have names

### Coordinate System

- Chart coordinates are mapped from data values to pixel positions within the calculated plot area
- Y-axis inversion is handled automatically (higher values appear higher on the chart)
- X-axis spacing is uniform based on the number of data points

### Performance Optimizations

- Uses batched `SKPath` objects for line and marker rendering to minimize draw calls
- Pre-calculates series colors to avoid per-point computations
- Early exit for invalid or empty data

## Dependencies

- `SkiaSharp` for graphics rendering
- `Microsoft.Extensions.Logging` for diagnostic logging
- Internal `LegendRenderer` for legend generation
- `ChartConstants` for font sizing

## Notes

- Requires at least two data points per series to render a line
- Series with fewer than two points are skipped
- Automatic decimal formatting for Y-axis labels based on value range to prevent overlap
- Uses a fixed color palette (Blue, Red, Green, Orange, Purple) that repeats for additional series