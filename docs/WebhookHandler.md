# WebhookHandler

`WebhookHandler` manages the lifecycle and delivery of webhook notifications for chart-related events within the skiasharp-chart-engine. It provides registration, unregistration, and subscription querying capabilities, along with asynchronous event handlers that process and dispatch callbacks when charts are created, updated, deleted, rendered, exported, or when errors occur. Each instance tracks its own identity, health status, delivery metrics, and configuration options.

## API

### Constructors

#### `public WebhookHandler`

Initializes a new instance of the `WebhookHandler` class. The default constructor prepares the handler with default options and an inactive, unconfigured subscription state. No webhook URL or event type is set at this stage; these must be supplied through subsequent configuration or registration calls.

### Methods

#### `public string RegisterWebhook`

Registers a webhook subscription based on the handler's current `WebhookUrl`, `EventType`, and `Options`. Returns a unique subscription identifier string. Throws an `InvalidOperationException` if `WebhookUrl` is null, empty, or not a valid absolute URI, or if `EventType` is not one of the recognized chart event types. Throws `ArgumentException` if a subscription with identical URL and event type already exists and the `Options` do not permit duplicate registrations.

#### `public bool UnregisterWebhook`

Unregisters the webhook subscription identified by the handler's current `Id`. Returns `true` if the subscription was found and successfully removed; returns `false` if no subscription with that identifier exists. Does not throw.

#### `public List<WebhookSubscription> GetSubscriptions`

Retrieves all currently active webhook subscriptions managed by this handler instance. Returns a `List<WebhookSubscription>`, which may be empty if no subscriptions are registered. The returned list is a snapshot copy; modifications to it do not affect the internal subscription store.

#### `public async Task OnChartCreatedAsync`

Invoked when a chart is created. Asynchronously delivers a webhook payload containing chart metadata to all registered subscriptions with the `ChartCreated` event type. Accepts a `ChartEventArgs` parameter carrying the chart's identifier, name, and creation timestamp. Returns a `Task` representing the asynchronous delivery operation. Throws `WebhookDeliveryException` if delivery fails after all configured retry attempts; individual subscription failures are captured in their `LastErrorMessage` and `IsHealthy` fields.

#### `public async Task OnChartUpdatedAsync`

Invoked when a chart is updated. Asynchronously delivers a webhook payload containing the updated chart's metadata and changed properties to all registered subscriptions with the `ChartUpdated` event type. Accepts a `ChartEventArgs` parameter. Returns a `Task`. Throws `WebhookDeliveryException` under the same aggregate failure conditions as `OnChartCreatedAsync`.

#### `public async Task OnChartDeletedAsync`

Invoked when a chart is deleted. Asynchronously delivers a webhook payload containing the deleted chart's identifier and deletion timestamp to all registered subscriptions with the `ChartDeleted` event type. Accepts a `ChartEventArgs` parameter. Returns a `Task`. Throws `WebhookDeliveryException` under the same aggregate failure conditions.

#### `public async Task OnChartRenderedAsync`

Invoked when a chart completes rendering. Asynchronously delivers a webhook payload containing the rendered chart's identifier, dimensions, and render duration to all registered subscriptions with the `ChartRendered` event type. Accepts a `ChartRenderEventArgs` parameter. Returns a `Task`. Throws `WebhookDeliveryException` under the same aggregate failure conditions.

#### `public async Task OnChartExportedAsync`

Invoked when a chart is exported to a file or stream. Asynchronously delivers a webhook payload containing the export format, destination path, and file size to all registered subscriptions with the `ChartExported` event type. Accepts a `ChartExportEventArgs` parameter. Returns a `Task`. Throws `WebhookDeliveryException` under the same aggregate failure conditions.

#### `public async Task OnErrorAsync`

Invoked when a chart processing error occurs. Asynchronously delivers a webhook payload containing the error message, stack trace, and associated chart identifier to all registered subscriptions with the `Error` event type. Accepts a `ChartErrorEventArgs` parameter. Returns a `Task`. Throws `WebhookDeliveryException` under the same aggregate failure conditions.

### Properties

#### `public string Id`

Gets the unique identifier of the webhook subscription currently associated with this handler instance. Set after a successful `RegisterWebhook` call. Returns `null` if no registration has been performed.

#### `public string EventType`

Gets or sets the event type string this handler is configured to listen for. Must be one of the recognized values: `ChartCreated`, `ChartUpdated`, `ChartDeleted`, `ChartRendered`, `ChartExported`, or `Error`. Setting an invalid value does not throw immediately but will cause `RegisterWebhook` to throw.

#### `public string WebhookUrl`

Gets or sets the destination URL to which webhook payloads are delivered. Must be a valid absolute HTTPS URI. Setting an invalid value does not throw immediately but will cause `RegisterWebhook` to throw.

#### `public bool IsActive`

Gets whether the subscription is currently active. Returns `true` if the subscription is registered and has not been explicitly paused or unregistered. Returns `false` if unregistered or if the subscription has been administratively disabled.

