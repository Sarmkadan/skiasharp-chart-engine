## RenderMetricsExtensions

`RenderMetricsExtensions` provides utility methods to analyze and compare rendering performance metrics. These extensions help evaluate rendering speed and efficiency by calculating metrics such as megabytes per second and data points per second.

### Usage example

```csharp
using System;
using SkiasharpChartEngine.Models;

public class RenderMetricsDemo
{
    public static void Main(string[] args)
    {
        // Sample render metrics
        RenderMetrics metrics = new RenderMetrics
        {
            TotalBytesRendered = 1024 * 1024 * 5, // 5 MB
            TotalDataPointsRendered = 10000,
            TotalRenderTimeMilliseconds = 2000 // 2 seconds
        };

        // Calculate render speed metrics
        double mbps = RenderMetricsExtensions.GetMegabytesPerSecond(metrics);
        double dps = RenderMetricsExtensions.GetDataPointsPerSecond(metrics);

        Console.WriteLine($"Render speed: {mbps:F2} MB/s, {dps:F0} data points/s");

        // Check if the render is fast
        bool isFast = RenderMetricsExtensions.IsFastRender(mbps, dps);
        Console.WriteLine($"Is fast render: {isFast}");

        // Compare to a baseline
        RenderMetrics baseline = new RenderMetrics
        {
            TotalBytesRendered = 1024 * 1024 * 3, // 3 MB
            TotalDataPointsRendered = 5000,
            TotalRenderTimeMilliseconds = 1500 // 1.5 seconds
        };
        string comparison = RenderMetricsExtensions.CompareToBaseline(metrics, baseline);
        Console.WriteLine($"Comparison to baseline: {comparison}");

        // Detailed string representation
        string detailed = RenderMetricsExtensions.ToDetailedString(metrics);
        Console.WriteLine(detailed);
    }
}
```
