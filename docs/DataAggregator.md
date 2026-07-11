# DataAggregator

The `DataAggregator` class provides a centralized mechanism for processing collections of `DataPoint` objects within the `skiasharp-chart-engine`. It facilitates the transformation of raw dataset entries into structured groupings by count or interval and performs essential statistical computations—including central tendency and dispersion metrics—required for accurate data visualization and reporting.

## API

*   `public DataAggregator()`
    Initializes a new instance of the `DataAggregator` class.

*   `public List<DataPoint> AggregateByCount`
    Gets or sets a list of data points summarized or partitioned based on count-based algorithms.

*   `public Dictionary<string, List<DataPoint>> AggregateByInterval`
    Gets or sets a dictionary of data points categorized by discrete time or value intervals, keyed by the interval descriptor.

*   `public DataStatistics CalculateStatistics`
    Gets the computed statistical summary of the dataset, returned as a `DataStatistics` object.

*   `public int Count`
    Gets the total number of data points currently held in the aggregator.

*   `public double Sum`
    Gets the sum of all data point values in the dataset.

*   `public double Average`
    Gets the arithmetic mean of the values in the dataset.

*   `public double Min`
    Gets the minimum value found in the dataset.

*   `public double Max`
    Gets the maximum value found in the dataset.

*   `public double Median`
    Gets the median value of the dataset.

*   `public double StandardDeviation`
    Gets the standard deviation of the values in the dataset.

*   `public double Range`
    Gets the range of the dataset, calculated as the difference between the maximum and minimum values.

*   `public DateTime CalculatedAt`
    Gets the timestamp indicating the date and time when the statistical analysis was last finalized.

## Usage

### Example 1: Calculating Basic Statistics
```csharp
var aggregator = new DataAggregator();
// Assuming data points have been populated
var stats = aggregator.CalculateStatistics;

Console.WriteLine($"Average value: {aggregator.Average}");
Console.WriteLine($"Data points analyzed: {aggregator.Count}");
Console.WriteLine($"Stats finalized at: {aggregator.CalculatedAt}");
```

### Example 2: Accessing Interval-Based Data
```csharp
var aggregator = new DataAggregator();
// Assuming data has been processed into intervals
if (aggregator.AggregateByInterval.ContainsKey("daily"))
{
    var dailyPoints = aggregator.AggregateByInterval["daily"];
    foreach (var point in dailyPoints)
    {
        // Process individual data points
    }
}
```

## Notes

*   **Thread Safety**: The `DataAggregator` class is not inherently thread-safe. Concurrent access to the instance properties, particularly during update operations or statistic calculation, may lead to inconsistent states. External synchronization is required when sharing instances across threads.
*   **Empty Datasets**: If the `DataAggregator` is initialized or accessed without any data points, statistical properties such as `Average`, `Median`, and `StandardDeviation` may return default values (e.g., `0.0` or `double.NaN`) depending on the internal implementation.
*   **Timestamping**: The `CalculatedAt` property reflects the time the `DataStatistics` object was last generated. Consumers should re-read this property if the underlying data has been modified.
