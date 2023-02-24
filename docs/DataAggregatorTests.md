# DataAggregatorTests
The `DataAggregatorTests` class is a test suite designed to validate the functionality of data aggregation methods. It provides a comprehensive set of test cases to ensure that data aggregation operations, such as grouping by count or interval, and calculating statistics, behave as expected under various input conditions, including edge cases like null or empty data points.

## API
The `DataAggregatorTests` class contains the following public members:
- `public DataAggregatorTests`: The constructor for the `DataAggregatorTests` class.
- `public void AggregateByCount_WithNullDataPoints_ReturnsEmptyList`: Tests that aggregating by count with null data points returns an empty list.
- `public void AggregateByCount_WithEmptyDataPoints_ReturnsEmptyList`: Tests that aggregating by count with empty data points returns an empty list.
- `public void AggregateByCount_WithZeroBucketCount_ThrowsArgumentException`: Tests that aggregating by count with a bucket count of zero throws an `ArgumentException`.
- `public void AggregateByCount_WithNegativeBucketCount_ThrowsArgumentException`: Tests that aggregating by count with a negative bucket count throws an `ArgumentException`.
- `public void AggregateByCount_WithAverageAggregation_ComputesAverageBuckets`: Tests that aggregating by count with average aggregation computes the average buckets correctly.
- `public void AggregateByCount_WithSumAggregation_ComputesSumBuckets`: Tests that aggregating by count with sum aggregation computes the sum buckets correctly.
- `public void AggregateByCount_WithMinAggregation_ComputesMinBuckets`: Tests that aggregating by count with min aggregation computes the min buckets correctly.
- `public void AggregateByCount_WithMaxAggregation_ComputesMaxBuckets`: Tests that aggregating by count with max aggregation computes the max buckets correctly.
- `public void AggregateByCount_WithMedianAggregation_ComputesMedianBuckets`: Tests that aggregating by count with median aggregation computes the median buckets correctly.
- `public void AggregateByCount_WithMoreBucketsThanPoints_CreatesOnePointPerBucket`: Tests that aggregating by count with more buckets than points creates one point per bucket.
- `public void AggregateByInterval_WithNullDataPoints_ReturnsEmptyDictionary`: Tests that aggregating by interval with null data points returns an empty dictionary.
- `public void AggregateByInterval_GroupsByLabel`: Tests that aggregating by interval groups data points by label.
- `public void AggregateByInterval_WithNullLabel_GroupsAsUnknown`: Tests that aggregating by interval with null labels groups them as unknown.
- `public void CalculateStatistics_WithNullDataPoints_ReturnsNull`: Tests that calculating statistics with null data points returns null.
- `public void CalculateStatistics_WithEmptyDataPoints_ReturnsNull`: Tests that calculating statistics with empty data points returns null.
- `public void CalculateStatistics_ComputesSumAndAverage`: Tests that calculating statistics computes the sum and average correctly.
- `public void CalculateStatistics_ComputesMinAndMax`: Tests that calculating statistics computes the min and max correctly.
- `public void CalculateStatistics_ComputesMedian`: Tests that calculating statistics computes the median correctly.
- `public void CalculateStatistics_ComputesRange`: Tests that calculating statistics computes the range correctly.

## Usage
The following examples demonstrate how to use the `DataAggregatorTests` class:
```csharp
// Example 1: Testing aggregation by count
DataAggregatorTests aggregatorTests = new DataAggregatorTests();
aggregatorTests.AggregateByCount_WithNullDataPoints_ReturnsEmptyList();
aggregatorTests.AggregateByCount_WithEmptyDataPoints_ReturnsEmptyList();

// Example 2: Testing statistics calculation
DataAggregatorTests statsTests = new DataAggregatorTests();
statsTests.CalculateStatistics_WithNullDataPoints_ReturnsNull();
statsTests.CalculateStatistics_ComputesSumAndAverage();
```

## Notes
When using the `DataAggregatorTests` class, consider the following:
- The class is designed for testing data aggregation and statistics calculation methods. It does not provide actual data aggregation or statistics calculation functionality.
- The test methods are designed to cover various edge cases, including null or empty data points, and invalid input parameters.
- The class does not provide any thread-safety guarantees. If you plan to use it in a multi-threaded environment, ensure that you implement proper synchronization mechanisms to avoid any potential issues.
- The `ArgumentException` is thrown when invalid input parameters are provided, such as a zero or negative bucket count. Ensure that you handle these exceptions properly in your code.
