// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.Rendering;

/// <summary>
/// Generates animation frames for chart transitions.
/// Supports easing functions for smooth animations.
/// </summary>
public class AnimationFrameGenerator
{
    private readonly ILogger<AnimationFrameGenerator> _logger;

    public AnimationFrameGenerator(ILogger<AnimationFrameGenerator> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // Generate animation frames between two charts
    public List<Chart> GenerateFrames(Chart startChart, Chart endChart, int frameCount, EasingFunction easingFunction = EasingFunction.EaseInOutQuad)
    {
        try
        {
            if (startChart == null || endChart == null || frameCount <= 0)
                return new List<Chart> { endChart };

            _logger.LogInformation("Generating {FrameCount} animation frames", frameCount);

            var frames = new List<Chart>();

            for (int i = 0; i <= frameCount; i++)
            {
                var progress = (double)i / frameCount;
                var easedProgress = _applyEasing(progress, easingFunction);

                var frame = _interpolateChart(startChart, endChart, easedProgress);
                frames.Add(frame);
            }

            _logger.LogDebug("Animation frames generated: {FrameCount}", frames.Count);
            return frames;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating animation frames");
            return new List<Chart> { endChart };
        }
    }

    // Generate frames with data value transitions
    public List<AnimationFrame> GenerateDataFrames(List<double> startValues, List<double> endValues, int frameCount)
    {
        try
        {
            if (startValues == null || endValues == null || startValues.Count != endValues.Count || frameCount <= 0)
                return new List<AnimationFrame>();

            var frames = new List<AnimationFrame>();

            for (int i = 0; i <= frameCount; i++)
            {
                var progress = (double)i / frameCount;
                var values = new List<double>();

                for (int j = 0; j < startValues.Count; j++)
                {
                    var interpolated = startValues[j] + (endValues[j] - startValues[j]) * progress;
                    values.Add(interpolated);
                }

                frames.Add(new AnimationFrame
                {
                    FrameNumber = i,
                    Progress = progress,
                    Values = values
                });
            }

            return frames;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating data frames");
            return new List<AnimationFrame>();
        }
    }

    private Chart _interpolateChart(Chart start, Chart end, double progress)
    {
        var chart = new Chart
        {
            Id = end.Id,
            Title = end.Title,
            ChartType = end.ChartType,
            Series = new List<ChartSeries>()
        };

        // Interpolate series
        for (int i = 0; i < end.Series.Count && i < start.Series.Count; i++)
        {
            var startSeries = start.Series[i];
            var endSeries = end.Series[i];

            var series = new ChartSeries
            {
                Name = endSeries.Name,
                Color = endSeries.Color,
                DataPoints = new List<DataPoint>()
            };

            for (int j = 0; j < endSeries.DataPoints.Count && j < startSeries.DataPoints.Count; j++)
            {
                var startValue = startSeries.DataPoints[j].Value;
                var endValue = endSeries.DataPoints[j].Value;
                var interpolatedValue = startValue + (endValue - startValue) * progress;

                series.DataPoints.Add(new DataPoint
                {
                    Label = endSeries.DataPoints[j].Label,
                    Value = interpolatedValue
                });
            }

            chart.Series.Add(series);
        }

        return chart;
    }

    private double _applyEasing(double progress, EasingFunction function)
    {
        return function switch
        {
            EasingFunction.Linear => progress,
            EasingFunction.EaseInQuad => progress * progress,
            EasingFunction.EaseOutQuad => progress * (2 - progress),
            EasingFunction.EaseInOutQuad => progress < 0.5 ? 2 * progress * progress : -1 + (4 - 2 * progress) * progress,
            EasingFunction.EaseInCubic => progress * progress * progress,
            EasingFunction.EaseOutCubic => 1 + (--progress) * progress * progress,
            _ => progress
        };
    }
}

public enum EasingFunction
{
    Linear,
    EaseInQuad,
    EaseOutQuad,
    EaseInOutQuad,
    EaseInCubic,
    EaseOutCubic
}

public class AnimationFrame
{
    public int FrameNumber { get; set; }
    public double Progress { get; set; }
    public List<double> Values { get; set; }
}
