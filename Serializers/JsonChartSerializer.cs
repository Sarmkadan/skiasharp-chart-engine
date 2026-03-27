// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.Serializers;

/// <summary>
/// Serializes and deserializes Chart objects to/from JSON
/// Provides abstraction for chart persistence and API communication
/// </summary>
public class JsonChartSerializer : IChartSerializer
{
    private readonly ILogger<JsonChartSerializer> _logger;
    private readonly JsonSerializerOptions _options;

    public JsonChartSerializer(ILogger<JsonChartSerializer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = CreateJsonOptions();
    }

    /// <summary>
    /// Serializes a chart object to JSON string
    /// </summary>
    public string Serialize(Chart chart)
    {
        try
        {
            if (chart == null)
                throw new ArgumentNullException(nameof(chart));

            var json = JsonSerializer.Serialize(chart, _options);
            _logger.LogDebug("Chart {ChartId} serialized successfully", chart.Id);
            return json;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error serializing chart");
            throw;
        }
    }

    /// <summary>
    /// Deserializes a JSON string to a Chart object
    /// </summary>
    public Chart? Deserialize(string json)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new ArgumentException("JSON cannot be empty", nameof(json));

            var chart = JsonSerializer.Deserialize<Chart>(json, _options);
            if (chart != null)
                _logger.LogDebug("Chart {ChartId} deserialized successfully", chart.Id);

            return chart;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid JSON format");
            throw new SerializationException("Failed to deserialize chart from JSON", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deserializing chart");
            throw;
        }
    }

    /// <summary>
    /// Serializes multiple charts to JSON array
    /// </summary>
    public string SerializeCollection(IEnumerable<Chart> charts)
    {
        try
        {
            if (charts == null)
                throw new ArgumentNullException(nameof(charts));

            var json = JsonSerializer.Serialize(charts, _options);
            _logger.LogDebug("Chart collection serialized successfully");
            return json;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error serializing chart collection");
            throw;
        }
    }

    /// <summary>
    /// Deserializes a JSON array to a collection of charts
    /// </summary>
    public List<Chart> DeserializeCollection(string json)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(json))
                return new List<Chart>();

            var charts = JsonSerializer.Deserialize<List<Chart>>(json, _options) ?? new List<Chart>();
            _logger.LogDebug("Chart collection deserialized: {Count} charts", charts.Count);
            return charts;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid JSON format for collection");
            throw new SerializationException("Failed to deserialize chart collection", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deserializing chart collection");
            throw;
        }
    }

    /// <summary>
    /// Validates JSON structure before deserialization
    /// </summary>
    public bool IsValidJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return false;

        try
        {
            using var doc = JsonDocument.Parse(json);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Formats JSON with indentation for readability
    /// </summary>
    public string PrettyPrint(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(doc.RootElement, _options);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to pretty print JSON");
            return json;
        }
    }

    private JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            }
        };

        return options;
    }
}

/// <summary>
/// Interface for chart serialization
/// Allows swapping different serialization implementations
/// </summary>
public interface IChartSerializer
{
    string Serialize(Chart chart);
    Chart? Deserialize(string json);
    string SerializeCollection(IEnumerable<Chart> charts);
    List<Chart> DeserializeCollection(string json);
    bool IsValidJson(string json);
    string PrettyPrint(string json);
}

/// <summary>
/// Custom exception for serialization failures
/// </summary>
public class SerializationException : Exception
{
    public SerializationException(string message) : base(message) { }

    public SerializationException(string message, Exception innerException)
        : base(message, innerException) { }
}
