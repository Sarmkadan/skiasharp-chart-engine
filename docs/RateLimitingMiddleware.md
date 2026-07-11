# RateLimitingMiddleware

`RateLimitingMiddleware` is a server-side component that enforces per-client request rate limits using a token-bucket algorithm. It tracks token consumption by client identifier, allows or denies incoming requests based on available tokens, and exposes current rate-limit state for monitoring and administrative reset operations.

## API

### public RateLimitingMiddleware

Constructor. Initializes a new instance of the middleware with the specified token-bucket capacity and refill interval. The internal storage for client buckets is prepared but initially empty.

- **Parameters**: Not explicitly listed in the public surface, but inferred from `MaxTokens` and `RefillIntervalSeconds` properties — expects a maximum token count and a refill interval in seconds.
- **Exceptions**: May throw `ArgumentOutOfRangeException` if `MaxTokens` is less than zero or `RefillIntervalSeconds` is less than or equal to zero.

### public bool AllowRequest

Evaluates whether a request identified by the current client identifier should be permitted. Consumes one token if the bucket has sufficient capacity; otherwise the request is denied.

- **Returns**: `true` if a token was available and consumed; `false` if the bucket is empty.
- **Remarks**: The client identifier is obtained via `CustomIdentifierExtractor` if set, otherwise through a default extraction mechanism (e.g., remote IP address). This method is thread-safe.

### public RateLimitInfo? GetRateLimitInfo

Retrieves the current rate-limit state for the client associated with the active request context, without consuming a token.

- **Returns**: A `RateLimitInfo` value containing `AvailableTokens`, `MaxTokens`, and `ResetTime` for the client, or `null` if no bucket exists for that client (i.e., the client has never made a request).
- **Remarks**: Does not mutate bucket state. Safe to call for diagnostic or header-injection purposes.

### public void ResetClientLimit

Resets the token bucket for a specified client identifier to its full capacity (`MaxTokens`) and sets the next reset time according to the refill schedule. If no bucket exists for the client, one is created.

- **Parameters**: A client identifier string (inferred from usage — the method requires a key to locate the bucket).
- **Exceptions**: May throw `ArgumentNullException` if the client identifier is `null`.
- **Remarks**: This is an administrative action; it discards any accumulated debt and restores the client to a clean state.

### public void Dispose

Releases all resources held by the middleware, including timers or background tasks used for bucket cleanup. After disposal, further calls to `AllowRequest` or other methods will likely throw `ObjectDisposedException`.

- **Remarks**: Must be called when the middleware is removed from the pipeline or the host shuts down.

### public TokenBucket

Represents the per-client token bucket instance. This property exposes the underlying bucket object for the current client context, allowing direct inspection or manipulation.

- **Type**: `TokenBucket` (a nested or related class).
- **Remarks**: Accessing this property when no client context is established may return `null` or throw, depending on implementation.

### public bool TryConsumeToken

Attempts to consume a single token from the current client’s bucket. Returns a boolean indicating success.

- **Returns**: `true` if a token was consumed; `false` if no tokens remain.
- **Remarks**: Unlike `AllowRequest`, this method does not necessarily integrate with the broader request-pipeline decision logic; it is a lower-level operation on the bucket itself. Thread-safe.

### public RateLimitInfo GetCurrentInfo

Returns the current rate-limit state for the current client context. Similar to `GetRateLimitInfo` but guaranteed to return a value (creates a new bucket if one does not exist).

- **Returns**: A `RateLimitInfo` struct or object with `AvailableTokens`, `MaxTokens`, and `ResetTime`.
- **Remarks**: This method always succeeds and never returns `null`. It may be used to seed a bucket for a first-time client.

### public bool IsExpired

Indicates whether the current client’s token bucket has expired due to inactivity. Expired buckets are candidates for cleanup.

- **Returns**: `true` if the bucket has not been accessed within the expiration window and should be purged; otherwise `false`.
- **Remarks**: The expiration policy is typically tied to the refill interval or a multiple thereof.

### public long MaxTokens

Gets the maximum number of tokens the bucket can hold. This is the burst capacity allowed for a client.

- **Remarks**: Set at construction time and immutable thereafter.

### public int RefillIntervalSeconds

Gets the interval, in seconds, at which tokens are replenished. One token is added per interval until `MaxTokens` is reached.

