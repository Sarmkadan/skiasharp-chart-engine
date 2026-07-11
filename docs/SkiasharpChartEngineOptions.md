# SkiasharpChartEngineOptions
The `SkiasharpChartEngineOptions` class provides a centralized configuration schema for controlling the behavior, performance, and default presentation styles of the SkiaSharp-based chart rendering engine.

## API

*   `public string CacheEnabled`
    Specifies whether the chart caching mechanism is enabled. While defined as a string, it typically accepts "true" or "false" to toggle the caching feature.
*   `public int CacheDurationSeconds`
    Defines the lifespan of cached chart items in seconds before they are marked for invalidation or expiration.
*   `public int MaxConcurrentRenders`
    Limits the number of simultaneous rendering operations allowed. This helps prevent resource exhaustion under high load.
*   `public int DefaultChartWidth`
    Sets the default horizontal dimension in pixels for charts when no specific width is provided.
*   `public int DefaultChartHeight`
    Sets the default vertical dimension in pixels for charts when no specific height is provided.
*   `public string DefaultBackgroundColor`
    Determines the default background color for the chart canvas, expressed as a string (e.g., a hex color code like "#FFFFFF").
*   `public bool UseAntiAliasing`
    Indicates whether anti-aliasing should be applied during rendering to improve visual quality by smoothing edges.
*   `public int MaxDataPointsPerSeries`
    Sets the upper limit on the number of data points permitted per individual data series to ensure performance consistency.
*   `public int MaxSeriesPerChart`
    Defines the maximum number of data series allowed within a single chart instance.
*   `public bool ValidateDataOnLoad`
    Determines whether the input data should be validated for integrity and schema conformance immediately upon loading, before rendering begins.

## Usage

```csharp
// Example 1: Direct instantiation and configuration
var options = new SkiasharpChartEngineOptions
{
    CacheEnabled = "true",
    CacheDurationSeconds = 300,
    MaxConcurrentRenders = 4,
    DefaultChartWidth = 800,
    DefaultChartHeight = 600,
    DefaultBackgroundColor = "#FFFFFF",
    UseAntiAliasing = true,
    MaxDataPointsPerSeries = 1000,
    MaxSeriesPerChart = 10,
    ValidateDataOnLoad = true
};
```

```csharp
// Example 2: Configuration via Dependency Injection (e.g., using IOptions pattern)
services.Configure<SkiasharpChartEngineOptions>(options =>
{
    options.CacheEnabled = Configuration["ChartSettings:CacheEnabled"];
    options.MaxConcurrentRenders = 2;
    options.DefaultBackgroundColor = "#F0F0F0";
    options.ValidateDataOnLoad = true;
});
```

## Notes

*   **Thread-Safety**: This class is intended to be used as a configuration object. While it is safe for multiple threads to read the properties concurrently, modifying these values during active rendering operations is not thread-safe and may lead to inconsistent rendering states.
*   **Configuration Values**: Ensure that `DefaultChartWidth` and `DefaultChartHeight` are set to positive values. Negative values or zero may cause rendering exceptions or result in a blank canvas.
*   **Data Limits**: Exceeding `MaxDataPointsPerSeries` or `MaxSeriesPerChart` may cause the rendering process to truncate data or throw validation exceptions, depending on the implementation of the data services.
*   **Color Formats**: `DefaultBackgroundColor` should be a valid SkiaSharp-compatible color string format. Invalid strings may cause rendering initialization to fail.
