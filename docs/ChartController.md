# ChartController

Provides endpoints and properties for managing chart resources, including creation, retrieval, updating, and deletion of charts with associated metadata and configuration.

## API

### `ChartController`
Entry point for chart management operations. This controller exposes asynchronous methods for chart lifecycle management and exposes chart properties.

### `async Task<ApiResponse<ChartDto>> GetChartAsync()`
Retrieves the chart associated with the controller's `Id`.

- **Return value**: An `ApiResponse<ChartDto>` containing the chart data if found, or an error response.
- **Exceptions**: May throw if the underlying data store is unavailable or the chart does not exist.

### `async Task<ApiResponse<string>> CreateChartAsync()`
Creates a new chart using the controller's current properties (`Title`, `ChartType`, `Configuration`).

- **Return value**: An `ApiResponse<string>` containing the unique identifier of the newly created chart.
- **Exceptions**: May throw if validation fails, required properties are missing, or the creation operation is rejected by the data store.

### `async Task<ApiResponse<bool>> UpdateChartAsync()`
Updates an existing chart identified by `Id` with the current controller properties.

- **Return value**: An `ApiResponse<bool>` indicating success (`true`) or failure (`false`) of the update operation.
- **Exceptions**: May throw if the chart does not exist, validation fails, or the update is rejected by the data store.

### `async Task<ApiResponse<bool>> DeleteChartAsync()`
Deletes the chart identified by `Id`.

- **Return value**: An `ApiResponse<bool>` indicating success (`true`) or failure (`false`) of the deletion.
- **Exceptions**: May throw if the chart does not exist or the deletion operation is rejected by the data store.

### `string? Id`
Gets or sets the unique identifier of the chart. Used to reference an existing chart in update and delete operations.

### `string? Title`
Gets or sets the title of the chart. Required for chart creation.

### `ChartType ChartType`
Gets or sets the type of the chart (e.g., bar, line, pie). Required for chart creation.

### `ChartConfiguration? Configuration`
Gets or sets the chart configuration object defining data, styling, and behavior. Optional depending on chart type.

### `DateTime CreatedAt`
Gets the timestamp when the chart was created. Read-only; set automatically on creation.

## Usage
