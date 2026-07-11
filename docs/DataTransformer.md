# DataTransformer

The `DataTransformer` class provides a comprehensive suite of methods for preprocessing and manipulating datasets intended for visualization within the `skiasharp-chart-engine` framework. It facilitates common data cleaning and transformation workflows, enabling developers to normalize, filter, scale, and smooth data points before rendering, ensuring that input data is appropriately formatted for specific chart types.

## API

### Constructors

#### `DataTransformer()`
Initializes a new instance of the `DataTransformer` class.

### Methods

#### `NormalizeValues(List<DataPoint> data)`
Normalizes the values of the provided data points to a range between 0 and 1.
*   **Parameters:** `data` (The list of `DataPoint` objects to normalize).
*   **Returns:** A new `List<DataPoint>` with normalized values.
*   **Throws:** `ArgumentNullException` if input is null.

#### `ApplyLogTransformation(List<DataPoint> data)`
Applies a logarithmic transformation to the values of the provided data points.
*   **Parameters:** `data` (The list of `DataPoint` objects to transform).
*   **Returns:** A new `List<DataPoint>` with log-transformed values.
*   **Throws:** `ArgumentException` if values contain non-positive numbers.

#### `FilterOutliers(List<DataPoint> data)`
Removes statistical outliers from the dataset based on standard deviation thresholds.
*   **Parameters:** `data` (The list of `DataPoint` objects to filter).
*   **Returns:** A new `List<DataPoint>` containing only the filtered data.

#### `ApplyMovingAverage(List<DataPoint> data)`
Applies a moving average calculation to smooth out fluctuations in the dataset.
*   **Parameters:** `data` (The list of `DataPoint` objects to smooth).
*   **Returns:** A new `List<DataPoint>` with smoothed values.

#### `ScaleValues(List<DataPoint> data)`
Scales the values of the provided data points by a defined factor.
*   **Parameters:** `data` (The list of `DataPoint` objects to scale).
*   **Returns:** A new `List<DataPoint>` with scaled values.

#### `OffsetValues(List<DataPoint> data)`
Applies a constant numerical offset to the values of the provided data points.
*   **Parameters:** `data` (The list of `DataPoint` objects to offset).
*   **Returns:** A new `List<DataPoint>` with offset values.

#### `RankDataPoints(List<DataPoint> data)`
Ranks the provided data points based on their values.
*   **Parameters:** `data` (The list of `DataPoint` objects to rank).
*   **Returns:** A new `List<RankedDataPoint>` containing the ranked data.

### Properties

#### `Label`
Gets or sets the string label associated with the data transformation context.

#### `Value`
Gets or sets the numerical value associated with the data transformation context.

#### `Rank`
Gets or sets the integer rank associated with the data transformation context.

## Usage

```csharp
// Example 1: Normalizing and filtering outliers
var rawData = GetChartData();
var transformer = new DataTransformer();

var normalizedData = transformer.NormalizeValues(rawData);
var cleanData = transformer.FilterOutliers(normalizedData);
```

```csharp
// Example 2: Applying moving average and ranking
var rawData = GetChartData();
var transformer = new DataTransformer();

var smoothedData = transformer.ApplyMovingAverage(rawData);
var rankedData = transformer.RankDataPoints(smoothedData);
```

## Notes

*   **Thread Safety:** The `DataTransformer` class and its methods are not thread-safe. If transformation operations are required across multiple threads, ensure that each thread utilizes its own instance of `DataTransformer`, or implement external locking mechanisms when sharing datasets.
*   **Edge Cases:**
    *   Methods operating on `List<DataPoint>` expect non-null, non-empty collections. Passing an empty list may result in an empty list return or an `ArgumentException` depending on the specific method implementation.
    *   `ApplyLogTransformation` requires all values to be positive. Values less than or equal to zero will result in an `ArgumentException`.
    *   `NormalizeValues` will behave unexpectedly if the dataset contains only identical values (leading to division by zero). Ensure validation logic is implemented upstream if datasets with uniform values are expected.
