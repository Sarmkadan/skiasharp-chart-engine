# ChartRenderingPipeline

`ChartRenderingPipeline` is the central orchestrator for multi-stage chart rendering workflows in the skiasharp-chart-engine. It allows developers to compose a sequence of processing stages and cross-cutting interceptors that operate on a shared data bag, culminating in an asynchronous execution that produces a `PipelineResult` containing success status, output, timing information, and per-stage diagnostics.

## API

### Constructors

**`public ChartRenderingPipeline()`**

Creates an empty pipeline with an initialized `Data` dictionary and `StartTime` set to the current UTC time. No stages or interceptors are registered by default.

---

### Properties

**`public Dictionary<string, object> Data`**

The shared data bag passed through every stage and interceptor. Stages read inputs from and write outputs to this dictionary. Callers can populate initial data before execution or inspect it afterward.

**`public DateTime StartTime`**

The UTC timestamp captured when the pipeline instance was constructed. Used internally to compute `TotalDurationMs` upon completion.

**`public string? ChartId`**

An optional identifier for the chart being rendered. Set this before execution to correlate results with a specific chart entity.

---

### Methods

**`public ChartRenderingPipeline AddStage(string stageName, Func<Dictionary<string, object>, Task<PipelineStageResult>> stageFunc)`**

Registers a named processing stage. Stages are executed in the order they are added.

- **Parameters:**
  - `stageName`: A unique, non-null name for the stage. Appears in `StageResults` and log output.
  - `stageFunc`: An asynchronous delegate that receives the shared `Data` dictionary and returns a `PipelineStageResult`. A `null` delegate throws `ArgumentNullException`.
- **Returns:** The same `ChartRenderingPipeline` instance for fluent chaining.
- **Throws:** `ArgumentNullException` if `stageName` or `stageFunc` is null.

**`public ChartRenderingPipeline AddInterceptor(Func<Dictionary<string, object>, Task> interceptorFunc)`**

Registers an interceptor that runs before *every* stage. Interceptors execute in the order they are added, immediately prior to each stage invocation. They receive the current `Data` dictionary but cannot short-circuit execution.

- **Parameters:**
  - `interceptorFunc`: An asynchronous delegate that receives the shared `Data` dictionary. A `null` delegate throws `ArgumentNullException`.
- **Returns:** The same `ChartRenderingPipeline` instance for fluent chaining.
- **Throws:** `ArgumentNullException` if `interceptorFunc` is null.

**`public async Task<PipelineResult> ExecuteAsync()`**

Executes the entire pipeline sequentially. For each registered stage, all interceptors are invoked in order, then the stage itself is invoked. If any stage returns `PipelineStageResult.Failure`, execution halts immediately and the pipeline result is marked as unsuccessful. If all stages complete with success, the result is marked as successful.

- **Returns:** A `PipelineResult` containing the aggregated outcome, output, duration, and per-stage details.
- **Throws:** Exceptions thrown by interceptors or stages are caught and reflected in the `PipelineResult.ErrorMessage` and `PipelineResult.Exception` properties; they do not propagate out of this method.

**`public void Set<T>(string key, T value)`**

Stores a strongly-typed value in the shared `Data` dictionary under the specified key. Overwrites any existing entry with the same key.

- **Parameters:**
  - `key`: The dictionary key. Must not be null.
  - `value`: The value to store.
- **Throws:** `ArgumentNullException` if `key` is null.

**`public T? Get<T>(string key)`**

Retrieves a value from the shared `Data` dictionary and attempts to cast it to type `T`. Returns `default(T?)` if the key does not exist or the value is not assignable to `T`.

- **Parameters:**
  - `key`: The dictionary key. Must not be null.
- **Returns:** The value cast to `T`, or `default(T?)` if missing or incompatible.
- **Throws:** `ArgumentNullException` if `key` is null.

---

### Nested Types and Static Members

**`public static PipelineStageResult Success`**

A static singleton `PipelineStageResult` instance representing a successful stage outcome. Stages should return this to indicate normal completion.

**`public static PipelineStageResult Failure`**

A static singleton `PipelineStageResult` instance representing a failed stage outcome. Returning this from any stage causes the pipeline to abort further stage execution.

**`public class PipelineResult`**

Represents the outcome of a full pipeline execution.

