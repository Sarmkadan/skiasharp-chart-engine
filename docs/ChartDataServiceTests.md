# ChartDataServiceTests

`ChartDataServiceTests` is the unit test suite for the `ChartDataService` class in the `skiasharp-chart-engine` project. It provides comprehensive coverage of chart validation, axis range calculation, data filtering, transformation, and normalization logic. Each test method verifies a specific behavior or edge case, ensuring that the service correctly handles valid inputs, throws appropriate exceptions for invalid arguments, and maintains immutability where required.

## API

### ChartDataServiceTests

The default constructor for the test class. It is invoked by the test runner to instantiate the test fixture before each test method executes.

### public void ValidateChart_WithNullChart_ThrowsArgumentNullException

Verifies that calling `ValidateChart` with a `null` chart reference throws an `ArgumentNullException`. This test ensures the service fails fast when presented with a missing chart object.

**Throws:** `ArgumentNullException` (expected in the test).

### public void ValidateChart_WithNoSeries_ThrowsInvalidChartDataException

Verifies that calling `ValidateChart` on a chart that contains zero data series throws an `InvalidChartDataException`. This guards against attempting to render or process a chart with no plottable data.

**Throws:** `InvalidChartDataException` (expected in the test).

### public void ValidateChart_WithValidChartData_DoesNotThrow

Confirms that `ValidateChart` completes without any exception when supplied with a properly constructed chart containing at least one valid series. This is the happy-path validation scenario.

### public void ValidateSeries_WithZeroLineWidth_ThrowsInvalidChartDataException

Verifies that validating a series whose line width is set to zero throws an `InvalidChartDataException`. A zero line width is considered invalid because it would produce an invisible or non-renderable stroke.

**Throws:** `InvalidChartDataException` (expected in the test).

### public void CalculateAxisRange_WithLinearScale_ReturnsExactMinAndMax

Tests that `CalculateAxisRange` for a linear scale returns a range whose minimum and maximum exactly match the minimum and maximum values present in the data collection. No padding or rounding is applied in this scenario.

**Returns:** A range object with `Min` and `Max` equal to the data extremes.

### public void CalculateAxisRange_WithLogarithmicScale_EnforcesMinimumOfOne

Verifies that when calculating an axis range with a logarithmic scale, the minimum value is forced to at least 1, even if the data contains values less than 1. This prevents invalid logarithmic domain inputs (log of zero or negative numbers).

**Returns:** A range object whose `Min` is 1 or greater.

### public void CalculateAxisRange_EmptyCollection_ReturnsDefaultRange

Tests that passing an empty data point collection to `CalculateAxisRange` returns a default range, typically zero-to-zero or another safe fallback, rather than throwing an exception or returning an undefined result.

**Returns:** A default range object.

### public void FilterDataPoints_WithPositiveYFilter_ReturnsOnlyMatchingPoints

Verifies that `FilterDataPoints` correctly applies a predicate that selects only points with positive Y values. The returned collection contains exclusively points where `Y > 0`, and all non-matching points are excluded.

**Returns:** A filtered collection of data points.

### public void FilterDataPoints_WithNullPredicate_ThrowsArgumentNullException

Ensures that passing a `null` predicate to `FilterDataPoints` throws an `ArgumentNullException`. The method requires a valid filter function to operate.

**Throws:** `ArgumentNullException` (expected in the test).

### public void TransformChartData_WithDoubleYTransformer_TransformsAllSeriesPoints

Tests that `TransformChartData` applies a transformation function (e.g., doubling the Y value) to every data point across all series in the chart. The returned chart contains the transformed values.

**Returns:** A new chart object with transformed data points.

### public void TransformChartData_OriginalChartIsNotMutated

Verifies that `TransformChartData` does not modify the original chart instance passed to it. The original chart’s data points remain unchanged, confirming that the method operates immutably and returns a new chart.

### public void NormalizeDataPoints_WithKnownRange_ScalesPointsToZeroOneInterval

