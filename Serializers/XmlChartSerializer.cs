// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.IO;
using Microsoft.Extensions.Logging;
using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.Serializers;

/// <summary>
/// Serializes and deserializes Charts to/from XML format
/// Useful for configuration files and data interchange
/// </summary>
public class XmlChartSerializer
{
    private readonly ILogger<XmlChartSerializer> _logger;
    private readonly XmlSerializer _chartSerializer;

    public XmlChartSerializer(ILogger<XmlChartSerializer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _chartSerializer = new XmlSerializer(typeof(Chart));
    }

    /// <summary>
    /// Serializes a chart to XML string
    /// </summary>
    public string Serialize(Chart chart)
    {
        try
        {
            if (chart == null)
                throw new ArgumentNullException(nameof(chart));

            using var writer = new StringWriter();
            _chartSerializer.Serialize(writer, chart);
            var xml = writer.ToString();

            _logger.LogDebug("Chart {ChartId} serialized to XML", chart.Id);
            return xml;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error serializing chart to XML");
            throw;
        }
    }

    /// <summary>
    /// Deserializes a chart from XML string
    /// </summary>
    public Chart? Deserialize(string xml)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(xml))
                throw new ArgumentException("XML cannot be empty", nameof(xml));

            using var reader = new StringReader(xml);
            var chart = _chartSerializer.Deserialize(reader) as Chart;

            if (chart != null)
                _logger.LogDebug("Chart {ChartId} deserialized from XML", chart.Id);

            return chart;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Invalid XML format");
            throw new SerializationException("Failed to deserialize chart from XML", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deserializing chart from XML");
            throw;
        }
    }

    /// <summary>
    /// Validates XML structure
    /// </summary>
    public bool IsValidXml(string xml)
    {
        if (string.IsNullOrWhiteSpace(xml))
            return false;

        try
        {
            XDocument.Parse(xml);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Pretty prints XML with formatting
    /// </summary>
    public string PrettyPrint(string xml)
    {
        try
        {
            var doc = XDocument.Parse(xml);
            return doc.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to pretty print XML");
            return xml;
        }
    }

    /// <summary>
    /// Extracts specific element from XML
    /// </summary>
    public string? ExtractElement(string xml, string elementName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(xml))
                return null;

            var doc = XDocument.Parse(xml);
            var element = doc.Descendants(elementName).FirstOrDefault();
            return element?.Value;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to extract element {ElementName}", elementName);
            return null;
        }
    }

    /// <summary>
    /// Merges multiple chart XMLs
    /// </summary>
    public string? MergeCharts(List<string> xmlCharts)
    {
        try
        {
            if (xmlCharts == null || xmlCharts.Count == 0)
                return null;

            var root = new XElement("Charts");

            foreach (var xml in xmlCharts)
            {
                try
                {
                    var doc = XDocument.Parse(xml);
                    root.Add(doc.Root);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to parse chart XML during merge");
                }
            }

            return new XDocument(root).ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error merging chart XMLs");
            throw;
        }
    }
}
