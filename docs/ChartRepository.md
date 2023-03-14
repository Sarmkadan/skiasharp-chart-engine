# ChartRepository

ChartRepository is a data access component responsible for managing the persistence and retrieval of Chart objects within the skiasharp-chart-engine. It provides both synchronous and asynchronous methods for common operations such as querying, creating, updating, and deleting chart records, abstracting storage concerns from the core chart logic.

## API

### `public ChartRepository()`
Initializes a new instance of the ChartRepository. The constructor may establish connections to underlying storage mechanisms or initialize internal state required for data operations.

### `public async Task<Chart?> GetByIdAsync(int id)`
Retrieves a single Chart by its unique identifier. Returns `null` if no matching chart exists.  
**Parameters**:  
- `id` (int): The unique identifier of the chart to retrieve.  
**Returns**: `Task<Chart?>` – A task representing the asynchronous operation, with the result being the found Chart or null.  
**Throws**: May throw exceptions related to data access failures (e.g., database connectivity issues).

### `public Chart? GetById(int id)`
Synchronous version of GetByIdAsync. Retrieves a single Chart by its unique identifier. Returns `null` if no matching chart exists.  
**Parameters**:  
- `id` (int): The unique identifier of the chart to retrieve.  
**Returns**: `Chart?` – The found Chart or null.  
**Throws**: May throw exceptions related to data access failures.

### `public async Task<List<Chart>> GetAllAsync()`
Retrieves all Chart records from storage.  
**Returns**: `Task<List<Chart>>` – A task representing the asynchronous operation, with the result being a list of all charts.  
**Throws**: May throw exceptions related to data access failures.

### `public List<Chart> GetAll()`
Synchronous version of GetAllAsync. Retrieves all Chart records.  
**Returns**: `List<Chart>` – A list of all charts.  
**Throws**: May throw exceptions related to data access failures.

### `public async Task<List<Chart>> GetByTypeAsync(ChartType type)`
Retrieves all Chart records matching the specified type.  
**Parameters**:  
- `type` (ChartType): The type of charts to filter by.  
**Returns**: `Task<List<Chart>>` – A task representing the asynchronous operation, with the result being a list of matching charts.  
**Throws**: May throw exceptions related to data access failures.

### `public List<Chart> GetByType(ChartType type)`
Synchronous version of GetByTypeAsync. Retrieves all Chart records matching the specified type.  
**Parameters**:  
- `type` (ChartType): The type of charts to filter by.  
**Returns**: `List<Chart>` – A list of matching charts.  
**Throws**: May throw exceptions related to data access failures.

### `public async Task<string> SaveAsync(Chart chart)`
Persists a new Chart to storage and returns its generated identifier.  
**Parameters**:  
- `chart` (Chart): The chart entity to save.  
**Returns**: `Task<string>` – A task representing the asynchronous operation, with the result being the saved chart's identifier.  
**Throws**: May throw exceptions if the chart is invalid or data access fails.

### `public string Save(Chart chart)`
Synchronous version of SaveAsync. Persists a new Chart and returns its identifier.  
**Parameters**:  
- `chart` (Chart): The chart entity to save.  
**Returns**: `string` – The saved chart's identifier.  
**Throws**: May throw exceptions if the chart is invalid or data access fails.

### `public async Task<bool> UpdateAsync(Chart chart)`
Updates an existing Chart in storage. Returns `true` if the update was successful, `false` otherwise.  
**Parameters**:  
- `chart` (Chart): The chart entity with updated values.  
**Returns**: `Task<bool>` – A task representing the asynchronous operation, with the result indicating success.  
**Throws**: May throw exceptions if the chart does not exist or data access fails.

### `public bool Update(Chart chart)`
Synchronous version of UpdateAsync. Updates an existing Chart.  
**Parameters**:  
- `chart` (Chart): The chart entity with updated values.  
**Returns**: `bool` – Indicates whether the update succeeded.  
**Throws**: May throw exceptions if the chart does not exist or data access fails.

