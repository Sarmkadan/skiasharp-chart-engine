// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SkiaSharpChartEngine.Constants;
using SkiaSharpChartEngine.Exceptions;

namespace SkiaSharpChartEngine.Models;

/// <summary>
/// Represents a complete chart model, including its data series, visual configuration,
/// metadata, and associated tags. This is the primary model used by the <see cref="ChartEngine"/>
/// for rendering and data processing operations.
/// </summary>
public class Chart
{
    private readonly List<ChartSeries> _series = new();
    private readonly ChartConfiguration _configuration;
    private string _id = Guid.NewGuid().ToString();

    [Required]
    public string Id
    {
        get => _id;
        set => _id = value ?? throw new ArgumentNullException(nameof(value));
    }

    public ChartType Type { get; set; } = ChartType.LineChart;

    public List<ChartSeries> Series
    {
        get => _series;
    }

    public ChartConfiguration Configuration
    {
        get => _configuration;
    }

    /// <summary>
    /// Alias for <see cref="ChartConfiguration.Title"/> exposed directly on the chart for
    /// convenience.
    /// </summary>
    public string Title
    {
        get => _configuration.Title;
        set => _configuration.Title = value;
    }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ModifiedAt { get; set; }

    public string? CreatedBy { get; set; }

    public bool IsTemplate { get; set; } = false;

    public Dictionary<string, object>? Tags { get; set; }

    public Chart(string? id = null)
    {
        if (id != null)
            Id = id;
        _configuration = new ChartConfiguration();
    }

    public Chart(ChartType type, string? id = null) : this(id)
    {
        Type = type;
    }

    public Chart(ChartConfiguration configuration, string? id = null) : this(id)
    {
        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));
        _configuration = configuration;
    }

    public void AddSeries(ChartSeries series)
    {
        if (series == null)
            throw new ArgumentNullException(nameof(series));

        if (_series.Count >= ChartConstants.MaxSeries)
            throw new InvalidOperationException($"Maximum number of series ({ChartConstants.MaxSeries}) reached");

        _series.Add(series);
        ModifiedAt = DateTime.UtcNow;
    }

    public void RemoveSeries(int index)
    {
        if (index < 0 || index >= _series.Count)
            throw new IndexOutOfRangeException($"Series index {index} is out of range");

        _series.RemoveAt(index);
        ModifiedAt = DateTime.UtcNow;
    }

    public void RemoveSeriesByName(string name)
    {
        var series = _series.FirstOrDefault(s => s.Name == name);
        if (series == null)
            throw new KeyNotFoundException($"Series '{name}' not found");

        _series.Remove(series);
        ModifiedAt = DateTime.UtcNow;
    }

    public ChartSeries? GetSeriesByName(string name)
    {
        return _series.FirstOrDefault(s => s.Name == name);
    }

    public int GetSeriesCount() => _series.Count;

    public int GetTotalDataPoints() => _series.Sum(s => s.GetDataPointCount());

    public void ClearAllSeries()
    {
        _series.Clear();
        ModifiedAt = DateTime.UtcNow;
    }

    public (double minX, double maxX, double minY, double maxY) GetDataBounds()
    {
        if (_series.Count == 0 || _series.All(s => s.GetDataPointCount() == 0))
            return (0, 1, 0, 1);

        var allPoints = _series.SelectMany(s => s.DataPoints).ToList();
        if (!allPoints.Any())
            return (0, 1, 0, 1);

        return (
            allPoints.Min(p => p.X),
            allPoints.Max(p => p.X),
            allPoints.Min(p => p.Y),
            allPoints.Max(p => p.Y)
        );
    }

    public bool ValidateForRendering()
    {
        try
        {
            Configuration.Validate();

            if (_series.Count == 0)
                throw new InvalidChartDataException("Chart must contain at least one series");

            if (_series.All(s => s.GetDataPointCount() == 0))
                throw new InvalidChartDataException("Chart must contain at least one data point");

            return true;
        }
        catch (Exception ex)
        {
            throw new InvalidChartDataException($"Chart validation failed: {ex.Message}", ex);
        }
    }

    public Chart Clone()
    {
        var cloned = new Chart(Type, Id)
        {
            CreatedAt = CreatedAt,
            ModifiedAt = ModifiedAt,
            CreatedBy = CreatedBy,
            IsTemplate = IsTemplate,
            Tags = Tags != null ? new Dictionary<string, object>(Tags) : null
        };

        foreach (var series in _series)
        {
            cloned.AddSeries(series.Clone());
        }

        return cloned;
    }

    public override string ToString() => $"Chart(Id={Id}, Type={Type}, Series={_series.Count})";
}
