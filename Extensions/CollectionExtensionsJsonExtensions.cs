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
/// Provides JSON serialization and deserialization extensions for collections and dictionaries.
/// </summary>
public static class CollectionExtensionsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private static readonly JsonSerializerOptions _jsonOptionsIndented = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes an enumerable collection to a JSON string.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="source">The collection to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the collection.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is null.</exception>
    public static string ToJson<T>(this IEnumerable<T> source, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(source);
        return JsonSerializer.Serialize(source, indented ? _jsonOptionsIndented : _jsonOptions);
    }

    /// <summary>
    /// Deserializes a JSON string to a collection of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized collection, or null if the JSON is null or whitespace.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is malformed or cannot be deserialized to the specified type.</exception>
    public static ICollection<T>? FromJsonToCollection<T>(this string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<ICollection<T>>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a collection of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized collection if successful; otherwise, null.</param>
    /// <returns>True if the JSON is valid and deserialization succeeded; otherwise, false.</returns>
    public static bool TryFromJsonToCollection<T>(this string? json, out ICollection<T>? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return true;
        }

        try
        {
            value = JsonSerializer.Deserialize<ICollection<T>>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>
    /// Serializes a dictionary to a JSON string.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
    /// <param name="dictionary">The dictionary to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the dictionary.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="dictionary"/> is null.</exception>
    public static string ToJson<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, bool indented = false)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(dictionary);
        return JsonSerializer.Serialize(dictionary, indented ? _jsonOptionsIndented : _jsonOptions);
    }

    /// <summary>
    /// Deserializes a JSON string to a dictionary.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized dictionary, or null if the JSON is null or whitespace.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is malformed or cannot be deserialized to the specified dictionary type.</exception>
    public static Dictionary<TKey, TValue>? FromJsonToDictionary<TKey, TValue>(this string? json)
        where TKey : notnull
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<Dictionary<TKey, TValue>>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a dictionary.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized dictionary if successful; otherwise, null.</param>
    /// <returns>True if the JSON is valid and deserialization succeeded; otherwise, false.</returns>
    public static bool TryFromJsonToDictionary<TKey, TValue>(this string? json, out Dictionary<TKey, TValue>? value)
        where TKey : notnull
    {
        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return true;
        }

        try
        {
            value = JsonSerializer.Deserialize<Dictionary<TKey, TValue>>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