- **`public bool IsSuccess`** — `true` if all stages completed successfully; `false` otherwise.
- **`public string? Message`** — A human-readable summary message set by the pipeline.
- **`public object? Output`** — The final output object, typically set by the last successful stage.
- **`public bool Success`** — Alias for `IsSuccess`; maintained for backward compatibility.
- **`public string? ErrorMessage`** — Populated with the first error message encountered during execution, if any.
- **`public Exception? Exception`** — The first exception caught during execution, if any.
- **`public List<StageExecutionResult> StageResults`** — A list containing execution metadata for each stage that was attempted.
- **`public long TotalDurationMs`** — Total wall-clock time in milliseconds from pipeline construction to completion of `ExecuteAsync`.

**`public class StageExecutionResult`**

Represents the outcome of a single stage execution.

- **`public string StageName`** — The name of the stage.
- **`public bool Success`** — Whether the stage completed successfully.
- **`public string? ErrorMessage`** — Error details if the stage failed.
- **`public Exception? Exception`** — The exception caught, if the stage threw.
- **`public long TotalDurationMs`** — Duration of this stage’s execution in milliseconds, including its interceptors.

## Usage

### Example 1: Basic Two-Stage Pipeline

```csharp
var pipeline = new ChartRenderingPipeline
{
    ChartId = "chart-001"
};

pipeline.Data["rawData"] = new double[] { 1.0, 2.0, 3.0 };

pipeline
    .AddStage("Validation", async data =>
    {
        if (!data.ContainsKey("rawData"))
            return PipelineStageResult.Failure;
        data["validated"] = true;
        return PipelineStageResult.Success;
    })
    .AddStage("Render", async data =>
    {
        var values = (double[])data["rawData"];
        // Simulate rendering logic
        data["output"] = $"Rendered {values.Length} points";
        return PipelineStageResult.Success;
    });

var result = await pipeline.ExecuteAsync();

Console.WriteLine($"Success: {result.IsSuccess}");
Console.WriteLine($"Output: {result.Output}");
Console.WriteLine($"Total ms: {result.TotalDurationMs}");
```

### Example 2: Pipeline with Interceptors and Error Handling

```csharp
var pipeline = new ChartRenderingPipeline();
pipeline.Set("threshold", 100);

pipeline.AddInterceptor(async data =>
{
    // Logging interceptor
    Console.WriteLine($"Intercepted: data contains {data.Count} keys");
});

pipeline
    .AddStage("Transform", async data =>
    {
        var threshold = pipeline.Get<int>("threshold");
        if (threshold < 0)
            return PipelineStageResult.Failure;
        data["transformed"] = true;
        return PipelineStageResult.Success;
    })
    .AddStage("Export", async data =>
    {
        throw new InvalidOperationException("Export engine unavailable");
    });

var result = await pipeline.ExecuteAsync();

if (!result.IsSuccess)
{
    Console.WriteLine($"Error: {result.ErrorMessage}");
    Console.WriteLine($"Exception: {result.Exception?.Message}");

    foreach (var stage in result.StageResults)
    {
        Console.WriteLine($"Stage '{stage.StageName}': Success={stage.Success}, Duration={stage.TotalDurationMs}ms");
    }
}
```

## Notes

- **Execution order:** Interceptors always run before each stage, in the order they were added. If multiple interceptors are registered, all of them execute prior to stage `N`, then again prior to stage `N+1`.
- **Early termination:** The pipeline stops executing further stages as soon as any stage returns `PipelineStageResult.Failure`. Interceptors do not run for skipped stages.
- **Exception handling:** Exceptions thrown by interceptors or stages are captured and stored in `PipelineResult.Exception` and `PipelineResult.ErrorMessage`. They do not propagate to the caller of `ExecuteAsync`. The first exception encountered sets the error state; subsequent exceptions in the same stage are not recorded.
- **Thread safety:** `ChartRenderingPipeline` is not thread-safe. It is designed for single-threaded composition and execution. Concurrent calls to `Set`, `Get`, or `ExecuteAsync` on the same instance will produce undefined behavior. Each execution should operate on its own pipeline instance.
- **Data dictionary typing:** `Set<T>` and `Get<T>` provide compile-time type convenience but the underlying storage is `Dictionary<string, object>`. `Get<T>` performs an `is` check and returns `default` if the stored object is not assignable to `T`. No exception is thrown for type mismatches.
- **`PipelineStageResult` singletons:** `Success` and `Failure` are static instances shared across all pipelines. They carry no contextual data; stage-specific details should be placed in the `Data` dictionary before returning.
- **`StartTime` and duration:** `StartTime` is fixed at construction, not at the beginning of `ExecuteAsync`. `TotalDurationMs` therefore includes any time spent configuring the pipeline before calling `ExecuteAsync`.
