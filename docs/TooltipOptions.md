# TooltipOptions

Configuration options for chart tooltips, controlling visual appearance, positioning, and behavior of tooltip elements rendered during user interaction.

## API

### `Enabled`
Gets or sets whether tooltips are enabled. When `false`, no tooltip rendering occurs regardless of other settings.

### `BackgroundColor`
Gets or sets the CSS-compatible background color string (e.g., `"#FF0000"` or `"rgba(255,0,0,0.5)"`) for the tooltip background.

### `BorderColor`
Gets or sets the CSS-compatible border color string for the tooltip frame.

### `TextColor`
Gets or sets the CSS-compatible text color string for tooltip content.

### `ContentTemplate`
Gets or sets an optional template string used to format tooltip content. When `null`, default formatting is applied.

### `BorderWidth`
Gets or sets the border width in pixels for the tooltip frame.

### `ShadowOpacity`
Gets or sets the opacity value (0.0 to 1.0) for the tooltip shadow effect.

### `Clone()`
Creates and returns a deep copy of the current `TooltipOptions` instance.

### `IsHit`
Gets or sets whether the tooltip is currently being interacted with (e.g., hovered or clicked).

### `DataPoint`
Gets or sets the `DataPoint` instance currently associated with the tooltip. May be `null` if no point is targeted.

### `Series`
Gets or sets the `ChartSeries` instance containing the data point associated with the tooltip. May be `null` if no series is targeted.

### `SeriesIndex`
Gets or sets the zero-based index of the series containing the data point associated with the tooltip.

### `DistancePx`
Gets or sets the minimum distance in pixels from the mouse pointer at which the tooltip should appear.

### `CanvasX`
Gets or sets the X-coordinate in canvas pixels where the tooltip is rendered.

### `CanvasY`
Gets or sets the Y-coordinate in canvas pixels where the tooltip is rendered.

### `TooltipText`
Gets or sets the raw text content displayed in the tooltip.

### `Region`
Gets or sets the `ChartRegion` in which the tooltip is currently visible.

### `PanX`
Gets or sets the horizontal pan offset in data units applied to the tooltip’s position.

### `PanY`
Gets or sets the vertical pan offset in data units applied to the tooltip’s position.

### `VisibleXRange`
Gets or sets a tuple `(Min, Max)` defining the visible X-axis range in data units within which the tooltip is rendered. Values outside this range may be clipped or hidden.

## Usage
