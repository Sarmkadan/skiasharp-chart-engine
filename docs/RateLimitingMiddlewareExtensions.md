# RateLimitingMiddlewareExtensions

The `RateLimitingMiddlewareExtensions` class provides extension methods for configuring and interacting with a `RateLimitingMiddleware` instance, as well as an implementation of the `ILogger` interface for logging within the middleware pipeline. Its static members allow you to attach a logger, retrieve current rate-limit information, check whether a request is allowed, and reset a client’s state. The instance members (`BeginScope`, `IsEnabled`, `Log`) fulfill the `ILogger` contract, enabling integration with standard .NET logging infrastructure.

## API

### `public static RateLimitingMiddleware WithLogger(this RateLimitingMiddleware middleware, ILogger logger)`

Attaches a custom `ILogger` to the middleware.

- **Parameters**  
  `middleware` – The `RateLimitingMiddleware` instance to configure.  
  `logger` – The `ILogger` implementation to use for logging.
- **Returns**  
  The same `RateLimitingMiddleware` instance with the logger applied.
- **Throws**  
  `ArgumentNullException` if `middleware` or `logger` is `null`.

### `public static RateLimitingMiddleware WithConsoleLogger(this RateLimitingMiddleware middleware)`

Attaches a console-based logger (typically `ConsoleLogger`) to the middleware.

- **Parameters**  
  `middleware` – The `RateLimitingMiddleware` instance to configure.
- **Returns**  
  The same `RateLimitingMiddleware` instance with a console logger applied.
- **Throws**  
  `ArgumentNullException` if `middleware` is `null`.

### `public static RateLimitingMiddleware WithNoLogging(this RateLimitingMiddleware middleware)`

Disables logging for the middleware by attaching a null or no-op logger.

- **Parameters**  
  `middleware` – The `RateLimitingMiddleware` instance to configure.
- **Returns**  
  The same `RateLimitingMiddleware` instance with logging disabled.
- **Throws**  
  `ArgumentNullException` if `middleware` is `null`.

### `public static RateLimitInfo? GetCurrentRateLimitInfo(string clientId)`

Retrieves the current rate-limit information for a given client.

- **Parameters**  
  `clientId` – A string identifying the client (e.g., IP address, API key).
- **Returns**  
  A `RateLimitInfo?` containing the current limit state, or `null` if no data exists for the client.
- **Throws**  
  `ArgumentNullException` if `clientId` is `null`.  
  `ArgumentException` if `clientId` is empty or whitespace.

### `public static bool CheckRateLimit(string clientId)`

Checks whether a request from the specified client is allowed under the current rate limit.

- **Parameters**  
  `clientId` – A string identifying the client.
- **Returns**  
  `true` if the request is permitted; `false` if the rate limit has been exceeded.
- **Throws**  
  `ArgumentNullException` if `clientId` is `null`.  
  `ArgumentException` if `clientId` is empty or whitespace.

### `public static void ResetClient(string clientId)`

Resets the rate-limit counters for the specified client, effectively clearing any accumulated usage.

- **Parameters**  
  `clientId` – A string identifying the client.
- **Throws**  
  `ArgumentNullException` if `clientId` is `null`.  
  `ArgumentException` if `clientId` is empty or whitespace.

### `public IDisposable? BeginScope<TState>(TState state)`

Begins a logical operation scope for logging. Implementation of `ILogger.BeginScope`.

- **Parameters**  
  `state` – The state object to associate with the scope.
- **Returns**  
  An `IDisposable` that ends the scope when disposed, or `null` if scopes are not supported.

### `public bool IsEnabled(LogLevel logLevel)`

Checks whether logging at the specified level is enabled. Implementation of `ILogger.IsEnabled`.

- **Parameters**  
  `logLevel` – The `LogLevel` to check.
- **Returns**  
  `true` if logging at that level is enabled; otherwise `false`.

### `public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)`

Writes a log entry. Implementation of `ILogger.Log`.

- **Parameters**  
  `logLevel` – The severity level of the log entry.  
  `eventId` – The event identifier.  
  `state` – The state object associated with the log entry.  
  `exception` – An optional exception to log.  
  `formatter` – A function that creates a formatted message string from the state and exception.
- **Throws**  
  `ArgumentNullException` if `formatter` is `null`.

## Usage

### Example 1: Configuring middleware with a custom logger

```csharp
using Microsoft.Extensions.Logging;
using SkiaSharp.ChartEngine.RateLimiting;

// Create a middleware instance (e.g., from dependency injection)
var middleware = new RateLimitingMiddleware();

// Attach a custom ILogger
ILogger myLogger = new MyCustomLogger();
middleware = middleware.WithLogger(myLogger);

// Alternatively, use the console logger
middleware = middleware.WithConsoleLogger();

// Or disable logging entirely
middleware = middleware.WithNoLogging();
```

### Example 2: Checking rate limits and resetting a client

```csharp
using SkiaSharp.ChartEngine.RateLimiting;

string clientId = "192.168.1.1";

// Check if the client is allowed to proceed
if (RateLimitingMiddlewareExtensions.CheckRateLimit(clientId))
{
    // Process the request
    Console.WriteLine("Request allowed.");
}
else
{
    Console.WriteLine("Rate limit exceeded. Returning 429.");
}

// Retrieve current rate-limit info for diagnostics
RateLimitInfo? info = RateLimitingMiddlewareExtensions.GetCurrentRateLimitInfo(clientId);
if (info.HasValue)
{
    Console.WriteLine($"Remaining requests: {info.Value.Remaining}");
}

// Reset the client's counters (e.g., after a manual override)
RateLimitingMiddlewareExtensions.ResetClient(clientId);
```

## Notes

- All static methods that accept a `clientId` throw `ArgumentNullException` for `null` input and `ArgumentException` for empty or whitespace strings. Always validate or sanitize client identifiers before calling these methods.
- The `WithLogger`, `WithConsoleLogger`, and `WithNoLogging` methods are extension methods on `RateLimitingMiddleware`. They modify the middleware instance in place and return the same instance for chaining.
- The instance members (`BeginScope`, `IsEnabled`, `Log`) are part of the `ILogger` implementation. Their behavior depends on the underlying logger attached via the static configuration methods. If no logger has been attached (or `WithNoLogging` was used), `IsEnabled` returns `false` and `Log` is a no-op.
- Thread safety: The static methods `GetCurrentRateLimitInfo`, `CheckRateLimit`, and `ResetClient` are thread-safe and can be called concurrently from multiple threads. The instance logging methods are also thread-safe as required by the `ILogger` contract.
- `BeginScope` may return `null` if the underlying logger does not support scopes. Callers should handle a `null` return gracefully.
- The `RateLimitInfo` structure returned by `GetCurrentRateLimitInfo` is a snapshot of the client’s state at the time of the call. It may become stale immediately after retrieval due to concurrent updates.
