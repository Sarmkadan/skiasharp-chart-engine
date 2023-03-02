// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.Examples;

/// <summary>
/// Represents a quick start example configuration for chart generation demonstrations
/// </summary>
public sealed record QuickStartExample
{
    /// <summary>
    /// Gets the example name
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the chart type for this example
    /// </summary>
    public ChartType ChartType { get; init; } = ChartType.LineChart;

    /// <summary>
    /// Gets the example description
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets the collection of data series for this example
    /// </summary>
    public List<ChartSeries> Series { get; init; } = new();

    /// <summary>
    /// Gets the chart configuration options
    /// </summary>
    public ChartConfiguration Configuration { get; init; } = new();

    /// <summary>
    /// Gets the example category
    /// </summary>
    public string Category { get; init; } = "General";

    /// <summary>
    /// Gets the difficulty level of the example
    /// </summary>
    public int DifficultyLevel { get; init; } = 1;
}

/// <summary>
/// Provides System.Text.Json serialization helpers for QuickStartExample data model
/// </summary>
public static class QuickStartExampleJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Serializes a QuickStartExample instance to a JSON string
    /// </summary>
    /// <param name="value">The QuickStartExample instance to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability</param>
    /// <returns>A JSON string representation of the QuickStartExample</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static string ToJson(this QuickStartExample value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions)
            {
                WriteIndented = true
            }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a QuickStartExample instance
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <returns>A QuickStartExample instance, or null if deserialization fails</returns>
    /// <exception cref="ArgumentException">Thrown when json is null or whitespace</exception>
    public static QuickStartExample? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            return JsonSerializer.Deserialize<QuickStartExample>(json, _jsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a QuickStartExample instance
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <param name="value">Output parameter receiving the deserialized instance</param>
    /// <returns>True if deserialization succeeds; otherwise, false</returns>
    /// <exception cref="ArgumentException">Thrown when json is null or whitespace</exception>
    public static bool TryFromJson(string json, out QuickStartExample? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<QuickStartExample>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}