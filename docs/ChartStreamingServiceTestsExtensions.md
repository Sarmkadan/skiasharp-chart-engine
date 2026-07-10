# ChartStreamingServiceTestsExtensions

Helper class providing extension methods for executing test suites related to `ChartStreamingService` functionality. Designed to simplify test execution in scenarios where multiple test categories need to be run in sequence or isolation.

## API

### `ExecuteAllTestsAsync`
Runs all test suites (`PublishingTests`, `LifecycleTests`, and `ConfigurationTests`) asynchronously in sequence. This is the primary entry point for executing the complete test suite.

- **Parameters**: None
- **Return value**: `Task` representing the asynchronous operation.
- **Exceptions**: Throws `AggregateException` if any of the underlying test suites fail.

### `ExecutePublishingTests`
Executes the test suite focused on data publishing behavior of the `ChartStreamingService`. Validates correct handling of streaming data, backpressure, and error propagation during data transmission.

- **Parameters**: None
- **Return value**: `void`
- **Exceptions**: Throws `Exception` if any publishing-related test fails.

### `ExecuteLifecycleTests`
Runs the test suite targeting service lifecycle management. Verifies proper initialization, disposal, and state transitions of the `ChartStreamingService` across different scenarios.

- **Parameters**: None
- **Return value**: `void`
- **Exceptions**: Throws `Exception` if any lifecycle-related test fails.

### `ExecuteConfigurationTests`
Executes the test suite validating configuration handling in the `ChartStreamingService`. Ensures that service behavior aligns with provided configuration settings, including edge cases and invalid inputs.

- **Parameters**: None
- **Return value**: `void`
- **Exceptions**: Throws `Exception` if any configuration-related test fails.

## Usage

```csharp
// Example 1: Running all tests asynchronously
await ChartStreamingServiceTestsExtensions.ExecuteAllTestsAsync();

// Example 2: Running only lifecycle tests before deployment
ChartStreamingServiceTestsExtensions.ExecuteLifecycleTests();
```

## Notes

- **Thread safety**: All methods are thread-safe and may be invoked concurrently from multiple threads without additional synchronization.
- **Failure handling**: Methods throwing exceptions do so only after completing all test cases; partial failures are aggregated where applicable.
- **Order dependency**: `ExecuteAllTestsAsync` runs tests in a fixed order (`PublishingTests`, `LifecycleTests`, `ConfigurationTests`); individual methods may be called independently if specific test categories are required.