- **Remarks**: Set at construction time and immutable thereafter.

### public Func<string>? CustomIdentifierExtractor

Gets or sets a delegate that extracts a client identifier string from the current request context. When `null`, the middleware falls back to a default extraction strategy.

- **Remarks**: Assign a custom function to key rate limits on API keys, user IDs, or other headers. The delegate is invoked on every call to `AllowRequest` or related methods. Must be thread-safe if the middleware is used concurrently.

### public long AvailableTokens

Gets the number of tokens currently available in the bucket for the active client context.

- **Remarks**: This value is a point-in-time snapshot and may change immediately after reading due to other requests.

### public long MaxTokens

(Already described above; listed again in the public surface — likely a duplicate entry or a per-client mirror of the global setting.)

### public DateTime ResetTime

Gets the next time at which the token bucket will receive a refill, expressed in UTC.

- **Remarks**: When `AvailableTokens` equals `MaxTokens`, `ResetTime` may be in the past. When tokens are exhausted, `ResetTime` indicates when the next token will be added.

## Usage

### Example 1: Basic integration in a request pipeline

```csharp
// Construct middleware: 100 tokens max, refill every 60 seconds
var rateLimiter = new RateLimitingMiddleware(maxTokens: 100, refillIntervalSeconds: 60);

// In request-handling code:
if (rateLimiter.AllowRequest())
{
    // Process the request
    await HandleRequestAsync();
}
else
{
    // Return 429 Too Many Requests with rate-limit headers
    var info = rateLimiter.GetCurrentInfo();
    context.Response.StatusCode = 429;
    context.Response.Headers["X-RateLimit-Remaining"] = info.AvailableTokens.ToString();
    context.Response.Headers["X-RateLimit-Reset"] = info.ResetTime.ToUnixTimeSeconds().ToString();
}
```

### Example 2: Custom client identification and administrative reset

```csharp
// Use API key header as client identifier
var rateLimiter = new RateLimitingMiddleware(maxTokens: 50, refillIntervalSeconds: 30);
rateLimiter.CustomIdentifierExtractor = () =>
{
    return context.Request.Headers["X-API-Key"].FirstOrDefault() ?? "anonymous";
};

// Allow or deny
if (!rateLimiter.AllowRequest())
{
    throw new RateLimitExceededException();
}

// Admin endpoint: reset a specific client
[HttpPost("admin/reset-rate-limit")]
public IActionResult ResetLimit(string apiKey)
{
    rateLimiter.ResetClientLimit(apiKey);
    return Ok();
}

// Cleanup during shutdown
rateLimiter.Dispose();
```

## Notes

- **Thread safety**: All public methods that read or mutate token-bucket state (`AllowRequest`, `TryConsumeToken`, `GetRateLimitInfo`, `GetCurrentInfo`, `ResetClientLimit`) are safe for concurrent use. The underlying bucket operations use atomic or locked access to prevent race conditions.
- **Expired buckets**: `IsExpired` returns `true` for clients that have been inactive beyond a configured threshold. Callers should periodically sweep expired buckets to prevent memory leaks; the middleware may or may not perform automatic cleanup depending on internal implementation.
- **Null returns**: `GetRateLimitInfo` returns `null` when no bucket exists for the client. Prefer `GetCurrentInfo` if a non-null result is required, as it will create a bucket on demand.
- **Disposal**: After `Dispose` is called, any further use of the middleware instance is invalid and will typically result in `ObjectDisposedException`. Ensure disposal occurs exactly once, typically at application shutdown.
- **CustomIdentifierExtractor**: This delegate is invoked frequently. It must be fast and must not throw. If it throws, the exception will propagate through `AllowRequest` and related methods, potentially aborting the request pipeline.
- **ResetTime semantics**: The `ResetTime` property reflects the next scheduled refill instant. It is not a “bucket fully refilled” timestamp unless the bucket is empty. When the bucket is at capacity, `ResetTime` may be in the past, indicating that a refill would have occurred but was skipped due to the bucket being full.
- **Duplicate `MaxTokens`**: The public surface lists `MaxTokens` twice. This likely represents both a global configuration property (set at construction) and a per-client snapshot property exposed via `RateLimitInfo` or the bucket object. Both reflect the same limit but from different scopes.
