# StringFormatBenchmarks

The `StringFormatBenchmarks` class provides a set of preconfigured string formatting operations used to measure performance of common text transformations in the skiasharp-chart-engine project. Each public member returns the result of a specific formatting routine applied to a fixed internal input, allowing consistent benchmarking of operations such as case conversion, CSV serialization, string repetition, number formatting with units, and percentage formatting.

## API

### `CamelCaseToTitleCase`
- **Type:** `string` (read-only property)
- **Description:** Returns the result of converting a predefined camelCase string to Title Case (e.g., "camelCaseExample" becomes "Camel Case Example").
- **Return value:** A `string` containing the title-cased representation.
- **Exceptions:** None. The internal input is guaranteed to be non-null and valid.

### `SnakeCaseToTitleCase`
- **Type:** `string` (read-only property)
- **Description:** Returns the result of converting a predefined snake_case string to Title Case (e.g., "snake_case_example" becomes "Snake Case Example").
- **Return value:** A `string` containing the title-cased representation.
- **Exceptions:** None.

### `ToCsvLine`
- **Type:** `string` (read-only property)
- **Description:** Returns the result of serializing a predefined set of values into a single CSV-formatted line (comma-separated, with proper escaping if needed).
- **Return value:** A `string` representing a CSV line.
- **Exceptions:** None.

### `Repeat_Twenty`
- **Type:** `string` (read-only property)
- **Description:** Returns the result of repeating a predefined string exactly twenty times and concatenating the repetitions.
- **Return value:** A `string` consisting of the repeated input.
- **Exceptions:** None.

### `FormatNumberWithUnits_Billions`
- **Type:** `string` (read-only property)
- **Description:** Returns the result of formatting a predefined numeric value (in the billions range) with an appropriate unit suffix (e.g., "1.23B").
- **Return value:** A `string` containing the formatted number and unit.
- **Exceptions:** None.

### `FormatNumberWithUnits_Thousands`
- **Type:** `string` (read-only property)
- **Description:** Returns the result of formatting a predefined numeric value (in the thousands range) with an appropriate unit suffix (e.g., "1.23K").
- **Return value:** A `string` containing the formatted number and unit.
- **Exceptions:** None.

### `FormatPercentage`
- **Type:** `string` (read-only property)
- **Description:** Returns the result of formatting a predefined decimal value as a percentage string (e.g., "12.34%").
- **Return value:** A `string` containing the percentage representation.
- **Exceptions:** None.

## Usage

The following examples demonstrate how to access the benchmark results in a typical benchmarking or diagnostic context.

```csharp
using SkiasharpChartEngine.Benchmarks;

var benchmarks = new StringFormatBenchmarks();

// Retrieve the title-cased version of a camelCase string
string title = benchmarks.CamelCaseToTitleCase;
Console.WriteLine(title); // Output example: "Camel Case Example"

// Retrieve a formatted percentage string
string percent = benchmarks.FormatPercentage;
Console.WriteLine(percent); // Output example: "12.34%"
```

```csharp
// Using multiple properties to compare formatting performance
var benchmarks = new StringFormatBenchmarks();

string csvLine = benchmarks.ToCsvLine;
string repeated = benchmarks.Repeat_Twenty;
string billions = benchmarks.FormatNumberWithUnits_Billions;
string thousands = benchmarks.FormatNumberWithUnits_Thousands;

// The returned strings can be used as inputs for further analysis or logging
Console.WriteLine($"CSV: {csvLine}");
Console.WriteLine($"Repeated: {repeated}");
Console.WriteLine($"Billions: {billions}");
Console.WriteLine($"Thousands: {thousands}");
```

## Notes

- **Edge Cases:** All members operate on fixed, non-null internal inputs. No empty strings, null values, or extreme numeric boundaries are tested by these benchmarks. The formatting logic is assumed to handle typical valid inputs without throwing exceptions.
- **Thread Safety:** The `StringFormatBenchmarks` class is thread-safe for read-only access. Since the properties are read-only and the underlying formatting operations are stateless (no mutation of shared state), multiple threads may read the same property concurrently without synchronization.
- **Performance Considerations:** Each property recomputes the formatted string on every access. In a benchmarking scenario, this is intentional to measure the cost of the formatting operation. For production use, consider caching the result if the same formatted string is needed repeatedly.
