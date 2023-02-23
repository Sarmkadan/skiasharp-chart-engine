# ChartModelsAndValidationTests

Unit test suite for the core data models and validation logic in the SkiaSharp Chart Engine. This class verifies correct behavior of `DataPoint`, `ChartSeries`, `Chart`, and related utility classes, including argument validation, cloning semantics, bounds calculation, and configuration validation.

## API

### `DataPoint_SettingXToNaN_ThrowsArgumentException`
Ensures that assigning `NaN` to the `X` property of a `DataPoint` raises an `ArgumentException`. This prevents invalid numeric states in chart data.

### `DataPoint_SettingYToPositiveInfinity_ThrowsArgumentException`
Ensures that assigning `PositiveInfinity` to the `Y` property of a `DataPoint` raises an `ArgumentException`. This prevents unbounded or invalid numeric states in chart data.

### `DataPoint_Clone_ProducesIndependentCopyWithSameValues`
Verifies that calling `Clone()` on a `DataPoint` returns a new instance with identical `X` and `Y` values, and that subsequent changes to the original do not affect the clone.

### `ChartSeries_AddDataPoint_IncreasesCount`
Confirms that invoking `AddDataPoint` on a `ChartSeries` increments the `Count` property, reflecting the addition of a new data point.

### `ChartSeries_GetYAxisRange_EmptySeries_ReturnsDefaultRange`
Validates that when `GetYAxisRange()` is called on a `ChartSeries` with no data points, it returns a default range suitable for rendering an empty series.

### `ChartSeries_GetYAxisRange_WithPoints_ReturnsActualBounds`
Ensures that `GetYAxisRange()` on a non-empty `ChartSeries` returns the minimum and maximum `Y` values across all points, suitable for axis scaling.

### `Chart_AddNullSeries_ThrowsArgumentNullException`
Confirms that passing `null` to the `AddSeries` method of a `Chart` throws an `ArgumentNullException`, enforcing non-null series requirements.

### `Chart_GetDataBounds_EmptyChart_ReturnsDefaultBounds`
Validates that when `GetDataBounds()` is invoked on a `Chart` with no series, it returns a default bounding rectangle appropriate for rendering an empty chart.

### `Chart_GetDataBounds_WithSeriesData_ReturnsCorrectBounds`
Ensures that `GetDataBounds()` on a `Chart` containing one or more series returns a rectangle that spans all data points across all series, suitable for view port calculation.

### `ChartValidator_ValidateChart_NullInput_IsNotValid`
Confirms that `ValidateChart(null)` returns a validation result indicating failure, ensuring null charts are rejected.

### `ChartValidator_ValidateChart_NoSeries_AddsSeriesError`
Validates that `ValidateChart` on a chart with no series adds a validation error indicating missing series, enforcing non-empty chart requirements.

### `ChartValidator_ValidateSeries_EmptyName_AddsNameError`
Ensures that `ValidateSeries` on a series with an empty or whitespace `Name` adds a validation error indicating an invalid name, enforcing naming constraints.

### `ChartValidator_ValidateConfiguration_DefaultConfig_IsValid`
Confirms that validating a default or minimal configuration via `ValidateConfiguration` returns a valid result, ensuring basic configurations are accepted.

### `ChartValidator_ValidateDataPoint_NullPoint_IsNotValid`
Validates that `ValidateDataPoint(null)` returns a result indicating failure, ensuring null data points are rejected during validation.

### `ColorHelper_HexToRgb_ForPureRed_ReturnsCorrectRgbString`
Ensures that converting the hex string `"#FF0000"` via `HexToRgb` returns the correct RGB string `"rgb(255, 0, 0)"`.

### `ColorHelper_RgbToHex_ForBlue_ReturnsUpperCaseHex`
Confirms that converting the RGB string `"rgb(0, 0, 255)"` via `RgbToHex` returns the uppercase hex string `"#0000FF"`.

### `ColorHelper_IsValidHexColor_ValidSixCharHex_ReturnsTrue`
Validates that `IsValidHexColor("#AABBCC")` returns `true`, confirming correct parsing of valid six-character hex color strings.

### `ColorHelper_IsValidHexColor_MissingHash_ReturnsFalse`
Ensures that `IsValidHexColor("AABBCC")` returns `false`, confirming that a leading `#` is required for valid hex color strings.

### `ColorHelper_LightenColor_IncreasesChannelValues`
Confirms that `LightenColor` increases the red, green, and blue channel values of the input color, producing a visibly lighter shade.

### `DataPointExtensions_GetDistance_KnownPoints_ReturnsEuclideanDistance`
Validates that `GetDistance` between two known `DataPoint` instances returns the correct Euclidean distance, ensuring geometric calculations are accurate.

## Usage
