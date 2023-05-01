// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Extension methods for ExportOptions to provide additional functionality
// =============================================================================

using System;
using System.IO;

namespace SkiaSharpChartEngine.Models;

/// <summary>
/// Extension methods for <see cref="ExportOptions"/> providing additional functionality
/// for configuring and manipulating export options.
/// </summary>
public static class ExportOptionsExtensions
{
    /// <summary>
    /// Creates a new <see cref="ExportOptions"/> with the same settings as the current instance
    /// but with the specified DPI value.
    /// </summary>
    /// <param name="options">The source <see cref="ExportOptions"/> instance.</param>
    /// <param name="dpi">The new DPI value.</param>
    /// <returns>A new <see cref="ExportOptions"/> instance with updated DPI.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    public static ExportOptions WithDPI(this ExportOptions options, int dpi)
    {
        ArgumentNullException.ThrowIfNull(options);

        var clone = options.Clone();
        clone.DPI = dpi;
        return clone;
    }

    /// <summary>
    /// Creates a new <see cref="ExportOptions"/> with the same settings as the current instance
    /// but with the specified Quality value.
    /// </summary>
    /// <param name="options">The source <see cref="ExportOptions"/> instance.</param>
    /// <param name="quality">The new Quality value (0.1 to 1.0).</param>
    /// <returns>A new <see cref="ExportOptions"/> instance with updated Quality.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="quality"/> is outside the valid range of 0.1 to 1.0.</exception>
    public static ExportOptions WithQuality(this ExportOptions options, float quality)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (quality < 0.1f || quality > 1.0f)
        {
            throw new ArgumentOutOfRangeException(nameof(quality), quality, "Quality must be between 0.1 and 1.0");
        }

