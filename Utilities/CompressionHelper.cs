// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SkiaSharpChartEngine.Utilities;

/// <summary>
/// Provides compression/decompression utilities for chart data and exports.
/// Supports GZip compression for efficient storage and transmission.
/// </summary>
public class CompressionHelper
{
    private readonly ILogger<CompressionHelper> _logger;

    public CompressionHelper(ILogger<CompressionHelper> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // Compress byte array using GZip
    public async Task<byte[]> CompressAsync(byte[] data)
    {
        try
        {
            if (data == null || data.Length == 0)
                return data;

            using var inputStream = new MemoryStream(data);
            using var outputStream = new MemoryStream();

            using (var gzipStream = new GZipStream(outputStream, CompressionMode.Compress))
            {
                await inputStream.CopyToAsync(gzipStream);
            }

            var compressed = outputStream.ToArray();
            var compressionRatio = (1.0 - (double)compressed.Length / data.Length) * 100;

            _logger.LogDebug("Data compressed: {OriginalSize}B -> {CompressedSize}B ({CompressionRatio:F1}%)",
                data.Length, compressed.Length, compressionRatio);

            return compressed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error compressing data");
            return data;
        }
    }

    // Decompress byte array
    public async Task<byte[]> DecompressAsync(byte[] compressedData)
    {
        try
        {
            if (compressedData == null || compressedData.Length == 0)
                return compressedData;

            using var inputStream = new MemoryStream(compressedData);
            using var outputStream = new MemoryStream();

            using (var gzipStream = new GZipStream(inputStream, CompressionMode.Decompress))
            {
                await gzipStream.CopyToAsync(outputStream);
            }

            var decompressed = outputStream.ToArray();
            _logger.LogDebug("Data decompressed: {CompressedSize}B -> {OriginalSize}B",
                compressedData.Length, decompressed.Length);

            return decompressed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decompressing data");
            return compressedData;
        }
    }

    // Compress string data
    public async Task<string> CompressStringAsync(string text)
    {
        try
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var data = System.Text.Encoding.UTF8.GetBytes(text);
            var compressed = await CompressAsync(data);
            return Convert.ToBase64String(compressed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error compressing string");
            return text;
        }
    }

    // Decompress string data
    public async Task<string> DecompressStringAsync(string compressedBase64)
    {
        try
        {
            if (string.IsNullOrEmpty(compressedBase64))
                return compressedBase64;

            var compressedData = Convert.FromBase64String(compressedBase64);
            var decompressed = await DecompressAsync(compressedData);
            return System.Text.Encoding.UTF8.GetString(decompressed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decompressing string");
            return compressedBase64;
        }
    }

    // Calculate compression ratio
    public double CalculateCompressionRatio(int originalSize, int compressedSize)
    {
        if (originalSize == 0)
            return 0;

        return (1.0 - (double)compressedSize / originalSize) * 100;
    }

    // Check if data is compressed (magic bytes)
    public bool IsGZipCompressed(byte[] data)
    {
        return data != null && data.Length >= 2 && data[0] == 0x1F && data[1] == 0x8B;
    }
}
