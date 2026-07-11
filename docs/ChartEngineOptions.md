# ChartEngineOptions

The `ChartEngineOptions` class provides a centralized configuration mechanism to customize the behavior, performance characteristics, and rendering parameters of the `skiasharp-chart-engine`. It allows developers to fine-tune resource utilization, set default aesthetic properties, and enforce data integrity checks, ensuring the engine operates optimally within specific application requirements.

## API

### Properties

*   **`CacheSize`** (`int`)  
    Gets or sets the maximum number of items to be held in the rendering cache when caching is enabled.
*   **`EnableLogging`** (`bool`)  
    Gets or sets a value indicating whether detailed logging is enabled for engine operations.
*   **`EnableCaching`** (`bool`)  
    Gets or sets a value indicating whether the engine should cache rendered chart elements to improve performance.
*   **`MaxConcurrentRenders`** (`int`)  
    Gets or sets the maximum number of simultaneous rendering operations the engine is permitted to execute.
*   **`DefaultChartWidth`** (`int`)  
    Gets or sets the default width in pixels for charts rendered without an explicit width.
*   **`DefaultChartHeight`** (`int`)  
    Gets or sets the default height in pixels for charts rendered without an explicit height.
*   **`DefaultBackgroundColor`** (`string`)  
    Gets or sets the default background color string (e.g., a hex code) used for chart canvases.
*   **`UseAntiAliasing`** (`bool`)  
    Gets or sets a value indicating whether anti-aliasing should be applied during rendering to improve visual quality.
*   **`MaxDataPointsPerSeries`** (`int`)  
    Gets or sets the maximum number of data points permitted for a single series, used to prevent excessive memory consumption.
*   **`MaxSeriesPerChart`** (`int`)  
    Gets or sets the maximum number of series allowed within a single chart instance.
*   **`CacheExpirationTime`** (`TimeSpan`)  
    Gets or sets the duration for which cached rendered elements remain valid before they are subject to eviction.
*   **`ValidateDataOnLoad`** (`bool`)  
    Gets or sets a value indicating whether the engine should perform strict validation on incoming chart data before processing.
*   **`CustomSettings`** (`Dictionary<string, object>?`)  
    Gets or sets an optional dictionary for providing implementation-specific or experimental configuration parameters.

### Methods

*   **`Validate()`** (`void`)  
    Validates the current configuration settings. Throws an `InvalidOperationException` or relevant validation exception if the current state of the properties violates internal constraints (e.g., negative dimensions, excessive limits).

## Usage

### Basic Configuration
```csharp
var options = new ChartEngineOptions
{
    EnableCaching = true,
    CacheSize = 100,
    CacheExpirationTime = TimeSpan.FromMinutes(30),
    DefaultChartWidth = 800,
    DefaultChartHeight = 600
};

options.Validate();
```

### Advanced Configuration with Custom Settings
```csharp
var options = new ChartEngineOptions
{
    MaxConcurrentRenders = 4,
    UseAntiAliasing = true,
    MaxDataPointsPerSeries = 1000,
    DefaultBackgroundColor = "#FFFFFF",
    CustomSettings = new Dictionary<string, object>
    {
        { "RenderMode", "HighPerformance" },
        { "TimeoutMs", 5000 }
    }
};

options.Validate();
```

## Notes

*   **Thread Safety**: `ChartEngineOptions` is designed primarily as a configuration data object. It is not inherently thread-safe for concurrent read/write operations. It is recommended to initialize and fully configure the object before passing it to rendering services, and treating it as read-only once the engine is operational.
*   **Validation**: Always call the `Validate()` method after configuring the object and before initializing the chart engine. This ensures that erroneous configurations (e.g., a negative `CacheSize` or invalid `MaxConcurrentRenders`) are caught early.
*   **Memory Management**: When setting `MaxDataPointsPerSeries` and `MaxSeriesPerChart`, consider the memory constraints of the host environment. Extremely high values can significantly increase the memory footprint of individual rendering operations.
