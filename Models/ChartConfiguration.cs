// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.ComponentModel.DataAnnotations;
using SkiaSharpChartEngine.Constants;

namespace SkiaSharpChartEngine.Models;

/// <summary>
/// Defines the visual and structural configuration for rendering a chart,
/// including dimensions, layout margins, axes scaling, color schemes,
/// animation settings, and export options.
/// </summary>
public class ChartConfiguration
{
    private int _width = ChartConstants.DefaultChartWidth;
    private int _height = ChartConstants.DefaultChartHeight;
    private string _title = "Chart";

    [Range(ChartConstants.MinimumChartWidth, ChartConstants.MaximumChartWidth)]
    public int Width
    {
        get => _width;
        set
        {
            if (value < ChartConstants.MinimumChartWidth || value > ChartConstants.MaximumChartWidth)
                throw new ArgumentOutOfRangeException(nameof(value), $"Width must be between {ChartConstants.MinimumChartWidth} and {ChartConstants.MaximumChartWidth}");
            _width = value;
        }
    }

    [Range(ChartConstants.MinimumChartHeight, ChartConstants.MaximumChartHeight)]
    public int Height
    {
        get => _height;
        set
        {
            if (value < ChartConstants.MinimumChartHeight || value > ChartConstants.MaximumChartHeight)
                throw new ArgumentOutOfRangeException(nameof(value), $"Height must be between {ChartConstants.MinimumChartHeight} and {ChartConstants.MaximumChartHeight}");
            _height = value;
        }
    }

    [StringLength(500, MinimumLength = 1)]
    public string Title
    {
        get => _title;
        set => _title = value?.Trim() ?? throw new ArgumentNullException(nameof(value));
    }

    public string? Subtitle { get; set; }

    public string? XAxisLabel { get; set; }

    public string? YAxisLabel { get; set; }

    public string BackgroundColor { get; set; } = ChartConstants.DefaultBackgroundColor;

    public string GridColor { get; set; } = ChartConstants.DefaultGridColor;

    public string AxisColor { get; set; } = ChartConstants.DefaultAxisColor;

    public string TextColor { get; set; } = ChartConstants.DefaultTextColor;

    public int MarginTop { get; set; } = ChartConstants.DefaultMarginTop;

    public int MarginBottom { get; set; } = ChartConstants.DefaultMarginBottom;

    public int MarginLeft { get; set; } = ChartConstants.DefaultMarginLeft;

    public int MarginRight { get; set; } = ChartConstants.DefaultMarginRight;

    public bool ShowLegend { get; set; } = true;

    public bool ShowGrid { get; set; } = true;

    public bool ShowAxisLabels { get; set; } = true;

    public bool ShowDataPointLabels { get; set; } = false;

    public AxisScaleType XAxisScaleType { get; set; } = AxisScaleType.Linear;

    public AxisScaleType YAxisScaleType { get; set; } = AxisScaleType.Linear;

    public double? XAxisMin { get; set; }

    public double? XAxisMax { get; set; }

    public double? YAxisMin { get; set; }

    public double? YAxisMax { get; set; }

    public bool EnableAnimation { get; set; } = true;

    public int AnimationDurationMs { get; set; } = ChartConstants.DefaultAnimationDurationMs;

    public ExportFormat? DefaultExportFormat { get; set; } = ExportFormat.PNG;

    public int ExportDPI { get; set; } = ChartConstants.DefaultExportDPI;

    public float ExportQuality { get; set; } = ChartConstants.DefaultExportQuality;

    public bool AntiAlias { get; set; } = true;

    /// <summary>
    /// Specifies the color scale interpolation mode used by heatmap charts.
    /// Defaults to <see cref="HeatmapColorScale.Linear"/>.
    /// </summary>
    public HeatmapColorScale HeatmapColorScale { get; set; } = HeatmapColorScale.Linear;

    public Dictionary<string, object>? CustomSettings { get; set; }

    public void Validate()
    {
        if (Width < ChartConstants.MinimumChartWidth)
            throw new InvalidOperationException($"Chart width cannot be less than {ChartConstants.MinimumChartWidth}");

        if (Height < ChartConstants.MinimumChartHeight)
            throw new InvalidOperationException($"Chart height cannot be less than {ChartConstants.MinimumChartHeight}");

        if (MarginTop < 0 || MarginBottom < 0 || MarginLeft < 0 || MarginRight < 0)
            throw new InvalidOperationException("Margins cannot be negative");

        if (string.IsNullOrWhiteSpace(Title))
            throw new InvalidOperationException("Chart title cannot be empty");
    }

    public ChartConfiguration Clone()
    {
        return new ChartConfiguration
        {
            Width = Width,
            Height = Height,
            Title = Title,
            Subtitle = Subtitle,
            XAxisLabel = XAxisLabel,
            YAxisLabel = YAxisLabel,
            BackgroundColor = BackgroundColor,
            GridColor = GridColor,
            AxisColor = AxisColor,
            TextColor = TextColor,
            MarginTop = MarginTop,
            MarginBottom = MarginBottom,
            MarginLeft = MarginLeft,
            MarginRight = MarginRight,
            ShowLegend = ShowLegend,
            ShowGrid = ShowGrid,
            ShowAxisLabels = ShowAxisLabels,
            ShowDataPointLabels = ShowDataPointLabels,
            XAxisScaleType = XAxisScaleType,
            YAxisScaleType = YAxisScaleType,
            XAxisMin = XAxisMin,
            XAxisMax = XAxisMax,
            YAxisMin = YAxisMin,
            YAxisMax = YAxisMax,
            EnableAnimation = EnableAnimation,
            AnimationDurationMs = AnimationDurationMs,
            DefaultExportFormat = DefaultExportFormat,
            ExportDPI = ExportDPI,
            ExportQuality = ExportQuality,
            AntiAlias = AntiAlias,
            HeatmapColorScale = HeatmapColorScale,
            CustomSettings = CustomSettings != null ? new Dictionary<string, object>(CustomSettings) : null
        };
    }
}