#### `public bool IsHealthy`

Gets the current health status of the subscription. Returns `false` if the most recent delivery attempt failed or if the endpoint has returned consecutive error responses exceeding the threshold defined in `Options`. Returns `true` otherwise. Reset to `true` upon the next successful delivery.

#### `public DateTime CreatedAt`

Gets the UTC timestamp when the subscription was first registered. Returns `DateTime.MinValue` if no registration has occurred.

#### `public DateTime? LastDeliveryAt`

Gets the UTC timestamp of the most recent delivery attempt, or `null` if no delivery has been attempted since registration.

#### `public int DeliveryCount`

Gets the total number of delivery attempts made for this subscription, including both successful and failed attempts.

#### `public string? LastErrorMessage`

Gets the error message from the most recent failed delivery attempt, or `null` if the last delivery succeeded or no delivery has been attempted.

#### `public WebhookOptions Options`

Gets or sets the configuration options controlling retry policy, timeout, secret signing, and custom headers for webhook deliveries. Modifications to this property affect subsequent deliveries but do not retroactively alter past delivery behavior.

## Usage

### Example 1: Registering and Handling a Chart Creation Webhook

```csharp
var handler = new WebhookHandler
{
    WebhookUrl = "https://api.example.com/hooks/chart-events",
    EventType = "ChartCreated",
    Options = new WebhookOptions
    {
        RetryCount = 3,
        TimeoutSeconds = 10,
        Secret = "hmac-secret-key"
    }
};

string subscriptionId = handler.RegisterWebhook();
Console.WriteLine($"Registered: {subscriptionId}");

// Later, when a chart is created elsewhere in the engine:
var eventArgs = new ChartEventArgs
{
    ChartId = "chart-001",
    ChartName = "Sales Q4",
    CreatedAt = DateTime.UtcNow
};

try
{
    await handler.OnChartCreatedAsync(eventArgs);
    Console.WriteLine($"Delivery count: {handler.DeliveryCount}");
    Console.WriteLine($"Healthy: {handler.IsHealthy}");
}
catch (WebhookDeliveryException ex)
{
    Console.WriteLine($"Delivery failed: {handler.LastErrorMessage}");
}
```

### Example 2: Querying Subscriptions and Handling Errors

```csharp
var handler = new WebhookHandler
{
    WebhookUrl = "https://monitoring.example.com/errors",
    EventType = "Error",
    Options = new WebhookOptions { RetryCount = 2 }
};

handler.RegisterWebhook();

// Retrieve all subscriptions for inspection
List<WebhookSubscription> allSubscriptions = handler.GetSubscriptions();
foreach (var sub in allSubscriptions)
{
    Console.WriteLine($"{sub.Id}: {sub.EventType} -> {sub.WebhookUrl}");
}

// When an error occurs in the chart pipeline:
var errorArgs = new ChartErrorEventArgs
{
    ChartId = "chart-002",
    ErrorMessage = "Render timeout exceeded",
    StackTrace = "...",
    OccurredAt = DateTime.UtcNow
};

await handler.OnErrorAsync(errorArgs);

if (!handler.IsHealthy)
{
    Console.WriteLine($"Webhook unhealthy. Last error: {handler.LastErrorMessage}");
    bool removed = handler.UnregisterWebhook();
    Console.WriteLine($"Unregistered: {removed}");
}
```

## Notes

- **Thread safety**: `RegisterWebhook`, `UnregisterWebhook`, and `GetSubscriptions` are thread-safe and may be called concurrently. The asynchronous event handlers (`OnChartCreatedAsync`, etc.) are designed for concurrent invocation but rely on the underlying HTTP client, which should be configured for concurrent access. Property getters and setters are not synchronized; external synchronization is required if multiple threads mutate `WebhookUrl`, `EventType`, or `Options` while registration or delivery is in progress.
- **Idempotency of registration**: Calling `RegisterWebhook` multiple times without changing `WebhookUrl` or `EventType` will throw if `Options.AllowDuplicateSubscriptions` is `false` (the default). Set this option to `true` to permit multiple identical subscriptions.
- **Unregistration state**: After `UnregisterWebhook` returns `true`, the `Id` property remains set to the now-removed subscription identifier but `IsActive` becomes `false`. Subsequent calls to event handlers will not deliver to this subscription.
- **Delivery failure handling**: Individual delivery failures mark `IsHealthy` as `false` and populate `LastErrorMessage`. The handler continues attempting delivery to other subscriptions. A `WebhookDeliveryException` is thrown only when all configured retries are exhausted for all subscriptions, allowing callers to implement circuit-breaking or fallback logic.
- **Timestamp granularity**: `CreatedAt` and `LastDeliveryAt` use UTC `DateTime` with millisecond precision. `CreatedAt` is set once at registration and never changes; `LastDeliveryAt` updates on every delivery attempt, successful or not.
- **Empty subscription lists**: `GetSubscriptions` returns an empty list, not null, when no subscriptions are registered. Callers can safely iterate without null checks.
