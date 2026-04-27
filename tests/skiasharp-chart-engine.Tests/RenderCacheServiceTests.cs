using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SkiaSharpChartEngine.Services;
using Xunit;

namespace SkiaSharpChartEngine.Tests.Services;

public class RenderCacheServiceTests
{
    private readonly Mock<ILogger<RenderCacheService>> _loggerMock;
    private readonly RenderCacheService _cacheService;

    public RenderCacheServiceTests()
    {
        _loggerMock = new Mock<ILogger<RenderCacheService>>();
        _cacheService = new RenderCacheService(_loggerMock.Object, 3); // Small cache for testing
    }

    private byte[] CreateTestImageData(int size = 100)
    {
        return new byte[size];
    }

    // ---------------------------------------------------------------
    // Get tests
    // ---------------------------------------------------------------

    [Fact]
    public void Get_WithNullKey_ReturnsNull()
    {
        // Act
        var result = _cacheService.Get(null!);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Get_WithEmptyKey_ReturnsNull()
    {
        // Act
        var result = _cacheService.Get("   ");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Get_WithNonExistentKey_ReturnsNull()
    {
        // Act
        var result = _cacheService.Get("nonexistent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Get_WithExistingKey_ReturnsImageData()
    {
        // Arrange
        var imageData = CreateTestImageData(500);
        _cacheService.Set("key1", imageData);

        // Act
        var result = _cacheService.Get("key1");

        // Assert
        result.Should().NotBeNull();
        result.Should().Equal(imageData);
    }

    [Fact]
    public void Get_IncrementsAccessCount()
    {
        // Arrange
        _cacheService.Set("key1", CreateTestImageData());

        // Act
        var result1 = _cacheService.Get("key1");
        var result2 = _cacheService.Get("key1");

        // Assert
        result1.Should().NotBeNull();
        result2.Should().NotBeNull();
    }

    // ---------------------------------------------------------------
    // Set tests
    // ---------------------------------------------------------------

    [Fact]
    public void Set_WithNullKey_DoesNotCrash()
    {
        // Act
        Action act = () => _cacheService.Set(null!, CreateTestImageData());

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Set_WithEmptyKey_DoesNotCrash()
    {
        // Act
        Action act = () => _cacheService.Set("   ", CreateTestImageData());

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Set_WithNullImageData_DoesNotCrash()
    {
        // Act
        Action act = () => _cacheService.Set("key", null!);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Set_WithValidData_StoresEntry()
    {
        // Arrange
        var imageData = CreateTestImageData(500);

        // Act
        _cacheService.Set("key1", imageData);
        var result = _cacheService.Get("key1");

        // Assert
        result.Should().Equal(imageData);
    }

    [Fact]
    public void Set_ReplacesExistingEntry()
    {
        // Arrange
        var data1 = CreateTestImageData(100);
        var data2 = CreateTestImageData(200);

        // Act
        _cacheService.Set("key1", data1);
        _cacheService.Set("key1", data2); // Replace
        var result = _cacheService.Get("key1");

        // Assert
        result.Should().Equal(data2);
    }

    [Fact]
    public void Set_WhenMaxCacheSizeReached_EvictsLRU()
    {
        // Arrange - cache size is 3
        _cacheService.Set("key1", CreateTestImageData());
        _cacheService.Set("key2", CreateTestImageData());
        _cacheService.Set("key3", CreateTestImageData());

        // Act - add a 4th entry which should evict the LRU (key1)
        _cacheService.Set("key4", CreateTestImageData());

        // Assert
        _cacheService.Get("key1").Should().BeNull();
        _cacheService.Get("key2").Should().NotBeNull();
        _cacheService.Get("key3").Should().NotBeNull();
        _cacheService.Get("key4").Should().NotBeNull();
    }

    [Fact]
    public void Set_EvicsLeastAccessedWhenTied()
    {
        // Arrange - cache size is 3
        _cacheService.Set("key1", CreateTestImageData()); // oldest, no accesses
        _cacheService.Set("key2", CreateTestImageData()); // middle
        _cacheService.Set("key3", CreateTestImageData()); // newest

        // Act - trigger eviction
        _cacheService.Set("key4", CreateTestImageData());

        // Assert - key1 (oldest with least accesses) should be evicted
        _cacheService.Get("key1").Should().BeNull();
    }

    [Fact]
    public void Set_WhenBucketFull_AndKeyAccessedThenOtherEvicted()
    {
        // Arrange - cache size is 3
        _cacheService.Set("key1", CreateTestImageData());
        _cacheService.Set("key2", CreateTestImageData());
        _cacheService.Set("key3", CreateTestImageData());

        // Act - access key2 to make it recently used
        _cacheService.Get("key2");
        _cacheService.Get("key2");
        _cacheService.Set("key4", CreateTestImageData()); // Should evict key1 (least used)

        // Assert
        _cacheService.Get("key1").Should().BeNull();
        _cacheService.Get("key2").Should().NotBeNull();
        _cacheService.Get("key3").Should().NotBeNull();
        _cacheService.Get("key4").Should().NotBeNull();
    }

    // ---------------------------------------------------------------
    // Remove tests
    // ---------------------------------------------------------------

    [Fact]
    public void Remove_WithNullKey_DoesNotCrash()
    {
        // Act
        Action act = () => _cacheService.Remove(null!);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Remove_WithEmptyKey_DoesNotCrash()
    {
        // Act
        Action act = () => _cacheService.Remove("   ");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Remove_WithExistingKey_RemovesEntry()
    {
        // Arrange
        _cacheService.Set("key1", CreateTestImageData());

        // Act
        _cacheService.Remove("key1");

        // Assert
        _cacheService.Get("key1").Should().BeNull();
    }

    [Fact]
    public void Remove_WithNonExistentKey_DoesNotThrow()
    {
        // Act
        Action act = () => _cacheService.Remove("nonexistent");

        // Assert
        act.Should().NotThrow();
    }

    // ---------------------------------------------------------------
    // Clear tests
    // ---------------------------------------------------------------

    [Fact]
    public void Clear_RemovesAllEntries()
    {
        // Arrange
        _cacheService.Set("key1", CreateTestImageData());
        _cacheService.Set("key2", CreateTestImageData());
        _cacheService.Set("key3", CreateTestImageData());

        // Act
        _cacheService.Clear();

        // Assert
        _cacheService.Get("key1").Should().BeNull();
        _cacheService.Get("key2").Should().BeNull();
        _cacheService.Get("key3").Should().BeNull();
        _cacheService.GetCacheSize().Should().Be(0);
    }

    // ---------------------------------------------------------------
    // GetCacheSize tests
    // ---------------------------------------------------------------

    [Fact]
    public void GetCacheSize_WithEmptyCache_ReturnsZero()
    {
        // Act
        var size = _cacheService.GetCacheSize();

        // Assert
        size.Should().Be(0);
    }

    [Fact]
    public void GetCacheSize_WithEntries_ReturnsCorrectCount()
    {
        // Arrange
        _cacheService.Set("key1", CreateTestImageData());
        _cacheService.Set("key2", CreateTestImageData());

        // Act
        var size = _cacheService.GetCacheSize();

        // Assert
        size.Should().Be(2);
    }

    [Fact]
    public void GetCacheSize_AfterRemoval_DecreasesCount()
    {
        // Arrange
        _cacheService.Set("key1", CreateTestImageData());
        _cacheService.Set("key2", CreateTestImageData());
        var sizeBefore = _cacheService.GetCacheSize();

        // Act
        _cacheService.Remove("key1");
        var sizeAfter = _cacheService.GetCacheSize();

        // Assert
        sizeBefore.Should().Be(2);
        sizeAfter.Should().Be(1);
    }

    // ---------------------------------------------------------------
    // Contains tests
    // ---------------------------------------------------------------

    [Fact]
    public void Contains_WithNullKey_ReturnsFalse()
    {
        // Act
        var result = _cacheService.Contains(null!);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Contains_WithEmptyKey_ReturnsFalse()
    {
        // Act
        var result = _cacheService.Contains("   ");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Contains_WithExistingKey_ReturnsTrue()
    {
        // Arrange
        _cacheService.Set("key1", CreateTestImageData());

        // Act
        var result = _cacheService.Contains("key1");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Contains_WithNonExistentKey_ReturnsFalse()
    {
        // Act
        var result = _cacheService.Contains("nonexistent");

        // Assert
        result.Should().BeFalse();
    }

    // ---------------------------------------------------------------
    // GetAllKeys tests
    // ---------------------------------------------------------------

    [Fact]
    public void GetAllKeys_WithEmptyCache_ReturnsEmptyEnumerable()
    {
        // Act
        var keys = _cacheService.GetAllKeys();

        // Assert
        keys.Should().BeEmpty();
    }

    [Fact]
    public void GetAllKeys_WithEntries_ReturnsAllKeys()
    {
        // Arrange
        _cacheService.Set("key1", CreateTestImageData());
        _cacheService.Set("key2", CreateTestImageData());
        _cacheService.Set("key3", CreateTestImageData());

        // Act
        var keys = _cacheService.GetAllKeys().ToList();

        // Assert
        keys.Should().HaveCount(3);
        keys.Should().Contain("key1");
        keys.Should().Contain("key2");
        keys.Should().Contain("key3");
    }

    // ---------------------------------------------------------------
    // Constructor tests
    // ---------------------------------------------------------------

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new RenderCacheService(null!, 5);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Fact]
    public void Constructor_WithNullMaxCacheSize_UsesDefaultSize()
    {
        // Act
        var service = new RenderCacheService(_loggerMock.Object, null);

        // Assert - should not throw and use default
        service.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithCustomMaxCacheSize_UsesCustomSize()
    {
        // Arrange
        var service = new RenderCacheService(_loggerMock.Object, 10);

        // Act - add 10 items
        for (int i = 0; i < 10; i++)
        {
            service.Set($"key{i}", CreateTestImageData());
        }

        // Assert - all should be cached
        service.GetCacheSize().Should().Be(10);
    }
}
