# CandlestickChartRenderer

The `CandlestickChartRenderer` class renders candlestick (OHLC) charts for financial data. Each data point is drawn as a candle: a rectangle spanning the open and close values, with a vertical wick spanning the high and low values. Bullish candles (close at or above open) are drawn in dark green, bearish candles (close below open) in dark red.

> **Build status:** `Rendering/CandlestickChartRenderer.cs` is currently excluded from compilation by the `<Compile Remove="Rendering/**/*.cs" />` entry in `skiasharp-chart-engine.csproj`. The file also reads `DataPoint.CustomProperties`, which is not defined on `DataPoint` (it exists on `ChartSeries`), so it would not compile if the exclusion were removed. See [Known Limitations](#known-limitations).

## API

### `public CandlestickChartRenderer(ILogger<CandlestickChartRenderer> logger)`

Initializes a new instance of the `CandlestickChartRenderer` with the specified logger.

- **Parameters**:
  - `logger`: The logger instance used for rendering diagnostics.
- **Exceptions**:
  - `ArgumentNullException` if `logger` is null.

### `public void Render(SKCanvas canvas, Chart chart, SKRect bounds)`

Renders a candlestick chart onto the supplied `SKCanvas`. Implements `IChartRenderer.Render`.

- **Parameters**:
  - `canvas`: The SkiaSharp canvas to draw on.
  - `chart`: The chart model containing series and OHLC data points.
  - `bounds`: The drawing bounds within the canvas.
- **Behavior**:
  - If `canvas`, `chart`, or `chart.Series` is null, or `chart.Series` is empty, logs a warning and returns without drawing.
  - Reserves 40 pixels of padding on each side of `bounds`, plus extra height for the title (`ChartConstants.TitleFontSize + 8`) and subtitle (`ChartConstants.SubtitleFontSize + 6`) when they are set.
  - Computes the value range from the `High` and `Low` custom properties of all data points across all series:
    - Minimum is the lowest `Low`, maximum is the highest `High`.
    - If no `High`/`Low` values exist, the range defaults to `0`–`100`.
    - If the range is greater than zero, it is padded by 5% at each end.
    - If the range is zero, the bounds are widened by 5 units on each side.
  - Renders the title and subtitle centered at the top of `bounds`.
  - Renders the candlesticks into the padded plot area (see [Candle Layout](#candle-layout)).
  - Renders the X and Y axes with six evenly spaced value labels on the Y axis.
  - Logs rendering progress and errors. Any exception is caught and logged at error level; the method does not throw.

## Data Requirements

Each `DataPoint` must carry its OHLC values in `CustomProperties` under these exact keys:

| Key     | Meaning                         |
|---------|---------------------------------|
| `Open`  | Opening price of the period     |
| `High`  | Highest price of the period     |
| `Low`   | Lowest price of the period      |
| `Close` | Closing price of the period     |

Values are converted with `Convert.ToDouble`, so any numeric type (or a numeric string) is accepted.

A data point that is missing any of the four keys, or has no `CustomProperties` dictionary, is skipped with a warning. The rest of the chart still renders.

## Candle Layout

- The candle slot width is `bounds.Width / (pointCount * 1.5)`, where `pointCount` is the number of data points in the **first** series.
- The horizontal step between candles is `1.5 ×` the candle width, which leaves room for wicks between bodies.
- Each candle's wick is drawn at the horizontal center of its slot, from `High` to `Low`, with a 2-pixel stroke.
- The body is a filled rectangle from `Open` to `Close` across the full candle width, with a 0.5-pixel black outline.
- Multiple series are drawn at the same horizontal position for each index, so later series paint over earlier ones.
- Y coordinates are inverted so that higher values appear higher on the canvas.

## Usage

### Example: Basic candlestick chart rendering

```csharp
using SkiaSharp;
using Microsoft.Extensions.Logging;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Rendering;

// Setup logger, chart data, and surface
ILogger<CandlestickChartRenderer> logger = LoggerFactory.Create(builder =>
    builder.AddConsole()).CreateLogger<CandlestickChartRenderer>();
var renderer = new CandlestickChartRenderer(logger);

var chart = new Chart
{
    Id = "candles-1",
    Configuration = new ChartConfiguration
    {
        Title = "ACME Daily Prices",
        Subtitle = "Sample OHLC data",
        TextColor = "#000000"
    },
    Series = new List<ChartSeries>
    {
        new ChartSeries
        {
            Name = "ACME",
            DataPoints = new List<DataPoint>
            {
                new DataPoint
                {
                    X = 0,
                    Value = 102,
                    CustomProperties = new Dictionary<string, object>
                    {
                        ["Open"] = 100.0, ["High"] = 106.0, ["Low"] = 98.5, ["Close"] = 104.0
                    }
                },
                new DataPoint
                {
                    X = 1,
                    Value = 101,
                    CustomProperties = new Dictionary<string, object>
                    {
                        ["Open"] = 104.0, ["High"] = 105.0, ["Low"] = 99.0, ["Close"] = 101.0
                    }
                }
            }
        }
    }
};

using var surface = SKSurface.Create(new SKImageInfo(800, 600));
var canvas = surface.Canvas;

renderer.Render(canvas, chart, new SKRect(0, 0, 800, 600));
```

## Notes

- **Colors**: Bullish candles use `SKColors.DarkGreen` for both body and wick; bearish candles use `SKColors.DarkRed`. A candle with `Close == Open` is treated as bullish.
- **Body Outline**: All bodies are outlined in black at 0.5 pixels, independent of the chart's `TextColor`.
- **Axis Labels**: Y-axis values are formatted with `"F0"` (no decimal places). Instruments priced below about 10 will show repeated or unclear labels.
- **No X-Axis Labels**: The X axis is a plain line. Category or date labels are not drawn.
- **Series Alignment**: Only the first series determines the number of candle slots. Other series are indexed by the same position.
- **Dependencies**: Relies on `SkiaSharp` for drawing and `Microsoft.Extensions.Logging` for diagnostics.

## Known Limitations

- **Not compiled**: The file is excluded by `Compile Remove="Rendering/**/*.cs"` in the project file.
- **Wrong property owner**: `DataPoint.CustomProperties` is referenced, but `DataPoint` does not define that property. Compiling the file would fail until OHLC values are read from a property that exists on `DataPoint`, or the property is added to the model.
- **Series length mismatch**: If a later series has fewer data points than the first series, indexing throws `ArgumentOutOfRangeException`. The exception is caught in `Render`, so no candles or axes are drawn after the failing point.
- **Invalid text color**: An unparsable `ChartConfiguration.TextColor` makes `SKColor.Parse` throw, which aborts the whole render.
- **Non-numeric OHLC values**: A value that `Convert.ToDouble` cannot parse throws during range calculation, which aborts the whole render.
- **Disposal**: The `SKPaint` and `SKTypeface` objects created in `_renderAxes` are not disposed.
- **Thread Safety**: The class holds only a logger and no mutable state, but the `Render` call draws on a caller-supplied canvas. Do not share one canvas across threads.
