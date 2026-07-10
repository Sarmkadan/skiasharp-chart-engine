# ChartEngineTests

The `ChartEngineTests` class serves as the comprehensive test suite for validating the core functionality of the SkiaSharp Chart Engine. It verifies the correct instantiation of the engine under various configurations, ensures robust error handling for invalid inputs during rendering and exporting operations, and confirms the successful processing of valid charts across synchronous and asynchronous execution paths. This class acts as the primary gatekeeper for regression testing, ensuring that chart generation, data serialization, and format exportation behave predictably under both standard and edge-case conditions.

## API

### Instantiation and Configuration

*   **`public void Create_WithDefaultConfiguration_ReturnsChartEngineInstance`**
    Validates that the chart engine can be instantiated using default settings. This method asserts that a valid instance is returned without throwing exceptions when no custom configuration is provided.

*   **`public void Create_WithCustomConfiguration_ConfiguresServices`**
    Verifies that the chart engine correctly initializes internal services when provided with a custom configuration object. It ensures that dependency injection or service registration logic respects the supplied parameters.

### Synchronous Rendering

*   **`public void RenderChart_WithNullChart_ThrowsArgumentNullException`**
    Ensures that the synchronous render method explicitly throws an `ArgumentNullException` when the input chart object is null, preventing silent failures or undefined behavior.

*   **`public void RenderChart_WithEmptySeries_ThrowsInvalidChartDataException`**
    Confirms that attempting to render a chart containing no data series results in a specific `InvalidChartDataException`, distinguishing data emptiness from other null reference errors.

*   **`public void RenderChart_WithValidChart_ReturnsSuccessfulRenderResult`**
    Tests the happy path for synchronous rendering, asserting that a well-formed chart with valid data returns a successful result object indicating completion.

*   **`public void RenderChart_WithMultipleSeries_RendersSuccessfully`**
    Validates the engine's ability to handle complexity by rendering a chart containing multiple distinct data series without performance degradation or layout errors.

*   **`public void RenderChart_WithDifferentChartTypes_RendersSuccessfully`**
    Ensures polymorphic rendering logic works correctly by testing various chart types (e.g., line, bar, pie) to verify that the engine adapts its drawing logic appropriately.

*   **`public void RenderChart_WithInvalidData_HandlesErrorGracefully`**
    Verifies that the engine catches internal data anomalies (such as mismatched axis lengths or NaN values) and handles them gracefully, typically by returning an error status rather than crashing.

### Asynchronous Rendering

*   **`public async Task RenderChartAsync_WithNullChart_ThrowsArgumentNullException`**
    The asynchronous counterpart to the null check, ensuring `ArgumentNullException` is thrown immediately upon invocation if the chart argument is null.

*   **`public async Task RenderChartAsync_WithEmptySeries_ReturnsFailureResult`**
    Unlike the synchronous version which throws, this test verifies that the asynchronous pipeline handles empty series by returning a structured failure result object, allowing the caller to inspect the error without catching an exception.

*   **`public async Task RenderChartAsync_WithValidChart_ReturnsSuccessfulRenderResult`**
    Confirms that the asynchronous rendering path completes successfully for valid inputs, yielding a result equivalent to the synchronous success case.

*   **`public async Task RenderChartAsync_WithCancellation_CancelsOperation`**
    Tests the cooperative cancellation mechanism, ensuring that providing a triggered `CancellationToken` halts the rendering operation and propagates the cancellation state correctly.

### Synchronous Exporting

*   **`public void ExportChart_WithNullChart_ThrowsArgumentNullException`**
    Asserts that the export method validates the chart argument and throws `ArgumentNullException` if the source chart is missing.

*   **`public void ExportChart_WithNullOptions_ThrowsArgumentNullException`**
    Asserts that the export method validates the options argument (containing format, dimensions, etc.) and throws `ArgumentNullException` if options are not provided.

*   **`public void ExportChart_WithValidChartAndPngFormat_ReturnsSuccessfulResult`**
    Verifies the standard export workflow, ensuring that a valid chart exported to PNG format returns a successful result containing the binary data or file path.

*   **`public void ExportChart_WithSvgFormat_ReturnsSvgContent`**
    Specifically tests the vector export pipeline, confirming that requesting SVG format yields valid SVG markup content rather than raster data.

*   **`public void ExportChart_WithUnsupportedFormat_HandlesErrorGracefully`**
    Ensures that requesting an unsupported file format does not crash the application but instead returns a controlled error response or throws a specific format exception.

