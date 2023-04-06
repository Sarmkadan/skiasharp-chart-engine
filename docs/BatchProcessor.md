# BatchProcessor

The `BatchProcessor` class provides a structured mechanism for executing high-volume tasks in discrete, manageable batches, facilitating efficient resource utilization and progress monitoring within the `skiasharp-chart-engine`. It handles the orchestration of work items, tracks successes and failures, and reports cumulative performance metrics during and after processing. This utility is designed for operations requiring controlled concurrency and robust error logging.

## API

### BatchProcessor()
Initializes a new instance of the `BatchProcessor` class.

### ProcessAsync
Executes a collection of work items asynchronously in batches.

### ProcessSingleAsync
Processes a single item asynchronously as part of the overall batch workflow.

### TotalItems (int)
Gets the total number of items scheduled for processing.

### ProcessedItems (int)
Gets the count of items that have been successfully processed.

### FailedItems (int)
Gets the count of items that encountered an error during processing.

### TotalBatches (int)
Gets the total number of batches processed during the operation.

### ElapsedMilliseconds (long)
Gets the time elapsed during the entire processing operation, measured in milliseconds.

### Errors (List<string>)
Gets a collection of error messages recorded during the processing operation.

## Usage

### Basic Batch Processing
```csharp
var processor = new BatchProcessor();
var items = new List<int> { 1, 2, 3, 4, 5 };

// Execute items in batches
await processor.ProcessAsync(items, batchSize: 2);

Console.WriteLine($"Processed {processor.ProcessedItems} items in {processor.ElapsedMilliseconds}ms.");
```

### Handling Processing Errors
```csharp
var processor = new BatchProcessor();
var items = new List<string> { "data1", "invalid", "data2" };

await processor.ProcessAsync(items, batchSize: 10);

if (processor.FailedItems > 0)
{
    Console.WriteLine("Errors encountered:");
    foreach (var error in processor.Errors)
    {
        Console.WriteLine($"- {error}");
    }
}
```

## Notes

- **Thread Safety**: This class maintains internal state regarding item counts, elapsed time, and errors. It is not inherently thread-safe for concurrent calls to its processing methods or modification of its properties from external threads during an active operation.
- **Empty Collections**: Providing an empty collection for processing results in an immediate completion with zero items processed and zero elapsed time.
- **Error Accumulation**: The `Errors` list will continue to grow as failures are encountered; users should be mindful of memory usage if processing an exceptionally large number of items with high failure rates.
- **Cancellation**: While the processing methods are asynchronous, they should be used in conjunction with standard `CancellationToken` patterns where supported to ensure timely termination of long-running operations.
