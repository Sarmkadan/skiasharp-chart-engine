# InputValidator

Provides a composable way to validate chart-related objects such as charts, series data, configurations, and data points. Validation results are collected in the `Errors` and `Warnings` lists, and custom validation logic can be supplied via the `Rule` delegate or the `AddRule` method.

## API

### InputValidator()
Initializes a new instance of the `InputValidator` class. The internal `Errors` and `Warnings` lists are instantiated empty, and the `Rule` property is set to `null`.

### ValidationResult ValidateChart(Chart chart)
Validates the supplied `Chart` instance.

- **Parameters**  
  - `chart`: The chart to validate. Must not be `null`.

- **Return value**  
  A `ValidationResult` indicating whether the chart passes validation.

- **Exceptions**  
  - `ArgumentNullException` if `chart` is `null`.

### ValidationResult ValidateSeriesData(SeriesData seriesData)
Validates the supplied `SeriesData` instance.

- **Parameters**  
  - `seriesData`: The series data to validate. Must not be `null`.

- **Return value**  
  A `ValidationResult` indicating whether the series data passes validation.

- **Exceptions**  
  - `ArgumentNullException` if `seriesData` is `null`.

### ValidationResult ValidateConfiguration(ChartConfiguration configuration)
Validates the supplied `ChartConfiguration` instance.

- **Parameters**  
  - `configuration`: The configuration to validate. Must not be `null`.

- **Return value**  
  A `ValidationResult` indicating whether the configuration passes validation.

- **Exceptions**  
  - `ArgumentNullException` if `configuration` is `null`.

### ValidationResult ValidateDataPoints(IEnumerable<DataPoint> dataPoints)
Validates the supplied collection of `DataPoint` instances.

- **Parameters**  
  - `dataPoints`: The data points to validate. Must not be `null`; individual elements may be `null` and will be treated as invalid.

- **Return value**  
  A `ValidationResult` indicating whether the data points pass validation.

- **Exceptions**  
  - `ArgumentNullException` if `dataPoints` is `null`.

### void AddRule()
Registers a validation rule delegate for use during validation operations. The rule to be used is taken from the current value of the `Rule` property; if `Rule` is `null`, no rule is executed.

- **Parameters**  
  None.

- **Return value**  
  None.

- **Exceptions**  
  None.

### List<string> Errors
Gets the list of error messages produced by the most recent validation operation. The list is not cleared automatically between calls; callers should clear it manually if needed.

### List<string> Warnings
Gets the list of warning messages produced by the most recent validation operation. The list is not cleared automatically between calls; callers should clear it manually if needed.

### void AddError(string message)
Appends an error message to the `Errors` list.

- **Parameters**  
  - `message`: The error message to add. Must not be `null`.

- **Exceptions**  
  - `ArgumentNullException` if `message` is `null`.

### void AddWarning(string message)
Appends a warning message to the `Warnings` list.

- **Parameters**  
  - `message`: The warning message to add. Must not be `null`.

- **Exceptions**  
  - `ArgumentNullException` if `message` is `null`.

### public override string ToString()
Returns a string representation of the validator, currently consisting of the `Name` and `Description` properties if they are set.

- **Return value**  
  A string describing the validator.

### string Name
Gets or sets a short identifier for the validator instance.

### string Description
Gets or sets a longer explanatory text for the validator instance.

### Func<Chart, bool>? Rule
Gets or sets a delegate that defines custom validation logic to be applied to a `Chart` object. The delegate receives a `Chart` and returns `true` if the chart satisfies the rule, otherwise `false`. If `null`, no custom rule is applied.

## Usage

```csharp
using SkiSharpChartEngine.Validation;

// Create a validator and supply a custom rule via the Rule property.
var validator = new InputValidator
{
    Name = "SampleValidator",
    Description = "Validates that a chart has at least one series.",
    Rule = chart => chart?.Series?.Count > 0
};

Chart myChart = GetChartFromSomewhere();
ValidationResult result = validator.ValidateChart(myChart);

if (!result.IsValid)
{
    foreach (string err in validator.Errors)
    {
        Console.Error.WriteLine($"Validation error: {err}");
    }
}
else
{
    Console.WriteLine("Chart validation succeeded.");
}
```

```csharp
using SkiSharpChartEngine.Validation;
using SkiSharpChartEngine.Data;

var validator = new InputValidator();
// Add a rule using the parameter‑less AddRule method (uses the Rule property internally).
validator.Rule = dataPoints => dataPoints.All(dp => dp.X >= 0);
validator.AddRule();

var points = new List<DataPoint>
{
    new DataPoint { X = 1, Y = 2 },
    new DataPoint { X = -3, Y = 4 } // will trigger a warning
};

ValidationResult dpResult = validator.ValidateDataPoints(points);
// Errors list remains empty because the rule only warns.
foreach (string warn in validator.Warnings)
{
    Console.WriteLine($"Warning: {warn}");
}
```

## Notes

- Validation methods do **not** clear the `Errors` or `Warnings` lists automatically. Repeated calls will accumulate messages unless the caller clears the lists explicitly.
- Passing `null` to any validation method or to `AddError`/`AddWarning` throws an `ArgumentNullException`.
- The `Rule` delegate is invoked only by the `AddRule` method; validation methods themselves do not consult the delegate unless the implementation chooses to do so. Consumers should ensure that any delegate assigned to `Rule` is thread‑safe if the validator instance may be accessed from multiple threads simultaneously.
- The `InputValidator` class itself contains no static state; however, the `Errors` and `Warnings` lists are mutable reference types. Concurrent modification of these lists from multiple threads without external synchronization can lead to undefined behavior. It is recommended to either restrict access to a single thread or synchronize access to the lists when sharing an instance.
