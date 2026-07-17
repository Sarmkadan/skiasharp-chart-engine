// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SkiaSharpChartEngine.API.Controllers;

/// <summary>
/// Provides System.Text.Json serialization extensions for ExportController
/// </summary>
public static class ExportControllerJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    /// <summary>
    /// Serializes the ExportController instance to a JSON string
    /// </summary>
    /// <param name="value">The ExportController instance to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability</param>
    /// <returns>A JSON string representation of the ExportController</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
    /// <exception cref="JsonException">Thrown when serialization fails</exception>
    public static string ToJson(this ExportController value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to an ExportController instance
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <returns>The deserialized ExportController instance, or null if JSON is null or empty</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null</exception>
    /// <exception cref="JsonException">Thrown when the JSON is malformed or cannot be deserialized</exception>
    public static ExportController? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<ExportController>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to an ExportController instance
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <param name="value">Receives the deserialized ExportController instance if successful</param>
    /// <returns>True if deserialization succeeded; otherwise, false</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null</exception>
    public static bool TryFromJson(string json, out ExportController? value)
        => TryFromJson(json, out value, out _);

    /// <summary>
    /// Attempts to deserialize a JSON string to an ExportController instance
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <param name="value">Receives the deserialized ExportController instance if successful</param>
    /// <param name="errorMessage">Receives the error message if deserialization fails</param>
    /// <returns>True if deserialization succeeded; otherwise, false</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null</exception>
    public static bool TryFromJson(string json, out ExportController? value, out string? errorMessage)
    {
        value = null;
        errorMessage = null;

        if (string.IsNullOrEmpty(json))
        {
            errorMessage = "JSON string is null or empty";
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<ExportController>(json, _jsonOptions);
            return true;
        }
        catch (JsonException ex)
        {
            errorMessage = ex.Message;
            return false;
        }
    }
}