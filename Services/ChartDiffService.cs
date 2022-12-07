// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.Services;

/// <summary>
/// Computes differences between chart versions for change tracking and auditing.
/// Identifies modifications to data, configuration, and metadata.
/// </summary>
public class ChartDiffService
{
    private readonly ILogger<ChartDiffService> _logger;

    public ChartDiffService(ILogger<ChartDiffService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // Compute diff between two charts
    public ChartDiff ComputeDiff(Chart oldChart, Chart newChart)
    {
        try
        {
            if (oldChart == null || newChart == null)
                return null;

            var diff = new ChartDiff
            {
                ChartId = newChart.Id,
                ComputedAt = DateTime.UtcNow,
                Changes = new List<Change>()
            };

            // Compare basic properties
            if (oldChart.Title != newChart.Title)
            {
                diff.Changes.Add(new Change
                {
                    Property = "Title",
                    OldValue = oldChart.Title,
                    NewValue = newChart.Title
                });
            }

            if (oldChart.ChartType != newChart.ChartType)
            {
                diff.Changes.Add(new Change
                {
                    Property = "ChartType",
                    OldValue = oldChart.ChartType.ToString(),
                    NewValue = newChart.ChartType.ToString()
                });
            }

            // Compare series
            _compareSeries(oldChart.Series, newChart.Series, diff);

            // Compare configuration
            _compareConfiguration(oldChart.ChartConfiguration, newChart.ChartConfiguration, diff);

            _logger.LogInformation("Chart diff computed: {ChartId}, Changes: {ChangeCount}", newChart.Id, diff.Changes.Count);
            return diff;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error computing chart diff");
            return null;
        }
    }

    // Generate diff report
    public string GenerateDiffReport(ChartDiff diff)
    {
        try
        {
            if (diff == null || diff.Changes.Count == 0)
                return "No changes detected.";

            var report = new System.Text.StringBuilder();
            report.AppendLine($"Chart Diff Report - {diff.ComputedAt:O}");
            report.AppendLine(new string('=', 50));
            report.AppendLine();

            foreach (var change in diff.Changes)
            {
                report.AppendLine($"Property: {change.Property}");
                report.AppendLine($"  Old: {change.OldValue ?? "(null)"}");
                report.AppendLine($"  New: {change.NewValue ?? "(null)"}");
                report.AppendLine();
            }

            return report.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating diff report");
            return "Error generating report";
        }
    }

    private void _compareSeries(List<ChartSeries> oldSeries, List<ChartSeries> newSeries, ChartDiff diff)
    {
        var oldCount = oldSeries?.Count ?? 0;
        var newCount = newSeries?.Count ?? 0;

        if (oldCount != newCount)
        {
            diff.Changes.Add(new Change
            {
                Property = "Series.Count",
                OldValue = oldCount.ToString(),
                NewValue = newCount.ToString()
            });
        }

        // Compare individual series
        if (oldSeries != null && newSeries != null)
        {
            for (int i = 0; i < Math.Min(oldCount, newCount); i++)
            {
                var oldSerie = oldSeries[i];
                var newSerie = newSeries[i];

                if (oldSerie.Name != newSerie.Name)
                {
                    diff.Changes.Add(new Change
                    {
                        Property = $"Series[{i}].Name",
                        OldValue = oldSerie.Name,
                        NewValue = newSerie.Name
                    });
                }

                var oldPointCount = oldSerie.DataPoints?.Count ?? 0;
                var newPointCount = newSerie.DataPoints?.Count ?? 0;

                if (oldPointCount != newPointCount)
                {
                    diff.Changes.Add(new Change
                    {
                        Property = $"Series[{i}].DataPoints.Count",
                        OldValue = oldPointCount.ToString(),
                        NewValue = newPointCount.ToString()
                    });
                }
            }
        }
    }

    private void _compareConfiguration(ChartConfiguration oldConfig, ChartConfiguration newConfig, ChartDiff diff)
    {
        if (oldConfig == null && newConfig == null)
            return;

        if (oldConfig == null || newConfig == null)
        {
            diff.Changes.Add(new Change
            {
                Property = "Configuration",
                OldValue = oldConfig == null ? "(null)" : "(set)",
                NewValue = newConfig == null ? "(null)" : "(set)"
            });
            return;
        }

        if (oldConfig.Width != newConfig.Width)
        {
            diff.Changes.Add(new Change
            {
                Property = "Configuration.Width",
                OldValue = oldConfig.Width.ToString(),
                NewValue = newConfig.Width.ToString()
            });
        }

        if (oldConfig.Height != newConfig.Height)
        {
            diff.Changes.Add(new Change
            {
                Property = "Configuration.Height",
                OldValue = oldConfig.Height.ToString(),
                NewValue = newConfig.Height.ToString()
            });
        }
    }
}

/// <summary>
/// Represents differences between two charts.
/// </summary>
public class ChartDiff
{
    public string ChartId { get; set; }
    public DateTime ComputedAt { get; set; }
    public List<Change> Changes { get; set; }

    public bool HasChanges => Changes.Count > 0;
}

/// <summary>
/// Represents a single property change.
/// </summary>
public class Change
{
    public string Property { get; set; }
    public string OldValue { get; set; }
    public string NewValue { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
}
