# ChartRenderingIntegrationTestsValidation

The `ChartRenderingIntegrationTestsValidation` class provides a set of static validation utilities for chart configurations, chart series, and complete chart objects. It is designed for use in integration tests to verify that chart definitions are well-formed before they are passed to the rendering engine. Each validation method returns a list of error messages or a boolean indicating validity, and the `Ensure` variants throw an exception when validation fails.

## API

### `ValidateChartConfiguration`
```csharp
public static IReadOnlyList<string> ValidateChartConfiguration(/* chart configuration object */)
```
Validates a chart configuration and returns a read-only list of validation error messages. If the configuration is valid, the list is empty.  
**Returns:** A list of strings describing each validation error.  
**Throws:** Does not throw; errors are returned in the list.

### `ValidateChartSeries`
```csharp
public static IReadOnlyList<string> ValidateChartSeries(/* chart series object */)
```
Validates a chart series and returns a read-only list of validation error messages. An empty list indicates a valid series.  
**Returns:** A list of strings describing each validation error.  
**Throws:** Does not throw; errors are returned in the list.

### `ValidateChart`
```csharp
public static IReadOnlyList<string> ValidateChart(/* chart object */)
```
Validates a complete chart (including its configuration and series) and returns a read-only list of validation error messages. An empty list indicates a valid chart.  
**Returns:** A list of strings describing each validation error.  
**Throws:** Does not throw; errors are returned in the list.

### `IsValidChartConfiguration`
```csharp
public static bool IsValidChartConfiguration(/* chart configuration object */)
```
Returns `true` if the chart configuration passes all validation rules; otherwise `false`.  
**Returns:** A boolean indicating validity.  
**Throws:** Does not throw.

### `IsValidChartSeries`
```csharp
public static bool IsValidChartSeries(/* chart series object */)
```
Returns `true` if the chart series passes all validation rules; otherwise `false`.  
**Returns:** A boolean indicating validity.  
**Throws:** Does not throw.

### `IsValidChart`
```csharp
public static bool IsValidChart(/* chart object */)
```
Returns `true` if the complete chart passes all validation rules; otherwise `false`.  
**Returns:** A boolean indicating validity.  
**Throws:** Does not throw.

### `EnsureValidChartConfiguration`
```csharp
public static void EnsureValidChartConfiguration(/* chart configuration object */)
```
Validates the chart configuration and throws a `ChartEngineException` (or the appropriate exception type) if any validation errors are found. If the configuration is valid, the method returns without throwing.  
**Throws:** `ChartEngineException` when validation fails.

### `EnsureValidChartSeries`
```csharp
public static void EnsureValidChartSeries(/* chart series object */)
```
Validates the chart series and throws a `ChartEngineException` if any validation errors are found.  
**Throws:** `ChartEngineException` when validation fails.

### `EnsureValidChart`
```csharp
public static void EnsureValidChart(/* chart object */)
```
Validates the complete chart and throws a `ChartEngineException` if any validation errors are found.  
**Throws:** `ChartEngineException` when validation fails.

## Usage

### Example 1: Validating a chart configuration before rendering

```csharp
using SkiasharpChartEngine;
using SkiasharpChartEngine.IntegrationTests;

var config = new ChartConfiguration
{
    Title = "Sample Chart",
    Width = 800,
    Height = 600
};

// Check validity without throwing
if (!ChartRenderingIntegrationTestsValidation.IsValidChartConfiguration(config))
{
    var errors = ChartRenderingIntegrationTestsValidation.ValidateChartConfiguration(config);
    foreach (var error in errors)
    {
        Console.WriteLine($"Validation error: {error}");
    }
    return;
}

// Proceed with rendering
ChartRenderingService.Render(config);
```

### Example 2: Using Ensure methods in a test

```csharp
using SkiasharpChartEngine;
using SkiasharpChartEngine.IntegrationTests;
using Xunit;

public class ChartValidationTests
{
    [Fact]
    public void ValidChart_DoesNotThrow()
    {
        var chart = new Chart
        {
            Configuration = new ChartConfiguration { Title = "Test", Width = 400, Height = 300 },
            Series = new List<ChartSeries>
            {
                new ChartSeries { Name = "Series1", Data = new[] { 1.0, 2.0, 3.0 } }
            }
        };

        // This will throw if the chart is invalid
        ChartRenderingIntegrationTestsValidation.EnsureValidChart(chart);
    }

    [Fact]
    public void InvalidConfiguration_Throws()
    {
        var config = new ChartConfiguration(); // Missing required fields

        Assert.Throws<ChartEngineException>(() =>
            ChartRenderingIntegrationTestsValidation.EnsureValidChartConfiguration(config));
    }
}
```

## Notes

- All methods are static and operate on the provided input objects without modifying them. They are thread-safe as long as the input objects are not mutated concurrently.
- The validation rules are implementation-defined and may include checks for null references, empty or out-of-range values, and structural consistency between configuration and series.
- The `Validate*` methods always return a list; an empty list indicates a valid input. The `IsValid*` methods are convenience wrappers that return `true` when the corresponding `Validate*` method returns an empty list.
- The `EnsureValid*` methods throw a `ChartEngineException` (or a derived exception type) with a message that aggregates all validation errors. If the input is `null`, the behavior is undefined (likely a `NullReferenceException` or a validation error depending on the implementation).
- These utilities are intended for test environments and may have performance overhead due to comprehensive validation; they are not optimized for production rendering paths.
