// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using FluentAssertions;
using SkiaSharpChartEngine.Extensions;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Utilities;
using Xunit;

namespace SkiaSharpChartEngine.Tests.Models;

public class ChartModelsAndValidationTests
{
    // -------------------------------------------------------------------------
    // DataPoint model tests
    // -------------------------------------------------------------------------

    [Fact]
    public void DataPoint_SettingXToNaN_ThrowsArgumentException()
    {
        // Arrange
        var point = new DataPoint(1.0, 2.0);

        // Act
        Action act = () => point.X = double.NaN;

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("*NaN*");
    }

    [Fact]
    public void DataPoint_SettingYToPositiveInfinity_ThrowsArgumentException()
    {
        // Arrange
        var point = new DataPoint(1.0, 2.0);

        // Act
        Action act = () => point.Y = double.PositiveInfinity;

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("*Infinity*");
    }

    [Fact]
    public void DataPoint_Clone_ProducesIndependentCopyWithSameValues()
    {
        // Arrange
        var original = new DataPoint(3.0, 7.5, "Q3", "#FF0000")
        {
            Metadata = new Dictionary<string, object> { ["key"] = "value" }
        };

        // Act
        var clone = original.Clone();
        clone.X = 99.0;

        // Assert
        original.X.Should().Be(3.0);
        clone.Y.Should().Be(7.5);
        clone.Label.Should().Be("Q3");
    }

    // -------------------------------------------------------------------------
    // ChartSeries model tests
    // -------------------------------------------------------------------------

    [Fact]
    public void ChartSeries_AddDataPoint_IncreasesCount()
    {
        // Arrange
        var series = new ChartSeries("Revenue");

        // Act
        series.AddDataPoint(1.0, 100.0);
        series.AddDataPoint(2.0, 150.0);

        // Assert
        series.GetDataPointCount().Should().Be(2);
    }

    [Fact]
    public void ChartSeries_GetYAxisRange_EmptySeries_ReturnsDefaultRange()
    {
        // Arrange
        var series = new ChartSeries("Empty");

        // Act
        var (min, max) = series.GetYAxisRange();

        // Assert
        min.Should().Be(0);
        max.Should().Be(1);
    }

    [Fact]
    public void ChartSeries_GetYAxisRange_WithPoints_ReturnsActualBounds()
    {
        // Arrange
        var series = new ChartSeries("Prices");
        series.AddDataPoint(0.0, 10.0);
        series.AddDataPoint(1.0, 50.0);
        series.AddDataPoint(2.0, 25.0);

        // Act
        var (min, max) = series.GetYAxisRange();

        // Assert
        min.Should().Be(10.0);
        max.Should().Be(50.0);
    }

    // -------------------------------------------------------------------------
    // Chart model tests
    // -------------------------------------------------------------------------

    [Fact]
    public void Chart_AddNullSeries_ThrowsArgumentNullException()
    {
        // Arrange
        var chart = new Chart("test-chart");

        // Act
        Action act = () => chart.AddSeries(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Chart_GetDataBounds_EmptyChart_ReturnsDefaultBounds()
    {
        // Arrange
        var chart = new Chart("empty-chart");

        // Act
        var (minX, maxX, minY, maxY) = chart.GetDataBounds();

        // Assert
        minX.Should().Be(0);
        maxX.Should().Be(1);
        minY.Should().Be(0);
        maxY.Should().Be(1);
    }

    [Fact]
    public void Chart_GetDataBounds_WithSeriesData_ReturnsCorrectBounds()
    {
        // Arrange
        var chart = new Chart("bounds-chart");
        var series = new ChartSeries("Data");
        series.AddDataPoint(-5.0, 0.0);
        series.AddDataPoint(10.0, 20.0);
        chart.AddSeries(series);

        // Act
        var (minX, maxX, minY, maxY) = chart.GetDataBounds();

        // Assert
        minX.Should().Be(-5.0);
        maxX.Should().Be(10.0);
        minY.Should().Be(0.0);
        maxY.Should().Be(20.0);
    }

    // -------------------------------------------------------------------------
    // ChartValidator tests
    // -------------------------------------------------------------------------

    [Fact]
    public void ChartValidator_ValidateChart_NullInput_IsNotValid()
    {
        // Act
        var result = ChartValidator.ValidateChart(null!);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Contains("null"));
    }

    [Fact]
    public void ChartValidator_ValidateChart_NoSeries_AddsSeriesError()
    {
        // Arrange
        var chart = new Chart("chart-no-series");

        // Act
        var result = ChartValidator.ValidateChart(chart);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("series"));
    }

