// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SkiaSharpChartEngine.Constants;
using SkiaSharpChartEngine.Exceptions;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Services;
using Xunit;

namespace SkiaSharpChartEngine.Tests.Services;

/// <summary>
/// Provides unit tests for the <see cref="ChartDataService"/> class.
/// Tests chart validation, data transformation, normalization, and axis range calculations
/// to ensure the ChartDataService handles various chart scenarios correctly.
/// </summary>
public class ChartDataServiceTests
{
    private readonly Mock<ILogger<ChartDataService>> _loggerMock;
    private readonly ChartDataService _service;

    public ChartDataServiceTests()
    {
        _loggerMock = new Mock<ILogger<ChartDataService>>();
        _service = new ChartDataService(_loggerMock.Object);
    }

    // -------------------------------------------------------------------------
/// <summary>
/// ValidateChart tests
/// </summary>
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that ValidateChart throws ArgumentNullException when passed a null chart.
    /// </summary>
    [Fact]
    public void ValidateChart_WithNullChart_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => _service.ValidateChart(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("chart");
    }

    /// <summary>
    /// Tests that ValidateChart throws InvalidChartDataException when chart has no series.
    /// </summary>
    [Fact]
    public void ValidateChart_WithNoSeries_ThrowsInvalidChartDataException()
    {
        // Arrange
        var chart = new Chart("empty-chart");

        // Act
        Action act = () => _service.ValidateChart(chart);

        // Assert
        act.Should().Throw<InvalidChartDataException>()
           .WithMessage("*at least one series*");
    }

