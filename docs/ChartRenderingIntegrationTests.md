# ChartRenderingIntegrationTests

`ChartRenderingIntegrationTests` is an integration test suite that validates the end-to-end rendering and export capabilities of the SkiaSharp chart engine. It exercises synchronous and asynchronous code paths for producing chart output in multiple formats (PNG, JPEG, SVG, WebP), writing to files and byte arrays, and verifies behaviours around caching, configuration, error handling, and parallel execution.

## API

### ChartRenderingIntegrationTests
Default constructor. Initializes a new instance of the test class. No parameters.

### void Dispose
Performs cleanup of resources allocated during test execution. Must be called once after tests complete. No parameters, no return value.

### void CanRenderSimpleLineChartToFile
Verifies that a simple line chart can be rendered and written to a file on disk. No parameters. Throws if the file cannot be created or the chart data is invalid.

### void CanRenderChartToByteArray
Verifies that a chart can be rendered into an in-memory byte array. No parameters. Throws if rendering fails or the resulting buffer is empty.

### async Task CanRenderChartAsyncToFile
Asynchronously renders a chart and writes it to a file. Returns a `Task` representing the operation. Throws if the file path is inaccessible or rendering fails asynchronously.

### void CanExportChartAsPng
Exports a chart in PNG format and verifies the output. No parameters. Throws if the PNG encoder fails or the output is not a valid PNG.

### async Task CanExportChartAsSvg
Asynchronously exports a chart as an SVG document. Returns a `Task`. Throws if SVG generation fails or the output is malformed.

### async Task CanExportChartAsJpeg
Asynchronously exports a chart as a JPEG image. Returns a `Task`. Throws if JPEG encoding fails or the output is not a valid JPEG.

### async Task CanExportChartAsWebP
Asynchronously exports a chart as a WebP image. Returns a `Task`. Throws if WebP encoding fails or the output is not a valid WebP.

### void CanRenderMultiSeriesChart
Verifies rendering of a chart containing multiple data series. No parameters. Throws if series overlap incorrectly or rendering fails.

### void CanToggleSeriesVisibility
Verifies that toggling series visibility on and off produces the expected visual changes in the rendered output. No parameters. Throws if visibility state is not reflected in the output.

### void CachingImprovesPerfomanceOnSecondRender
Asserts that a second render of the same chart configuration completes faster than the first, confirming the caching layer is effective. No parameters. Throws if the second render is not measurably faster.

### void DifferentChartsProduceDifferentCacheKeys
Verifies that charts with different configurations generate distinct cache keys, preventing incorrect cache hits. No parameters. Throws if two different charts share the same key.

### void CanApplyCustomConfiguration
Verifies that a custom chart configuration object is correctly applied and reflected in the rendered output. No parameters. Throws if the configuration is ignored or misapplied.

### void CanDisableGridAndLegend
Verifies that disabling the grid and legend produces a chart without those elements. No parameters. Throws if grid lines or legend entries appear when disabled.

### void RenderingFailsWithMissingData
Asserts that attempting to render a chart with missing or null data throws an appropriate exception. No parameters. Expects an exception to be thrown.

### void ExportFailsWithInvalidPath
Asserts that exporting a chart to an invalid file path (e.g., a non-existent directory) throws an appropriate exception. No parameters. Expects an exception to be thrown.

### async Task CanRenderMultipleChartsInParallel
Asynchronously renders multiple charts concurrently and verifies all complete successfully without interference. Returns a `Task`. Throws if any parallel render fails or produces corrupted output.

### async Task CanExportMultipleChartsInParallel
Asynchronously exports multiple charts to different formats in parallel and verifies all outputs. Returns a `Task`. Throws if any parallel export fails or produces invalid files.

### void CanRenderChartWithSingleDataPoint
Verifies that a chart containing only a single data point renders without errors and produces valid output. No parameters. Throws if the edge case of a single-point dataset causes a rendering failure.

## Usage

```csharp
// Running the full suite with standard test infrastructure
[TestClass]
public class ChartRenderingValidation
{
    [TestMethod]
    public void RunAllIntegrationTests()
    {
        using var tests = new ChartRenderingIntegrationTests();

        tests.CanRenderSimpleLineChartToFile();
        tests.CanRenderChartToByteArray();
        tests.CanExportChartAsPng();
        tests.CanRenderMultiSeriesChart();
        tests.CanToggleSeriesVisibility();
        tests.CachingImprovesPerfomanceOnSecondRender();
        tests.DifferentChartsProduceDifferentCacheKeys();
        tests.CanApplyCustomConfiguration();
        tests.CanDisableGridAndLegend();
        tests.CanRenderChartWithSingleDataPoint();
    }

    [TestMethod]
    public async Task RunAllAsyncIntegrationTests()
    {
        using var tests = new ChartRenderingIntegrationTests();

        await tests.CanRenderChartAsyncToFile();
        await tests.CanExportChartAsSvg();
        await tests.CanExportChartAsJpeg();
        await tests.CanExportChartAsWebP();
        await tests.CanRenderMultipleChartsInParallel();
        await tests.CanExportMultipleChartsInParallel();
    }
}
```

```csharp
// Targeted validation of error paths
[TestMethod]
public void ValidateErrorHandling()
{
    using var tests = new ChartRenderingIntegrationTests();

    // These are expected to throw
    Assert.ThrowsException<ChartRenderingException>(() => tests.RenderingFailsWithMissingData());
    Assert.ThrowsException<ChartExportException>(() => tests.ExportFailsWithInvalidPath());
}
```

## Notes

- **Disposal**: `Dispose` must be called after all test methods have executed to release unmanaged resources held by the rendering engine. Failure to dispose may leak file handles or memory.
- **Thread safety**: The synchronous methods are designed to run sequentially within a single test context. The parallel async methods (`CanRenderMultipleChartsInParallel`, `CanExportMultipleChartsInParallel`) internally validate that the engine supports concurrent access without corruption, but callers should not assume all members are thread-safe for arbitrary parallel invocation outside these controlled tests.
- **Edge cases**: `CanRenderChartWithSingleDataPoint` explicitly covers the boundary condition where a dataset contains exactly one point. `RenderingFailsWithMissingData` and `ExportFailsWithInvalidPath` are negative tests that expect exceptions; they should be wrapped in assertion blocks as shown in the usage examples.
- **Caching**: `CachingImprovesPerfomanceOnSecondRender` relies on a warm cache. If the test environment purges caches between calls, the performance improvement may not be observed and the test will fail. `DifferentChartsProduceDifferentCacheKeys` ensures cache isolation between distinct chart configurations.
