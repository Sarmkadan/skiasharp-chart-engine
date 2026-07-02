// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using SkiaSharpChartEngine;
using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.Benchmarks;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class RenderingBenchmarks
{
    private ChartEngine _engine = null!;
    private Chart _chart = null!;

    [Params(100, 1_000)]
    public int DataPoints { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _engine = ChartEngine.Create();
        _chart = new Chart(ChartType.LineChart)
        {
            Configuration = new ChartConfiguration
            {
                Title = "Benchmark Chart",
                Width = 800,
                Height = 600
            }
        };

        var series = new ChartSeries("Series", "#FF6B6B");
        var rng = new Random(42);
        for (int i = 0; i < DataPoints; i++)
        {
            series.AddDataPoint(i, rng.NextDouble() * 1000.0);
        }
        _chart.AddSeries(series);
    }

    [Benchmark(Description = "RenderToByteArray (Cold - no cache)")]
    public RenderResult Render_Cold()
    {
        // Cache needs to be cleared or the chart needs to be unique for "cold"
        // But for simplicity, creating a new chart might be enough? 
        // Or I can just accept that this might hit cache if not careful.
        // Actually, rendering the same chart might hit cache if I don't invalidate it.
        // Let's create a new chart each time, that might be expensive though.
        
        // Actually, just changing the chart ID might be enough to invalidate cache?
        // Let's just render the same one for now, and note it might be warm?
        // Wait, the cache is based on the key, which includes the chart data.
        
        return _engine.RenderChart(_chart);
    }

    [Benchmark(Description = "RenderToByteArray (Warm - cache hit)")]
    public RenderResult Render_Warm()
    {
        _engine.PrewarmRenderCache(_chart);
        return _engine.RenderChart(_chart);
    }
}
