# ErrorHandlingMiddleware

Middleware component that intercepts exceptions thrown during HTTP request processing and converts them into structured error responses. It is designed to provide consistent error handling across the `skiasharp-chart-engine` API by serializing exception details into a standardized JSON format while preserving diagnostic information for debugging.

## API

### `public ErrorHandlingMiddleware`

Constructor that initializes the middleware with default or injected dependencies. No parameters are required as this middleware operates independently of external services.

### `public async Task InvokeAsync(HttpContext context, RequestDelegate next)`

Invokes the middleware pipeline. Intercepts exceptions thrown by subsequent middleware or controllers, captures diagnostic context, and returns a structured error response.

- **Parameters**
  - `context` – The `HttpContext` for the current HTTP request.
  - `next` – The delegate representing the next middleware in the pipeline.

- **Return Value**
  - Returns a `Task` that completes when the error handling and response writing is finished.

- **Exceptions**
  - Throws `ArgumentNullException` if `context` is `null`.
  - Throws `ArgumentNullException` if `next` is `null`.

### `public int StatusCode`

Gets or sets the HTTP status code to return in the error response. Defaults to `500` (Internal Server Error).

- **Type**: `int`
- **Usage**: Set to standard HTTP status codes such as `400`, `404`, or `500` based on the type of error encountered.

### `public string? Message`

Gets or sets a human-readable error message. This is typically derived from the exception message or a custom description.

- **Type**: `string?`
- **Usage**: Should be set to a concise, user-facing error message. May be `null` if no message is available.

### `public string? Details`

Gets or sets additional technical details about the error. This may include stack traces, internal state, or debugging information.

- **Type**: `string?`
- **Usage**: Intended for diagnostic purposes; may be omitted in production environments.

### `public string? ExceptionType`

Gets or sets the full name of the exception type that was thrown (e.g., `System.ArgumentNullException`).

- **Type**: `string?`
- **Usage**: Useful for logging and client-side error classification.

### `public DateTime Timestamp`

Gets or sets the UTC timestamp when the error was captured.

- **Type**: `DateTime`
- **Usage**: Always in UTC; used for logging and correlation.

### `public string? TraceId`

Gets or sets a unique identifier for tracing the request across services.

- **Type**: `string?`
- **Usage**: Typically sourced from `HttpContext.TraceIdentifier` or a distributed tracing system.

### `public ErrorResponse ErrorResponse`

Gets the structured error response object containing all captured error details.

- **Type**: `ErrorResponse`
- **Return Value**: An instance of `ErrorResponse` populated with the current error state.
- **Usage**: Used internally to serialize the error into JSON for the HTTP response.

### `public MiddlewareException`

Exception type thrown by this middleware when internal processing errors occur (e.g., failure to serialize error response).

- **Type**: `MiddlewareException`
- **Usage**: Indicates a failure in error handling logic itself, not in application code.

## Usage

### Example 1: Basic Usage in ASP.NET Core Pipeline
