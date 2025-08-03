// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.IO;

namespace SkiaSharpChartEngine.Utilities;

/// <summary>
/// Helper methods for bitmap/image operations
/// Provides utilities for image data handling and validation
/// </summary>
public static class BitmapHelper
{
    /// <summary>
    /// Validates if byte array is a valid PNG image
    /// Checks PNG magic bytes: 89 50 4E 47
    /// </summary>
    public static bool IsPngValid(byte[] data)
    {
        if (data == null || data.Length < 8)
            return false;

        // PNG magic bytes
        return data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47;
    }

    /// <summary>
    /// Validates if byte array is a valid JPEG image
    /// Checks JPEG magic bytes: FF D8 FF
    /// </summary>
    public static bool IsJpegValid(byte[] data)
    {
        if (data == null || data.Length < 3)
            return false;

        return data[0] == 0xFF && data[1] == 0xD8 && data[2] == 0xFF;
    }

    /// <summary>
    /// Gets image dimensions from PNG data
    /// </summary>
    public static (int Width, int Height)? GetPngDimensions(byte[] data)
    {
        try
        {
            if (!IsPngValid(data) || data.Length < 24)
                return null;

            // Width is at bytes 16-19 (big-endian)
            int width = (data[16] << 24) | (data[17] << 16) | (data[18] << 8) | data[19];
            // Height is at bytes 20-23 (big-endian)
            int height = (data[20] << 24) | (data[21] << 16) | (data[22] << 8) | data[23];

            return (width, height);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Calculates file size for target dimensions and DPI
    /// Estimates rough output size based on resolution
    /// </summary>
    public static long EstimateFileSize(int width, int height, float dpi, string format)
    {
        // Rough estimation: width * height * bytes_per_pixel
        // PNG typically 3-4 bytes per pixel with compression
        // JPEG typically 1-2 bytes per pixel depending on quality

        var pixelCount = (long)width * height;
        var bytesPerPixel = format.ToLowerInvariant() switch
        {
            "png" => 3,      // 3-4 bytes average with compression
            "jpeg" or "jpg" => 1,  // 1-2 bytes with compression
            "bmp" => 4,      // 4 bytes per pixel, no compression
            "svg" => 2,      // Typically smaller than raster
            _ => 3
        };

        var estimatedSize = pixelCount * bytesPerPixel;
        return Math.Max(1024, estimatedSize / 2); // Assume 50% compression
    }

    /// <summary>
    /// Detects image format from magic bytes
    /// </summary>
    public static string? DetectFormat(byte[] data)
    {
        if (data == null || data.Length < 4)
            return null;

        // PNG: 89 50 4E 47
        if (data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47)
            return "png";

        // JPEG: FF D8 FF
        if (data[0] == 0xFF && data[1] == 0xD8 && data[2] == 0xFF)
            return "jpeg";

        // BMP: 42 4D
        if (data[0] == 0x42 && data[1] == 0x4D)
            return "bmp";

        // GIF: 47 49 46 38
        if (data[0] == 0x47 && data[1] == 0x49 && data[2] == 0x46 && data[3] == 0x38)
            return "gif";

        // WEBP: RIFF ... WEBP
        if (data.Length >= 12 && data[0] == 0x52 && data[1] == 0x49 && data[2] == 0x46 && data[3] == 0x46)
        {
            if (data[8] == 0x57 && data[9] == 0x45 && data[10] == 0x42 && data[11] == 0x50)
                return "webp";
        }

        return null;
    }

    /// <summary>
    /// Calculates DPI from PPI (pixels per inch)
    /// </summary>
    public static int GetDpiFromPpi(int ppi)
    {
        return ppi <= 0 ? 96 : ppi;
    }

    /// <summary>
    /// Converts DPI to PPI
    /// </summary>
    public static float ConvertDpiToPpi(float dpi)
    {
        return Math.Max(72f, dpi);
    }

    /// <summary>
    /// Calculates physical size from pixel dimensions and DPI
    /// </summary>
    public static (double WidthInches, double HeightInches) GetPhysicalSize(int widthPixels, int heightPixels, float dpi)
    {
        if (dpi <= 0) dpi = 96;
        return (widthPixels / dpi, heightPixels / dpi);
    }

    /// <summary>
    /// Calculates pixel dimensions from physical size and DPI
    /// </summary>
    public static (int WidthPixels, int HeightPixels) GetPixelDimensions(
        double widthInches,
        double heightInches,
        float dpi)
    {
        if (dpi <= 0) dpi = 96;
        return ((int)(widthInches * dpi), (int)(heightInches * dpi));
    }

    /// <summary>
    /// Validates image dimensions are within reasonable bounds
    /// </summary>
    public static bool ValidateDimensions(int width, int height)
    {
        return width >= 1 && width <= 10000 && height >= 1 && height <= 10000;
    }

    /// <summary>
    /// Calculates aspect ratio
    /// </summary>
    public static float GetAspectRatio(int width, int height)
    {
        if (height == 0) return 0;
        return (float)width / height;
    }

    /// <summary>
    /// Scales dimensions while maintaining aspect ratio
    /// </summary>
    public static (int ScaledWidth, int ScaledHeight) ScaleMaintainingRatio(
        int originalWidth,
        int originalHeight,
        int maxWidth,
        int maxHeight)
    {
        var aspectRatio = GetAspectRatio(originalWidth, originalHeight);

        if (originalWidth > maxWidth)
        {
            originalWidth = maxWidth;
            originalHeight = (int)(maxWidth / aspectRatio);
        }

        if (originalHeight > maxHeight)
        {
            originalHeight = maxHeight;
            originalWidth = (int)(maxHeight * aspectRatio);
        }

        return (originalWidth, originalHeight);
    }
}
