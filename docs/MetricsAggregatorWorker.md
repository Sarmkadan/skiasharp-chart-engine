# MetricsAggregatorWorker

The `MetricsAggregatorWorker` provides a centralized mechanism for tracking, aggregating, and retrieving statistical metrics within the `skiasharp-chart-engine` pipeline. It is designed to capture performance indicators, resource usage, or data distribution snapshots over time, automatically calculating essential statistical summaries—including count, sum, average, minimum, maximum, median, and standard deviation—for recorded numerical values associated with named metrics and optional tags.

## API

*   `MetricsAggregatorWorker()`: Initializes a new instance of the `MetricsAggregatorWorker` class.
*   `void Start()`: Starts the metrics aggregation service.
*   `void Stop()`: Stops the metrics aggregation service.
*   `void RecordMetric(string name, double value, string tags)`: Records a numerical value for a metric identified by its name, optionally associated with a string of tags.
*   `MetricStatistics GetMetricStatistics(string name)`: Retrieves the calculated statistics for the specified metric name. Returns `null` if the metric has not been recorded.
*   `Dictionary<string, MetricStatistics> GetAllMetrics()`: Returns a dictionary containing all currently recorded metrics, mapped by their unique names.
*   `void ClearMetrics()`: Removes all recorded metrics and their associated statistics.
*   `int GetMetricsCount()`: Returns the total number of unique metrics currently being tracked.
*   `string Name`: The name identifier for the metric.
*   `List<double> Values`: The list of raw numerical values recorded for the metric.
*   `string Tags`: The tags associated with the metric.
*   `DateTime LastUpdated`: The timestamp of the most recent update to the metric.
*   `int Count`: The total number of recorded values for the metric.
*   `double Sum`: The sum of all recorded values for the metric.
*   `double Average`: The arithmetic mean of all recorded values for the metric.
*   `double Min`: The minimum value recorded for the metric.
*   `double Max`: The maximum value recorded for the metric.
*   `double Median`: The median value of all recorded values for the metric.
*   `double StdDev`: The standard deviation of the recorded values for the metric.

## Usage

### Recording and Retrieving Metric Statistics

```csharp
var aggregator = new MetricsAggregatorWorker();
aggregator.Start();

// Record metrics with tags
aggregator.RecordMetric("render-time", 15.5, "gpu");
aggregator.RecordMetric("render-time", 18.2, "gpu");

// Retrieve statistics for a specific metric
var stats = aggregator.GetMetricStatistics("render-time");
if (stats != null)
{
    Console.WriteLine($"Metric: {stats.Name}, Average: {stats.Average}, Count: {stats.Count}");
}

aggregator.Stop();
```

### Clearing Metrics and Checking Count

```csharp
var aggregator = new MetricsAggregatorWorker();
aggregator.RecordMetric("data-load", 100.0, "network");
aggregator.RecordMetric("data-load", 150.0, "network");

Console.WriteLine($"Total metrics before clear: {aggregator.GetMetricsCount()}");

aggregator.ClearMetrics();

Console.WriteLine($"Total metrics after clear: {aggregator.GetMetricsCount()}");
```

## Notes

*   **Thread Safety**: The `MetricsAggregatorWorker` is designed to be thread-safe, allowing multiple threads to call `RecordMetric` concurrently without requiring external locking mechanisms.
*   **Metric Retrieval**: `GetMetricStatistics` returns `null` if the requested metric name does not exist. Ensure that calls to `GetMetricStatistics` account for this potential null return value.
*   **Statistics Calculation**: Statistical values like `Average`, `Median`, and `StdDev` are computed based on the `Values` list stored for the metric. For metrics with a high volume of data points, be aware that storing all values in the `Values` list may impact memory usage.
*   **State Management**: Use `Start()` and `Stop()` to properly initialize and dispose of the worker service, especially if the service manages background processing threads.
