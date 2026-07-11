# ConcurrencyLimiter

`ConcurrencyLimiter` provides a mechanism to restrict the number of concurrent operations that can execute at any given time, assisting in resource management within the `skiasharp-chart-engine` by limiting the parallelism of tasks or methods.

## API

### ConcurrencyLimiter(int maxConcurrency)
Initializes a new instance with the specified maximum number of concurrent operations allowed.
*   **Parameters:** `maxConcurrency` (int) - The maximum number of slots to allow.
*   **Throws:** `ArgumentOutOfRangeException` if `maxConcurrency` is less than 1.

### ExecuteAsync\<T\>(Func\<Task\<T\>\> action)
Executes an asynchronous function within the limiter's constraints.
*   **Parameters:** `action` - The asynchronous function to execute.
*   **Returns:** A `Task` that yields the result of the `action`.

### ExecuteAsync(Func\<Task\> action)
Executes an asynchronous action within the limiter's constraints.
*   **Parameters:** `action` - The asynchronous action to execute.
*   **Returns:** A `Task` representing the asynchronous operation.

### Execute\<T\>(Func\<T\> action)
Executes a synchronous function within the limiter's constraints.
*   **Parameters:** `action` - The function to execute.
*   **Returns:** The result of the `action`.

### GetAvailableSlots()
Retrieves the number of slots currently available for execution.
*   **Returns:** (int) The number of free slots.

### GetUsedSlots()
Retrieves the number of slots currently in use.
*   **Returns:** (int) The number of occupied slots.

### WaitAsync()
Asynchronously waits until a slot becomes available.
*   **Returns:** A `Task` that completes when a slot is available.

### Dispose()
Releases all resources used by the limiter, including the underlying synchronization primitive.

## Usage

### Limiting parallel rendering tasks
```csharp
var limiter = new ConcurrencyLimiter(4);

// Limit concurrent rendering operations to 4
await limiter.ExecuteAsync(async () =>
{
    await RenderChartAsync();
});
```

### Manual slot management
```csharp
var limiter = new ConcurrencyLimiter(2);

// Manually wait for a slot to become available
await limiter.WaitAsync();

try
{
    // Perform operations while holding the slot
    PerformExpensiveCalculation();
}
finally
{
    // Ensure cleanup or state management as required
}
```

## Notes

*   **Thread Safety:** All public members of `ConcurrencyLimiter` are thread-safe and can be accessed concurrently from multiple threads.
*   **Exceptions:** If the action provided to `Execute` or `ExecuteAsync` throws an exception, that exception is propagated to the caller. The limiter will release the slot appropriately upon exception.
*   **Disposal:** The `Dispose` method should be called to properly release the underlying synchronization resources once the `ConcurrencyLimiter` is no longer required. Attempting to use the limiter after calling `Dispose` will result in an `ObjectDisposedException`.
