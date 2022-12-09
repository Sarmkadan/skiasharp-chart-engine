// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.API.Requests;

/// <summary>
/// Request model for creating a new chart
/// Validates required fields and chart configuration
/// </summary>
public class CreateChartRequest
{
    /// <summary>
    /// Chart title
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Type of chart (Line, Bar, Pie, Heatmap)
    /// </summary>
    public ChartType ChartType { get; set; }

    /// <summary>
    /// Chart configuration with display settings
    /// </summary>
    public ChartConfiguration? Configuration { get; set; }

    /// <summary>
    /// Data series for the chart
    /// </summary>
    public List<ChartSeries>? Series { get; set; }

    /// <summary>
    /// Validates the request before processing
    /// </summary>
    public bool IsValid(out string? errorMessage)
    {
        errorMessage = null;

        if (string.IsNullOrWhiteSpace(Title))
        {
            errorMessage = "Chart title is required and cannot be empty";
            return false;
        }

        if (Title.Length > 200)
        {
            errorMessage = "Chart title must not exceed 200 characters";
            return false;
        }

        return true;
    }
}

/// <summary>
/// Request model for updating an existing chart
/// Allows partial updates of chart properties
/// </summary>
public class UpdateChartRequest
{
    /// <summary>
    /// Optional new chart title
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Optional new configuration
    /// </summary>
    public ChartConfiguration? Configuration { get; set; }

    /// <summary>
    /// Optional updated data series
    /// </summary>
    public List<ChartSeries>? Series { get; set; }

    /// <summary>
    /// Validates the request - ensures at least one field is being updated
    /// </summary>
    public bool IsValid(out string? errorMessage)
    {
        errorMessage = null;

        if (string.IsNullOrEmpty(Title) && Configuration == null && (Series == null || Series.Count == 0))
        {
            errorMessage = "At least one field must be provided for update";
            return false;
        }

        if (!string.IsNullOrEmpty(Title) && Title.Length > 200)
        {
            errorMessage = "Chart title must not exceed 200 characters";
            return false;
        }

        return true;
    }
}

/// <summary>
/// Request model for rendering a chart
/// Specifies output format and dimensions
/// </summary>
public class RenderChartRequest
{
    /// <summary>
    /// Chart ID to render
    /// </summary>
    public required string ChartId { get; set; }

    /// <summary>
    /// Output width in pixels
    /// </summary>
    public int Width { get; set; } = 800;

    /// <summary>
    /// Output height in pixels
    /// </summary>
    public int Height { get; set; } = 600;

    /// <summary>
    /// DPI for high-resolution output
    /// </summary>
    public float Dpi { get; set; } = 96f;

    /// <summary>
    /// Validates rendering parameters
    /// </summary>
    public bool IsValid(out string? errorMessage)
    {
        errorMessage = null;

        if (string.IsNullOrWhiteSpace(ChartId))
        {
            errorMessage = "Chart ID is required";
            return false;
        }

        if (Width < 100 || Width > 4000)
        {
            errorMessage = "Width must be between 100 and 4000 pixels";
            return false;
        }

        if (Height < 100 || Height > 4000)
        {
            errorMessage = "Height must be between 100 and 4000 pixels";
            return false;
        }

        if (Dpi < 72f || Dpi > 600f)
        {
            errorMessage = "DPI must be between 72 and 600";
            return false;
        }

        return true;
    }
}

/// <summary>
/// Request model for batch chart rendering
/// Allows rendering multiple charts in a single request
/// </summary>
public class BatchRenderRequest
{
    /// <summary>
    /// List of chart IDs to render
    /// </summary>
    public required List<string> ChartIds { get; set; }

    /// <summary>
    /// Common rendering settings for all charts
    /// </summary>
    public RenderChartRequest? RenderSettings { get; set; }

    /// <summary>
    /// Validates batch request
    /// </summary>
    public bool IsValid(out string? errorMessage)
    {
        errorMessage = null;

        if (ChartIds == null || ChartIds.Count == 0)
        {
            errorMessage = "At least one chart ID must be provided";
            return false;
        }

        if (ChartIds.Count > 100)
        {
            errorMessage = "Maximum 100 charts can be rendered in a single batch";
            return false;
        }

        return true;
    }
}
