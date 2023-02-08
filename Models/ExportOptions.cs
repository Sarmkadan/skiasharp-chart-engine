// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.IO;
using System.ComponentModel.DataAnnotations;
using SkiaSharpChartEngine.Constants;
using SkiaSharpChartEngine.Exceptions;

namespace SkiaSharpChartEngine.Models;

/// <summary>
/// Options for exporting charts to various formats
/// </summary>
public class ExportOptions
{
    private string _filename = "chart";
    private ExportFormat _format = ExportFormat.PNG;

    [Required]
    [StringLength(255, MinimumLength = 1)]
    public string Filename
    {
        get => _filename;
        set => _filename = value?.Trim() ?? throw new ArgumentNullException(nameof(value));
    }

    public ExportFormat Format
    {
        get => _format;
        set => _format = value;
    }

    [Range(10, 600)]
    public int DPI { get; set; } = ChartConstants.DefaultExportDPI;

    [Range(0.1f, 1.0f)]
    public float Quality { get; set; } = ChartConstants.DefaultExportQuality;

    public bool EmbedFonts { get; set; } = false;

    public bool PreserveAspectRatio { get; set; } = true;

    public string? OutputDirectory { get; set; }

    /// <summary>
    /// Alias for <see cref="Filename"/> matching the naming used by callers that treat
    /// export options as a plain file-system destination.
    /// </summary>
    public string FileName
    {
        get => Filename;
        set => Filename = value;
    }

    /// <summary>
    /// Alias for <see cref="OutputDirectory"/> matching the naming used by callers that
    /// treat export options as a plain file-system destination.
    /// </summary>
    public string? DirectoryPath
    {
        get => OutputDirectory;
        set => OutputDirectory = value;
    }

    public Dictionary<string, object>? CustomFormatOptions { get; set; }

    public ExportOptions() { }

    public ExportOptions(string filename, ExportFormat format)
    {
        Filename = filename;
        Format = format;
    }

    public ExportOptions(string filename, ExportFormat format, int dpi, float quality)
    {
        Filename = filename;
        Format = format;
        DPI = dpi;
        Quality = quality;
    }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Filename))
            throw new InvalidOperationException("Filename cannot be empty");

        if (DPI < 10 || DPI > 600)
            throw new InvalidOperationException("DPI must be between 10 and 600");

        if (Quality < 0.1f || Quality > 1.0f)
            throw new InvalidOperationException("Quality must be between 0.1 and 1.0");

        if (!Enum.IsDefined(typeof(ExportFormat), Format))
            throw new InvalidOperationException("Invalid export format");
    }

    public string GetFullPath()
    {
        var directory = string.IsNullOrWhiteSpace(OutputDirectory) ? Environment.CurrentDirectory : OutputDirectory;
        var extension = GetFileExtension(Format);
        return Path.Combine(directory, $"{Filename}.{extension}");
    }

    public static string GetFileExtension(ExportFormat format) => format switch
    {
        ExportFormat.PNG => "png",
        ExportFormat.SVG => "svg",
        ExportFormat.PDF => "pdf",
        ExportFormat.JPEG => "jpg",
        ExportFormat.WEBP => "webp",
        _ => throw new UnsupportedExportFormatException(format.ToString())
    };

    public ExportOptions Clone()
    {
        return new ExportOptions
        {
            Filename = Filename,
            Format = Format,
            DPI = DPI,
            Quality = Quality,
            EmbedFonts = EmbedFonts,
            PreserveAspectRatio = PreserveAspectRatio,
            OutputDirectory = OutputDirectory,
            CustomFormatOptions = CustomFormatOptions != null ? new Dictionary<string, object>(CustomFormatOptions) : null
        };
    }

    public override string ToString() => $"ExportOptions(Filename={Filename}, Format={Format}, DPI={DPI})";
}
