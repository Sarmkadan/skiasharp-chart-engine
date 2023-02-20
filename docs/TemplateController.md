# TemplateController

The `TemplateController` class provides a centralized interface for managing chart templates within the skiasharp-chart-engine. It exposes synchronous and asynchronous operations to retrieve, create, and delete `ChartTemplate` objects, encapsulating the underlying data access logic and returning standardized `ApiResponse` wrappers that indicate success, failure, and any associated error information.

## API

### `public TemplateController()`

Initializes a new instance of the `TemplateController`. The constructor sets up any required internal dependencies (e.g., a data store or service layer). No parameters are required.

### `public ApiResponse<List<ChartTemplate>> GetAllTemplates`

Retrieves all available chart templates.

- **Returns**: An `ApiResponse<List<ChartTemplate>>` containing the full list of templates on success, or an error response if the operation fails.
- **Throws**: This method does not throw exceptions; errors are captured in the returned `ApiResponse`.

### `public ApiResponse<ChartTemplate> GetTemplateById`

Retrieves a single chart template by its unique identifier.

- **Parameters**:
  - `string id` (or `int id` depending on implementation) – The identifier of the template to retrieve.
- **Returns**: An `ApiResponse<ChartTemplate>` containing the matching template on success, or an error response (e.g., `NotFound`) if no template with the given ID exists.
- **Throws**: This method does not throw exceptions; errors are captured in the returned `ApiResponse`.

### `public ApiResponse<List<ChartTemplate>> GetTemplatesByType`

Retrieves all chart templates that match a specified type.

- **Parameters**:
  - `string type` (or an enum) – The type filter to apply (e.g., `"line"`, `"bar"`, `"pie"`).
- **Returns**: An `ApiResponse<List<ChartTemplate>>` containing the filtered list of templates on success, or an error response if the type parameter is invalid or the operation fails.
- **Throws**: This method does not throw exceptions; errors are captured in the returned `ApiResponse`.

### `public async Task<ApiResponse<ChartTemplate>> CreateTemplateAsync`

Asynchronously creates a new chart template.

- **Parameters**:
  - `ChartTemplate template` – The template object to persist. Must contain valid data (e.g., a non-null name and configuration).
- **Returns**: A `Task<ApiResponse<ChartTemplate>>` that resolves to the created template (including any server-generated ID) on success, or an error response if validation fails or the operation cannot be completed.
- **Throws**: This method does not throw exceptions; errors are captured in the returned `ApiResponse`.

### `public async Task<ApiResponse<bool>> DeleteTemplateAsync`

Asynchronously deletes an existing chart template by its identifier.

- **Parameters**:
  - `string id` (or `int id`) – The identifier of the template to delete.
- **Returns**: A `Task<ApiResponse<bool>>` that resolves to `true` if the template was successfully deleted, or `false` (with an error response) if the template does not exist or deletion fails.
- **Throws**: This method does not throw exceptions; errors are captured in the returned `ApiResponse`.

## Usage

### Example 1: Retrieving and filtering templates

```csharp
var controller = new TemplateController();

// Get all templates
var allResponse = controller.GetAllTemplates();
if (allResponse.Success)
{
    Console.WriteLine($"Found {allResponse.Data.Count} templates.");
}

// Get templates by type
var barResponse = controller.GetTemplatesByType("bar");
if (barResponse.Success)
{
    foreach (var template in barResponse.Data)
    {
        Console.WriteLine($"Bar template: {template.Name}");
    }
}
```

### Example 2: Creating and deleting a template asynchronously

```csharp
var controller = new TemplateController();

// Create a new template
var newTemplate = new ChartTemplate
{
    Name = "Sales Dashboard",
    Type = "line",
    Configuration = "{ \"xAxis\": \"date\", \"yAxis\": \"revenue\" }"
};

var createResponse = await controller.CreateTemplateAsync(newTemplate);
if (createResponse.Success)
{
    Console.WriteLine($"Created template with ID: {createResponse.Data.Id}");

    // Delete the template we just created
    var deleteResponse = await controller.DeleteTemplateAsync(createResponse.Data.Id);
    Console.WriteLine(deleteResponse.Success ? "Deleted successfully." : "Deletion failed.");
}
```

## Notes

- **Error handling**: All methods return an `ApiResponse` object. Always check the `Success` property before accessing `Data`. The `ErrorMessage` property provides details when `Success` is `false`.
- **Thread safety**: The synchronous methods (`GetAllTemplates`, `GetTemplateById`, `GetTemplatesByType`) are not guaranteed to be thread-safe if the underlying data store is mutated concurrently. The asynchronous methods (`CreateTemplateAsync`, `DeleteTemplateAsync`) are safe to call from multiple threads, but concurrent creation or deletion of the same template ID may result in race conditions. Use external synchronization if needed.
- **Null/empty parameters**: Passing `null` or an empty string for identifier or type parameters may result in an `ApiResponse` with `Success = false` and an appropriate error message. The controller does not throw `ArgumentNullException`; validation is handled internally.
- **Idempotency**: `DeleteTemplateAsync` is idempotent: deleting a non-existent template returns a response with `Success = false` (typically a `NotFound` error) rather than throwing.
