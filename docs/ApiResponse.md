# ApiResponse

The `ApiResponse<T>` class provides a unified envelope for API responses in the `skiasharp-chart-engine`. It standardizes the structure of data returned by endpoints, encapsulating status information, payload, optional error messages, and pagination metadata to facilitate predictable communication between the client and the server.

## API

### Constructors
*   **ApiResponse()**: Initializes a new instance of the `ApiResponse<T>` class.

### Properties
*   **StatusCode (int)**: Gets or sets the HTTP status code representing the outcome of the request.
*   **Success (bool)**: A flag indicating whether the request was processed successfully.
*   **Data (T? / List<T>)**: The payload returned by the request. This can be a single instance of `T` or a `List<T>` for paginated collections.
*   **Message (string?)**: An optional message, typically used to provide descriptive feedback or error details when `Success` is false.
*   **Timestamp (DateTime)**: The date and time when the response object was initialized.
*   **TraceId (string?)**: A unique identifier associated with the request, useful for debugging and tracing through distributed systems.
*   **PageNumber (int)**: For paginated responses, indicates the current page index.
*   **PageSize (int)**: For paginated responses, indicates the number of items per page.

### Factory Methods
These static methods provide convenient ways to instantiate an `ApiResponse<T>` with standard HTTP status codes:

*   **Ok(T data)**: Returns a successful response (200 OK).
*   **BadRequest(string message)**: Returns a failure response (400 Bad Request).
*   **Unauthorized(string message)**: Returns a failure response (401 Unauthorized).
*   **Forbidden(string message)**: Returns a failure response (403 Forbidden).
*   **NotFound(string message)**: Returns a failure response (404 Not Found).
*   **Conflict(string message)**: Returns a failure response (409 Conflict).
*   **InternalError(string message)**: Returns a failure response (500 Internal Server Error).
*   **ServiceUnavailable(string message)**: Returns a failure response (503 Service Unavailable).

## Usage

### Standard Response

```csharp
public ApiResponse<UserDto> GetUser(int id)
{
    var user = _userService.Get(id);
    if (user == null)
        return ApiResponse<UserDto>.NotFound("User not found.");

    return ApiResponse<UserDto>.Ok(user);
}
```

### Paginated Response

```csharp
public ApiResponse<List<ChartData>> GetChartData(int page, int pageSize)
{
    var data = _chartService.GetPage(page, pageSize);
    var response = ApiResponse<List<ChartData>>.Ok(data);
    response.PageNumber = page;
    response.PageSize = pageSize;
    return response;
}
```

## Notes

*   **Thread Safety**: The `ApiResponse<T>` class is a data carrier and is not inherently thread-safe. Instances should be treated as immutable once populated, or accessed within a controlled scope.
*   **Serialization**: Ensure that the `Data` property is serializable by the configured JSON formatter.
*   **Nullability**: The `Data`, `Message`, and `TraceId` properties are nullable. Consumers should handle potential null values appropriately, particularly when `Success` is false.
*   **Initialization**: The `Timestamp` property is automatically initialized to the current UTC time upon instantiation.
