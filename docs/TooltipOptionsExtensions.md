# TooltipOptionsExtensions
The `TooltipOptionsExtensions` class provides a set of static methods for customizing and manipulating `TooltipOptions` instances. These methods allow for easy application of common themes, sizes, and styles to tooltips, as well as cloning and conditional display.

## API
* `WithLightTheme`: Applies a light theme to a `TooltipOptions` instance. Returns the modified `TooltipOptions` instance.
* `WithDarkTheme`: Applies a dark theme to a `TooltipOptions` instance. Returns the modified `TooltipOptions` instance.
* `WithHighContrast`: Applies a high contrast theme to a `TooltipOptions` instance. Returns the modified `TooltipOptions` instance.
* `WithLargeSize`: Sets the size of a `TooltipOptions` instance to large. Returns the modified `TooltipOptions` instance.
* `WithSmallSize`: Sets the size of a `TooltipOptions` instance to small. Returns the modified `TooltipOptions` instance.
* `Clone`: Creates a deep copy of a `TooltipOptions` instance. Returns the cloned `TooltipOptions` instance.
* `ShouldShow`: Determines whether a tooltip should be displayed based on the provided `TooltipOptions` instance. Returns a boolean value indicating whether the tooltip should be shown.
* `WithBackgroundColor`: Sets the background color of a `TooltipOptions` instance. Returns the modified `TooltipOptions` instance.

## Usage
The following examples demonstrate how to use the `TooltipOptionsExtensions` class:
```csharp
// Example 1: Applying a dark theme and large size to a tooltip
var tooltipOptions = new TooltipOptions();
tooltipOptions = tooltipOptions.WithDarkTheme();
tooltipOptions = tooltipOptions.WithLargeSize();

// Example 2: Cloning a tooltip options instance and conditionally displaying a tooltip
var originalTooltipOptions = new TooltipOptions();
var clonedTooltipOptions = originalTooltipOptions.Clone();
if (TooltipOptionsExtensions.ShouldShow(clonedTooltipOptions))
{
    // Display the tooltip
}
```

## Notes
When using the `TooltipOptionsExtensions` class, note that the `WithLightTheme`, `WithDarkTheme`, and `WithHighContrast` methods will override any existing theme settings on the `TooltipOptions` instance. The `WithLargeSize` and `WithSmallSize` methods will override any existing size settings. The `Clone` method creates a deep copy of the `TooltipOptions` instance, which can be useful for creating multiple tooltips with similar settings. The `ShouldShow` method can be used to conditionally display tooltips based on the provided `TooltipOptions` instance. The `TooltipOptionsExtensions` class is thread-safe, as all methods are static and do not modify external state. However, the `TooltipOptions` instances being manipulated may not be thread-safe, depending on their implementation.
