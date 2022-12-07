# Frequently Asked Questions

## Installation & Setup

### Q: How do I install the SkiaSharp Chart Engine?

**A:** Use NuGet:
```bash
dotnet add package SkiaSharpChartEngine
```

Or in Visual Studio Package Manager:
```
Install-Package SkiaSharpChartEngine
```

### Q: What .NET versions are supported?

**A:** The library targets .NET 10.0. It may work on earlier versions, but 10.0+ is required for full feature support.

### Q: Do I need to install SkiaSharp separately?

**A:** No, SkiaSharp (v2.88.8+) is included as a dependency and installed automatically.

### Q: Can I use this in a class library?

**A:** Yes, the library is a standard .NET 10.0 library and can be referenced from any .NET 10.0+ project.

---

## Usage & API

### Q: How do I create a simple chart?

**A:** Here's the minimal code:
```csharp
var engine = ChartEngine.Create();
var chart = new Chart(ChartType.LineChart);
var series = new ChartSeries("Data", "#FF6B6B");
series.AddDataPoint(1, 10);
series.AddDataPoint(2, 20);
chart.AddSeries(series);
var result = engine.RenderChart(chart);
```

### Q: What chart types are supported?

**A:** 
- LineChart (trends)
- BarChart (horizontal bars)
- ColumnChart (vertical bars)
- PieChart (composition)
- HeatmapChart (2D patterns)
- AreaChart (filled trends)
- ScatterChart (correlations)

### Q: Can I customize colors?

**A:** Yes, use hex color codes:
```csharp
var series = new ChartSeries("Data", "#FF6B6B");  // Bright red
var config = new ChartConfiguration
{
    BackgroundColor = "#FFFFFF",  // White
    AxisColor = "#333333",        // Dark gray
    GridColor = "#CCCCCC"         // Light gray
};
```

### Q: How do I export to different formats?

**A:** Use ExportFormat enum:
```csharp
var formats = new[] { ExportFormat.PNG, ExportFormat.SVG, ExportFormat.PDF };
foreach (var format in formats)
{
    var options = new ExportOptions("chart", format, "output/");
    engine.ExportChart(chart, options);
}
```

### Q: Can I use async/await?

**A:** Yes, all operations have async variants:
```csharp
var result = await engine.RenderChartAsync(chart);
var exportResult = await engine.ExportChartAsync(chart, options);
var saved = await engine.SaveChartAsync(chart);
```

### Q: Is it thread-safe?

**A:** Yes, all services are thread-safe. You can call methods from multiple threads concurrently.

---

## Performance

### Q: How fast does it render charts?

**A:** Typical rendering times:
- Simple chart (1 series, 10 points): ~10ms
- Medium chart (3 series, 100 points): ~50ms
- Complex chart (5 series, 500 points): ~200ms
- Cached chart: <1ms

### Q: How can I improve rendering performance?

**A:** 
1. Enable caching: `engine.PrewarmRenderCache(chart)`
2. Reduce data point count
3. Simplify chart configuration
4. Reuse ChartEngine instance
5. Use cached exports when possible

### Q: Does the library use GPU acceleration?

**A:** Currently, no. It uses CPU-based rendering via SkiaSharp. GPU support may be added in a future version.

### Q: How much memory does it use?

**A:** Approximately:
- Per render: 50MB (800x600 chart)
- Per cached chart: 2-5MB
- Configuration: <1KB

### Q: Can I render very large charts (4000x3000)?

**A:** Yes, but expect slower rendering:
```csharp
var config = new ChartConfiguration
{
    Width = 4000,
    Height = 3000
};
var chart = new Chart(config);  // Will render but take longer
```

---

## Caching & Storage

### Q: How does caching work?

**A:** Charts are cached automatically after rendering. The cache is invalidated when:
- Chart data changes (`AddSeries`, `ModifiedAt` updated)
- Configurable TTL expires (default: 5 minutes)

