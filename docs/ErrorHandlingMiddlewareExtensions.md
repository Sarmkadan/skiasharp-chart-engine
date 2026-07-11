# ErrorHandlingMiddlewareExtensions

Provides factory methods for configuring `ErrorHandlingMiddleware` with different logging strategies, utilities for processing exceptions into structured error responses, and a `CapturingLogger`/`CapturingLoggerProvider` pair that stores log entries in memory for later inspection. This type is the primary configuration surface for the error-handling pipeline in the `skiasharp-chart-engine` project.

## API

### Static Factory Methods

#### `WithLogger`
```csharp
public static ErrorHandlingMiddleware WithLogger(/* ILoggerFactory or ILogger */)
```
Creates an `ErrorHandlingMiddleware` instance that logs through a supplied, production-grade `ILogger` or `ILoggerFactory`. The returned middleware writes structured log events for every captured exception and request context.  
**Returns:** A fully configured `ErrorHandlingMiddleware`.  
**Throws:** `ArgumentNullException` if the logger argument is null.

#### `WithConsoleLogger`
```csharp
public static ErrorHandlingMiddleware WithConsoleLogger()
```
Creates an `ErrorHandlingMiddleware` that writes log output directly to the console. Intended for development and debugging scenarios.  
**Returns:** A middleware instance with console logging enabled.  
**Throws:** Nothing.

#### `WithNoLogging`
```csharp
public static ErrorHandlingMiddleware WithNoLogging()
```
Creates an `ErrorHandlingMiddleware` that performs no logging whatsoever. Exception handling and error-response generation still occur, but no log entries are emitted.  
**Returns:** A middleware instance with logging fully suppressed.  
**Throws:** Nothing.

#### `WithCapturingLogger`
```csharp
public static ErrorHandlingMiddleware WithCapturingLogger()
```
Creates an `ErrorHandlingMiddleware` backed by a `CapturingLoggerProvider`. All log entries are retained in memory and can be enumerated after the pipeline executes. Useful for unit testing and diagnostic tooling.  
**Returns:** A middleware instance whose logger captures entries in memory.  
**Throws:** Nothing.

### Exception Processing

#### `ProcessExceptionAsync`
```csharp
public static async Task<MiddlewareException> ProcessExceptionAsync(HttpContext context, Exception exception)
```
Wraps a raw exception into a `MiddlewareException` enriched with request metadata (path, method, trace identifier, timestamp). This is the canonical entry point for normalizing exceptions before they reach the response generation step.  
**Parameters:**  
- `context` — the current `HttpContext`; must not be null.  
- `exception` — the unhandled exception; must not be null.  
**Returns:** A task that resolves to a `MiddlewareException` carrying the original exception and contextual information.  
**Throws:** `ArgumentNullException` if either argument is null.

#### `CreateErrorResponseAsync`
```csharp
public static async Task<ErrorResponse> CreateErrorResponseAsync(MiddlewareException middlewareException)
```
Builds a client-facing `ErrorResponse` from a `MiddlewareException`. The response includes a sanitized message, status code, and a correlation identifier.  
**Parameters:**  
- `middlewareException` — the normalized exception produced by `ProcessExceptionAsync`.  
**Returns:** A task that resolves to an `ErrorResponse` ready for serialization.  
**Throws:** `ArgumentNullException` if `middlewareException` is null.

#### `MapException`
```csharp
public static (int StatusCode, string Message) MapException(Exception exception)
```
Maps common exception types to HTTP status codes and user-safe messages. For example, `ArgumentException` maps to 400, `UnauthorizedAccessException` to 403, and unrecognized types to 500 with a generic message.  
**Parameters:**  
- `exception` — the exception to classify.  
**Returns:** A tuple containing the recommended HTTP status code and a non-revealing message string.  
**Throws:** Nothing; returns the generic 500 mapping for null input.

### CapturingLoggerProvider

```csharp
public CapturingLoggerProvider
```
An `ILoggerProvider` implementation that creates `CapturingLogger` instances. All log entries written through loggers it creates are appended to an internal, thread-safe collection.

