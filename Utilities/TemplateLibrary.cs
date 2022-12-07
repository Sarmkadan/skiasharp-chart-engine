// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.Utilities;

/// <summary>
/// Library of predefined chart templates for quick chart creation.
/// Provides templates for common use cases.
/// </summary>
public class TemplateLibrary
{
    private readonly Dictionary<string, ChartTemplate> _templates;
    private readonly ILogger<TemplateLibrary> _logger;

    public TemplateLibrary(ILogger<TemplateLibrary> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _templates = new Dictionary<string, ChartTemplate>();
        _initializeDefaultTemplates();
    }

    // Get template by name
    public ChartTemplate GetTemplate(string name)
    {
        if (_templates.TryGetValue(name, out var template))
        {
            _logger.LogDebug("Template retrieved: {TemplateName}", name);
            return template;
        }

        _logger.LogWarning("Template not found: {TemplateName}", name);
        return null;
    }

    // Get all templates
    public Dictionary<string, ChartTemplate> GetAllTemplates() => new Dictionary<string, ChartTemplate>(_templates);

    // Get templates by category
    public List<ChartTemplate> GetTemplatesByCategory(string category)
    {
        return _templates.Values
            .Where(t => t.Category == category)
            .ToList();
    }

    // Add custom template
    public void AddTemplate(string name, ChartTemplate template)
    {
        if (string.IsNullOrWhiteSpace(name) || template == null)
            return;

        _templates[name] = template;
        _logger.LogInformation("Custom template added: {TemplateName}", name);
    }

    // Create chart from template
    public Chart CreateChartFromTemplate(string templateName, string chartId = "")
    {
        try
        {
            var template = GetTemplate(templateName);
            if (template == null)
            {
                _logger.LogWarning("Cannot create chart, template not found: {TemplateName}", templateName);
                return null;
            }

            var chart = new Chart
            {
                Id = string.IsNullOrEmpty(chartId) ? Guid.NewGuid().ToString() : chartId,
                Title = template.Name,
                ChartType = template.ChartConfiguration?.ChartType ?? ChartType.Line,
                Series = new List<ChartSeries>(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _logger.LogInformation("Chart created from template: {TemplateName}", templateName);
            return chart;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating chart from template");
            return null;
        }
    }

    // List template names
    public IEnumerable<string> ListTemplateNames() => _templates.Keys;

    // Get template count
    public int GetTemplateCount() => _templates.Count;

    private void _initializeDefaultTemplates()
    {
        // Simple line chart template
        _templates["simple_line"] = new ChartTemplate
        {
            Id = "simple_line",
            Name = "Simple Line Chart",
            Description = "Basic line chart for trend analysis",
            Category = "Line",
            ChartConfiguration = new ChartConfiguration
            {
                ChartType = ChartType.Line,
                Width = 800,
                Height = 600
            },
            CreatedAt = DateTime.UtcNow
        };

        // Multi-series bar chart
        _templates["grouped_bar"] = new ChartTemplate
        {
            Id = "grouped_bar",
            Name = "Grouped Bar Chart",
            Description = "Multiple series displayed as grouped bars",
            Category = "Bar",
            ChartConfiguration = new ChartConfiguration
            {
                ChartType = ChartType.Bar,
                Width = 800,
                Height = 600
            },
            CreatedAt = DateTime.UtcNow
        };

        // Distribution heatmap
        _templates["heatmap"] = new ChartTemplate
        {
            Id = "heatmap",
            Name = "Heatmap",
            Description = "2D distribution with color intensity",
            Category = "Heatmap",
            ChartConfiguration = new ChartConfiguration
            {
                ChartType = ChartType.Heatmap,
                Width = 600,
                Height = 600
            },
            CreatedAt = DateTime.UtcNow
        };

        // Pie chart
        _templates["pie"] = new ChartTemplate
        {
            Id = "pie",
            Name = "Pie Chart",
            Description = "Proportion visualization",
            Category = "Pie",
            ChartConfiguration = new ChartConfiguration
            {
                ChartType = ChartType.Pie,
                Width = 600,
                Height = 600
            },
            CreatedAt = DateTime.UtcNow
        };

        _logger.LogInformation("Default templates initialized: {Count}", _templates.Count);
    }
}
