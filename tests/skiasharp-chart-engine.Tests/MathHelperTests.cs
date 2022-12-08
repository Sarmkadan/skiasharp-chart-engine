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

public class MathHelperTests
{
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

    [Fact]
    public void Normalize_MidpointBetweenBounds_ReturnsZeroPointFive()
    {
        // Act
        var result = MathHelper.Normalize(50f, 0f, 100f);

        // Assert
        result.Should().BeApproximately(0.5f, 0.0001f);
    }

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

    [Fact]
    public void GenerateAxisTicks_ZeroRange_ReturnsSingleTick()
    {
        // Act
        var ticks = MathHelper.GenerateAxisTicks(10f, 10f);

        // Assert
        ticks.Should().ContainSingle().Which.Should().Be(10f);
    }

    [Fact]
    public void Clamp_ValueAboveMax_ReturnsMax()
    {
        // Act
        var result = MathHelper.Clamp(150f, 0f, 100f);

        // Assert
        result.Should().Be(100f);
    }

    [Fact]
    public void Clamp_ValueBelowMin_ReturnsMin()
    {
        // Act
        var result = MathHelper.Clamp(-10f, 0f, 100f);

        // Assert
        result.Should().Be(0f);
    }
}
