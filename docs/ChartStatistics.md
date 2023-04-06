# ChartStatistics

The `ChartStatistics` class provides comprehensive statistical analysis capabilities for data series within the `skiasharp-chart-engine` framework. It facilitates the computation of fundamental descriptive statistics, identifies outliers, and performs trend analysis, enabling developers to derive actionable insights from complex charting datasets.

## API

### Constructors

*   **`ChartStatistics()`**
    Initializes a new instance of the `ChartStatistics` class.

### Methods

*   **`SeriesStatistics CalculateSeriesStatistics(IEnumerable<double> data, string seriesName)`**
    Calculates a summary of statistical metrics for a provided collection of data points.
    *   **Parameters:** `data` (The collection of numerical values), `seriesName` (The identifier for the series).
    *   **Returns:** A `SeriesStatistics` object containing the computed metrics.
    *   **Throws:** `ArgumentNullException` if `data` is null; `ArgumentException` if `data` is empty.

*   **`List<OutlierInfo> DetectOutliers(IEnumerable<double> data)`**
    Identifies outliers within a data series based on statistical distribution analysis.
    *   **Parameters:** `data` (The collection of numerical values).
    *   **Returns:** A `List<OutlierInfo>` containing details of identified outliers.

*   **`double CalculateTrend(IEnumerable<double> data)`**
    Calculates a trend value (typically a slope or gradient) for the provided data set.
    *   **Parameters:** `data` (The collection of numerical values).
    *   **Returns:** A `double` representing the calculated trend.

### Properties

*   **`string SeriesName`**
    Gets or sets the name associated with the chart series.
*   **`int Count`**
    Gets the total number of data points.
*   **`double Sum`**
    Gets the arithmetic sum of the data points.
*   **`double Mean`**
    Gets the arithmetic mean (average) of the data points.
*   **`double Median`**
    Gets the median value of the data points.
*   **`double Mode`**
    Gets the most frequently occurring value in the data set.
*   **`double Min`**
    Gets the minimum value in the data set.
*   **`double Max`**
    Gets the maximum value in the data set.
*   **`double Range`**
    Gets the difference between the maximum and minimum values.
*   **`double Variance`**
    Gets the statistical variance of the data points.
*   **`double StandardDeviation`**
    Gets the standard deviation of the data points.
*   **`double CoefficientOfVariation`**
    Gets the coefficient of variation (ratio of standard deviation to the mean).
*   **`double Q1`**
    Gets the first quartile (25th percentile).
*   **`double Q3`**
    Gets the third quartile (75th percentile).
*   **`double IQR`**
    Gets the interquartile range (difference between Q3 and Q1).
*   **`double Skewness`**
    Gets the measure of the asymmetry of the data distribution.

## Usage

### Calculating Basic Statistics
```csharp
var data = new List<double> { 10.5, 12.0, 15.2, 9.8, 14.1 };
var statsEngine = new ChartStatistics();
var seriesStats = statsEngine.CalculateSeriesStatistics(data, "RevenueSeries");

Console.WriteLine($"Mean: {seriesStats.Mean}");
Console.WriteLine($"Standard Deviation: {seriesStats.StandardDeviation}");
```

### Detecting Outliers
```csharp
var data = new List<double> { 10, 12, 11, 13, 100, 12, 11 };
var statsEngine = new ChartStatistics();
var outliers = statsEngine.DetectOutliers(data);

foreach (var outlier in outliers)
{
    Console.WriteLine($"Outlier detected at value: {outlier.Value}");
}
```

## Notes

*   **Empty Datasets:** Methods that process `IEnumerable<double>` will throw an `ArgumentException` if the input collection is empty, as statistical operations like mean or variance are undefined for such sets.
*   **Thread Safety:** The `ChartStatistics` instance is not inherently thread-safe for concurrent read/write operations. When multiple threads are analyzing different datasets, ensure each thread utilizes its own instance of `ChartStatistics`.
*   **Performance:** For extremely large datasets (millions of points), `CalculateSeriesStatistics` may incur significant CPU overhead due to sorting operations required for median and quartile calculations. Ensure data is pre-filtered if high-performance real-time updates are required.
