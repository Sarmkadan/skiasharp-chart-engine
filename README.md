// entire file content ...

// ... (rest of the file remains the same)

## OptimizationRecommendation

`OptimizationRecommendation` represents a suggested optimization for improving chart performance. It provides information about the category of optimization, severity of the issue, and recommended actions. 

### Usage example

```csharp
using SkiasharpChartEngine.Utilities;

public class OptimizationRecommendationDemo
{
    public static void Main(string[] args)
    {
        // Create a performance optimizer instance
        var performanceOptimizer = new PerformanceOptimizer();

        // Analyze a chart
        var analysis = performanceOptimizer.AnalyzeChart(new Chart());

        // Get optimization recommendations
        var recommendations = analysis.Recommendations;

        // Iterate through recommendations
        foreach (var recommendation in recommendations)
        {
            Console.WriteLine($"Category: {recommendation.Category}, Severity: {recommendation.Severity}, Message: {recommendation.Message}, Action: {recommendation.Action}");

            // Check if downsampling is recommended
            if (recommendation.Downsample != null)
            {
                Console.WriteLine($"Downsample data points: {recommendation.Downsample.Count}");
            }

            // Estimate memory usage
            Console.WriteLine($"Estimated memory usage: {recommendation.EstimateMemoryUsage} bytes");
        }
    }
}
```

// ... (rest of the file remains the same)
