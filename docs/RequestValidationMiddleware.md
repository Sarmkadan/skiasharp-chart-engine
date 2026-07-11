# RequestValidationMiddleware

Middleware that validates incoming HTTP requests based on configurable rules such as header presence, payload size, JSON schema conformity, and query parameter constraints. It can be inserted into an ASP.NET Core pipeline to enforce request contracts before they reach application handlers.

## API

### RequestValidationMiddleware()
Creates a new instance with all validation features disabled and an empty list of allowed content types.  
- **Parameters:** None  
- **Returns:** The newly created middleware instance.  
- **Exceptions:** None.

### ValidateHeaders
Gets or sets a flag indicating whether the middleware should validate required request headers.  
- **Parameters:** None  
- **Returns:** `true` if header validation is enabled; otherwise `false`.  
- **Exceptions:** None.

### ValidatePayloadSize
Gets or sets a flag indicating whether the middleware should validate the size of the request payload against a predefined limit.  
- **Parameters:** None  
- **Returns:** `true` if payload size validation is enabled; otherwise `false`.  
- **Exceptions:** None.

### ValidateJsonSchema
Gets or sets a flag indicating whether the middleware should validate the JSON body of the request against a pre‑loaded JSON schema.  
- **Parameters:** None  
- **Returns:** `true` if JSON schema validation is enabled; otherwise `false`.  
- **Exceptions:** None.

### ValidateQueryParameters
Gets or sets a dictionary that maps query‑parameter names to validation expressions (e.g., regular expressions or allowed values). The middleware checks each supplied parameter against its associated expression.  
- **Parameters:** None  
- **Returns:** A `Dictionary<string, string>` containing the validation rules.  
- **Exceptions:** Setting the property to `null` throws an `ArgumentNullException`.

### AddAllowedContentType(string contentType)
Adds a MIME type to the internal whitelist of allowed request content types. Requests with a `Content-Type` header not present in this list are rejected when content‑type validation is active.  
- **Parameters:**  
  - `contentType`: The MIME type to add (e.g., `"application/json"`).  
- **Returns:** `void`.  
- **Exceptions:**  
  - `ArgumentNullException` if `contentType` is `null`.  
  - `ArgumentException` if `contentType` is empty or consists only of whitespace.

## Usage

```csharp
using Microsoft.AspNetCore.Builder;
using SkiasharpChartEngine.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Register the middleware
builder.Services.AddSingleton<RequestValidationMiddleware>();

var app = builder.Build();

var validationMiddleware = app.Services.GetRequiredService<RequestValidationMiddleware>();
validationMiddleware.ValidateHeaders = true;
validationMiddleware.ValidatePayloadSize = true;
validationMiddleware.ValidateJsonSchema = true;
validationMiddleware.AddAllowedContentType("application/json");
validationMiddleware.ValidateQueryParameters = new Dictionary<string, string>
{
    { "page", @"^\d+$" },          // page must be a positive integer
    { "sort", "asc|desc" }         // sort must be either "asc" or "desc"
};

app.Use(async (ctx, next) =>
{
    await validationMiddleware.Invoke(ctx, next);
});

app.Run();
```

```csharp
using Microsoft.AspNetCore.Http;
using SkiasharpChartEngine.Middleware;

public class RequestValidationMiddleware
{
    // Assume the class implements the members documented above.
    public async Task Invoke(HttpContext context, Func<Task> next)
    {
        // Example of using the properties inside the middleware logic.
        if (ValidateHeaders && !context.Request.Headers.ContainsKey("X-Client-Version"))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("Missing required header: X-Client-Version");
            return;
        }

        if (ValidatePayloadSize && context.Request.ContentLength > 1_048_576) // 1 MB limit
        {
            context.Response.StatusCode = StatusCodes.Status413PayloadTooLarge;
            await context.Response.WriteAsync("Payload size exceeds the allowed limit.");
            return;
        }

        // Further validation steps (JSON schema, query parameters, content type) omitted for brevity.

        await next();
    }
}
```

## Notes
- Modifying any of the configuration properties (`ValidateHeaders`, `ValidatePayloadSize`, `ValidateJsonSchema`, `ValidateQueryParameters`, or the allowed content type list) after the middleware has begun processing requests can lead to race conditions. It is recommended to set all options before adding the middleware to the pipeline.
- The `ValidateQueryParameters` dictionary is mutable; concurrent modifications from multiple threads may cause undefined behavior. Treat the dictionary as immutable after the middleware is started.
- If `ValidateQueryParameters` contains a key with an empty string or a value that is `null`, the middleware’s behavior is unspecified; avoid such entries.
- The middleware does not automatically enforce a maximum payload size; the `ValidatePayloadSize` flag merely indicates that a size check should be performed. The actual limit must be defined elsewhere in the application logic or via additional middleware.
- All validation steps are short‑circuiting: if any check fails, the middleware writes an appropriate error response and does not invoke the next delegate.
