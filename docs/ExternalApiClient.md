# ExternalApiClient

A lightweight HTTP client wrapper designed to simplify interactions with external RESTful APIs by providing strongly-typed request methods and flexible header management.

## API

### `public ExternalApiClient`

Initializes a new instance of the `ExternalApiClient` with default settings. The client is configured to use `System.Net.Http.HttpClient` internally with default timeout and no default headers.

### `public async Task<T> GetAsync<T>(string url)`

Sends an HTTP GET request to the specified URL and deserializes the response body into an object of type `T`.

- **url**: The endpoint URL to send the request to.
- **Return value**: A `Task<T>` that resolves to the deserialized response content.
- **Throws**: `HttpRequestException` if the request fails or the response status code indicates an error. `JsonException` if the response body cannot be deserialized.

### `public async Task<T> PostAsync<T>(string url, object? body)`

Sends an HTTP POST request to the specified URL with an optional JSON-serialized body and deserializes the response into an object of type `T`.

- **url**: The endpoint URL to send the request to.
- **body**: The optional request payload to serialize as JSON.
- **Return value**: A `Task<T>` that resolves to the deserialized response content.
- **Throws**: `HttpRequestException` if the request fails or the response status code indicates an error. `JsonException` if the request body or response cannot be serialized or deserialized.

### `public async Task<T> PutAsync<T>(string url, object? body)`

Sends an HTTP PUT request to the specified URL with an optional JSON-serialized body and deserializes the response into an object of type `T`.

- **url**: The endpoint URL to send the request to.
- **body**: The optional request payload to serialize as JSON.
- **Return value**: A `Task<T>` that resolves to the deserialized response content.
- **Throws**: `HttpRequestException` if the request fails or the response status code indicates an error. `JsonException` if the request body or response cannot be serialized or deserialized.

### `public async Task<bool> DeleteAsync(string url)`

Sends an HTTP DELETE request to the specified URL and returns a boolean indicating whether the operation succeeded.

- **url**: The endpoint URL to send the request to.
- **Return value**: A `Task<bool>` that resolves to `true` if the response status code is in the 2xx range; otherwise `false`.
- **Throws**: `HttpRequestException` if the request fails.

### `public void SetAuthorizationHeader(string scheme, string parameter)`

Configures the authorization header for subsequent requests using the specified scheme and parameter.

- **scheme**: The authentication scheme (e.g., "Bearer").
- **parameter**: The token or credential value.
- **Throws**: `ArgumentNullException` if `scheme` or `parameter` is null.

### `public void AddHeader(string name, string value)`

Adds a custom HTTP header to be included in all subsequent requests.

- **name**: The header name.
- **value**: The header value.
- **Throws**: `ArgumentNullException` if `name` or `value` is null.

## Usage
