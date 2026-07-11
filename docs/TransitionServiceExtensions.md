# TransitionServiceExtensions

The `TransitionServiceExtensions` class provides a set of static extension methods designed to integrate animation and transition capabilities into the SkiaSharp Chart Engine. It facilitates the registration of transition services within the dependency injection container and offers utility methods to instantiate, configure, and execute transition timelines for chart elements, bridging the gap between static data updates and smooth visual interpolations.

## API

### AddChartTransitions
Registers the core transition services required for chart animations into the specified service collection.
*   **Parameters**: `IServiceCollection services` - The service collection to configure.
*   **Return Value**: `IServiceCollection` - The same service collection instance, allowing for method chaining.
*   **Exceptions**: Throws `ArgumentNullException` if `services` is null.

### AddSkiaSharpChartEngineWithTransitions
Registers the main SkiaSharp Chart Engine services along with the necessary transition dependencies in a single call. This method ensures the engine is configured to support animated state changes.
*   **Parameters**: `IServiceCollection services` - The service collection to configure.
*   **Return Value**: `IServiceCollection` - The configured service collection.
*   **Exceptions**: Throws `ArgumentNullException` if `services` is null.

### BeginTransition
Initiates a new transition timeline, typically used to start an animation sequence from the current state of a chart element to a new target state.
*   **Parameters**: Depends on the specific overload, but generally accepts the target object or state definition and optional duration/easing parameters.
*   **Return Value**: `TransitionTimeline` - An instance representing the active animation sequence.
*   **Exceptions**: May throw `InvalidOperationException` if the target object is not in a valid state for animation.

### TransitionTo
Creates a transition definition that moves a specific property or state from its current value to a defined target value.
*   **Parameters**: The source object, the target value, and optionally a `TransitionOptions` configuration.
*   **Return Value**: `TransitionTimeline` - A timeline configured to execute the specific transition.
*   **Exceptions**: Throws `ArgumentException` if the source and target types are incompatible for interpolation.

### ToTransitionOptions
Converts specific animation parameters (such as duration, easing type, or delay) into a standardized `TransitionOptions` object used by the engine.
*   **Parameters**: Animation configuration values (e.g., `TimeSpan duration`, `EasingType easing`).
*   **Return Value**: `TransitionOptions` - A populated options object ready for use in transition methods.
*   **Exceptions**: None typically thrown; invalid values may result in default options being applied.

### ToAnimatedTimeline
Explicitly converts a transition definition or a set of transition options into an executable `TransitionTimeline` instance, preparing it for scheduling or immediate execution.
*   **Parameters**: A transition definition or `TransitionOptions`.
*   **Return Value**: `TransitionTimeline` - The ready-to-run timeline.
*   **Exceptions**: Throws `InvalidOperationException` if the input definition is incomplete or malformed.

## Usage

### Registering Services in Dependency Injection
The following example demonstrates how to configure the application's service container to support the SkiaSharp Chart Engine with full transition capabilities using the dedicated extension method.

```csharp
using Microsoft.Extensions.DependencyInjection;
using SkiaSharp.ChartEngine;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Registers the chart engine and all required transition services
        services.AddSkiaSharpChartEngineWithTransitions();

        // Alternatively, if the engine is already registered, add only transition support:
        // services.AddChartTransitions();
    }
}
```

### Executing a Custom Transition
This example illustrates creating a transition options object and executing a transition to animate a chart metric from its current value to a new target value.

```csharp
using SkiaSharp.ChartEngine.Transitions;
using System;

public class ChartAnimator
{
    public void AnimateMetric(IMetricTarget target, double newValue)
    {
        // Define specific animation behavior
        var options = TransitionServiceExtensions.ToTransitionOptions(
            duration: TimeSpan.FromMilliseconds(500),
            easing: EasingType.CubicInOut
        );

        // Create and begin the transition timeline
        var timeline = TransitionServiceExtensions.TransitionTo(target, newValue, options);
        
        // The timeline is now ready to be processed by the engine's render loop
        timeline.Start();
    }
}
```

## Notes

*   **Thread Safety**: The static methods within `TransitionServiceExtensions` are thread-safe for invocation. However, the resulting `TransitionTimeline` instances are generally not thread-safe and should be accessed and modified only on the thread responsible for the UI rendering loop or the specific synchronization context of the chart engine.
*   **Service Registration Order**: When using `AddSkiaSharpChartEngineWithTransitions`, ensure no conflicting manual registrations of core engine services exist in the `IServiceCollection` prior to this call, as it may lead to duplicate service definitions or resolution errors.
*   **Invalid States**: Methods like `BeginTransition` and `TransitionTo` rely on the internal state of the chart elements. Attempting to initiate a transition on a disposed chart object or an element that has not been fully initialized will result in an `InvalidOperationException`.
*   **Type Compatibility**: The interpolation logic assumes numeric or color-based properties. Passing complex objects that do not have registered interpolators to `TransitionTo` will result in an `ArgumentException` at runtime.
