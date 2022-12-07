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

    public async Task<RenderResult> ExportAsync(Chart chart, ExportOptions options, CancellationToken cancellationToken = default)
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

            _logger.LogInformation($"Exporting chart {chart.Id} as {options.Format} to {options.GetFullPath()}");

            var result = await _renderingService.RenderWithExportAsync(chart, options, cancellationToken);

            if (result.Success)
            {
                _logger.LogInformation($"Chart exported successfully: {result.OutputPath}");
            }
            else
            {
                _logger.LogError($"Chart export failed: {result.ErrorMessage}");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during chart export");
            return RenderResult.CreateFailure(chart?.Id ?? "unknown", ex.Message, ex);
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

            _logger.LogInformation($"Exporting chart {chart.Id} as {options.Format}");

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
