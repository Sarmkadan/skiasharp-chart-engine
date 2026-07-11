# HealthCheckService

The `HealthCheckService` is designed to monitor the operational status of components within the `skiasharp-chart-engine`. It provides a centralized mechanism for registering, executing, and retrieving health diagnostic information for various system parts.

## API

### HealthCheckService

*   `HealthCheckService()`: Initializes a new instance of the `HealthCheckService` class.
*   `void RegisterCheck(object check)`: Registers a new health check component with the service.
*   `async Task<HealthCheckResult> CheckHealthAsync()`: Executes all registered health checks asynchronously and aggregates the results into a `HealthCheckResult`.
*   `HealthStatus GetLastStatus()`: Returns the `HealthStatus` of the last executed health check aggregation.
*   `int GetCheckCount()`: Returns the total number of currently registered health checks.
*   `override string ToString()`: Returns a string representation of the service state.

### HealthCheckResult

Represents the outcome of an aggregate health check execution.

*   `HealthStatus Status { get; }`: Indicates the overall health status of the engine.
*   `DateTime Timestamp { get; }`: The timestamp when the result was generated.
*   `DateTime CheckedAt { get; }`: The specific time when the health check execution was performed.
*   `List<HealthCheckEntry> Entries { get; }`: A collection of individual `HealthCheckEntry` objects for each registered component.
*   `override string ToString()`: Returns a string representation of the health result.

### HealthCheckEntry

Represents the health status of a specific component.

*   `string Name { get; }`: The identifier for the component checked.
*   `HealthStatus Status { get; }`: The health status result for this specific component.
*   `string? Description { get; }`: An optional description providing details about the component's status.
*   `TimeSpan Duration { get; }`: The time taken to execute this specific check.
*   `Dictionary<string, object> Data { get; }`: Additional metadata or diagnostic data associated with this entry.

### BasicHealthCheck

A base or default implementation of a component check.

*   `string Name { get; }`: The identifier of the check.
*   `async Task<HealthCheckEntry> CheckAsync()`: Executes the logic for this specific check and returns a `HealthCheckEntry`.

## Usage

```csharp
// Example 1: Registering and executing a health check
var healthService = new HealthCheckService();
healthService.RegisterCheck(new MyCustomCheck());

var result = await healthService.CheckHealthAsync();
Console.WriteLine($"Overall Status: {result.Status}");
```

```csharp
// Example 2: Using BasicHealthCheck implementation
public class DatabaseCheck : BasicHealthCheck
{
    public override async Task<HealthCheckEntry> CheckAsync()
    {
        // Implementation logic...
        return new HealthCheckEntry("Database", HealthStatus.Healthy, "Connected", TimeSpan.FromMilliseconds(50), new Dictionary<string, object>());
    }
}
```

## Notes

*   **Thread Safety**: `CheckHealthAsync` is designed to be thread-safe; however, users should ensure that individual `RegisterCheck` operations are managed appropriately, typically during application initialization, to avoid race conditions during aggregate checks.
*   **Edge Cases**: If `CheckHealthAsync` is called when no checks are registered, it will return a `HealthCheckResult` with an empty `Entries` list and potentially a default status (typically `Healthy` or `Unknown` depending on engine configuration).
*   **Performance**: The `Duration` property in `HealthCheckEntry` should be monitored for performance degradation in heavily loaded systems.
