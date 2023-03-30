# RenderPipelineService

The `RenderPipelineService` manages the sequential execution of rendering stages required to generate chart visualizations in the `skiasharp-chart-engine`. It provides a structured mechanism to build, execute, and inspect a pipeline, allowing developers to define a custom sequence of operations that process raw data into a renderable chart output.

## API

### RenderPipelineService

*   `public RenderPipelineService()`
    *   Initializes a new instance of the `RenderPipelineService` class.
*   `public void AddStage(IPipelineStage stage)`
    *   Adds a new stage to the end of the rendering pipeline.
    *   Parameters: `stage` (the `IPipelineStage` instance to add).
*   `public async Task<PipelineResult> ExecuteAsync()`
    *   Executes all registered pipeline stages in sequence asynchronously.
    *   Returns: A `PipelineResult` containing the final output and detailed execution telemetry.
*   `public IReadOnlyList<IPipelineStage> GetStages()`
    *   Returns the current list of registered pipeline stages.
*   `public void Clear()`
    *   Removes all registered stages from the pipeline, resetting the service state.

### PipelineResult

*   `public bool Success`
    *   Indicates whether the entire pipeline execution was successful.
*   `public string Message`
    *   A summary message regarding the pipeline execution outcome.
*   `public Chart Chart`
    *   The resulting `Chart` object if the execution was successful.
*   `public string ChartId`
    *   The identifier associated with the generated chart.
*   `public string Error`
    *   Contains error details if the pipeline execution failed.
*   `public DateTime StartedAt`
    *   The timestamp when the pipeline execution began.
*   `public DateTime CompletedAt`
    *   The timestamp when the pipeline execution finished.
*   `public long TotalElapsedMs`
    *   The total time taken for the pipeline execution in milliseconds.
*   `public List<StageResult> StageResults`
    *   A collection of results from each individual stage in the pipeline.

### StageResult

*   `public string StageName`
    *   The name of the pipeline stage.
*   `public bool Success`
    *   Indicates whether this specific stage completed successfully.
*   `public string Message`
    *   A message describing the outcome of the stage.
*   `public long ElapsedMs`
    *   The time taken for this specific stage to execute in milliseconds.

## Usage

### Example 1: Basic Pipeline Execution
```csharp
var pipeline = new RenderPipelineService();
pipeline.AddStage(new DataLoadStage());
pipeline.AddStage(new RenderStage());

PipelineResult result = await pipeline.ExecuteAsync();

if (result.Success)
{
    Console.WriteLine($"Chart {result.ChartId} rendered successfully.");
}
else
{
    Console.WriteLine($"Pipeline failed: {result.Error}");
}
```

### Example 2: Inspecting Stage Results
```csharp
var pipeline = new RenderPipelineService();
// Assume stages are added here...

PipelineResult result = await pipeline.ExecuteAsync();

foreach (var stageResult in result.StageResults)
{
    Console.WriteLine($"Stage: {stageResult.StageName}, Success: {stageResult.Success}, Duration: {stageResult.ElapsedMs}ms");
}
```

## Notes

*   **Thread Safety:** `RenderPipelineService` is not thread-safe. Instances should not be shared across threads during the registration or execution phase.
*   **State Management:** Calling `ExecuteAsync` multiple times on the same `RenderPipelineService` instance will re-run the registered stages. To change the pipeline composition, use `Clear` followed by `AddStage` calls.
*   **Execution Order:** Stages are executed strictly in the order they were added via `AddStage`. If a stage fails, subsequent stages are generally skipped, and the `PipelineResult` will reflect the failure state based on the implementation of the `IPipelineStage` interface.
