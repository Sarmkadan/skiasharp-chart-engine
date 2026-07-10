# ApiResponseExtensions
The `ApiResponseExtensions` class provides a set of extension methods for working with API responses in the skiasharp-chart-engine project. These methods enable developers to easily create and manipulate API responses, including adding trace IDs, handling errors, and converting responses to standard or paginated formats.

## API
* `public static ApiResponse<T> WithTraceId<T>(...)`: Adds a trace ID to an API response. The purpose of this method is to enable tracing and logging of API requests. It takes an `ApiResponse<T>` as input and returns a new `ApiResponse<T>` with the trace ID added. This method does not throw any exceptions.
* `public static ApiResponse<T> WithError<T>(...)`: Creates an API response with an error. The purpose of this method is to handle error cases in API requests. It takes an error as input and returns an `ApiResponse<T>` representing the error. This method does not throw any exceptions.
* `public static ApiResponse<List<T>> ToStandardResponse<T>(...)`: Converts an API response to a standard response format. The purpose of this method is to normalize API responses to a standard format. It takes an `ApiResponse<T>` as input and returns an `ApiResponse<List<T>>` in the standard format. This method does not throw any exceptions.
* `public static PaginatedResponse<T> ToPaginatedResponse<T>(...)`: Converts an API response to a paginated response format. The purpose of this method is to enable pagination of API responses. It takes an `ApiResponse<T>` as input and returns a `PaginatedResponse<T>` in the paginated format. This method does not throw any exceptions.

## Usage
The following examples demonstrate how to use the `ApiResponseExtensions` class:
```csharp
// Example 1: Adding a trace ID to an API response
var response = new ApiResponse<MyData>();
var responseWithTraceId = response.WithTraceId("my-trace-id");

// Example 2: Converting an API response to a paginated response
var paginatedResponse = new ApiResponse<MyData>().ToPaginatedResponse();
```

## Notes
When using the `ApiResponseExtensions` class, note that the `WithError` method will override any existing data in the response with the error data. Additionally, the `ToStandardResponse` and `ToPaginatedResponse` methods will create new response objects, leaving the original response unchanged. The `ApiResponseExtensions` class is designed to be thread-safe, as it only uses immutable data structures and does not maintain any internal state. However, the thread-safety of the methods depends on the thread-safety of the input data and the underlying API response implementation.
