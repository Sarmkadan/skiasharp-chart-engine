// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Security.Cryptography;
using System.Text;
using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.Caching;

/// <summary>
/// Builds consistent cache keys for chart renders
/// Ensures same key for identical render parameters
/// </summary>
public static class CacheKeyBuilder
{
    private const string KeyPrefix = "chart_";
    private const string RenderKeyPrefix = "render_";
    private const string SeriesKeyPrefix = "series_";

    /// <summary>
    /// Builds cache key for a chart object
    /// </summary>
    public static string BuildChartKey(string chartId)
    {
        if (string.IsNullOrEmpty(chartId))
            throw new ArgumentException("Chart ID cannot be empty", nameof(chartId));

        return $"{KeyPrefix}{chartId}";
    }

    /// <summary>
    /// Builds cache key for rendered chart output
    /// Includes dimensions and format to ensure unique cache entries
    /// </summary>
    public static string BuildRenderKey(string chartId, int width, int height, float dpi, string format)
    {
        if (string.IsNullOrEmpty(chartId))
            throw new ArgumentException("Chart ID cannot be empty", nameof(chartId));

        var keyComponents = $"{chartId}_{width}x{height}_{dpi}dpi_{format}";
        return $"{RenderKeyPrefix}{HashKey(keyComponents)}";
    }

    /// <summary>
    /// Builds cache key for chart series
    /// </summary>
    public static string BuildSeriesKey(string chartId, string seriesName)
    {
        if (string.IsNullOrEmpty(chartId))
            throw new ArgumentException("Chart ID cannot be empty", nameof(chartId));

        var keyComponents = $"{chartId}_{seriesName}";
        return $"{SeriesKeyPrefix}{HashKey(keyComponents)}";
    }

    /// <summary>
    /// Builds cache key for configuration
    /// </summary>
    public static string BuildConfigurationKey(ChartType chartType)
    {
        return $"config_{chartType.ToString().ToLowerInvariant()}";
    }

    /// <summary>
    /// Builds cache key for axis calculations
    /// </summary>
    public static string BuildAxisKey(float minValue, float maxValue, int tickCount)
    {
        var keyComponents = $"axis_{minValue}_{maxValue}_{tickCount}";
        return HashKey(keyComponents);
    }

    /// <summary>
    /// Builds cache key for color palette
    /// </summary>
    public static string BuildPaletteKey(string paletteName)
    {
        if (string.IsNullOrEmpty(paletteName))
            throw new ArgumentException("Palette name cannot be empty", nameof(paletteName));

        return $"palette_{paletteName.ToLowerInvariant()}";
    }

    /// <summary>
    /// Extracts chart ID from cache key
    /// </summary>
    public static string? ExtractChartIdFromKey(string cacheKey)
    {
        if (string.IsNullOrEmpty(cacheKey))
            return null;

        if (cacheKey.StartsWith(KeyPrefix))
        {
            return cacheKey[KeyPrefix.Length..];
        }

        return null;
    }

    /// <summary>
    /// Hashes a key to create a shorter, consistent identifier
    /// </summary>
    private static string HashKey(string keyComponent)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(keyComponent));
        return Convert.ToBase64String(hashedBytes)[..16];
    }

    /// <summary>
    /// Builds a pattern for cache invalidation (e.g., "chart_abc123_*")
    /// </summary>
    public static string BuildInvalidationPattern(string chartId)
    {
        if (string.IsNullOrEmpty(chartId))
            throw new ArgumentException("Chart ID cannot be empty", nameof(chartId));

        return $"{KeyPrefix}{chartId}_*";
    }

    /// <summary>
    /// Validates cache key format
    /// </summary>
    public static bool IsValidCacheKey(string key)
    {
        if (string.IsNullOrEmpty(key))
            return false;

        return key.Length > 0 && char.IsLetterOrDigit(key[0]);
    }

    /// <summary>
    /// Creates a composite key for multiple parameters
    /// </summary>
    public static string BuildCompositeKey(params object?[] parameters)
    {
        var parts = parameters
            .Where(p => p != null)
            .Select(p => p!.ToString() ?? string.Empty)
            .ToArray();

        return HashKey(string.Join("_", parts));
    }
}
