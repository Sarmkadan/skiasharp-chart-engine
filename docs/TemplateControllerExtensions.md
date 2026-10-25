# TemplateControllerExtensions

`TemplateControllerExtensions` is a static class in `API.Controllers` (the file uses the namespace `API.Controllers`, not `SkiaSharpChartEngine.API.Controllers`) that adds helper methods to `TemplateController`. It provides an existence check and a bulk delete by template type. Both methods validate their arguments before doing any work.

## API

* `public static async Task<bool> TemplateExistsAsync(this TemplateController controller, int templateId)`: Checks whether a template exists. Rejects `templateId` values less than or equal to 0 with `ArgumentOutOfRangeException`. Calls `controller.GetTemplateById(templateId)` and returns `true` when the response reports success and carries a non-null value. Throws `ArgumentNullException` if `controller` is null.
* `public static async Task DeleteAllTemplatesOfTypeAsync(this TemplateController controller, string templateType)`: Deletes every template of the given type. Throws `ArgumentNullException` if `controller` is null, and `ArgumentException` if `templateType` is null or empty. Calls `controller.GetTemplatesByType(templateType)`, then calls `controller.DeleteTemplateAsync(template.Id)` for each returned template in turn. If the lookup fails or returns no value, nothing is deleted and no exception is thrown.

## Usage

The methods are called on an existing `TemplateController` instance:

```csharp
// Requires a TemplateController instance, e.g. resolved from DI or created in a test.
TemplateController controller = /* ... */;

// Check whether template 42 exists
bool exists = await controller.TemplateExistsAsync(42);

// Delete every template of a given type
await controller.DeleteAllTemplatesOfTypeAsync("bar");
```

## Notes

* **The file does not compile against the current `TemplateController`.** The extension code does not match the types it uses:
  * `GetTemplateById` takes a `string` ID and is synchronous (it returns `ApiResponse<ChartTemplate>`), but the extension passes an `int` and `await`s the result.
  * `GetTemplatesByType` takes a `ChartType` and is synchronous (it returns `ApiResponse<List<ChartTemplate>>`), but the extension passes a `string` and `await`s the result.
  * `DeleteTemplateAsync` takes a `string` template ID and an optional `CancellationToken`. The extension passes `template.Id`, but `ChartTemplate` exposes `TemplateId`, not `Id`.
  * `ApiResponse<T>` exposes `Success` and `Data`. The extension uses `IsSuccess` and `Value`, which do not exist.
* The `API/` folder is excluded from the build by the `<Compile Remove="API/**/*.cs" />` entry in `skiasharp-chart-engine.csproj`. The file is therefore not compiled by `dotnet build`, which is why the mismatches above do not break the build. Re-enabling the folder would require fixing them first.
* The signatures and behavior above describe what the source file says. They have not been compiled or tested, and they do not match the current `TemplateController` API.
* `DeleteAllTemplatesOfTypeAsync` ignores the `ApiResponse<bool>` returned by each `DeleteTemplateAsync` call. A failed deletion is not reported, and the loop continues with the next template. Deletions run one at a time, and the method does not accept a `CancellationToken`.
* `TemplateExistsAsync` returns `false` both when the template is missing and when the lookup fails for any other reason. The two cases cannot be told apart.
