# PdfReportGeneratorTests

Unit test class for verifying the behavior of `PdfReportGenerator` in generating PDF reports with various configurations. The tests cover validation, edge cases, and integration scenarios to ensure correct PDF generation under different conditions.

## API

### `PdfReportGeneratorTests`
Public test class containing unit tests for PDF report generation functionality. All tests validate the behavior of `PdfReportGenerator` using mocked or minimal dependencies.

### `async Task GenerateAsync_WithNullSections_ThrowsArgumentNullException`
Validates that passing `null` as the sections parameter results in an `ArgumentNullException`. Ensures the generator enforces non-null input validation.

**Parameters**
- None (test method with hardcoded `null` input)

**Return Value**
- None (throws exception)

**Exceptions**
- `ArgumentNullException` when `sections` is `null`

---

### `async Task GenerateAsync_EmptySections_ReturnsPdfBytes`
Ensures that an empty collection of sections still produces a valid PDF byte array. Tests handling of minimal input without errors.

**Parameters**
- None (test method with empty `sections` collection)

**Return Value**
- `Task<byte[]>`: PDF content as byte array

**Exceptions**
- None

---

### `async Task GenerateAsync_TextOnlySection_ReturnsPdfBytes`
Validates that a report containing only text sections (no charts) generates a valid PDF. Confirms text rendering path works independently.

**Parameters**
- None (test method with text-only `sections`)

**Return Value**
- `Task<byte[]>`: PDF content as byte array

**Exceptions**
- None

---
### `async Task GenerateAsync_WithChart_CallsRenderingService`
Ensures that when a chart section is included, the rendering service is invoked. Verifies integration between report generation and chart rendering.

**Parameters**
- None (test method with chart-including `sections`)

**Return Value**
- `Task`: Completes when rendering is triggered

**Exceptions**
- None

---
### `async Task GenerateAsync_MultipleSections_ProducesSinglePdf`
Confirms that multiple sections are combined into a single PDF output. Tests concatenation or sequential rendering logic.

**Parameters**
- None (test method with multiple `sections`)

**Return Value**
- `Task<byte[]>`: Single PDF byte array containing all sections

**Exceptions**
- None

---
### `async Task GenerateAsync_RenderFailure_StillProducesPdf`
Validates that even if a rendering operation fails (e.g., chart rendering), the PDF is still produced with available content. Tests resilience to partial failures.

**Parameters**
- None (test method with failing render scenario)

**Return Value**
- `Task<byte[]>`: PDF byte array, possibly with fallback content

**Exceptions**
- None

---
### `async Task GenerateToFileAsync_WithEmptyPath_ThrowsArgumentNullException`
Ensures that passing an empty or null file path results in an `ArgumentNullException`. Validates path validation logic.

**Parameters**
- None (test method with empty `filePath`)

**Return Value**
- None (throws exception)

**Exceptions**
- `ArgumentNullException` when `filePath` is empty or `null`

---
### `async Task GenerateToFileAsync_ValidPath_WritesFile`
Confirms that a valid file path results in a PDF file being written to disk. Tests file system interaction and output correctness.

**Parameters**
- None (test method with valid `filePath`)

**Return Value**
- `Task`: Completes when file write finishes

**Exceptions**
- None

## Usage

### Basic PDF Generation with Text Sections