Tests that `NormalizeDataPoints` scales a collection of data points into the [0, 1] interval based on a known input range. After normalization, the minimum value maps to 0 and the maximum maps to 1, with intermediate values proportionally distributed.

**Returns:** A collection of normalized data points bounded between 0 and 1.

### public void ValidateChart_AfterSuccessfulValidation_LogsInformationMessage

Verifies that a successful call to `ValidateChart` produces an information-level log message. This confirms that the service records normal operational events through the logging infrastructure.

## Usage

### Example 1: Full validation and transformation pipeline

```csharp
[Test]
public void ProcessChart_ValidInput_CompletesSuccessfully()
{
    // Arrange
    var service = new ChartDataService();
    var chart = new Chart
    {
        Series = new List<Series>
        {
            new Series
            {
                LineWidth = 2.0,
                DataPoints = new List<DataPoint>
                {
                    new DataPoint(1, 10),
                    new DataPoint(2, 20),
                    new DataPoint(3, 15)
                }
            }
        }
    };

    // Act & Assert - validation does not throw
    Assert.DoesNotThrow(() => service.ValidateChart(chart));

    // Transform data (double Y values)
    var transformedChart = service.TransformChartData(chart, p => new DataPoint(p.X, p.Y * 2));

    // Original chart remains unchanged
    Assert.That(chart.Series[0].DataPoints[0].Y, Is.EqualTo(10));

    // Transformed chart has doubled values
    Assert.That(transformedChart.Series[0].DataPoints[0].Y, Is.EqualTo(20));
}
```

### Example 2: Filtering and normalizing with axis range calculation

```csharp
[Test]
public void FilterAndNormalize_ProducesScaledOutput()
{
    // Arrange
    var service = new ChartDataService();
    var dataPoints = new List<DataPoint>
    {
        new DataPoint(0, -5),
        new DataPoint(1, 3),
        new DataPoint(2, 8),
        new DataPoint(3, -2),
        new DataPoint(4, 6)
    };

    // Act - filter only positive Y values
    var filtered = service.FilterDataPoints(dataPoints, p => p.Y > 0);

    // Assert - only three points remain
    Assert.That(filtered.Count, Is.EqualTo(3));
    Assert.That(filtered.All(p => p.Y > 0), Is.True);

    // Calculate range on filtered data
    var range = service.CalculateAxisRange(filtered, ScaleType.Linear);
    Assert.That(range.Min, Is.EqualTo(3));
    Assert.That(range.Max, Is.EqualTo(8));

    // Normalize filtered points to [0, 1]
    var normalized = service.NormalizeDataPoints(filtered, range);
    Assert.That(normalized[0].Y, Is.EqualTo(0.0).Within(1e-6));   // min -> 0
    Assert.That(normalized[2].Y, Is.EqualTo(1.0).Within(1e-6));   // max -> 1
}
```

## Notes

- **Exception handling:** Methods that accept predicates, transformers, or chart references treat `null` arguments as immediate errors, throwing `ArgumentNullException` before any processing occurs. Validation-specific failures (missing series, zero line width) throw `InvalidChartDataException` to distinguish data integrity issues from programming errors.
- **Immutability:** Transformation methods such as `TransformChartData` and `NormalizeDataPoints` return new instances and do not mutate the input objects. Callers must capture the return value to access modified data.
- **Logarithmic scale constraints:** When using logarithmic scale axis calculations, the minimum bound is clamped to 1. Data sets containing zero or negative values will have those values excluded or clamped depending on upstream filtering; the range calculation itself does not filter data points.
- **Empty collections:** `CalculateAxisRange` with an empty collection returns a default range rather than throwing. Callers should guard against rendering charts with empty series at a higher level if a meaningful range is required.
- **Thread safety:** The test methods themselves are single-threaded unit tests. The underlying `ChartDataService` methods are expected to be stateless and therefore safe for concurrent invocation, but no explicit thread-safety guarantees are tested within this suite.
