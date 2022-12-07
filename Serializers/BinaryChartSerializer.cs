// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.Serializers;

/// <summary>
/// Binary serializer for efficient chart storage and transmission.
/// Provides compact binary format for reduced storage footprint.
/// </summary>
public class BinaryChartSerializer
{
    private readonly ILogger<BinaryChartSerializer> _logger;
    private const byte FormatVersion = 1;

    public BinaryChartSerializer(ILogger<BinaryChartSerializer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // Serialize chart to binary format
    public async Task<byte[]> SerializeAsync(Chart chart)
    {
        try
        {
            if (chart == null)
                throw new ArgumentNullException(nameof(chart));

            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms, Encoding.UTF8);

            // Write format version
            writer.Write(FormatVersion);

            // Write chart metadata
            writer.Write(chart.Id ?? "");
            writer.Write(chart.Title ?? "");
            writer.Write((int)chart.ChartType);
            writer.Write(chart.CreatedAt.Ticks);
            writer.Write(chart.UpdatedAt.Ticks);

            // Write series count
            var seriesCount = chart.Series?.Count ?? 0;
            writer.Write(seriesCount);

            // Write series data
            if (chart.Series != null)
            {
                foreach (var series in chart.Series)
                {
                    await _serializeSeriesAsync(writer, series);
                }
            }

            await writer.BaseStream.FlushAsync();
            var result = ms.ToArray();

            _logger.LogInformation("Chart serialized to binary format: {Size} bytes", result.Length);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error serializing chart to binary format");
            throw;
        }
    }

    // Deserialize chart from binary format
    public async Task<Chart> DeserializeAsync(byte[] data)
    {
        try
        {
            if (data == null || data.Length == 0)
                throw new ArgumentException("Data cannot be empty", nameof(data));

            using var ms = new MemoryStream(data);
            using var reader = new BinaryReader(ms, Encoding.UTF8);

            // Read format version
            var version = reader.ReadByte();
            if (version != FormatVersion)
                throw new InvalidOperationException($"Unsupported binary format version: {version}");

            // Read chart metadata
            var chart = new Chart
            {
                Id = reader.ReadString(),
                Title = reader.ReadString(),
                ChartType = (ChartType)reader.ReadInt32(),
                CreatedAt = new DateTime(reader.ReadInt64()),
                UpdatedAt = new DateTime(reader.ReadInt64()),
                Series = new System.Collections.Generic.List<ChartSeries>()
            };

            // Read series
            var seriesCount = reader.ReadInt32();
            for (int i = 0; i < seriesCount; i++)
            {
                var series = await _deserializeSeriesAsync(reader);
                if (series != null)
                    chart.Series.Add(series);
            }

            _logger.LogInformation("Chart deserialized from binary format");
            await Task.Delay(10); // Simulate async operation
            return chart;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deserializing binary chart data");
            throw;
        }
    }

    // Serialize series to binary
    private async Task _serializeSeriesAsync(BinaryWriter writer, ChartSeries series)
    {
        await Task.Delay(5); // Simulate async work
        writer.Write(series.Name ?? "");
        writer.Write(series.Color ?? "");

        var pointCount = series.DataPoints?.Count ?? 0;
        writer.Write(pointCount);

        if (series.DataPoints != null)
        {
            foreach (var point in series.DataPoints)
            {
                writer.Write(point.Label ?? "");
                writer.Write(point.Value);
            }
        }
    }

    // Deserialize series from binary
    private async Task<ChartSeries> _deserializeSeriesAsync(BinaryReader reader)
    {
        await Task.Delay(5); // Simulate async work
        var series = new ChartSeries
        {
            Name = reader.ReadString(),
            Color = reader.ReadString(),
            DataPoints = new System.Collections.Generic.List<DataPoint>()
        };

        var pointCount = reader.ReadInt32();
        for (int i = 0; i < pointCount; i++)
        {
            series.DataPoints.Add(new DataPoint
            {
                Label = reader.ReadString(),
                Value = reader.ReadDouble()
            });
        }

        return series;
    }
}
