// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace SkiaSharpChartEngine.Utilities;

/// <summary>
/// Mathematical utilities for chart calculations
/// Provides functions for data transformation and scaling
/// </summary>
public static class MathHelper
{
    /// <summary>
    /// Finds the minimum and maximum values in a collection
    /// </summary>
    public static (float Min, float Max) GetMinMax(IEnumerable<float> values)
    {
        if (!values.Any())
            throw new ArgumentException("Collection cannot be empty", nameof(values));

        return (values.Min(), values.Max());
    }

    /// <summary>
    /// Normalizes values to a 0-1 range based on min/max
    /// Useful for scaling data to chart dimensions
    /// </summary>
    public static float Normalize(float value, float min, float max)
    {
        if (Math.Abs(max - min) < float.Epsilon)
            return 0.5f;

        return (value - min) / (max - min);
    }

    /// <summary>
    /// Scales normalized value to target range
    /// Inverse of Normalize for positioning on canvas
    /// </summary>
    public static float ScaleToRange(float normalizedValue, float targetMin, float targetMax)
    {
        return targetMin + (normalizedValue * (targetMax - targetMin));
    }

    /// <summary>
    /// Calculates linear interpolation between two points
    /// Used for smooth animations and transitions
    /// </summary>
    public static float Lerp(float start, float end, float t)
    {
        t = Math.Clamp(t, 0f, 1f);
        return start + (end - start) * t;
    }

    /// <summary>
    /// Calculates optimal axis tick intervals based on data range
    /// Helps generate readable axis labels
    /// </summary>
    public static List<float> GenerateAxisTicks(float min, float max, int desiredTickCount = 5)
    {
        var range = max - min;
        if (Math.Abs(range) < float.Epsilon)
            return new List<float> { min };

        // Calculate tick interval
        var rawInterval = range / desiredTickCount;
        var magnitude = (float)Math.Pow(10, Math.Floor(Math.Log10(rawInterval)));
        var normalizedInterval = rawInterval / magnitude;

        float interval = normalizedInterval switch
        {
            <= 1f => 1f * magnitude,
            <= 2f => 2f * magnitude,
            <= 5f => 5f * magnitude,
            _ => 10f * magnitude
        };

        var ticks = new List<float>();
        var tickValue = (float)Math.Floor(min / interval) * interval;

        while (tickValue <= max + interval)
        {
            if (tickValue >= min)
                ticks.Add(tickValue);

            tickValue += interval;
        }

        return ticks;
    }

    /// <summary>
    /// Clamps a value between minimum and maximum bounds
    /// </summary>
    public static float Clamp(float value, float min, float max)
    {
        return Math.Max(min, Math.Min(max, value));
    }

    /// <summary>
    /// Calculates the average of a collection
    /// </summary>
    public static float Average(IEnumerable<float> values)
    {
        var enumerable = values.ToList();
        if (!enumerable.Any())
            throw new ArgumentException("Collection cannot be empty", nameof(values));

        return enumerable.Sum() / enumerable.Count;
    }

    /// <summary>
    /// Calculates the standard deviation of a collection
    /// Useful for statistical analysis in charts
    /// </summary>
    public static float StandardDeviation(IEnumerable<float> values)
    {
        var enumerable = values.ToList();
        if (enumerable.Count < 2)
            throw new ArgumentException("Collection must have at least 2 elements", nameof(values));

        var mean = Average(enumerable);
        var sumOfSquaredDiffs = enumerable.Sum(x => (x - mean) * (x - mean));
        var variance = sumOfSquaredDiffs / (enumerable.Count - 1);

        return (float)Math.Sqrt(variance);
    }

    /// <summary>
    /// Applies easing function for smooth animations
    /// </summary>
    public static float EaseInOutQuad(float t)
    {
        t = Math.Clamp(t, 0f, 1f);
        return t < 0.5f ? 2f * t * t : -1f + (4f - 2f * t) * t;
    }

    /// <summary>
    /// Checks if two floating point numbers are approximately equal
    /// Accounts for floating point precision errors
    /// </summary>
    public static bool ApproximatelyEqual(float a, float b, float epsilon = 0.0001f)
    {
        return Math.Abs(a - b) < epsilon;
    }

    /// <summary>
    /// Rounds a number to the nearest order of magnitude
    /// Useful for axis label generation
    /// </summary>
    public static float RoundToMagnitude(float value)
    {
        if (value == 0)
            return 0;

        var magnitude = (float)Math.Pow(10, Math.Floor(Math.Log10(Math.Abs(value))));
        return (float)Math.Round(value / magnitude) * magnitude;
    }
}
