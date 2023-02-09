// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.Models;

/// <summary>
/// Provides extension methods for <see cref="TooltipOptions"/> to simplify common tooltip configuration scenarios.
/// </summary>
public static class TooltipOptionsExtensions
{
    /// <summary>
    /// Creates a new <see cref="TooltipOptions"/> instance with default values for a light theme.
    /// </summary>
    /// <returns>A new instance with light theme colors.</returns>
    public static TooltipOptions WithLightTheme(this TooltipOptions options)
    {
        options.BackgroundColor = "#FFFFFF";
        options.BorderColor = "#CCCCCC";
        options.TextColor = "#333333";
        options.ShadowOpacity = 0.15f;
        return options;
    }

    /// <summary>
    /// Creates a new <see cref="TooltipOptions"/> instance with default values for a dark theme.
    /// </summary>
    /// <returns>A new instance with dark theme colors.</returns>
    public static TooltipOptions WithDarkTheme(this TooltipOptions options)
    {
        options.BackgroundColor = "#1E1E1E";
        options.BorderColor = "#404040";
        options.TextColor = "#E0E0E0";
        options.ShadowOpacity = 0.3f;
        return options;
    }

    /// <summary>
    /// Creates a new <see cref="TooltipOptions"/> instance configured for high contrast accessibility.
    /// </summary>
    /// <returns>A new instance with high contrast colors.</returns>
    public static TooltipOptions WithHighContrast(this TooltipOptions options)
    {
        options.BackgroundColor = "#000000";
        options.BorderColor = "#FFFFFF";
        options.TextColor = "#FFFFFF";
        options.ShadowOpacity = 0.0f;
        options.BorderWidth = 2f;
        return options;
    }

    /// <summary>
    /// Creates a new <see cref="TooltipOptions"/> instance with increased size for better visibility.
    /// </summary>
    /// <param name="fontSize">The font size to use (default: 14px).</param>
    /// <param name="padding">The padding to use (default: 12px).</param>
    /// <param name="borderRadius">The border radius to use (default: 8px).</param>
    /// <returns>A new instance with increased dimensions.</returns>
    public static TooltipOptions WithLargeSize(this TooltipOptions options, float fontSize = 14f, float padding = 12f, float borderRadius = 8f)
    {
        options.FontSize = fontSize;
        options.Padding = padding;
        options.BorderRadius = borderRadius;
        return options;
    }

    /// <summary>
    /// Creates a new <see cref="TooltipOptions"/> instance with reduced size for compact displays.
    /// </summary>
    /// <param name="fontSize">The font size to use (default: 10px).</param>
    /// <param name="padding">The padding to use (default: 6px).</param>
    /// <param name="borderRadius">The border radius to use (default: 4px).</param>
    /// <returns>A new instance with reduced dimensions.</returns>
    public static TooltipOptions WithSmallSize(this TooltipOptions options, float fontSize = 10f, float padding = 6f, float borderRadius = 4f)
    {
        options.FontSize = fontSize;
        options.Padding = padding;
        options.BorderRadius = borderRadius;
        return options;
    }

    /// <summary>
    /// Creates a copy of the current <see cref="TooltipOptions"/> with all properties copied.
    /// </summary>
    /// <param name="options">The source tooltip options.</param>
    /// <returns>A deep copy of the tooltip options.</returns>
    public static TooltipOptions Clone(this TooltipOptions options)
    {
        return new TooltipOptions
        {
            Enabled = options.Enabled,
            BackgroundColor = options.BackgroundColor,
            BorderColor = options.BorderColor,
            TextColor = options.TextColor,
            Padding = options.Padding,
            BorderRadius = options.BorderRadius,
            FontSize = options.FontSize,
            HitRadius = options.HitRadius,
            ContentTemplate = options.ContentTemplate,
            BorderWidth = options.BorderWidth,
            ShadowOpacity = options.ShadowOpacity
        };
    }

    /// <summary>
    /// Determines whether the tooltip should be visible based on the current configuration.
    /// </summary>
    /// <param name="options">The tooltip options.</param>
    /// <returns>True if the tooltip is enabled and should be visible; otherwise, false.</returns>
    public static bool ShouldShow(this TooltipOptions options)
    {
        return options.Enabled && options.BackgroundColor != null && options.TextColor != null;
    }

    /// <summary>
    /// Sets the tooltip colors to match the specified background color while maintaining contrast.
    /// </summary>
    /// <param name="backgroundColor">The background color in hex format (e.g., "#FFFFFF").</param>
    /// <returns>The updated tooltip options.</returns>
    public static TooltipOptions WithBackgroundColor(this TooltipOptions options, string backgroundColor)
    {
        options.BackgroundColor = backgroundColor;

        // Auto-adjust text color based on background brightness
        if (backgroundColor != null)
        {
            var color = backgroundColor.Trim('#');
            if (color.Length == 6 && int.TryParse(color.Substring(0, 2), System.Globalization.NumberStyles.HexNumber, null, out var r) &&
                int.TryParse(color.Substring(2, 2), System.Globalization.NumberStyles.HexNumber, null, out var g) &&
                int.TryParse(color.Substring(4, 2), System.Globalization.NumberStyles.HexNumber, null, out var b))
            {
                var brightness = (r * 299 + g * 587 + b * 114) / 1000;
                options.TextColor = brightness > 128 ? "#000000" : "#FFFFFF";
            }
        }

        return options;
    }
}