# ChartDataServiceTestsExtensions

The `ChartDataServiceTestsExtensions` class provides a set of extension methods designed to streamline the creation of test data and the verification of chart objects within the `skiasharp-chart-engine` test suite. By extending the `ChartDataServiceTests` context, these utilities facilitate the rapid instantiation of single-series and multi-series charts with predefined configurations, while also offering fluent assertion capabilities to validate chart equivalence during unit testing.

## API

### CreateTestChart
```csharp
public static Chart CreateTestChart(this ChartDataServiceTests _, string chartName, IEnumerable<...> data)
```
Constructs a new `Chart` instance populated with a single default series using the provided data points. This method is intended for scenarios requiring a basic chart structure for validation.
*   **Parameters**:
    *   `_`: The `ChartDataServiceTests` instance acting as the extension context.
    *   `chartName`: The title or identifier assigned to the generated chart.
    *   `data`: An enumerable collection of data points used to populate the primary series.
*   **Returns**: A fully initialized `Chart` object.
*   **Throws**: Throws an `ArgumentNullException` if `chartName` or `data` is null.

### CreateTestChartWithSeries
```csharp
public static Chart CreateTestChartWithSeries(...)
```
Generates a `Chart` object allowing for explicit configuration of the series collection. This overload supports more complex test cases where specific series properties or multiple distinct series need to be defined manually before assertion.
*   **Parameters**: Accepts arguments defining the chart metadata and the specific series configuration (exact signature details depend on internal series builders).
*   **Returns**: A `Chart` instance containing the specified series.
*   **Throws**: May throw exceptions if the series configuration is invalid or if required parameters are missing.

### CreateMultiSeriesChart
```csharp
public static Chart CreateMultiSeriesChart(this ChartDataServiceTests _, string chartName, IEnumerable<(string seriesName, IEnumerable<...> data)> seriesData)
```
Creates a `Chart` containing multiple distinct data series. Each entry in the input collection defines a specific series name and its corresponding dataset, enabling comprehensive testing of multi-series rendering logic.
*   **Parameters**:
    *   `_`: The `ChartDataServiceTests` instance acting as the extension context.
    *   `chartName`: The title or identifier assigned to the generated chart.
    *   `seriesData`: A collection of tuples, where each tuple contains a `seriesName` (string) and the associated `data` points (IEnumerable).
*   **Returns**: A `Chart` object populated with multiple series.
*   **Throws**: Throws an `ArgumentNullException` if `chartName` or `seriesData` is null. Throws an `ArgumentException` if any series name within the collection is null or empty.

### ShouldBeEquivalentTo
```csharp
public static void ShouldBeEquivalentTo(...)
```
Performs a deep comparison between two `Chart` instances to verify structural and data equivalence. This method is typically used within test assertions to ensure the output of a service matches the expected result.
*   **Parameters**: Takes the actual `Chart` instance and the expected `Chart` instance (or configuration) for comparison.
*   **Returns**: `void`.
*   **Throws**: Throws an assertion exception (e.g., `XunitException` or `NUnitAssertionException` depending on the test framework linked) if the charts are not equivalent.

## Usage

### Creating a Single-Series Test Chart
The following example demonstrates how to generate a basic chart with a single data series for immediate assertion.

```csharp
using SkiSharpChartEngine.Tests.Extensions;

public class ChartServiceTests : ChartDataServiceTests
{
    [Fact]
    public void GenerateChart_ShouldReturnValidStructure()
    {
        var dataPoints = new[] { 10.5, 20.0, 15.3, 30.0 };
        
        // Utilize the extension to create a test chart
        var chart = this.CreateTestChart("SalesOverview", dataPoints);

        // Assert properties
        Assert.Equal("SalesOverview", chart.Title);
        Assert.Single(chart.Series);
    }
}
```

### Creating and Validating a Multi-Series Chart
This example illustrates the creation of a chart with multiple named series and the use of the equivalence assertion.

```csharp
using SkiSharpChartEngine.Tests.Extensions;

public class MultiSeriesChartTests : ChartDataServiceTests
{
    [Fact]
    public void MultiSeriesChart_ShouldMatchExpectedConfiguration()
    {
        var seriesCollection = new List<(string Name, IEnumerable<double> Data)>
        {
            ("Q1", new[] { 100.0, 120.0 }),
            ("Q2", new[] { 130.0, 145.0 }),
            ("Q3", new[] { 110.0, 125.0 })
        };

        var actualChart = this.CreateMultiSeriesChart("QuarterlyResults", seriesCollection);
        
        var expectedChart = new Chart 
        { 
            Title = "QuarterlyResults",
            Series = new List<Series> 
            {
                new Series("Q1", new[] { 100.0, 120.0 }),
                new Series("Q2", new[] { 130.0, 145.0 }),
                new Series("Q3", new[] { 110.0, 125.0 })
            }
        };

        // Validate deep equivalence
        actualChart.ShouldBeEquivalentTo(expectedChart);
    }
}
```

## Notes

*   **Test Context Dependency**: All instance extension methods require a valid `ChartDataServiceTests` context (`this` reference). Invoking these methods statically without a proper test class instance will result in a compilation error.
*   **Data Integrity**: The `CreateMultiSeriesChart` method assumes that the provided `seriesName` strings are unique within the collection. Duplicate series names may lead to ambiguous test results or runtime errors depending on the underlying `Chart` implementation's handling of duplicate keys.
*   **Thread Safety**: As these methods are designed specifically for unit testing workflows, they are not guaranteed to be thread-safe. They should only be invoked within the context of a single test execution thread.
*   **Null Handling**: While the methods guard against null inputs for critical parameters like `chartName` and data collections, passing an empty (but non-null) collection will result in a valid `Chart` object with zero data points, which may affect rendering-related assertions.
