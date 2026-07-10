# ChartEventPublisher

`ChartEventPublisher` is a lightweight event-dispatching component within the SkiaSharp.Chart.Engine project. It allows decoupled components to react to chart lifecycle events (creation, update, deletion, rendering, export) and error conditions without holding direct references to publishers or subscribers.

## API

### `public ChartEventPublisher()`

Initializes a new instance of the `ChartEventPublisher` with no subscribers.

### `public void Subscribe(IChartEventSubscriber subscriber)`

Registers a subscriber to receive chart-related events.

- **Parameters**
  - `subscriber`: An object implementing `IChartEventSubscriber` that will be notified when events occur.
- **Throws**
  - `ArgumentNullException`: if `subscriber` is `null`.

### `public void Unsubscribe(IChartEventSubscriber subscriber)`

Removes a previously registered subscriber so it no longer receives events.

- **Parameters**
  - `subscriber`: The subscriber to remove.
- **Throws**
  - `ArgumentNullException`: if `subscriber` is `null`.

### `public async Task PublishChartCreatedAsync(ChartEventArgs args)`

Notifies all subscribers that a new chart has been created.

- **Parameters**
  - `args`: Event data describing the created chart.
- **Returns**
  - A `Task` representing the asynchronous notification operation.
- **Throws**
  - `ArgumentNullException`: if `args` is `null`.

### `public async Task PublishChartUpdatedAsync(ChartEventArgs args)`

Notifies all subscribers that an existing chart has been updated.

- **Parameters**
  - `args`: Event data describing the updated chart.
- **Returns**
  - A `Task` representing the asynchronous notification operation.
- **Throws**
  - `ArgumentNullException`: if `args` is `null`.

### `public async Task PublishChartDeletedAsync(ChartEventArgs args)`

Notifies all subscribers that a chart has been deleted.

- **Parameters**
  - `args`: Event data describing the deleted chart.
- **Returns**
  - A `Task` representing the asynchronous notification operation.
- **Throws**
  - `ArgumentNullException`: if `args` is `null`.

### `public async Task PublishChartRenderedAsync(ChartEventArgs args)`

Notifies all subscribers that a chart has been rendered.

- **Parameters**
  - `args`: Event data describing the rendered chart.
- **Returns**
  - A `Task` representing the asynchronous notification operation.
- **Throws**
  - `ArgumentNullException`: if `args` is `null`.

### `public async Task PublishChartExportedAsync(ChartExportEventArgs args)`

Notifies all subscribers that a chart has been exported.

- **Parameters**
  - `args`: Event data describing the exported chart and output format.
- **Returns**
  - A `Task` representing the asynchronous notification operation.
- **Throws**
  - `ArgumentNullException`: if `args` is `null`.

### `public async Task PublishErrorAsync(ErrorEventArgs args)`

Notifies all subscribers that an error occurred during chart processing.

- **Parameters**
  - `args`: Event data describing the error and context.
- **Returns**
  - A `Task` representing the asynchronous notification operation.
- **Throws**
  - `ArgumentNullException`: if `args` is `null`.

### `public int GetSubscriberCount()`

Returns the current number of registered subscribers.

- **Returns**
  - The number of subscribers.

## Usage

```csharp
// Example 1: Logging chart lifecycle events
var publisher = new ChartEventPublisher();
var logger = new ChartEventLogger();

publisher.Subscribe(logger);

await publisher.PublishChartCreatedAsync(
    new ChartEventArgs(ChartId.New(), "Sales Report", DateTime.UtcNow));

await publisher.PublishChartUpdatedAsync(
    new ChartEventArgs(new ChartId(123), "Sales Report", DateTime.UtcNow));

await publisher.PublishChartDeletedAsync(
    new ChartEventArgs(new ChartId(123), "Sales Report", DateTime.UtcNow));

// Example 2: Conditional export handling
var exporter = new ChartExporter();
publisher.Subscribe(exporter);

await publisher.PublishChartExportedAsync(
    new ChartExportEventArgs(new ChartId(456), "Dashboard", ExportFormat.Png, "/output/dashboard.png"));
```

## Notes

- Thread safety: All public methods are safe to call concurrently from multiple threads. Internally, the publisher uses a concurrent collection to store subscribers and locks only during add/remove operations, ensuring that `Publish*` calls do not block each other.
- Subscriber lifetime: The publisher holds only weak references to subscribers. If a subscriber is garbage-collected while still registered, it will be automatically removed during the next publish cycle.
- Error handling: Exceptions thrown by individual subscribers are caught and logged (if a logger subscriber is present), but do not prevent other subscribers from receiving the event.
- Performance: Publishing is batched asynchronously; slow subscribers will not delay the caller.
