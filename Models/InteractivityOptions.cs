// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.ComponentModel.DataAnnotations;

namespace SkiaSharpChartEngine.Models;

/// <summary>
/// Configuration options controlling interactive tooltip appearance and hit-testing behaviour.
/// </summary>
public class TooltipOptions
{
    private float _padding = 8f;
    private float _borderRadius = 6f;
    private float _fontSize = 12f;
    private float _hitRadius = 16f;

    /// <summary>Gets or sets whether tooltips are enabled for this chart.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Gets or sets the tooltip background color in hex format (e.g. <c>#FFFFFF</c>).</summary>
    [StringLength(9, MinimumLength = 7)]
    public string BackgroundColor { get; set; } = "#FFFFFF";

    /// <summary>Gets or sets the tooltip border color in hex format.</summary>
    [StringLength(9, MinimumLength = 7)]
    public string BorderColor { get; set; } = "#CCCCCC";

    /// <summary>Gets or sets the tooltip text color in hex format.</summary>
    [StringLength(9, MinimumLength = 7)]
    public string TextColor { get; set; } = "#333333";

    /// <summary>Gets or sets the internal padding in pixels around tooltip text.</summary>
    [Range(0f, 64f)]
    public float Padding
    {
        get => _padding;
        set
        {
            if (value < 0f || value > 64f)
                throw new ArgumentOutOfRangeException(nameof(value), "Padding must be between 0 and 64.");
            _padding = value;
        }
    }

    /// <summary>Gets or sets the corner radius of the tooltip box in pixels.</summary>
    [Range(0f, 32f)]
    public float BorderRadius
    {
        get => _borderRadius;
        set
        {
            if (value < 0f || value > 32f)
                throw new ArgumentOutOfRangeException(nameof(value), "BorderRadius must be between 0 and 32.");
            _borderRadius = value;
        }
    }

    /// <summary>Gets or sets the tooltip text font size in pixels.</summary>
    [Range(8f, 48f)]
    public float FontSize
    {
        get => _fontSize;
        set
        {
            if (value < 8f || value > 48f)
                throw new ArgumentOutOfRangeException(nameof(value), "FontSize must be between 8 and 48.");
            _fontSize = value;
        }
    }

    /// <summary>
    /// Gets or sets the maximum pixel distance from a data point that still registers a tooltip hit.
    /// </summary>
    [Range(4f, 128f)]
    public float HitRadius
    {
        get => _hitRadius;
        set
        {
            if (value < 4f || value > 128f)
                throw new ArgumentOutOfRangeException(nameof(value), "HitRadius must be between 4 and 128.");
            _hitRadius = value;
        }
    }

    /// <summary>
    /// Gets or sets an optional content template. Supports <c>{x}</c>, <c>{y}</c>, <c>{label}</c>,
    /// and <c>{series}</c> placeholders.
    /// </summary>
    [StringLength(1000)]
    public string? ContentTemplate { get; set; }

    /// <summary>Gets or sets the tooltip border stroke width in pixels.</summary>
    [Range(0f, 8f)]
    public float BorderWidth { get; set; } = 1f;

    /// <summary>Gets or sets the drop-shadow opacity (0 = none, 1 = fully opaque).</summary>
    [Range(0f, 1f)]
    public float ShadowOpacity { get; set; } = 0.15f;

    /// <summary>Creates a deep copy of this <see cref="TooltipOptions"/>.</summary>
    public TooltipOptions Clone() => new()
    {
        Enabled          = Enabled,
        BackgroundColor  = BackgroundColor,
        BorderColor      = BorderColor,
        TextColor        = TextColor,
        Padding          = _padding,
        BorderRadius     = _borderRadius,
        FontSize         = _fontSize,
        HitRadius        = _hitRadius,
        ContentTemplate  = ContentTemplate,
        BorderWidth      = BorderWidth,
        ShadowOpacity    = ShadowOpacity
    };
}

