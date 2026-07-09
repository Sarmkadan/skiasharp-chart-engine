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