### Q: How do I clear the cache?

**A:** Cache is cleared automatically, but you can also:
```csharp
var cacheService = engine.GetServiceProvider()
    .GetRequiredService<IRenderCacheService>();
cacheService.Clear();
```

### Q: Can I use external storage instead of in-memory?

**A:** Currently, the repository is in-memory. You can implement a custom `IChartRepository` for external storage:
```csharp
public class CustomChartRepository : IChartRepository
{
    // Implement methods to use your storage
}
```

### Q: Does the library persist charts to disk?

**A:** The in-memory repository does not persist to disk. You must manually save charts:
```csharp
var json = JsonConvert.SerializeObject(chart);
File.WriteAllText("chart.json", json);
```

---

## Data & Validation

### Q: What are the data point limits?

**A:** 
- Max series per chart: 100
- Max data points per series: 500
- Max total data points: 50,000

### Q: What happens if I add invalid data?

**A:** The library validates before rendering:
```csharp
try 
{
    if (chart.ValidateForRendering())
        var result = engine.RenderChart(chart);
}
catch (InvalidChartDataException ex)
{
    Console.WriteLine(ex.Message);
}
```

### Q: Can I use zero or negative values?

**A:** Yes, the library handles any finite numbers. Inf/NaN are invalid.

### Q: How do I format axis labels?

**A:** Configure via `ChartConfiguration`:
```csharp
config.XAxisLabel = "Time (seconds)";
config.YAxisLabel = "Temperature (°C)";
```

### Q: Can I use custom data types?

**A:** DataPoints use double for X/Y. Convert your data:
```csharp
var myData = new MyDataClass { Timestamp = DateTime.Now, Value = 42 };
series.AddDataPoint(myData.Timestamp.Ticks, myData.Value);
```

---

## Deployment

### Q: Can I use this in ASP.NET Core?

**A:** Yes, it's built for ASP.NET Core:
```csharp
services.AddSkiaSharpChartEngine();
services.AddControllers();

app.MapPost("/api/charts/render", async (ChartRequest req, ChartEngine engine) =>
{
    var result = await engine.RenderChartAsync(req.Chart);
    return result.IsSuccessful 
        ? Results.File(result.Data as byte[], "image/png")
        : Results.BadRequest(result.ErrorMessage);
});
```

### Q: How do I deploy to Docker?

**A:** Use the provided Dockerfile:
```bash
docker build -t chart-engine:latest .
docker run -p 5000:5000 chart-engine:latest
```

### Q: Can I use this on Linux?

**A:** Yes, it's cross-platform. SkiaSharp has Linux support.

### Q: What about Kubernetes?

**A:** Yes, it's container-native. See the deployment guide for Kubernetes manifests.

### Q: Can I run multiple instances behind a load balancer?

**A:** Yes, each instance is stateless (except for in-memory repository). Use sticky sessions if needed for repository access.

---

## Error Handling

### Q: How do I handle errors?

**A:** All operations return `RenderResult`:
```csharp
var result = engine.RenderChart(chart);
if (!result.IsSuccessful)
{
    Console.WriteLine($"Error: {result.ErrorMessage}");
    Console.WriteLine($"Details: {result.Exception?.Message}");
}
```

### Q: What exceptions can be thrown?

**A:** Main exceptions:
- `ArgumentNullException` - Null parameter
- `InvalidChartDataException` - Invalid chart data
- `InvalidOperationException` - Operation not allowed
- `IOException` - File I/O issues

### Q: How do I debug rendering issues?

**A:**
1. Check `RenderResult.IsSuccessful`
2. Review `RenderResult.ErrorMessage`
3. Validate chart: `chart.ValidateForRendering()`
4. Check logs: Use `ILogger<ChartEngine>`

### Q: What if rendering times out?

**A:** Use `CancellationToken`:
```csharp
var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
try
{
    var result = await engine.RenderChartAsync(chart, cts.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine("Render timed out");
}
```

