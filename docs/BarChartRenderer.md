# BarChartRenderer

Specialized renderer for bar charts. Supports grouped bars, stacked bars, and horizontal/vertical orientation.

## Overview

The `BarChartRenderer` class implements the `IChartRenderer` interface to render bar charts using SkiaSharp. It supports both grouped (side-by-side) and stacked bar configurations.

## Properties

| Property | Type | Description |
|----------|------|-------------|
| `Stacked` | `bool` | When true, bars are rendered in stacked mode (cumulative per category). Default is `false`. |

## Constructor

```csharp
public BarChartRenderer(ILogger<BarChartRenderer> logger)
```

Parameters:
- `logger`: Logger instance for rendering diagnostics.

## Methods

### Public Methods

| Method | Description |
|--------|-------------|
| `Render(SKCanvas canvas, Chart chart, SKRect bounds)` | Renders the bar chart to the provided canvas within the specified bounds. |

### Private Methods

| Method | Description |
|--------|-------------|
| `_renderTitleAndSubtitle` | Renders chart title and subtitle if present. |
| `_renderBars` | Renders the bars based on stacked or grouped mode. |
| `_renderAxes` | Renders X and Y axes with labels. |
| `_getColor` | Returns a color from a predefined palette based on series index. |

## Rendering Details

### Layout Calculation

1. **Title/Subtitle Space**: Calculates height required for title and subtitle to adjust the plot area.
2. **Chart Bounds**: Inner rectangle after accounting for padding and title/subtitle space.
3. **Value Range**: Determines minimum and maximum values for scaling:
   - In stacked mode: computes cumulative sums per category.
   - In grouped mode: uses raw values across all series.
4. **Bar Dimensions**:
   - **Stacked Mode**: Single bar per category with height representing cumulative value.
   - **Grouped Mode**: Multiple bars per category (one per series) with spacing between groups.
5. **Value Labels**: Draws numeric values on top of each bar segment (stacked) or bar (grouped).
6. **Axes**: Renders X and Y axes with tick labels on Y-axis.
7. **Legend**: Renders legend if series have names.

### Constants

The class uses several private constants for layout and styling:
- `ChartPadding`: 40f (padding around chart)
- `TitleSpacing`: 8f (space between title and subtitle)
- `SubtitleSpacing`: 6f (space after subtitle)
- `TitleTopOffset`: 4f (title vertical offset)
- `SubtitleTopSpacing`: 2f (subtitle vertical offset)
- `BarSpacing`: 2f (space between bars in grouped mode)
- `ValueLabelFontSize`: 9f (font size for value labels)
- `ValueLabelCenterDivisor`: 2f (used for centering value labels)
- `ValueLabelTopOffset`: 5f (offset above bar for value labels)
- `AxisStrokeWidth`: 1f (stroke width for axes)
- `AxisLabelFontSize`: 10f (font size for axis labels)
- `AxisIntervalCount`: 5f (number of Y-axis intervals)
- `AxisLabelHorizontalOffset`: 35f (horizontal offset for Y-axis labels)
- `AxisLabelVerticalOffset`: 3f (vertical offset for Y-axis labels)

### Color Palette

Uses a predefined array of colors: Blue, Red, Green, Orange, Purple. Colors are assigned to series in order and repeat if there are more series than colors.

## Usage Example

```csharp
var renderer = new BarChartRenderer(logger);
renderer.Render(canvas, chart, new SKRect(0, 0, 800, 600));
```

## Notes

- The renderer expects a valid `Chart` object with at least one series containing data points.
- Null or invalid parameters result in early return with a warning log.
- Errors during rendering are caught and logged as errors.
- The renderer logs information at the start of rendering and debug information upon completion.