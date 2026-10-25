# HealthCheck

The `HealthCheck` types, defined in `Diagnostics/HealthCheck.cs`, provide a lightweight way to run a set of named health checks against the `skiasharp-chart-engine` and aggregate the results into a single status. `HealthCheckService` owns the registered checks, runs them on demand, and remembers the most recent overall status. Individual checks implement `IHealthCheck`; `BasicHealthCheck` is a ready-made implementation that wraps a delegate.

## API

### HealthCheckService

#### Constructor
```csharp
public HealthCheckService(ILogger<HealthCheckService> logger)
```
Initializes a new instance of the `HealthCheckService` and logs its initialization.
*   **Parameters:**
    *   `logger`: The logger used for diagnostic output. Must not be `null`.
*   **Throws:** `ArgumentNullException` if `logger` is `null`.

#### RegisterCheck
```csharp
public void RegisterCheck(IHealthCheck check)
```
Adds a health check to the list that is executed by `CheckHealthAsync`.
*   **Parameters:**
    *   `check`: The `IHealthCheck` to register. Must not be `null`.
*   **Throws:** `ArgumentNullException` if `check` is `null`.
*   **Notes:** Checks are stored in registration order and run in that order. Registering the same check twice, or two checks with the same `Name`, is not prevented; both run.

#### CheckHealthAsync
```csharp
public async Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default)
```
Runs every registered check sequentially and aggregates the outcome.
*   **Parameters:**
    *   `cancellationToken`: Passed to each check's `CheckAsync`. Cancellation is not handled by the service itself; a check that throws `OperationCanceledException` is recorded as an unhealthy entry like any other exception.
*   **Returns:** A `HealthCheckResult` containing the overall `Status`, the `Timestamp` and `CheckedAt` values (both set to the UTC time at which the run started), and one `HealthCheckEntry` per registered check.
*   **Behavior:**
    *   If a check throws, the exception is logged at `Error` level and an entry is added with `Status = Unhealthy`, `Description` set to the exception message, and `Duration = TimeSpan.Zero`. The exception does not propagate and the remaining checks still run.
    *   The overall `Status` is `Healthy` only when every entry is `Healthy`. Any other status, including `Degraded`, makes the overall status `Unhealthy`. The service never returns `Degraded` itself.
    *   When the overall status is not `Healthy`, a warning is logged with the number of failed checks.
    *   The last overall status is stored so that `GetLastStatus` can return it.
    *   With no registered checks, the result is `Healthy` with an empty `Entries` list.

#### GetLastStatus
```csharp
public HealthStatus GetLastStatus()
```
Returns the overall status from the most recent `CheckHealthAsync` call.
*   **Returns:** `HealthStatus.Healthy` if no check run has completed yet; otherwise the status produced by the last run.

#### GetCheckCount
```csharp
public int GetCheckCount()
```
Returns the number of registered checks.
*   **Returns:** An `int` count of the checks added with `RegisterCheck`.

### IHealthCheck
Contract for a custom health check.
*   **Name** (`string`, get-only): The identifier reported in `HealthCheckEntry.Name`.
*   **CheckAsync** (`Task<HealthCheckEntry> CheckAsync(CancellationToken cancellationToken = default)`): Performs the check and returns its entry. The implementation is responsible for setting `Status`, `Description`, `Duration`, and optionally `Data`.

### BasicHealthCheck
A simple `IHealthCheck` that evaluates an optional asynchronous delegate.

#### Constructor
```csharp
public BasicHealthCheck(string name, Func<Task<bool>>? checkDelegate = null)
```
*   **Parameters:**
    *   `name`: The name reported in the check's entry.
    *   `checkDelegate`: A delegate returning `true` when healthy. When `null`, the check always reports healthy.
*   **Notes:** The constructor does not validate `name`.

#### CheckAsync
Invokes the delegate and maps the outcome to an entry.
*   **Returns:** A `HealthCheckEntry` with:
    *   `Status = Healthy` and `Description = "Check passed"` when the delegate returns `true` (or no delegate was supplied).
    *   `Status = Unhealthy` and `Description = "Check failed"` when the delegate returns `false`.
    *   `Status = Unhealthy` and `Description` set to the exception message when the delegate throws.
    *   `Duration` set to the elapsed time of the call.
