// SkiasharpChartEngineOptions.cs
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace SkiaSharpChartEngine.Configuration;

public class SkiasharpChartEngineOptions
{
    [Required]
    public string CacheEnabled { get; set; } = "true";

    [Required]
    public int CacheDurationSeconds { get; set; } = 3600;

    [Required]
    public int MaxConcurrentRenders { get; set; } = 10;

    [Required]
    public int DefaultChartWidth { get; set; } = 800;

    [Required]
    public int DefaultChartHeight { get; set; } = 600;

    [Required]
    public string DefaultBackgroundColor { get; set; } = "#FFFFFF";

    [Required]
    public bool UseAntiAliasing { get; set; } = true;

    [Required]
    public int MaxDataPointsPerSeries { get; set; } = 1000;

    [Required]
    public int MaxSeriesPerChart { get; set; } = 10;

    [Required]
    public bool ValidateDataOnLoad { get; set; } = true;
}
