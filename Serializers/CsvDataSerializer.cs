// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Utilities;

namespace SkiaSharpChartEngine.Serializers;

/// <summary>
/// Serializes Chart data to CSV format
/// Exports chart data for use in spreadsheets and data analysis tools
/// </summary>
public class CsvDataSerializer : IDataSerializer
{
    private readonly ILogger<CsvDataSerializer> _logger;

    public CsvDataSerializer(ILogger<CsvDataSerializer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Serializes chart series to CSV format
    /// Each series becomes a column with values
    /// </summary>
    public string Serialize(Chart chart)
    {
        try
        {
            if (chart == null)
                throw new ArgumentNullException(nameof(chart));

            if (chart.Series == null || chart.Series.Count == 0)
            {
                _logger.LogWarning("Chart {ChartId} has no series data", chart.Id);
                return string.Empty;
            }

            var csv = new StringBuilder();

            // Write header row with series names
            var headers = new List<string> { "Index" };
            headers.AddRange(chart.Series.Select(s => StringFormatHelper.ToCsvLine(s.Name ?? "Series")));
            csv.AppendLine(StringFormatHelper.ToCsvLine(headers.ToArray()));

            // Find maximum number of data points
            var maxDataPoints = chart.Series.Max(s => s.DataPoints?.Count ?? 0);

            // Write data rows
            for (int i = 0; i < maxDataPoints; i++)
            {
                var row = new List<object> { i };

                foreach (var series in chart.Series)
                {
                    if (series.DataPoints != null && i < series.DataPoints.Count)
                    {
                        row.Add(series.DataPoints[i].Value);
                    }
                    else
                    {
                        row.Add(string.Empty);
                    }
                }

                csv.AppendLine(StringFormatHelper.ToCsvLine(row.ToArray()));
            }

            _logger.LogInformation("Chart {ChartId} exported to CSV with {RowCount} rows",
                chart.Id, maxDataPoints + 1);

            return csv.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error serializing chart to CSV");
            throw;
        }
    }

    /// <summary>
    /// Serializes with additional metadata about the chart
    /// Includes chart info and configuration in comments
    /// </summary>
    public string SerializeWithMetadata(Chart chart)
    {
        try
        {
            var csv = new StringBuilder();

            // Write metadata as comments
            csv.AppendLine($"# Chart: {chart.Title}");
            csv.AppendLine($"# ID: {chart.Id}");
            csv.AppendLine($"# Type: {chart.ChartType}");
            csv.AppendLine($"# Generated: {DateTime.UtcNow:O}");
            csv.AppendLine();

            csv.Append(Serialize(chart));

            return csv.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error serializing chart to CSV with metadata");
            throw;
        }
    }

    /// <summary>
    /// Exports data in wide format (data points as columns)
    /// Alternative to the default long format
    /// </summary>
    public string SerializeWideFormat(Chart chart)
    {
        try
        {
            if (chart?.Series == null)
                return string.Empty;

            var csv = new StringBuilder();

            // Headers: series names
            var headers = chart.Series.Select(s => s.Name ?? "Series").ToList();
            csv.AppendLine(StringFormatHelper.ToCsvLine(headers.Cast<object>().ToArray()));

            // Find max data points
            var maxPoints = chart.Series.Max(s => s.DataPoints?.Count ?? 0);

            // Write value rows
            for (int i = 0; i < maxPoints; i++)
            {
                var values = new List<object>();

                foreach (var series in chart.Series)
                {
                    if (series.DataPoints != null && i < series.DataPoints.Count)
                    {
                        values.Add(series.DataPoints[i].Value);
                    }
                    else
                    {
                        values.Add(string.Empty);
                    }
                }

                csv.AppendLine(StringFormatHelper.ToCsvLine(values.ToArray()));
            }

            return csv.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error serializing to wide format");
            throw;
        }
    }

    /// <summary>
    /// Validates CSV content structure
    /// </summary>
    public bool IsValidCsv(string csv)
    {
        if (string.IsNullOrWhiteSpace(csv))
            return false;

        try
        {
            var lines = csv.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            if (lines.Length < 2)
                return false;

            var headerCells = lines[0].Split(',');
            var expectedCellCount = headerCells.Length;

            // Check that all rows have the same number of cells
            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i]))
                    continue;

                var cells = lines[i].Split(',');
                if (cells.Length != expectedCellCount)
                    return false;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets the MIME type for CSV
    /// </summary>
    public string GetMimeType() => "text/csv";

    /// <summary>
    /// Gets the file extension for CSV
    /// </summary>
    public string GetFileExtension() => ".csv";
}

/// <summary>
/// Interface for data serialization
/// Allows different export formats
/// </summary>
public interface IDataSerializer
{
    string Serialize(Chart chart);
    string GetMimeType();
    string GetFileExtension();
}
