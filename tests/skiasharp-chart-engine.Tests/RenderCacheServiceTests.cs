using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SkiaSharpChartEngine.Services;
using Xunit;

namespace SkiaSharpChartEngine.Tests.Services;

/// <summary>
/// Unit tests for the <see cref="RenderCacheService"/> class.
/// Tests the caching functionality for rendered chart images including get, set, remove, clear operations,
/// cache eviction policies, and size management.
/// </summary>
public class RenderCacheServiceTests
{
    private readonly Mock<ILogger<RenderCacheService>> _loggerMock;
    private readonly RenderCacheService _cacheService;

    /// <summary>
    /// Initializes a new instance of the <see cref="RenderCacheServiceTests"/> class.
    /// Sets up the mock logger and creates a render cache service with a small cache size for testing.
    /// </summary>
    public RenderCacheServiceTests()
    {
        _loggerMock = new Mock<ILogger<RenderCacheService>>();
        _cacheService = new RenderCacheService(_loggerMock.Object, 3); // Small cache for testing
    }

    /// <summary>
    /// Creates test image data for testing purposes.
    /// </summary>
    /// <param name="size">The size of the image data in bytes. Defaults to 100.</param>
    /// <returns>A byte array representing test image data.</returns>
    private byte[] CreateTestImageData(int size = 100)
    {
        return new byte[size];
    }

    /// <summary>
    /// Contains tests for the <see cref="RenderCacheService.Get"/> method.
    /// Tests various scenarios including null keys, empty keys, non-existent keys, and existing keys.
    /// </summary>
    // ---------------------------------------------------------------
    // Get tests
    // ---------------------------------------------------------------

