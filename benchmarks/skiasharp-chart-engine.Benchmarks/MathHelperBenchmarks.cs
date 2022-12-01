// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using SkiaSharpChartEngine.Utilities;

namespace SkiaSharpChartEngine.Benchmarks;

/// <summary>
/// Benchmarks for MathHelper hot paths: min/max, average, and standard deviation.
/// Compares IEnumerable-based overloads against zero-allocation ReadOnlySpan overloads.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class MathHelperBenchmarks
{
    private float[] _data = null!;

    [Params(100, 1_000, 10_000)]
    public int DataSize { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var rng = new Random(42);
        _data = new float[DataSize];
        for (int i = 0; i < DataSize; i++)
            _data[i] = (float)(rng.NextDouble() * 1000.0);
    }

    [Benchmark(Baseline = true, Description = "GetMinMax (IEnumerable — 3 passes)")]
    public (float Min, float Max) GetMinMax_Enumerable()
        => MathHelper.GetMinMax((IEnumerable<float>)_data);

    [Benchmark(Description = "GetMinMax (ReadOnlySpan — 1 pass)")]
    public (float Min, float Max) GetMinMax_Span()
        => MathHelper.GetMinMax(_data.AsSpan());

    [Benchmark(Description = "Average (IEnumerable — ToList + Sum)")]
    public float Average_Enumerable()
        => MathHelper.Average((IEnumerable<float>)_data);

    [Benchmark(Description = "Average (ReadOnlySpan — single pass)")]
    public float Average_Span()
        => MathHelper.Average(_data.AsSpan());

    [Benchmark(Description = "StandardDeviation (IEnumerable — ArrayPool buffer)")]
    public float StdDev_Enumerable()
        => MathHelper.StandardDeviation((IEnumerable<float>)_data);

    [Benchmark(Description = "StandardDeviation (ReadOnlySpan — zero alloc)")]
    public float StdDev_Span()
        => MathHelper.StandardDeviation(_data.AsSpan());
}