*   **Notes:** The `cancellationToken` argument is not passed to the delegate. The delegate type does not accept a token, so cancellation cannot interrupt it.

### HealthCheckResult
The aggregated result of a `CheckHealthAsync` run.
*   **Status** (`HealthStatus`): The overall status.
*   **Timestamp** (`DateTime`): The UTC time at which the run started.
*   **CheckedAt** (`DateTime`): The UTC time at which the run started. Currently set to the same value as `Timestamp`.
*   **Entries** (`List<HealthCheckEntry>`): One entry per registered check, in registration order. Defaults to an empty list.
*   **ToString()**: Returns a summary such as `Status: Healthy, Healthy: 3/3`.

### HealthCheckEntry
The outcome of a single health check.
*   **Name** (`string`): The name of the check. Defaults to an empty string.
*   **Status** (`HealthStatus`): The status of this check.
*   **Description** (`string?`): A human-readable description, typically a success message or an exception message.
*   **Duration** (`TimeSpan`): How long the check took to run.
*   **Data** (`Dictionary<string, object>`): Optional extra values reported by the check. Defaults to an empty dictionary. The service does not populate it.

### HealthStatus
```csharp
public enum HealthStatus
{
    Healthy,
    Degraded,
    Unhealthy
}
```
*   **Healthy**: The check or system is operating normally.
*   **Degraded**: Reserved for checks that want to signal partial functionality. A `Degraded` entry still makes the overall result `Unhealthy`, as described under `CheckHealthAsync`.
*   **Unhealthy**: The check failed or threw an exception.

## Usage

### Running Built-in Checks
```csharp
var logger = loggerFactory.CreateLogger<HealthCheckService>();
var health = new HealthCheckService(logger);

health.RegisterCheck(new BasicHealthCheck("Renderer", async () =>
{
    // Return true when the renderer responds
    return await rendererProbeAsync();
}));

health.RegisterCheck(new BasicHealthCheck("Cache"));

HealthCheckResult result = await health.CheckHealthAsync();
Console.WriteLine(result);                          // Status: Healthy, Healthy: 2/2
Console.WriteLine(health.GetLastStatus());          // Healthy
```

### Implementing a Custom Check
```csharp
public sealed class ExportDirectoryHealthCheck : IHealthCheck
{
    private readonly string _path;

    public ExportDirectoryHealthCheck(string path) => _path = path;

    public string Name => "ExportDirectory";

    public Task<HealthCheckEntry> CheckAsync(CancellationToken cancellationToken = default)
    {
        var start = DateTime.UtcNow;
        bool writable = Directory.Exists(_path);

        return Task.FromResult(new HealthCheckEntry
        {
            Name = Name,
            Status = writable ? HealthStatus.Healthy : HealthStatus.Unhealthy,
            Description = writable ? "Directory exists" : "Directory is missing",
            Duration = DateTime.UtcNow - start,
            Data = new Dictionary<string, object> { ["Path"] = _path }
        });
    }
}
```

### Interpreting Results
```csharp
foreach (var entry in result.Entries.Where(e => e.Status != HealthStatus.Healthy))
{
    Console.WriteLine($"{entry.Name}: {entry.Status} - {entry.Description}");
}
```

## Integration

In the Web API host, `Program.cs` registers `HealthCheckService` as a singleton (`AddSingleton<HealthCheckService>()`), and `HealthController` in `API/Controllers` takes it through constructor injection. Those files are in the `API/` folder, which the project's `.csproj` excludes from the build, so the HTTP endpoint is not compiled by default. Checks are registered by application code, because the service starts with no checks.

## Thread Safety

`HealthCheckService` stores checks in a plain `List<IHealthCheck>` and does not synchronize access. Register all checks during startup, before any concurrent calls to `CheckHealthAsync`. Concurrent runs are not guarded, and `_lastStatus` is written without synchronization, so `GetLastStatus` may briefly return a value from a different run.