---

## Advanced Topics

### Q: How do I implement custom dependency injection?

**A:** Create your own container:
```csharp
var services = new ServiceCollection();
services.AddLogging();
services.AddSkiaSharpChartEngine(options => 
{
    options.CacheEnabled = true;
    options.MaxConcurrentRenders = 20;
});
// Add your own services
services.AddSingleton<MyService>();

var provider = services.BuildServiceProvider();
var engine = new ChartEngine(provider);
```

### Q: Can I extend the rendering pipeline?

**A:** The library uses `ChartRenderingPipeline`. You can:
1. Create a custom renderer implementing `IChartRenderer`
2. Register it in DI
3. Override the default implementation

### Q: How do I monitor rendering metrics?

**A:** Use `MetricsCollector`:
```csharp
var metrics = engine.GetServiceProvider()
    .GetRequiredService<MetricsCollector>();
var stats = metrics.GetRenderStatistics();
Console.WriteLine($"Total renders: {stats.TotalRenders}");
Console.WriteLine($"Average time: {stats.AverageRenderTime}ms");
```

### Q: Can I batch render multiple charts?

**A:** Yes, using concurrent rendering:
```csharp
var charts = new[] { chart1, chart2, chart3 };
var tasks = charts.Select(c => engine.RenderChartAsync(c));
var results = await Task.WhenAll(tasks);
```

---

## Troubleshooting

### Q: My chart isn't rendering - what do I check?

**A:** 1. Verify `RenderResult.IsSuccessful` is true
2. Check `RenderResult.ErrorMessage`
3. Ensure chart has series: `chart.Series.Count > 0`
4. Ensure series have data: `series.DataPoints.Count > 0`
5. Call `chart.ValidateForRendering()`

### Q: Colors look wrong - how do I fix it?

**A:** Use hex color codes (not color names):
```csharp
// ✓ Correct
new ChartSeries("Data", "#FF6B6B");

// ✗ Wrong
new ChartSeries("Data", "red");
```

### Q: Memory is growing - is there a leak?

**A:** Check:
1. Dispose SkiaSharp resources in advanced scenarios
2. Monitor cache size
3. Review `RenderResult` lifetime
4. Check for circular references in chart models

### Q: Export file is empty or corrupted

**A:** Ensure:
1. `ExportOptions.GetFullPath()` is writable
2. Output directory exists: `Directory.CreateDirectory(outputPath)`
3. Chart is valid: `chart.ValidateForRendering()`
4. Disk has sufficient space

### Q: Getting OutOfMemoryException

**A:** Solutions:
1. Reduce chart size (Width/Height)
2. Reduce data point count
3. Limit concurrent renders
4. Reduce cache size
5. Increase available memory

---

## Contributing & Support

### Q: How can I contribute?

**A:** See [Contributing Guidelines](../CONTRIBUTING.md):
1. Fork the repository
2. Create feature branch
3. Submit pull request
4. Follow code style guidelines

### Q: Where do I report bugs?

**A:** [GitHub Issues](https://github.com/vladyslav-zaiets/skiasharp-chart-engine/issues)

### Q: Is there a community forum?

**A:** Not yet, but you can discuss in GitHub Discussions.

### Q: How often is it updated?

**A:** Regular updates are released as bugs are fixed and features added. Follow the [CHANGELOG](../CHANGELOG.md).

---

## Roadmap

### Planned Features
- Custom chart types
- Animation support
- Distributed caching (Redis)
- GPU acceleration
- Real-time data binding
- Web components (Blazor)

### Known Limitations
- No animation support (v1.x)
- Single-threaded rendering per chart
- In-memory storage only
- Limited to SkiaSharp supported formats

---

Have a question not listed here? [Open a GitHub issue](https://github.com/vladyslav-zaiets/skiasharp-chart-engine/issues)!
