# LegendRenderer

Renders a legend box with a color swatch and name for each visible series. The box is placed at one of the four corners of the chart bounds.

## Overview

The `LegendRenderer` class draws a rounded, semi-transparent white box that lists each visible, named series in `Chart.Series`. Each entry shows a filled color swatch followed by the series name. Chart renderers such as `BarChartRenderer` create one instance and call `Render` after drawing the plot area.

The class is defined in `Rendering/LegendRenderer.cs` together with the `LegendCorner` enum.

> **Build note:** `skiasharp-chart-engine.csproj` contains `<Compile Remove="Rendering/**/*.cs" />`, so this file is excluded from the main build. Check the csproj before relying on it being compiled.

## Properties

`LegendRenderer` exposes no public properties.

## Constructor

```csharp
public LegendRenderer(ILogger<LegendRenderer> logger)
```

Parameters:
- `logger`: Logger used for legend diagnostics. Throws `ArgumentNullException` if `null`.

## Methods

### Public Methods

| Method | Description |
|--------|-------------|
| `Render(SKCanvas canvas, Chart chart, SKRect bounds, LegendCorner corner = LegendCorner.TopRight)` | Draws the legend for the chart's visible, named series in the given corner of `bounds`. Does nothing if there is nothing to draw. |

### Private Methods

| Method | Description |
|--------|-------------|
| `CalculateLegendWidth` | Returns the box width from the widest series name (measured in bold) plus swatch, spacing and padding. |
| `CalculateLegendHeight` | Returns the box height as one item row per series plus padding. |
| `CalculateLegendBounds` | Maps the requested `LegendCorner` to a rectangle inside `bounds`. |
| `DrawLegendBackground` | Draws the filled rounded background and the gray border. |
| `DrawLegendItems` | Draws the swatch and name for each series, one row per series. |
| `ParseColor` | Converts `ChartSeries.Color` to `SKColor`, falling back to blue. |

## Enum: LegendCorner

| Value | Position |
|-------|----------|
| `TopLeft` | Top-left corner of `bounds`. |
| `TopRight` | Top-right corner of `bounds`. This is the default. |
| `BottomLeft` | Bottom-left corner of `bounds`. |
| `BottomRight` | Bottom-right corner of `bounds`. |

Any value outside these four falls through to `TopRight`.

## Rendering Details

### Series Filtering

1. A series is included only if `IsVisible` is `true` and `Name` is not null, empty or whitespace.
2. If `canvas`, `chart`, or `chart.Series` is `null`, or the chart has no series, the method logs at debug level and returns.
3. If no series remain after filtering, the method logs at debug level and returns. Nothing is drawn.

### Layout Calculation

Constants used: `LegendItemHeight` 20, `LegendSwatchWidth` 20, `LegendPadding` 10, `LegendSpacing` 5.

- **Width**: `LegendSwatchWidth + LegendSpacing + widest name + LegendPadding * 2`. The name is measured with `ChartConstants.LegendFontSize` in bold. The result is truncated to `int`.
- **Height**: `LegendItemHeight * seriesCount + LegendPadding * 2`.
- **Position**: The box sits `LegendPadding` pixels inside the chosen corner of `bounds`.

### Drawing

1. **Background**: A rounded rectangle with radius 4, filled with white at alpha 240.
2. **Border**: A 1 px stroke in gray at alpha 180, same radius.
3. **Items**: Starting at `bounds.Top + LegendPadding`, each series gets one row:
   - A filled swatch 20 × 20 px at `LegendPadding` from the left edge.
   - The series name at `LegendSwatchWidth + LegendSpacing` from the left edge, in `ChartConstants.DefaultTextColor`, bold, `ChartConstants.LegendFontSize`. The baseline is 3 px above the row bottom.

### Color Parsing

`ParseColor` takes `ChartSeries.Color`:
- A null, empty or whitespace value returns `SKColors.Blue`.
- Otherwise it tries `SKColor.TryParse`. Any string it accepts (for example `#RRGGBB`) is used as is.
- If parsing fails or throws, it returns `SKColors.Blue`.

### Error Handling

`Render` wraps all of its work in a `try`/`catch (Exception)`. Any exception is logged at error level and not rethrown, so a failing legend never aborts the chart render.

## Constants

| Constant | Value | Purpose |
|----------|-------|---------|
| `LegendItemHeight` | 20 | Row height and swatch height in pixels. |
| `LegendSwatchWidth` | 20 | Swatch width in pixels. |
| `LegendPadding` | 10 | Inset from the chart corner and inside the box. |
| `LegendSpacing` | 5 | Gap between swatch and text. |

Shared values come from `ChartConstants`: `LegendFontSize` (12) and `DefaultTextColor` (`#333333`).

## Usage Example

```csharp
var legend = new LegendRenderer(loggerFactory.CreateLogger<LegendRenderer>());

// Draw the legend in the bottom-left corner of the chart area
legend.Render(canvas, chart, bounds, LegendCorner.BottomLeft);
```

## Related

- [BarChartRenderer](BarChartRenderer.md): Calls `Render` with `LegendCorner.TopRight`.
- [ChartConstants](ChartConstants.md): Source of the legend font size and text color.
