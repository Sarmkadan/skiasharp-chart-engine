# BubbleChartRenderer

The `BubbleChartRenderer` class is responsible for rendering bubble charts where each data point is represented as a circle (bubble). The radius of each bubble is determined by the data point's value using a square-root scaling algorithm.

## Key Features

- **Square-root radius scaling**: The radius is calculated as `Math.Sqrt(dataPoint.Value / maxSize) * MaxRadius`, ensuring that the area of the bubble is proportional to the data value.
- **Semi-transparent fills**: Bubbles are rendered with 50% opacity (alpha = 128) to allow overlapping bubbles to be visible.
- **Automatic axis rendering**: X and Y axes are drawn with labels for the Y-axis.
- **Title and subtitle support**: Chart titles and subtitles are rendered above the plot area.

## Constants

### MaxRadius
```csharp
private const float MaxRadius = 20f; // maximum radius for the largest bubble
```
This constant defines the maximum radius (in pixels) that any bubble can have. The largest bubble (corresponding to the maximum data value in the dataset) will have this radius. Smaller bubbles are scaled down proportionally using the square-root formula.

## Radius Scaling Logic

The radius for each bubble is computed in the `Render` method:

```csharp
// Radius scaling using sqrt
var radius = Math.Sqrt(dataPoint.Value / maxSize) * MaxRadius;
```

Where:
- `dataPoint.Value` is the size value for the current data point.
- `maxSize` is the maximum size value across all data points in the chart (used for normalization).
- `MaxRadius` is the constant defined above.

This formula ensures that:
1. The bubble with `dataPoint.Value = maxSize` gets a radius of `MaxRadius`.
2. The area of the bubble (π × radius²) is proportional to `dataPoint.Value`, making the visualization perceptually accurate for area-based comparisons.

## Rendering Process

1. **Validation**: Checks for null parameters and empty series.
2. **Layout Calculation**: 
   - Computes space for title and subtitle.
   - Defines the chart bounds within the canvas, applying padding.
3. **Data Normalization**:
   - Determines the minimum and maximum values across all series for Y-axis scaling.
   - Computes `maxSize` for radius scaling (currently uses the same value set as Y-axis values).
4. **Drawing**:
   - Renders title and subtitle.
   - For each data point in each series:
     - Calculates X position based on label hash (simplified distribution).
     - Calculates Y position based on normalized value.
     - Computes bubble radius using square-root scaling.
     - Draws a semi-transparent circle.
   - Renders X and Y axes with Y-axis labels.

## Notes

- The X-axis positioning uses a simplified hash-based distribution for demonstration purposes. In a real-world scenario, this would typically be replaced with proper categorical or numerical axis mapping.
- The Y-axis labels are formatted as integers (`F0` format specifier).
- The renderer logs key events (start, completion, errors) using the injected `ILogger`.