# DataAggregatorTestsExtensions

Utility class providing extension methods and helper methods for testing data aggregation functionality in `DataAggregatorTests`. Designed to simplify test setup, data generation, and assertion logic when verifying aggregation behavior.

## API

### `WithDataPoints(this DataAggregatorTests tests, IEnumerable<DataPoint> dataPoints)`

Extends a `DataAggregatorTests` instance with a predefined set of data points for aggregation testing.

- **Parameters**:
  - `dataPoints`: Collection of `DataPoint` objects to be aggregated during test execution.
- **Return value**: The same `DataAggregatorTests` instance for method chaining.
- **Throws**: `ArgumentNullException` if `dataPoints` is `null`.

### `WithDataPoints(this DataAggregatorTests tests, params DataPoint[] dataPoints)`

Overload allowing inline specification of data points for aggregation testing.

- **Parameters**:
  - `dataPoints`: Variable number of `DataPoint` objects to be aggregated.
- **Return value**: The same `DataAggregatorTests` instance for method chaining.
- **Throws**: `ArgumentNullException` if `dataPoints` is `null`.

### `ShouldAggregateToCount(this DataAggregatorTests tests)`

Asserts that the aggregator correctly counts the number of data points processed.

- **Parameters**: None.
- **Throws**: `AssertionException` if the actual count does not match the expected value.

### `ShouldAggregateToValues(this DataAggregatorTests tests)`

Asserts that the aggregator correctly extracts and returns the aggregated values from the data points.

- **Parameters**: None.
- **Throws**: `AssertionException` if the aggregated values do not match the expected collection.

### `ShouldCalculateStatistics(this DataAggregatorTests tests)`

Asserts that the aggregator correctly computes statistical measures (e.g., mean, min, max) over the data points.

- **Parameters**: None.
- **Throws**: `AssertionException` if the computed statistics do not match the expected values.

### `CreateSequentialDataPoints(int count, double startValue = 0.0, double step = 1.0) -> List<DataPoint>`

Generates a list of sequentially increasing `DataPoint` objects with linearly increasing values.

- **Parameters**:
  - `count`: Number of data points to generate.
  - `startValue`: Initial value of the first data point (default: `0.0`).
  - `step`: Increment between consecutive data points (default: `1.0`).
- **Return value**: List of generated `DataPoint` objects.
- **Throws**: `ArgumentOutOfRangeException` if `count` is negative.

### `CreateRandomDataPoints(int count, double minValue = 0.0, double maxValue = 100.0) -> List<DataPoint>`

Generates a list of `DataPoint` objects with random values within a specified range.

- **Parameters**:
  - `count`: Number of data points to generate.
  - `minValue`: Minimum possible value for a data point (default: `0.0`).
  - `maxValue`: Maximum possible value for a data point (default: `100.0`).
- **Return value**: List of generated `DataPoint` objects.
- **Throws**: `ArgumentOutOfRangeException` if `count` is negative or if `minValue` > `maxValue`.

### `ShouldGroupByLabel(this DataAggregatorTests tests)`

Asserts that the aggregator correctly groups data points by their label during processing.

- **Parameters**: None.
- **Throws**: `AssertionException` if the grouping does not match the expected structure.

### `ShouldHandleNullDataPoints(this DataAggregatorTests tests)`

Asserts that the aggregator gracefully handles `null` data points without throwing exceptions.

- **Parameters**: None.
- **Throws**: `AssertionException` if the aggregator throws an exception when processing `null` data points.

## Usage
