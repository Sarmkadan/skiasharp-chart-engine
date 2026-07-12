// README.md
## Configuration

The Skiasharp Chart Engine supports the following configuration settings:

* `CacheEnabled`: Enable or disable caching.
* `CacheDurationSeconds`: Cache expiration time in seconds.
* `MaxConcurrentRenders`: Maximum number of chart renders that may run concurrently.
* `DefaultChartWidth`: Default chart width.
* `DefaultChartHeight`: Default chart height.
* `DefaultBackgroundColor`: Default chart background color.
* `UseAntiAliasing`: Enable or disable anti-aliasing.
* `MaxDataPointsPerSeries`: Maximum number of data points per series.
* `MaxSeriesPerChart`: Maximum number of series per chart.
* `ValidateDataOnLoad`: Enable or disable data validation on load.

These settings can be configured in the `appsettings.json` file.

## Example Configuration

```json
{
  "SkiasharpChartEngine": {
    "CacheEnabled": "true",
    "CacheDurationSeconds": 3600,
    "MaxConcurrentRenders": 10,
    "DefaultChartWidth": 800,
    "DefaultChartHeight": 600,
    "DefaultBackgroundColor": "#FFFFFF",
    "UseAntiAliasing": true,
    "MaxDataPointsPerSeries": 1000,
    "MaxSeriesPerChart": 10,
    "ValidateDataOnLoad": true
  }
}
```

## ChartEngineExtensions

`ChartEngineExtensions` provides a set of convenience extension methods that simplify common chart‑engine workflows such as rendering a chart, saving the result to a file, creating a chart instance, and rendering directly to a memory stream. The methods are available in both synchronous and asynchronous variants, allowing you to choose the most appropriate approach for your application.

### Usage example

```csharp
using System;
using System.IO;
using System.Threading.Tasks;
using SkiasharpChartEngine.Models;   // RenderResult, Chart
using SkiasharpChartEngine.Extensions; // ChartEngineExtensions

public class ChartDemo
{
    public static async Task Main(string[] args)
    {
        // 1️⃣ Render a chart and save it to a file (async)
        RenderResult asyncResult = await ChartEngineExtensions
            .RenderAndSaveChartAsync("chart1.png", width: 800, height: 600);

        // 2️⃣ Render a chart and save it to a file (sync)
        RenderResult syncResult = ChartEngineExtensions
            .RenderAndSaveChart("chart2.png", width: 800, height: 600);

        // 3️⃣ Create a chart instance and persist it (async)
        Chart asyncChart = await ChartEngineExtensions
            .CreateAndSaveChartAsync("myChart", width: 800, height: 600);

        // 4️⃣ Create a chart instance and persist it (sync)
        Chart syncChart = ChartEngineExtensions
            .CreateAndSaveChart("myChartSync", width: 800, height: 600);

        // 5️⃣ Render a chart directly to a memory stream (async)
        await using MemoryStream asyncStream = await ChartEngineExtensions
            .RenderToStreamAsync(width: 800, height: 600);
        // e.g., send asyncStream to a client or save it later

        // 6️⃣ Render a chart directly to a memory stream (sync)
        using MemoryStream syncStream = ChartEngineExtensions
            .RenderToStream(width: 800, height: 600);
        // e.g., write syncStream to a file
        File.WriteAllBytes("chart3.png", syncStream.ToArray());

        // 7️⃣ Quick one‑off render that returns a RenderResult (async)
        RenderResult quickResult = await ChartEngineExtensions
            .QuickRenderAsync(width: 800, height: 600);

        Console.WriteLine("All chart operations completed successfully.");
    }
}
```

The example demonstrates each public member of `ChartEngineExtensions` in a realistic scenario, showing both the asynchronous and synchronous overloads. Adjust the parameters (file paths, dimensions, etc.) to fit your specific needs.