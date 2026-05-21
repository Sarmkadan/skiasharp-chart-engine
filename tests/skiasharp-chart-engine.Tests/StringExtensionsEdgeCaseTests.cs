#nullable enable
using FluentAssertions;
using SkiaSharpChartEngine.Extensions;
using Xunit;

namespace SkiaSharpChartEngine.Tests;

public sealed class StringExtensionsEdgeCaseTests
{
    [Fact]
    public void ToKebabCase_NullInput_ReturnsNull() =>
        ((string?)null).ToKebabCase().Should().BeNull();

    [Fact]
    public void ToKebabCase_EmptyInput_ReturnsEmpty() =>
        "".ToKebabCase().Should().BeEmpty();

    [Theory]
    [InlineData("HelloWorld", "hello-world")]
    [InlineData("ChartEngine", "chart-engine")]
    [InlineData("a", "a")]
    public void ToKebabCase_VariousInputs(string input, string expected) =>
        input.ToKebabCase().Should().Be(expected);

    [Fact]
    public void ToSnakeCase_NullInput_ReturnsNull() =>
        ((string?)null).ToSnakeCase().Should().BeNull();

    [Fact]
    public void ToSnakeCase_EmptyInput_ReturnsEmpty() =>
        "".ToSnakeCase().Should().BeEmpty();

    [Theory]
    [InlineData("HelloWorld", "hello_world")]
    [InlineData("ChartEngine", "chart_engine")]
    public void ToSnakeCase_VariousInputs(string input, string expected) =>
        input.ToSnakeCase().Should().Be(expected);

    [Fact]
    public void ToPascalCase_NullInput_ReturnsNull() =>
        ((string?)null).ToPascalCase().Should().BeNull();

    [Fact]
    public void ToPascalCase_EmptyInput_ReturnsEmpty() =>
        "".ToPascalCase().Should().BeEmpty();

    [Theory]
    [InlineData("hello-world", "HelloWorld")]
    [InlineData("chart_engine", "ChartEngine")]
    [InlineData("some name", "SomeName")]
    public void ToPascalCase_VariousInputs(string input, string expected) =>
        input.ToPascalCase().Should().Be(expected);

    [Fact]
    public void ToDoubleOrDefault_ValidNumber_ReturnsNumber() =>
        "3.14".ToDoubleOrDefault().Should().BeApproximately(3.14, 0.001);

    [Fact]
    public void ToDoubleOrDefault_InvalidString_ReturnsDefault() =>
        "not-a-number".ToDoubleOrDefault(99.0).Should().Be(99.0);

    [Fact]
    public void ToDoubleOrDefault_EmptyString_ReturnsDefault() =>
        "".ToDoubleOrDefault(0).Should().Be(0);
}
