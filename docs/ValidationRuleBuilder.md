# ValidationRuleBuilder

`ValidationRuleBuilder` is a fluent API designed for constructing and executing validation logic for objects of generic type `T`. It enables developers to chain various validation predicates, conditional checks, and constraints to ensure data integrity within the `skiasharp-chart-engine` framework.

## API

### Constructors

*   **`public ValidationRuleBuilder()`**
    Initializes a new instance of the `ValidationRuleBuilder` class.

### Methods

*   **`public ValidationRuleBuilder<T> AddRule(Func<T, bool> predicate, string errorMessage, string ruleName)`**
    Adds a custom validation rule to the builder.
    *   `predicate`: The logic to evaluate.
    *   `errorMessage`: The message returned if validation fails.
    *   `ruleName`: A unique identifier for the rule.
    *   Returns: The current `ValidationRuleBuilder<T>` instance.

*   **`public ValidationRuleBuilder<T> IsRequired()`**
    Adds a constraint ensuring the object is not null or empty.
    *   Returns: The current `ValidationRuleBuilder<T>` instance.

*   **`public ValidationRuleBuilder<T> IsInRange(T min, T max)`**
    Adds a constraint verifying the value is within a specified range.
    *   `min`: The minimum inclusive value.
    *   `max`: The maximum inclusive value.
    *   Returns: The current `ValidationRuleBuilder<T>` instance.

*   **`public ValidationRuleBuilder<T> HasLength(int min, int max)`**
    Adds a constraint verifying the object length is within the specified range (applicable for strings or collections).
    *   `min`: Minimum inclusive length.
    *   `max`: Maximum inclusive length.
    *   Returns: The current `ValidationRuleBuilder<T>` instance.

*   **`public ValidationRuleBuilder<T> When(Func<T, bool> condition)`**
    Applies subsequent rules only if the specified condition is met.
    *   `condition`: The predicate that must return true to apply the next rule.
    *   Returns: The current `ValidationRuleBuilder<T>` instance.

*   **`public ValidationResult Validate(T instance)`**
    Executes all configured validation rules against the provided instance.
    *   `instance`: The object to validate.
    *   Returns: A `ValidationResult` object containing the outcome and any error messages.

*   **`public int GetRuleCount()`**
    Returns the number of rules configured in the builder.

*   **`public void ClearRules()`**
    Removes all configured validation rules from the builder.

*   **`public override string ToString()`**
    Returns a string representation of the builder state.

### Properties

*   **`public Func<T, bool> Predicate`**
    The current active predicate function.
*   **`public string ErrorMessage`**
    The error message associated with the current rule.
*   **`public string RuleName`**
    The name of the current rule.
*   **`public bool IsValid`**
    Indicates whether the last validation execution was successful.
*   **`public List<string> Errors`**
    A list of error messages generated during validation.

## Usage

```csharp
// Example 1: Basic required field and range validation
var builder = new ValidationRuleBuilder<int>();
builder.IsRequired().IsInRange(0, 100);
var result = builder.Validate(50);

if (!result.IsValid)
{
    Console.WriteLine($"Validation failed: {string.Join(", ", result.Errors)}");
}
```

```csharp
// Example 2: Complex conditional validation
var stringBuilder = new ValidationRuleBuilder<string>();
stringBuilder
    .When(s => s != null)
    .HasLength(5, 10)
    .AddRule(s => s.Contains("@"), "Must contain @", "EmailFormat");

var result = stringBuilder.Validate("test@example");
```

## Notes

*   **Thread Safety**: `ValidationRuleBuilder` instances are not thread-safe. Avoid sharing a single builder instance across multiple threads for concurrent validation operations.
*   **Validation Execution**: The `Validate` method processes rules in the order they were added.
*   **Performance**: If a large number of rules are configured, consider clearing the builder using `ClearRules()` after execution to free up resources if the builder instance is reused.
