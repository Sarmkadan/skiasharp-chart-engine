# ChartStreamingServiceTests

A test fixture that verifies the behavior of the chart streaming service responsible for registering charts, publishing data points, managing sliding windows, auto‑creating series, and flushing buffered updates. Each test method isolates a specific contract of the service and asserts the expected outcome or exception.

## API

### ChartStreamingServiceTests()
Default constructor. Creates a new instance of the test fixture. No parameters. Returns a ready‑to‑use test object. Does not throw under normal circumstances.

### void Register_NewChart_IsRegisteredSuccessfully()
Purpose: Confirms that calling the service’s register chart API with a new chart identifier results in the chart being present in the service’s internal registry.  
Parameters: None.  
Return value: None.  
Throws: No exceptions are expected; if the registration fails the test will fail due to an assertion.

### void Publish_UnregisteredChart_ThrowsInvalidOperationException()
Purpose: Verifies that attempting to publish a data point to a chart that has not been registered causes the service to throw an `InvalidOperationException`.  
Parameters: None.  
Return value: None.  
Throws: The test expects an `InvalidOperationException`; any other outcome results in a test failure.

### void Publish_ValidPoint_IsAppliedToSnapshot()
Purpose: Ensures that publishing a single valid data point to a registered chart updates the chart’s snapshot with that point.  
Parameters: None.  
Return value: None.  
Throws: No exceptions are expected; failure to update the snapshot causes the test to fail.

### void PublishBatch_MultiplePoints_AllApplied()
Purpose: Checks that publishing a batch containing multiple data points results in every point being applied to the chart’s snapshot in the order provided.  
Parameters: None.  
Return value: None.  
Throws: No exceptions are expected; missing or incorrectly ordered points cause the test to fail.

### void WindowSize_Enforced_OldestPointsDropped()
Purpose: Validates that when the number of points exceeds the configured window size, the service automatically discards the oldest points to maintain the window constraint.  
Parameters: None.  
Return value: None.  
Throws: No exceptions are expected; violation of the window size rule leads to a test failure.

### void AutoCreateSeries_WhenSeriesDoesNotExist_SeriesCreated()
Purpose: Asserts that if a data point is published for a series that does not yet exist, the service automatically creates the series before applying the point.  
Parameters: None.  
Return value: None.  
Throws: No exceptions are expected; failure to create the series causes the test to fail.

### void Unregister_PublishAfterwards_ThrowsInvalidOperationException()
Purpose: Confirms that after a chart has been unregistered, any subsequent publish operation for that chart throws an `InvalidOperationException`.  
Parameters: None.  
Return value: None.  
Throws: The test expects an `InvalidOperationException`; any other result marks the test as failed.

### async Task FlushAsync_AppliesBufferedPoints()
Purpose: Asynchronously verifies that calling the service’s flush operation applies all previously buffered points to the chart’s snapshot.  
Parameters: None.  
Return value: A `Task` representing the asynchronous operation.  
Throws: No exceptions are expected; if buffered points are not applied the test will fail. The test method must be awaited.

## Usage

```csharp
using Xunit;
using SkiasharpChartEngine.Tests;

public class ChartStreamingServiceTestsDemo
{
    [Fact]
    public void RegisterNewChartSucceeds()
    {
        // Arrange
        var test = new ChartStreamingServiceTests();

        // Act & Assert
        test.Register_NewChart_IsRegisteredSuccessfully(); // passes if registration works
    }
}
```

```csharp
using System.Threading.Tasks;
using Xunit;
using SkiasharpChartEngine.Tests;

public class ChartStreamingServiceTestsAsyncDemo
{
    [Fact]
    public async Task FlushAppliesBufferedPoints()
    {
        // Arrange
        var test = new ChartStreamingServiceTests();

        // Act
        await test.FlushAsync_AppliesBufferedPoints();

        // Assert: implicit – test method throws if flush does not apply points
    }
}
```

## Notes
- Each test method is designed to run in isolation; the fixture does not retain state between method invocations.
- The asynchronous test `FlushAsync_AppliesBufferedPoints` must be awaited; failure to do so will result in the test completing before the flush operation finishes, leading to false positives.
- Methods that assert throwing of `InvalidOperationException` depend on the pre‑condition that the chart is either unregistered or has been explicitly unregistered; calling them outside those conditions will cause the test to fail rather than throw unexpectedly.
- The test class contains no static members and is therefore thread‑safe only when each thread operates on its own instance; sharing a single instance across concurrent test executions is not supported and may produce undefined behavior.
