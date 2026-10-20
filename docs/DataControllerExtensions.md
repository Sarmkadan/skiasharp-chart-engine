# DataControllerExtensions

**Namespace:** `SkiaSharpChartEngine.API.Controllers`
**File:** `API/Controllers/DataControllerExtensions.cs`
**Type:** `public static class`

## Overview

Extension methods for `DataController` that compose existing controller operations (aggregation, filtering, validation, resampling) into higher-level workflows. Every method is asynchronous and returns an `ApiResponse<T>`, propagating errors from the underlying controller calls.

## Methods

### GetDataPointsInRangeAsync

```csharp
public static async Task<ApiResponse<List<DataPoint>>> GetDataPointsInRangeAsync(
    this DataController controller,
    double minX,
    double maxX)
```

Aggregates all data points via `controller.AggregateDataAsync()`, then filters the result to the inclusive X-axis range `[minX, maxX]`.

**Parameters**

| Name | Type | Description |
|------|------|-------------|
| `controller` | `DataController` | The controller instance (extended). |
| `minX` | `double` | Minimum X value (inclusive). |
| `maxX` | `double` | Maximum X value (inclusive). |

**Returns** - `ApiResponse<List<DataPoint>>` with the filtered points on success, or the upstream error response.

**Exceptions**

- `ArgumentNullException` - `controller` is null.
- `ArgumentException` (via `BadRequest`) - `minX` is greater than `maxX`.

---

### ValidateAndResampleDataAsync

```csharp
public static async Task<ApiResponse<object>> ValidateAndResampleDataAsync(
    this DataController controller,
    List<DataPoint> dataPoints,
    int newSamplingRate)
```

Validates the supplied data points via `controller.ValidateDataPoints()`. If validation succeeds, resamples them to the target count via `controller.ResampleDataAsync()`.

**Parameters**

| Name | Type | Description |
|------|------|-------------|
| `controller` | `DataController` | The controller instance (extended). |
| `dataPoints` | `List<DataPoint>` | Data points to validate and resample. |
| `newSamplingRate` | `int` | Target number of data points after resampling. |

**Returns** - `ApiResponse<object>` with the resampled result on success, or the validation error response.

**Exceptions**

- `ArgumentNullException` - `controller` or `dataPoints` is null.

---

### AggregateAndFilterDataAsync

```csharp
public static async Task<ApiResponse<List<DataPoint>>> AggregateAndFilterDataAsync(
    this DataController controller,
    List<DataPoint> dataPoints,
    double minX,
    double maxX)
```

Aggregates the supplied data points using the `"average"` strategy with a bucket size of `1` via `controller.AggregateDataAsync(dataPoints, "average", 1)`, then filters the result to the inclusive X-axis range `[minX, maxX]`.

**Parameters**

| Name | Type | Description |
|------|------|-------------|
| `controller` | `DataController` | The controller instance (extended). |
| `dataPoints` | `List<DataPoint>` | Data points to aggregate and filter. |
| `minX` | `double` | Minimum X value (inclusive). |
| `maxX` | `double` | Maximum X value (inclusive). |

**Returns** - `ApiResponse<List<DataPoint>>` with the aggregated and filtered points on success, or the upstream error response.

**Exceptions**

- `ArgumentNullException` - `controller` or `dataPoints` is null.
- `ArgumentException` (via `BadRequest`) - `minX` is greater than `maxX`.

## Dependencies

- `DataController` - provides `AggregateDataAsync`, `FilterByRange`, `ValidateDataPoints`, `ResampleDataAsync`.
- `ApiResponse<T>` - standard response wrapper with `IsSuccess`, `StatusCode`, `Message`, `Value` and a `BadRequest` factory.
- `DataPoint` - the data model representing a single chart data point.
