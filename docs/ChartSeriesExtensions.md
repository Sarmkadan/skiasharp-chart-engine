# ChartSeriesExtensions
The `ChartSeriesExtensions` class is a static class of extension methods for `ChartSeries`. It adds helpers for inspecting the Y values of a series (minimum, maximum, range), checking whether a series is empty, and rescaling Y values into a new range. It lives in the `SkiaSharpChartEngine.Models` namespace, so no extra `using` is needed beyond the models namespace.

## API
* `public static double MinY(this ChartSeries series)`: Returns the smallest Y value in the series. Returns `0` if the series has no data points.
* `public static double MaxY(this ChartSeries series)`: Returns the largest Y value in the series. Returns `0` if the series has no data points.
* `public static double Range(this ChartSeries series)`: Returns `MaxY - MinY`, computed from the series' Y-axis range. Returns `0` if the series has no data points.
* `public static bool IsEmpty(this ChartSeries series)`: Returns `true` if the series contains no data points, `false` otherwise.
* `public static void NormalizeY(this ChartSeries series, double newMin, double newMax)`: Linearly rescales the Y value of every data point from the series' current Y range to `[newMin, newMax]`. The data points are modified in place; nothing is returned. Does nothing if the series is empty or if all Y values are equal (the current range is zero).

All methods throw `ArgumentNullException` when `series` is `null`.

## Usage
```csharp
using SkiaSharpChartEngine.Models;

var series = new ChartSeries { Name = "Temperature" };
series.AddDataPoints(new[]
{
    new DataPoint(0, 10),
    new DataPoint(1, 20),
    new DataPoint(2, 30)
});

double min = series.MinY();      // 10
double max = series.MaxY();      // 30
double range = series.Range();   // 20

// Rescale Y values into [0, 1]; the points are updated in place
series.NormalizeY(0, 1);         // Y values become 0, 0.5, 1

bool empty = new ChartSeries { Name = "Empty" }.IsEmpty(); // true
```

## Notes
* `MinY`, `MaxY` and `Range` return `0` for an empty series rather than throwing. Use `IsEmpty()` first if you need to distinguish "no data" from "all values are zero".
* `NormalizeY` mutates the existing `DataPoint` objects, so it affects every chart or collection that shares those points. Unlike the `ChartExtensions` helpers, it does not return a new series.
* When all Y values are equal, `NormalizeY` leaves the values unchanged instead of dividing by zero.
* `DataPoint.Y` rejects `NaN` and infinite values, so the results of these methods are always finite for a series built through the public API.
