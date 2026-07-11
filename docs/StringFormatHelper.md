# StringFormatHelper

The `StringFormatHelper` class provides a collection of static utility methods for string manipulation, data formatting, and sanitization within the `skiasharp-chart-engine` project. These methods are designed to ensure consistent, readable data representation and robust string handling across UI components and data labels, reducing boilerplate code for common text-processing tasks.

## API

### FormatNumberWithUnits(double value, string suffix)
Formats a numeric value by appending a unit suffix, potentially scaling large numbers (e.g., converting 1500 to "1.5K").
- **Parameters**: `double value` (the number to format), `string suffix` (the unit label to append).
- **Returns**: A formatted string representing the scaled value and unit.

### FormatCurrency(decimal value, string cultureCode)
Formats a decimal value as a currency string based on the provided culture code.
- **Parameters**: `decimal value` (the currency amount), `string cultureCode` (e.g., "en-US").
- **Returns**: A culture-aware formatted currency string.

### FormatPercentage(double value, int decimalPlaces)
Formats a numeric value as a percentage.
- **Parameters**: `double value` (the decimal value to format), `int decimalPlaces` (number of decimal places to include).
- **Returns**: A formatted percentage string (e.g., 0.15 becomes "15%").

### TruncateWithEllipsis(string input, int maxLength)
Truncates a string to a maximum length and appends an ellipsis (...) if the string exceeds the limit.
- **Parameters**: `string input` (the string to truncate), `int maxLength` (the maximum allowed length including the ellipsis).
- **Returns**: The truncated string.

### CamelCaseToTitleCase(string input)
Converts a camelCase string to a title-cased string with spaces (e.g., "chartDataLabel" to "Chart Data Label").
- **Parameters**: `string input` (the camelCase string).
- **Returns**: The title-cased string.

### SnakeCaseToTitleCase(string input)
Converts a snake_case string to a title-cased string with spaces (e.g., "chart_data_label" to "Chart Data Label").
- **Parameters**: `string input` (the snake_case string).
- **Returns**: The title-cased string.

### Sanitize(string input)
Removes potentially unsafe characters or performs basic cleaning on a string, useful for rendering user-provided input in chart elements.
- **Parameters**: `string input` (the raw string to sanitize).
- **Returns**: A cleaned string.

### PadForAlignment(string input, int totalWidth, char paddingChar)
Pads a string to a specified width for alignment purposes in text layouts.
- **Parameters**: `string input` (the string to pad), `int totalWidth` (the desired total width), `char paddingChar` (the character used for padding).
- **Returns**: A right-padded string.

### FormatTimespan(TimeSpan span, string format)
Formats a `TimeSpan` object into a human-readable string representation.
- **Parameters**: `TimeSpan span` (the timespan to format), `string format` (the format specifier).
- **Returns**: A formatted timespan string.

### Repeat(string input, int count)
Repeats a string a specified number of times.
- **Parameters**: `string input` (the string to repeat), `int count` (the number of repetitions).
- **Returns**: A new string composed of the repeated input.

### ToCsvLine(params string[] values)
Escapes and joins a series of strings into a single CSV-compatible line.
- **Parameters**: `params string[] values` (the values to join).
- **Returns**: A single comma-separated string line.

## Usage

```csharp
// Example 1: Formatting chart axis labels
double dataValue = 12500.5;
string axisLabel = StringFormatHelper.FormatNumberWithUnits(dataValue, " units");
// Result: "12.5K units"

string title = StringFormatHelper.CamelCaseToTitleCase("totalRevenueReport");
// Result: "Total Revenue Report"
```

```csharp
// Example 2: Creating a CSV export row
string[] rowData = { "Category A", "100", "15.5%" };
string csvLine = StringFormatHelper.ToCsvLine(rowData);
// Result: "Category A,100,15.5%"

string displayName = StringFormatHelper.TruncateWithEllipsis("Long Series Name Data Point", 15);
// Result: "Long Series Na..."
```

## Notes

- **Thread-Safety**: All methods within `StringFormatHelper` are implemented as static, stateless operations. They are fully thread-safe and can be called concurrently from multiple threads without locking.
- **Input Handling**: Most methods handle `null` or empty string inputs gracefully by returning the input or a default empty string, depending on the specific method logic. It is recommended to validate critical inputs before passing them to avoid unexpected results.
- **Performance**: While these methods are designed for efficiency, they involve string allocation. In high-frequency rendering scenarios, use these methods judiciously to minimize garbage collection impact.
