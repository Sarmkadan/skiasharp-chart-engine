# PieChartRenderer

Specialized renderer for pie charts that renders donut and pie charts with labels, legends, and optional 3D effect.

## Overview

The `PieChartRenderer` class implements the `IChartRenderer` interface to draw pie charts using SkiaSharp. It handles rendering of pie/donut charts with data labels, automatic color assignment, and support for titles and subtitles.

## Key Features

- Renders pie charts with optional donut (inner radius) support
- Automatic data point labeling with percentage values
- Configurable inner radius for donut charts
- Sub-pixel gap correction for high-DPI displays
- Support for chart titles and subtitles
- Automatic color palette assignment
- Diagnostic logging for rendering operations

## Usage

The renderer is typically used through the chart engine's rendering pipeline:

```csharp
var renderer = new PieChartRenderer(logger);
renderer.Render(canvas, chart, bounds, innerRadiusRatio); // innerRadiusRatio: 0 for pie, 0-0.9 for donut
```

## Implementation Details

### Rendering Process

1. **Parameter Validation**: Checks for null canvas, chart, or empty series
2. **Layout Calculation**: Computes chart bounds accounting for titles, subtitles, and padding
3. **Title/Subtitle Rendering**: Draws chart title and subtitle if present
4. **Data Processing**: Calculates total value and processes each data point
5. **Slice Rendering**: Draws each pie slice with optional donut hole
6. **Label Rendering**: Draws percentage labels outside each slice
7. **Logging**: Outputs diagnostic information about the rendering process

### Coordinate System

- Chart is centered within the calculated bounds
- Radius is determined by the smaller dimension of width/height divided by 3
- Slice angles are calculated based on data point values relative to total
- Labels are positioned at a fixed distance outside the pie radius

### Visual Enhancements

- **Sweep Overlap**: Applies a 0.5 degree overlap to each slice to close sub-pixel gaps that appear at segment boundaries on high-DPI/Retina displays
- **Anti-aliasing**: Uses SkiaSharp anti-aliasing for smooth edges
- **Color Cycling**: Automatically assigns colors from a predefined palette that repeats for additional series

### Parameters

- `canvas`: SKCanvas to draw on
- `chart`: Chart object containing data and configuration
- `bounds`: SKRect defining the drawing area
- `innerRadiusRatio`: Ratio of inner radius to outer radius (0 for pie chart, 0-0.9 for donut chart)

## Dependencies

- `SkiaSharp` for graphics rendering
- `Microsoft.Extensions.Logging` for diagnostic logging
- `ChartConstants` for font sizing (used in title/subtitle rendering)

## Notes

- Uses the first series in the chart for data points (mirrors PieChartRenderer behaviour)
- Requires at least one data point with a value greater than zero
- Inner radius ratio must be between 0 and 0.9 (validated in method)
- Uses a fixed color palette (RoyalBlue, OrangeRed, ForestGreen, Gold, Violet, Turquoise, Salmon, SeaGreen) that repeats for additional slices
- Label distance is fixed at 50 pixels from the pie edge
- Text labels use 10pt font size in black color