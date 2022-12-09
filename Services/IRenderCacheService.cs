// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SkiaSharpChartEngine.Services;

/// <summary>
/// Interface for caching rendered chart data
/// </summary>
public interface IRenderCacheService
{
    byte[]? Get(string cacheKey);

    void Set(string cacheKey, byte[] imageData);

    void Remove(string cacheKey);

    void Clear();

    int GetCacheSize();

    bool Contains(string cacheKey);

    IEnumerable<string> GetAllKeys();
}
