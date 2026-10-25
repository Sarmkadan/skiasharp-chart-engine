# ScatterPlotRenderer

Renderer for scatter plots. Draws every data point of every series as a filled circle, colours each series from a fixed eight-colour palette, and draws an X and Y axis with five value labels on the Y axis.

> **Build status:** `Rendering/**/*.cs` is excluded from compilation in `skiasharp-chart-engine.csproj` (`<Compile Remove="Rendering/**/*.cs" />`). This class is therefore not part of the built assembly today. It can only be used once that exclusion is removed or the file is moved out of `Rendering/`.

## Overview

The `ScatterPlotRenderer` class implements the `IChartRenderer` interface. Only the **`Value`** of each `DataPoint` controls vertical placement. The horizontal position is not taken from `DataPoint.X`; it is derived from the point's `Label` (see [Position Mapping](#position-mapping)). This is a simplification, noted in the source as "simplified", and it means the chart does not show true X/Y scatter data.

The class has an optional third-dimension encoding in its summary comment ("size/color encoding"), but the code does not implement it. Point size is always `PointSize` and colour depends only on the series index.

## Properties

The class exposes no public properties. Its only state is the logger and a set of private constants (see [Constants](#constants)).

## Constructor

```csharp
public ScatterPlotRenderer(ILogger<ScatterPlotRenderer> logger)
```

Parameters:
- `logger`: Logger instance for rendering diagnostics. Must not be `null`; otherwise an `ArgumentNullException` is thrown.

## Methods

### Public Methods

| Method | Description |
|--------|-------------|
| `Render(SKCanvas canvas, Chart chart, SKRect bounds)` | Renders the scatter plot onto the provided canvas within the specified bounds. Implements `IChartRenderer.Render`. |

### Private Methods

| Method | Description |
|--------|-------------|
| `_renderAxes(SKCanvas canvas, SKRect bounds, double minValue, double maxValue)` | Draws the X and Y axis lines along the bottom and left edges of `bounds`, then five Y-axis value labels formatted with `"F0"`. |
| `_getColor(int index)` | Returns a colour from a fixed eight-colour palette by index, wrapping with modulo. |

## Rendering Details

### Error Handling

`Render` wraps all of its work in a `try/catch (Exception)`. Errors are logged at `Error` level and **not rethrown**. Callers receive no exception from `Render` for invalid input. Check the log output if a chart appears blank.

### Early Exits

`Render` does not return early for empty data. It always draws the axes. It returns without drawing only when `canvas` or `chart` is `null`, in which case a warning is logged.

`chart.Series` being `null` is not a warning case. It is replaced with an empty list, so only the axes are drawn.

### Layout Calculation

1. **Plot Bounds**: The plot area is `bounds` inset by `ChartPadding` (40 px) on all four sides.
2. **Value Range**: The minimum and maximum of all `DataPoint.Value` across all series.
   - No points at all: the range falls back to `0..1`.
   - Single value, or all values equal (zero span): the range is widened to `value - 1 .. value + 1`.
3. **Axis Labels**: Five evenly spaced divisions (`YAxisDivisions` = 5), so six labels from `minValue` to `maxValue`.

### Position Mapping

- **Y (vertical)**: `normalized = (Value - minValue) / valueRange`, then `y = chartBounds.Bottom - normalized * chartBounds.Height`. The minimum value sits at the bottom edge and the maximum at the top edge.
- **X (horizontal)**: `xNorm = (Label?.GetHashCode() ?? 0) % 100 / 100.0`, then `x = chartBounds.Left + xNorm * chartBounds.Width`.
  - `DataPoint.X` is ignored.
  - Points with a `null` label all map to `x = chartBounds.Left`.
  - `string.GetHashCode()` is randomised per process in .NET, so the horizontal position of a labelled point can change between runs.
  - A negative hash code gives a negative remainder, so `xNorm` is in `(-1, 0]`. Such points are drawn to the left of the plot area, and possibly outside `bounds`.

### Drawing Order

1. **Series points**: For each series in order, the circles are added to one `SKPath` and drawn with a single `DrawPath` call. Each circle has radius `PointSize` (5 px). Fill style, anti-aliased.
2. **Axes**: The X axis is a line along `chartBounds.Bottom`. The Y axis is a line along `chartBounds.Left`. Both are black, 1 px, anti-aliased.
3. **Y-axis labels**: Six labels at `i / 5` of the plot height, from bottom (`i = 0`) to top (`i = 5`). Each label is `value.ToString("F0")`, placed at `Left - 35` px, with a 3 px baseline offset. Text size 10 px, black.

The axes are drawn after the points, so they overlay the points that lie on the edges.

### Value Handling

- Values are used as-is. They are not clipped, and `DataPoint.Value` is not checked for `NaN` or infinity. A non-finite value makes the range or positions non-finite.
- Labels are formatted with `ToString("F0")`, which uses the current culture and rounds to whole numbers. Values under 1 in magnitude may therefore appear as `0`, and the span from `minValue` to `maxValue` may show repeated labels when the range is small.

### Colours

- Series colour: the palette is `Red`, `Blue`, `Green`, `Orange`, `Purple`, `Brown`, `Pink`, `Cyan`. Series `i` uses `palette[i % 8]`.
- Axis lines and Y-axis labels are always black. They do not follow `ChartConfiguration.TextColor`.
- `DataPoint.Color` and `ChartSeries` colour settings are not read.

### Title, Subtitle and Legend

The renderer draws no title, subtitle, X-axis labels or legend. Chart metadata is not read beyond `chart.Id` (used only in a log message) and `chart.Series`.

## Constants

| Constant | Value | Purpose |
|----------|-------|---------|
| `PointSize` | `5f` | Radius in pixels of each data point circle. |
| `ChartPadding` | `40f` | Inset from `bounds` to the plot area on each side. |
| `AxisStrokeWidth` | `1f` | Stroke width for the X and Y axis lines. |
| `AxisLabelTextSize` | `10f` | Font size in pixels for Y-axis value labels. |
| `YAxisDivisions` | `5` | Number of evenly spaced divisions on the Y axis. |
| `YAxisLabelLeftOffset` | `35f` | Horizontal offset from the Y axis to the label anchor (to the left). |
| `YAxisLabelBaselineOffset` | `3f` | Vertical offset applied to Y-axis labels for baseline alignment. |
| `LabelHashModulus` | `100` | Modulus applied to label hash codes to derive the X position. |

## Known Limitations

- **Not compiled**: see the build status note above.
- **X position is not data**: `DataPoint.X` is ignored. Horizontal placement comes from the label hash, so two charts with the same data can look different across runs.
- **Negative hash codes**: points whose label hash is negative are drawn left of the plot area, and may fall outside `bounds`.
- **No X axis scale**: there are no X-axis labels or ticks, so horizontal position carries no readable meaning.
- **Multi-series colours**: colours are fixed by series index and cannot be set per series or per point.
- **Size and colour encoding**: the summary mentions third-dimension encoding, but it is not implemented. `CustomRadius` and `DataPoint.Color` are ignored.
- **Unvalidated values**: `NaN` and infinite values are not filtered.
- **Labels not culture-invariant**: `ToString("F0")` depends on the current culture.
- **Resource disposal**: the `SKPaint` instances created in `Render` and `_renderAxes` are not disposed.
- **Fixed label colours**: axis and label colours do not follow `TextColor`.

## Related

- `IChartRenderer`: the interface this class implements.
- `RadarChartRenderer`: another renderer that uses the `Rendering/` folder pattern and a fixed palette.
- `DataPoint`: the model providing `Value`, `Label`, `X` and `CustomRadius`.
- `ChartSeries`: holds the `DataPoints` each series renders.
- `Chart`: provides `Id` and the `Series` list.