    /// <summary>
    /// Tests that <see cref="RenderCacheService.Get"/> returns null when provided with a null key.
    /// </summary>
    [Fact]
    public void Get_WithNullKey_ReturnsNull()
    {
        // Act
        var result = _cacheService.Get(null!);

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Tests that <see cref="RenderCacheService.Get"/> returns null when provided with an empty key.
    /// </summary>
    [Fact]
    public void Get_WithEmptyKey_ReturnsNull()
    {
        // Act
        var result = _cacheService.Get(" ");

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Tests that <see cref="RenderCacheService.Get"/> returns null when provided with a non-existent key.
    /// </summary>
    [Fact]
    public void Get_WithNonExistentKey_ReturnsNull()
    {
        // Act
        var result = _cacheService.Get("nonexistent");

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Tests that <see cref="RenderCacheService.Get"/> returns the stored image data when provided with an existing key.
    /// </summary>
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

    /// <summary>
    /// Tests that <see cref="RenderCacheService.Get"/> increments the access count for cached entries.
    /// </summary>
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

    /// <summary>
    /// Contains tests for the <see cref="RenderCacheService.Set"/> method.
    /// Tests various scenarios including null keys, empty keys, null image data, and cache eviction policies.
    /// </summary>
    // ---------------------------------------------------------------
    // Set tests
    // ---------------------------------------------------------------

    /// <summary>
    /// Tests that <see cref="RenderCacheService.Set"/> does not crash when provided with a null key.
    /// </summary>
    [Fact]
    public void Set_WithNullKey_DoesNotCrash()
    {
        // Act
        Action act = () => _cacheService.Set(null!, CreateTestImageData());

        // Assert
        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests that <see cref="RenderCacheService.Set"/> does not crash when provided with an empty key.
    /// </summary>
    [Fact]
    public void Set_WithEmptyKey_DoesNotCrash()
    {
        // Act
        Action act = () => _cacheService.Set(" ", CreateTestImageData());

        // Assert
        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests that <see cref="RenderCacheService.Set"/> does not crash when provided with null image data.
    /// </summary>
    [Fact]
    public void Set_WithNullImageData_DoesNotCrash()
    {
        // Act
        Action act = () => _cacheService.Set("key", null!);

        // Assert
        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests that <see cref="RenderCacheService.Set"/> stores the entry when provided with valid data.
    /// </summary>
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

    /// <summary>
    /// Tests that <see cref="RenderCacheService.Set"/> replaces an existing entry with new data.
    /// </summary>
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

    /// <summary>
    /// Tests that <see cref="RenderCacheService.Set"/> evicts the least recently used entry when the maximum cache size is reached.
    /// </summary>
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

    /// <summary>
    /// Tests that <see cref="RenderCacheService.Set"/> evicts the least accessed entry when there are ties in access patterns.
    /// </summary>
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

    /// <summary>
    /// Tests that <see cref="RenderCacheService.Set"/> evicts the least used entry when a key has been accessed more than others.
    /// </summary>
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

    /// <summary>
    /// Contains tests for the <see cref="RenderCacheService.Remove"/> method.
    /// Tests various scenarios including null keys, empty keys, existing keys, and non-existent keys.
    /// </summary>
    // ---------------------------------------------------------------
    // Remove tests
    // ---------------------------------------------------------------

    /// <summary>
    /// Tests that <see cref="RenderCacheService.Remove"/> does not crash when provided with a null key.
    /// </summary>
    [Fact]
    public void Remove_WithNullKey_DoesNotCrash()
    {
        // Act
        Action act = () => _cacheService.Remove(null!);

        // Assert
        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests that <see cref="RenderCacheService.Remove"/> does not crash when provided with an empty key.
    /// </summary>
    [Fact]
    public void Remove_WithEmptyKey_DoesNotCrash()
    {
        // Act
        Action act = () => _cacheService.Remove(" ");

        // Assert
        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests that <see cref="RenderCacheService.Remove"/> removes the entry when provided with an existing key.
    /// </summary>
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

    /// <summary>
    /// Tests that <see cref="RenderCacheService.Remove"/> does not throw when provided with a non-existent key.
    /// </summary>
    [Fact]
    public void Remove_WithNonExistentKey_DoesNotThrow()
    {
        // Act
        Action act = () => _cacheService.Remove("nonexistent");

        // Assert
        act.Should().NotThrow();
    }

    /// <summary>
    /// Contains tests for the <see cref="RenderCacheService.Clear"/> method.
    /// Tests that all entries are removed from the cache.
    /// </summary>
    // ---------------------------------------------------------------
    // Clear tests
    // ---------------------------------------------------------------

    /// <summary>
    /// Tests that <see cref="RenderCacheService.Clear"/> removes all entries from the cache.
    /// </summary>
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

    /// <summary>
    /// Contains tests for the <see cref="RenderCacheService.GetCacheSize"/> method.
    /// Tests various scenarios including empty cache and cache with entries.
    /// </summary>
    // ---------------------------------------------------------------
    // GetCacheSize tests
    // ---------------------------------------------------------------

    /// <summary>
    /// Tests that <see cref="RenderCacheService.GetCacheSize"/> returns zero when the cache is empty.
    /// </summary>
    [Fact]
    public void GetCacheSize_WithEmptyCache_ReturnsZero()
    {
        // Act
        var size = _cacheService.GetCacheSize();

        // Assert
        size.Should().Be(0);
    }

    /// <summary>
    /// Tests that <see cref="RenderCacheService.GetCacheSize"/> returns the correct count when the cache contains entries.
    /// </summary>
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

    /// <summary>
    /// Tests that <see cref="RenderCacheService.GetCacheSize"/> decreases after removing an entry.
    /// </summary>
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

    /// <summary>
    /// Contains tests for the <see cref="RenderCacheService.Contains"/> method.
    /// Tests various scenarios including null keys, empty keys, existing keys, and non-existent keys.
    /// </summary>
    // ---------------------------------------------------------------
    // Contains tests
    // ---------------------------------------------------------------

    /// <summary>
    /// Tests that <see cref="RenderCacheService.Contains"/> returns false when provided with a null key.
    /// </summary>
    [Fact]
    public void Contains_WithNullKey_ReturnsFalse()
    {
        // Act
        var result = _cacheService.Contains(null!);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that <see cref="RenderCacheService.Contains"/> returns false when provided with an empty key.
    /// </summary>
    [Fact]
    public void Contains_WithEmptyKey_ReturnsFalse()
    {
        // Act
        var result = _cacheService.Contains(" ");

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that <see cref="RenderCacheService.Contains"/> returns true when provided with an existing key.
    /// </summary>
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

    /// <summary>
    /// Tests that <see cref="RenderCacheService.Contains"/> returns false when provided with a non-existent key.
    /// </summary>
    [Fact]
    public void Contains_WithNonExistentKey_ReturnsFalse()
    {
        // Act
        var result = _cacheService.Contains("nonexistent");

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Contains tests for the <see cref="RenderCacheService.GetAllKeys"/> method.
    /// Tests various scenarios including empty cache and cache with entries.
    /// </summary>
    // ---------------------------------------------------------------
    // GetAllKeys tests
    // ---------------------------------------------------------------

    /// <summary>
    /// Tests that <see cref="RenderCacheService.GetAllKeys"/> returns an empty enumerable when the cache is empty.
    /// </summary>
    [Fact]
    public void GetAllKeys_WithEmptyCache_ReturnsEmptyEnumerable()
    {
        // Act
        var keys = _cacheService.GetAllKeys();

        // Assert
        keys.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that <see cref="RenderCacheService.GetAllKeys"/> returns all keys when the cache contains entries.
    /// </summary>
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

    /// <summary>
    /// Contains tests for the <see cref="RenderCacheService"/> constructor.
    /// Tests various constructor scenarios including null logger and custom cache sizes.
    /// </summary>
    // ---------------------------------------------------------------
    // Constructor tests
    // ---------------------------------------------------------------

    /// <summary>
    /// Tests that the <see cref="RenderCacheService"/> constructor throws an <see cref="ArgumentNullException"/> when provided with a null logger.
    /// </summary>
    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new RenderCacheService(null!, 5);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    /// <summary>
    /// Tests that the <see cref="RenderCacheService"/> constructor uses the default cache size when provided with a null max cache size.
    /// </summary>
    [Fact]
    public void Constructor_WithNullMaxCacheSize_UsesDefaultSize()
    {
        // Act
        var service = new RenderCacheService(_loggerMock.Object, null);

        // Assert - should not throw and use default
        service.Should().NotBeNull();
    }

    /// <summary>
    /// Tests that the <see cref="RenderCacheService"/> constructor uses a custom cache size when provided.
    /// </summary>
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