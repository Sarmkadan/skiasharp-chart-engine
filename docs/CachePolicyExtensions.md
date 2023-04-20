# CachePolicyExtensions

Provides extension methods for configuring and validating `CachePolicy` instances in SkiaSharp.Chart.Engine. These methods allow fine-grained control over cache expiration, eviction callbacks, and priority settings to optimize memory usage and performance in chart rendering scenarios.

## API

### `WithPriority`
Configures the priority level of a `CachePolicy` instance.

- **Parameters**
  - `this CachePolicy policy`: The policy to configure.
  - `CacheItemPriority priority`: The priority level to assign (e.g., `CacheItemPriority.Normal`, `CacheItemPriority.High`).

- **Return Value**
  Returns the same `CachePolicy` instance with its priority set to the specified value.

- **Exceptions**
  Throws `ArgumentOutOfRangeException` if `priority` is not a defined `CacheItemPriority` value.

---

### `WithPostEvictionCallback`
Adds a callback to be invoked after an item is evicted from the cache.

- **Parameters**
  - `this CachePolicy policy`: The policy to configure.
  - `Action<EvictionReason, object> callback`: A delegate that accepts the eviction reason and the evicted item.

- **Return Value**
  Returns the same `CachePolicy` instance with the callback registered.

- **Exceptions**
  Throws `ArgumentNullException` if `callback` is `null`.

---

### `WithExpiration`
Sets an expiration policy for the cache.

- **Parameters**
  - `this CachePolicy policy`: The policy to configure.
  - `TimeSpan expiration`: The duration after which items should expire.

- **Return Value**
  Returns the same `CachePolicy` instance with the expiration time set.

- **Exceptions**
  Throws `ArgumentOutOfRangeException` if `expiration` is negative or exceeds the maximum allowed duration.

---

### `EnsureValidSlidingExpiration`
Validates that a sliding expiration duration is within acceptable bounds.

- **Parameters**
  - `this CachePolicy policy`: The policy to validate.
  - `TimeSpan slidingExpiration`: The sliding expiration duration to check.

- **Return Value**
  Returns the same `CachePolicy` instance if the sliding expiration is valid.

- **Exceptions**
  Throws `ArgumentOutOfRangeException` if `slidingExpiration` is negative, zero, or exceeds the maximum allowed duration.

## Usage

### Example 1: Configuring a Cache with Priority and Expiration
