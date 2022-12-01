// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Buffers;

namespace SkiaSharpChartEngine.Utilities;

/// <summary>
/// Mathematical utilities for chart calculations.
/// Provides functions for data transformation and scaling.
/// </summary>
public static class MathHelper
{
    /// <summary>
    /// Finds the minimum and maximum values in a collection in a single pass.
    /// </summary>
    public static (float Min, float Max) GetMinMax(IEnumerable<float> values)
    {
        float min = float.MaxValue;
        float max = float.MinValue;
        bool hasValues = false;

        foreach (var v in values)
        {
            if (v < min) min = v;
            if (v > max) max = v;
            hasValues = true;
        }

        if (!hasValues)
            throw new ArgumentException("Collection cannot be empty", nameof(values));

        return (min, max);
    }

    /// <summary>
    /// Finds the minimum and maximum values in a span in a single pass.
    /// Preferred overload — avoids IEnumerable overhead entirely.
    /// </summary>
    public static (float Min, float Max) GetMinMax(ReadOnlySpan<float> values)
    {
        if (values.IsEmpty)
            throw new ArgumentException("Collection cannot be empty", nameof(values));

        float min = values[0];
        float max = values[0];

        for (int i = 1; i < values.Length; i++)
        {
            if (values[i] < min) min = values[i];
            if (values[i] > max) max = values[i];
        }

        return (min, max);
    }

    /// <summary>
    /// Normalizes values to a 0-1 range based on min/max.
    /// Useful for scaling data to chart dimensions.
    /// </summary>
    public static float Normalize(float value, float min, float max)
    {
        if (Math.Abs(max - min) < float.Epsilon)
            return 0.5f;

        return (value - min) / (max - min);
    }

    /// <summary>
    /// Scales normalized value to target range.
    /// Inverse of Normalize for positioning on canvas.
    /// </summary>
    public static float ScaleToRange(float normalizedValue, float targetMin, float targetMax)
    {
        return targetMin + (normalizedValue * (targetMax - targetMin));
    }

    /// <summary>
    /// Calculates linear interpolation between two points.
    /// Used for smooth animations and transitions.
    /// </summary>
    public static float Lerp(float start, float end, float t)
    {
        t = Math.Clamp(t, 0f, 1f);
        return start + (end - start) * t;
    }

    /// <summary>
    /// Calculates optimal axis tick intervals based on data range.
    /// Helps generate readable axis labels.
    /// </summary>
    public static List<float> GenerateAxisTicks(float min, float max, int desiredTickCount = 5)
    {
        var range = max - min;
        if (Math.Abs(range) < float.Epsilon)
            return new List<float> { min };

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
    /// Clamps a value between minimum and maximum bounds.
    /// </summary>
    public static float Clamp(float value, float min, float max)
    {
        return Math.Max(min, Math.Min(max, value));
    }

    /// <summary>
    /// Calculates the average of a collection without intermediate allocation.
    /// </summary>
    public static float Average(IEnumerable<float> values)
    {
        float sum = 0f;
        int count = 0;

        foreach (var v in values)
        {
            sum += v;
            count++;
        }

        if (count == 0)
            throw new ArgumentException("Collection cannot be empty", nameof(values));

        return sum / count;
    }

    /// <summary>
    /// Calculates the average of a span. Zero-allocation hot path.
    /// </summary>
    public static float Average(ReadOnlySpan<float> values)
    {
        if (values.IsEmpty)
            throw new ArgumentException("Collection cannot be empty", nameof(values));

        float sum = 0f;
        foreach (var v in values)
            sum += v;

        return sum / values.Length;
    }

    /// <summary>
    /// Calculates the standard deviation of a collection.
    /// Uses ArrayPool to avoid heap allocation from materializing the sequence.
    /// </summary>
    public static float StandardDeviation(IEnumerable<float> values)
    {
        const int InitialCapacity = 64;
        float[] buffer = ArrayPool<float>.Shared.Rent(InitialCapacity);
        int count = 0;

        try
        {
            foreach (var v in values)
            {
                if (count == buffer.Length)
                {
                    float[] grown = ArrayPool<float>.Shared.Rent(buffer.Length * 2);
                    buffer.AsSpan(0, count).CopyTo(grown);
                    ArrayPool<float>.Shared.Return(buffer);
                    buffer = grown;
                }
                buffer[count++] = v;
            }

            if (count < 2)
                throw new ArgumentException("Collection must have at least 2 elements", nameof(values));

            return StandardDeviation(buffer.AsSpan(0, count));
        }
        finally
        {
            ArrayPool<float>.Shared.Return(buffer);
        }
    }

    /// <summary>
    /// Calculates the standard deviation of a span. Zero-allocation hot path.
    /// </summary>
    public static float StandardDeviation(ReadOnlySpan<float> values)
    {
        if (values.Length < 2)
            throw new ArgumentException("Collection must have at least 2 elements", nameof(values));

        float sum = 0f;
        foreach (var v in values)
            sum += v;

        float mean = sum / values.Length;
        float sumOfSquaredDiffs = 0f;

        foreach (var v in values)
        {
            float diff = v - mean;
            sumOfSquaredDiffs += diff * diff;
        }

        return (float)Math.Sqrt(sumOfSquaredDiffs / (values.Length - 1));
    }

    /// <summary>
    /// Applies easing function for smooth animations.
    /// </summary>
    public static float EaseInOutQuad(float t)
    {
        t = Math.Clamp(t, 0f, 1f);
        return t < 0.5f ? 2f * t * t : -1f + (4f - 2f * t) * t;
    }

    /// <summary>
    /// Checks if two floating point numbers are approximately equal.
    /// Accounts for floating point precision errors.
    /// </summary>
    public static bool ApproximatelyEqual(float a, float b, float epsilon = 0.0001f)
    {
        return Math.Abs(a - b) < epsilon;
    }

    /// <summary>
    /// Rounds a number to the nearest order of magnitude.
    /// Useful for axis label generation.
    /// </summary>
    public static float RoundToMagnitude(float value)
    {
        if (value == 0)
            return 0;

        var magnitude = (float)Math.Pow(10, Math.Floor(Math.Log10(Math.Abs(value))));
        return (float)Math.Round(value / magnitude) * magnitude;
    }
}
