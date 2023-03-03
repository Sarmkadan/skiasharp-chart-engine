// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SkiaSharpChartEngine.Extensions;

/// <summary>
/// Provides JSON serialization and deserialization extensions for CollectionExtensions.
/// </summary>
public static class CollectionExtensionsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };


    private static readonly JsonSerializerOptions _jsonOptionsIndented = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    /// <summary>
    /// Serializes CollectionExtensions class metadata to a JSON string.
    /// </summary>
    /// <param name="_">Dummy parameter for extension method syntax.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the CollectionExtensions type information.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="_"/> is null.</exception>
    public static string ToJson(this object? _, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(_);

        // Since CollectionExtensions is a static class, we serialize its type information
        // This provides a consistent API that can be used for serialization/deserialization patterns
        var data = new
        {
            TypeName = nameof(CollectionExtensions),
            Methods = new[]
            {
                "Batch<T>",
                "IsNullOrEmpty<T>",
                "GetOrDefault<T>",
                "Shuffle<T>",
                "GetRandom<T>",
                "DistinctBy<T, TKey>",
                "ContainsAny<T>",
                "FindDuplicates<T>",
                "ToDictionaryWithDuplicates<TKey, TValue>",
                "SumBy<T>",
                "AverageOrDefault<T>",
                "ChunkBy<T>",
                "Interleave<T>"
            }
        };

        return JsonSerializer.Serialize(data, indented ? _jsonOptionsIndented : _jsonOptions);
    }

    /// <summary>
    /// Deserializes a JSON string to a marker indicating CollectionExtensions type.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>Always returns null since CollectionExtensions is a static class and cannot be instantiated.</returns>
    /// <exception cref="JsonException">Thrown when the JSON is malformed.</exception>
    public static object? FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        // Deserialize to verify JSON is valid, but return null since we can't instantiate a static class
        _ = JsonSerializer.Deserialize<object>(json, _jsonOptions);
        return null;
    }

    /// <summary>
    /// Attempts to deserialize a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives null (CollectionExtensions is a static class and cannot be instantiated).</param>
    /// <returns>True if the JSON is valid; otherwise, false.</returns>
    public static bool TryFromJson(string json, out object? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return true;
        }

        try
        {
            _ = JsonSerializer.Deserialize<object>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}