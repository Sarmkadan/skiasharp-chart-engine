# ChartControllerExtensions

`ChartControllerExtensions` is a static class in `SkiaSharpChartEngine.API.Controllers` that adds convenience overloads to `ChartController`. Each method validates its arguments, then forwards the call to the matching `ChartController` action and returns its result unchanged. The class adds no behavior of its own beyond argument checks and, in one case, building an `UpdateChartRequest`.

## API

* `public static Task<ApiResponse<ChartDto>> GetChartByIdAsync(this ChartController controller, string id)`: Retrieves a chart by its ID. Calls `GetChartAsync(id)` with the default `CancellationToken`. Throws `ArgumentNullException` if `controller` or `id` is null, and `ArgumentException` if `id` is empty.
* `public static Task<ApiResponse<bool>> UpdateChartConfigurationAsync(this ChartController controller, string id, ChartConfiguration configuration)`: Replaces a chart's configuration. Wraps `configuration` in an `UpdateChartRequest` and calls `UpdateChartAsync(id, request)`. Throws `ArgumentNullException` if `controller` or `configuration` is null, and `ArgumentException` if `id` is empty.
* `public static Task<ApiResponse<bool>> DeleteChartAndDataAsync(this ChartController controller, string id)`: Deletes a chart by calling `DeleteChartAsync(id)`. The name says the chart's associated data is removed too. Whether that holds depends on `ChartController.DeleteChartAsync`, which this class does not change. Throws `ArgumentNullException` if `controller` is null, and `ArgumentException` if `id` is empty.

All three methods are `async` and return the `ApiResponse<T>` produced by the underlying controller action.

## Usage

The extension methods are called on an existing `ChartController` instance:

```csharp
// Requires a ChartController instance, e.g. resolved from DI or created in a test.
ChartController controller = /* ... */;

// Fetch a chart by ID
ApiResponse<ChartDto> chart = await controller.GetChartByIdAsync("chart-123");

// Replace the configuration of an existing chart
ChartConfiguration config = new ChartConfiguration();
ApiResponse<bool> updated = await controller.UpdateChartConfigurationAsync("chart-123", config);

// Delete a chart and its data
ApiResponse<bool> deleted = await controller.DeleteChartAndDataAsync("chart-123");
```

## Notes

* The `GetChartByIdAsync`, `UpdateChartConfigurationAsync` and `DeleteChartAndDataAsync` methods do not accept a `CancellationToken`. Callers who need cancellation should call `GetChartAsync`, `UpdateChartAsync` or `DeleteChartAsync` on the controller directly.
* The `API/` folder is excluded from the build by the `<Compile Remove="API/**/*.cs" />` entry in `skiasharp-chart-engine.csproj`. This file is therefore not compiled in the default build, so the signatures above are documented from the source and have not been verified by compiling this file.
* The overloads do not catch exceptions from the controller. Any exception thrown by the underlying action propagates to the caller.
