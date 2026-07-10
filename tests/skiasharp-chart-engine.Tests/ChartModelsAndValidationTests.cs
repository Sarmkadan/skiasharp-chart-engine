// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using FluentAssertions;
using SkiaSharpChartEngine.Extensions;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Utilities;
using Xunit;

namespace SkiaSharpChartEngine.Tests.Models;

/// <summary>
/// Contains unit tests for chart models and validation logic in the SkiaSharpChartEngine library.
/// This test class validates the behavior of DataPoint, ChartSeries, Chart, ChartValidator,
/// ColorHelper, and their associated extension methods.
/// </summary>
public class ChartModelsAndValidationTests
{
    // -------------------------------------------------------------------------
    // DataPoint model tests
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that setting the X coordinate of a DataPoint to NaN throws an ArgumentException.
    /// </summary>
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

    /// <summary>
    /// Tests that setting the Y coordinate of a DataPoint to PositiveInfinity throws an ArgumentException.
    /// </summary>
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

    /// <summary>
    /// Tests that the Clone method produces an independent copy of a DataPoint with the same values.
    /// </summary>
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

    /// <summary>
    /// Tests that adding a data point to a ChartSeries increases its count.
    /// </summary>
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

    /// <summary>
    /// Tests that GetYAxisRange on an empty ChartSeries returns the default range of 0 to 1.
    /// </summary>
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

    /// <summary>
    /// Tests that GetYAxisRange on a ChartSeries with data points returns the actual minimum and maximum Y values.
    /// </summary>
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

    /// <summary>
    /// Tests that adding a null series to a Chart throws an ArgumentNullException.
    /// </summary>
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

    /// <summary>
    /// Tests that GetDataBounds on an empty Chart returns the default bounds of 0 to 1 for both X and Y axes.
    /// </summary>
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

    /// <summary>
    /// Tests that GetDataBounds on a Chart with series data returns the correct minimum and maximum bounds for both axes.
    /// </summary>
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

    /// <summary>
    /// Tests that ChartValidator.ValidateChart returns invalid when given null input.
    /// </summary>
    [Fact]
    public void ChartValidator_ValidateChart_NullInput_IsNotValid()
    {
        // Act
        var result = ChartValidator.ValidateChart(null!);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Contains("null"));
    }

    /// <summary>
    /// Tests that ChartValidator.ValidateChart adds an error when a Chart has no series.
    /// </summary>
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

    /// <summary>
    /// Tests that ChartValidator.ValidateSeries adds an error when a ChartSeries has an empty name.
    /// </summary>
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

    /// <summary>
    /// Tests that ChartValidator.ValidateConfiguration returns valid for a default configuration.
    /// </summary>
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

    /// <summary>
    /// Tests that ChartValidator.ValidateDataPoint returns invalid when given a null DataPoint.
    /// </summary>
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

    /// <summary>
    /// Tests that ColorHelper.HexToRgb converts pure red hex color (#FF0000) to the correct RGB string.
    /// </summary>
    [Fact]
    public void ColorHelper_HexToRgb_ForPureRed_ReturnsCorrectRgbString()
    {
        // Act
        var result = ColorHelper.HexToRgb("#FF0000");

        // Assert
        result.Should().Be("rgb(255, 0, 0)");
    }

    /// <summary>
    /// Tests that ColorHelper.RgbToHex converts blue RGB values (0, 0, 255) to the correct hex string (#0000FF).
    /// </summary>
    [Fact]
    public void ColorHelper_RgbToHex_ForBlue_ReturnsUpperCaseHex()
    {
        // Act
        var result = ColorHelper.RgbToHex(0, 0, 255);

        // Assert
        result.Should().Be("#0000FF");
    }

    /// <summary>
    /// Tests that ColorHelper.IsValidHexColor returns true for a valid six-character hex color string.
    /// </summary>
    [Fact]
    public void ColorHelper_IsValidHexColor_ValidSixCharHex_ReturnsTrue()
    {
        ColorHelper.IsValidHexColor("#1F77B4").Should().BeTrue();
    }

    /// <summary>
    /// Tests that ColorHelper.IsValidHexColor returns false when the hex color string is missing the hash prefix.
    /// </summary>
    [Fact]
    public void ColorHelper_IsValidHexColor_MissingHash_ReturnsFalse()
    {
        ColorHelper.IsValidHexColor("FF0000").Should().BeFalse();
    }

    /// <summary>
    /// Tests that ColorHelper.LightenColor increases the RGB channel values by the specified factor.
    /// </summary>
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

    /// <summary>
    /// Tests that DataPointExtensions.GetDistance calculates the Euclidean distance between two DataPoints correctly.
    /// This test uses a 3-4-5 right triangle to verify the calculation.
    /// </summary>
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

    /// <summary>
    /// Tests that DataPointExtensions.Offset shifts both X and Y coordinates by the specified amounts.
    /// </summary>
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

    /// <summary>
    /// Tests that DataPointExtensions.Scale multiplies both X and Y coordinates by the specified factors.
    /// </summary>
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