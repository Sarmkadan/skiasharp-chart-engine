# DataControllerValidation

Static utility class providing validation functionality for data controllers within the skiasharp-chart-engine. Offers methods to validate data controller configurations, check their validity status, and ensure they meet required constraints before use in chart rendering operations.

## API

### Validate Methods

Multiple overloaded `Validate` methods are available for different data controller types:

**Purpose:** Performs validation checks on the specified data controller and returns a list of validation error messages.

**Parameters:** Varies by overload - accepts different data controller types as input.

**Return Value:** `IReadOnlyList<string>` containing validation error messages. Returns an empty list if the data controller is valid.

**Throws:** Does not throw exceptions; all validation errors are returned as strings in the result list.

### IsValid Properties

Multiple `IsValid` properties exist, one for each supported data controller type:

**Purpose:** Gets a boolean indicating whether the associated data controller is currently in a valid state.

**Parameters:** None (parameterless property).

**Return Value:** `bool` - `true` if the data controller passes all validation checks, `false` otherwise.

**Throws:** Does not throw exceptions.

### EnsureValid Methods

Multiple overloaded `EnsureValid` methods correspond to each data controller type:

**Purpose:** Validates the specified data controller and throws an exception if validation fails.

**Parameters:** Varies by overload - accepts different data controller types as input.

**Return Value:** `void`

**Throws:** `InvalidOperationException` when the data controller fails validation, with the validation error messages included in the exception details.

## Usage

```csharp
// Example 1: Checking validity before chart operations
var lineController = new LineDataController(chartData);
if (DataControllerValidation.IsValid(lineController))
{
    // Proceed with chart rendering
    renderingService.Render(lineController);
}
else
{
    // Handle invalid state
    var errors = DataControllerValidation.Validate(lineController);
    Console.WriteLine($"Validation failed: {string.Join(", ", errors)}");
}
```

```csharp
// Example 2: Using EnsureValid for fail-fast validation
try
{
    var barController = new BarDataController(chartData);
    DataControllerValidation.EnsureValid(barController);
    // Controller is guaranteed valid - proceed with operations
    chartEngine.AddController(barController);
}
catch (InvalidOperationException ex)
{
    // Validation failed - handle error
    Logger.LogError($"Bar controller validation failed: {ex.Message}");
}
```

## Notes

- All members are static and thread-safe for concurrent access
- The `Validate` methods never throw exceptions; they collect all validation errors and return them
- `EnsureValid` methods provide a fail-fast approach by throwing `InvalidOperationException` immediately upon validation failure
- Empty validation result lists indicate successful validation with no errors
- Validation typically checks for null references, data integrity, and configuration completeness required for chart rendering
- The multiple overloads suggest support for various chart-specific data controller implementations (line, bar, pie, etc.)
