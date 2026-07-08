// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using SkiaSharpChartEngine.Caching;
using SkiaSharpChartEngine.Constants;
using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.Benchmarks;

/// <summary>
/// Benchmarks for CacheKeyBuilder hot paths.
/// Key operations include SHA-256 hashing, frozen-dictionary lookups, and composite key assembly.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class CacheKeyBenchmarks
{
    private const string ChartId = "chart-abc-123-xyz-789-mnop";
    private const string SeriesName = "Monthly Sales Performance Q3";

    [Benchmark(Description = "BuildRenderKey — hash 800×600 PNG key")]
    public string BuildRenderKey()
        => CacheKeyBuilder.BuildRenderKey(ChartId, 800, 600, 1.0f, "PNG");

    [Benchmark(Description = "BuildSeriesKey — hash chart+series name")]
    public string BuildSeriesKey()
        => CacheKeyBuilder.BuildSeriesKey(ChartId, SeriesName);

    [Benchmark(Description = "BuildAxisKey — hash min/max/ticks")]
    public string BuildAxisKey()
        => CacheKeyBuilder.BuildAxisKey(0f, 100f, 5);

    [Benchmark(Description = "BuildConfigurationKey — FrozenDictionary lookup")]
    public string BuildConfigurationKey_Line()
        => CacheKeyBuilder.BuildConfigurationKey(ChartType.LineChart);

    [Benchmark(Description = "BuildConfigurationKey — FrozenDictionary lookup (Bar)")]
    public string BuildConfigurationKey_Bar()
        => CacheKeyBuilder.BuildConfigurationKey(ChartType.BarChart);

    [Benchmark(Description = "BuildCompositeKey — 5-param loop (no LINQ)")]
    public string BuildCompositeKey()
        => CacheKeyBuilder.BuildCompositeKey(ChartId, 800, 600, "PNG", "v1.2.0");
}
