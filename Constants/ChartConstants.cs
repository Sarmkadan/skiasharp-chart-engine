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
    public const int SubtitleFontSize = 16;
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

/// <summary>
/// Identifies the logical region of a chart canvas that a screen coordinate falls within.
/// Returned by <see cref="IInteractivityService.HitTest"/> to enable region-specific
/// interactions such as axis clicks, legend toggles, and plot-area drill-down.
/// </summary>
public enum ChartRegion
{
    /// <summary>The coordinate is inside the plot area where data series are drawn.</summary>
    PlotArea = 0,
    /// <summary>The coordinate is in the horizontal axis band below the plot area.</summary>
    XAxis = 1,
    /// <summary>The coordinate is in the vertical axis band to the left of the plot area.</summary>
    YAxis = 2,
    /// <summary>The coordinate is over the chart title area.</summary>
    Title = 3,
    /// <summary>The coordinate is over the legend.</summary>
    Legend = 4,
    /// <summary>The coordinate is outside all named regions (e.g. chart padding).</summary>
    Outside = 5
}
/// <list type="bullet">
///   <item><term>Linear</term><description>Default. Color interpolates linearly between min and max.</description></item>
///   <item><term>Logarithmic</term><description>Applies log10 compression before interpolation. Useful when data has outliers or a wide dynamic range.</description></item>
///   <item><term>Quantile</term><description>Ranks values and maps by percentile. Each colour band covers an equal number of data points, giving the best contrast for skewed distributions.</description></item>
/// </list>
/// </summary>
public enum HeatmapColorScale
{
    Linear = 0,
    Logarithmic = 1,
    Quantile = 2
}
