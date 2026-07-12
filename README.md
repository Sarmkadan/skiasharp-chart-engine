
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

## RateLimitingMiddlewareExtensions

`RateLimitingMiddlewareExtensions` provides utility methods to configure and manage rate limiting in ASP.NET Core applications. It allows you to enable or disable rate limiting, set up logging, and reset client rate limit information.

### Usage example

```csharp
using Microsoft.AspNetCore.Builder;
using SkiasharpChartEngine.Middleware;

public class RateLimitingDemo
{
    public static void Main(string[] args)
    {
        // Create a new instance of RateLimitingMiddleware
        var rateLimitingMiddleware = RateLimitingMiddlewareExtensions.WithConsoleLogger();

        // Check if rate limiting is enabled
        bool isEnabled = rateLimitingMiddleware.IsEnabled;

        // Log a message
        rateLimitingMiddleware.Log(new { Message = "Rate limiting log message" });

        // Begin a scope
        using (var scope = rateLimitingMiddleware.BeginScope(new { }))
        {
            // ...
        }

        // Reset client rate limit information
        rateLimitingMiddleware.ResetClient("clientId");

        // Check rate limit
        bool isWithinLimit = rateLimitingMiddleware.CheckRateLimit("clientId");

        // Get current rate limit information
        RateLimitInfo? rateLimitInfo = RateLimitingMiddlewareExtensions.GetCurrentRateLimitInfo();
    }
}
```

## PerformanceMonitoringMiddlewareExtensions

`PerformanceMonitoringMiddlewareExtensions` provides utility methods to monitor and analyze performance statistics for operations within an application. This extension enables tracking performance metrics such as execution time and success rates.

### Usage example

```csharp
using System;
using SkiasharpChartEngine.Middleware;

public class PerformanceMonitoringDemo
{
    public static void Main(string[] args)
    {
        // Start monitoring performance
        using (var context = PerformanceMonitoringMiddlewareExtensions.StartMonitoring())
        {
            try
            {
                // Simulate some operation
                System.Threading.Thread.Sleep(100);
            }
            catch (Exception ex)
            {
                // Handle exception
            }
            finally
            {
                // End monitoring and get performance statistics
                var statistics = PerformanceMonitoringMiddlewareExtensions.EndMonitoring(context);

                // Check if the operation was slow
                bool wasSlow = PerformanceMonitoringMiddlewareExtensions.WasSlowOperation(statistics);

                // Get performance report
                string report = PerformanceMonitoringMiddlewareExtensions.GetPerformanceReport(statistics);
                Console.WriteLine(report);

                // Get all operation statistics
                var allStatistics = PerformanceMonitoringMiddlewareExtensions.GetAllOperationStatistics();
                Console.WriteLine($"Operations: {string.Join(", ", allStatistics.Keys)}");

                // Check if the operation was successful
                bool isSuccessful = PerformanceMonitoringMiddlewareExtensions.IsOperationSuccessful(statistics);

                // Get the slowest operations
                var slowestOperations = PerformanceMonitoringMiddlewareExtensions.GetSlowestOperations();
                Console.WriteLine($"Slowest operations: {string.Join(", ", slowestOperations.Select(s => s.OperationName))}");
            }
        }
    }
}
