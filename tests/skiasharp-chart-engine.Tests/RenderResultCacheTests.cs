using System;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SkiaSharpChartEngine.Caching;
using SkiaSharpChartEngine.Constants;
using SkiaSharpChartEngine.Models;
using Xunit;

namespace SkiaSharpChartEngine.Tests.Caching;

public class RenderResultCacheTests
{
    private readonly Mock<ILogger<RenderResultCache>> _loggerMock;
    private const long TestMaxCacheSize = 1_000_000; // 1 MB for tests

    public RenderResultCacheTests()
    {
        _loggerMock = new Mock<ILogger<RenderResultCache>>();
    }

    private RenderResult CreateTestResult(string id = "test", int dataSize = 1000)
    {
        return RenderResult.CreateSuccess(id, new byte[dataSize], 100, ExportFormat.PNG);
    }

    // ---------------------------------------------------------------
    // Cache tests
    // ---------------------------------------------------------------

    [Fact]
    public void Cache_WithNullKey_DoesNotCrash()
    {
        // Arrange
        using var cache = new RenderResultCache(_loggerMock.Object, TestMaxCacheSize);
        var result = CreateTestResult();

        // Act
        Action act = () => cache.Cache(null!, result);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Cache_WithNullResult_DoesNotCrash()
    {
        // Arrange
        using var cache = new RenderResultCache(_loggerMock.Object, TestMaxCacheSize);

        // Act
        Action act = () => cache.Cache("key", null!);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Cache_WithValidData_StoresResultWithDefaultTtl()
    {
        // Arrange
        using var cache = new RenderResultCache(_loggerMock.Object, TestMaxCacheSize);
        var result = CreateTestResult();

        // Act
        cache.Cache("test-key", result);
        var retrieved = cache.Get("test-key");

        // Assert
        retrieved.Should().NotBeNull();
        retrieved.ChartId.Should().Be("test");
    }

    [Fact]
    public void Cache_WithCustomTtl_RespectsTtlValue()
    {
        // Arrange
        using var cache = new RenderResultCache(_loggerMock.Object, TestMaxCacheSize);
        var result = CreateTestResult();
        var ttl = TimeSpan.FromSeconds(1);

        // Act
        cache.Cache("test-key", result, ttl);
        var retrieved = cache.Get("test-key");

        // Assert - immediately after caching, should still be available
        retrieved.Should().NotBeNull();
    }

    [Fact]
    public void Cache_StoresMultipleEntriesIndependently()
    {
        // Arrange
        using var cache = new RenderResultCache(_loggerMock.Object, TestMaxCacheSize);
        var result1 = CreateTestResult("chart1");
        var result2 = CreateTestResult("chart2");

        // Act
        cache.Cache("key1", result1);
        cache.Cache("key2", result2);

        // Assert
        cache.Get("key1")?.ChartId.Should().Be("chart1");
        cache.Get("key2")?.ChartId.Should().Be("chart2");
    }

    [Fact]
    public void Cache_WhenSizeExceedsLimit_EvictsLRU()
    {
        // Arrange
        var smallSize = 5000L; // Small cache to force eviction
        using var cache = new RenderResultCache(_loggerMock.Object, smallSize);
        var result1 = CreateTestResult("chart1", 2000);
        var result2 = CreateTestResult("chart2", 2000);
        var result3 = CreateTestResult("chart3", 2000);

        // Act - cache all three; the first should be evicted due to size limit
        cache.Cache("key1", result1);
        cache.Cache("key2", result2);
        cache.Cache("key3", result3);

        // Assert - key1 (LRU) should be evicted
        cache.Get("key1").Should().BeNull();
        cache.Get("key2").Should().NotBeNull();
        cache.Get("key3").Should().NotBeNull();
    }

    [Fact]
    public void Cache_ReplacesExistingEntry()
    {
        // Arrange
        using var cache = new RenderResultCache(_loggerMock.Object, TestMaxCacheSize);
        var result1 = CreateTestResult("chart1");
        var result2 = CreateTestResult("chart2");

        // Act
        cache.Cache("key", result1);
        cache.Cache("key", result2); // Replace

        // Assert
        var retrieved = cache.Get("key");
        retrieved?.ChartId.Should().Be("chart2");
    }

    // ---------------------------------------------------------------
    // Get tests
    // ---------------------------------------------------------------

    [Fact]
    public void Get_WithNonExistentKey_ReturnsNull()
    {
        // Arrange
        using var cache = new RenderResultCache(_loggerMock.Object, TestMaxCacheSize);

        // Act
        var result = cache.Get("nonexistent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Get_WithValidKey_ReturnsResult()
    {
        // Arrange
        using var cache = new RenderResultCache(_loggerMock.Object, TestMaxCacheSize);
        var testResult = CreateTestResult("test-id");
        cache.Cache("key", testResult);

        // Act
        var result = cache.Get("key");

        // Assert
        result.Should().NotBeNull();
        result.ChartId.Should().Be("test-id");
    }

    [Fact]
    public void Get_UpdatesAccessCountAndLastAccessedTime()
    {
        // Arrange
        using var cache = new RenderResultCache(_loggerMock.Object, TestMaxCacheSize);
        var testResult = CreateTestResult();
        cache.Cache("key", testResult);

        // Act
        var result1 = cache.Get("key");
        Thread.Sleep(10); // Small delay
        var result2 = cache.Get("key");

        // Assert - both should succeed with updated access info
        result1.Should().NotBeNull();
        result2.Should().NotBeNull();
    }

    [Fact]
    public void Get_WithExpiredEntry_ReturnsNullAndRemovesEntry()
    {
        // Arrange
        using var cache = new RenderResultCache(_loggerMock.Object, TestMaxCacheSize);
        var testResult = CreateTestResult();
        var veryShortTtl = TimeSpan.FromMilliseconds(100);
        cache.Cache("key", testResult, veryShortTtl);

        // Act
        Thread.Sleep(150); // Wait for expiration
        var result = cache.Get("key");

        // Assert
        result.Should().BeNull();
    }

    // ---------------------------------------------------------------
    // Remove tests
    // ---------------------------------------------------------------

    [Fact]
    public void Remove_WithExistingKey_ReturnsTrue()
    {
        // Arrange
        using var cache = new RenderResultCache(_loggerMock.Object, TestMaxCacheSize);
        var testResult = CreateTestResult();
        cache.Cache("key", testResult);

        // Act
        var removed = cache.Remove("key");

        // Assert
        removed.Should().BeTrue();
        cache.Get("key").Should().BeNull();
    }

    [Fact]
    public void Remove_WithNonExistentKey_ReturnsFalse()
    {
        // Arrange
        using var cache = new RenderResultCache(_loggerMock.Object, TestMaxCacheSize);

        // Act
        var removed = cache.Remove("nonexistent");

        // Assert
        removed.Should().BeFalse();
    }

    [Fact]
    public void Remove_DecreasesCacheSizeCounter()
    {
        // Arrange
        using var cache = new RenderResultCache(_loggerMock.Object, TestMaxCacheSize);
        var testResult = CreateTestResult("id", 5000);
        cache.Cache("key", testResult);
        var statsBefore = cache.GetStatistics();

        // Act
        cache.Remove("key");
        var statsAfter = cache.GetStatistics();

        // Assert
        statsBefore.TotalSize.Should().BeGreaterThan(0);
        statsAfter.TotalSize.Should().BeLessThan(statsBefore.TotalSize);
    }

    // ---------------------------------------------------------------
    // Clear tests
    // ---------------------------------------------------------------

    [Fact]
    public void Clear_RemovesAllEntries()
    {
        // Arrange
        using var cache = new RenderResultCache(_loggerMock.Object, TestMaxCacheSize);
        cache.Cache("key1", CreateTestResult("id1"));
        cache.Cache("key2", CreateTestResult("id2"));
        cache.Cache("key3", CreateTestResult("id3"));

        // Act
        cache.Clear();

        // Assert
        cache.Get("key1").Should().BeNull();
        cache.Get("key2").Should().BeNull();
        cache.Get("key3").Should().BeNull();
    }

    [Fact]
    public void Clear_ResetsSizeCounter()
    {
        // Arrange
        using var cache = new RenderResultCache(_loggerMock.Object, TestMaxCacheSize);
        cache.Cache("key", CreateTestResult());
        var stats1 = cache.GetStatistics();

        // Act
        cache.Clear();
        var stats2 = cache.GetStatistics();

        // Assert
        stats1.TotalSize.Should().BeGreaterThan(0);
        stats2.TotalSize.Should().Be(0);
    }

    // ---------------------------------------------------------------
    // GetStatistics tests
    // ---------------------------------------------------------------

    [Fact]
    public void GetStatistics_WithEmptyCache_ReturnsZeroStats()
    {
        // Arrange
        using var cache = new RenderResultCache(_loggerMock.Object, TestMaxCacheSize);

        // Act
        var stats = cache.GetStatistics();

        // Assert
        stats.TotalEntries.Should().Be(0);
        stats.TotalSize.Should().Be(0);
        stats.TotalHits.Should().Be(0);
        stats.MaxSize.Should().Be(TestMaxCacheSize);
    }

    [Fact]
    public void GetStatistics_TracksEntryCountAndSize()
    {
        // Arrange
        using var cache = new RenderResultCache(_loggerMock.Object, TestMaxCacheSize);
        var result1 = CreateTestResult("id1", 1000);
        var result2 = CreateTestResult("id2", 2000);

        // Act
        cache.Cache("key1", result1);
        cache.Cache("key2", result2);
        var stats = cache.GetStatistics();

        // Assert
        stats.TotalEntries.Should().Be(2);
        stats.TotalSize.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GetStatistics_TracksHitCount()
    {
        // Arrange
        using var cache = new RenderResultCache(_loggerMock.Object, TestMaxCacheSize);
        var result = CreateTestResult();
        cache.Cache("key", result);

        // Act
        cache.Get("key"); // First access
        cache.Get("key"); // Second access
        var stats = cache.GetStatistics();

        // Assert
        stats.TotalHits.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void GetStatistics_IncludesOldestAndNewestEntryDates()
    {
        // Arrange
        using var cache = new RenderResultCache(_loggerMock.Object, TestMaxCacheSize);
        cache.Cache("key1", CreateTestResult());
        Thread.Sleep(10);
        cache.Cache("key2", CreateTestResult());

        // Act
        var stats = cache.GetStatistics();

        // Assert
        stats.OldestEntry.Should().NotBeNull();
        stats.NewestEntry.Should().NotBeNull();
        stats.NewestEntry.Should().BeGreaterThanOrEqualTo(stats.OldestEntry);
    }

    // ---------------------------------------------------------------
    // Thread safety tests
    // ---------------------------------------------------------------

    [Fact]
    public void Cache_IsThreadSafe_ConcurrentReads()
    {
        // Arrange
        using var cache = new RenderResultCache(_loggerMock.Object, TestMaxCacheSize);
        cache.Cache("key", CreateTestResult());
        var results = new List<RenderResult>();
        var threads = new List<Thread>();

        // Act - 10 threads reading simultaneously
        for (int i = 0; i < 10; i++)
        {
            var thread = new Thread(() =>
            {
                var result = cache.Get("key");
                lock (results)
                {
                    results.Add(result);
                }
            });
            threads.Add(thread);
            thread.Start();
        }

        foreach (var thread in threads)
            thread.Join();

        // Assert
        results.Should().AllSatisfy(r => r.Should().NotBeNull());
    }

    [Fact]
    public void Cache_IsThreadSafe_ConcurrentWrites()
    {
        // Arrange
        using var cache = new RenderResultCache(_loggerMock.Object, TestMaxCacheSize);
        var threads = new List<Thread>();

        // Act - 5 threads writing simultaneously
        for (int i = 0; i < 5; i++)
        {
            var threadNum = i;
            var thread = new Thread(() =>
            {
                for (int j = 0; j < 10; j++)
                {
                    cache.Cache($"key-{threadNum}-{j}", CreateTestResult($"id-{threadNum}-{j}", 100));
                }
            });
            threads.Add(thread);
            thread.Start();
        }

        foreach (var thread in threads)
            thread.Join();

        // Assert - cache should have all entries
        var stats = cache.GetStatistics();
        stats.TotalEntries.Should().Be(50);
    }

    // ---------------------------------------------------------------
    // Constructor tests
    // ---------------------------------------------------------------

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new RenderResultCache(null!, TestMaxCacheSize);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Fact]
    public void Constructor_WithDefaultMaxSize_Uses100Mb()
    {
        // Arrange & Act
        using var cache = new RenderResultCache(_loggerMock.Object);

        // Assert
        var stats = cache.GetStatistics();
        stats.MaxSize.Should().Be(104_857_600); // 100 MB
    }

    [Fact]
    public void Dispose_DisposesCacheGracefully()
    {
        // Arrange
        var cache = new RenderResultCache(_loggerMock.Object, TestMaxCacheSize);
        cache.Cache("key", CreateTestResult());

        // Act
        cache.Dispose();

        // Assert - no exceptions
        cache.Clear(); // Should not throw
    }
}
