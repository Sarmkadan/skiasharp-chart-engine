# DataController

The `DataController` is a service component within the SkiaSharp Chart Engine responsible for managing and processing time-series data points. It provides asynchronous and synchronous methods to validate, filter, aggregate, and resample data for chart visualization and analysis.

## API

### `public DataController`

The default constructor initializes a new instance of the `DataController` with default configuration settings for data processing.

### `public async Task<ApiResponse<object>> GetDataStatisticsAsync`

Retrieves statistical summaries of the dataset, such as min, max, mean, and standard deviation. This method is intended for analytical purposes and supports large datasets through asynchronous execution.

- **Parameters**: None.
- **Return value**: An `ApiResponse<object>` containing a dictionary of statistical metrics. The object structure is defined by the consuming application.
- **Exceptions**: Throws if internal data access fails or if the dataset is empty.

### `public ApiResponse<object> ValidateDataPoints`

Validates the integrity of a collection of data points, ensuring they conform to expected schema and value constraints (e.g., non-null timestamps, finite numeric values).

- **Parameters**: None (uses internally managed data).
- **Return value**: An `ApiResponse<object>` with a boolean `IsValid` flag and a list of validation errors, if any.
- **Exceptions**: Throws if data access fails or if the internal dataset is uninitialized.

### `public async Task<ApiResponse<List<DataPoint>>> AggregateDataAsync`

Computes aggregated values (e.g., sum, average) over specified intervals (e.g., hourly, daily) from the dataset. Supports asynchronous execution for performance with large datasets.

- **Parameters**: None.
- **Return value**: An `ApiResponse<List<DataPoint>>` containing aggregated data points. Returns an empty list if no data is available.
- **Exceptions**: Throws if aggregation logic fails or if the internal dataset is empty.

### `public ApiResponse<List<DataPoint>> FilterByRange`

Filters the dataset to include only data points within a specified time range. The range is inclusive of the start and end timestamps.

- **Parameters**: None (uses internally managed data and filter criteria).
- **Return value**: An `ApiResponse<List<DataPoint>>` containing the filtered data points. Returns an empty list if no data falls within the range.
- **Exceptions**: Throws if the internal dataset is uninitialized or if the filter parameters are invalid.

### `public async Task<ApiResponse<List<DataPoint>>> ResampleDataAsync`

Resamples the dataset to a uniform time interval, interpolating or aggregating values as needed. Useful for aligning irregularly sampled data to a consistent grid.

- **Parameters**: None.
- **Return value**: An `ApiResponse<List<DataPoint>>` containing the resampled data points. Returns an empty list if the dataset is empty.
- **Exceptions**: Throws if resampling logic fails or if the target interval is invalid.

## Usage

### Example 1: Validating and Filtering Data