        var clone = options.Clone();
        clone.Quality = quality;
        return clone;
    }

    /// <summary>
    /// Creates a new <see cref="ExportOptions"/> with the same settings as the current instance
    /// but with the specified OutputDirectory.
    /// </summary>
    /// <param name="options">The source <see cref="ExportOptions"/> instance.</param>
    /// <param name="outputDirectory">The new output directory path.</param>
    /// <returns>A new <see cref="ExportOptions"/> instance with updated OutputDirectory.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    public static ExportOptions WithOutputDirectory(this ExportOptions options, string outputDirectory)
    {
        ArgumentNullException.ThrowIfNull(options);

        var clone = options.Clone();
        clone.OutputDirectory = outputDirectory;
        return clone;
    }

    /// <summary>
    /// Creates a new <see cref="ExportOptions"/> with the same settings as the current instance
    /// but with the specified filename (without extension).
    /// </summary>
    /// <param name="options">The source <see cref="ExportOptions"/> instance.</param>
    /// <param name="filename">The new filename without extension.</param>
    /// <returns>A new <see cref="ExportOptions"/> instance with updated filename.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="filename"/> is <see langword="null"/>, empty, or consists only of whitespace.</exception>
    public static ExportOptions WithFilename(this ExportOptions options, string filename)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(filename))
        {
            throw new ArgumentException("Filename cannot be null or whitespace", nameof(filename));
        }

        var clone = options.Clone();
        clone.Filename = filename.Trim();
        return clone;
    }

    /// <summary>
    /// Creates a new <see cref="ExportOptions"/> with the same settings as the current instance
    /// but with the specified format.
    /// </summary>
    /// <param name="options">The source <see cref="ExportOptions"/> instance.</param>
    /// <param name="format">The new export format.</param>
    /// <returns>A new <see cref="ExportOptions"/> instance with updated format.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    public static ExportOptions WithFormat(this ExportOptions options, ExportFormat format)
    {
        ArgumentNullException.ThrowIfNull(options);

        var clone = options.Clone();
        clone.Format = format;
        return clone;
    }

    /// <summary>
    /// Gets the full file path with the specified suffix appended to the filename.
    /// </summary>
    /// <param name="options">The <see cref="ExportOptions"/> instance.</param>
    /// <param name="suffix">The suffix to append to the filename.</param>
    /// <returns>The full file path with suffix.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="suffix"/> is <see langword="null"/>, empty, or consists only of whitespace.</exception>
    public static string GetFullPathWithSuffix(this ExportOptions options, string suffix)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(suffix))
        {
            throw new ArgumentException("Suffix cannot be null or whitespace", nameof(suffix));
        }

        var directory = string.IsNullOrWhiteSpace(options.OutputDirectory)
            ? Environment.CurrentDirectory
            : options.OutputDirectory;

        var extension = ExportOptions.GetFileExtension(options.Format);
        var filenameWithSuffix = $"{options.Filename}_{suffix.Trim()}.{extension}";
        return Path.Combine(directory, filenameWithSuffix);
    }

    /// <summary>
    /// Determines whether the export format is a raster format (PNG, JPEG, WEBP).
    /// </summary>
    /// <param name="options">The <see cref="ExportOptions"/> instance.</param>
    /// <returns><see langword="true"/> if the format is a raster format; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    public static bool IsRasterFormat(this ExportOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return options.Format is ExportFormat.PNG or ExportFormat.JPEG or ExportFormat.WEBP;
    }

    /// <summary>
    /// Determines whether the export format is a vector format (SVG, PDF).
    /// </summary>
    /// <param name="options">The <see cref="ExportOptions"/> instance.</param>
    /// <returns><see langword="true"/> if the format is a vector format; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    public static bool IsVectorFormat(this ExportOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return options.Format is ExportFormat.SVG or ExportFormat.PDF;
    }

    /// <summary>
    /// Creates a new <see cref="ExportOptions"/> instance with default settings for high-quality PNG export.
    /// </summary>
    /// <param name="filename">The output filename without extension.</param>
    /// <returns>A new <see cref="ExportOptions"/> instance configured for high-quality PNG.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="filename"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="filename"/> is <see langword="null"/>, empty, or consists only of whitespace.</exception>
    public static ExportOptions ForHighQualityPNG(string filename)
    {
        ArgumentNullException.ThrowIfNull(filename);

        if (string.IsNullOrWhiteSpace(filename))
        {
            throw new ArgumentException("Filename cannot be null or whitespace", nameof(filename));
        }

        return new ExportOptions
        {
            Filename = filename.Trim(),
            Format = ExportFormat.PNG,
            DPI = 300,
            Quality = 1.0f,
            EmbedFonts = true,
            PreserveAspectRatio = true
        };
    }

    /// <summary>
    /// Creates a new <see cref="ExportOptions"/> instance with default settings for web-optimized JPEG export.
    /// </summary>
    /// <param name="filename">The output filename without extension.</param>
    /// <returns>A new <see cref="ExportOptions"/> instance configured for web JPEG.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="filename"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="filename"/> is <see langword="null"/>, empty, or consists only of whitespace.</exception>
    public static ExportOptions ForWebJPEG(string filename)
    {
        ArgumentNullException.ThrowIfNull(filename);

        if (string.IsNullOrWhiteSpace(filename))
        {
            throw new ArgumentException("Filename cannot be null or whitespace", nameof(filename));
        }

        return new ExportOptions
        {
            Filename = filename.Trim(),
            Format = ExportFormat.JPEG,
            DPI = 96,
            Quality = 0.85f,
            EmbedFonts = false,
            PreserveAspectRatio = true
        };
    }

    /// <summary>
    /// Creates a new <see cref="ExportOptions"/> instance with default settings for SVG vector export.
    /// </summary>
    /// <param name="filename">The output filename without extension.</param>
    /// <returns>A new <see cref="ExportOptions"/> instance configured for SVG.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="filename"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="filename"/> is <see langword="null"/>, empty, or consists of only whitespace.</exception>
    public static ExportOptions ForVectorSVG(string filename)
    {
        ArgumentNullException.ThrowIfNull(filename);

        if (string.IsNullOrWhiteSpace(filename))
        {
            throw new ArgumentException("Filename cannot be null or whitespace", nameof(filename));
        }

        return new ExportOptions
        {
            Filename = filename.Trim(),
            Format = ExportFormat.SVG,
            DPI = 72,
            Quality = 1.0f,
            EmbedFonts = true,
            PreserveAspectRatio = true
        };
    }
}