/// <summary>
/// The result of a tooltip hit-test: which data point (if any) is nearest to the pointer.
/// </summary>
public class TooltipHitResult
{
    /// <summary>Gets or sets whether a data point was found within the configured hit radius.</summary>
    public bool IsHit { get; set; }

    /// <summary>Gets or sets the nearest matched data point, or <c>null</c> when there is no hit.</summary>
    public DataPoint? DataPoint { get; set; }

    /// <summary>Gets or sets the series that owns the matched data point.</summary>
    public ChartSeries? Series { get; set; }

    /// <summary>Gets or sets the zero-based index of the matched series within the chart.</summary>
    public int SeriesIndex { get; set; } = -1;

    /// <summary>Gets or sets the Euclidean canvas distance in pixels between the pointer and the data point.</summary>
    public double DistancePx { get; set; } = double.MaxValue;

    /// <summary>Gets or sets the canvas X coordinate (pixels) of the matched data point.</summary>
    public float CanvasX { get; set; }

    /// <summary>Gets or sets the canvas Y coordinate (pixels) of the matched data point.</summary>
    public float CanvasY { get; set; }

    /// <summary>Gets or sets the formatted tooltip string ready for rendering.</summary>
    public string TooltipText { get; set; } = string.Empty;

    /// <summary>Singleton representing a hit-test with no match.</summary>
    public static readonly TooltipHitResult Miss = new() { IsHit = false };
}

/// <summary>
/// Tracks the current zoom level and pan offset for an interactive chart viewport.
/// Zoom and pan values are combined to derive a <see cref="VisibleXRange"/> and
/// <see cref="VisibleYRange"/> over the chart's data coordinate space.
/// </summary>
public class ViewportState
{
    private double _zoomX = 1.0;
    private double _zoomY = 1.0;

    /// <summary>Gets or sets the horizontal zoom factor (1.0 = no zoom, &gt;1.0 = zoomed in).</summary>
    [Range(0.01, 100.0)]
    public double ZoomX
    {
        get => _zoomX;
        set
        {
            if (value < 0.01 || value > 100.0)
                throw new ArgumentOutOfRangeException(nameof(value), "ZoomX must be between 0.01 and 100.");
            _zoomX = value;
        }
    }

    /// <summary>Gets or sets the vertical zoom factor (1.0 = no zoom, &gt;1.0 = zoomed in).</summary>
    [Range(0.01, 100.0)]
    public double ZoomY
    {
        get => _zoomY;
        set
        {
            if (value < 0.01 || value > 100.0)
                throw new ArgumentOutOfRangeException(nameof(value), "ZoomY must be between 0.01 and 100.");
            _zoomY = value;
        }
    }

    /// <summary>Gets or sets the horizontal pan offset expressed in data-coordinate units.</summary>
    public double PanX { get; set; }

    /// <summary>Gets or sets the vertical pan offset expressed in data-coordinate units.</summary>
    public double PanY { get; set; }

    /// <summary>Gets the visible X-axis data range after applying current zoom and pan.</summary>
    public (double Min, double Max) VisibleXRange { get; set; }

    /// <summary>Gets the visible Y-axis data range after applying current zoom and pan.</summary>
    public (double Min, double Max) VisibleYRange { get; set; }

    /// <summary>Gets whether the viewport is in its default state (no zoom, no pan).</summary>
    public bool IsDefault => _zoomX == 1.0 && _zoomY == 1.0 && PanX == 0.0 && PanY == 0.0;

    /// <summary>Resets zoom and pan to defaults, clearing the cached visible ranges.</summary>
    public void Reset()
    {
        _zoomX = 1.0;
        _zoomY = 1.0;
        PanX = 0.0;
        PanY = 0.0;
        VisibleXRange = default;
        VisibleYRange = default;
    }

    /// <summary>Creates a deep copy of this <see cref="ViewportState"/>.</summary>
    public ViewportState Clone() => new()
    {
        ZoomX         = _zoomX,
        ZoomY         = _zoomY,
        PanX          = PanX,
        PanY          = PanY,
        VisibleXRange = VisibleXRange,
        VisibleYRange = VisibleYRange
    };
}
