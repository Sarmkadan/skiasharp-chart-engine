// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Constants;
using SkiaSharpChartEngine.Exceptions;

namespace SkiaSharpChartEngine.Services;

/// <summary>
/// Service for exporting charts to various file formats
/// </summary>
public class ExportService : IExportService
{
    private readonly IChartRenderingService _renderingService;
    private readonly ILogger<ExportService> _logger;

    private static readonly HashSet<ExportFormat> SupportedFormats = new()
    {
        ExportFormat.PNG,
        ExportFormat.SVG,
        ExportFormat.JPEG,
    ExportFormat.WEBP,
            ExportFormat.CSV,
        ExportFormat.TSV
        };

    public ExportService(IChartRenderingService renderingService, ILogger<ExportService> logger)
    {
        _renderingService = renderingService ?? throw new ArgumentNullException(nameof(renderingService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Exports a chart to the specified format.
    /// Validation and argument errors propagate as exceptions so the API layer
    /// can map them to appropriate HTTP status codes (400/422).
    /// Infrastructure errors (I/O, SkiaSharp) are caught and returned as a
    /// failed <see cref="RenderResult"/>.
    /// </summary>
    public async Task<RenderResult> ExportAsync(Chart chart, ExportOptions options, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(chart);
        ArgumentNullException.ThrowIfNull(options);

        if (!SupportsFormat(options.Format))
            throw new UnsupportedExportFormatException(options.Format.ToString());

        options.Validate();

        try
        {
            _logger.LogInformation("Exporting chart {ChartId} as {Format} to {Path}",
                chart.Id, options.Format, options.GetFullPath());

            var result = await _renderingService.RenderWithExportAsync(chart, options, cancellationToken);

            if (result.Success)
            {
                _logger.LogInformation("Chart exported successfully: {OutputPath}", result.OutputPath);
            }
            else
            {
                _logger.LogError("Chart export failed: {ErrorMessage}", result.ErrorMessage);
            }

            return result;
        }
        catch (Exception ex) when (ex is not ArgumentException and not UnsupportedExportFormatException)
        {
            _logger.LogError(ex, "Infrastructure error during chart export");
            return RenderResult.CreateFailure(chart.Id, ex.Message, ex);
        }
    }

    public RenderResult Export(Chart chart, ExportOptions options)
    {
        if (chart == null)
            throw new ArgumentNullException(nameof(chart));

        if (options == null)
            throw new ArgumentNullException(nameof(options));

        if (!SupportsFormat(options.Format))
            throw new UnsupportedExportFormatException(options.Format.ToString());

        options.Validate();

        try
        {
            _logger.LogInformation($"Exporting chart {chart.Id} as {options.Format}");

            // Route through the format-aware export path (RenderToFile only ever
            // produces PNG) so that SVG/JPEG/WebP requests are honored here too.
            return _renderingService.RenderWithExportAsync(chart, options).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during chart export");
            return RenderResult.CreateFailure(chart?.Id ?? "unknown", ex.Message, ex);
        }
    }

    public bool SupportsFormat(ExportFormat format)
    {
        return SupportedFormats.Contains(format);
    }

    public IEnumerable<ExportFormat> GetSupportedFormats()
    {
        return SupportedFormats.OrderBy(f => f.ToString()).ToList();
    }

    /// <summary>
    /// Exports chart series data to CSV format.
    /// </summary>
    public async Task<RenderResult> ExportToCsvAsync(Chart chart, ExportOptions options, CancellationToken cancellationToken = default)
    {
        return await ExportToDelimitedAsync(chart, options, ",", cancellationToken);
    }

    /// <summary>
    /// Exports chart series data to TSV format.
    /// </summary>
    public async Task<RenderResult> ExportToTsvAsync(Chart chart, ExportOptions options, CancellationToken cancellationToken = default)
    {
        return await ExportToDelimitedAsync(chart, options, "\t", cancellationToken);
    }

    /// <summary>
    /// Exports chart series data to CSV format synchronously.
    /// </summary>
    public RenderResult ExportToCsv(Chart chart, ExportOptions options)
    {
        return ExportToDelimited(chart, options, ",");
    }

    /// <summary>
    /// Exports chart series data to TSV format synchronously.
    /// </summary>
    public RenderResult ExportToTsv(Chart chart, ExportOptions options)
    {
        return ExportToDelimited(chart, options, "\t");
    }

    private async Task<RenderResult> ExportToDelimitedAsync(Chart chart, ExportOptions options, string delimiter, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(chart);
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(delimiter))
            throw new ArgumentException("Delimiter cannot be null or whitespace", nameof(delimiter));

        try
        {
            _logger.LogInformation("Exporting chart {ChartId} to {Format} format at {Path}",
                chart.Id, options.Format, options.GetFullPath());

            var csvContent = GenerateCsvContent(chart, delimiter);
            var outputPath = options.GetFullPath();

            await File.WriteAllTextAsync(outputPath, csvContent, cancellationToken);

            _logger.LogInformation("Chart series data exported successfully to CSV/TSV: {OutputPath}", outputPath);

            return RenderResult.CreateSuccess(chart.Id, outputPath, csvContent.Length, ExportFormat.CSV);
        }
        catch (Exception ex) when (ex is not ArgumentException)
        {
            _logger.LogError(ex, "Error during CSV/TSV export");
            return RenderResult.CreateFailure(chart.Id, ex.Message, ex);
        }
    }

    private RenderResult ExportToDelimited(Chart chart, ExportOptions options, string delimiter)
    {
        ArgumentNullException.ThrowIfNull(chart);
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(delimiter))
            throw new ArgumentException("Delimiter cannot be null or whitespace", nameof(delimiter));

        try
        {
            _logger.LogInformation("Exporting chart {ChartId} to {Format} format", chart.Id, options.Format);

            var csvContent = GenerateCsvContent(chart, delimiter);
            var outputPath = options.GetFullPath();

            File.WriteAllText(outputPath, csvContent);

            _logger.LogInformation("Chart series data exported successfully to CSV/TSV: {OutputPath}", outputPath);

            return RenderResult.CreateSuccess(chart.Id, outputPath, csvContent.Length, ExportFormat.CSV);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during CSV/TSV export");
            return RenderResult.CreateFailure(chart.Id, ex.Message, ex);
        }
    }

    private string GenerateCsvContent(Chart chart, string delimiter)
    {
        using var writer = new StringWriter();
        var delimiterChar = delimiter[0];

        // Write header row
        if (chart.Series.Count > 0)
        {
            writer.Write("Series");
            writer.Write(delimiterChar);
            writer.Write("X");
            writer.Write(delimiterChar);
            writer.Write("Y");

            bool hasLabels = chart.Series.Any(s => s.DataPoints.Any(dp => !string.IsNullOrEmpty(dp.Label)));
            if (hasLabels)
            {
                writer.Write("Label");
                writer.Write(delimiterChar);
            }

            writer.WriteLine();

            for (int seriesIndex = 0; seriesIndex < chart.Series.Count; seriesIndex++)
            {
                var series = chart.Series[seriesIndex];

                for (int pointIndex = 0; pointIndex < series.DataPoints.Count; pointIndex++)
                {
                    var point = series.DataPoints[pointIndex];

                    writer.Write(EscapeCsvField(series.Name));
                    writer.Write(delimiterChar);
                    writer.Write(FormatNumber(point.X, delimiterChar));
                    writer.Write(delimiterChar);
                    writer.Write(FormatNumber(point.Y, delimiterChar));

                    if (hasLabels && !string.IsNullOrEmpty(point.Label))
                    {
                        writer.Write(delimiterChar);
                        writer.Write(EscapeCsvField(point.Label));
                    }

                    writer.WriteLine();
                }
            }
        }

        return writer.ToString();
    }

    private string EscapeCsvField(string? field)
    {
        if (string.IsNullOrEmpty(field))
            return "";

        if (field.Contains('\n') || field.Contains('\r') || field.Contains('"') || field.Contains(','))
        {
            return '"' + field.Replace("\"", "\"\"") + '"';
        }

        return field;
    }

    private string FormatNumber(double value, char delimiter)
    {
        return value.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }
}