### `public async Task<bool> DeleteAsync(int id)`
Deletes a Chart by its identifier. Returns `true` if deletion succeeded, `false` otherwise.  
**Parameters**:  
- `id` (int): The unique identifier of the chart to delete.  
**Returns**: `Task<bool>` – A task representing the asynchronous operation, with the result indicating success.  
**Throws**: May throw exceptions if data access fails.

### `public bool Delete(int id)`
Synchronous version of DeleteAsync. Deletes a Chart by its identifier.  
**Parameters**:  
- `id` (int): The unique identifier of the chart to delete.  
**Returns**: `bool` – Indicates whether the deletion succeeded.  
**Throws**: May throw exceptions if data access fails.

### `public async Task<bool> ExistsAsync(int id)`
Checks whether a Chart with the specified identifier exists in storage.  
**Parameters**:  
- `id` (int): The unique identifier to check.  
**Returns**: `Task<bool>` – A task representing the asynchronous operation, with the result indicating existence.  
**Throws**: May throw exceptions related to data access failures.

### `public bool Exists(int id)`
Synchronous version of ExistsAsync. Checks for chart existence.  
**Parameters**:  
- `id` (int): The unique identifier to check.  
**Returns**: `bool` – Indicates whether the chart exists.  
**Throws**: May throw exceptions related to data access failures.

### `public async Task<int> GetCountAsync()`
Retrieves the total number of Chart records in storage.  
**Returns**: `Task<int>` – A task representing the asynchronous operation, with the result being the count.  
**Throws**: May throw exceptions related to data access failures.

### `public int GetCount()`
Synchronous version of GetCountAsync. Retrieves the total number of Chart records.  
**Returns**: `int` – The count of all charts.  
**Throws**: May throw exceptions related to data access failures.

### `public async Task<List<Chart>> SearchAsync(string searchTerm)`
Retrieves Chart records matching a search term (implementation-defined criteria).  
**Parameters**:  
- `searchTerm` (string): The term to search for within chart metadata.  
**Returns**: `Task<List<Chart>>` – A task representing the asynchronous operation, with the result being matching charts.  
**Throws**: May throw exceptions related to data access failures.

### `public List<Chart> Search(string searchTerm)`
Synchronous version of SearchAsync. Retrieves charts matching a search term.  
**Parameters**:  
- `searchTerm` (string): The term to search for within chart metadata.  
**Returns**: `List<Chart>` – A list of matching charts.  
**Throws**: May throw exceptions related to data access failures.

## Usage

```csharp
// Example 1: Retrieving a chart asynchronously
var repository = new ChartRepository();
try 
{
    Chart? chart = await repository.GetByIdAsync(42);
    if (chart != null)
    {
        Console.WriteLine($"Found chart: {chart.Title}");
    }
    else
    {
        Console.WriteLine("Chart not found.");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error retrieving chart: {ex.Message}");
}
```

```csharp
// Example 2: Saving a new chart synchronously
var newChart = new Chart 
{ 
    Title = "Sales Data", 
    Type = ChartType.Line, 
    Data = new double[] { 10, 20, 30 } 
};

var repository = new ChartRepository();
try 
{
    string chartId = repository.Save(newChart);
    Console.WriteLine($"Chart saved with ID: {chartId}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error saving chart: {ex.Message}");
}
```

## Notes

- **Thread Safety**: Synchronous methods (`GetById`, `GetAll`, etc.) are not guaranteed to be thread-safe. Concurrent access to the same repository instance should be avoided unless externally synchronized. Asynchronous methods may offer better scalability in concurrent scenarios but still require proper coordination at the application level.
- **Null Handling**: Methods returning `Chart?` (e.g., `GetById`) will return `null` when no entity matches the query. Callers must explicitly check for null to avoid NullReferenceExceptions.
- **Search Semantics**: The `Search` method's matching behavior (e.g., case sensitivity, field scope) is determined by the underlying implementation and is not specified in the interface.
- **Save vs Update**: The `Save` method is intended for new entities and returns an identifier, while `Update` modifies existing entities and returns a boolean status. Attempting to update a non-existent chart may result in failure or undefined behavior depending on implementation.
