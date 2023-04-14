# ChartRenderingPipelineExtensions

A set of static extension methods that facilitate building, configuring, and executing a `ChartRenderingPipeline`. The methods enable fluent addition of processing stages and interceptors, as well as asynchronous execution that returns either the successful stage results or the result of a particular stage.

## API

### AddStages
Adds one or more processing stages to the chart rendering pipeline.

- **Parameters**
  - `pipeline`: The `ChartRenderingPipeline` instance to extend.
  - `stages`: A variable‑length list of `IStage` objects to be added.
- **Return value**: The same `ChartRenderingPipeline` instance, allowing further chaining.
- **Exceptions**: 
  - `ArgumentNullException` if `pipeline` or any element in `stages` is `null`.

### AddInterceptors
Adds one or more interceptors that can observe or modify stage execution.

- **Parameters**
  - `pipeline`: The `ChartRenderingPipeline` instance to extend.
  - `interceptors`: A variable‑length list of `IInterceptor` objects to be added.
- **Return value**: The same `ChartRenderingPipeline` instance, allowing further chaining.
- **Exceptions**: 
  - `ArgumentNullException` if `pipeline` or any element in `interceptors` is `null`.

### ExecuteAndGetSuccessfulStagesAsync
Asynchronously runs the pipeline and returns the results of all stages that completed successfully.

- **Parameters**
  - `pipeline`: The `ChartRenderingPipeline` to execute.
  - `cancellationToken` (optional): A `System.Threading.CancellationToken` to observe cancellation requests.
- **Return value**: A `Task<IReadOnlyList<StageExecutionResult>>` containing the execution outcome for each stage that succeeded; stages that threw or were cancelled are omitted from the list.
- **Exceptions**: 
  - `OperationCanceledException` if the token is triggered.
  - `InvalidOperationException` if the pipeline contains no stages.
  - Any exception thrown by a stage propagates out of the method.

### ExecuteAndGetStageResultAsync
Asynchronously runs the pipeline and returns the result of the first stage that yields a non‑null output, or `null` if no stage produces a result.

- **Parameters**
  - `pipeline`: The `ChartRenderingPipeline` to execute.
  - `cancellationToken` (optional): A `System.Threading.CancellationToken` to observe cancellation requests.
- **Return value**: A `Task<StageExecutionResult?>` where the value is the execution outcome of the selected stage, or `null` when no stage yields a result.
- **Exceptions**: 
  - `OperationCanceledException` if the token is triggered.
  - `InvalidOperationException` if the pipeline is not configured with any stages.
  - Any exception thrown by a stage propagates out of the method.

## Usage

### Example 1: Building a pipeline and collecting successful stage results
```csharp
using SkiaSharp.ChartEngine;
using SkiaSharp.ChartEngine.Pipelines;

var pipeline = new ChartRenderingPipeline()
    .AddStages(new DataLoadStage(), new AggregationStage(), new RenderStage())
    .AddInterceptors(new LoggingInterceptor(), new TimingInterceptor());

var successfulResults = await pipeline.ExecuteAndGetSuccessfulStagesAsync();
foreach (var result in successfulResults)
{
    Console.WriteLine($"Stage '{result.StageName}' succeeded in {result.Duration}");
}
```

### Example 2: Retrieving the result of the first productive stage
```csharp
using SkiaSharp.ChartEngine;
using SkiaSharp.ChartEngine.Pipelines;

var pipeline = new ChartRenderingPipeline()
    .AddStages(new ParseStage(), TransformStage.Instance, new RenderStage());

var stageResult = await pipeline.ExecuteAndGetStageResultAsync();
if (stageResult.HasValue)
{
    var res = stageResult.Value;
    Console.WriteLine($"Stage '{res.StageName}' produced output: {res.Output}");
}
else
{
    Console.WriteLine("No stage produced a result.");
}
```

## Notes

- The `AddStages` and `AddInterceptors` methods mutate the supplied `ChartRenderingPipeline` instance by appending the provided stages or interceptors. They return the same instance to enable fluent chaining.
- Execution methods are safe to invoke from multiple threads concurrently **as long as the pipeline instance is not modified while execution is in progress**. Adding stages or interceptors after an execution has begun leads to undefined behavior.
- If a stage throws an exception, `ExecuteAndGetSuccessfulStagesAsync` excludes that stage from the returned list but does not suppress the exception; it bubbles up to the caller.
- `ExecuteAndGetStageResultAsync` returns `null` when every stage either fails or yields a null output. Callers should check for `null` before accessing the value.
- Cancellation tokens are respected; if cancellation is requested, the methods throw `OperationCanceledException` and no partial results are returned.
- The methods do not capture a `SynchronizationContext`; continuations run on thread‑pool threads unless the caller provides a custom scheduler via the token.
