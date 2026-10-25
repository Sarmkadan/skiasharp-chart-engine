# HeatmapRenderer

Specialized renderer for heatmap charts. Lays the first series' data points out as a near-square grid of cells, colors each cell by its normalized value, and writes the value inside the cell.

## Overview

The `HeatmapRenderer` class implements the `IChartRenderer` interface to render 2D heatmaps using SkiaSharp. It reads the data from the **first series only** and supports three color scale modes selected through `ChartConfiguration.HeatmapColorScale`:

| Mode | Behavior |
|------|----------|
| `HeatmapColorScale.Linear` | Default. Linear mapping from min to max. |
| `HeatmapColorScale.Logarithmic` | Values are log10-compressed before linear mapping, so small differences stay visible across wide value ranges. |
| `HeatmapColorScale.Quantile` | Rank-based mapping, so each color band covers roughly the same number of cells. Suited to skewed distributions. |

The enum is defined in `Constants/ChartConstants.cs`.

## Properties

The class has no public properties. Its only state is a logger and a private constant:

| Member | Type | Description |
|--------|------|-------------|
| `_logger` | `ILogger<HeatmapRenderer>` | Logger for rendering diagnostics. Set in the constructor. |
| `CellPadding` | `const float` | `2f`. Gap between cells and around the grid edge. |

## Constructor

```csharp
public HeatmapRenderer(ILogger<HeatmapRenderer> logger)
```

Parameters:
- `logger`: Logger instance for rendering diagnostics. Must not be `null`; otherwise an `ArgumentNullException` is thrown.

## Methods

### Public Methods

| Method | Description |
|--------|-------------|
| `Render(SKCanvas canvas, Chart chart, SKRect bounds)` | Renders the heatmap into `bounds`. Never throws: invalid input and exceptions are logged and the method returns. |

### Private Methods

| Method | Description |
|--------|-------------|
| `_renderTitleAndSubtitle` | Draws the centered title (bold) and subtitle at the top of `bounds`. |
| `_normalizeValues` | Dispatches to the normalization routine for the selected color scale. |
| `_normalizeLinear` | Linear min-max normalization to `[0, 1]`. |
| `_normalizeLogarithmic` | Shifts values so the minimum is at least 1, applies `log10`, then normalizes linearly. |
| `_normalizeQuantile` | Assigns each value its rank divided by `count - 1`. |
| `_calculateGridDimensions` | Computes the grid shape from the number of points. |
| `_getHeatColor` | Maps a normalized value in `[0, 1]` to a color. |

## Rendering Details

### Render Pipeline

1. **Guard Clauses**: If `canvas` or `chart` is `null`, a warning is logged and the method returns.
2. **Title/Subtitle Space**: Adds `ChartConstants.TitleFontSize + 8` when a title is set and `ChartConstants.SubtitleFontSize + 6` when a subtitle is set. The heatmap area starts below that height.
3. **Data Selection**: Takes `Chart.Series.FirstOrDefault()`. If there is no series, or it has no data points, a warning is logged and nothing is drawn.
4. **Grid Shape**: Computes `rows` and `cols` from the point count (see below).
5. **Normalization**: Normalizes all values with the configured color scale. Missing configuration falls back to `Linear`.
6. **Title/Subtitle**: Drawn at the top of the original `bounds`.
7. **Cells**: Each cell is filled with its heat color, outlined in gray (1px), and labeled with its value formatted as `F1` (one decimal place), centered horizontally and vertically.
8. **Logging**: A debug entry records the grid size and color scale.

Any exception thrown during rendering is caught and logged as an error.

### Grid Layout

The grid shape is derived from the number of data points `n`:

- `rows = floor(sqrt(n))`
- `cols = ceil(n / rows)`

Points fill the grid row by row. If `n` is not a perfect product of `rows` and `cols`, the last row is only partly filled and the remaining cells are left empty.

| Points | Rows | Columns |
|--------|------|---------|
| 1 | 1 | 1 |
| 4 | 2 | 2 |
| 6 | 2 | 3 |
| 10 | 3 | 4 |
| 16 | 4 | 4 |

Cell size:
- `cellWidth = (heatmapBounds.Width - (cols + 1) * CellPadding) / cols`
- `cellHeight = (heatmapBounds.Height - (rows + 1) * CellPadding) / rows`

Each cell is offset by `CellPadding` from its neighbors and from the grid edge.

### Normalization

All normalizers return a value in `[0, 1]` for each input value, in input order.

- **Linear**: `(value - min) / (max - min)`. If all values are equal (range is 0), every cell gets `0.5`, which renders white.
- **Logarithmic**: If the minimum is below 1, all values are shifted by `1 - min`. Then `log10(value + shift)` is computed and the result is normalized linearly. Values are never clamped, so negative inputs are handled by the shift.
- **Quantile**: Values are sorted, and each value gets `rank / (count - 1)`. Ties are ranked by sort order, so equal values can get different colors. With a single value the result is `0.5`.

An empty input list is returned unchanged.

### Color Mapping

`_getHeatColor` interpolates through white:

| Normalized value | Color |
|------------------|-------|
| `0.0` | Blue `(0, 0, 255)` |
| `0.5` | White `(255, 255, 255)` |
| `1.0` | Red `(255, 0, 0)` |

Below `0.5`, red and green rise from 0 to 255 while blue stays at 255. Above `0.5`, green and blue fall from 255 to 0 while red stays at 255.

### Cell Labels

- Values are formatted with `ToString("F1")`, so `12.345` is drawn as `12.3`.
- Text uses a fixed size of 10 and black color. It is centered using `MeasureText`.
- Labels are not clipped. If a cell is too small for its text, the text can overflow the cell.

### Layout Notes

- The title and subtitle are drawn relative to `bounds`, while the cells are drawn in the area below them. The heatmap's top edge is therefore offset by the title height.
- `CellPadding` is not scaled to the chart size.

## Usage Example

```csharp
using Microsoft.Extensions.Logging;
using SkiaSharp;
using SkiaSharpChartEngine.Constants;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Rendering;

var logger = loggerFactory.CreateLogger<HeatmapRenderer>();
var renderer = new HeatmapRenderer(logger);

var chart = new Chart(new ChartConfiguration
{
    Title = "Weekly Activity",
    HeatmapColorScale = HeatmapColorScale.Quantile
});

var series = new ChartSeries();
series.AddDataPoints(values.Select(v => new DataPoint { Value = v }));
chart.AddSeries(series);

using var bitmap = new SKBitmap(800, 600);
using var canvas = new SKCanvas(bitmap);
canvas.Clear(SKColors.White);

renderer.Render(canvas, chart, new SKRect(0, 0, 800, 600));
```

## Dependencies

- `SkiaSharp`: Canvas, paint, and color types.
- `Microsoft.Extensions.Logging`: Logging via `ILogger<T>`.
- `SkiaSharpChartEngine.Constants`: `ChartConstants` (title/subtitle font sizes) and `HeatmapColorScale`.
- `SkiaSharpChartEngine.Models`: `Chart`, `ChartConfiguration`, `ChartSeries`, `DataPoint`.

## Build Note

The `Rendering/` folder is excluded from the main project by `<Compile Remove="Rendering/**/*.cs" />` in `skiasharp-chart-engine.csproj`, so `HeatmapRenderer.cs` is not compiled by `dotnet build` of that project. This document describes the source as written.

## See Also

- `IChartRenderer`: Interface implemented by all renderers.
- `ChartConfiguration`: Source of `HeatmapColorScale`, `Title`, `Subtitle`, and `TextColor`.
- `BarChartRenderer`: Another renderer, with grouped and stacked modes.
