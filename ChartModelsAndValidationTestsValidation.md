# ChartModelsAndValidationTestsValidation

Static validation helper class for SkiaSharp chart models (`DataPoint`, `ChartSeries`, `Chart`, and `ChartConfiguration`). Provides extension methods to validate chart model instances and throw descriptive exceptions for invalid data. Designed for use in unit tests and model construction scenarios where invalid data should fail fast with clear diagnostics.

## API

### Validate(this DataPoint value) → IReadOnlyList<string>

Validates a `DataPoint` instance and returns a list of human-readable validation problems.

- **Parameters:** `value` – The data point to validate
- **Returns:** List of validation problems; empty if valid
- **Exceptions:** Throws `ArgumentNullException` if `value` is null
- **Validation rules:**
  - X and Y coordinates must not be NaN or Infinity
  - Color must be a valid hex color (#RRGGBB or #RRGGBBAA)
  - CustomRadius (if specified) must be positive


### IsValid(this DataPoint value) → bool

Determines whether the specified `DataPoint` instance is valid.

- **Parameters:** `value` – The data point to check
- **Returns:** `true` if valid; `false` otherwise
- **Exceptions:** None – returns `false` for invalid data rather than throwing


### EnsureValid(this DataPoint value) → void

Validates the specified `DataPoint` instance and throws an `ArgumentException` with a detailed message listing all validation problems if the instance is invalid.

- **Parameters:** `value` – The data point to validate
- **Exceptions:**
  - `ArgumentNullException` if `value` is null
  - `ArgumentException` if `value` contains validation problems, with a message listing each issue on a new line prefixed with "-"


### Validate(this ChartSeries value) → IReadOnlyList<string>

Validates a `ChartSeries` instance and returns a list of human-readable validation problems.

- **Parameters:** `value` – The chart series to validate
- **Returns:** List of validation problems; empty if valid
- **Exceptions:** Throws `ArgumentNullException` if `value` is null
- **Validation rules:**
  - Name must not be empty or whitespace
  - LineWidth must be greater than 0
  - Color must be a valid hex color (#RRGGBB or #RRGGBBAA)
  - YAxisMin must not be greater than YAxisMax (if both are specified)


### IsValid(this ChartSeries value) → bool

Determines whether the specified `ChartSeries` instance is valid.

- **Parameters:** `value` – The chart series to check
- **Returns:** `true` if valid; `false` otherwise
- **Exceptions:** None – returns `false` for invalid data rather than throwing


### EnsureValid(this ChartSeries value) → void

Validates the specified `ChartSeries` instance and throws an `ArgumentException` with a detailed message listing all validation problems if the instance is invalid.

- **Parameters:** `value` – The chart series to validate
- **Exceptions:**
  - `ArgumentNullException` if `value` is null
  - `ArgumentException` if `value` contains validation problems, with a message listing each issue on a new line prefixed with "-"


### Validate(this Chart value) → IReadOnlyList<string>

Validates a `Chart` instance and returns a list of human-readable validation problems.

- **Parameters:** `value` – The chart to validate
- **Returns:** List of validation problems; empty if valid
- **Exceptions:** Throws `ArgumentNullException` if `value` is null
- **Validation rules:**
  - Id must not be empty
  - Must contain at least one series
  - CreatedAt must not be default DateTime
  - Must contain at least one data point across all series
  - Delegates validation to the ChartConfiguration instance


### IsValid(this Chart value) → bool

Determines whether the specified `Chart` instance is valid.

- **Parameters:** `value` – The chart to check
- **Returns:** `true` if valid; `false` otherwise
- **Exceptions:** None – returns `false` for invalid data rather than throwing


### EnsureValid(this Chart value) → void

Validates the specified `Chart` instance and throws an `ArgumentException` with a detailed message listing all validation problems if the instance is invalid.

- **Parameters:** `value` – The chart to validate
- **Exceptions:**
  - `ArgumentNullException` if `value` is null
  - `ArgumentException` if `value` contains validation problems, with a message listing each issue on a new line prefixed with "-"


### Validate(this ChartConfiguration value) → IReadOnlyList<string>

Validates a `ChartConfiguration` instance and returns a list of human-readable validation problems.

- **Parameters:** `value` – The chart configuration to validate
- **Returns:** List of validation problems; empty if valid
- **Exceptions:** Throws `ArgumentNullException` if `value` is null
- **Validation rules:**
  - Width must be between `ChartConstants.MinimumChartWidth` and `ChartConstants.MaximumChartWidth`
  - Height must be between `ChartConstants.MinimumChartHeight` and `ChartConstants.MaximumChartHeight`
  - Title must not be empty
  - BackgroundColor, GridColor, AxisColor, and TextColor must be valid hex colors
  - Margins must not be negative
  - AnimationDurationMs must be greater than 0
  - ExportDPI must be greater than 0
  - ExportQuality must be between 0 and 1


### IsValid(this ChartConfiguration value) → bool

Determines whether the specified `ChartConfiguration` instance is valid.

- **Parameters:** `value` – The chart configuration to check
- **Returns:** `true` if valid; `false` otherwise
- **Exceptions:** None – returns `false` for invalid data rather than throwing

- **Note:** This method catches and suppresses any exceptions during validation to return a boolean result


### EnsureValid(this ChartConfiguration value) → void

Validates the specified `ChartConfiguration` instance and throws an `ArgumentException` with a detailed message listing all validation problems if the instance is invalid.

- **Parameters:** `value` – The chart configuration to validate
- **Exceptions:**
  - `ArgumentNullException` if `value` is null
  - `ArgumentException` if `value` contains validation problems, with a message listing each issue on a new line prefixed with "-"


## Usage

### Example 1: Validating a DataPoint in a test
```csharp
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Tests.Models;

var validPoint = new DataPoint { X = 1.5, Y = 2.3, Color = "#FF0000" };
var problems = validPoint.Validate();
Assert.Empty(problems);

var invalidPoint = new DataPoint { X = double.NaN, Y = double.PositiveInfinity, Color = "invalid" };
problems = invalidPoint.Validate();
Assert.Equal(3, problems.Count);
Assert.Contains("DataPoint.X cannot be NaN", problems);
Assert.Contains("DataPoint.Y cannot be Infinity", problems);
Assert.Contains("DataPoint.Color must be a valid hex color (#RRGGBB or #RRGGBBAA)", problems);
```

### Example 2: Using EnsureValid to fail fast in model construction
```csharp
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Tests.Models;

public Chart CreateDefaultChart()
{
    var config = new ChartConfiguration
    {
        Width = 800,
        Height = 600,
        Title = "Sales Data",
        BackgroundColor = "#FFFFFF",
        GridColor = "#CCCCCC",
        AxisColor = "#333333",
        TextColor = "#000000",
        MarginTop = 20,
        MarginBottom = 20,
        MarginLeft = 40,
        MarginRight = 20,
        AnimationDurationMs = 500,
        ExportDPI = 300,
        ExportQuality = 0.95f
    };
    
    config.EnsureValid(); // Throws if configuration is invalid
    
    var series = new ChartSeries
    {
        Name = "Q2 Sales",
        LineWidth = 2,
        Color = "#0066CC",
        DataPoints = new List<DataPoint>
        {
            new DataPoint { X = 1, Y = 1200, Color = "#0066CC" },
            new DataPoint { X = 2, Y = 1500, Color = "#0066CC" },
            new DataPoint { X = 3, Y = 1800, Color = "#0066CC" }
        }
    };
    
    var chart = new Chart
    {
        Id = Guid.NewGuid().ToString(),
        Series = { series },
        CreatedAt = DateTime.UtcNow,
        Configuration = config
    };
    
    chart.EnsureValid(); // Validates entire chart including nested configuration
    return chart;
}
```

## Notes

- **Null handling:** All methods throw `ArgumentNullException` when passed a null instance, enforced via `ArgumentNullException.ThrowIfNull`.

- **Thread safety:** These extension methods are stateless and only read their input parameters, making them thread-safe for concurrent access on different instances.

- **Performance:** Validation methods allocate lists only when problems are found; successful validation returns an empty list without allocation.

- **Error messages:** Validation failures produce descriptive, actionable messages that include the field name and specific constraint violation, formatted for readability in test output and logs.

- **Aggregation:** Chart validation aggregates problems from all nested components (series, data points, configuration) into a single list for comprehensive diagnostics.

- **IsValid vs Validate:** Use `IsValid()` when you only need a boolean result without detailed diagnostics; use `Validate()` when you need to inspect specific validation problems programmatically.

- **EnsureValid vs Validate:** Use `EnsureValid()` in production code to fail fast with exceptions; use `Validate()` in tests when you need to assert specific validation outcomes.