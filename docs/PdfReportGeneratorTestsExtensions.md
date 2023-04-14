# PdfReportGeneratorTestsExtensions

The `PdfReportGeneratorTestsExtensions` class provides a set of static extension methods and helper utilities designed to streamline the creation and validation of PDF report structures within unit tests for the `skiasharp-chart-engine` project. By encapsulating the boilerplate logic required to instantiate `ReportSection` objects and assert their presence within a report, this type enables developers to write concise, readable, and maintainable test cases for report generation logic without needing to manually construct complex object graphs for every scenario.

## API

### `CreateTextSection`
```csharp
public static ReportSection CreateTextSection(string content)
```
Constructs a new `ReportSection` instance configured to render plain text. This method simplifies the setup of text-based content blocks in test scenarios.
*   **Parameters**:
    *   `content` (`string`): The textual content to be included in the section.
*   **Returns**: A new `ReportSection` object initialized with the provided text.
*   **Throws**: `ArgumentNullException` if `content` is null.

### `CreateChartSection`
```csharp
public static ReportSection CreateChartSection(byte[] imageBytes, string title)
```
Constructs a new `ReportSection` instance configured to render a chart image. This is used to simulate sections containing visual data representations.
*   **Parameters**:
    *   `imageBytes` (`byte[]`): The raw binary data of the chart image (e.g., PNG or JPEG).
    *   `title` (`string`): The caption or title associated with the chart.
*   **Returns**: A new `ReportSection` object initialized with the provided image and title.
*   **Throws**: `ArgumentNullException` if `imageBytes` or `title` is null.

### `ShouldContainSections`
```csharp
public static void ShouldContainSections(this List<ReportSection> sections, int expectedCount)
```
An assertion helper that validates whether a list of report sections contains the exact expected number of items. It is typically used within test methods to verify report structure integrity.
*   **Parameters**:
    *   `sections` (`List<ReportSection>`): The collection of sections to evaluate.
    *   `expectedCount` (`int`): The anticipated number of sections in the list.
*   **Returns**: `void`.
*   **Throws**: Throws an assertion exception (specific to the testing framework in use) if the count of `sections` does not match `expectedCount`, or if `sections` is null.

### `CreateMultiSectionReport`
```csharp
public static List<ReportSection> CreateMultiSectionReport(params ReportSection[] sections)
```
Aggregates individual `ReportSection` instances into a single list representing a complete multi-section report. This facilitates the setup of complex reports composed of mixed content types.
*   **Parameters**:
    *   `sections` (`params ReportSection[]`): A variable number of `ReportSection` objects to include in the report.
*   **Returns**: A `List<ReportSection>` containing all provided sections in the order they were passed.
*   **Throws**: `ArgumentNullException` if the `sections` array itself is null (though an empty array is valid and returns an empty list).

## Usage

The following examples demonstrate how to utilize these extensions to construct test data and validate report outputs.

**Example 1: Creating and validating a simple text-only report**
```csharp
using System.Collections.Generic;
using SkiasharpChartEngine.Tests.Extensions;
using SkiasharpChartEngine.Models;

public class ReportGenerationTests
{
    [Fact]
    public void GenerateReport_ShouldCreateTwoTextSections()
    {
        // Arrange
        var header = PdfReportGeneratorTestsExtensions.CreateTextSection("Executive Summary");
        var body = PdfReportGeneratorTestsExtensions.CreateTextSection("Detailed analysis follows below.");

        // Act
        var report = PdfReportGeneratorTestsExtensions.CreateMultiSectionReport(header, body);

        // Assert
        report.ShouldContainSections(2);
        Assert.Equal("Executive Summary", report[0].Content);
    }
}
```

**Example 2: Constructing a mixed-content report with charts and text**
```csharp
using System;
using System.Collections.Generic;
using SkiasharpChartEngine.Tests.Extensions;
using SkiasharpChartEngine.Models;

public class ComplexReportTests
{
    [Fact]
    public void GenerateReport_ShouldHandleMixedContent()
    {
        // Arrange
        var dummyImage = new byte[] { 0x89, 0x50, 0x4E, 0x47 }; // Minimal PNG header
        var chartSection = PdfReportGeneratorTestsExtensions.CreateChartSection(dummyImage, "Q1 Sales Trends");
        var summarySection = PdfReportGeneratorTestsExtensions.CreateTextSection("Sales increased by 15%.");

        // Act
        var report = PdfReportGeneratorTestsExtensions.CreateMultiSectionReport(
            PdfReportGeneratorTestsExtensions.CreateTextSection("Quarterly Report"),
            chartSection,
            summarySection
        );

        // Assert
        report.ShouldContainSections(3);
        Assert.Equal("Q1 Sales Trends", report[1].Title);
    }
}
```

## Notes

*   **Null Safety**: All factory methods (`CreateTextSection`, `CreateChartSection`, `CreateMultiSectionReport`) strictly enforce non-null arguments for their primary data parameters. Passing `null` for content, image data, or titles will result in an immediate `ArgumentNullException`, ensuring that invalid report states are caught during test setup rather than during execution.
*   **Thread Safety**: As this class consists entirely of static methods that operate on provided parameters without maintaining any internal mutable static state, it is inherently thread-safe. Multiple threads can safely call these methods concurrently to generate test data.
*   **Parameter Order**: In `CreateMultiSectionReport`, the order of the `params` array directly dictates the sequence of sections in the returned list. This is critical for tests verifying the specific layout or rendering order of the final PDF.
*   **Assertion Behavior**: The `ShouldContainSections` method is designed to integrate with standard testing frameworks. If the assertion fails, it throws an exception that halts the current test execution; it does not return a boolean value for manual checking.
