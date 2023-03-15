# LoggingMiddleware

The `LoggingMiddleware` class provides a lightweight, reusable component for capturing and emitting diagnostic information about HTTP requests and responses within the skiasharp-chart-engine pipeline. It aggregates request metadata, timing data, and optional body content into a structured log entry that can be emitted via an external logger or consumed directly for debugging and monitoring purposes.

## API

### LoggingMiddleware
Represents the middleware instance. Create a new instance per request to avoid cross‑request state contamination.

### LogRequest
**Purpose:** Records the initiation of an HTTP request and populates the request‑specific properties (`Method`, `Path`, `QueryString`, `Headers`, `RemoteIpAddress`, `TraceId`).  
**Parameters:** `HttpContext context` – the context of the incoming request.  
**Return:** `void`.  
**Throws:** `ArgumentNullException` if `context` is `null`.

### LogResponse
**Purpose:** Records the completion of an HTTP response, updates timing information, and makes the data available for summary generation.  
**Parameters:**  
- `HttpContext context` – the context of the request/response exchange.  
- `int statusCode` – the HTTP status code returned to the client.  
**Return:** `void`.  
**Throws:** `ArgumentNullException` if `context` is `null`.

### LogRequestBody
**Purpose:** Captures the raw body of the incoming request (when present) and stores it for later inclusion in the log summary.  
**Parameters:** `HttpContext context` – the context whose body should be read.  
**Return:** `string` – the request body as a UTF‑8 string, or `Empty` if no body is present.  
**Throws:**  
- `InvalidOperationException` if the request body cannot be read (e.g., stream not seekable or already consumed).  
- `ArgumentNullException` if `context` is `null`.

### LogError
**Purpose:** Logs an unhandled exception that occurred during request processing, associating it with the current request’s trace identifier.  
**Parameters:** `Exception ex` – the exception to log.  
**Return:** `void`.  
**Throws:** `ArgumentNullException` if `ex` is `null`.

### TraceId
**Purpose:** Gets or sets the correlation identifier used to tie together request‑related log entries.  
**Type:** `string`.  
**Return:** The current trace identifier.  
**Throws:** None.

### Method
**Purpose:** Gets or sets the HTTP method (e.g., `GET`, `POST`) of the request being logged.  
**Type:** `string?`.  
**Return:** The HTTP method or `null` if not yet set.  
**Throws:** None.

### Path
**Purpose:** Gets or sets the request path (e.g., `/api/values`).  
**Type:** `string?`.  
**Return:** The request path or `null` if not yet set.  
**Throws:** None.

### QueryString
**Purpose:** Gets or sets the raw query string component of the request URL (excluding the leading `?`).  
**Type:** `string?`.  
**Return:** The query string or `null` if not yet set.  
**Throws:** None.

### Headers
**Purpose:** Gets or sets the collection of request headers.  
**Type:** `Dictionary<string,string>?`.  
**Return:** A dictionary of header names to values, or `null` if no headers have been captured.  
**Throws:** None.

### RemoteIpAddress
**Purpose:** Gets or sets the IP address of the client that sent the request.  
**Type:** `string?`.  
**Return:** The remote IP address or `null` if not yet set.  
**Throws:** None.

### Stopwatch
**Purpose:** Gets or sets the `System.Diagnostics.Stopwatch` instance used to measure the elapsed time of the request.  
**Type:** `Stopwatch?`.  
**Return:** The stopwatch or `null` if timing has not been started.  
**Throws:** None.

### LoggingContext
**Purpose:** Gets or sets an auxiliary context object that can carry additional, user‑defined properties for logging.  
**Type:** `LoggingContext` (a class defined elsewhere in the project).  
**Return:** The current logging context instance.  
**Throws:** None.

### GetSummary
**Purpose:** Produces a single‑line, human‑readable summary of the logged request/response, incorporating all available properties.  
**Parameters:** None.  
**Return:** `string` – a formatted summary suitable for output to a logger or console.  
**Throws:** `InvalidOperationException` if essential data such as `TraceId` or `Method` is missing when the summary is requested.

