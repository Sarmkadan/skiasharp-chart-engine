# HttpChartClient

The `HttpChartClient` provides a lightweight wrapper for communicating with a remote chart‑generation service over HTTP. It enables asynchronous retrieval of chart metadata, submission of new chart definitions, and downloading of rendered chart images, while allowing callers to configure default request headers and authentication tokens.

## API

### `HttpChartClient()`
Creates a new instance with no pre‑configured headers or authentication. The underlying `HttpClient` is disposed when `Dispose()` is called.

### `Task<Chart?> GetChartAsync(ChartRequest request, CancellationToken cancellationToken = default)`
Sends a GET request to obtain chart metadata.

- **Parameters**
  - `request`: Contains the identifier or query parameters needed to locate the chart on the server.
  - `cancellationToken`: Optional token to cancel the operation.
- **Return value**: A `Task` that completes with the deserialized `Chart` object if the request succeeds, or `null` if the service returns a 404 or no chart matches the request.
- **Exceptions**
  - `ArgumentNullException` if `request` is `null`.
  - `HttpRequestException` for network failures or non‑success status codes other than 404.
  - `OperationCanceledException` if the request is cancelled via `cancellationToken`.

### `Task<bool> PostChartAsync(ChartDefinition definition, CancellationToken cancellationToken = default)`
Submits a new chart definition to the service.

- **Parameters**
  - `definition`: The chart definition to be stored or processed by the service.
  - `cancellationToken`: Optional token to cancel the operation.
- **Return value**: A `Task` that completes with `true` when the service acknowledges successful receipt and processing; `false` if the service indicates the chart was not accepted (e.g., validation failure).
- **Exceptions**
  - `ArgumentNullException` if `definition` is `null`.
  - `HttpRequestException` for network errors or unexpected HTTP status codes.
  - `OperationCanceledException` if the request is cancelled.

### `Task<byte[]?> GetRenderedChartAsync(ChartRenderOptions options, CancellationToken cancellationToken = default)`
Retrieves the binary image data for a rendered chart.

- **Parameters**
  - `options`: Specifies the chart identifier, format (e.g., PNG, JPEG), dimensions, and any rendering overrides.
  - `cancellationToken`: Optional token to cancel the operation.
- **Return value**: A `Task` that completes with the chart’s image bytes if the service returns a 200 response, or `null` if the chart cannot be rendered (e.g., 404 or rendering error).
- **Exceptions**
  - `ArgumentNullException` if `options` is `null`.
  - `HttpRequestException` for transport problems or non‑success status codes other than 404.
  - `OperationCanceledException` if the request is cancelled.

### `void SetDefaultHeaders(IDictionary<string, string> headers)`
Applies a set of headers that will be sent with every subsequent request made by this client instance.

- **Parameters**
  - `headers`: Key/value pairs representing HTTP headers. Existing headers with the same keys are replaced.
- **Exceptions**
  - `ArgumentNullException` if `headers` is `null`.
  - `ArgumentException` if any header name or value is invalid according to `HttpClient` restrictions.

### `void SetAuthenticationToken(string token)`
Configures a Bearer token that will be added to the `Authorization` header of each request.

- **Parameters**
  - `token`: The token string. Passing `null` or an empty string removes the token from future requests.
- **Exceptions**
  - None; the method simply stores the token for later use.

### `void Dispose()`
Releases the underlying `HttpClient` and associated resources. After disposal, any further method call will throw an `ObjectDisposedException`.

## Usage

```csharp
using var client = new HttpChartClient();

// Configure default headers and authentication once for the lifetime of the client.
client.SetDefaultHeaders(new Dictionary<string, string>
{
    ["Accept"] = "application/json",
    ["User-Agent"] = "skiasharp-chart-engine/1.0"
});
client.SetAuthenticationToken("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...");

// Retrieve chart metadata.
var request = new ChartRequest { ChartId = "sales-2024-Q3" };
Chart? chart = await client.GetChartAsync(request);
if (chart != null)
{
    Console.WriteLine($"Chart title: {chart.Title}");
}

// Get the rendered image (PNG, 800x600).
var renderOptions = new ChartRenderOptions
{
    ChartId = "sales-2024-Q3",
    Format = ImageFormat.Png,
    Width = 800,
    Height = 600
};
byte[]? imageBytes = await client.GetRenderedChartAsync(renderOptions);
if (imageBytes != null)
{
    await File.WriteAllBytesAsync("sales-chart.png", imageBytes);
}
```

```csharp
// Example: posting a new chart definition and verifying acceptance.
using var client = new HttpChartClient();
client.SetAuthenticationToken("secret-token");

var newChart = new ChartDefinition
{
    Name = "Monthly Expenses",
    Type = ChartType.Bar,
    Data = new[] { 1200, 950, 1100, 1300 }
};

bool accepted = await client.PostChartAsync(newChart);
if (accepted)
{
    Console.WriteLine("Chart successfully posted.");
}
else
{
    Console.WriteLine("Chart rejected by the service.");
}
```

## Notes

- The client is **not thread‑safe** for concurrent modifications of default headers or the authentication token. If multiple threads need to invoke methods simultaneously, either synchronize access to `SetDefaultHeaders`/`SetAuthenticationToken` or create separate `HttpChartClient` instances per thread.
- After calling `Dispose()`, any subsequent asynchronous operation will complete faulted with an `ObjectDisposedException`. It is safe to call `Dispose()` multiple times; extra calls are no‑ops.
- Methods that may return `null` (`GetChartAsync`, `GetRenderedChartAsync`) do so only when the service responds with a 404 Not Found or an equivalent “no content” scenario. Other error conditions result in exceptions.
- `SetDefaultHeaders` replaces existing headers with the same name; to preserve a header, include it again in the supplied dictionary.
- The authentication token is applied as a Bearer token (`Authorization: Bearer <token>`). If the service expects a different scheme, adjust the token format before calling `SetAuthenticationToken` or manually set the appropriate header via `SetDefaultHeaders`.
