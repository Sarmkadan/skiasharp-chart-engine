# CreateChartRequestExtensions

`CreateChartRequestExtensions` is a static class in the `SkiasharpChartEngine.API.Requests` namespace that adds three helper methods to `CreateChartRequest`. They cover validation and null-safe access to the request's `Series` and `Configuration` properties. The class holds no state and does not change the request it is called on.

> **Note:** The file `API/Requests/CreateChartRequestExtensions.cs` is excluded from the build by `<Compile Remove="API/**/*.cs" />` in `skiasharp-chart-engine.csproj`. It is not compiled today, and as written it would not compile if it were included. See [Known issues](#known-issues).

## API

* `public static void Validate(this CreateChartRequest request)`: Throws `ArgumentNullException` if `request` is null. Otherwise it checks `request.IsValid`, and if the request is invalid it throws `ArgumentException` with the message `"The chart request is invalid."` and the parameter name `request`. The `errorMessage` produced by `CreateChartRequest.IsValid` is discarded.
* `public static IReadOnlyList<ChartSeries> GetSeriesOrDefault(this CreateChartRequest request)`: Throws `ArgumentNullException` if `request` is null. Returns `request.Series` when it is set, typed as `IReadOnlyList<ChartSeries>`. This is the same list instance, not a copy. When `Series` is null, it returns an empty list.
* `public static ChartConfiguration GetConfigurationOrDefault(this CreateChartRequest request)`: Throws `ArgumentNullException` if `request` is null. Returns `request.Configuration` when it is set. When it is null, it returns a new `ChartConfiguration` with default values. The new instance is not assigned back to `request.Configuration`, so each call with a null configuration returns a different object.

## Usage

The helpers are called on a `CreateChartRequest` instance:

```csharp
CreateChartRequest request = new CreateChartRequest
{
    Title = "Monthly Revenue",
    ChartType = ChartType.Line,
    Series = null,
    Configuration = null
};

// Throws ArgumentException when Title is empty or longer than 200 characters
request.Validate();

// Safe to enumerate: an empty list when Series is null
IReadOnlyList<ChartSeries> series = request.GetSeriesOrDefault();

// Never null: a default ChartConfiguration when Configuration is null
ChartConfiguration configuration = request.GetConfigurationOrDefault();
```

## Notes

* `Validate` is the only method that can throw for invalid data. `GetSeriesOrDefault` and `GetConfigurationOrDefault` only throw on a null `request`.
* `Validate` relies on the request's own validation rules. Those rules are in `CreateChartRequest.IsValid` in `API/Requests/ChartRequests.cs`: the title must not be empty or whitespace and must not exceed 200 characters.
* The helpers do not validate the contents of `Series` or `Configuration`. Their nested validation, such as `ChartConfiguration.Validate()`, has to be called separately.

## Known issues

These issues come from reading the source against the current `CreateChartRequest` definition. The file has not been compiled with them in place.

* **Namespace mismatch.** The file declares `namespace SkiasharpChartEngine.API.Requests` with a lowercase `a` in "Skiasharp". The project's root namespace and the namespace of `CreateChartRequest` are `SkiaSharpChartEngine.API.Requests`. Because of this, `CreateChartRequest` is not found when the file is compiled.
* **Missing `using` directive.** `ChartSeries` and `ChartConfiguration` live in `SkiaSharpChartEngine.Models`. The file has no `using SkiaSharpChartEngine.Models;`, so those names do not resolve.
* **`IsValid` is a method, not a property.** `CreateChartRequest.IsValid` is declared as `bool IsValid(out string? errorMessage)`. `Validate` reads it as `!request.IsValid`, which does not compile. The call needs an `out` argument, for example `if (!request.IsValid(out string? errorMessage))`, and the message can then be used in the exception.

Fixing these would change the code, so this document does not do it. The fixes belong in a separate change that also decides whether the file should be added to the build.
