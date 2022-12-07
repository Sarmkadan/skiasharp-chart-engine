// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SkiaSharp;

namespace SkiaSharpChartEngine.Utilities;

/// <summary>
/// Manages chart themes and color schemes.
/// Provides predefined themes and custom theme creation.
/// </summary>
public class ThemeManager
{
    private readonly Dictionary<string, ChartTheme> _themes;
    private readonly ILogger<ThemeManager> _logger;
    private string _currentTheme;

    public ThemeManager(ILogger<ThemeManager> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _themes = new Dictionary<string, ChartTheme>(StringComparer.OrdinalIgnoreCase);
        _initializeDefaultThemes();
    }

    // Register a theme
    public void RegisterTheme(string name, ChartTheme theme)
    {
        if (string.IsNullOrWhiteSpace(name) || theme == null)
            return;

        _themes[name] = theme;
        _logger.LogInformation("Theme registered: {ThemeName}", name);
    }

    // Get theme by name
    public ChartTheme GetTheme(string name)
    {
        if (_themes.TryGetValue(name, out var theme))
        {
            return theme;
        }

        _logger.LogWarning("Theme not found: {ThemeName}", name);
        return _themes["default"]; // Fallback to default
    }

    // Set current theme
    public void SetCurrentTheme(string name)
    {
        if (_themes.ContainsKey(name))
        {
            _currentTheme = name;
            _logger.LogInformation("Current theme changed to: {ThemeName}", name);
        }
        else
        {
            _logger.LogWarning("Cannot set theme, not found: {ThemeName}", name);
        }
    }

    // Get current theme
    public ChartTheme GetCurrentTheme()
    {
        return GetTheme(_currentTheme ?? "default");
    }

    // Get available themes
    public IEnumerable<string> GetAvailableThemes() => _themes.Keys;

    private void _initializeDefaultThemes()
    {
        // Light theme
        _themes["light"] = new ChartTheme
        {
            Name = "Light",
            BackgroundColor = SKColors.White,
            ForegroundColor = SKColors.Black,
            GridColor = SKColors.LightGray,
            AxisColor = SKColors.Black,
            TextColor = SKColors.Black,
            SeriesColors = new[]
            {
                SKColors.RoyalBlue, SKColors.OrangeRed, SKColors.ForestGreen,
                SKColors.Gold, SKColors.Violet, SKColors.Turquoise
            }
        };

        // Dark theme
        _themes["dark"] = new ChartTheme
        {
            Name = "Dark",
            BackgroundColor = new SKColor(30, 30, 30),
            ForegroundColor = SKColors.White,
            GridColor = new SKColor(80, 80, 80),
            AxisColor = SKColors.White,
            TextColor = SKColors.White,
            SeriesColors = new[]
            {
                new SKColor(100, 150, 255), new SKColor(255, 100, 100), new SKColor(100, 200, 100),
                new SKColor(255, 200, 100), new SKColor(200, 100, 255), new SKColor(100, 200, 200)
            }
        };

        // Default theme
        _themes["default"] = _themes["light"];
        _currentTheme = "default";

        _logger.LogInformation("Default themes initialized");
    }
}

/// <summary>
/// Represents a chart theme with colors and styling.
/// </summary>
public class ChartTheme
{
    public string Name { get; set; }
    public SKColor BackgroundColor { get; set; }
    public SKColor ForegroundColor { get; set; }
    public SKColor GridColor { get; set; }
    public SKColor AxisColor { get; set; }
    public SKColor TextColor { get; set; }
    public SKColor[] SeriesColors { get; set; }
    public float FontSize { get; set; } = 12f;
    public float LineWidth { get; set; } = 2f;
}
