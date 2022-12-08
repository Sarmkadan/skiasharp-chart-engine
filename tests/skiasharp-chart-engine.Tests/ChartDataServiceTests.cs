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
    // ValidateChart tests
    // -------------------------------------------------------------------------

    [Fact]
    public void ValidateChart_WithNullChart_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => _service.ValidateChart(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("chart");
    }

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
    // CalculateAxisRange tests
    // -------------------------------------------------------------------------

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
    // FilterDataPoints tests
    // -------------------------------------------------------------------------

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
    // TransformChartData tests
    // -------------------------------------------------------------------------

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
    // NormalizeDataPoints tests
    // -------------------------------------------------------------------------

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
    // Logger interaction tests (Moq verification)
    // -------------------------------------------------------------------------

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