## Usage

### Example 1: Integration into an ASP.NET Core pipeline
```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

public class Startup
{
    public void Configure(IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            var middleware = new LoggingMiddleware();

            // Populate request‑level data
            middleware.LogRequest(context);
            middleware.LogRequestBody(context);

            try
            {
                await next();

                // Assume a 200 OK unless the pipeline sets a different status
                int statusCode = context.Response.StatusCode;
                middleware.LogResponse(context, statusCode);
            }
            catch (Exception ex)
            {
                middleware.LogError(ex);
                throw; // re‑throw after logging
            }
            finally
            {
                // Emit the final log entry
                var summary = middleware.GetSummary();
                // In a real app, inject ILogger<T> and use it here
                System.Diagnostics.Debug.WriteLine(summary);
            }
        });

        // ... rest of the pipeline (controllers, static files, etc.)
    }
}
```

### Example 2: Manual creation and usage in a test harness
```csharp
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

public class LoggingMiddlewareDemo
{
    public static async Task Main()
    {
        // Simulate an HttpContext
        var context = new DefaultHttpContext
        {
            TraceIdentifier = "abc-123",
            Request =
            {
                Method = HttpMethods.Post,
                Path = "/api/upload",
                QueryString = new QueryString("?id=42"),
                Scheme = "http",
                Host = new HostString("localhost:5000")
            },
            Connection = { RemoteIpAddress = IPAddress.Parse("10.0.0.5") }
        };

        // Add some headers
        context.Request.Headers["Content-Type"] = "application/json";
        context.Request.Headers["Authorization"] = "Bearer token";

        var middleware = new LoggingMiddleware();

        // Simulate setting properties directly (as the real middleware would do)
        middleware.TraceId = context.TraceIdentifier;
        middleware.Method = context.Request.Method;
        middleware.Path = context.Request.Path.Value;
        middleware.QueryString = context.Request.QueryString.Value;
        middleware.Headers = new Dictionary<string, string>
        {
            ["Content-Type"] = context.Request.Headers["Content-Type"],
            ["Authorization"] = context.Request.Headers["Authorization"]
        };
        middleware.RemoteIpAddress = context.Connection.RemoteIpAddress?.ToString();

        // Start timing
        middleware.Stopwatch = Stopwatch.StartNew();

        // Log request and body (body omitted for brevity)
        middleware.LogRequest(context);
        middleware.LogRequestBody(context);

        try
        {
            // Simulate processing...
            await Task.Delay(120);
        }
        catch (Exception ex)
        {
            middleware.LogError(ex);
        }
        finally
        {
            // Stop timing and log response
            middleware.Stopwatch.Stop();
            middleware.LogResponse(context, (int)HttpStatusCode.OK);

            // Output summary
            Console.WriteLine(middleware.GetSummary());
        }
    }
}
```

## Notes
- The class is **not thread‑safe**. Each instance should be used for a single logical request; concurrent access to its properties from multiple threads may result in inconsistent state.
- Properties such as `Headers`, `Stopwatch`, and `LoggingContext` can be `null` if the middleware has not yet populated them. Consumers (e.g., `GetSummary`) must handle null values gracefully or throw an informative exception as documented.
- `GetSummary` will throw an `InvalidOperationException` if essential fields like `TraceId` or `Method` have not been set, ensuring that incomplete log entries are not silently emitted.
- The `LogRequestBody` method consumes the request body stream; calling it after the body has already been read elsewhere will result in an `InvalidOperationException`. In typical middleware usage, this method should be invoked before any downstream component reads the body.
- Setting `Stopwatch` to a non‑null instance does not automatically start it; the caller is responsible for invoking `Start` (or using `Stopwatch.StartNew`) before the request processing begins and `Stop` after it ends. Failure to do so will result in inaccurate timing information in the summary.
