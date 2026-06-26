// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SkiaSharpChartEngine.Configuration;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Repository;
using SkiaSharpChartEngine.Services;

namespace SkiaSharpChartEngine;

/// <summary>
/// Provides the main entry point and facade for the SkiaSharp Chart Engine.
/// This class exposes a unified API for rendering, exporting, and managing
/// chart definitions, leveraging dependency injection for service resolution.
/// </summary>
public class ChartEngine
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IChartRenderingService _renderingService;
    private readonly IChartRepository _repository;
    private readonly IExportService _exportService;
    private readonly IConfigurationService _configurationService;
    private readonly IChartDataService _dataService;
    private readonly ILogger<ChartEngine> _logger;

    public ChartEngine(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _renderingService = serviceProvider.GetRequiredService<IChartRenderingService>();
        _repository = serviceProvider.GetRequiredService<IChartRepository>();
        _exportService = serviceProvider.GetRequiredService<IExportService>();
        _configurationService = serviceProvider.GetRequiredService<IConfigurationService>();
        _dataService = serviceProvider.GetRequiredService<IChartDataService>();
        _logger = serviceProvider.GetRequiredService<ILogger<ChartEngine>>();
    }

    /// <summary>
    /// Create a new chart engine with default configuration
    /// </summary>
    public static ChartEngine Create()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSkiaSharpChartEngine();

        var provider = services.BuildServiceProvider();
        return new ChartEngine(provider);
    }

    /// <summary>
    /// Create a new chart engine with custom configuration
    /// </summary>
    public static ChartEngine Create(Action<IServiceCollection> configureServices)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        configureServices(services);

        var provider = services.BuildServiceProvider();
        return new ChartEngine(provider);
    }

    public async Task<RenderResult> RenderChartAsync(Chart chart, CancellationToken cancellationToken = default)
    {
        try
        {
            if (chart == null)
                throw new ArgumentNullException(nameof(chart));

            _dataService.ValidateChart(chart);
            return await _renderingService.RenderToByteArrayAsync(chart, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering chart");
            return RenderResult.CreateFailure(chart?.Id ?? "unknown", ex.Message, ex);
        }
    }

    public async Task<RenderResult> ExportChartAsync(Chart chart, ExportOptions options, CancellationToken cancellationToken = default)
    {
        try
        {
            if (chart == null)
                throw new ArgumentNullException(nameof(chart));

            if (options == null)
                throw new ArgumentNullException(nameof(options));

            return await _exportService.ExportAsync(chart, options, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting chart");
            return RenderResult.CreateFailure(chart?.Id ?? "unknown", ex.Message, ex);
        }
    }

    public RenderResult RenderChart(Chart chart)
    {
        try
        {
            if (chart == null)
                throw new ArgumentNullException(nameof(chart));

            _dataService.ValidateChart(chart);
            return _renderingService.RenderToByteArray(chart);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering chart");
            return RenderResult.CreateFailure(chart?.Id ?? "unknown", ex.Message, ex);
        }
    }

    public RenderResult ExportChart(Chart chart, ExportOptions options)
    {
        try
        {
            if (chart == null)
                throw new ArgumentNullException(nameof(chart));

            if (options == null)
                throw new ArgumentNullException(nameof(options));

            return _exportService.Export(chart, options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting chart");
            return RenderResult.CreateFailure(chart?.Id ?? "unknown", ex.Message, ex);
        }
    }

    public async Task<string> SaveChartAsync(Chart chart, CancellationToken cancellationToken = default)
    {
        return await _repository.SaveAsync(chart, cancellationToken);
    }

    public string SaveChart(Chart chart)
    {
        return _repository.Save(chart);
    }

    public async Task<Chart?> GetChartAsync(string chartId, CancellationToken cancellationToken = default)
    {
        return await _repository.GetByIdAsync(chartId, cancellationToken);
    }

    public Chart? GetChart(string chartId)
    {
        return _repository.GetById(chartId);
    }

    public async Task<bool> UpdateChartAsync(Chart chart, CancellationToken cancellationToken = default)
    {
        return await _repository.UpdateAsync(chart, cancellationToken);
    }

    public bool UpdateChart(Chart chart)
    {
        return _repository.Update(chart);
    }

    public async Task<bool> DeleteChartAsync(string chartId, CancellationToken cancellationToken = default)
    {
        return await _repository.DeleteAsync(chartId, cancellationToken);
    }

    public bool DeleteChart(string chartId)
    {
        return _repository.Delete(chartId);
    }

    public ChartConfiguration GetDefaultConfiguration()
    {
        return _configurationService.GetDefaultConfiguration();
    }

    public ChartConfiguration GetConfigurationTemplate(ChartType chartType)
    {
        return _configurationService.CreateConfigurationFromTemplate(chartType);
    }

    public IEnumerable<ExportFormat> GetSupportedExportFormats()
    {
        return _exportService.GetSupportedFormats();
    }

    public void PrewarmRenderCache(Chart chart)
    {
        _renderingService.PrewarmCache(chart);
    }

    public IServiceProvider GetServiceProvider() => _serviceProvider;
}
