// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using FluentAssertions;
using SkiaSharpChartEngine.Caching;
using SkiaSharpChartEngine.Constants;
using Xunit;

namespace SkiaSharpChartEngine.Tests.Caching;

/// <summary>
/// Provides unit tests for the <see cref="CacheKeyBuilder"/> class.
/// </summary>
public class CacheKeyBuilderTests
{
    [Fact]
    public void BuildChartKey_WithValidId_ReturnsPrefixedId()
    {
        var result = CacheKeyBuilder.BuildChartKey("sales-2026");

        result.Should().Be("chart_sales-2026");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void BuildChartKey_WithNullOrEmptyId_ThrowsArgumentException(string? chartId)
    {
        Action act = () => CacheKeyBuilder.BuildChartKey(chartId!);

        act.Should().Throw<ArgumentException>().WithParameterName("chartId");
    }

    [Fact]
    public void BuildRenderKey_WithSameInputs_ReturnsSameKey()
    {
        var first = CacheKeyBuilder.BuildRenderKey("sales", 800, 600, 96f, "png");
        var second = CacheKeyBuilder.BuildRenderKey("sales", 800, 600, 96f, "png");

        first.Should().Be(second);
    }

    [Fact]
    public void BuildRenderKey_WhenRenderParameterDiffers_ReturnsDifferentKeys()
    {
        var keys = new[]
        {
            CacheKeyBuilder.BuildRenderKey("sales", 800, 600, 96f, "png"),
            CacheKeyBuilder.BuildRenderKey("sales", 801, 600, 96f, "png"),
            CacheKeyBuilder.BuildRenderKey("sales", 800, 601, 96f, "png"),
            CacheKeyBuilder.BuildRenderKey("sales", 800, 600, 97f, "png"),
            CacheKeyBuilder.BuildRenderKey("sales", 800, 600, 96f, "jpeg")
        };

        keys.Should().OnlyHaveUniqueItems();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void BuildRenderKey_WithNullOrEmptyChartId_ThrowsArgumentException(string? chartId)
    {
        Action act = () => CacheKeyBuilder.BuildRenderKey(chartId!, 800, 600, 96f, "png");

        act.Should().Throw<ArgumentException>().WithParameterName("chartId");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void BuildRenderKey_WithNullOrEmptyFormat_ThrowsArgumentException(string? format)
    {
        Action act = () => CacheKeyBuilder.BuildRenderKey("sales", 800, 600, 96f, format!);

        act.Should().Throw<ArgumentException>().WithParameterName("format");
    }

    [Fact]
    public void BuildConfigurationKey_ForEveryChartType_ReturnsLowercaseChartTypeKey()
    {
        foreach (var chartType in Enum.GetValues<ChartType>())
        {
            var result = CacheKeyBuilder.BuildConfigurationKey(chartType);

            result.Should().Be($"config_{chartType.ToString().ToLowerInvariant()}");
        }
    }
}