    /// <summary>
    /// Tests that ValidateChart does not throw when chart has valid data with at least one series.
    /// </summary>
    [Fact]
    public void ValidateChart_WithValidChartData_DoesNotThrow()
    {
        // Arrange
        var chart = new Chart("valid-chart");
        var series = new ChartSeries("Revenue");
        series.AddDataPoint(1.0, 100.0);
        series.AddDataPoint(2.0, 150.0);
        chart.AddSeries(series);

        // Act
        Action act = () => _service.ValidateChart(chart);

        // Assert
        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests that ValidateSeries throws InvalidChartDataException when series has zero line width.
    /// </summary>
    [Fact]
    public void ValidateSeries_WithZeroLineWidth_ThrowsInvalidChartDataException()
    {
        // Arrange - LineWidth of 0 is invalid
        var series = new ChartSeries("Bad Series") { LineWidth = 0f };

        // Act
        Action act = () => _service.ValidateSeries(series);

        // Assert
        act.Should().Throw<InvalidChartDataException>()
           .WithMessage("*Line width*");
    }

    // -------------------------------------------------------------------------
    /// <summary>
    /// CalculateAxisRange tests
    /// </summary>
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that CalculateAxisRange returns exact minimum and maximum values for linear scale.
    /// </summary>
    [Fact]
    public void CalculateAxisRange_WithLinearScale_ReturnsExactMinAndMax()
    {
        // Arrange
        var values = new double[] { 5.0, 10.0, 3.0, 8.0 };

        // Act
        var (min, max) = _service.CalculateAxisRange(values, AxisScaleType.Linear);

        // Assert
        min.Should().Be(3.0);
        max.Should().Be(10.0);
    }

    /// <summary>
    /// Tests that CalculateAxisRange enforces minimum value of 1 for logarithmic scale
    /// since log(0) is undefined.
    /// </summary>
    [Fact]
    public void CalculateAxisRange_WithLogarithmicScale_EnforcesMinimumOfOne()
    {
        // Arrange - logarithmic scale cannot have min < 1 (log(0) is undefined)
        var values = new double[] { 0.5, 1.0, 100.0 };

        // Act
        var (min, max) = _service.CalculateAxisRange(values, AxisScaleType.Logarithmic);

        // Assert - minimum is clamped to 1 for log scale
        min.Should().BeGreaterThanOrEqualTo(1.0);
    }

    /// <summary>
    /// Tests that CalculateAxisRange returns default range (0.0 to 1.0) when given empty collection.
    /// </summary>
    [Fact]
    public void CalculateAxisRange_EmptyCollection_ReturnsDefaultRange()
    {
        // Arrange
        var values = new double[0];

        // Act
        var (min, max) = _service.CalculateAxisRange(values, AxisScaleType.Linear);

        // Assert
        min.Should().Be(0.0);
        max.Should().Be(1.0);
    }

    // -------------------------------------------------------------------------
    /// <summary>
    /// FilterDataPoints tests
    /// </summary>
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that FilterDataPoints returns only data points where Y value is positive.
    /// </summary>
    [Fact]
    public void FilterDataPoints_WithPositiveYFilter_ReturnsOnlyMatchingPoints()
    {
        // Arrange
        var points = new List<DataPoint>
        {
            new DataPoint(1.0, 5.0),
            new DataPoint(2.0, -3.0),
            new DataPoint(3.0, 8.0),
            new DataPoint(4.0, -1.0)
        };

        // Act
        var result = _service.FilterDataPoints(points, p => p.Y > 0);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(p => p.Y.Should().BePositive());
    }

    /// <summary>
    /// Tests that FilterDataPoints throws ArgumentNullException when predicate is null.
    /// </summary>
    [Fact]
    public void FilterDataPoints_WithNullPredicate_ThrowsArgumentNullException()
    {
        // Arrange
        var points = new List<DataPoint> { new DataPoint(1.0, 2.0) };

        // Act
        Action act = () => _service.FilterDataPoints(points, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("predicate");
    }

    // -------------------------------------------------------------------------
    /// <summary>
    /// TransformChartData tests
    /// </summary>
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that TransformChartData applies the transformer function to all data points,
    /// doubling the Y values in this test case.
    /// </summary>
    [Fact]
    public void TransformChartData_WithDoubleYTransformer_TransformsAllSeriesPoints()
    {
        // Arrange
        var chart = new Chart("transform-chart");
        var series = new ChartSeries("Data");
        series.AddDataPoint(1.0, 10.0);
        series.AddDataPoint(2.0, 20.0);
        chart.AddSeries(series);

        // Act - double every Y value
        var transformed = _service.TransformChartData(chart, p => new DataPoint(p.X, p.Y * 2));

        // Assert
        var transformedPoints = transformed.Series[0].DataPoints;
        transformedPoints[0].Y.Should().Be(20.0);
        transformedPoints[1].Y.Should().Be(40.0);
    }

    /// <summary>
    /// Tests that TransformChartData returns a new chart instance without mutating the original chart.
    /// </summary>
    [Fact]
    public void TransformChartData_OriginalChartIsNotMutated()
    {
        // Arrange
        var chart = new Chart("immutable-chart");
        var series = new ChartSeries("Data");
        series.AddDataPoint(1.0, 100.0);
        chart.AddSeries(series);

        // Act - transform returns a clone, original should not change
        _service.TransformChartData(chart, p => new DataPoint(p.X, p.Y + 999.0));

        // Assert - original data is untouched
        chart.Series[0].DataPoints[0].Y.Should().Be(100.0);
    }

    // -------------------------------------------------------------------------
    /// <summary>
    /// NormalizeDataPoints tests
    /// </summary>
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that NormalizeDataPoints scales all data points to the [0, 1] interval
    /// based on the minimum and maximum values in the collection.
    /// </summary>
    [Fact]
    public void NormalizeDataPoints_WithKnownRange_ScalesPointsToZeroOneInterval()
    {
        // Arrange
        var points = new List<DataPoint>
        {
            new DataPoint(0.0, 0.0),
            new DataPoint(5.0, 50.0),
            new DataPoint(10.0, 100.0)
        };

        // Act
        _service.NormalizeDataPoints(points);

        // Assert - first point maps to 0,0 and last to 1,1
        points[0].X.Should().BeApproximately(0.0, 0.0001);
        points[0].Y.Should().BeApproximately(0.0, 0.0001);
        points[2].X.Should().BeApproximately(1.0, 0.0001);
        points[2].Y.Should().BeApproximately(1.0, 0.0001);
    }

    // -------------------------------------------------------------------------
    /// <summary>
    /// Logger interaction tests (Moq verification)
    /// </summary>
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that ValidateChart logs an information message after successful validation.
    /// Verifies that the ILogger is called at Information level.
    /// </summary>
    [Fact]
    public void ValidateChart_AfterSuccessfulValidation_LogsInformationMessage()
    {
        // Arrange
        var chart = new Chart("logged-chart");
        var series = new ChartSeries("Metrics");
        series.AddDataPoint(0.0, 42.0);
        chart.AddSeries(series);

        // Act
        _service.ValidateChart(chart);

        // Assert - verify that ILogger.Log was called at Information level
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("validation passed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }
}