### Asynchronous Exporting

*   **`public async Task ExportChartAsync_WithNullChart_ThrowsArgumentNullException`**
    Validates null checking for the chart argument within the asynchronous export method.

*   **`public async Task ExportChartAsync_WithNullOptions_ThrowsArgumentNullException`**
    Validates null checking for the options argument within the asynchronous export method.

*   **`public async Task ExportChartAsync_WithValidChartAndPngFormat_ReturnsSuccessfulResult`**
    Confirms that the asynchronous export pipeline successfully generates and returns the requested PNG output for valid inputs.

## Usage

The following examples demonstrate how the behaviors verified by `ChartEngineTests` translate to consumer code patterns.

### Example 1: Robust Synchronous Rendering with Error Handling
This pattern mirrors the logic tested in `RenderChart_WithEmptySeries_ThrowsInvalidChartDataException` and `RenderChart_WithValidChart_ReturnsSuccessfulRenderResult`.

```csharp
using SkiaSharp.ChartEngine;
using SkiaSharp.ChartEngine.Exceptions;

public byte[] GenerateChartSafe(ChartDefinition chart)
{
    var engine = new ChartEngine(); // Verified by Create_WithDefaultConfiguration_ReturnsChartEngineInstance
    
    try
    {
        var result = engine.RenderChart(chart);
        
        if (!result.IsSuccess)
        {
            // Handle logical failures gracefully
            Console.WriteLine($"Render failed: {result.ErrorMessage}");
            return null;
        }

        return result.ImageData;
    }
    catch (ArgumentNullException)
    {
        // Caught per RenderChart_WithNullChart_ThrowsArgumentNullException
        throw new ArgumentException("Chart definition cannot be null.", nameof(chart));
    }
    catch (InvalidChartDataException ex)
    {
        // Caught per RenderChart_WithEmptySeries_ThrowsInvalidChartDataException
        Console.WriteLine($"Data error: {ex.Message}");
        return GeneratePlaceholderImage();
    }
}
```

### Example 2: Asynchronous Export with Cancellation Support
This pattern reflects the behavior verified in `ExportChartAsync_WithValidChartAndPngFormat_ReturnsSuccessfulResult` and `RenderChartAsync_WithCancellation_CancelsOperation`.

```csharp
using SkiaSharp.ChartEngine;
using SkiaSharp.ChartEngine.Models;

public async Task<bool> ExportChartAsync(ChartDefinition chart, string filePath, CancellationToken cancellationToken)
{
    var engine = new ChartEngine();
    var options = new ExportOptions { Format = ImageFormat.Png, Width = 800, Height = 600 };

    // Validates arguments internally as per ExportChartAsync_WithNullOptions_ThrowsArgumentNullException
    if (chart == null || options == null) 
        return false;

    try
    {
        var result = await engine.ExportChartAsync(chart, options, cancellationToken);

        if (result.IsSuccess)
        {
            await System.IO.File.WriteAllBytesAsync(filePath, result.Data, cancellationToken);
            return true;
        }
        
        return false;
    }
    catch (OperationCanceledException)
    {
        // Expected behavior when token is triggered (RenderChartAsync_WithCancellation_CancelsOperation)
        Console.WriteLine("Export operation cancelled by user.");
        return false;
    }
}
```

## Notes

*   **Exception Consistency**: There is a notable divergence in error handling strategies between synchronous and asynchronous empty-series scenarios. The synchronous `RenderChart` throws an `InvalidChartDataException` for empty series, whereas the asynchronous `RenderChartAsync` returns a failure result object. Consumers must adapt their error handling logic based on the execution context (sync vs. async).
*   **Null Argument Enforcement**: Both rendering and exporting methods, regardless of being synchronous or asynchronous, strictly enforce non-null arguments for the primary `Chart` object. The `Export` methods additionally enforce non-null `Options`. Failure to provide these results in immediate `ArgumentNullException` termination.
*   **Thread Safety**: As this is a test suite validating a rendering engine that likely wraps native SkiaSharp resources, the underlying `ChartEngine` instance should be considered stateful during operation. While the test methods themselves are isolated, sharing a single engine instance across multiple concurrent threads without external synchronization may lead to race conditions, particularly during the `Render` and `Export` phases where native memory is allocated.
*   **Format Support**: The engine explicitly distinguishes between raster (PNG) and vector (SVG) outputs. Tests confirm that unsupported formats are handled gracefully, implying that format validation occurs before heavy processing begins.
