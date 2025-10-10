// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.Services;

/// <summary>
/// Interface for managing chart configurations
/// </summary>
public interface IConfigurationService
{
    ChartConfiguration GetDefaultConfiguration();

    ChartConfiguration GetConfiguration(string configName);

    void SaveConfiguration(string configName, ChartConfiguration configuration);

    void DeleteConfiguration(string configName);

    IEnumerable<string> ListConfigurations();

    ChartConfiguration CreateConfigurationFromTemplate(ChartType chartType);

    bool ConfigurationExists(string configName);
}
