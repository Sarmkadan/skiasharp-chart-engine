using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Utilities;
using Xunit;

namespace SkiaSharpChartEngine.Tests.Utilities;

public class DataAggregatorTests
{
    private readonly Mock<ILogger<DataAggregator>> _loggerMock;
    private readonly DataAggregator _aggregator;

    public DataAggregatorTests()
    {
        _loggerMock = new Mock<ILogger<DataAggregator>>();
        _aggregator = new DataAggregator(_loggerMock.Object);
    }

    // ---------------------------------------------------------------
    // AggregateByCount tests
    // ---------------------------------------------------------------

    [Fact]
    public void AggregateByCount_WithNullDataPoints_ReturnsEmptyList()
    {
        // Act
        var result = _aggregator.AggregateByCount(null!, 5, AggregationType.Average);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void AggregateByCount_WithEmptyDataPoints_ReturnsEmptyList()
    {
        // Arrange
        var dataPoints = new List<DataPoint>();

        // Act
        var result = _aggregator.AggregateByCount(dataPoints, 5, AggregationType.Average);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void AggregateByCount_WithZeroBucketCount_ThrowsArgumentException()
    {
        // Arrange
        var dataPoints = new List<DataPoint> { new DataPoint(1.0, 100.0) };

        // Act
        Action act = () => _aggregator.AggregateByCount(dataPoints, 0, AggregationType.Average);

        // Assert
        act.Should().Throw<ArgumentException>().WithParameterName("bucketCount");
    }

    [Fact]
    public void AggregateByCount_WithNegativeBucketCount_ThrowsArgumentException()
    {
        // Arrange
        var dataPoints = new List<DataPoint> { new DataPoint(1.0, 100.0) };

        // Act
        Action act = () => _aggregator.AggregateByCount(dataPoints, -1, AggregationType.Average);

        // Assert
        act.Should().Throw<ArgumentException>().WithParameterName("bucketCount");
    }

    [Fact]
    public void AggregateByCount_WithAverageAggregation_ComputesAverageBuckets()
    {
        // Arrange
        var dataPoints = new List<DataPoint>
        {
            new DataPoint(1.0, 10.0),
            new DataPoint(2.0, 20.0),
            new DataPoint(3.0, 30.0),
            new DataPoint(4.0, 40.0)
        };

        // Act - aggregate into 2 buckets
        var result = _aggregator.AggregateByCount(dataPoints, 2, AggregationType.Average);

        // Assert
        result.Should().HaveCount(2);
        result[0].Value.Should().BeApproximately(15.0, 0.01); // (10+20)/2
        result[1].Value.Should().BeApproximately(35.0, 0.01); // (30+40)/2
    }

    [Fact]
    public void AggregateByCount_WithSumAggregation_ComputesSumBuckets()
    {
        // Arrange
        var dataPoints = new List<DataPoint>
        {
            new DataPoint(1.0, 10.0),
            new DataPoint(2.0, 20.0),
            new DataPoint(3.0, 30.0),
            new DataPoint(4.0, 40.0)
        };

        // Act - aggregate into 2 buckets
        var result = _aggregator.AggregateByCount(dataPoints, 2, AggregationType.Sum);

        // Assert
        result.Should().HaveCount(2);
        result[0].Value.Should().Be(30.0); // 10+20
        result[1].Value.Should().Be(70.0); // 30+40
    }

    [Fact]
    public void AggregateByCount_WithMinAggregation_ComputesMinBuckets()
    {
        // Arrange
        var dataPoints = new List<DataPoint>
        {
            new DataPoint(1.0, 50.0),
            new DataPoint(2.0, 10.0),
            new DataPoint(3.0, 40.0),
            new DataPoint(4.0, 30.0)
        };

        // Act
        var result = _aggregator.AggregateByCount(dataPoints, 2, AggregationType.Min);

        // Assert
        result.Should().HaveCount(2);
        result[0].Value.Should().Be(10.0); // min(50, 10)
        result[1].Value.Should().Be(30.0); // min(40, 30)
    }

    [Fact]
    public void AggregateByCount_WithMaxAggregation_ComputesMaxBuckets()
    {
        // Arrange
        var dataPoints = new List<DataPoint>
        {
            new DataPoint(1.0, 50.0),
            new DataPoint(2.0, 10.0),
            new DataPoint(3.0, 40.0),
            new DataPoint(4.0, 30.0)
        };

        // Act
        var result = _aggregator.AggregateByCount(dataPoints, 2, AggregationType.Max);

        // Assert
        result.Should().HaveCount(2);
        result[0].Value.Should().Be(50.0); // max(50, 10)
        result[1].Value.Should().Be(40.0); // max(40, 30)
    }

    [Fact]
    public void AggregateByCount_WithMedianAggregation_ComputesMedianBuckets()
    {
        // Arrange
        var dataPoints = new List<DataPoint>
        {
            new DataPoint(1.0, 10.0),
            new DataPoint(2.0, 20.0),
            new DataPoint(3.0, 30.0),
            new DataPoint(4.0, 40.0)
        };

        // Act
        var result = _aggregator.AggregateByCount(dataPoints, 2, AggregationType.Median);

        // Assert
        result.Should().HaveCount(2);
        result[0].Value.Should().BeApproximately(15.0, 0.01); // median(10, 20)
        result[1].Value.Should().BeApproximately(35.0, 0.01); // median(30, 40)
    }

    [Fact]
    public void AggregateByCount_WithMoreBucketsThanPoints_CreatesOnePointPerBucket()
    {
        // Arrange
        var dataPoints = new List<DataPoint>
        {
            new DataPoint(1.0, 100.0),
            new DataPoint(2.0, 200.0)
        };

        // Act
        var result = _aggregator.AggregateByCount(dataPoints, 5, AggregationType.Average);

        // Assert - should only aggregate existing points
        result.Count.Should().BeLessThanOrEqualTo(dataPoints.Count);
    }

    // ---------------------------------------------------------------
    // AggregateByInterval tests
    // ---------------------------------------------------------------

    [Fact]
    public void AggregateByInterval_WithNullDataPoints_ReturnsEmptyDictionary()
    {
        // Act
        var result = _aggregator.AggregateByInterval(null!, AggregationType.Average);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void AggregateByInterval_GroupsByLabel()
    {
        // Arrange
        var dataPoints = new List<DataPoint>
        {
            new DataPoint(1.0, 100.0) { Label = "Q1" },
            new DataPoint(2.0, 150.0) { Label = "Q1" },
            new DataPoint(3.0, 200.0) { Label = "Q2" },
            new DataPoint(4.0, 250.0) { Label = "Q2" }
        };

        // Act
        var result = _aggregator.AggregateByInterval(dataPoints, AggregationType.Average);

        // Assert
        result.Should().HaveCount(2);
        result["Q1"].Should().HaveCount(2);
        result["Q2"].Should().HaveCount(2);
    }

    [Fact]
    public void AggregateByInterval_WithNullLabel_GroupsAsUnknown()
    {
        // Arrange
        var dataPoints = new List<DataPoint>
        {
            new DataPoint(1.0, 100.0) { Label = null },
            new DataPoint(2.0, 150.0) { Label = null }
        };

        // Act
        var result = _aggregator.AggregateByInterval(dataPoints, AggregationType.Average);

        // Assert
        result.Should().ContainKey("unknown");
        result["unknown"].Should().HaveCount(2);
    }

    // ---------------------------------------------------------------
    // CalculateStatistics tests
    // ---------------------------------------------------------------

    [Fact]
    public void CalculateStatistics_WithNullDataPoints_ReturnsNull()
    {
        // Act
        var result = _aggregator.CalculateStatistics(null!);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void CalculateStatistics_WithEmptyDataPoints_ReturnsNull()
    {
        // Arrange
        var dataPoints = new List<DataPoint>();

        // Act
        var result = _aggregator.CalculateStatistics(dataPoints);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void CalculateStatistics_ComputesSumAndAverage()
    {
        // Arrange
        var dataPoints = new List<DataPoint>
        {
            new DataPoint(1.0, 10.0),
            new DataPoint(2.0, 20.0),
            new DataPoint(3.0, 30.0)
        };

        // Act
        var result = _aggregator.CalculateStatistics(dataPoints);

        // Assert
        result.Should().NotBeNull();
        result!.Sum.Should().Be(60.0);
        result.Average.Should().Be(20.0);
        result.Count.Should().Be(3);
    }

    [Fact]
    public void CalculateStatistics_ComputesMinAndMax()
    {
        // Arrange
        var dataPoints = new List<DataPoint>
        {
            new DataPoint(1.0, 50.0),
            new DataPoint(2.0, 10.0),
            new DataPoint(3.0, 40.0)
        };

        // Act
        var result = _aggregator.CalculateStatistics(dataPoints);

        // Assert
        result.Should().NotBeNull();
        result!.Min.Should().Be(10.0);
        result.Max.Should().Be(50.0);
    }

    [Fact]
    public void CalculateStatistics_ComputesMedian()
    {
        // Arrange
        var dataPoints = new List<DataPoint>
        {
            new DataPoint(1.0, 10.0),
            new DataPoint(2.0, 20.0),
            new DataPoint(3.0, 30.0),
            new DataPoint(4.0, 40.0),
            new DataPoint(5.0, 50.0)
        };

        // Act
        var result = _aggregator.CalculateStatistics(dataPoints);

        // Assert
        result.Should().NotBeNull();
        result!.Median.Should().Be(30.0);
    }

    [Fact]
    public void CalculateStatistics_ComputesRange()
    {
        // Arrange
        var dataPoints = new List<DataPoint>
        {
            new DataPoint(1.0, 10.0),
            new DataPoint(2.0, 100.0)
        };

        // Act
        var result = _aggregator.CalculateStatistics(dataPoints);

        // Assert
        result.Should().NotBeNull();
        result!.Range.Should().Be(90.0);
    }

    [Fact]
    public void CalculateStatistics_ComputesStandardDeviation()
    {
        // Arrange
        var dataPoints = new List<DataPoint>
        {
            new DataPoint(1.0, 10.0),
            new DataPoint(2.0, 20.0),
            new DataPoint(3.0, 30.0)
        };

        // Act
        var result = _aggregator.CalculateStatistics(dataPoints);

        // Assert
        result.Should().NotBeNull();
        result!.StandardDeviation.Should().BeGreaterThan(0);
    }

    [Fact]
    public void CalculateStatistics_IncludesCalculatedAtTimestamp()
    {
        // Arrange
        var dataPoints = new List<DataPoint> { new DataPoint(1.0, 100.0) };
        var beforeTime = DateTime.UtcNow;

        // Act
        var result = _aggregator.CalculateStatistics(dataPoints);

        // Assert
        result.Should().NotBeNull();
        result!.CalculatedAt.Should().BeOnOrAfter(beforeTime);
    }

    // ---------------------------------------------------------------
    // Error handling tests
    // ---------------------------------------------------------------

    [Fact]
    public void AggregateByCount_WithInvalidAggregationType_ReturnsAverageFallback()
    {
        // Arrange
        var dataPoints = new List<DataPoint>
        {
            new DataPoint(1.0, 10.0),
            new DataPoint(2.0, 20.0)
        };

        // Act - use invalid aggregation type (should fall back to average)
        var result = _aggregator.AggregateByCount(dataPoints, 1, (AggregationType)999);

        // Assert
        result.Should().HaveCount(1);
        result[0].Value.Should().BeApproximately(15.0, 0.01);
    }

    // ---------------------------------------------------------------
    // Constructor tests
    // ---------------------------------------------------------------

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new DataAggregator(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }
}
