// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using SkiaSharpChartEngine.Constants;

namespace SkiaSharpChartEngine.Configuration;

/// <summary>
/// Configuration options for the chart engine
/// </summary>
public class ChartEngineOptions
{
    public int CacheSize { get; set; } = ChartConstants.CacheSize;

    public bool EnableLogging { get; set; } = true;

    public bool EnableCaching { get; set; } = true;

    /// <summary>
    /// Alias for <see cref="EnableCaching"/> used by hosting bootstrap code.
    /// </summary>
    public bool CacheEnabled
    {
        get => EnableCaching;
        set => EnableCaching = value;
    }

    /// <summary>
    /// Cache expiration expressed in whole seconds, backed by <see cref="CacheExpirationTime"/>.
    /// </summary>
    public int CacheDurationSeconds
    {
        get => (int)CacheExpirationTime.TotalSeconds;
        set => CacheExpirationTime = TimeSpan.FromSeconds(value);
    }

    /// <summary>
    /// Maximum number of chart renders that may run concurrently.
    /// </summary>
    public int MaxConcurrentRenders { get; set; } = Environment.ProcessorCount;

    public int DefaultChartWidth { get; set; } = ChartConstants.DefaultChartWidth;

    public int DefaultChartHeight { get; set; } = ChartConstants.DefaultChartHeight;

    public string DefaultBackgroundColor { get; set; } = ChartConstants.DefaultBackgroundColor;

    public bool UseAntiAliasing { get; set; } = true;

    public int MaxDataPointsPerSeries { get; set; } = ChartConstants.MaxDataPoints;

    public int MaxSeriesPerChart { get; set; } = ChartConstants.MaxSeries;

    public TimeSpan CacheExpirationTime { get; set; } = TimeSpan.FromHours(1);

    public bool ValidateDataOnLoad { get; set; } = true;

    public Dictionary<string, object>? CustomSettings { get; set; }

    public void Validate()
    {
        if (CacheSize < 1)
            throw new InvalidOperationException("CacheSize must be at least 1");

        if (DefaultChartWidth < ChartConstants.MinimumChartWidth)
            throw new InvalidOperationException($"DefaultChartWidth must be at least {ChartConstants.MinimumChartWidth}");

        if (DefaultChartHeight < ChartConstants.MinimumChartHeight)
            throw new InvalidOperationException($"DefaultChartHeight must be at least {ChartConstants.MinimumChartHeight}");

        if (MaxDataPointsPerSeries < 10)
            throw new InvalidOperationException("MaxDataPointsPerSeries must be at least 10");

        if (MaxSeriesPerChart < 1)
            throw new InvalidOperationException("MaxSeriesPerChart must be at least 1");
    }
}
