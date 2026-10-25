# IChartRepository

`IChartRepository` defines the persistence contract for `Chart` objects in the `skiasharp-chart-engine`. It covers retrieval by id or type, saving, updating, deleting, existence checks, counting, and predicate-based search. Each operation is offered in both synchronous and asynchronous forms so that callers can choose the model that fits their context, while the storage mechanism itself stays behind the abstraction.

The engine registers `ChartRepository`, an in-memory implementation, as a singleton in `Configuration/ServiceCollectionExtensions.cs`. `ChartEngine` resolves `IChartRepository` from the service provider. To use external storage, implement this interface and register your implementation in its place.

**Namespace:** `SkiaSharpChartEngine.Repository`
**Declaration:** `public interface IChartRepository`
**Source:** `Repository/IChartRepository.cs`

## API

### Retrieval

### `Task<Chart?> GetByIdAsync(string chartId, CancellationToken cancellationToken = default)`
### `Chart? GetById(string chartId)`
Retrieves the chart with the given identifier.
- **Parameters:** `chartId` - The identifier of the chart to retrieve. `cancellationToken` - Cancels the asynchronous operation.
- **Returns:** The matching `Chart`, or `null` if no chart with that identifier exists.

### `Task<List<Chart>> GetAllAsync(CancellationToken cancellationToken = default)`
### `List<Chart> GetAll()`
Retrieves all charts held by the repository.
- **Returns:** A `List<Chart>` containing every stored chart. The list is empty if the repository holds no charts.

### `Task<List<Chart>> GetByTypeAsync(ChartType chartType, CancellationToken cancellationToken = default)`
### `List<Chart> GetByType(ChartType chartType)`
Retrieves all charts of the specified type.
- **Parameters:** `chartType` - The `ChartType` to filter by.
- **Returns:** A `List<Chart>` of matching charts, or an empty list if none match.

### `Task<List<Chart>> SearchAsync(Func<Chart, bool> predicate, CancellationToken cancellationToken = default)`
### `List<Chart> Search(Func<Chart, bool> predicate)`
Retrieves all charts that satisfy a caller-supplied condition.
- **Parameters:** `predicate` - The condition each chart is tested against.
- **Returns:** A `List<Chart>` of charts for which `predicate` returns `true`.

### Persistence

### `Task<string> SaveAsync(Chart chart, CancellationToken cancellationToken = default)`
### `string Save(Chart chart)`
Stores a chart in the repository.
- **Parameters:** `chart` - The `Chart` to store. Its `Id` is used as the storage key.
- **Returns:** The identifier of the saved chart.

### `Task<bool> UpdateAsync(Chart chart, CancellationToken cancellationToken = default)`
### `bool Update(Chart chart)`
Replaces an existing chart with the supplied version.
- **Parameters:** `chart` - The updated `Chart`. Its `Id` identifies the record to replace.
- **Returns:** `true` if the chart was found and updated; `false` if no chart with that `Id` exists.

### `Task<bool> DeleteAsync(string chartId, CancellationToken cancellationToken = default)`
### `bool Delete(string chartId)`
Removes the chart with the given identifier.
- **Parameters:** `chartId` - The identifier of the chart to remove.
- **Returns:** `true` if the chart was removed; `false` if no chart with that identifier exists.

### Inspection

### `Task<bool> ExistsAsync(string chartId, CancellationToken cancellationToken = default)`
### `bool Exists(string chartId)`
Checks whether a chart with the given identifier is stored.
- **Parameters:** `chartId` - The identifier to check.
- **Returns:** `true` if a matching chart exists; otherwise `false`.

### `Task<int> GetCountAsync(CancellationToken cancellationToken = default)`
### `int GetCount()`
Returns the number of charts in the repository.
- **Returns:** The total number of stored charts.

## Behavior Notes

- Methods with an `Async` suffix accept a `CancellationToken` and return a `Task` (or `Task<T>`). The synchronous counterparts take no token.
- Sync and async pairs are expected to perform the same operation and return the same result.
- The contract does not require any particular storage semantics. The in-memory `ChartRepository` returns copies of stored charts, so changes made to a returned `Chart` do not affect the repository until it is passed back to `Save` or `Update`. Custom implementations should document their own behavior.

## Usage

### Save, Retrieve, and Delete
```csharp
IChartRepository repository = serviceProvider.GetRequiredService<IChartRepository>();

var chart = new Chart { /* ... configuration ... */ };
string id = await repository.SaveAsync(chart);

Chart? stored = await repository.GetByIdAsync(id);

bool deleted = await repository.DeleteAsync(id);
```

### Querying Charts
```csharp
// All line charts
List<Chart> lineCharts = repository.GetByType(ChartType.LineChart);

// Charts matching a custom condition
List<Chart> recent = repository.Search(c => c.ModifiedAt > DateTime.UtcNow.AddDays(-1));
```

### Updating a Chart
```csharp
if (repository.Exists(chart.Id))
{
    chart.Title = "Updated title";
    bool updated = repository.Update(chart);
}
```

### Implementing a Custom Repository
```csharp
public class CustomChartRepository : IChartRepository
{
    // Implement every member of IChartRepository against your storage backend.
}

services.AddSingleton<IChartRepository, CustomChartRepository>();
```

## See Also

- `ChartRepository` - The default in-memory implementation.
- `Chart` - The model type stored by the repository.
- `ChartType` - The enumeration used by `GetByType`.
