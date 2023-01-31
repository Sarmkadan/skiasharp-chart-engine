// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Buffers;
using System.Collections.Frozen;
using System.Security.Cryptography;
using System.Text;
using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.Caching;

/// <summary>
/// Builds consistent cache keys for chart renders.
/// Ensures the same key for identical render parameters.
/// </summary>
public static class CacheKeyBuilder
{
    private const string KeyPrefix = "chart_";
    private const string RenderKeyPrefix = "render_";
    private const string SeriesKeyPrefix = "series_";

    // Pre-computed lookup avoids ToString() + ToLowerInvariant() on every call.
    private static readonly FrozenDictionary<ChartType, string> _configKeysByType =
        Enum.GetValues<ChartType>()
            .ToFrozenDictionary(
                t => t,
                t => $"config_{t.ToString().ToLowerInvariant()}");

    /// <summary>
    /// Builds cache key for a chart object.
    /// </summary>
    public static string BuildChartKey(string chartId)
    {
        if (string.IsNullOrEmpty(chartId))
            throw new ArgumentException("Chart ID cannot be empty", nameof(chartId));

        return $"{KeyPrefix}{chartId}";
    }

    /// <summary>
    /// Builds cache key for rendered chart output.
    /// Includes dimensions and format to ensure unique cache entries.
    /// </summary>
    public static string BuildRenderKey(string chartId, int width, int height, float dpi, string format)
    {
        if (string.IsNullOrEmpty(chartId))
            throw new ArgumentException("Chart ID cannot be empty", nameof(chartId));

        var keyComponents = $"{chartId}_{width}x{height}_{dpi}dpi_{format}";
        return $"{RenderKeyPrefix}{HashKey(keyComponents)}";
    }

    /// <summary>
    /// Builds cache key for chart series.
    /// </summary>
    public static string BuildSeriesKey(string chartId, string seriesName)
    {
        if (string.IsNullOrEmpty(chartId))
            throw new ArgumentException("Chart ID cannot be empty", nameof(chartId));

        var keyComponents = $"{chartId}_{seriesName}";
        return $"{SeriesKeyPrefix}{HashKey(keyComponents)}";
    }

    /// <summary>
    /// Builds cache key for configuration using a pre-computed frozen lookup.
    /// </summary>
    public static string BuildConfigurationKey(ChartType chartType) =>
        _configKeysByType.TryGetValue(chartType, out var key)
            ? key
            : $"config_{chartType.ToString().ToLowerInvariant()}";

    /// <summary>
    /// Builds cache key for axis calculations.
    /// </summary>
    public static string BuildAxisKey(float minValue, float maxValue, int tickCount)
    {
        var keyComponents = $"axis_{minValue}_{maxValue}_{tickCount}";
        return HashKey(keyComponents);
    }

    /// <summary>
    /// Builds cache key for color palette.
    /// </summary>
    public static string BuildPaletteKey(string paletteName)
    {
        if (string.IsNullOrEmpty(paletteName))
            throw new ArgumentException("Palette name cannot be empty", nameof(paletteName));

        return $"palette_{paletteName.ToLowerInvariant()}";
    }

    /// <summary>
    /// Extracts chart ID from cache key.
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
    /// Hashes a key to create a shorter, consistent identifier.
    /// Uses SHA256.HashData static method and stackalloc to avoid per-call allocations.
    /// </summary>
    private static string HashKey(string keyComponent)
    {
        int maxByteCount = Encoding.UTF8.GetMaxByteCount(keyComponent.Length);
        byte[]? rented = null;
        Span<byte> inputBuffer = maxByteCount <= 512
            ? stackalloc byte[maxByteCount]
            : (rented = ArrayPool<byte>.Shared.Rent(maxByteCount));

        try
        {
            int written = Encoding.UTF8.GetBytes(keyComponent, inputBuffer);

            Span<byte> hashBuffer = stackalloc byte[32]; // SHA-256 output is always 32 bytes
            SHA256.HashData(inputBuffer[..written], hashBuffer);
            return Convert.ToBase64String(hashBuffer)[..16];
        }
        finally
        {
            if (rented != null)
                ArrayPool<byte>.Shared.Return(rented);
        }
    }

    /// <summary>
    /// Builds a pattern for cache invalidation (e.g., "chart_abc123_*").
    /// </summary>
    public static string BuildInvalidationPattern(string chartId)
    {
        if (string.IsNullOrEmpty(chartId))
            throw new ArgumentException("Chart ID cannot be empty", nameof(chartId));

        return $"{KeyPrefix}{chartId}_*";
    }

    /// <summary>
    /// Validates cache key format.
    /// </summary>
    public static bool IsValidCacheKey(string key)
    {
        if (string.IsNullOrEmpty(key))
            return false;

        return key.Length > 0 && char.IsLetterOrDigit(key[0]);
    }

    /// <summary>
    /// Creates a composite key for multiple parameters without LINQ intermediate allocations.
    /// </summary>
    public static string BuildCompositeKey(params object?[] parameters)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < parameters.Length; i++)
        {
            if (parameters[i] is null) continue;
            if (sb.Length > 0) sb.Append('_');
            sb.Append(parameters[i]!.ToString());
        }
        return HashKey(sb.ToString());
    }
}
