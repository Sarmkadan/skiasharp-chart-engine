// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Constants;
using SkiaSharpChartEngine.Exceptions;

namespace SkiaSharpChartEngine.Services;

/// <summary>
/// Service for managing chart configurations and templates
/// </summary>
public class ConfigurationService : IConfigurationService
{
    private readonly Dictionary<string, ChartConfiguration> _configurations = new();
    private readonly ILogger<ConfigurationService> _logger;

    public ConfigurationService(ILogger<ConfigurationService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        InitializeDefaultConfigurations();
    }

    public ChartConfiguration GetDefaultConfiguration()
    {
        _logger.LogDebug("Retrieving default configuration");
        return new ChartConfiguration();
    }

    public ChartConfiguration GetConfiguration(string configName)
    {
        if (string.IsNullOrWhiteSpace(configName))
            throw new ArgumentNullException(nameof(configName));

        if (!_configurations.TryGetValue(configName, out var config))
            throw new ResourceNotFoundException(nameof(ChartConfiguration), configName);

        _logger.LogInformation($"Retrieved configuration: {configName}");
        return config.Clone();
    }

    public void SaveConfiguration(string configName, ChartConfiguration configuration)
    {
        if (string.IsNullOrWhiteSpace(configName))
            throw new ArgumentNullException(nameof(configName));

        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        configuration.Validate();

        _configurations[configName] = configuration.Clone();
        _logger.LogInformation($"Saved configuration: {configName}");
    }

    public void DeleteConfiguration(string configName)
    {
        if (string.IsNullOrWhiteSpace(configName))
            throw new ArgumentNullException(nameof(configName));

        if (_configurations.Remove(configName))
        {
            _logger.LogInformation($"Deleted configuration: {configName}");
        }
    }

    public IEnumerable<string> ListConfigurations()
    {
        return _configurations.Keys.OrderBy(k => k).ToList();
    }

    public ChartConfiguration CreateConfigurationFromTemplate(ChartType chartType)
    {
        _logger.LogDebug($"Creating configuration from template: {chartType}");

        return chartType switch
        {
            ChartType.LineChart => new ChartConfiguration
            {
                Title = "Line Chart",
                XAxisScaleType = AxisScaleType.Linear,
                YAxisScaleType = AxisScaleType.Linear,
                ShowGrid = true,
                ShowLegend = true,
                EnableAnimation = true
            },
            ChartType.BarChart => new ChartConfiguration
            {
                Title = "Bar Chart",
                XAxisScaleType = AxisScaleType.Categorical,
                YAxisScaleType = AxisScaleType.Linear,
                ShowGrid = true,
                ShowLegend = true,
                MarginBottom = 80
            },
            ChartType.PieChart => new ChartConfiguration
            {
                Title = "Pie Chart",
                ShowLegend = true,
                ShowGrid = false,
                ShowAxisLabels = false,
                Width = 600,
                Height = 600
            },
            ChartType.HeatmapChart => new ChartConfiguration
            {
                Title = "Heatmap",
                XAxisScaleType = AxisScaleType.Categorical,
                YAxisScaleType = AxisScaleType.Categorical,
                ShowGrid = true,
                ShowLegend = true
            },
            ChartType.AreaChart => new ChartConfiguration
            {
                Title = "Area Chart",
                XAxisScaleType = AxisScaleType.Linear,
                YAxisScaleType = AxisScaleType.Linear,
                ShowGrid = true,
                ShowLegend = true,
                EnableAnimation = true
            },
            ChartType.ScatterChart => new ChartConfiguration
            {
                Title = "Scatter Plot",
                XAxisScaleType = AxisScaleType.Linear,
                YAxisScaleType = AxisScaleType.Linear,
                ShowGrid = true,
                ShowLegend = true
            },
            ChartType.ColumnChart => new ChartConfiguration
            {
                Title = "Column Chart",
                XAxisScaleType = AxisScaleType.Categorical,
                YAxisScaleType = AxisScaleType.Linear,
                ShowGrid = true,
                ShowLegend = true
            },
            _ => GetDefaultConfiguration()
        };
    }

    public bool ConfigurationExists(string configName)
    {
        return !string.IsNullOrWhiteSpace(configName) && _configurations.ContainsKey(configName);
    }

    private void InitializeDefaultConfigurations()
    {
        var lineChartConfig = CreateConfigurationFromTemplate(ChartType.LineChart);
        _configurations["default_line"] = lineChartConfig;

        var barChartConfig = CreateConfigurationFromTemplate(ChartType.BarChart);
        _configurations["default_bar"] = barChartConfig;

        var pieChartConfig = CreateConfigurationFromTemplate(ChartType.PieChart);
        _configurations["default_pie"] = pieChartConfig;

        _logger.LogInformation("Initialized default configurations");
    }
}
