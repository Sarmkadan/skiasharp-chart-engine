// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.Utilities;

/// <summary>
/// Utility class for generating test and sample data points
/// </summary>
public static class DataPointGenerator
{
    public static List<DataPoint> GenerateLinearData(int count, double slope = 1, double intercept = 0)
    {
        if (count <= 0)
            throw new ArgumentException("Count must be greater than 0");

        var points = new List<DataPoint>();
        for (int i = 0; i < count; i++)
        {
            var x = i;
            var y = slope * x + intercept;
            points.Add(new DataPoint(x, y));
        }
        return points;
    }

    public static List<DataPoint> GenerateSinusoidalData(int count, double amplitude = 10, double frequency = 1)
    {
        if (count <= 0)
            throw new ArgumentException("Count must be greater than 0");

        var points = new List<DataPoint>();
        for (int i = 0; i < count; i++)
        {
            var x = i;
            var y = amplitude * Math.Sin(2 * Math.PI * frequency * i / count);
            points.Add(new DataPoint(x, y));
        }
        return points;
    }

    public static List<DataPoint> GenerateRandomData(int count, double minValue = 0, double maxValue = 100)
    {
        if (count <= 0)
            throw new ArgumentException("Count must be greater than 0");

        if (minValue > maxValue)
            throw new ArgumentException("MinValue must be less than MaxValue");

        var random = new Random();
        var points = new List<DataPoint>();

        for (int i = 0; i < count; i++)
        {
            var x = i;
            var y = minValue + random.NextDouble() * (maxValue - minValue);
            points.Add(new DataPoint(x, y));
        }

        return points;
    }

    public static List<DataPoint> GenerateGaussianData(int count, double mean = 50, double stdDev = 10)
    {
        if (count <= 0)
            throw new ArgumentException("Count must be greater than 0");

        var random = new Random();
        var points = new List<DataPoint>();

        for (int i = 0; i < count; i++)
        {
            var u1 = random.NextDouble();
            var u2 = random.NextDouble();
            var z = Math.Sqrt(-2 * Math.Log(u1)) * Math.Cos(2 * Math.PI * u2);
            var y = mean + stdDev * z;

            points.Add(new DataPoint(i, y));
        }

        return points;
    }

    public static List<DataPoint> GenerateExponentialData(int count, double baseValue = 1, double growthRate = 0.1)
    {
        if (count <= 0)
            throw new ArgumentException("Count must be greater than 0");

        var points = new List<DataPoint>();

        for (int i = 0; i < count; i++)
        {
            var x = i;
            var y = baseValue * Math.Exp(growthRate * i);
            points.Add(new DataPoint(x, y));
        }

        return points;
    }

    public static List<DataPoint> GeneratePolynomialData(int count, double[] coefficients)
    {
        if (count <= 0)
            throw new ArgumentException("Count must be greater than 0");

        if (coefficients == null || coefficients.Length == 0)
            throw new ArgumentNullException(nameof(coefficients));

        var points = new List<DataPoint>();

        for (int i = 0; i < count; i++)
        {
            var x = (double)i;
            var y = 0.0;

            for (int j = 0; j < coefficients.Length; j++)
            {
                y += coefficients[j] * Math.Pow(x, j);
            }

            points.Add(new DataPoint(x, y));
        }

        return points;
    }
}
