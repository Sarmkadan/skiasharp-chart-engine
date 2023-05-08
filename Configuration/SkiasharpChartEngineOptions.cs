using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace SkiaSharpChartEngine.Configuration;

/// <summary>
/// Configuration options for the SkiaSharp chart engine.
/// </summary>
public class SkiasharpChartEngineOptions
{
    /// <summary>
    /// Indicates whether caching is enabled. Defaults to "true".
    /// </summary>
    [Required]
    public string CacheEnabled { get; set; } = "true";

    /// <summary>
    /// The duration, in seconds, that cached chart images are retained.
    /// </summary>
    [Required]
    public int CacheDurationSeconds { get; set; } = 3600;

    /// <summary>
    /// The maximum number of chart rendering operations that can run concurrently.
    /// </summary>
    [Required]
    public int MaxConcurrentRenders { get; set; } = 10;

    /// <summary>
    /// The default width, in pixels, for generated charts.
    /// </summary>
    [Required]
    public int DefaultChartWidth { get; set; } = 800;

    /// <summary>
    /// The default height, in pixels, for generated charts.
    /// </summary>
    [Required]
    public int DefaultChartHeight { get; set; } = 600;

    /// <summary>
    /// The default background color for charts, specified as a hex string (e.g., "#FFFFFF").
    /// </summary>
    [Required]
    public string DefaultBackgroundColor { get; set; } = "#FFFFFF";

    /// <summary>
    /// Determines whether anti-aliasing is applied when rendering charts.
    /// </summary>
    [Required]
    public bool UseAntiAliasing { get; set; } = true;

    /// <summary>
    /// The maximum number of data points allowed per series before trimming or error.
    /// </summary>
    [Required]
    public int MaxDataPointsPerSeries { get; set; } = 1000;

    /// <summary>
    /// The maximum number of series that can be included in a single chart.
    /// </summary>
    [Required]
    public int MaxSeriesPerChart { get; set; } = 10;

    /// <summary>
    /// Indicates whether data validation should occur when loading chart data.
    /// </summary>
    [Required]
    public bool ValidateDataOnLoad { get; set; } = true;
}
