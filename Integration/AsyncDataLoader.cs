// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.Integration;

/// <summary>
/// Asynchronously loads chart data from various sources
/// Supports JSON, CSV, and remote HTTP sources
/// </summary>
public class AsyncDataLoader
{
    private readonly ILogger<AsyncDataLoader> _logger;
    private readonly int _maxFileSize = 50 * 1024 * 1024; // 50MB

    public AsyncDataLoader(ILogger<AsyncDataLoader> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Loads chart data from a file
    /// Automatically detects format based on file extension
    /// </summary>
    public async Task<Chart?> LoadChartFromFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                _logger.LogWarning("File not found: {FilePath}", filePath);
                return null;
            }

            var fileInfo = new FileInfo(filePath);
            if (fileInfo.Length > _maxFileSize)
            {
                _logger.LogError("File exceeds maximum size of {MaxSize}MB", _maxFileSize / (1024 * 1024));
                return null;
            }

            var extension = Path.GetExtension(filePath).ToLowerInvariant();

            return extension switch
            {
                ".json" => await LoadJsonAsync(filePath, cancellationToken),
                ".csv" => await LoadCsvAsync(filePath, cancellationToken),
                _ => null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading chart from file {FilePath}", filePath);
            throw;
        }
    }

    /// <summary>
    /// Loads chart data from JSON file
    /// </summary>
    private async Task<Chart?> LoadJsonAsync(string filePath, CancellationToken cancellationToken)
    {
        try
        {
            var json = await File.ReadAllTextAsync(filePath, cancellationToken);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var chart = JsonSerializer.Deserialize<Chart>(json, options);
            if (chart != null)
            {
                _logger.LogInformation("Chart loaded from JSON: {FilePath}", filePath);
            }

            return chart;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid JSON format in {FilePath}", filePath);
            return null;
        }
    }

    /// <summary>
    /// Loads chart data from CSV file
    /// Parses CSV and converts to Chart format
    /// </summary>
    private async Task<Chart?> LoadCsvAsync(string filePath, CancellationToken cancellationToken)
    {
        try
        {
            var lines = await File.ReadAllLinesAsync(filePath, cancellationToken);

            if (lines.Length < 2)
            {
                _logger.LogWarning("CSV file has insufficient data: {FilePath}", filePath);
                return null;
            }

            // Parse CSV header
            var headers = ParseCsvLine(lines[0]);
            if (headers.Count < 2)
            {
                _logger.LogWarning("CSV header must have at least 2 columns: {FilePath}", filePath);
                return null;
            }

            // Create chart with series for each column (except first)
            var chart = new Chart
            {
                Id = Guid.NewGuid().ToString(),
                Title = Path.GetFileNameWithoutExtension(filePath),
                ChartType = ChartType.Line,
                Series = new List<ChartSeries>()
            };

            // Create a series for each data column
            for (int col = 1; col < headers.Count; col++)
            {
                chart.Series.Add(new ChartSeries
                {
                    Name = headers[col],
                    DataPoints = new List<DataPoint>()
                });
            }

            // Parse data rows
            for (int row = 1; row < lines.Length; row++)
            {
                var values = ParseCsvLine(lines[row]);
                if (values.Count < headers.Count)
                    continue;

                var label = values[0];

                for (int col = 1; col < values.Count; col++)
                {
                    if (float.TryParse(values[col], NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
                    {
                        chart.Series[col - 1].DataPoints.Add(new DataPoint
                        {
                            Label = label,
                            Value = value
                        });
                    }
                }
            }

            _logger.LogInformation("Chart loaded from CSV: {FilePath} with {SeriesCount} series",
                filePath, chart.Series.Count);

            return chart;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading CSV file: {FilePath}", filePath);
            return null;
        }
    }

    /// <summary>
    /// Loads multiple charts from a directory
    /// </summary>
    public async Task<List<Chart>> LoadChartsFromDirectoryAsync(
        string directoryPath,
        string searchPattern = "*.json",
        CancellationToken cancellationToken = default)
    {
        var charts = new List<Chart>();

        try
        {
            if (!Directory.Exists(directoryPath))
            {
                _logger.LogWarning("Directory not found: {DirectoryPath}", directoryPath);
                return charts;
            }

            var files = Directory.GetFiles(directoryPath, searchPattern, SearchOption.TopDirectoryOnly);

            _logger.LogInformation("Loading {FileCount} files from {DirectoryPath}", files.Length, directoryPath);

            foreach (var file in files)
            {
                var chart = await LoadChartFromFileAsync(file, cancellationToken);
                if (chart != null)
                {
                    charts.Add(chart);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading charts from directory {DirectoryPath}", directoryPath);
        }

        return charts;
    }

    /// <summary>
    /// Loads data points from a CSV string
    /// </summary>
    public List<DataPoint> ParseCsvData(string csv)
    {
        var dataPoints = new List<DataPoint>();

        if (string.IsNullOrWhiteSpace(csv))
            return dataPoints;

        var lines = csv.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var parts = ParseCsvLine(line);
            if (parts.Count >= 2)
            {
                if (float.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
                {
                    dataPoints.Add(new DataPoint
                    {
                        Label = parts[0],
                        Value = value
                    });
                }
            }
        }

        return dataPoints;
    }

    private List<string> ParseCsvLine(string line)
    {
        var values = new List<string>();
        var current = string.Empty;
        var inQuotes = false;

        foreach (var c in line)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                values.Add(current.Trim());
                current = string.Empty;
            }
            else
            {
                current += c;
            }
        }

        if (!string.IsNullOrEmpty(current))
        {
            values.Add(current.Trim());
        }

        return values;
    }

    /// <summary>
    /// Validates if a file can be loaded
    /// </summary>
    public bool CanLoadFile(string filePath)
    {
        if (!File.Exists(filePath))
            return false;

        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        var validExtensions = new[] { ".json", ".csv" };

        return validExtensions.Contains(extension);
    }
}
