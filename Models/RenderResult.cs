// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;

namespace SkiaSharpChartEngine.Models;

/// <summary>
/// Result of a chart rendering operation
/// </summary>
public class RenderResult
{
    public string ChartId { get; set; } = string.Empty;

    public bool Success { get; set; }

    public byte[]? ImageData { get; set; }

    public long? FileSizeBytes { get; set; }

    public string? OutputPath { get; set; }

    public ExportFormat? ExportFormat { get; set; }

    public DateTime RenderedAt { get; set; } = DateTime.UtcNow;

    public long RenderTimeMs { get; set; }

    public string? ErrorMessage { get; set; }

    public Exception? Exception { get; set; }

    public Dictionary<string, object>? Metadata { get; set; }

    public RenderResult() { }

    public RenderResult(string chartId)
    {
        ChartId = chartId;
    }

    public RenderResult(string chartId, bool success)
    {
        ChartId = chartId;
        Success = success;
    }

    public static RenderResult CreateSuccess(string chartId, byte[] imageData, long renderTimeMs, ExportFormat format)
    {
        return new RenderResult
        {
            ChartId = chartId,
            Success = true,
            ImageData = imageData,
            FileSizeBytes = imageData.Length,
            ExportFormat = format,
            RenderTimeMs = renderTimeMs,
            RenderedAt = DateTime.UtcNow
        };
    }

    public static RenderResult CreateSuccess(string chartId, string outputPath, long renderTimeMs, ExportFormat format)
    {
        var fileInfo = new FileInfo(outputPath);
        return new RenderResult
        {
            ChartId = chartId,
            Success = true,
            OutputPath = outputPath,
            FileSizeBytes = fileInfo.Exists ? fileInfo.Length : null,
            ExportFormat = format,
            RenderTimeMs = renderTimeMs,
            RenderedAt = DateTime.UtcNow
        };
    }

    public static RenderResult CreateFailure(string chartId, string errorMessage, Exception? exception = null)
    {
        return new RenderResult
        {
            ChartId = chartId,
            Success = false,
            ErrorMessage = errorMessage,
            Exception = exception,
            RenderedAt = DateTime.UtcNow
        };
    }

    public override string ToString()
        => $"RenderResult(ChartId={ChartId}, Success={Success}, RenderTimeMs={RenderTimeMs})";
}
