# MathHelperBenchmarks

The `MathHelperBenchmarks` class serves as a dedicated benchmark harness for evaluating the performance characteristics of statistical calculations within the `skiasharp-chart-engine` project. It facilitates direct comparison between standard enumerable-based implementations and optimized span-based implementations for common mathematical operations such as minimum/maximum detection, averaging, and standard deviation. By exposing setup routines and paired method variants, this type enables precise measurement of memory allocation overhead and execution speed differences when processing floating-point datasets of varying sizes.

## API

### `DataSize`
```csharp
public int DataSize { get; set; }
```
Gets or sets the number of elements to be generated and processed during the benchmark execution. This property determines the scale of the dataset used by the `Setup` method and subsequently by all calculation methods. Changing this value requires re-executing `Setup` to ensure internal data structures match the new size.

### `Setup`
```csharp
public void Setup()
```
Initializes the internal state required for benchmarking. This method typically generates a dataset of floating-point numbers matching the current `DataSize` and prepares any necessary memory structures (such as arrays or spans) for the subsequent test methods. It must be called before invoking any of the calculation methods to ensure valid data is available.
*   **Parameters**: None.
*   **Returns**: `void`.
*   **Throws**: May throw `ArgumentOutOfRangeException` if `DataSize` is set to a non-positive value.

### `GetMinMax_Enumerable`
```csharp
public (float Min, float Max) GetMinMax_Enumerable()
```
Calculates the minimum and maximum values from the prepared dataset using a standard `IEnumerable<float>` approach. This method serves as the baseline for performance comparison against span-based optimizations.
*   **Parameters**: None.
*   **Returns**: A tuple containing `Min` and `Max` float values.
*   **Throws**: Throws `InvalidOperationException` if `Setup` has not been called or if the internal dataset is empty.

### `GetMinMax_Span`
```csharp
public (float Min, float Max) GetMinMax_Span()
```
Calculates the minimum and maximum values from the prepared dataset using a `Span<float>` approach. This implementation avoids enumerator allocations and is expected to yield higher performance on large datasets compared to the enumerable variant.
*   **Parameters**: None.
*   **Returns**: A tuple containing `Min` and `Max` float values.
*   **Throws**: Throws `InvalidOperationException` if `Setup` has not been called or if the internal dataset is empty.

### `Average_Enumerable`
```csharp
public float Average_Enumerable()
```
Computes the arithmetic mean of the dataset using standard `IEnumerable<float>` iteration logic.
*   **Parameters**: None.
*   **Returns**: The calculated average as a `float`.
*   **Throws**: Throws `InvalidOperationException` if the dataset is empty or uninitialized.

### `Average_Span`
```csharp
public float Average_Span()
```
Computes the arithmetic mean of the dataset using `Span<float>` iteration logic to minimize overhead.
*   **Parameters**: None.
*   **Returns**: The calculated average as a `float`.
*   **Throws**: Throws `InvalidOperationException` if the dataset is empty or uninitialized.

### `StdDev_Enumerable`
```csharp
public float StdDev_Enumerable()
```
Calculates the standard deviation of the dataset using an `IEnumerable<float>` implementation. This involves two passes or stateful accumulation over the enumerable sequence.
*   **Parameters**: None.
*   **Returns**: The standard deviation as a `float`.
*   **Throws**: Throws `InvalidOperationException` if the dataset is empty or uninitialized.

### `StdDev_Span`
```csharp
public float StdDev_Span()
```
Calculates the standard deviation of the dataset using a `Span<float>` implementation, optimized for contiguous memory access.
*   **Parameters**: None.
*   **Returns**: The standard deviation as a `float`.
*   **Throws**: Throws `InvalidOperationException` if the dataset is empty or uninitialized.

## Usage

### Example 1: Basic Benchmark Initialization and Execution
This example demonstrates how to configure the benchmark with a specific dataset size, initialize the data, and execute both the baseline and optimized versions of the Min/Max calculation.

```csharp
using System;

// Initialize the benchmark harness
var benchmarks = new MathHelperBenchmarks();
benchmarks.DataSize = 10000;

// Prepare the internal dataset
benchmarks.Setup();

// Execute enumerable-based calculation (baseline)
var resultEnumerable = benchmarks.GetMinMax_Enumerable();
Console.WriteLine($"Enumerable Min: {resultEnumerable.Min}, Max: {resultEnumerable.Max}");

// Execute span-based calculation (optimized)
var resultSpan = benchmarks.GetMinMax_Span();
Console.WriteLine($"Span Min: {resultSpan.Min}, Max: {resultSpan.Max}");
```

### Example 2: Comparative Statistical Analysis
This example illustrates running a full suite of statistical comparisons (Average and Standard Deviation) to verify that both implementations produce consistent results before measuring performance.

```csharp
using System;

var benchmarks = new MathHelperBenchmarks();
benchmarks.DataSize = 50000;
benchmarks.Setup();

// Compare Average implementations
float avgEnum = benchmarks.Average_Enumerable();
float avgSpan = benchmarks.Average_Span();

if (Math.Abs(avgEnum - avgSpan) > 0.0001f)
{
    throw new Exception("Discrepancy detected in Average calculations.");
}

// Compare Standard Deviation implementations
float sdEnum = benchmarks.StdDev_Enumerable();
float sdSpan = benchmarks.StdDev_Span();

Console.WriteLine($"Standard Deviation (Enumerable): {sdEnum}");
Console.WriteLine($"Standard Deviation (Span): {sdSpan}");
```

## Notes

*   **Initialization Requirement**: All calculation methods (`GetMinMax_*`, `Average_*`, `StdDev_*`) depend on the internal state populated by `Setup()`. Invoking these methods prior to calling `Setup` or after modifying `DataSize` without re-calling `Setup` will result in an `InvalidOperationException`.
*   **Empty Dataset Handling**: If `DataSize` is set to zero, `Setup` may create an empty collection. Subsequent calls to calculation methods on an empty dataset will throw exceptions, as mathematical operations like average and standard deviation are undefined for empty sequences in this context.
*   **Thread Safety**: The `MathHelperBenchmarks` class is not thread-safe. The `Setup` method mutates internal state, and the calculation methods read from this shared state. Concurrent execution of `Setup` and any calculation method, or concurrent modifications of `DataSize`, may lead to race conditions and inconsistent results. Instances should be confined to a single thread or protected by external synchronization.
*   **Data Consistency**: The `Enumerable` and `Span` variants operate on the same underlying data generated during `Setup`. They are designed to produce numerically identical results (within floating-point precision limits); significant deviations usually indicate an implementation error rather than expected behavior.
