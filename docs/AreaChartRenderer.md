# AreaChartRenderer

The `AreaChartRenderer` class renders area charts by drawing lines for each series and filling the area beneath them down to the baseline. It reuses axis and grid drawing logic from the line renderer for visual consistency and supports configurable fill opacity.

## API

### `public AreaChartRenderer(ILogger<AreaChartRenderer> logger)`

Initializes a new instance of the `AreaChartRenderer` with the specified logger.

- **Parameters**:
  - `logger`: The logger instance used for rendering diagnostics.
- **Exceptions**: 
  - `ArgumentNullException` if `logger` is null.

### `public void Render(SKCanvas canvas, Chart chart, SKRect bounds)`

Renders an area chart onto the supplied `SKCanvas`.

- **Parameters**:
  - `canvas`: The SkiaSharp canvas to draw on.
  - `chart`: The chart model containing series and data points to render.
  - `bounds`: The drawing bounds within the canvas.
- **Behavior**:
  - If `canvas`, `chart`, or `chart.Series` is null or empty, logs a warning and returns.
  - Calculates space for title and subtitle, then renders them.
  - For each series with at least two data points:
    - Computes screen coordinates for data points.
    - Constructs a path consisting of the line through all points and down to the baseline.
    - Fills the path with the series color at `FillOpacity` (0.3) opacity.
    - Strokes the path with the series color at full opacity.
    - Draws markers at each data point.
  - Renders X and Y axes with labels.
  - Renders a legend if any series have names.
  - Logs rendering progress and errors.

## Usage

### Example: Basic area chart rendering

```csharp
using SkiaSharp;
using Microsoft.Extensions.Logging;
using SkiasharpChartEngine.Models;
using SkiasharpChartEngine.Rendering;

// Setup logger, chart data, and surface
ILogger<AreaChartRenderer> logger = LoggerFactory.Create(builder => 
    builder.AddConsole()).CreateLogger<AreaChartRenderer>();
var renderer = new AreaChartRenderer(logger);

var chart = new Chart
{
    Id = "area-chart-1",
    Configuration = new ChartConfiguration
    {
        Title = "Sample Area Chart",
        Subtitle = "Demonstrating basic usage",
        TextColor = "#000000"
    },
    Series = new List<ChartSeries>
    {
        new ChartSeries
        {
            Name = "Series A",
            DataPoints = new List<DataPoint>
            {
                new DataPoint { X = 0, Value = 10 },
                new DataPoint { X = 1, Value = 25 },
                new DataPoint { X = 2, Value = 15 },
                new DataPoint { X = 3, Value = 30 },
                new DataPoint { X = 4, Value = 20 }
            }
        }
    }
};

using var surface = SKSurface.Create(new SKImageInfo(800, 600));
var canvas = surface.Canvas;

// Render the chart to the entire surface
renderer.Render(canvas, chart, new SKRect(0, 0, 800, 600));

// Save or display the resulting image...
```

### Example: Multiple series with legend

```csharp
// ... (same setup as above)

var chart = new Chart
{
    Id = "multi-series-area",
    Configuration = new ChartConfiguration
    {
        Title = "Multi-Series Area Chart",
        TextColor = "#222222"
    },
    Series = new List<ChartSeries>
    {
        new ChartSeries
        {
            Name = "Product A",
            DataPoints = new List<DataPoint>
            {
                new DataPoint { X = 0, Value = 5 },
                new DataPoint { X = 1, Value = 12 },
                new DataPoint { X = 2, Value = 9 },
                new DataPoint { X = 3, Value = 15 },
                new DataPoint { X = 4, Value = 7 }
            }
        },
        new ChartSeries
        {
            Name = "Product B",
            DataPoints = new List<DataPoint>
            {
                new DataPoint { X = 0, Value = 3 },
                new DataPoint { X = 1, Value = 8 },
                new DataPoint { X = 2, Value = 6 },
                new DataPoint { X = 3, Value = 10 },
                new DataPoint { X = 4, Value = 4 }
            }
        }
    }
};

renderer.Render(canvas, chart, new SKRect(0, 0, 800, 600));
// The legend will appear in the top-right corner automatically.
```

## Notes

- **Fill Opacity**: The area fill uses a constant opacity of `0.3` (30%) to ensure the line remains visible and overlapping series can be distinguished.
- **Marker Size**: Data point markers are drawn with a radius of `4` pixels, matching the line renderer style.
- **Baseline**: The filled area extends down to the bottom of the chart bounds (the baseline), regardless of the minimum data value.
- **Axis Reuse**: Axis drawing (lines and labels) is identical to that used by `LineChartRenderer` to maintain visual consistency across chart types.
- **Error Handling**: Exceptions during rendering are caught and logged as errors; the method does not throw.
- **Thread Safety**: This class is not thread-safe due to its use of mutable rendering state; however, instantiating a new renderer per thread or chart is safe.
- **Dependencies**: Relies on `SkiaSharp` for drawing and `Microsoft.Extensions.Logging` for diagnostics.