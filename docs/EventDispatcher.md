# EventDispatcher

Centralized event routing utility that allows subscribers to register for named events and dispatchers to raise them. It maintains a registry of event names to handler delegates and provides synchronous and asynchronous dispatching capabilities.

## API

### `public EventDispatcher()`

Initializes a new, empty event dispatcher with no registered handlers.

### `public void Subscribe(string eventType, Action handler)`

Registers a handler delegate for a specific event type.

- **eventType**: The name of the event to subscribe to. Must not be null or empty.
- **handler**: The delegate invoked when the event is dispatched. Must not be null.
- Throws `ArgumentNullException` if `eventType` or `handler` is null.
- Throws `ArgumentException` if `eventType` is empty.

### `public void Unsubscribe(string eventType, Action handler)`

Removes a previously registered handler for a specific event type.

- **eventType**: The name of the event to unsubscribe from. Must not be null or empty.
- **handler**: The delegate to remove. Must not be null.
- Throws `ArgumentNullException` if `eventType` or `handler` is null.
- Throws `ArgumentException` if `eventType` is empty.

### `public void Dispatch(string eventType)`

Synchronously invokes all handlers registered for the specified event type.

- **eventType**: The name of the event to raise. Must not be null or empty.
- Throws `ArgumentNullException` if `eventType` is null.
- Throws `ArgumentException` if `eventType` is empty.

### `public async Task DispatchAsync(string eventType)`

Asynchronously invokes all handlers registered for the specified event type.

- **eventType**: The name of the event to raise. Must not be null or empty.
- Returns: A `Task` representing the asynchronous operation.
- Throws `ArgumentNullException` if `eventType` is null.
- Throws `ArgumentException` if `eventType` is empty.

### `public int GetHandlerCount(string eventType)`

Returns the number of handlers currently registered for the specified event type.

- **eventType**: The name of the event to query. Must not be null or empty.
- Returns: The count of handlers for `eventType`; zero if no handlers are registered.
- Throws `ArgumentNullException` if `eventType` is null.
- Throws `ArgumentException` if `eventType` is empty.

### `public IEnumerable<string> GetSubscribedEventTypes()`

Enumerates the names of all event types that currently have at least one registered handler.

- Returns: An `IEnumerable<string>` of event type names.

### `public void Clear()`

Removes all registered handlers from the dispatcher, effectively resetting it to an empty state.
