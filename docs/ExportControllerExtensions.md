# ExportControllerExtensions

`ExportControllerExtensions` is a static class in `API.Controllers` that adds helper methods to `ExportController`. It exposes three read-only accessors for batch render statistics. Each method checks its argument for null, then reads a property from the controller. The class does not render or export anything itself.

## API

* `public static bool HasFailedRenders(this ExportController controller)`: Returns `true` when the controller reports at least one failed render, that is, when `FailedRenders > 0`. Throws `ArgumentNullException` if `controller` is null.
* `public static int GetTotalChartsRendered(this ExportController controller)`: Returns the total number of charts the controller reports as rendered (`TotalCharts`). Throws `ArgumentNullException` if `controller` is null.
* `public static IReadOnlyList<IndividualRenderResult> GetRenderResults(this ExportController controller)`: Returns a read-only view of the per-chart render results (`RenderResults.AsReadOnly()`). The view wraps the controller's list, so it reflects later changes to that list. Throws `ArgumentNullException` if `controller` is null.

## Usage

The methods are called on an existing `ExportController` instance:

```csharp
// Requires an ExportController instance, e.g. resolved from DI or created in a test.
ExportController controller = /* ... */;

// Check whether any render in the last batch failed
bool anyFailed = controller.HasFailedRenders();

// Total number of charts reported by the controller
int total = controller.GetTotalChartsRendered();

// Inspect the individual results
foreach (IndividualRenderResult result in controller.GetRenderResults())
{
    Console.WriteLine($"{result.ChartId}: {(result.Success ? "ok" : result.Message)}");
}
```

## Notes

* **Compile status: the file does not compile if included.** `ExportController` (in `API/Controllers/ExportController.cs`) does not declare `FailedRenders`, `TotalCharts` or `RenderResults`. Those properties exist on `BatchExportResult`, which is declared in the same file. The extension methods therefore refer to members that `ExportController` lacks. This is a code defect, not a documentation error. The `.cs` file was not changed as part of this documentation task.
* The `API/` folder is excluded from the build by the `<Compile Remove="API/**/*.cs" />` entry in `skiasharp-chart-engine.csproj`. The file is therefore not compiled by `dotnet build`, which is why the mismatch above does not fail the default build. The descriptions above come from reading the source and have not been verified by compiling this file.
* `IndividualRenderResult` has the properties `ChartId`, `Success`, `Message` and `FileSize`. The example above uses `ChartId`, `Success` and `Message`.
* The methods do not catch exceptions. Any exception raised while reading the controller's members propagates to the caller.
* The methods do not return or modify any render state. They have no side effects.
