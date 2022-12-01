// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.Utilities;

/// <summary>
/// Exports charts to various formats for sharing and archival.
/// Supports JSON, CSV, and custom format exports.
/// </summary>
public class ChartExporter
{
    private readonly ILogger<ChartExporter> _logger;

    public ChartExporter(ILogger<ChartExporter> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // Export chart to JSON
    public async Task<string> ExportToJsonAsync(Chart chart)
    {
        try
        {
            if (chart == null)
                throw new ArgumentNullException(nameof(chart));

            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(chart, options);

            _logger.LogInformation("Chart exported to JSON: {ChartId}, Size: {Size}B", chart.Id, json.Length);
            await Task.Delay(10); // Simulate async operation
            return json;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting chart to JSON");
            throw;
        }
    }

    // Export chart to CSV
    public async Task<string> ExportToCsvAsync(Chart chart)
    {
        try
        {
            if (chart == null)
                throw new ArgumentNullException(nameof(chart));

            var csv = new System.Text.StringBuilder();
            csv.AppendLine($"Chart,{chart.Title}");
            csv.AppendLine($"Type,{chart.ChartType}");
            csv.AppendLine();

            foreach (var series in chart.Series)
            {
                csv.AppendLine($"Series,{series.Name}");
                csv.AppendLine("Label,Value");

                foreach (var point in series.DataPoints)
                {
                    csv.AppendLine($"{point.Label},{point.Value}");
                }

                csv.AppendLine();
            }

            _logger.LogInformation("Chart exported to CSV: {ChartId}", chart.Id);
            await Task.Delay(10); // Simulate async operation
            return csv.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting chart to CSV");
            throw;
        }
    }

    // Export chart metadata summary
    public ExportSummary GenerateExportSummary(Chart chart)
    {
        try
        {
            if (chart == null)
                return null;

            var totalPoints = 0;
            var minValue = double.MaxValue;
            var maxValue = double.MinValue;

            foreach (var series in chart.Series)
            {
                foreach (var point in series.DataPoints)
                {
                    totalPoints++;
                    minValue = Math.Min(minValue, point.Value);
                    maxValue = Math.Max(maxValue, point.Value);
                }
            }

            return new ExportSummary
            {
                ChartId = chart.Id,
                Title = chart.Title,
                ChartType = chart.ChartType.ToString(),
                SeriesCount = chart.Series.Count,
                TotalDataPoints = totalPoints,
                MinValue = minValue == double.MaxValue ? 0 : minValue,
                MaxValue = maxValue == double.MinValue ? 0 : maxValue,
                CreatedAt = chart.CreatedAt,
                ExportedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating export summary");
            return null;
        }
    }

    // Validate export format
    public bool IsValidExportFormat(string format)
    {
        var validFormats = new[] { "json", "csv", "xml", "png", "svg" };
        var isValid = validFormats.Contains(format.ToLower());

        if (!isValid)
        {
            _logger.LogWarning("Invalid export format: {Format}", format);
        }

        return isValid;
    }
}

/// <summary>
/// Summary of exported chart.
/// </summary>
public class ExportSummary
{
    public string ChartId { get; set; }
    public string Title { get; set; }
    public string ChartType { get; set; }
    public int SeriesCount { get; set; }
    public int TotalDataPoints { get; set; }
    public double MinValue { get; set; }
    public double MaxValue { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExportedAt { get; set; }
}