    [Fact]
    public void ChartValidator_ValidateSeries_EmptyName_AddsNameError()
    {
        // Arrange - ChartSeries initializes Name to empty string
        var series = new ChartSeries();

        // Act
        var result = ChartValidator.ValidateSeries(series, 0);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("empty name"));
    }

    [Fact]
    public void ChartValidator_ValidateConfiguration_DefaultConfig_IsValid()
    {
        // Arrange - default config has valid dimensions and colors
        var config = new ChartConfiguration();

        // Act
        var result = ChartValidator.ValidateConfiguration(config);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ChartValidator_ValidateDataPoint_NullPoint_IsNotValid()
    {
        // Act
        var result = ChartValidator.ValidateDataPoint(null!, 0);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle();
    }

    // -------------------------------------------------------------------------
    // ColorHelper tests
    // -------------------------------------------------------------------------

    [Fact]
    public void ColorHelper_HexToRgb_ForPureRed_ReturnsCorrectRgbString()
    {
        // Act
        var result = ColorHelper.HexToRgb("#FF0000");

        // Assert
        result.Should().Be("rgb(255, 0, 0)");
    }

    [Fact]
    public void ColorHelper_RgbToHex_ForBlue_ReturnsUpperCaseHex()
    {
        // Act
        var result = ColorHelper.RgbToHex(0, 0, 255);

        // Assert
        result.Should().Be("#0000FF");
    }

    [Fact]
    public void ColorHelper_IsValidHexColor_ValidSixCharHex_ReturnsTrue()
    {
        ColorHelper.IsValidHexColor("#1F77B4").Should().BeTrue();
    }

    [Fact]
    public void ColorHelper_IsValidHexColor_MissingHash_ReturnsFalse()
    {
        ColorHelper.IsValidHexColor("FF0000").Should().BeFalse();
    }

    [Fact]
    public void ColorHelper_LightenColor_IncreasesChannelValues()
    {
        // Arrange - start with dark grey (#404040 = rgb(64,64,64))
        var original = "#404040";

        // Act
        var lightened = ColorHelper.LightenColor(original, 0.5f);

        // Compare channel: 64 + (255-64)*0.5 = 64+95.5 = 159 → 0x9F
        lightened.Should().Be("#9F9F9F");
    }

    // -------------------------------------------------------------------------
    // DataPointExtensions tests
    // -------------------------------------------------------------------------

    [Fact]
    public void DataPointExtensions_GetDistance_KnownPoints_ReturnsEuclideanDistance()
    {
        // Arrange - 3-4-5 right triangle
        var a = new DataPoint(0.0, 0.0);
        var b = new DataPoint(3.0, 4.0);

        // Act
        var distance = a.GetDistance(b);

        // Assert
        distance.Should().BeApproximately(5.0, 0.0001);
    }

    [Fact]
    public void DataPointExtensions_Offset_ShiftsXAndYBySpecifiedAmount()
    {
        // Arrange
        var point = new DataPoint(1.0, 2.0);

        // Act
        var shifted = point.Offset(3.0, -1.0);

        // Assert
        shifted.X.Should().Be(4.0);
        shifted.Y.Should().Be(1.0);
    }

    [Fact]
    public void DataPointExtensions_Scale_MultipliesCoordinates()
    {
        // Arrange
        var point = new DataPoint(3.0, 5.0);

        // Act
        var scaled = point.Scale(2.0, 0.5);

        // Assert
        scaled.X.Should().Be(6.0);
        scaled.Y.Should().BeApproximately(2.5, 0.0001);
    }
}
