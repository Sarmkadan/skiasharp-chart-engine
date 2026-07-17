# TemplateControllerValidation

`TemplateControllerValidation` is a static utility class responsible for validating chart templates and their associated configurations within the SkiaSharp Chart Engine. It provides methods to check the validity of template names, IDs, chart configurations, and other related components, ensuring that all required parameters meet the necessary constraints before rendering operations.

## API

### Validate

**Purpose**: Validates all aspects of the current chart template configuration and returns a list of error messages.

**Return Value**: `IReadOnlyList<string>` containing validation errors, or an empty list if valid.

**Exceptions**: None thrown; errors are returned as strings.

---

### IsValid

**Purpose**: Determines whether the current chart template configuration is valid.

**Return Value**: `bool` indicating validity (`true` if no errors, `false` otherwise).

**Exceptions**: None thrown.

---

### EnsureValid

**Purpose**: Throws an exception if the current chart template configuration is invalid.

**Exceptions**: Throws `InvalidOperationException` with a message containing all validation errors if invalid.

---

### ValidateTemplateName

**Purpose**: Validates the template name for correctness and adherence to naming rules.

**Return Value**: `IReadOnlyList<string>` of errors related to the template name.

**Exceptions**: None thrown.

---

### ValidateTemplateId

**Purpose**: Validates the template identifier for format and uniqueness.

**Return Value**: `IReadOnlyList<string>` of errors related to the template ID.

**Exceptions**: None thrown.

---

### ValidateChartConfiguration

**Purpose**: Validates the overall chart configuration structure and required settings.

**Return Value**: `IReadOnlyList<string>` of errors related to chart configuration.

**Exceptions**: None thrown.

---

### ValidateChartTemplate

**Purpose**: Validates the chart template definition for completeness and correctness.

**Return Value**: `IReadOnlyList<string>` of errors related to the chart template.

**Exceptions**: None thrown.

---

### ValidateChartType

**Purpose**: Validates the chart type specification against supported types.

**Return Value**: `IReadOnlyList<string>` of errors related to the chart type.

**Exceptions**: None thrown.

---

### ValidateCancellationToken

**Purpose**: Validates the cancellation token for proper usage in asynchronous operations.

**Return Value**: `IReadOnlyList<string>` of errors related to the cancellation token.

**Exceptions**: None thrown.

---

## Usage

### Example 1: Validate and Check Errors

```csharp
var errors = TemplateControllerValidation.Validate();
if (errors.Any())
{
    Console.WriteLine("Validation failed:");
    foreach (var error in errors)
    {
        Console.WriteLine($"- {error}");
    }
}
else
{
    Console.WriteLine("Template is valid.");
}
```

### Example 2: Ensure Valid Before Rendering

```csharp
try
{
    TemplateControllerValidation.EnsureValid();
    // Proceed with rendering
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Template validation error: {ex.Message}");
}
```

---

## Notes

- All methods are static and stateless, but may rely on shared configuration state. Thread-safety depends on the underlying implementation of accessed properties.
- `EnsureValid` throws an exception with aggregated error messages, making it suitable for fail-fast scenarios.
- Individual validation methods (e.g., `ValidateTemplateName`) allow granular checks without triggering full validation.
- `IsValid` provides a lightweight check for quick validity assessment without retrieving detailed error messages.
