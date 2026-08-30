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
        ArgumentNullException.ThrowIfNull(oldChart);
        ArgumentNullException.ThrowIfNull(newChart);

        try
        {
            var diff = new ChartDiff
            {
                ChartId = newChart.Id,
                ComputedAt = DateTime.UtcNow,
                Changes = new List<Change>()
            };

            // Compare basic properties
            if (oldChart.Title != newChart.Title)
            {
                AddChange(diff, "Title", oldChart.Title, newChart.Title);
            }

            if (oldChart.ChartType != newChart.ChartType)
            {
                AddChange(diff, "ChartType", oldChart.ChartType.ToString(), newChart.ChartType.ToString());
            }

            // Compare series
            CompareSeries(oldChart.Series, newChart.Series, diff);

            // Compare configuration
            CompareConfiguration(oldChart.ChartConfiguration, newChart.ChartConfiguration, diff);

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
        ArgumentNullException.ThrowIfNull(diff);

        try
        {
            if (diff.Changes.Count == 0)
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

    private void CompareSeries(List<ChartSeries> oldSeries, List<ChartSeries> newSeries, ChartDiff diff)
    {
        var oldCount = oldSeries?.Count ?? 0;
        var newCount = newSeries?.Count ?? 0;

        if (oldCount != newCount)
        {
            AddChange(diff, "Series.Count", oldCount.ToString(), newCount.ToString());
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
                    AddChange(diff, $"Series[{i}].Name", oldSerie.Name, newSerie.Name);
                }

                var oldPointCount = oldSerie.DataPoints?.Count ?? 0;
                var newPointCount = newSerie.DataPoints?.Count ?? 0;

                if (oldPointCount != newPointCount)
                {
                    AddChange(
                        diff,
                        $"Series[{i}].DataPoints.Count",
                        oldPointCount.ToString(),
                        newPointCount.ToString());
                }
            }
        }
    }

    private void CompareConfiguration(ChartConfiguration oldConfig, ChartConfiguration newConfig, ChartDiff diff)
    {
        void CompareAndAdd<T>(string property, T oldValue, T newValue)
        {
            if (!EqualityComparer<T>.Default.Equals(oldValue, newValue))
            {
                AddChange(diff, property, oldValue?.ToString(), newValue?.ToString());
            }
        }

        if (oldConfig == null && newConfig == null)
            return;

        if (oldConfig == null || newConfig == null)
        {
            AddChange(
                diff,
                "Configuration",
                oldConfig == null ? "(null)" : "(set)",
                newConfig == null ? "(null)" : "(set)");
            return;
        }

        CompareAndAdd("Configuration.Width", oldConfig.Width, newConfig.Width);
        CompareAndAdd("Configuration.Height", oldConfig.Height, newConfig.Height);
    }

    private void AddChange(ChartDiff diff, string property, string? oldValue, string? newValue)
    {
        diff.Changes.Add(new Change
        {
            Property = property,
            OldValue = oldValue,
            NewValue = newValue
        });
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

    public ChartDiff()
    {
        ChartId = string.Empty;
        Changes = new List<Change>();
    }

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

    public Change()
    {
        Property = string.Empty;
        OldValue = string.Empty;
        NewValue = string.Empty;
    }
}