#### `CreateLogger`
```csharp
public ILogger CreateLogger(string categoryName)
```
Creates a new `CapturingLogger` for the given category name.  
**Returns:** An `ILogger` that stores log entries in the provider’s shared collection.  
**Throws:** Nothing.

#### `Dispose`
```csharp
public void Dispose()
```
Releases the provider and clears the internal log entry collection. After disposal, any further logging through loggers created by this provider is a no-op.  
**Throws:** Nothing.

### CapturingLogger

```csharp
public CapturingLogger
```
An `ILogger` implementation that records log entries into the owning `CapturingLoggerProvider`. Each entry captures the log level, event ID, formatted message, and optional scope state.

#### `BeginScope<TState>`
```csharp
public IDisposable? BeginScope<TState>(TState state)
```
Begins a logging scope. The returned `IDisposable` pops the scope on disposal. Scopes are recorded as part of the log entry state.  
**Returns:** A scope handle, or null if the logger is disabled.  
**Throws:** Nothing.

#### `IsEnabled`
```csharp
public bool IsEnabled(LogLevel logLevel)
```
Returns `true` for all log levels; this logger captures everything.  
**Throws:** Nothing.

#### `Log<TState>`
```csharp
public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
```
Records a log entry into the provider’s collection. The entry includes the formatted message, log level, event ID, exception, and any active scope state.  
**Throws:** Nothing; silently drops the entry if the provider has been disposed.

## Usage

### Example 1: Production Pipeline with Structured Logging
```csharp
// In Startup.cs or Program.cs
var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddJsonConsole();
    builder.AddApplicationInsights();
});

var middleware = ErrorHandlingMiddlewareExtensions.WithLogger(loggerFactory);

app.Use(middleware.InvokeAsync);
```
The middleware logs every exception as structured JSON, including correlation IDs and request paths, and returns sanitized `ErrorResponse` payloads to callers.

### Example 2: Unit Testing with Captured Logs
```csharp
var middleware = ErrorHandlingMiddlewareExtensions.WithCapturingLogger();

var context = new DefaultHttpContext();
context.Request.Method = "POST";
context.Request.Path = "/api/render";

var exception = new InvalidOperationException("Chart data malformed");
var mwEx = await ErrorHandlingMiddlewareExtensions.ProcessExceptionAsync(context, exception);
var response = await ErrorHandlingMiddlewareExtensions.CreateErrorResponseAsync(mwEx);

// Assert response shape
Assert.Equal(500, response.StatusCode);
Assert.Contains("correlationId", response.CorrelationId);

// Inspect captured logs
var provider = (CapturingLoggerProvider)middleware.Logger;
var entries = provider.GetLogEntries(); // hypothetical accessor
Assert.Contains(entries, e => e.LogLevel == LogLevel.Error && e.Message.Contains("Chart data"));
```

## Notes

- **Thread safety:** `CapturingLoggerProvider` and `CapturingLogger` use internal synchronization when appending log entries. Multiple threads may log concurrently without corruption. `MapException`, `ProcessExceptionAsync`, and `CreateErrorResponseAsync` are stateless static methods and safe for concurrent use.
- **Disposal behavior:** Disposing a `CapturingLoggerProvider` clears all captured entries and causes subsequent `Log` calls on any logger it created to become no-ops. Loggers are not individually disposable; disposal is centralized through the provider.
- **Null handling:** `MapException` accepts null input and returns a generic 500 mapping rather than throwing. All other public static methods throw `ArgumentNullException` for required reference-type arguments.
- **Scope lifetime:** Scopes created by `CapturingLogger.BeginScope` are recorded in log entries emitted while the scope is active. Failing to dispose a scope handle keeps the scope state attached to all subsequent entries from that logger until the logger itself is garbage-collected or the provider is disposed.
- **Middleware configuration:** The factory methods (`WithLogger`, `WithConsoleLogger`, `WithNoLogging`, `WithCapturingLogger`) are the only intended ways to obtain an `ErrorHandlingMiddleware` instance. Direct instantiation is not supported by the public API surface.
