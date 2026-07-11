# ChartValidator

The `ChartValidator` class provides a comprehensive suite of validation routines for the `skiasharp-chart-engine` ecosystem, ensuring that `Chart`, `Series`, `DataPoint`, and `Configuration` objects conform to structural and semantic requirements prior to rendering. It serves as both a static entry point for rapid object validation and an instance-based mechanism for accumulating custom validation errors and warnings during complex object construction or configuration.

## API

### Static Methods

*   `public static ValidationResult ValidateChart(Chart chart)`
    Validates the structure and integrity of a `Chart` instance. Returns a `ValidationResult` containing any discovered errors or warnings. Throws `ArgumentNullException` if `chart` is null.

*   `public static ValidationResult ValidateSeries(Series series)`
    Validates the configuration and data integrity of a `Series` instance. Returns a `ValidationResult`. Throws `ArgumentNullException` if `series` is null.

*   `public static ValidationResult ValidateDataPoint(DataPoint point)`
    Performs validation checks on a single `DataPoint`. Returns a `ValidationResult`. Throws `ArgumentNullException` if `point` is null.

*   `public static ValidationResult ValidateConfiguration(Configuration config)`
    Validates the settings and constraints within a `Configuration` object. Returns a `ValidationResult`. Throws `ArgumentNullException` if `config` is null.

### Instance Methods

*   `public void AddError(string message)`
    Registers a critical validation error with the current validator instance.

*   `public void AddWarning(string message)`
    Registers a validation warning with the current validator instance.

*   `public void Merge(ValidationResult other)`
    Integrates the errors and warnings from another `ValidationResult` instance into the current validator.

*   `public override string ToString()`
    Returns a formatted string representation of the current validator state, aggregating all added errors and warnings.

## Usage

### Validating a Chart Object
```csharp
using SkiaSharp.ChartEngine;

// Assume chart instance is initialized
var chart = GetChartConfiguration();
ValidationResult result = ChartValidator.ValidateChart(chart);

if (!result.IsValid)
{
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"Validation Error: {error}");
    }
}
```

### Accumulating Custom Validation
```csharp
using SkiaSharp.ChartEngine;

var validator = new ChartValidator();

// Perform custom business logic validation
if (customData.Value < 0)
{
    validator.AddError("Data value cannot be negative.");
}

if (customData.IsDepreciated)
{
    validator.AddWarning("Data point is using a deprecated schema.");
}

// Merge with framework-level validation
var frameworkResult = ChartValidator.ValidateDataPoint(customData);
validator.Merge(frameworkResult);

Console.WriteLine(validator.ToString());
```

## Notes

*   **Thread Safety**: The static validation methods are generally thread-safe provided the input objects are not modified by other threads during validation. The `ChartValidator` instance methods are not thread-safe; if a `ChartValidator` instance is shared across threads, access must be synchronized externally.
*   **ValidationResult**: The `ValidationResult` returned by static methods represents a point-in-time snapshot of the object's validity and does not maintain a reference to the validated object after the method returns.
*   **Performance**: While validation is generally efficient, performing extensive `ValidateChart` calls in tight loops (e.g., inside an animation frame) should be avoided to prevent unnecessary overhead.
