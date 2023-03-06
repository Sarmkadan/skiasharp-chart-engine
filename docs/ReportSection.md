# ReportSection

`ReportSection` is a configuration class used in the SkiaSharp Chart Engine to define the layout and styling of individual sections within a generated PDF report. It encapsulates properties for text content, chart rendering, page formatting, and visual styling to control the appearance of each report section.

## API

### `Heading`
- **Purpose**: Gets or sets the heading text for the report section.
- **Type**: `string?`
- **Remarks**: If `null`, the heading is omitted. Leading or trailing whitespace is trimmed during rendering.

### `BodyText`
- **Purpose**: Gets or sets the body text content for the report section.
- **Type**: `string?`
- **Remarks**: Supports basic Markdown formatting (e.g., bold, italics). If `null`, the body is omitted.

### `Chart`
- **Purpose**: Gets or sets the chart to be rendered in this section.
- **Type**: `Chart?`
- **Remarks**: If `null`, no chart is rendered. The chart is scaled according to `ImageFit` and `ChartDpi`.

### `ImageFit`
- **Purpose**: Gets or sets how the chart image should fit within the available space.
- **Type**: `PdfImageFit`
- **Remarks**: Defaults to `PdfImageFit.Contain`. Other values include `Fill`, `Cover`, and `ScaleDown`.

### `PageBreakBefore`
- **Purpose**: Gets or sets whether a page break should be inserted before this section.
- **Type**: `bool`
- **Remarks**: Defaults to `false`. Useful for forcing new sections to start on a new page.

### `Title`
- **Purpose**: Gets or sets the main title of the report.
- **Type**: `string`
- **Remarks**: Required for document structure. Empty or whitespace-only values may result in rendering errors.

### `Subtitle`
- **Purpose**: Gets or sets the subtitle text displayed below the title.
- **Type**: `string?`
- **Remarks**: If `null`, the subtitle is omitted. Supports basic Markdown formatting.

### `TitleFontSize`
- **Purpose**: Gets or sets the font size for the report title.
- **Type**: `float`
- **Remarks**: Defaults to a platform-specific value (typically `24.0f`). Must be positive.

### `HeadingFontSize`
- **Purpose**: Gets or sets the font size for section headings.
- **Type**: `float`
- **Remarks**: Defaults to a platform-specific value (typically `18.0f`). Must be positive.

### `BodyFontSize`
- **Purpose**: Gets or sets the font size for body text.
- **Type**: `float`
- **Remarks**: Defaults to a platform-specific value (typically `12.0f`). Must be positive.

### `ChartDpi`
- **Purpose**: Gets or sets the DPI (dots per inch) for rendering the chart.
- **Type**: `int`
- **Remarks**: Defaults to `96`. Higher values produce crisper images but increase file size.

### `ShowPageNumbers`
- **Purpose**: Gets or sets whether page numbers should be displayed in the footer.
- **Type**: `bool`
- **Remarks**: Defaults to `true`. Page numbers are rendered in the format "Page X of Y".

### `PageBackgroundColor`
- **Purpose**: Gets or sets the background color of the page as a hex string (e.g., `"#FFFFFF"`).
- **Type**: `string`
- **Remarks**: Defaults to `"#FFFFFF"` (white). Invalid values may fall back to white.

### `TextColor`
- **Purpose**: Gets or sets the primary text color as a hex string.
- **Type**: `string`
- **Remarks**: Defaults to `"#000000"` (black). Invalid values may fall back to black.

### `AccentColor`
- **Purpose**: Gets or sets the accent color used for highlights and chart elements as a hex string.
- **Type**: `string`
- **Remarks**: Defaults to `"#0078D7"` (a shade of blue). Invalid values may fall back to the default.

## Usage

### Example 1: Basic Report Section with Chart
