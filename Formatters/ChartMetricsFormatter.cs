// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.Formatters;

/// <summary>
/// Formats chart metrics and statistics for display and reporting
/// Converts internal metrics to human-readable formats
/// </summary>
public class ChartMetricsFormatter
{
    private readonly ILogger<ChartMetricsFormatter> _logger;

    public ChartMetricsFormatter(ILogger<ChartMetricsFormatter> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Formats render metrics as a readable string
    /// </summary>
    public string FormatRenderMetrics(RenderMetrics metrics)
    {
        if (metrics == null)
            throw new ArgumentNullException(nameof(metrics));

        var sb = new StringBuilder();

        sb.AppendLine("=== Render Metrics ===");
        sb.AppendLine($"Chart ID: {metrics.ChartId}");
        sb.AppendLine($"Render Time: {metrics.RenderTimeMs}ms");
        sb.AppendLine($"Memory Used: {FormatBytes(metrics.MemoryUsedBytes)}");
        sb.AppendLine($"Output Size: {FormatBytes(metrics.OutputSizeBytes)}");
        sb.AppendLine($"Data Points: {metrics.DataPointCount}");
        sb.AppendLine($"Series Count: {metrics.SeriesCount}");
        sb.AppendLine($"Timestamp: {metrics.Timestamp:O}");

        return sb.ToString();
    }

    /// <summary>
    /// Formats render metrics as JSON
    /// </summary>
    public string FormatRenderMetricsAsJson(RenderMetrics metrics)
    {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            return JsonSerializer.Serialize(metrics, options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error formatting metrics as JSON");
            return "{\"error\": \"Failed to serialize metrics\"}";
        }
    }

    /// <summary>
    /// Formats chart configuration for display
    /// </summary>
    public string FormatChartConfiguration(ChartConfiguration config)
    {
        if (config == null)
            return "No configuration";

        var sb = new StringBuilder();

        sb.AppendLine("=== Chart Configuration ===");
        sb.AppendLine($"Width: {config.Width}px");
        sb.AppendLine($"Height: {config.Height}px");
        sb.AppendLine($"Theme: {config.Theme}");
        sb.AppendLine($"Show Legend: {config.ShowLegend}");
        sb.AppendLine($"Show Grid: {config.ShowGrid}");
        sb.AppendLine($"Title: {config.Title}");

        if (config.ColorPalette != null)
        {
            sb.AppendLine($"Color Palette: {config.ColorPalette.Name}");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Formats chart summary statistics
    /// </summary>
    public string FormatChartSummary(Chart chart)
    {
        if (chart == null)
            throw new ArgumentNullException(nameof(chart));

        var sb = new StringBuilder();
        var totalDataPoints = 0;
        var totalValue = 0f;
        var minValue = float.MaxValue;
        var maxValue = float.MinValue;

        if (chart.Series != null)
        {
            foreach (var series in chart.Series)
            {
                if (series.DataPoints != null)
                {
                    totalDataPoints += series.DataPoints.Count;

                    foreach (var point in series.DataPoints)
                    {
                        if (!float.IsNaN(point.Value) && !float.IsInfinity(point.Value))
                        {
                            totalValue += point.Value;
                            minValue = Math.Min(minValue, point.Value);
                            maxValue = Math.Max(maxValue, point.Value);
                        }
                    }
                }
            }
        }

        sb.AppendLine("=== Chart Summary ===");
        sb.AppendLine($"Chart ID: {chart.Id}");
        sb.AppendLine($"Title: {chart.Title}");
        sb.AppendLine($"Type: {chart.ChartType}");
        sb.AppendLine($"Series Count: {chart.Series?.Count ?? 0}");
        sb.AppendLine($"Total Data Points: {totalDataPoints}");

        if (totalDataPoints > 0)
        {
            sb.AppendLine($"Average Value: {(totalValue / totalDataPoints):F2}");
            sb.AppendLine($"Min Value: {minValue:F2}");
            sb.AppendLine($"Max Value: {maxValue:F2}");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Formats a comparison between two render results
    /// </summary>
    public string FormatComparison(RenderMetrics metrics1, RenderMetrics metrics2)
    {
        if (metrics1 == null || metrics2 == null)
            return "Cannot compare null metrics";

        var sb = new StringBuilder();
        var timeDiff = metrics2.RenderTimeMs - metrics1.RenderTimeMs;
        var timePercentDiff = metrics1.RenderTimeMs > 0
            ? (timeDiff / (double)metrics1.RenderTimeMs * 100)
            : 0;

        sb.AppendLine("=== Metrics Comparison ===");
        sb.AppendLine($"Render Time: {metrics1.RenderTimeMs}ms vs {metrics2.RenderTimeMs}ms");
        sb.AppendLine($"  Difference: {timeDiff:+0;-0}ms ({timePercentDiff:+0.0;-0.0}%)");
        sb.AppendLine($"Memory: {FormatBytes(metrics1.MemoryUsedBytes)} vs {FormatBytes(metrics2.MemoryUsedBytes)}");
        sb.AppendLine($"Output Size: {FormatBytes(metrics1.OutputSizeBytes)} vs {FormatBytes(metrics2.OutputSizeBytes)}");

        return sb.ToString();
    }

    /// <summary>
    /// Formats data point for display
    /// </summary>
    public string FormatDataPoint(DataPoint point, string? format = null)
    {
        if (point == null)
            return "null";

        if (string.IsNullOrEmpty(format))
            format = "F2";

        var sb = new StringBuilder();

        if (!string.IsNullOrEmpty(point.Label))
        {
            sb.Append(point.Label).Append(": ");
        }

        sb.Append(point.Value.ToString(format));

        if (!string.IsNullOrEmpty(point.Category))
        {
            sb.Append(" (").Append(point.Category).Append(")");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Formats byte size in human-readable format
    /// </summary>
    public static string FormatBytes(long bytes)
    {
        return bytes switch
        {
            < 1024 => $"{bytes}B",
            < 1024 * 1024 => $"{bytes / 1024.0:F1}KB",
            < 1024 * 1024 * 1024 => $"{bytes / (1024.0 * 1024.0):F1}MB",
            _ => $"{bytes / (1024.0 * 1024.0 * 1024.0):F1}GB"
        };
    }

    /// <summary>
    /// Formats memory statistics
    /// </summary>
    public string FormatMemoryStatistics(MemoryStatistics stats)
    {
        var sb = new StringBuilder();

        sb.AppendLine("=== Memory Statistics ===");
        sb.AppendLine($"Total Memory: {FormatBytes(stats.TotalMemory)}");
        sb.AppendLine($"Used Memory: {FormatBytes(stats.UsedMemory)}");
        sb.AppendLine($"Available Memory: {FormatBytes(stats.AvailableMemory)}");
        sb.AppendLine($"Usage Percentage: {stats.UsagePercentage:F1}%");

        return sb.ToString();
    }
}

/// <summary>
/// Memory statistics
/// </summary>
public class MemoryStatistics
{
    public long TotalMemory { get; set; }
    public long UsedMemory { get; set; }
    public long AvailableMemory { get; set; }

    public double UsagePercentage =>
        TotalMemory > 0 ? (UsedMemory * 100.0 / TotalMemory) : 0;
}
