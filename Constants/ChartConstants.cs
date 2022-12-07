// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SkiaSharpChartEngine.Constants;

/// <summary>
/// Global constants for chart engine operations
/// </summary>
public static class ChartConstants
{
    // Default dimensions
    public const int DefaultChartWidth = 800;
    public const int DefaultChartHeight = 600;
    public const int MinimumChartWidth = 200;
    public const int MinimumChartHeight = 150;
    public const int MaximumChartWidth = 4000;
    public const int MaximumChartHeight = 4000;

    // Margins and padding
    public const int DefaultMarginTop = 40;
    public const int DefaultMarginBottom = 60;
    public const int DefaultMarginLeft = 80;
    public const int DefaultMarginRight = 40;
    public const int DefaultPadding = 10;

    // Font sizes
    public const int TitleFontSize = 24;
    public const int LegendFontSize = 12;
    public const int AxisLabelFontSize = 11;
    public const int TickLabelFontSize = 10;

    // Colors (ARGB hex format)
    public const string DefaultBackgroundColor = "#FFFFFF";
    public const string DefaultGridColor = "#E0E0E0";
    public const string DefaultAxisColor = "#000000";
    public const string DefaultTextColor = "#333333";

    // Performance limits
    public const int MaxDataPoints = 100000;
    public const int MaxSeries = 50;
    public const int CacheSize = 100;

    // Export settings
    public const int DefaultExportDPI = 96;
    public const float DefaultExportQuality = 0.95f;

    // Animation defaults
    public const int DefaultAnimationDurationMs = 500;
    public const int DefaultAnimationFramerate = 60;
}

/// <summary>
/// Export format enumeration
/// </summary>
public enum ExportFormat
{
    PNG = 0,
    SVG = 1,
    PDF = 2,
    JPEG = 3,
    WEBP = 4
}

/// <summary>
/// Chart type enumeration
/// </summary>
public enum ChartType
{
    LineChart = 0,
    BarChart = 1,
    PieChart = 2,
    HeatmapChart = 3,
    AreaChart = 4,
    ScatterChart = 5,
    ColumnChart = 6
}

/// <summary>
/// Data point state enumeration
/// </summary>
public enum DataPointState
{
    Normal = 0,
    Highlighted = 1,
    Selected = 2,
    Disabled = 3
}

/// <summary>
/// Axis scale type enumeration
/// </summary>
public enum AxisScaleType
{
    Linear = 0,
    Logarithmic = 1,
    Categorical = 2,
    DateTimeLinear = 3
}
