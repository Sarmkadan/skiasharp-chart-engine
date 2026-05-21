// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
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
        ExportFormat.WEBP
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
        try
        {
            if (chart == null)
                throw new ArgumentNullException(nameof(chart));

            if (options == null)
                throw new ArgumentNullException(nameof(options));

            if (!SupportsFormat(options.Format))
                throw new UnsupportedExportFormatException(options.Format.ToString());

            options.Validate();

            _logger.LogInformation("Exporting chart {ChartId} as {Format}", chart.Id, options.Format);

            return _renderingService.RenderToFile(chart, options.GetFullPath());
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
}
