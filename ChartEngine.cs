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

    /// <summary>
    /// Initializes a new instance of the <see cref="ChartEngine"/> class using the specified service provider.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve the engine's required services.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="serviceProvider"/> is null.</exception>
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
    /// Creates a new chart engine with default configuration.
    /// </summary>
    /// <returns>A new <see cref="ChartEngine"/> instance configured with default services.</returns>
    public static ChartEngine Create()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSkiaSharpChartEngine();

        var provider = services.BuildServiceProvider();
        return new ChartEngine(provider);
    }

    /// <summary>
    /// Creates a new chart engine with custom configuration.
    /// </summary>
    /// <param name="configureServices">A delegate used to customize the service collection before the engine is built.</param>
    /// <returns>A new <see cref="ChartEngine"/> instance configured with the customized services.</returns>
    public static ChartEngine Create(Action<IServiceCollection> configureServices)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSkiaSharpChartEngine();
        configureServices(services);

        var provider = services.BuildServiceProvider();
        return new ChartEngine(provider);
    }

    /// <summary>
    /// Validates and renders the specified chart asynchronously.
    /// </summary>
    /// <param name="chart">The chart to render.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that resolves to a <see cref="RenderResult"/> describing the outcome of the render operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="chart"/> is null.</exception>
    public async Task<RenderResult> RenderChartAsync(Chart chart, CancellationToken cancellationToken = default)
    {
        if (chart == null)
            throw new ArgumentNullException(nameof(chart));

        try
        {
            _dataService.ValidateChart(chart);
            return await _renderingService.RenderToByteArrayAsync(chart, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering chart");
            return RenderResult.CreateFailure(chart?.Id ?? "unknown", ex.Message, ex);
        }
    }

    /// <summary>
    /// Exports the specified chart asynchronously using the given export options.
    /// </summary>
    /// <param name="chart">The chart to export.</param>
    /// <param name="options">The options controlling the export format and behavior.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that resolves to a <see cref="RenderResult"/> describing the outcome of the export operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="chart"/> or <paramref name="options"/> is null.</exception>
    public async Task<RenderResult> ExportChartAsync(Chart chart, ExportOptions options, CancellationToken cancellationToken = default)
    {
        if (chart == null)
            throw new ArgumentNullException(nameof(chart));

        if (options == null)
            throw new ArgumentNullException(nameof(options));

        try
        {
            return await _exportService.ExportAsync(chart, options, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting chart");
            return RenderResult.CreateFailure(chart?.Id ?? "unknown", ex.Message, ex);
        }
    }

    /// <summary>
    /// Validates and renders the specified chart synchronously.
    /// </summary>
    /// <param name="chart">The chart to render.</param>
    /// <returns>A <see cref="RenderResult"/> describing the outcome of the render operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="chart"/> is null.</exception>
    public RenderResult RenderChart(Chart chart)
    {
        if (chart == null)
            throw new ArgumentNullException(nameof(chart));

        try
        {
            _dataService.ValidateChart(chart);
            return _renderingService.RenderToByteArray(chart);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering chart");
            return RenderResult.CreateFailure(chart?.Id ?? "unknown", ex.Message, ex);
        }
    }

    /// <summary>
    /// Exports the specified chart synchronously using the given export options.
    /// </summary>
    /// <param name="chart">The chart to export.</param>
    /// <param name="options">The options controlling the export format and behavior.</param>
    /// <returns>A <see cref="RenderResult"/> describing the outcome of the export operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="chart"/> or <paramref name="options"/> is null.</exception>
    public RenderResult ExportChart(Chart chart, ExportOptions options)
    {
        if (chart == null)
            throw new ArgumentNullException(nameof(chart));

        if (options == null)
            throw new ArgumentNullException(nameof(options));

        try
        {
            return _exportService.Export(chart, options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting chart");
            return RenderResult.CreateFailure(chart?.Id ?? "unknown", ex.Message, ex);
        }
    }

    /// <summary>
    /// Saves the specified chart to the underlying repository asynchronously.
    /// </summary>
    /// <param name="chart">The chart to save.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that resolves to the identifier assigned to the saved chart.</returns>
    public async Task<string> SaveChartAsync(Chart chart, CancellationToken cancellationToken = default)
    {
        return await _repository.SaveAsync(chart, cancellationToken);
    }

    /// <summary>
    /// Saves the specified chart to the underlying repository synchronously.
    /// </summary>
    /// <param name="chart">The chart to save.</param>
    /// <returns>The identifier assigned to the saved chart.</returns>
    public string SaveChart(Chart chart)
    {
        return _repository.Save(chart);
    }

    /// <summary>
    /// Retrieves a chart by its identifier from the underlying repository asynchronously.
    /// </summary>
    /// <param name="chartId">The unique identifier of the chart to retrieve.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that resolves to the matching <see cref="Chart"/>, or null if no chart was found.</returns>
    public async Task<Chart?> GetChartAsync(string chartId, CancellationToken cancellationToken = default)
    {
        return await _repository.GetByIdAsync(chartId, cancellationToken);
    }

    /// <summary>
    /// Retrieves a chart by its identifier from the underlying repository synchronously.
    /// </summary>
    /// <param name="chartId">The unique identifier of the chart to retrieve.</param>
    /// <returns>The matching <see cref="Chart"/>, or null if no chart was found.</returns>
    public Chart? GetChart(string chartId)
    {
        return _repository.GetById(chartId);
    }

    /// <summary>
    /// Updates an existing chart in the underlying repository asynchronously.
    /// </summary>
    /// <param name="chart">The chart containing the updated state.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that resolves to true if the update succeeded; otherwise, false.</returns>
    public async Task<bool> UpdateChartAsync(Chart chart, CancellationToken cancellationToken = default)
    {
        return await _repository.UpdateAsync(chart, cancellationToken);
    }

    /// <summary>
    /// Updates an existing chart in the underlying repository synchronously.
    /// </summary>
    /// <param name="chart">The chart containing the updated state.</param>
    /// <returns>true if the update succeeded; otherwise, false.</returns>
    public bool UpdateChart(Chart chart)
    {
        return _repository.Update(chart);
    }

    /// <summary>
    /// Deletes a chart by its identifier from the underlying repository asynchronously.
    /// </summary>
    /// <param name="chartId">The unique identifier of the chart to delete.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that resolves to true if the deletion succeeded; otherwise, false.</returns>
    public async Task<bool> DeleteChartAsync(string chartId, CancellationToken cancellationToken = default)
    {
        return await _repository.DeleteAsync(chartId, cancellationToken);
    }

    /// <summary>
    /// Deletes a chart by its identifier from the underlying repository synchronously.
    /// </summary>
    /// <param name="chartId">The unique identifier of the chart to delete.</param>
    /// <returns>true if the deletion succeeded; otherwise, false.</returns>
    public bool DeleteChart(string chartId)
    {
        return _repository.Delete(chartId);
    }

    /// <summary>
    /// Gets the default chart configuration provided by the configuration service.
    /// </summary>
    /// <returns>A <see cref="ChartConfiguration"/> representing the default configuration.</returns>
    public ChartConfiguration GetDefaultConfiguration()
    {
        return _configurationService.GetDefaultConfiguration();
    }

    /// <summary>
    /// Creates a configuration template tailored to the specified chart type.
    /// </summary>
    /// <param name="chartType">The type of chart for which to create the template.</param>
    /// <returns>A <see cref="ChartConfiguration"/> pre-populated with sensible defaults for the given chart type.</returns>
    public ChartConfiguration GetConfigurationTemplate(ChartType chartType)
    {
        return _configurationService.CreateConfigurationFromTemplate(chartType);
    }

    /// <summary>
    /// Gets the collection of export formats supported by the export service.
    /// </summary>
    /// <returns>An enumerable of supported <see cref="ExportFormat"/> values.</returns>
    public IEnumerable<ExportFormat> GetSupportedExportFormats()
    {
        return _exportService.GetSupportedFormats();
    }

    /// <summary>
    /// Warms up the render cache for the specified chart so subsequent renders are faster.
    /// </summary>
    /// <param name="chart">The chart whose resources should be pre-cached.</param>
    public void PrewarmRenderCache(Chart chart)
    {
        _renderingService.PrewarmCache(chart);
    }

    /// <summary>
    /// Exposes the underlying service provider used by this engine instance.
    /// </summary>
    /// <returns>The <see cref="IServiceProvider"/> backing this <see cref="ChartEngine"/>.</returns>
    public IServiceProvider GetServiceProvider() => _serviceProvider;
}
