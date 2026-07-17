// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace SkiaSharpChartEngine.Events;

/// <summary>
/// Provides System.Text.Json serialization extensions for <see cref="ChartEventPublisher"/>
/// </summary>
public static class ChartEventPublisherJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver()
    };

    /// <summary>
    /// Serializes the <see cref="ChartEventPublisher"/> instance to a JSON string
    /// </summary>
    /// <param name="value">The <see cref="ChartEventPublisher"/> instance to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability</param>
    /// <returns>A JSON string representation of the <see cref="ChartEventPublisher"/></returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/></exception>
    public static string ToJson(this ChartEventPublisher value, bool indented = false) =>
        value is null
            ? throw new ArgumentNullException(nameof(value))
            : JsonSerializer.Serialize(
                value,
                indented
                    ? new JsonSerializerOptions(_jsonSerializerOptions) { WriteIndented = true }
                    : _jsonSerializerOptions);

    /// <summary>
    /// Deserializes a JSON string to a <see cref="ChartEventPublisher"/> instance
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <returns>A <see cref="ChartEventPublisher"/> instance, or <see langword="null"/> if the JSON is empty or whitespace</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/></exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized</exception>
    public static ChartEventPublisher? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        return string.IsNullOrWhiteSpace(json)
            ? null
            : JsonSerializer.Deserialize<ChartEventPublisher>(json, _jsonSerializerOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="ChartEventPublisher"/> instance
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <param name="value">Receives the deserialized <see cref="ChartEventPublisher"/> instance, or <see langword="null"/> on failure</param>
    /// <returns><see langword="true"/> if deserialization succeeded; otherwise, <see langword="false"/></returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/></exception>
    public static bool TryFromJson(string json, out ChartEventPublisher? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<ChartEventPublisher>(json, _jsonSerializerOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}