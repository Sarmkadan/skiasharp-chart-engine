// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.ComponentModel.DataAnnotations;

namespace SkiaSharpChartEngine.Models;

/// <summary>
/// Template for creating charts with predefined settings
/// </summary>
public class ChartTemplate
{
    private string _name = string.Empty;
    private string _description = string.Empty;

    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Name
    {
        get => _name;
        set => _name = value?.Trim() ?? throw new ArgumentNullException(nameof(value));
    }

    [StringLength(1000)]
    public string Description
    {
        get => _description;
        set => _description = value?.Trim() ?? string.Empty;
    }

    public string TemplateId { get; set; } = Guid.NewGuid().ToString();

    public ChartType ChartType { get; set; } = ChartType.LineChart;

    public ChartConfiguration BaseConfiguration { get; set; } = new();

    public List<ChartSeries> DefaultSeries { get; set; } = new();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string? CreatedBy { get; set; }

    public Dictionary<string, object>? CustomProperties { get; set; }

    public ChartTemplate() { }

    public ChartTemplate(string name, ChartType chartType)
    {
        Name = name;
        ChartType = chartType;
    }

    public Chart CreateChartFromTemplate()
    {
        var chart = new Chart(ChartType)
        {
            CreatedAt = DateTime.UtcNow,
            CreatedBy = CreatedBy
        };

        // Copy configuration
        var config = BaseConfiguration.Clone();
        foreach (var prop in config.GetType().GetProperties())
        {
            var value = prop.GetValue(config);
            typeof(ChartConfiguration).GetProperty(prop.Name)?.SetValue(chart.Configuration, value);
        }

        // Copy default series
        foreach (var series in DefaultSeries)
        {
            chart.AddSeries(series.Clone());
        }

        return chart;
    }

    public ChartTemplate Clone()
    {
        return new ChartTemplate(Name, ChartType)
        {
            TemplateId = TemplateId,
            Description = Description,
            BaseConfiguration = BaseConfiguration.Clone(),
            DefaultSeries = DefaultSeries.Select(s => s.Clone()).ToList(),
            CreatedAt = CreatedAt,
            CreatedBy = CreatedBy,
            CustomProperties = CustomProperties != null ? new Dictionary<string, object>(CustomProperties) : null
        };
    }

    public override string ToString() => $"ChartTemplate(Name={Name}, Type={ChartType})";
}
