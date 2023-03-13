// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Linq;
using FluentAssertions;
using SkiaSharpChartEngine.Utilities;
using Xunit;

namespace SkiaSharpChartEngine.Tests.Utilities;

/// <summary>
/// Provides unit tests for the <see cref="MathHelper"/> class to verify mathematical utility functions used in chart rendering calculations.
/// </summary>
public class MathHelperTests
{
    /// <summary>
    /// Tests that <see cref="MathHelper.GetMinMax"/> throws an <see cref="ArgumentException"/> when provided with an empty collection.
    /// </summary>
    [Fact]
    public void GetMinMax_EmptyCollection_ThrowsArgumentException()
    {
        // Arrange
        var values = Enumerable.Empty<float>();

        // Act
        Action act = () => MathHelper.GetMinMax(values);

        // Assert
        act.Should().Throw<ArgumentException>().WithParameterName("values");
    }

    /// <summary>
    /// Tests that <see cref="MathHelper.Normalize"/> returns 0.5 when min and max values are equal.
    /// This edge case ensures the normalization function handles degenerate ranges correctly.
    /// </summary>
    [Fact]
    public void Normalize_WhenMinAndMaxAreEqual_ReturnsHalf()
    {
        // Arrange
        float value = 7f, min = 7f, max = 7f;

        // Act
        var result = MathHelper.Normalize(value, min, max);

        // Assert
        result.Should().Be(0.5f);
    }

    /// <summary>
    /// Tests that <see cref="MathHelper.Normalize"/> returns 0.5 when the input value is exactly at the midpoint between min and max bounds.
    /// </summary>
    [Fact]
    public void Normalize_MidpointBetweenBounds_ReturnsZeroPointFive()
    {
        // Act
        var result = MathHelper.Normalize(50f, 0f, 100f);

        // Assert
        result.Should().BeApproximately(0.5f, 0.0001f);
    }

    /// <summary>
    /// Tests that <see cref="MathHelper.Lerp"/> clamps the interpolation parameter t to the range [0, 1], returning the end value when t exceeds 1.
    /// </summary>
    [Fact]
    public void Lerp_TExceedsOne_ClampsToEndValue()
    {
        // Arrange - t > 1 should be clamped to 1, returning the end value
        float start = 10f, end = 20f, t = 5f;

        // Act
        var result = MathHelper.Lerp(start, end, t);

        // Assert
        result.Should().Be(end);
    }

    /// <summary>
    /// Tests that <see cref="MathHelper.Lerp"/> returns the start value when the interpolation parameter t is 0.
    /// </summary>
    [Fact]
    public void Lerp_TIsZero_ReturnsStartValue()
    {
        // Arrange
        float start = 10f, end = 20f, t = 0f;

        // Act
        var result = MathHelper.Lerp(start, end, t);

        // Assert
        result.Should().Be(start);
    }

    /// <summary>
    /// Tests that <see cref="MathHelper.StandardDeviation"/> throws an <see cref="ArgumentException"/> when provided with a collection containing only a single element.
    /// Standard deviation calculation requires at least 2 elements to compute sample variance.
    /// </summary>
    [Fact]
    public void StandardDeviation_CollectionWithSingleElement_ThrowsArgumentException()
    {
        // Arrange - standard deviation requires at least 2 elements (sample variance)
        var values = new float[] { 5f };

        // Act
        Action act = () => MathHelper.StandardDeviation(values);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that <see cref="MathHelper.StandardDeviation"/> returns the correct standard deviation for two symmetric values.
    /// For input [2, 4], the mean is 3, squared differences are [1, 1], sample variance is 2, and standard deviation is sqrt(2).
    /// </summary>
    [Fact]
    public void StandardDeviation_TwoSymmetricValues_ReturnsExpectedDeviation()
    {
        // Arrange - [2, 4]: mean=3, squared diffs=[1,1], sample variance=2, std dev=sqrt(2)
        var values = new float[] { 2f, 4f };

        // Act
        var result = MathHelper.StandardDeviation(values);

        // Assert
        result.Should().BeApproximately((float)Math.Sqrt(2), 0.0001f);
    }

    /// <summary>
    /// Tests that <see cref="MathHelper.GenerateAxisTicks"/> returns a single tick value when the input range is zero (min equals max).
    /// This ensures axis tick generation handles degenerate ranges correctly.
    /// </summary>
    [Fact]
    public void GenerateAxisTicks_ZeroRange_ReturnsSingleTick()
    {
        // Act
        var ticks = MathHelper.GenerateAxisTicks(10f, 10f);

        // Assert
        ticks.Should().ContainSingle().Which.Should().Be(10f);
    }

    /// <summary>
    /// Tests that <see cref="MathHelper.Clamp"/> returns the maximum value when the input value exceeds the specified maximum bound.
    /// </summary>
    [Fact]
    public void Clamp_ValueAboveMax_ReturnsMax()
    {
        // Act
        var result = MathHelper.Clamp(150f, 0f, 100f);

        // Assert
        result.Should().Be(100f);
    }

    /// <summary>
    /// Tests that <see cref="MathHelper.Clamp"/> returns the minimum value when the input value is below the specified minimum bound.
    /// </summary>
    [Fact]
    public void Clamp_ValueBelowMin_ReturnsMin()
    {
        // Act
        var result = MathHelper.Clamp(-10f, 0f, 100f);

        // Assert
        result.Should().Be(0f);
    }
}
