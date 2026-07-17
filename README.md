# SkiaSharp Chart Engine

A .NET chart rendering library (SkiaSharp-based) with an ASP.NET Core Web API host. Usable standalone via `ChartEngine.Create()` or as a service through `AddSkiaSharpChartEngine()`.

## ThemeManager

`ThemeManager` provides a centralized system for managing chart themes and color schemes. It allows developers to register custom themes, switch the active theme at runtime, and retrieve theme properties (such as background/foreground colors, font settings, and series colors) to ensure consistent styling across generated charts.

```csharp
using System;
using Microsoft.Extensions.Logging.Abstractions;
using SkiaSharp;
using SkiaSharpChartEngine.Utilities;

public class ThemeManagerExample
{
    public static void Main()
    {
        // Initialize ThemeManager with a logger
        var logger = new NullLogger<ThemeManager>();
        var themeManager = new ThemeManager(logger);

        // Example 1: List all available theme names
        Console.WriteLine("Available themes: " + string.Join(", ", themeManager.GetAvailableThemes()));

        // Example 2: Retrieve and inspect the current theme
        var currentTheme = themeManager.GetCurrentTheme();
        Console.WriteLine($"Current theme: {currentTheme.Name}");
        Console.WriteLine($"Background Color: {currentTheme.BackgroundColor}");
        Console.WriteLine($"Series Colors count: {currentTheme.SeriesColors.Length}");

        // Example 3: Register a custom theme
        var customTheme = new ChartTheme
        {
            Name = "Corporate Blue",
            BackgroundColor = SKColors.WhiteSmoke,
            ForegroundColor = SKColors.Black,
            GridColor = SKColors.LightGray,
            AxisColor = SKColors.DarkBlue,
            TextColor = SKColors.DarkBlue,
            SeriesColors = new[] { SKColors.DodgerBlue, SKColors.MidnightBlue },
            FontSize = 14f,
            LineWidth = 3f
        };
        themeManager.RegisterTheme("corporate", customTheme);

        // Example 4: Switch to the new theme
        themeManager.SetCurrentTheme("corporate");
        Console.WriteLine($"Switched to: {themeManager.GetCurrentTheme().Name}");

        // Example 5: Get a specific theme
        var darkTheme = themeManager.GetTheme("dark");
        Console.WriteLine($"Retrieved theme: {darkTheme.Name}, Axis Color: {darkTheme.AxisColor}");
    }
}
```

## DataPointGenerator

`DataPointGenerator` is a utility class for generating test and sample data points for chart development and testing. It provides several methods to create different types of data distributions including linear, sinusoidal, random, Gaussian, exponential, and polynomial patterns. This is particularly useful when creating sample charts, testing rendering performance, or generating realistic-looking datasets without manual data entry.

```csharp
using System;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Utilities;

public class DataPointGeneratorExample
{
    public static void Main()
    {
        // Example 1: Generate linear data (y = x)
        var linearData = DataPointGenerator.GenerateLinearData(count: 100);
        
        // Example 2: Generate sinusoidal data (sine wave)
        var sineData = DataPointGenerator.GenerateSinusoidalData(
            count: 100,
            amplitude: 5,
            frequency: 2
        );
        
        // Example 3: Generate random data within a range
        var randomData = DataPointGenerator.GenerateRandomData(
            count: 50,
            minValue: 0,
            maxValue: 100
        );
        
        // Example 4: Generate Gaussian (normal) distribution data
        var gaussianData = DataPointGenerator.GenerateGaussianData(
            count: 100,
            mean: 50,
            stdDev: 15
        );
        
        // Example 5: Generate exponential growth data
        var exponentialData = DataPointGenerator.GenerateExponentialData(
            count: 50,
            baseValue: 1,
            growthRate: 0.05
        );
        
        // Example 6: Generate polynomial data (y = 2x² + 3x + 1)
        var polynomialData = DataPointGenerator.GeneratePolynomialData(
            count: 100,
            coefficients: new double[] { 1, 3, 2 } // [constant, linear, quadratic]
        );
    }
}
```

## ColorHelper

`ColorHelper` is a utility class for performing common color operations and conversions. It provides methods for working with hex and RGB color formats, manipulating color brightness (lightening/darkening), validating color formats, and accessing default color palettes. This is particularly useful for chart rendering where you need to generate colors dynamically or convert between different color representations.

```csharp
using System;
using SkiaSharpChartEngine.Utilities;

public class ColorHelperExample
{
    public static void Main()
    {
        // Example 1: Get a color from the default palette by index
        string colorAtIndex = ColorHelper.GetColorAtIndex(0);
        Console.WriteLine($"Color at index 0: {colorAtIndex}"); // #1f77b4

        // Example 2: Get the complete default color palette
        var palette = ColorHelper.GetDefaultColorPalette();
        Console.WriteLine($"Default palette has {palette.Count} colors");

        // Example 3: Convert hex color to RGB/RGBA format
        string hexColor = "#3498db";
        string rgb = ColorHelper.HexToRgb(hexColor);
        Console.WriteLine($"Hex to RGB: {rgb}"); // rgb(52, 152, 219)

        string hexWithAlpha = "#3498db80";
        string rgba = ColorHelper.HexToRgb(hexWithAlpha);
        Console.WriteLine($"Hex to RGBA: {rgba}"); // rgba(52, 152, 219, 0.50)

        // Example 4: Convert RGB values to hex format
        string hexFromRgb = ColorHelper.RgbToHex(255, 99, 71);
        Console.WriteLine($"RGB to Hex: {hexFromRgb}"); // #ff6347

        // Example 5: Lighten a color by a factor
        string lightened = ColorHelper.LightenColor("#3498db", 0.3f);
        Console.WriteLine($"Lightened color: {lightened}"); // #6bb5f3

        // Example 6: Darken a color by a factor
        string darkened = ColorHelper.DarkenColor("#3498db", 0.3f);
        Console.WriteLine($"Darkened color: {darkened}"); // #246a9a

        // Example 7: Validate a hex color
        bool isValid = ColorHelper.IsValidHexColor("#ff5733");
        Console.WriteLine($"Is #ff5733 valid? {isValid}"); // True
        
        bool isInvalid = ColorHelper.IsValidHexColor("not-a-color");
        Console.WriteLine($"Is 'not-a-color' valid? {isInvalid}"); // False
    }
}
```

## PathHelper

`PathHelper` provides utility methods for file system operations, path manipulation, and directory management. It includes path validation to prevent directory traversal attacks, safe filename generation, unique filename generation, directory existence checks, path combination utilities, and file cleanup operations.

```csharp
using System;
using SkiaSharpChartEngine.Utilities;

public class PathHelperExample
{
    public static void Main()
    {
        // Validate a path is safe and doesn't escape intended directory
        bool isValid = PathHelper.IsValidPath("charts/export.png", "/home/user/project");
        Console.WriteLine($"Is path valid? {isValid}"); // True

        // Get a safe filename by removing invalid characters
        string safeFilename = PathHelper.GetSafeFilename("chart:data?.png");
        Console.WriteLine($"Safe filename: {safeFilename}"); // "chart_data_.png"

        // Generate a unique filename if file exists
        string uniqueFilename = PathHelper.GetUniqueFilename("/tmp/charts", "chart.png");
        Console.WriteLine($"Unique filename: {uniqueFilename}");

        // Ensure directory exists, creating it if necessary
        bool dirCreated = PathHelper.EnsureDirectoryExists("/tmp/charts/export");
        Console.WriteLine($"Directory created: {dirCreated}");

        // Get file extension for export format
        string extension = PathHelper.GetFileExtension("png");
        Console.WriteLine($"Extension: {extension}"); // ".png"

        // Get MIME type for format
        string mimeType = PathHelper.GetMimeType("svg");
        Console.WriteLine($"MIME type: {mimeType}"); // "image/svg+xml"

        // Combine path segments safely
        string combinedPath = PathHelper.CombinePath("charts", "exports", "chart.png");
        Console.WriteLine($"Combined path: {combinedPath}");

        // Get relative path from base directory
        string relativePath = PathHelper.GetRelativePath(
            "/home/user/project/charts",
            "/home/user/project/charts/exports/chart.png");
        Console.WriteLine($"Relative path: {relativePath}");

        // Normalize path separators for current platform
        string normalizedPath = PathHelper.NormalizePath("charts/exports\\chart.png");
        Console.WriteLine($"Normalized path: {normalizedPath}");

        // Clean up old files in directory
        int deletedCount = PathHelper.CleanupOldFiles("/tmp/old-charts", TimeSpan.FromDays(7));
        Console.WriteLine($"Deleted {deletedCount} old files");
    }
}
```

## MathHelper

`MathHelper` provides mathematical utilities for chart calculations, including data normalization, interpolation, statistical analysis, and axis tick generation. These functions are optimized for performance with both `IEnumerable<float>` and `ReadOnlySpan<float>` overloads to minimize allocations.

```csharp
using System;
using SkiaSharpChartEngine.Utilities;

public class MathHelperExample
{
    public static void Main()
    {
        // Example 1: Find min/max values in a data series
        var data = new float[] { 1.2f, 3.4f, 5.6f, 2.1f, 4.8f };
        var (min, max) = MathHelper.GetMinMax(data);
        Console.WriteLine($"Data range: {min} to {max}");

        // Example 2: Normalize data to 0-1 range for chart scaling
        var normalized = data.Select(v => MathHelper.Normalize(v, min, max)).ToArray();

        // Example 3: Generate readable axis ticks
        var ticks = MathHelper.GenerateAxisTicks(min, max, desiredTickCount: 6);

        // Example 4: Calculate statistics for data analysis
        var average = MathHelper.Average(data);
        var stdDev = MathHelper.StandardDeviation(data);
        Console.WriteLine($"Average: {average:F2}, StdDev: {stdDev:F2}");

        // Example 5: Linear interpolation for smooth animations
        var interpolatedValue = MathHelper.Lerp(0f, 100f, 0.5f);
        Console.WriteLine($"Lerp(0, 100, 0.5) = {interpolatedValue}");

        // Example 6: Clamp values to valid ranges
        var clampedValue = MathHelper.Clamp(-10f, 0f, 100f);
        Console.WriteLine($"Clamp(-10, 0, 100) = {clampedValue}");

        // Example 7: Check if values are approximately equal (floating point safe)
        var isEqual = MathHelper.ApproximatelyEqual(0.1f + 0.2f, 0.3f);
        Console.WriteLine($"0.1 + 0.2 ≈ 0.3? {isEqual}");

        // Example 8: Round to nearest magnitude for axis labels
        var rounded = MathHelper.RoundToMagnitude(1234.56f);
        Console.WriteLine($"RoundToMagnitude(1234.56) = {rounded}");

        // Example 9: Scale normalized values to target range
        var scaled = MathHelper.ScaleToRange(0.75f, 0f, 1000f);
        Console.WriteLine($"ScaleToRange(0.75, 0, 1000) = {scaled}");
    }
}
```

## ChartExporter

`ChartExporter` provides functionality to export charts to various formats including JSON, CSV, and generates export summaries with metadata. It's useful for sharing charts, archiving data, and generating reports.

```csharp
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Utilities;

// Initialize exporter with logger
var logger = new NullLogger<ChartExporter>();
var exporter = new ChartExporter(logger);

// Create a sample chart
var chart = new Chart("sales-chart-2024");
chart.Title = "Quarterly Sales 2024";
chart.ChartType = ChartType.Line;

var series = new ChartSeries("Revenue")
{
    LineWidth = 2.5f,
    Color = "#2E86C1"
};
series.AddDataPoint("Q1", 100000.0);
series.AddDataPoint("Q2", 125000.0);
series.AddDataPoint("Q3", 150000.0);
series.AddDataPoint("Q4", 175000.0);
chart.AddSeries(series);

// Export to JSON format
var jsonExport = await exporter.ExportToJsonAsync(chart);
Console.WriteLine($"JSON export size: {jsonExport.Length} bytes");

// Export to CSV format
var csvExport = await exporter.ExportToCsvAsync(chart);
Console.WriteLine($"CSV export size: {csvExport.Length} bytes");

// Generate export summary with metadata
var summary = exporter.GenerateExportSummary(chart);
Console.WriteLine($"Chart exported: {summary.Title}");
Console.WriteLine($"  - Type: {summary.ChartType}");
Console.WriteLine($"  - Series: {summary.SeriesCount}");
Console.WriteLine($"  - Data points: {summary.TotalDataPoints}");
Console.WriteLine($"  - Value range: [{summary.MinValue}, {summary.MaxValue}]");
Console.WriteLine($"  - Created: {summary.CreatedAt}");
Console.WriteLine($"  - Exported: {summary.ExportedAt}");

// Validate export format
bool isValid = exporter.IsValidExportFormat("json");
Console.WriteLine($"Is 'json' a valid format? {isValid}");
```

## DateTimeHelper

`DateTimeHelper` provides date/time utilities for chart data processing and axis formatting. It includes methods for Unix timestamp conversion, business day calculations, period boundaries, week number extraction, and human-readable formatting.

```csharp
using System;
using SkiaSharpChartEngine.Utilities;

public class DateTimeHelperExample
{
    public static void Main()
    {
        // Example 1: Format datetime for chart axis labels
        var date1 = new DateTime(2024, 3, 15, 14, 30, 0);
        var date2 = new DateTime(2024, 3, 15, 9, 15, 0);
        var formatted1 = DateTimeHelper.FormatForAxis(date1, date2, new DateTime(2024, 3, 20));
        Console.WriteLine($"Formatted axis label: {formatted1}"); // "14:30"

        // Example 2: Convert Unix timestamp to DateTime
        var timestamp = 1710508800L; // 2024-03-15 00:00:00 UTC
        var fromUnix = DateTimeHelper.FromUnixTimestamp(timestamp);
        Console.WriteLine($"From Unix timestamp: {fromUnix}");

        // Example 3: Convert DateTime to Unix timestamp
        var toUnix = DateTimeHelper.ToUnixTimestamp(new DateTime(2024, 3, 15));
        Console.WriteLine($"To Unix timestamp: {toUnix}");

        // Example 4: Calculate business days between two dates
        var startDate = new DateTime(2024, 3, 1);
        var endDate = new DateTime(2024, 3, 15);
        var businessDays = DateTimeHelper.GetBusinessDaysBetween(startDate, endDate);
        Console.WriteLine($"Business days between: {businessDays}");

        // Example 5: Get period start (day, week, month, year)
        var periodStart = DateTimeHelper.GetPeriodStart(new DateTime(2024, 3, 15), DateTimePeriod.Month);
        Console.WriteLine($"Month start: {periodStart}"); // "2024-03-01 00:00:00"

        // Example 6: Get period end
        var periodEnd = DateTimeHelper.GetPeriodEnd(new DateTime(2024, 3, 15), DateTimePeriod.Month);
        Console.WriteLine($"Month end: {periodEnd}"); // "2024-03-31 23:59:59"

        // Example 7: Get week number
        var weekNumber = DateTimeHelper.GetWeekNumber(new DateTime(2024, 3, 15));
        Console.WriteLine($"Week number: {weekNumber}"); // 11

        // Example 8: Check if date is weekend
        var isWeekend = DateTimeHelper.IsWeekend(new DateTime(2024, 3, 16)); // Saturday
        Console.WriteLine($"Is weekend: {isWeekend}"); // True

        // Example 9: Get next occurrence of a day of week
        var nextMonday = DateTimeHelper.GetNextDayOfWeek(new DateTime(2024, 3, 15), DayOfWeek.Monday);
        Console.WriteLine($"Next Monday: {nextMonday}"); // "2024-03-18 00:00:00"

        // Example 10: Format timespan in human-readable format
        var timespan = TimeSpan.FromMinutes(90) + TimeSpan.FromSeconds(45);
        var formattedTimespan = DateTimeHelper.FormatTimespan(timespan);
        Console.WriteLine($"Formatted timespan: {formattedTimespan}"); // "1h 30m"

        // Example 11: Calculate age in years
        var birthDate = new DateTime(1990, 5, 20);
        var age = DateTimeHelper.GetAgeInYears(birthDate);
        Console.WriteLine($"Age: {age} years");

        // Example 12: Check if two dates are on the same day
        var sameDay = DateTimeHelper.IsSameDay(
            new DateTime(2024, 3, 15, 10, 0, 0),
            new DateTime(2024, 3, 15, 20, 0, 0)
        );
        Console.WriteLine($"Same day: {sameDay}"); // True
    }
}
```

## BitmapHelper

`BitmapHelper` provides utility methods for working with bitmap and image data. It includes image format validation, dimension calculations, DPI/PPI conversions, and aspect ratio utilities. This is useful for validating images, estimating file sizes, and calculating physical dimensions from pixel data.

```csharp
using System;
using SkiaSharpChartEngine.Utilities;

public class BitmapHelperExample
{
    public static void Main()
    {
        // Example 1: Validate PNG image data
        var pngData = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        bool isPngValid = BitmapHelper.IsPngValid(pngData);
        Console.WriteLine($"Is PNG valid? {isPngValid}"); // True

        // Example 2: Validate JPEG image data
        var jpegData = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 };
        bool isJpegValid = BitmapHelper.IsJpegValid(jpegData);
        Console.WriteLine($"Is JPEG valid? {isJpegValid}"); // True

        // Example 3: Get PNG dimensions
        var pngWithDimensions = new byte[] {
            0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A,
            0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52,
            0x00, 0x00, 0x00, 0xC8, // Width: 200
            0x00, 0x00, 0x00, 0x96, // Height: 150
            0x08, 0x06, 0x00, 0x00
        };
        var dimensions = BitmapHelper.GetPngDimensions(pngWithDimensions);
        if (dimensions.HasValue)
        {
            Console.WriteLine($"PNG dimensions: {dimensions.Value.Width}x{dimensions.Value.Height}");
        }

        // Example 4: Detect image format
        string detectedFormat = BitmapHelper.DetectFormat(pngData);
        Console.WriteLine($"Detected format: {detectedFormat}"); // "png"

        // Example 5: Estimate file size for a 1920x1080 PNG
        long estimatedSize = BitmapHelper.EstimateFileSize(1920, 1080, 96f, "png");
        Console.WriteLine($"Estimated PNG size: {estimatedSize} bytes");

        // Example 6: Convert PPI to DPI
        int dpi = BitmapHelper.GetDpiFromPpi(300);
        Console.WriteLine($"DPI from 300 PPI: {dpi}"); // 300

        // Example 7: Convert DPI to PPI
        float ppi = BitmapHelper.ConvertDpiToPpi(96f);
        Console.WriteLine($"PPI from 96 DPI: {ppi}"); // 96

        // Example 8: Calculate physical size from pixel dimensions
        var physicalSize = BitmapHelper.GetPhysicalSize(1920, 1080, 96f);
        Console.WriteLine($"Physical size at 96 DPI: {physicalSize.WidthInches:F2}\" x {physicalSize.HeightInches:F2}\"");

        // Example 9: Calculate pixel dimensions from physical size
        var pixelDimensions = BitmapHelper.GetPixelDimensions(8.5, 11, 300f);
        Console.WriteLine($"Pixel dimensions at 300 DPI: {pixelDimensions.WidthPixels}x{pixelDimensions.HeightPixels}");

        // Example 10: Validate image dimensions
        bool isValid = BitmapHelper.ValidateDimensions(1920, 1080);
        Console.WriteLine($"Are dimensions valid? {isValid}"); // True

        // Example 11: Calculate aspect ratio
        float aspectRatio = BitmapHelper.GetAspectRatio(16, 9);
        Console.WriteLine($"Aspect ratio (16:9): {aspectRatio:F2}"); // 1.78

        // Example 12: Scale image maintaining aspect ratio
        var scaledSize = BitmapHelper.ScaleMaintainingRatio(1920, 1080, 800, 600);
        Console.WriteLine($"Scaled to fit 800x600: {scaledSize.ScaledWidth}x{scaledSize.ScaledHeight}");
    }
}
```

## Architecture

See [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) for the component breakdown, data flow, design decisions, extension points, and known limitations. The sections below are per-type usage examples.

## TemplateLibrary

`TemplateLibrary` provides a centralized repository for managing reusable chart templates. It allows developers to register, query, and instantiate chart templates, facilitating the rapid creation of standardized charts across the application.

```csharp
using System;
using System.Linq;
using Microsoft.Extensions.Logging.Abstractions;
using SkiaSharpChartEngine.Utilities;
using SkiaSharpChartEngine.Models;

public class TemplateLibraryExample
{
    public static void Main()
    {
        // Initialize template library with a logger
        var logger = new NullLogger<TemplateLibrary>();
        var library = new TemplateLibrary(logger);

        // Example 1: List all available template names
        Console.WriteLine("Available templates: " + string.Join(", ", library.ListTemplateNames()));

        // Example 2: Retrieve a specific template
        var template = library.GetTemplate("simple_line");
        if (template != null)
        {
            Console.WriteLine($"Retrieved template: {template.Name} (Category: {template.Category})");
        }

        // Example 3: Get all templates by category
        var lineTemplates = library.GetTemplatesByCategory("Line");
        Console.WriteLine($"Found {lineTemplates.Count} line templates");

        // Example 4: Add a custom template
        var customTemplate = new ChartTemplate 
        { 
            Name = "Custom Area Chart", 
            Category = "Area",
            ChartConfiguration = new ChartConfiguration { ChartType = ChartType.Area } 
        };
        library.AddTemplate("custom_area", customTemplate);
        
        // Example 5: Instantiate a chart from a template
        var chart = library.CreateChartFromTemplate("custom_area", "my-custom-chart-id");
        if (chart != null)
        {
            Console.WriteLine($"Created chart: {chart.Title} (Type: {chart.ChartType})");
        }

        // Example 6: Get total template count
        Console.WriteLine($"Total templates in library: {library.GetTemplateCount()}");
    }
}
```

## VersionCompatibilityChecker

`VersionCompatibilityChecker` validates version compatibility for serialized charts and configurations. It ensures backward compatibility by checking if a version falls within the supported range (1.0.0 - 1.9.9), determines if migrations are needed, and validates version sequences. This utility is essential for maintaining data integrity when loading or saving chart configurations across different library versions.

## DataTransformer

`DataTransformer` provides a suite of data processing utilities for chart data manipulation. It includes methods for normalizing values, applying mathematical transformations, filtering outliers, calculating moving averages, scaling and offsetting data, and ranking data points. This transformer is particularly useful for preparing raw data before chart rendering, ensuring consistent and meaningful visualizations.

```csharp
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging.Abstractions;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Utilities;

public class DataTransformerExample
{
    public static void Main()
    {
        // Sample chart data points
        var dataPoints = new List<DataPoint>
        {
            new DataPoint { Label = "Jan", Value = 100 },
            new DataPoint { Label = "Feb", Value = 150 },
            new DataPoint { Label = "Mar", Value = 200 },
            new DataPoint { Label = "Apr", Value = 180 },
            new DataPoint { Label = "May", Value = 220 }
        };

        // Initialize DataTransformer with logger
        var logger = new NullLogger<DataTransformer>();
        var transformer = new DataTransformer(logger);

        // Example 1: Normalize values to 0-1 range
        var normalized = transformer.NormalizeValues(dataPoints);
        Console.WriteLine($"Normalized values: {normalized.Count} points");

        // Example 2: Apply logarithmic transformation for exponential data
        var logTransformed = transformer.ApplyLogTransformation(dataPoints);
        Console.WriteLine($"Log transformed values: {logTransformed.Count} points");

        // Example 3: Filter outliers using standard deviation
        var filtered = transformer.FilterOutliers(dataPoints, stdDevThreshold: 1.5);
        Console.WriteLine($"Filtered outliers: {filtered.Count} points remaining");

        // Example 4: Apply moving average for smoothing
        var smoothed = transformer.ApplyMovingAverage(dataPoints, windowSize: 3);
        Console.WriteLine($"Smoothed with moving average: {smoothed.Count} points");

        // Example 5: Scale values by multiplier
        var scaled = transformer.ScaleValues(dataPoints, multiplier: 2.5);
        Console.WriteLine($"Scaled values by 2.5");

        // Example 6: Offset values by a constant
        var offset = transformer.OffsetValues(dataPoints, offset: 50);
        Console.WriteLine($"Offset values by 50");

        // Example 7: Rank data points by value
        var ranked = transformer.RankDataPoints(dataPoints);
        Console.WriteLine($"Ranked data points:");
        foreach (var point in ranked)
        {
            Console.WriteLine($"  {point.Label}: Rank {point.Rank}");
        }
    }
}
```

```csharp
using System;
using Microsoft.Extensions.Logging.Abstractions;
using SkiaSharpChartEngine.Utilities;

public class VersionCompatibilityCheckerExample
{
    public static void Main()
    {
        // Initialize with a logger
        var logger = new NullLogger<VersionCompatibilityChecker>();
        var checker = new VersionCompatibilityChecker(logger);

        // Example 1: Check if a version is compatible
        bool isCompatible = checker.IsCompatible("1.0.0");
        Console.WriteLine($"Version 1.0.0 is compatible: {isCompatible}");

        // Example 2: Get current version
        string currentVersion = checker.GetCurrentVersion();
        Console.WriteLine($"Current version: {currentVersion}");

        // Example 3: Check if migration is needed
        bool needsMigration = checker.MigrationNeeded("0.9.0");
        Console.WriteLine($"Migration needed from 0.9.0: {needsMigration}");

        // Example 4: Get version information
        var versionInfo = checker.GetVersionInfo();
        Console.WriteLine($"Version Info: {versionInfo}");
        Console.WriteLine($"Supported range: {versionInfo.MinSupportedVersion} - {versionInfo.MaxSupportedVersion}");
        Console.WriteLine($"Checked at: {versionInfo.CheckedAt}");

        // Example 5: Validate version sequence
        bool isValidSequence = checker.ValidateVersionSequence("1.0.0", "1.1.0", "1.2.0");
        Console.WriteLine($"Version sequence is valid: {isValidSequence}");
    }
}
```

## BatchProcessor


`BatchProcessor` processes items in batches, providing an efficient way to handle bulk operations with configurable batch sizing and timeout settings. It is designed to manage large collections by breaking them into manageable chunks, ensuring timely processing, and tracking metrics such as total items, processed items, failed items, and elapsed time.

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using SkiaSharpChartEngine.Utilities;

public class BatchProcessorExample
{
    public static async Task Main()
    {
        var logger = new NullLogger<BatchProcessor<int>>();
        var items = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

        // Define the batch processing logic
        Func<List<int>, Task> processor = async (batch) =>
        {
            Console.WriteLine($"Processing batch of {batch.Count} items...");
            await Task.Delay(100); // Simulate work
        };

        // Create processor with batch size of 3 and 1000ms timeout
        var batchProcessor = new BatchProcessor<int>(3, 1000, processor, logger);

        // Process items
        await batchProcessor.ProcessAsync(items);

        // Example: Process a single item
        await batchProcessor.ProcessSingleAsync(11);

        Console.WriteLine("Batch processing finished.");
    }
}
```

## RenderResultCache

`RenderResultCache` is an in-memory cache specifically designed for storing rendered chart results. It automatically tracks cache hits/misses, evicts stale entries based on time-to-live (TTL), and maintains comprehensive statistics about cache usage. The cache uses a least-recently-used (LRU) eviction policy when approaching maximum size limits and includes a background timer for periodic cleanup of expired entries.

```csharp
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SkiaSharpChartEngine.Caching;
using SkiaSharpChartEngine.Models;

public class RenderResultCacheExample
{
    public static void Main()
    {
        // Initialize logger and cache with 100MB maximum size
        var logger = new NullLogger<RenderResultCache>();
        var cache = new RenderResultCache(logger, maxCacheSizeBytes: 100_000_000);

        // Example 1: Create a render result to cache
        var renderResult = new RenderResult
        {
            ImageData = new byte[1024 * 1024], // 1MB of image data
            Width = 1920,
            Height = 1080,
            Format = "PNG"
        };

        // Example 2: Cache the render result with default 1-hour TTL
        string cacheKey = "chart-sales-2024-line-1920x1080";
        cache.Cache(cacheKey, renderResult);
        Console.WriteLine("Render result cached successfully");

        // Example 3: Cache with custom TTL (24 hours)
        cache.Cache(
            "chart-sales-2024-line-800x600",
            renderResult,
            TimeSpan.FromHours(24)
        );

        // Example 4: Retrieve cached result
        var cachedResult = cache.Get(cacheKey);
        if (cachedResult != null)
        {
            Console.WriteLine($"Cache hit! Retrieved render result: {cachedResult.Width}x{cachedResult.Height}");
            Console.WriteLine($"Format: {cachedResult.Format}");
        }
        else
        {
            Console.WriteLine("Cache miss - entry not found or expired");
        }

        // Example 5: Remove specific cache entry
        bool removed = cache.Remove(cacheKey);
        Console.WriteLine($"Cache entry removed: {removed}");

        // Example 6: Get cache statistics
        var stats = cache.GetStatistics();
        Console.WriteLine($"\nCache Statistics:");
        Console.WriteLine($" Total entries: {stats.TotalEntries}");
        Console.WriteLine($" Total size: {stats.TotalSize:N0} bytes ({stats.TotalSize / 1024 / 1024} MB)");
        Console.WriteLine($" Max size: {stats.MaxSize:N0} bytes ({stats.MaxSize / 1024 / 1024} MB)");
        Console.WriteLine($" Total hits: {stats.TotalHits}");
        Console.WriteLine($" Total evictions: {stats.EvictionCount}");
        Console.WriteLine($" Oldest entry: {stats.OldestEntry}");
        Console.WriteLine($" Newest entry: {stats.NewestEntry}");

        // Example 7: Clear all cache entries
        cache.Clear();
        Console.WriteLine("All cache entries cleared");

        // Dispose cache when done
        cache.Dispose();
    }
}
```

## ConcurrencyLimiter

`ConcurrencyLimiter` provides a semaphore-based mechanism to throttle concurrent operations. It is particularly useful for limiting resource-intensive tasks such as chart rendering to prevent excessive CPU or memory consumption.

```csharp
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using SkiaSharpChartEngine.Utilities;

public class ConcurrencyLimiterExample
{
    public static async Task Main()
    {
        var logger = new NullLogger<ConcurrencyLimiter>();
        // Limit concurrency to 2 simultaneous operations
        var limiter = new ConcurrencyLimiter(2, logger);

        // Example 1: Async operation with return value
        var result = await limiter.ExecuteAsync(async () =>
        {
            await Task.Delay(100);
            return "Task completed";
        });
        Console.WriteLine(result);

        // Example 2: Async operation without return value
        await limiter.ExecuteAsync(async () =>
        {
            await Task.Delay(100);
            Console.WriteLine("Task executed");
        });

        // Example 3: Sync operation
        var syncResult = limiter.Execute(() => 42);
        Console.WriteLine($"Sync result: {syncResult}");

        // Example 4: Check capacity
        Console.WriteLine($"Available slots: {limiter.GetAvailableSlots()}");
        Console.WriteLine($"Used slots: {limiter.GetUsedSlots()}");

        // Example 5: Wait until a slot becomes available
        await limiter.WaitAsync();

        limiter.Dispose();
    }
}
```

## ColorSchemeManager

`ColorSchemeManager` provides a centralized system for managing and generating color schemes for chart visualization. It allows developers to register custom color schemes, retrieve existing schemes by name, and generate color variations such as complementary, analogous, and triadic color palettes. This is particularly useful for creating visually appealing and consistent color schemes across multiple chart types.

```csharp
using System;
using Microsoft.Extensions.Logging.Abstractions;
using SkiaSharp;
using SkiaSharpChartEngine.Utilities;

public class ColorSchemeManagerExample
{
    public static void Main()
    {
        // Initialize ColorSchemeManager with a logger
        var logger = new NullLogger<ColorSchemeManager>();
        var schemeManager = new ColorSchemeManager(logger);

        // Example 1: List all available color scheme names
        Console.WriteLine("Available color schemes: " + string.Join(", ", schemeManager.ListAvailableSchemes()));

        // Example 2: Get a specific color scheme by name
        var scheme = schemeManager.GetScheme("vibrant");
        Console.WriteLine($"Retrieved scheme: {scheme.Name} ({scheme.Description})");
        Console.WriteLine($"Colors: {string.Join(", ", Array.ConvertAll(scheme.Colors, c => c.ToString()))}");

        // Example 3: Register a custom color scheme
        var customScheme = new ColorScheme
        {
            Name = "Corporate Teal",
            Description = "Professional color scheme with teal and gray tones",
            Colors = new[] { SKColors.Teal, SKColors.DarkSlateGray, SKColors.LightSlateGray, SKColors.WhiteSmoke }
        };
        schemeManager.RegisterScheme("corporate_teal", customScheme);

        // Example 4: Get colors from the newly registered scheme
        var corporateScheme = schemeManager.GetScheme("corporate_teal");
        Console.WriteLine($"Registered scheme: {corporateScheme.Name}");

        // Example 5: Generate complementary colors
        var baseColor = SKColors.DodgerBlue;
        var complementary = schemeManager.GetComplementary(baseColor);
        Console.WriteLine($"Complementary of {baseColor}: {complementary}");

        // Example 6: Generate analogous colors
        var analogousColors = schemeManager.GetAnalogous(baseColor, 5);
        Console.WriteLine($"Analogous colors (count: {analogousColors.Length}): {string.Join(", ", analogousColors)}");

        // Example 7: Generate triadic colors
        var triadicColors = schemeManager.GetTriadic(baseColor);
        Console.WriteLine($"Triadic colors (count: {triadicColors.Length}): {string.Join(", ", triadicColors)}");

        // Example 8: Get a specific color from a scheme by index
        var firstColor = schemeManager.GetColor("vibrant", 0);
        Console.WriteLine($"First color from 'vibrant' scheme: {firstColor}");
    }
}
```

## CachePolicy

`CachePolicy` is a configuration class that defines caching behavior for chart rendering operations. It controls expiration policies, priority-based eviction, and post-eviction callbacks, enabling fine-grained control over cache management strategies.

```csharp
using System;
using SkiaSharpChartEngine.Caching;

public class CachePolicyExample
{
    public static void Main()
    {
        // Example 1: Create a cache policy with absolute expiration (1 hour from now)
        var absoluteExpirationPolicy = new CachePolicy
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
            Priority = CachePriority.Normal
        };

        Console.WriteLine($"Absolute expiration policy created: {absoluteExpirationPolicy.GetAbsoluteExpiration}");

        // Example 2: Create a cache policy with sliding expiration (30 minutes sliding window)
        var slidingExpirationPolicy = new CachePolicy
        {
            SlidingExpiration = TimeSpan.FromMinutes(30),
            Priority = CachePriority.High
        };

        Console.WriteLine($"Sliding expiration policy created: {slidingExpirationPolicy.GetAbsoluteExpiration}");

        // Example 3: Create a cache policy with both absolute and sliding expiration
        var combinedExpirationPolicy = new CachePolicy
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24),
            SlidingExpiration = TimeSpan.FromMinutes(15),
            Priority = CachePriority.Normal
        };

        Console.WriteLine($"Combined expiration policy created: {combinedExpirationPolicy.GetAbsoluteExpiration}");

        // Example 4: Use factory methods for common scenarios
        var minutesPolicy = CachePolicy.ExpiresAfterMinutes(30);
        Console.WriteLine($"30-minute expiration policy: {minutesPolicy.GetAbsoluteExpiration}");

        var hoursPolicy = CachePolicy.ExpiresAfterHours(2);
        Console.WriteLine($"2-hour expiration policy: {hoursPolicy.GetAbsoluteExpiration}");

        // Example 5: Configure post-eviction callback
        var policyWithCallback = new CachePolicy
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
            Priority = CachePriority.Normal,
            PostEvictionCallbacks = key =>
            {
                Console.WriteLine($"Cache entry evicted: {key}");
                // Additional cleanup logic can be added here
            }
        };

        Console.WriteLine("Policy with eviction callback configured");

        // Example 6: Check policy validity
        Console.WriteLine($"Is absolute expiration policy valid: {absoluteExpirationPolicy.IsValid}");
        Console.WriteLine($"Is sliding expiration policy valid: {slidingExpirationPolicy.IsValid}");

        // Example 7: Access cache policy properties
        Console.WriteLine($"\nCache policy properties:");
        Console.WriteLine($"  Key: {absoluteExpirationPolicy.Key}");
        Console.WriteLine($"  CreatedAt: {absoluteExpirationPolicy.CreatedAt}");
        Console.WriteLine($"  Priority: {absoluteExpirationPolicy.Priority}");
        Console.WriteLine($"  Age: {absoluteExpirationPolicy.GetAge}");
        Console.WriteLine($"  Is expired: {absoluteExpirationPolicy.IsExpired}");
        Console.WriteLine($"  Time to expiration: {absoluteExpirationPolicy.GetTimeToExpiration}");
    }
}
```

## TemplateController

`TemplateController` is a REST API controller that handles template management operations. It provides endpoints for retrieving, creating, updating, and deleting chart templates.

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SkiaSharpChartEngine.API.Controllers;
using SkiaSharpChartEngine.API.Responses;
using SkiaSharpChartEngine.Models;

public class TemplateControllerExample
{
    public static async Task Main(string[] args)
    {
        // Initialize controller (typically injected via DI in real applications)
        var templateController = new TemplateController(
            new ChartDataService(), // Mock service for demonstration
            new Microsoft.Extensions.Logging.Abstractions.NullLogger<TemplateController>()
        );

        // Get all templates
        var templatesResponse = await templateController.GetAllTemplatesAsync();
        Console.WriteLine($"Templates count: {templatesResponse.Data?.Count}");

        // Get a template by ID
        var templateResponse = await templateController.GetTemplateByIdAsync("template-123");
        Console.WriteLine($"Template ID: {templateResponse.Data?.Id}");
        Console.WriteLine($"Template Name: {templateResponse.Data?.Name}");

        // Get templates by type
        var templatesByTypeResponse = await templateController.GetTemplatesByTypeAsync("line");
        Console.WriteLine($"Templates count by type: {templatesByTypeResponse.Data?.Count}");

        // Create a new template
        var createdTemplateResponse = await templateController.CreateTemplateAsync(
            new ChartTemplate { Name = "My Template", Type = "line" }
        );
        Console.WriteLine($"Created template ID: {createdTemplateResponse.Data?.Id}");

        // Delete a template
        var deleted = await templateController.DeleteTemplateAsync("template-123");
        Console.WriteLine($"Template deleted: {deleted}");
    }
}
```

## ChartTemplate

`ChartTemplate` is a reusable template for creating charts with predefined settings, configurations, and default series. It serves as a blueprint for generating charts with consistent styling and data structure, reducing boilerplate code when creating similar charts across your application.

Chart templates store chart type, base configuration (colors, margins, axis labels), default series with their data points, metadata like creation timestamp and author, and custom properties for extended functionality. Templates can be cloned, modified, and used to generate new chart instances on demand.

```csharp
using System;
using System.Collections.Generic;
using SkiaSharpChartEngine.Models;

public class ChartTemplateExample
{
    public static void Main()
    {
        // Example 1: Create a basic chart template
        var basicTemplate = new ChartTemplate("Sales Dashboard", ChartType.LineChart)
        {
            Description = "Quarterly sales performance template",
            CreatedBy = "system"
        };
        
        Console.WriteLine($"Created template: {basicTemplate}");
        Console.WriteLine($"Template ID: {basicTemplate.TemplateId}");
        Console.WriteLine($"Created at: {basicTemplate.CreatedAt}");
        
        // Example 2: Configure base chart configuration
        basicTemplate.BaseConfiguration = new ChartConfiguration
        {
            Title = "Sales Performance",
            Subtitle = "Q2 2024 Report",
            XAxisLabel = "Quarter",
            YAxisLabel = "Revenue ($)",
            BackgroundColor = "#FFFFFF",
            MarginTop = 40,
            MarginBottom = 60,
            MarginLeft = 60,
            MarginRight = 40
        };
        
        // Example 3: Add default series with data points
        var revenueSeries = new ChartSeries("Revenue")
        {
            LineWidth = 2.5f,
            Color = "#2E86C1",
            Description = "Quarterly revenue"
        };
        
        revenueSeries.AddDataPoint(1.0, 100000.0);
        revenueSeries.AddDataPoint(2.0, 125000.0);
        revenueSeries.AddDataPoint(3.0, 150000.0);
        revenueSeries.AddDataPoint(4.0, 175000.0);
        
        basicTemplate.DefaultSeries.Add(revenueSeries);
        
        // Example 4: Add custom properties for extended functionality
        basicTemplate.CustomProperties = new Dictionary<string, object>
        {
            ["unit"] = "USD",
            ["precision"] = 2,
            ["autoRefreshInterval"] = 300 // 5 minutes in seconds
        };
        
        // Example 5: Clone a template to create variations
        var salesTemplateClone = basicTemplate.Clone();
        salesTemplateClone.Name = "Sales Dashboard (Clone)";
        salesTemplateClone.Description = "Modified version of sales template";
        
        // Example 6: Generate a chart from the template
        var chartFromTemplate = basicTemplate.CreateChartFromTemplate();
        Console.WriteLine($"Generated chart: {chartFromTemplate.Title}");
        Console.WriteLine($"Chart type: {chartFromTemplate.Type}");
        Console.WriteLine($"Number of series: {chartFromTemplate.Series.Count}");
        Console.WriteLine($"Data points in first series: {chartFromTemplate.Series[0].GetDataPointCount()}");
        
        // Example 7: Modify template configuration
        var expenseTemplate = new ChartTemplate("Expense Tracker", ChartType.BarChart)
        {
            BaseConfiguration = new ChartConfiguration
            {
                Title = "Monthly Expenses",
                XAxisLabel = "Category",
                YAxisLabel = "Amount ($)",
                ShowGrid = true,
                ShowLegend = true
            }
        };
        
        var marketingSeries = new ChartSeries("Marketing")
        {
            Color = "#E74C3C",
            BarWidth = 0.8f
        };
        marketingSeries.AddDataPoint(1.0, 15000.0);
        marketingSeries.AddDataPoint(2.0, 18000.0);
        marketingSeries.AddDataPoint(3.0, 12000.0);
        
        expenseTemplate.DefaultSeries.Add(marketingSeries);
        
        // Generate chart from expense template
        var expenseChart = expenseTemplate.CreateChartFromTemplate();
        Console.WriteLine($"Expense chart generated: {expenseChart.Series[0].Name}");
    }
}
```

## ChartEngineCliInterface

`ChartEngineCliInterface` is the command-line interface for the SkiaSharp Chart Engine. It provides a structured way to parse command-line arguments and execute CLI commands for chart rendering, exporting, validation, and other operations. The interface supports multiple commands (render, export, validate, help, version) and handles argument parsing with proper error logging and user feedback.

The CLI interface can be used standalone or integrated into larger applications, providing a consistent entry point for chart-related operations through the command pattern.

```csharp
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SkiaSharpChartEngine.CLI;
using SkiaSharpChartEngine.Models;

public class ChartEngineCliInterfaceExample
{
    public static async Task Main(string[] args)
    {
        // Initialize chart engine and logger
        var chartEngine = ChartEngine.Create();
        var logger = new NullLogger<ChartEngineCliInterface>();
        
        // Create CLI interface instance
        var cliInterface = new ChartEngineCliInterface(chartEngine, logger);
        
        // Example 1: Display help
        Console.WriteLine("=== Displaying help ===");
        await cliInterface.ExecuteAsync(new[] { "help" });
        
        // Example 2: Display version
        Console.WriteLine("\n=== Displaying version ===");
        await cliInterface.ExecuteAsync(new[] { "version" });
        
        // Example 3: Render a chart (basic usage)
        Console.WriteLine("\n=== Rendering chart ===");
        int renderResult = await cliInterface.ExecuteAsync(new[] {
            "render",
            "--type", "line",
            "--output", "./output/line-chart.png"
        });
        Console.WriteLine($"Render command result: {(renderResult == 0 ? "Success" : "Failed")}");
        
        // Example 4: Export chart to multiple formats
        Console.WriteLine("\n=== Exporting chart ===");
        int exportResult = await cliInterface.ExecuteAsync(new[] {
            "export",
            "--chart-id", "chart-123",
            "--formats", "png,svg,pdf"
        });
        Console.WriteLine($"Export command result: {(exportResult == 0 ? "Success" : "Failed")}");
        
        // Example 5: Validate chart configuration
        Console.WriteLine("\n=== Validating configuration ===");
        int validateResult = await cliInterface.ExecuteAsync(new[] {
            "validate",
            "--config", "./configs/chart-config.json"
        });
        Console.WriteLine($"Validate command result: {(validateResult == 0 ? "Success" : "Failed")}");
    }
}
```

## CommandRouter

`CommandRouter` is a command routing system that implements the command pattern for CLI applications. It routes command-line arguments to appropriate command executors based on registered command names, providing a clean separation between command parsing, routing, and execution. The router supports help display, error handling, and maintains a registry of available commands.

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SkiaSharpChartEngine.CLI;

public class CommandRouterExample
{
    public static async Task Main(string[] args)
    {
        // Initialize logger and argument parser
        var logger = new NullLogger<CommandRouter>();
        var argumentParser = new ArgumentParser(logger);
        
        // Create command router
        var commandRouter = new CommandRouter(logger, argumentParser);
        
        // Register command handlers
        commandRouter.RegisterCommand("render", new RenderChartCommandExecutor());
        commandRouter.RegisterCommand("export", new ExportChartCommandExecutor());
        commandRouter.RegisterCommand("validate", new ValidateChartCommandExecutor());
        
        // Display available commands
        commandRouter.DisplayHelp();
        
        // Get list of registered commands
        var commands = commandRouter.GetRegisteredCommands();
        Console.WriteLine($"\nRegistered commands: {string.Join(", ", commands)}");
        
        // Route and execute a command (simulated)
        var result = await commandRouter.RouteAsync(new[] { "render", "--chart=quarterly-sales", "--output=chart.png" });
        Console.WriteLine($"Command executed with result: {result}");
    }
}

// Example command executor implementation
public class RenderChartCommandExecutor : ICommandExecutor
{
    public async Task<bool> ExecuteAsync(Dictionary<string, string> arguments)
    {
        Console.WriteLine("Executing render command...");
        
        if (arguments.TryGetValue("chart", out var chartName))
        {
            Console.WriteLine($"Rendering chart: {chartName}");
        }
        
        if (arguments.TryGetValue("output", out var outputFile))
        {
            Console.WriteLine($"Output file: {outputFile}");
        }
        
        // Simulate async work
        await Task.Delay(100);
        
        return true;
    }
}

// Example command executor for export
public class ExportChartCommandExecutor : ICommandExecutor
{
    public async Task<bool> ExecuteAsync(Dictionary<string, string> arguments)
    {
        Console.WriteLine("Executing export command...");
        
        if (arguments.TryGetValue("chart", out var chartName))
        {
            Console.WriteLine($"Exporting chart: {chartName}");
        }
        
        if (arguments.TryGetValue("format", out var format))
        {
            Console.WriteLine($"Export format: {format}");
        }
        
        await Task.Delay(50);
        return true;
    }
}

// Example command executor for validation
public class ValidateChartCommandExecutor : ICommandExecutor
{
    public async Task<bool> ExecuteAsync(Dictionary<string, string> arguments)
    {
        Console.WriteLine("Executing validate command...");
        
        if (arguments.TryGetValue("chart", out var chartName))
        {
            Console.WriteLine($"Validating chart: {chartName}");
        }
        
        await Task.Delay(25);
        return true;
    }
}
```

## AsyncDataLoader

`AsyncDataLoader` is a utility class for asynchronously loading chart data from various sources including JSON files, CSV files, and directories. It provides methods for loading individual charts or multiple charts from file systems, with automatic format detection and validation.

```csharp
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SkiaSharpChartEngine.Integration;
using SkiaSharpChartEngine.Models;

public class AsyncDataLoaderExample
{
    public static async Task Main(string[] args)
    {
        // Initialize the data loader with logger
        var logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<AsyncDataLoader>();
        var dataLoader = new AsyncDataLoader(logger);

        // Example 1: Load a single chart from a JSON file
        var jsonChart = await dataLoader.LoadChartFromFileAsync("chart-data.json");
        Console.WriteLine(jsonChart != null
            ? $"Loaded JSON chart: {jsonChart.Title}"
            : "Failed to load JSON chart");

        // Example 2: Load a chart from a CSV file
        var csvChart = await dataLoader.LoadChartFromFileAsync("sales-data.csv");
        Console.WriteLine(csvChart != null
            ? $"Loaded CSV chart with {csvChart.Series.Count} series"
            : "Failed to load CSV chart");

        // Example 3: Load multiple charts from a directory
        var chartsFromDir = await dataLoader.LoadChartsFromDirectoryAsync(
            "./charts/",
            "*.json"
        );
        Console.WriteLine($"Loaded {chartsFromDir.Count} charts from directory");

        // Example 4: Parse CSV data directly
        var csvData = "Quarter,Revenue,Expenses\nQ1,100000,85000\nQ2,125000,92000";
        var dataPoints = dataLoader.ParseCsvData(csvData);
        Console.WriteLine($"Parsed {dataPoints.Count} data points from CSV");

        // Example 5: Check if a file can be loaded
        bool canLoad = dataLoader.CanLoadFile("chart.json");
        Console.WriteLine($"Can load file: {canLoad}");
    }
}
```

## ChartEngineException

`ChartEngineException` is a custom exception class that represents an error that occurred during chart engine operations. It provides a way to handle and propagate errors in a meaningful way.

```csharp
try
{
    // Code that may throw an exception
}
catch (ChartEngineException ex)
{
    Console.WriteLine($"Error code: {ex.ErrorCode}");
    Console.WriteLine($"Error message: {ex.Message}");
}

// Example of throwing a ChartEngineException
throw new ChartEngineException("Invalid chart data", new InvalidChartDataException("Invalid data points"));
```

## ChartRenderingPipeline

`ChartRenderingPipeline` is a pipeline processor for chart rendering operations. It executes a sequence of stages sequentially with support for middleware-style interceptors. The pipeline maintains execution context, tracks performance metrics, and provides detailed execution results for each stage.

The pipeline supports:
- Adding custom rendering stages
- Registering interceptors for before/after pipeline execution
- Tracking execution context and data across stages
- Measuring performance of individual stages
- Comprehensive error handling and reporting

```csharp
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Pipeline;

public class ChartRenderingPipelineExample
{
    public static async Task Main(string[] args)
    {
        // Initialize pipeline with logger
        var logger = new NullLogger<ChartRenderingPipeline>();
        var pipeline = new ChartRenderingPipeline(logger);

        // Create a simple chart with data
        var chart = new Chart("sales-chart");
        var series = new ChartSeries("Revenue")
        {
            LineWidth = 2.5f,
            Color = "#2E86C1"
        };
        series.AddDataPoint(1.0, 100000.0);
        series.AddDataPoint(2.0, 125000.0);
        series.AddDataPoint(3.0, 150000.0);
        series.AddDataPoint(4.0, 175000.0);
        chart.AddSeries(series);

        // Add a custom pipeline stage
        pipeline.AddStage(new DataValidationStage());
        
        // Add a pipeline interceptor for logging
        pipeline.AddInterceptor(new LoggingInterceptor());

        // Create pipeline context with initial data
        var context = new PipelineContext();
        context.Set("requestId", Guid.NewGuid());
        context.Set("userId", "user-123");

        // Execute the pipeline
        var result = await pipeline.ExecuteAsync(chart, context);

        // Check execution results
        Console.WriteLine($"Pipeline completed: {result.Success}");
        Console.WriteLine($"Total duration: {result.TotalDurationMs}ms");
        Console.WriteLine($"Successful stages: {result.SuccessfulStages}");
        Console.WriteLine($"Failed stages: {result.FailedStages}");

        if (!result.Success)
        {
            Console.WriteLine($"Error: {result.ErrorMessage}");
        }

        // Access stage-specific results
        foreach (var stageResult in result.StageResults)
        {
            Console.WriteLine($"Stage '{stageResult.StageName}': {stageResult.Success} in {stageResult.DurationMs}ms");
            if (!stageResult.Success && stageResult.Message != null)
            {
                Console.WriteLine($"  Error: {stageResult.Message}");
            }
        }

        // Access context data after execution
        var requestId = context.Get<Guid>("requestId");
        Console.WriteLine($"Request ID from context: {requestId}");
    }
}

// Example pipeline stage implementation
public class DataValidationStage : IPipelineStage
{
    public string Name => "DataValidation";

    public async Task<PipelineStageResult> ExecuteAsync(
        Chart chart, 
        PipelineContext context, 
        System.Threading.CancellationToken cancellationToken)
    {
        // Validate chart has data
        if (chart.Series.Count == 0)
        {
            return PipelineStageResult.Failure("Chart has no series data");
        }

        // Validate each series has data points
        foreach (var series in chart.Series)
        {
            if (series.DataPoints.Count == 0)
            {
                return PipelineStageResult.Failure($"Series '{series.Name}' has no data points");
            }
        }

        // Store validation result in context
        context.Set("validationPassed", true);
        context.Set("dataQualityScore", 95.5);

        return PipelineStageResult.Success("Data validated successfully");
    }
}

// Example pipeline interceptor implementation
public class LoggingInterceptor : IPipelineInterceptor
{
    public async Task OnBeforeAsync(
        PipelineContext context, 
        System.Threading.CancellationToken cancellationToken)
    {
        var requestId = context.Get<Guid>("requestId");
        Console.WriteLine($"[Before] Pipeline starting - Request ID: {requestId}");
        await Task.CompletedTask;
    }

    public async Task OnAfterAsync(
        PipelineResult result, 
        PipelineContext context, 
        System.Threading.CancellationToken cancellationToken)
    {
        var requestId = context.Get<Guid>("requestId");
        var duration = result.TotalDurationMs;
        Console.WriteLine($"[After] Pipeline completed - Request ID: {requestId}, Duration: {duration}ms, Success: {result.Success}");
        await Task.CompletedTask;
    }
}
```

## ChartSeries

`ChartSeries` represents a data series in a chart. It contains configuration properties for visual appearance (line width, color, visibility), axis range constraints, descriptive metadata, and data points. Chart series are the building blocks for multi-series charts and support various chart types including line, bar, pie, and more.

```csharp
using System;
using SkiaSharpChartEngine.Models;

public class ChartSeriesExample
{
    public static void Main()
    {
        // Create a line series with custom styling
        var revenueSeries = new ChartSeries("Revenue")
        {
            SeriesType = ChartType.LineChart,
            LineWidth = 2.5f,
            Color = "#2E86C1",
            IsVisible = true,
            Description = "Quarterly revenue performance",
            ZIndex = 1
        };
        
        // Add data points to the series
        revenueSeries.AddDataPoint(1.0, 100000.0);
        revenueSeries.AddDataPoint(2.0, 125000.0);
        revenueSeries.AddDataPoint(3.0, 150000.0);
        revenueSeries.AddDataPoint(4.0, 175000.0);
        
        // Set Y-axis range constraints
        revenueSeries.YAxisMin = 0;
        revenueSeries.YAxisMax = 200000;
        
        // Add custom properties for extended functionality
        revenueSeries.CustomProperties = new Dictionary<string, object>
        {
            ["unit"] = "USD",
            ["precision"] = 2
        };
        
        Console.WriteLine($"Series created: {revenueSeries.Name}");
        Console.WriteLine($"Data points: {revenueSeries.GetDataPointCount()}");
        Console.WriteLine($"Y-axis range: [{revenueSeries.GetYAxisRange().min}, {revenueSeries.GetYAxisRange().max}]");
        
        // Clone a series for reuse with different data
        var clonedSeries = revenueSeries.Clone();
        clonedSeries.Name = "Revenue (Clone)";
        clonedSeries.AddDataPoint(5.0, 200000.0);
        
        Console.WriteLine($"Cloned series: {clonedSeries.Name}");
        Console.WriteLine($"Cloned data points: {clonedSeries.GetDataPointCount()}");
    }
}
```



## ChartBuilder

The `ChartBuilder` class provides a fluent interface for creating and configuring charts with a clean, method-chained API. It allows you to define chart properties such as title, size, margins, axes, colors, and series through a builder pattern before finalizing the chart with the `Build()` method.



### Example Usage

```csharp
using SkiaSharpChartEngine;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Utilities;

// Create a line chart with multiple series
var chart = new ChartBuilder(ChartType.LineChart)
    .WithTitle("Sales Performance 2024")
    .WithSubtitle("Quarterly revenue comparison")
    .WithSize(800, 600)
    .WithMargins(40, 60, 40, 60)
    .WithAxisLabels("Quarter", "Revenue ($)")
    .WithAxisScales(AxisScaleType.Linear, AxisScaleType.Linear)
    .WithAxisRange(null, null, 0, 100000)
    .WithBackgroundColor("#FFFFFF")
    .WithGridColor("#E0E0E0")
    .ShowGrid(true)
    .ShowLegend(true)
    .ShowAxisLabels(true)
    .ShowDataPointLabels(true)
    .EnableAnimation(true)
    .WithAnimationDuration(1500)
    .EnableAntiAliasing(true)
    .AddSeries("Q1 2024", "#2E86C1")
    .AddDataPointToLastSeries(1, 45000, "Jan")
    .AddDataPointToLastSeries(2, 52000, "Feb")
    .AddDataPointToLastSeries(3, 61000, "Mar")
    .AddSeries("Q2 2024", "#E74C3C")
    .AddDataPointToLastSeries(1, 58000, "Apr")
    .AddDataPointToLastSeries(2, 63000, "May")
    .AddDataPointToLastSeries(3, 72000, "Jun")
    .Build();
```

### Alternative Usage with Predefined Data

```csharp
// Create a chart with predefined data points
var dataPoints = new List<DataPoint>
{
    new DataPoint(1, 45000),
    new DataPoint(2, 52000),
    new DataPoint(3, 61000)
};

var chart = new ChartBuilder(ChartType.BarChart)
    .WithTitle("Product Sales")
    .WithSize(600, 400)
    .AddSeriesWithData("Product A", dataPoints, "#3498DB")
    .Build();
```

## AnimationFrameGenerator

```csharp
using System.Collections.Generic;
using SkiaSharpChartEngine.Rendering;

public class AnimationFrameGeneratorExample
{
    public static async Task Main(string[] args)
    {
        // Initialize animation frame generator
        var animationFrameGenerator = new AnimationFrameGenerator();

        // Generate frames
        var frames = animationFrameGenerator.GenerateFrames();
        Console.WriteLine($"Generated frames count: {frames.Count}");

        // Generate data frames
        var dataFrames = animationFrameGenerator.GenerateDataFrames();
        Console.WriteLine($"Generated data frames count: {dataFrames.Count}");

        // Get current frame number
        var frameNumber = animationFrameGenerator.FrameNumber;
        Console.WriteLine($"Current frame number: {frameNumber}");

        // Get current progress
        var progress = animationFrameGenerator.Progress;
        Console.WriteLine($"Current progress: {progress}");

        // Get current values
        var values = animationFrameGenerator.Values;
        Console.WriteLine($"Current values count: {values.Count}");
    }
}
```

## IInteractivityService

`IInteractivityService` provides interactive chart capabilities including nearest-point tooltip hit-testing and zoom/pan viewport management over a chart's data coordinate space. It enables interactive features like hover tooltips, chart zooming, panning, and viewport reset operations.

```csharp
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SkiaSharp;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Services;

public class InteractivityServiceExample
{
    public static void Main()
    {
        // Initialize services
        var logger = new NullLogger<InteractivityService>();
        var interactivityService = new InteractivityService(logger);

        // Create a chart with sample data
        var chart = new Chart("sales-chart");
        var series = new ChartSeries("Revenue")
        {
            LineWidth = 2.5f,
            Color = "#2E86C1"
        };
        series.AddDataPoint(1.0, 100000.0);
        series.AddDataPoint(2.0, 125000.0);
        series.AddDataPoint(3.0, 150000.0);
        series.AddDataPoint(4.0, 175000.0);
        chart.AddSeries(series);

        // Example 1: Hit-test to find nearest data point to cursor position
        var hitResult = interactivityService.HitTest(
            chart,
            pointerX: 200,
            pointerY: 300,
            canvasWidth: 800,
            canvasHeight: 600
        );
        
        if (hitResult.IsHit)
        {
            Console.WriteLine($"Hit data point: X={hitResult.DataPoint.X}, Y={hitResult.DataPoint.Y}");
            Console.WriteLine($"Tooltip text: {hitResult.TooltipText}");
        }

        // Example 2: Zoom in at a specific point
        var viewport = new ViewportState();
        var zoomedViewport = interactivityService.Zoom(
            chart,
            viewport,
            anchorX: 400,
            anchorY: 300,
            canvasWidth: 800,
            canvasHeight: 600,
            factor: 2.0 // 2x zoom in
        );
        Console.WriteLine($"Zoomed viewport: ZoomX={zoomedViewport.ZoomX:F2}, ZoomY={zoomedViewport.ZoomY:F2}");

        // Example 3: Pan the viewport
        var pannedViewport = interactivityService.Pan(
            chart,
            zoomedViewport,
            deltaX: 50,
            deltaY: 25,
            canvasWidth: 800,
            canvasHeight: 600
        );
        Console.WriteLine($"Panned viewport: PanX={pannedViewport.PanX:F2}, PanY={pannedViewport.PanY:F2}");

        // Example 4: Reset viewport to show full data range
        var resetViewport = interactivityService.ResetViewport(chart);
        Console.WriteLine($"Reset viewport visible range: X=[{resetViewport.VisibleXRange.Min:F2}, {resetViewport.VisibleXRange.Max:F2}], Y=[{resetViewport.VisibleYRange.Min:F2}, {resetViewport.VisibleYRange.Max:F2}]");

        // Example 5: Get visible data range for current viewport
        var visibleRange = interactivityService.GetVisibleRange(chart, pannedViewport);
        Console.WriteLine($"Visible range: X=[{visibleRange.minX:F2}, {visibleRange.maxX:F2}], Y=[{visibleRange.minY:F2}, {visibleRange.maxY:F2}]");

        // Example 6: Format tooltip with custom template
        var tooltipWithTemplate = interactivityService.FormatTooltip(
            hitResult,
            new TooltipOptions
            {
                ContentTemplate = "{series}: {y:C0} at {x}"
            }
        );
        Console.WriteLine($"Formatted tooltip: {tooltipWithTemplate}");
    }
}
```

## ChartInteractionService

`ChartInteractionService` is the default implementation of `IChartInteractionService` that handles user interactions with chart elements such as clicks, hovers, selections, and context menu gestures. It delegates hit-testing to `IInteractivityService` and maintains per-chart selection state in a thread-safe dictionary, enabling interactive chart features like data point selection and hover tooltips.

The service provides both synchronous and asynchronous methods for processing interactions, supports toggling data point selections, clearing selections, and retrieving the current selection state for any chart.

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SkiaSharp;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Services;

public class ChartInteractionServiceExample
{
    public static void Main()
    {
        // Initialize services
        var logger = new NullLogger<ChartInteractionService>();
        var interactivityService = new InteractivityService();
        var interactionService = new ChartInteractionService(interactivityService, logger);

        // Create a chart with sample data
        var chart = new Chart("sales-chart");
        var series = new ChartSeries("Revenue")
        {
            LineWidth = 2.5f,
            Color = "#2E86C1"
        };
        series.AddDataPoint(1.0, 100000.0);
        series.AddDataPoint(2.0, 125000.0);
        series.AddDataPoint(3.0, 150000.0);
        series.AddDataPoint(4.0, 175000.0);
        chart.AddSeries(series);

        // Example 1: Process a click interaction
        var clickArgs = interactionService.ProcessInteraction(
            chart,
            ChartInteractionType.Click,
            pointerX: 200,
            pointerY: 300,
            canvasWidth: 800,
            canvasHeight: 600
        );
        Console.WriteLine($"Click processed - Hit: {clickArgs.Region}, Tooltip: {clickArgs.TooltipText}");

        // Example 2: Process hover interaction
        var hoverArgs = interactionService.ProcessInteraction(
            chart,
            ChartInteractionType.Hover,
            pointerX: 250,
            pointerY: 280,
            canvasWidth: 800,
            canvasHeight: 600
        );
        Console.WriteLine($"Hover processed - Hit: {hoverArgs.Region}, Data point: {hoverArgs.HitDataPoint?.X},{hoverArgs.HitDataPoint?.Y}");

        // Example 3: Toggle selection on a data point
        bool selectionToggled = interactionService.ToggleSelection(
            chart,
            pointerX: 200,
            pointerY: 300,
            canvasWidth: 800,
            canvasHeight: 600
        );
        Console.WriteLine($"Selection toggled: {selectionToggled}");

        // Example 4: Get current selection state
        var selection = interactionService.GetSelection(chart);
        Console.WriteLine($"Current selection - Series count: {selection.Count}");
        foreach (var seriesSelection in selection)
        {
            Console.WriteLine($"  {seriesSelection.Key}: {seriesSelection.Value.Count} points");
        }

        // Example 5: Clear all selections
        interactionService.ClearSelection(chart);
        Console.WriteLine("Selection cleared");

        // Example 6: Process async interaction
        var asyncTask = interactionService.ProcessInteractionAsync(
            chart,
            ChartInteractionType.Click,
            pointerX: 300,
            pointerY: 250,
            canvasWidth: 800,
            canvasHeight: 600
        );
        var asyncArgs = asyncTask.Result;
        Console.WriteLine($"Async interaction completed - Hit: {asyncArgs.Region}");
    }
}

// Subscribe to events to handle interactions
public class ChartInteractionHandler
{
    private readonly IChartInteractionService _interactionService;

    public ChartInteractionHandler(IChartInteractionService interactionService)
    {
        _interactionService = interactionService;
        _interactionService.Clicked += OnChartClicked;
        _interactionService.Hovered += OnChartHovered;
        _interactionService.SelectionChanged += OnSelectionChanged;
    }

    private void OnChartClicked(object sender, ChartInteractionEventArgs e)
    {
        var chart = (Chart)sender!;
        Console.WriteLine($"[Clicked] Chart: {chart.Id}, Region: {e.Region}");
    }

    private void OnChartHovered(object sender, ChartInteractionEventArgs e)
    {
        var chart = (Chart)sender!;
        Console.WriteLine($"[Hovered] Chart: {chart.Id}, Tooltip: {e.TooltipText}");
    }

    private void OnSelectionChanged(object sender, ChartSelectionChangedEventArgs e)
    {
        var chart = (Chart)sender!;
        Console.WriteLine($"[SelectionChanged] Chart: {chart.Id}, Points: {e.SelectedPoints.Count}");
    }
}
```

## ChartTransitionEngine

`ChartTransitionEngine` is the production implementation of `IChartTransitionEngine` that handles animated transitions between chart states. It renders smooth animations by interpolating between chart configurations and data points across multiple frames, producing PNG-encoded frames that can be combined into video or displayed sequentially.

The engine supports both simple two-chart transitions and complex multi-keyframe animations through `TransitionTimeline`. It handles frame scheduling, easing functions, parallel rendering for performance, and comprehensive error handling with detailed logging.

```csharp
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SkiaSharpChartEngine.Animation;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Services;

public class ChartTransitionEngineExample
{
    public static async Task Main(string[] args)
    {
        // Initialize services
        var logger = new NullLogger<ChartTransitionEngine>();
        var renderer = new ChartRenderingService();
        var transitionEngine = new ChartTransitionEngine(renderer, logger);

        // Example 1: Create a simple transition between two charts
        var chartA = new Chart("initial-chart")
        {
            Title = "Starting Chart",
            Configuration = new ChartConfiguration
            {
                Width = 800,
                Height = 600,
                BackgroundColor = "#FFFFFF"
            }
        };
        
        var seriesA = new ChartSeries("Revenue")
        {
            LineWidth = 2.5f,
            Color = "#2E86C1"
        };
        seriesA.AddDataPoint(1.0, 100000.0);
        seriesA.AddDataPoint(2.0, 125000.0);
        seriesA.AddDataPoint(3.0, 150000.0);
        chartA.AddSeries(seriesA);

        var chartB = new Chart("final-chart")
        {
            Title = "Final Chart",
            Configuration = new ChartConfiguration
            {
                Width = 800,
                Height = 600,
                BackgroundColor = "#FFFFFF"
            }
        };
        
        var seriesB = new ChartSeries("Revenue")
        {
            LineWidth = 2.5f,
            Color = "#E74C3C" // Changed color for transition effect
        };
        seriesB.AddDataPoint(1.0, 150000.0);
        seriesB.AddDataPoint(2.0, 180000.0);
        seriesB.AddDataPoint(3.0, 210000.0);
        chartB.AddSeries(seriesB);

        // Render transition with default settings
        var transitionResult = await transitionEngine.RenderTransitionAsync(
            chartA, 
            chartB, 
            settings: new AnimationSettings { DurationMs = 1000, EasingType = EasingFunction.EaseOutBack },
            options: new TransitionOptions { FrameRate = 30, Quality = 90 }
        );

        if (transitionResult.Success)
        {
            Console.WriteLine($"Transition rendered successfully!");
            Console.WriteLine($"Duration: {transitionResult.TotalDurationMs}ms");
            Console.WriteLine($"Frames: {transitionResult.Frames.Count}");
            Console.WriteLine($"Metadata: {transitionResult.Metadata.Count} items");
            
            // Save frames to files (in real app, you'd combine them into video)
            for (int i = 0; i < transitionResult.Frames.Count; i++)
            {
                string framePath = $"./transition-frames/frame-{i:D4}.png";
                await File.WriteAllBytesAsync(framePath, transitionResult.Frames[i].ImageData);
            }
        }
        else
        {
            Console.WriteLine($"Transition failed: {transitionResult.ErrorMessage}");
        }

        // Example 2: Create a transition timeline with multiple keyframes
        var timeline = transitionEngine.CreateTimeline();
        timeline.StartWith(chartA)
                .AppendTransition(chartB, durationMs: 500, TransitionEasing.Spring)
                .AppendTransition(chartA, durationMs: 500, TransitionEasing.EaseOutBounce);

        var multiFrameResult = await transitionEngine.RenderTransitionAsync(timeline);
        Console.WriteLine($"Multi-frame timeline rendered: {multiFrameResult.Frames.Count} frames");

        // Example 3: Render a single frame at specific progress
        byte[] singleFrame = await transitionEngine.RenderFrameAsync(
            chartA, 
            chartB, 
            progress: 0.5, // 50% through the transition
            easing: TransitionEasing.EaseInOutElastic
        );
        Console.WriteLine($"Single frame rendered: {singleFrame.Length} bytes");
    }
}
```

## TransitionServiceExtensions

`TransitionServiceExtensions` provides extension methods for configuring the SkiaSharp Chart Engine's animated transitions subsystem and creating transition timelines directly from chart instances. It offers both dependency injection registration methods for setting up the transition engine in ASP.NET Core applications, and fluent API methods for building chart transition animations inline.

The extension methods allow you to:

- Register the `IChartTransitionEngine` and default `TransitionOptions` with the DI container
- Configure transition settings during registration
- Create transition timelines from charts using fluent syntax
- Bridge between legacy animation settings and the new transition engine

```csharp
using System;
using Microsoft.Extensions.DependencyInjection;
using SkiaSharpChartEngine;
using SkiaSharpChartEngine.Animation;
using SkiaSharpChartEngine.Models;

public class TransitionServiceExtensionsExample
{
    public static void ConfigureServices(IServiceCollection services)
    {
        // Example 1: Register transitions with default options
        services.AddChartTransitions();

        // Example 2: Register transitions with custom configuration
        services.AddChartTransitions(options =>
        {
            options.FrameRate = 60;
            options.Quality = 100;
            options.Playback = PlaybackMode.Loop;
        });

        // Example 3: Register the complete chart engine with transitions in one call
        services.AddSkiaSharpChartEngineWithTransitions(
            configureEngine: engineOptions =>
            {
                engineOptions.DefaultChartWidth = 1920;
                engineOptions.DefaultChartHeight = 1080;
            },
            configureTransitions: transitionOptions =>
            {
                transitionOptions.FrameRate = 30;
                transitionOptions.EnableParallelRendering = true;
            }
        );
    }

    public static void CreateTransitionAnimations()
    {
        // Example 4: Create a transition timeline from a chart
        var chartA = new Chart("initial-chart")
        {
            Title = "Initial State"
        };

        var timeline = chartA.BeginTransition()
            .AppendTransition(
                new Chart("final-chart") { Title = "Final State" },
                durationMs: 500,
                TransitionEasing.EaseOutBack
            )
            .AppendTransition(
                new Chart("intermediate-chart") { Title = "Intermediate State" },
                durationMs: 400,
                TransitionEasing.Spring
            );

        Console.WriteLine($"Created timeline with {timeline.TotalDurationMs}ms duration");

        // Example 5: Create a two-keyframe transition between charts
        var chart1 = new Chart("chart-1")
        {
            Title = "Chart 1",
            Configuration = new ChartConfiguration
            {
                AnimationDurationMs = 300
            }
        };

        var chart2 = new Chart("chart-2")
        {
            Title = "Chart 2",
            Configuration = new ChartConfiguration
            {
                AnimationDurationMs = 300
            }
        };

        var transition = chart1.TransitionTo(chart2, durationMs: 600, TransitionEasing.EaseInOutElastic);
        Console.WriteLine($"Created transition: {transition.TotalDurationMs}ms with elastic easing");

        // Example 6: Create timeline using chart's animation duration
        var autoTimeline = chart1.ToAnimatedTimeline(chart2, TransitionEasing.EaseOutBounce);
        Console.WriteLine($"Auto-duration timeline: {autoTimeline.TotalDurationMs}ms using chart config");

        // Example 7: Bridge legacy animation settings to transition options
        var legacySettings = new AnimationSettings
        {
            FrameRate = 24,
            Quality = 85
        };

        var transitionOptions = legacySettings.ToTransitionOptions();
        Console.WriteLine($"Bridged options: {transitionOptions.FrameRate}fps, quality {transitionOptions.Quality}");
    }
}
```

## ChartProcessingWorker

`ChartProcessingWorker` is a background worker that processes chart rendering jobs asynchronously without blocking main application threads. It maintains a queue of chart jobs, processes them with configurable concurrency, and provides comprehensive job tracking with callbacks for completion and error handling. The worker is ideal for batch processing, web API scenarios, or any application requiring non-blocking chart rendering operations.

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Workers;

public class ChartProcessingWorkerExample
{
    public static async Task Main(string[] args)
    {
        // Initialize logger and worker with 4 concurrent processors
        var logger = new NullLogger<ChartProcessingWorker>();
        using var worker = new ChartProcessingWorker(logger, concurrencyLevel: 4);

        Console.WriteLine("Worker started with concurrency level: " + worker.GetStatistics().ConcurrencyLevel);

        // Example 1: Create a chart processing job
        var chartJob = new ChartProcessingJob
        {
            ChartTitle = "Quarterly Revenue Report",
            ChartId = "q2-revenue-2024",
            Parameters = new Dictionary<string, object>
            {
                ["width"] = 1920,
                ["height"] = 1080,
                ["format"] = "PNG"
            },
            ProcessingDelegate = async (job, cancellationToken) =>
            {
                // Simulate chart rendering work
                await Task.Delay(1000, cancellationToken);
                job.Result = new { ImageSize = "1920x1080", Format = "PNG" };
            },
            OnComplete = job => 
            {
                Console.WriteLine($"Job completed: {job.JobId}");
                Console.WriteLine($"Duration: {job.GetDuration()?.TotalSeconds:F2}s");
                Console.WriteLine($"Result: {job.Result}");
            },
            OnError = (job, ex) =>
            {
                Console.WriteLine($"Job failed: {ex.Message}");
            }
        };

        // Example 2: Enqueue the job and get job ID
        string jobId = worker.EnqueueJob(chartJob);
        Console.WriteLine($"Job enqueued with ID: {jobId}");

        // Example 3: Monitor job statistics
        var stats = worker.GetStatistics();
        Console.WriteLine($"Pending jobs: {stats.PendingJobs}");
        Console.WriteLine($"Worker running: {stats.IsRunning}");

        // Example 4: Wait for job completion (in real app, use proper async handling)
        await Task.Delay(2000);

        // Example 5: Gracefully shutdown when done
        await worker.StopAsync();
        Console.WriteLine("Worker stopped gracefully");
    }
}
```

## MetricsAggregatorWorker

`MetricsAggregatorWorker` is a background worker that aggregates performance metrics at regular intervals. It collects metric values, calculates statistics (count, sum, average, min, max, median, standard deviation), detects anomalies, and raises events for monitoring and alerting purposes. The worker runs on a configurable interval (default: 5 minutes) and maintains comprehensive statistics for all recorded metrics.

```csharp
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SkiaSharpChartEngine.Workers;

public class MetricsAggregatorWorkerExample
{
    public static void Main()
    {
        // Initialize logger
        var logger = new NullLogger<MetricsAggregatorWorker>();
        
        // Create metrics aggregator worker with default 5-minute interval
        using var metricsAggregator = new MetricsAggregatorWorker(logger);
        
        // Subscribe to metrics aggregated events
        metricsAggregator.MetricsAggregated += (sender, e) =>
        {
            Console.WriteLine($"Metrics aggregated at {e.AggregatedAt}");
            Console.WriteLine($"Total metrics: {e.Statistics.Count}");
            Console.WriteLine($"Anomalies detected: {e.Anomalies.Count}");
            
            foreach (var stat in e.Statistics.Values)
            {
                Console.WriteLine($"\nMetric: {stat.Name}");
                Console.WriteLine($"  Count: {stat.Count}");
                Console.WriteLine($"  Sum: {stat.Sum:F2}");
                Console.WriteLine($"  Average: {stat.Average:F2}");
                Console.WriteLine($"  Min: {stat.Min:F2}");
                Console.WriteLine($"  Max: {stat.Max:F2}");
                Console.WriteLine($"  Median: {stat.Median:F2}");
                Console.WriteLine($"  StdDev: {stat.StdDev:F2}");
                Console.WriteLine($"  Tags: {stat.Tags}");
            }
        };
        
        // Start the worker
        metricsAggregator.Start();
        Console.WriteLine("Metrics aggregator worker started...");
        
        // Record some metrics
        metricsAggregator.RecordMetric("ResponseTime", 125.5, "api=userservice");
        metricsAggregator.RecordMetric("ResponseTime", 135.2, "api=userservice");
        metricsAggregator.RecordMetric("ResponseTime", 118.7, "api=userservice");
        metricsAggregator.RecordMetric("ResponseTime", 142.3, "api=userservice");
        metricsAggregator.RecordMetric("MemoryUsage", 156.8, "type=heap");
        metricsAggregator.RecordMetric("MemoryUsage", 162.1, "type=heap");
        metricsAggregator.RecordMetric("MemoryUsage", 159.4, "type=heap");
        
        // Get individual metric statistics
        var responseTimeStats = metricsAggregator.GetMetricStatistics("ResponseTime");
        Console.WriteLine($"\nResponseTime stats: Count={responseTimeStats?.Count}, Avg={responseTimeStats?.Average:F2}ms");
        
        // Get all metrics
        var allMetrics = metricsAggregator.GetAllMetrics();
        Console.WriteLine($"\nTotal metrics tracked: {allMetrics.Count}");
        
        // Get metrics count
        Console.WriteLine($"Metrics count: {metricsAggregator.GetMetricsCount()}");
        
        // Wait for aggregation cycle
        Console.WriteLine("Waiting for aggregation cycle...");
        System.Threading.Thread.Sleep(10000);
        
        // Stop the worker
        metricsAggregator.Stop();
        Console.WriteLine("Metrics aggregator worker stopped");
    }
}
```

## CacheCleanupWorker

`CacheCleanupWorker` is a background worker that maintains cache health by periodically cleaning up expired entries and managing memory usage. It automatically removes expired cache entries, monitors cache utilization, and removes low-priority entries when cache usage exceeds 90% to prevent memory pressure. The worker runs on a configurable interval (default: 10 minutes) and provides comprehensive statistics about cleanup operations.

```csharp
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SkiaSharpChartEngine.Workers;
using SkiaSharpChartEngine.Caching;

public class CacheCleanupWorkerExample
{
    public static void Main()
    {
        // Initialize logger and cache service
        var logger = new NullLogger<CacheCleanupWorker>();
        var cacheService = new DistributedCacheService(logger, maxSizeBytes: 10 * 1024 * 1024); // 10MB cache
        
        // Create cache cleanup worker with default 10-minute interval
        using var cleanupWorker = new CacheCleanupWorker(logger, cacheService);
        
        Console.WriteLine("Cache cleanup worker started...");
        
        // Example 1: Get current cleanup statistics
        var stats = cleanupWorker.GetStatistics();
        Console.WriteLine($"Initial stats: {stats}");
        Console.WriteLine($"Current cache size: {stats.CurrentCacheSize} bytes");
        Console.WriteLine($"Cache utilization: {stats.CacheUtilizationPercentage:F1}%");
        
        // Example 2: Manually trigger a cleanup cycle
        cleanupWorker.TriggerCleanup();
        
        // Example 3: Wait and check updated statistics
        System.Threading.Thread.Sleep(5000); // Wait for cleanup to complete
        
        var updatedStats = cleanupWorker.GetStatistics();
        Console.WriteLine($"\nUpdated stats after cleanup: {updatedStats}");
        Console.WriteLine($"Total cleanups performed: {updatedStats.TotalCleanups}");
        Console.WriteLine($"Total entries removed: {updatedStats.TotalEntriesRemoved}");
        
        // Example 4: Monitor cache statistics over time
        for (int i = 0; i < 3; i++)
        {
            var currentStats = cleanupWorker.GetStatistics();
            Console.WriteLine($"\nCache state {i + 1}: {currentStats.CurrentCacheSize}/{currentStats.MaxCacheSize} bytes ({currentStats.CacheUtilizationPercentage:F1}%)");
            System.Threading.Thread.Sleep(2000);
        }
    }
}
```

## DistributedCacheService

`DistributedCacheService` is an in-memory distributed cache service designed for chart rendering operations. It provides thread-safe caching with support for expiration policies, sliding expiration, priority-based eviction, and comprehensive monitoring. The service automatically cleans up expired entries and maintains cache statistics to help monitor memory usage and performance.


```csharp
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SkiaSharpChartEngine.Caching;
using SkiaSharpChartEngine.Models;

public class DistributedCacheServiceExample
{
public static void Main()
{
// Initialize logger and cache service with 100MB maximum size
var logger = new NullLogger<DistributedCacheService>();
var cacheService = new DistributedCacheService(logger, maxSizeBytes: 100_000_000);

Console.WriteLine("Distributed cache service initialized");

// Example 1: Set a cache entry with default policy
var chartData = new Chart("sales-chart")
{
Title = "Quarterly Sales Report",
Configuration = new ChartConfiguration { Width = 1920, Height = 1080 }
};

cacheService.Set("chart-sales-2024", chartData);
Console.WriteLine("Cache entry set: chart-sales-2024");

// Example 2: Set a cache entry with custom expiration policy
var chartWithExpiration = new Chart("expense-chart");
var policy = new CachePolicy
{
AbsoluteExpiration = DateTime.UtcNow.AddHours(1),
SlidingExpiration = TimeSpan.FromMinutes(30),
Priority = CachePriority.High
};

cacheService.Set("chart-expenses-2024", chartWithExpiration, policy);
Console.WriteLine("Cache entry set with custom policy: chart-expenses-2024");

// Example 3: Try to get a cached entry
if (cacheService.TryGet<Chart>("chart-sales-2024", out var cachedChart))
{
Console.WriteLine($"Cache hit! Retrieved chart: {cachedChart.Title}");
Console.WriteLine($"Chart ID: {cachedChart.Id}");
Console.WriteLine($"Chart type: {cachedChart.Type}");
}
else
{
Console.WriteLine("Cache miss - entry not found or expired");
}

// Example 4: Remove a specific cache entry
cacheService.Remove("chart-sales-2024");
Console.WriteLine("Cache entry removed: chart-sales-2024");

// Example 5: Remove all entries matching a pattern
int removedCount = cacheService.RemovePattern("chart-*");
Console.WriteLine($"Removed {removedCount} entries matching pattern 'chart-*'");

// Example 6: Clear all cache entries
cacheService.Clear();
Console.WriteLine("All cache entries cleared");

// Example 7: Get cache statistics
var stats = cacheService.GetStatistics();
Console.WriteLine($"\nCache Statistics:");
Console.WriteLine($"  Entries: {stats.EntryCount}");
Console.WriteLine($"  Size: {stats.SizeInBytes:N0} bytes");
Console.WriteLine($"  Max size: {stats.MaxSizeInBytes:N0} bytes");
Console.WriteLine($"  Utilization: {stats.UtilizationPercentage:F1}%");

// Example 8: Get metadata for all entries
var metadataList = cacheService.GetMetadata();
Console.WriteLine($"\nMetadata for {metadataList.Count} entries:");
foreach (var metadata in metadataList)
{
Console.WriteLine($"  Key: {metadata.Key}");
Console.WriteLine($"    Type: {metadata.ValueType}");
Console.WriteLine($"    Created: {metadata.CreatedAt}");
Console.WriteLine($"    Last accessed: {metadata.LastAccessedAt}");
Console.WriteLine($"    Priority: {metadata.Priority}");
Console.WriteLine($"    Size: {metadata.SizeInBytes} bytes");
}

// Example 9: Set multiple entries with different priorities
cacheService.Set("chart-priority-low", new Chart("low-priority"), 
new CachePolicy { Priority = CachePriority.Low });
cacheService.Set("chart-priority-normal", new Chart("normal-priority"),
new CachePolicy { Priority = CachePriority.Normal });
cacheService.Set("chart-priority-high", new Chart("high-priority"),
new CachePolicy { Priority = CachePriority.High });
Console.WriteLine("Set entries with different priorities");

// Example 10: Dispose the cache service (automatically cleans up resources)
cacheService.Dispose();
Console.WriteLine("Cache service disposed");
}
}
```

## TransitionOptions

`TransitionOptions` configures how chart transitions are rendered, controlling frame rate, quality, playback behavior, parallel rendering, and diagnostics. All settings are validated on property setters, and instances can be cloned to create modified copies without altering the original.

The options include playback modes (`PlaybackMode.Once`, `Loop`, `Bounce`), frame rate and quality settings, pre/post delays, parallel rendering controls, and frame timing collection for performance profiling.

```csharp
using System;
using SkiaSharpChartEngine.Animation;
using SkiaSharpChartEngine.Models;

public class TransitionOptionsExample
{
    public static void Main()
    {
        // Example 1: Create default transition options
        var defaultOptions = new TransitionOptions();
        Console.WriteLine(defaultOptions);
        
        // Example 2: Configure high-quality rendering with looping playback
        var highQualityOptions = new TransitionOptions
        {
            FrameRate = 60,
            Quality = 100,
            Playback = PlaybackMode.Loop,
            LoopCount = -1, // Infinite looping
            PreDelayMs = 500, // 500ms delay before starting
            PostDelayMs = 1000 // 1s delay after completion
        };
        
        // Example 3: Enable parallel rendering for performance
        var parallelOptions = new TransitionOptions
        {
            EnableParallelRendering = true,
            MaxParallelism = 8,
            CollectFrameTimings = true // Enable performance profiling
        };
        
        // Example 4: Clone and modify existing options
        var clonedOptions = defaultOptions.Clone();
        clonedOptions.FrameRate = 30;
        clonedOptions.Quality = 90;
        
        // Example 5: Use predefined presets
        var previewOptions = TransitionOptions.Preview;
        var loopingOptions = TransitionOptions.Looping;
        var highQualityPreset = TransitionOptions.HighQuality;
        
        // Example 6: Validate options before use
        try
        {
            highQualityOptions.Validate();
            Console.WriteLine("Options validated successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Validation failed: {ex.Message}");
        }
        
        // Example 7: Calculate frame interval
        Console.WriteLine($"Frame interval at 60fps: {highQualityOptions.FrameIntervalMs:F2}ms");
        Console.WriteLine($"Frame interval at 30fps: {previewOptions.FrameIntervalMs:F2}ms");
    }
}
```

```csharp
using System;
using SkiaSharpChartEngine.Animation;
using SkiaSharpChartEngine.Models;

public class TransitionTimelineExample
{
    public static void Main()
    {
        // Example 1: Create a simple two-state transition
        var chartA = new Chart("chart-a")
        {
            Title = "Initial State"
        };
        var chartB = new Chart("chart-b")
        {
            Title = "Final State"
        };
        
        var simpleTransition = TransitionTimeline.Between(chartA, chartB, durationMs: 600);
        Console.WriteLine(simpleTransition);
        
        // Example 2: Build a multi-step timeline with fluent API
        var multiStepTransition = new TransitionTimeline()
            .StartWith(chartA)
            .AppendTransition(chartB, durationMs: 400, TransitionEasing.EaseOutBack)
            .AppendTransition(chartA, durationMs: 600, TransitionEasing.Spring)
            .Repeat(3);
        
        Console.WriteLine($"Multi-step timeline: {multiStepTransition.TotalDurationMs}ms duration, {multiStepTransition.LoopCount} loops");
        
        // Example 3: Create timeline from steps
        var stepTransition = TransitionTimeline.FromSteps(
            TransitionEasing.EaseInOutCubic,
            (chartA, 300),
            (chartB, 500),
            (chartA, 300)
        );
        
        Console.WriteLine($"Step-based timeline: {stepTransition.TotalDurationMs}ms total duration");
        
        // Example 4: Add keyframe at specific time
        var chartC = new Chart("chart-c") { Title = "Intermediate State" };
        var timelineWithExplicitTime = new TransitionTimeline()
            .AddKeyframe(chartA, timeMs: 0)
            .AddKeyframe(chartC, timeMs: 250)
            .AddKeyframe(chartB, timeMs: 750);
        
        Console.WriteLine($"Explicit time timeline: {timelineWithExplicitTime.TotalDurationMs}ms duration");
        
        // Example 5: Enumerate segments
        foreach (var segment in timelineWithExplicitTime.GetSegments())
        {
            Console.WriteLine($"Segment: {segment.From.TimeMs}ms -> {segment.To.TimeMs}ms");
        }
        
        // Example 6: Validate timeline before rendering
        try
        {
            timelineWithExplicitTime.Validate();
            Console.WriteLine("Timeline is valid and ready for rendering");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Validation failed: {ex.Message}");
        }
    }
}
```

## TransitionEasingCalculator

`TransitionEasingCalculator` evaluates easing functions for chart transitions, providing smooth animation effects by transforming linear progress values into non-linear easing curves. It supports all CSS easing presets plus spring-physics and bounce variants, returning eased values that create natural acceleration and deceleration effects in chart animations.

The calculator handles both standard easing functions (quadratic, cubic, exponential) and advanced physics-based animations (spring, elastic, bounce), with the spring easing intentionally allowing transient values outside the [0, 1] range to create natural overshoot effects.

```csharp
using System;
using SkiaSharpChartEngine.Animation;
using SkiaSharpChartEngine.Models;

public class TransitionEasingCalculatorExample
{
    public static void Main()
    {
        // Example 1: Linear easing (constant velocity)
        double linearResult = TransitionEasingCalculator.Calculate(TransitionEasing.Linear, 0.5);
        Console.WriteLine($"Linear easing at t=0.5: {linearResult:F4}");
        // Output: Linear easing at t=0.5: 0.5000

        // Example 2: Ease-in quadratic (gradual acceleration)
        double easeInQuadResult = TransitionEasingCalculator.Calculate(TransitionEasing.EaseInQuad, 0.5);
        Console.WriteLine($"EaseInQuad at t=0.5: {easeInQuadResult:F4}");
        // Output: EaseInQuad at t=0.5: 0.2500

        // Example 3: Ease-out cubic (gradual deceleration)
        double easeOutCubicResult = TransitionEasingCalculator.Calculate(TransitionEasing.EaseOutCubic, 0.5);
        Console.WriteLine($"EaseOutCubic at t=0.5: {easeOutCubicResult:F4}");
        // Output: EaseOutCubic at t=0.5: 0.8750

        // Example 4: Spring easing (physically-accurate damped spring)
        double springResult = TransitionEasingCalculator.Calculate(TransitionEasing.Spring, 0.5);
        Console.WriteLine($"Spring easing at t=0.5: {springResult:F4}");
        // Output: Spring easing at t=0.5: 0.8415 (may vary slightly due to physics calculation)

        // Example 5: Ease-out back (overshoots slightly before settling)
        double easeOutBackResult = TransitionEasingCalculator.Calculate(TransitionEasing.EaseOutBack, 0.8);
        Console.WriteLine($"EaseOutBack at t=0.8: {easeOutBackResult:F4}");
        // Output: EaseOutBack at t=0.8: 1.0192 (overshoots beyond 1.0)

        // Example 6: Ease-out elastic (spring-like snap effect)
        double easeOutElasticResult = TransitionEasingCalculator.Calculate(TransitionEasing.EaseOutElastic, 0.7);
        Console.WriteLine($"EaseOutElastic at t=0.7: {easeOutElasticResult:F4}");
        // Output: EaseOutElastic at t=0.7: 0.9712

        // Example 7: Ease-out bounce (simulates ball bouncing to rest)
        double easeOutBounceResult = TransitionEasingCalculator.Calculate(TransitionEasing.EaseOutBounce, 0.3);
        Console.WriteLine($"EaseOutBounce at t=0.3: {easeOutBounceResult:F4}");
        // Output: EaseOutBounce at t=0.3: 0.2813

        // Example 8: Using easing with TransitionTimeline for smooth chart transitions
        var chart1 = new Chart("chart-1");
        var chart2 = new Chart("chart-2");
        
        var timeline = TransitionTimeline.Between(chart1, chart2, durationMs: 1000)
            .WithEasing(TransitionEasing.EaseInOutElastic);
        
        Console.WriteLine($"Timeline with elastic easing: {timeline.TotalDurationMs}ms duration");
        
        // Example 9: Create custom easing curve by calculating intermediate values
        Console.WriteLine("\nCustom easing curve progression:");
        for (double t = 0.0; t <= 1.0; t += 0.2)
        {
            double easedValue = TransitionEasingCalculator.Calculate(TransitionEasing.EaseInOutCubic, t);
            Console.WriteLine($"  t={t:F1} -> eased={easedValue:F3}");
        }
        // Output shows smooth cubic easing progression
    }
}
```

## CacheKeyBuilder

`CacheKeyBuilder` is a utility class for generating and parsing cache keys used by the SkiaSharp Chart Engine's caching system. It provides methods to build consistent, structured cache keys for various chart components including charts, series, configurations, axes, palettes, and render operations. The class also includes utilities for validating cache keys, extracting chart IDs, and building invalidation patterns for cache management.

```csharp
using System;
using SkiaSharpChartEngine.Caching;

public class CacheKeyBuilderExample
{
    public static void Main()
    {
        // Example 1: Build a cache key for a chart
        string chartKey = CacheKeyBuilder.BuildChartKey(
            chartId: "sales-dashboard-2024",
            chartType: "line",
            width: 1920,
            height: 1080
        );
        Console.WriteLine($"Chart cache key: {chartKey}");
        
        // Example 2: Build a cache key for chart rendering
        string renderKey = CacheKeyBuilder.BuildRenderKey(
            chartId: "sales-dashboard-2024",
            chartType: "line",
            configurationHash: "a1b2c3d4",
            dataHash: "e5f6g7h8"
        );
        Console.WriteLine($"Render cache key: {renderKey}");
        
        // Example 3: Build a cache key for a series
        string seriesKey = CacheKeyBuilder.BuildSeriesKey(
            chartId: "sales-dashboard-2024",
            seriesName: "Revenue",
            seriesType: "line",
            color: "#2E86C1"
        );
        Console.WriteLine($"Series cache key: {seriesKey}");
        
        // Example 4: Build a cache key for chart configuration
        string configKey = CacheKeyBuilder.BuildConfigurationKey(
            chartId: "sales-dashboard-2024",
            title: "Sales Dashboard",
            backgroundColor: "#FFFFFF",
            showGrid: true,
            showLegend: true
        );
        Console.WriteLine($"Configuration cache key: {configKey}");
        
        // Example 5: Build a cache key for an axis
        string axisKey = CacheKeyBuilder.BuildAxisKey(
            chartId: "sales-dashboard-2024",
            axisType: "x",
            label: "Quarter",
            min: 0,
            max: 4,
            showGridLines: true
        );
        Console.WriteLine($"Axis cache key: {axisKey}");
        
        // Example 6: Build a cache key for a color palette
        string paletteKey = CacheKeyBuilder.BuildPaletteKey(
            chartId: "sales-dashboard-2024",
            paletteType: "default",
            colorCount: 5
        );
        Console.WriteLine($"Palette cache key: {paletteKey}");
        
        // Example 7: Extract chart ID from a cache key
        string? extractedChartId = CacheKeyBuilder.ExtractChartIdFromKey(chartKey);
        Console.WriteLine($"Extracted chart ID: {extractedChartId}");
        
        // Example 8: Build an invalidation pattern for cache cleanup
        string invalidationPattern = CacheKeyBuilder.BuildInvalidationPattern(
            chartId: "sales-dashboard-*"
        );
        Console.WriteLine($"Invalidation pattern: {invalidationPattern}");
        
        // Example 9: Validate a cache key
        bool isValid = CacheKeyBuilder.IsValidCacheKey(chartKey);
        Console.WriteLine($"Is valid cache key: {isValid}");
        
        // Example 10: Build a composite key for complex caching scenarios
        string compositeKey = CacheKeyBuilder.BuildCompositeKey(
            "sales-dashboard-2024",
            "Revenue",
            "line",
            "#2E86C1",
            "a1b2c3d4"
        );
        Console.WriteLine($"Composite cache key: {compositeKey}");
    }
}
```

## ValidationRuleBuilder

`ValidationRuleBuilder` provides a fluent API for defining and applying validation rules to generic objects. It simplifies the process of creating complex validation logic by allowing developers to chain conditions like required checks, range constraints, and length validations.

### Example

```csharp
// Example: Using ValidationRuleBuilder to validate a string
// Assuming 'builder' is an instance of ValidationRuleBuilder<string>
builder.AddRule(s => !string.IsNullOrEmpty(s), "Cannot be null or empty")
       .IsRequired(s => s, "Username")
       .HasLength(s => s, 5, 10, "Username");

var result = builder.Validate("my-data");

if (!result.IsValid)
{
    foreach (var error in result.Errors)
    {
        Console.WriteLine(error);
    }
}

builder.ClearRules();
```

## ChartStreamingService

`ChartStreamingService` is the default implementation of `IChartStreamingService` that provides real-time data streaming capabilities for charts. It enables dynamic chart updates by buffering incoming data points and rendering frames at configurable intervals, making it ideal for live dashboards, monitoring applications, and real-time data visualization scenarios.

The service maintains a separate channel for each registered chart, allowing lock-free concurrent publishing of data points while efficiently managing memory usage through configurable buffer sizes and sliding window constraints. It supports both synchronous and asynchronous rendering modes, with automatic cleanup of old data points to prevent unbounded memory growth.

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Streaming;

public class ChartStreamingServiceExample
{
    public static async Task Main(string[] args)
    {
        // Initialize services
        var logger = new NullLogger<ChartStreamingService>();
        var renderingService = new ChartRenderingService();
        var streamingService = new ChartStreamingService(renderingService, logger);

        // Example 1: Register a chart for streaming
        var chart = new Chart("live-sales-chart")
        {
            Title = "Live Sales Dashboard",
            Configuration = new ChartConfiguration
            {
                Width = 1920,
                Height = 1080,
                BackgroundColor = "#FFFFFF"
            }
        };

        var streamingOptions = new StreamingChartOptions
        {
            MaxBufferSize = 1000, // Maximum buffered data points
            WindowSize = 500,      // Keep only last 500 data points
            FlushIntervalMs = 100,  // Render at 10fps
            ReplaceOnUpdate = false  // Append new data to existing series
        };

        streamingService.Register(chart, streamingOptions);
        Console.WriteLine("Chart registered for streaming");

        // Example 2: Publish data points to the chart
        var dataPoints = new List<StreamDataPoint>
        {
            new StreamDataPoint("Revenue", 1.0, 100000.0, "Q1"),
            new StreamDataPoint("Revenue", 2.0, 125000.0, "Q2"),
            new StreamDataPoint("Revenue", 3.0, 150000.0, "Q3"),
            new StreamDataPoint("Revenue", 4.0, 175000.0, "Q4"),
            new StreamDataPoint("Expenses", 1.0, 85000.0, "Q1"),
            new StreamDataPoint("Expenses", 2.0, 92000.0, "Q2"),
            new StreamDataPoint("Expenses", 3.0, 98000.0, "Q3"),
            new StreamDataPoint("Expenses", 4.0, 105000.0, "Q4")
        };

        int publishedCount = streamingService.PublishBatch("live-sales-chart", dataPoints);
        Console.WriteLine($"Published {publishedCount} data points");

        // Example 3: Get current snapshot of the chart
        var snapshot = streamingService.GetSnapshot("live-sales-chart");
        Console.WriteLine($"Snapshot retrieved: {snapshot.Series.Count} series, {snapshot.GetTotalDataPointCount()} total data points");

        // Example 4: Flush buffered data and render a frame
        int flushedCount = await streamingService.FlushAsync("live-sales-chart");
        Console.WriteLine($"Flushed {flushedCount} buffered data points");

        // Example 5: Stream frames asynchronously (simulated with timeout)
        Console.WriteLine("Starting frame streaming...");
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        
        await foreach (var frame in streamingService.RenderFramesAsync("live-sales-chart", cts.Token))
        {
            Console.WriteLine($"Frame {frame.FrameNumber}: {frame.ImageData.Length} bytes, rendered in {frame.RenderTimeMs}ms");
            
            // In a real application, you would:
            // - Save the frame to disk
            // - Send it over a network connection
            // - Display it in a UI element
            // - Combine frames into a video
            break; // Only process one frame for this example
        }

        // Example 6: Unregister chart when done
        streamingService.Unregister("live-sales-chart");
        Console.WriteLine("Chart unregistered from streaming");
    }
}
```

## StreamingChartOptions

`StreamingChartOptions` provides configuration for real-time chart data streaming operations. It controls buffering behavior, sliding window sizing, auto-render intervals, and series update strategies to optimize performance for streaming scenarios.

The options include maximum buffer size to prevent memory pressure, sliding window size for limiting historical data retention, flush interval for controlling render frequency, and a replace-on-update flag to control whether new data replaces existing series or appends to it.

```csharp
using System;
using SkiaSharpChartEngine.Streaming;

public class StreamingChartOptionsExample
{
    public static void Main()
    {
        // Example 1: Create default streaming options
        var defaultOptions = new StreamingChartOptions();
        Console.WriteLine($"Default buffer size: {defaultOptions.MaxBufferSize}");
        Console.WriteLine($"Default window size: {defaultOptions.WindowSize}");
        Console.WriteLine($"Default flush interval: {defaultOptions.FlushIntervalMs}ms");
        Console.WriteLine($"Replace on update: {defaultOptions.ReplaceOnUpdate}");

        // Example 2: Configure high-performance streaming with large buffer
        var highPerfOptions = new StreamingChartOptions
        {
            MaxBufferSize = 5000,           // Allow up to 5000 buffered items
            WindowSize = 1000,               // Keep only last 1000 data points per series
            FlushIntervalMs = 50,            // Render at 20fps
            ReplaceOnUpdate = false           // Append new data to existing series
        };

        // Example 3: Configure for real-time monitoring with windowing
        var monitoringOptions = new StreamingChartOptions
        {
            MaxBufferSize = 2000,
            WindowSize = 500,                // Show only recent 500 points
            FlushIntervalMs = 100,           // Render at 10fps
            ReplaceOnUpdate = true            // Replace entire series on each update
        };

        // Example 4: Validate options before use
        try
        {
            var testOptions = new StreamingChartOptions { MaxBufferSize = 100 };
            testOptions.Validate(); // Throws if invalid
            Console.WriteLine("Options validated successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Validation failed: {ex.Message}");
        }

        // Example 5: Clone and modify existing options
        var clonedOptions = defaultOptions with { WindowSize = 250 };
        Console.WriteLine($"Cloned options window size: {clonedOptions.WindowSize}");
    }
}
```

## ChartEventPublisher

`ChartEventPublisher` implements the publish-subscribe pattern for chart events in the SkiaSharp chart engine. It allows components to subscribe to various chart events (creation, update, deletion, rendering, export, and errors) and notifies all registered subscribers when these events occur. The publisher provides thread-safe subscription management and asynchronous event broadcasting with comprehensive logging.

```csharp
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SkiaSharpChartEngine.Events;

public class ChartEventPublisherExample
{
    public static async Task Main(string[] args)
    {
        // Initialize event publisher with logger
        var logger = new NullLogger<ChartEventPublisher>();
        var eventPublisher = new ChartEventPublisher(logger);

        // Check initial subscriber count
        Console.WriteLine($"Initial subscriber count: {eventPublisher.GetSubscriberCount()}");

        // Create a subscriber implementation
        var chartLogger = new ChartEventLoggerSubscriber();

        // Subscribe to events
        eventPublisher.Subscribe(chartLogger);
        Console.WriteLine($"After subscribe - Subscriber count: {eventPublisher.GetSubscriberCount()}");

        // Publish various chart events
        await eventPublisher.PublishChartCreatedAsync(
            new ChartCreatedEvent(Guid.NewGuid(), DateTime.UtcNow, "line-chart-001"));

        await eventPublisher.PublishChartUpdatedAsync(
            new ChartUpdatedEvent(Guid.NewGuid(), DateTime.UtcNow, "line-chart-001", "Updated title"));

        await eventPublisher.PublishChartRenderedAsync(
            new ChartRenderedEvent(Guid.NewGuid(), DateTime.UtcNow, "line-chart-001", 800, 600));

        await eventPublisher.PublishChartExportedAsync(
            new ChartExportedEvent(Guid.NewGuid(), DateTime.UtcNow, "line-chart-001", "chart.png"));

        await eventPublisher.PublishErrorAsync(
            new ChartErrorEvent(Guid.NewGuid(), DateTime.UtcNow, "Failed to render chart", new Exception("Rendering error")));

        // Unsubscribe
        eventPublisher.Unsubscribe(chartLogger);
        Console.WriteLine($"After unsubscribe - Subscriber count: {eventPublisher.GetSubscriberCount()}");
    }
}

// Example subscriber implementation
public class ChartEventLoggerSubscriber : IChartEventSubscriber
{
    public async Task OnChartCreatedAsync(ChartCreatedEvent @event)
    {
        Console.WriteLine($"[ChartCreated] Chart: {@event.ChartId}, Timestamp: {@event.Timestamp}");
        await Task.CompletedTask;
    }

    public async Task OnChartUpdatedAsync(ChartUpdatedEvent @event)
    {
        Console.WriteLine($"[ChartUpdated] Chart: {@event.ChartId}, Title: {@event.NewTitle}");
        await Task.CompletedTask;
    }

    public async Task OnChartDeletedAsync(ChartDeletedEvent @event)
    {
        Console.WriteLine($"[ChartDeleted] Chart: {@event.ChartId}");
        await Task.CompletedTask;
    }

    public async Task OnChartRenderedAsync(ChartRenderedEvent @event)
    {
        Console.WriteLine($"[ChartRendered] Chart: {@event.ChartId}, Size: {@event.Width}x{@event.Height}");
        await Task.CompletedTask;
    }

    public async Task OnChartExportedAsync(ChartExportedEvent @event)
    {
        Console.WriteLine($"[ChartExported] Chart: {@event.ChartId}, File: {@event.FilePath}");
        await Task.CompletedTask;
    }

    public async Task OnErrorAsync(ChartErrorEvent @event)
    {
        Console.WriteLine($"[Error] Chart: {@event.ChartId}, Message: {@event.Message}");
        await Task.CompletedTask;
    }
}
```

## EventDispatcher

`EventDispatcher` is a central event dispatcher implementing the publish-subscribe pattern. It manages event subscriptions and dispatches events to all registered handlers, supporting both synchronous and asynchronous event handling. The dispatcher provides thread-safe subscription management, comprehensive logging, and utilities for inspecting the current subscription state.

```csharp
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SkiaSharpChartEngine.Events;

public class EventDispatcherExample
{
    public static async Task Main(string[] args)
    {
        // Initialize dispatcher with logger
        var logger = new NullLogger<EventDispatcher>();
        var dispatcher = new EventDispatcher(logger);

        // Check initial handler count
        Console.WriteLine($"Initial handler count for 'chart.created': {dispatcher.GetHandlerCount("chart.created")}");

        // Create handlers
        var syncHandler = new SyncEventHandler();
        var asyncHandler = new AsyncEventHandler();

        // Subscribe handlers
        dispatcher.Subscribe("chart.created", syncHandler);
        dispatcher.Subscribe("chart.created", asyncHandler);
        dispatcher.Subscribe("chart.updated", syncHandler);

        Console.WriteLine($"After subscribe - 'chart.created' handlers: {dispatcher.GetHandlerCount("chart.created")}");
        Console.WriteLine($"Subscribed event types: {string.Join(", ", dispatcher.GetSubscribedEventTypes())}");

        // Dispatch synchronous event
        dispatcher.Dispatch("chart.created", new { ChartId = "line-chart-001", Timestamp = DateTime.UtcNow });

        // Dispatch asynchronous event
        await dispatcher.DispatchAsync("chart.updated", new { 
            ChartId = "bar-chart-002", 
            Timestamp = DateTime.UtcNow,
            Changes = new[] { "Title", "DataPoints" }
        });

        // Unsubscribe
        dispatcher.Unsubscribe("chart.created", syncHandler);
        Console.WriteLine($"After unsubscribe - 'chart.created' handlers: {dispatcher.GetHandlerCount("chart.created")}");

        // Clear all handlers
        dispatcher.Clear();
        Console.WriteLine($"After clear - 'chart.updated' handlers: {dispatcher.GetHandlerCount("chart.updated")}");
    }
}

// Synchronous event handler implementation
public class SyncEventHandler : IEventHandler
{
    public void Handle(string eventType, object eventData)
    {
        Console.WriteLine($"[Sync] Received {eventType}: {System.Text.Json.JsonSerializer.Serialize(eventData)}");
    }
}

// Asynchronous event handler implementation
public class AsyncEventHandler : IAsyncEventHandler
{
    public async Task HandleAsync(string eventType, object eventData)
    {
        Console.WriteLine($"[Async] Processing {eventType}...");
        await Task.Delay(100); // Simulate async work
        Console.WriteLine($"[Async] Completed {eventType}");
    }
    
    public void Handle(string eventType, object eventData)
    {
        Console.WriteLine($"[Async] Sync fallback for {eventType}");
    }
}
```

## IChartEventSubscriber

`IChartEventSubscriber` is an interface that defines the contract for receiving chart events from the `ChartEventPublisher`. It provides a standardized way for components to react to various chart lifecycle events such as creation, updates, deletions, rendering, exports, and errors. Implementations of this interface can be registered with the event publisher to receive asynchronous notifications about chart events.

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SkiaSharpChartEngine.Events;

public class CustomChartEventSubscriber : IChartEventSubscriber
{
    public string EventId { get; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; } = DateTime.UtcNow;
    public string? SourceName { get; set; } = "CustomChartMonitor";
    public Dictionary<string, object> Metadata { get; } = new Dictionary<string, object>();

    public virtual string GetEventName()
    {
        return "CustomChartEvent";
    }

    public Task OnChartCreatedAsync(ChartCreatedEvent @event)
    {
        Console.WriteLine($"[CustomSubscriber] Chart created: {@event.ChartId}");
        Console.WriteLine($"  Event ID: {EventId}");
        Console.WriteLine($"  Timestamp: {Timestamp}");
        Console.WriteLine($"  Source: {SourceName}");
        
        Metadata["chartType"] = @event.ChartId.Contains("line") ? "line" : "other";
        Metadata["eventSource"] = SourceName;
        
        return Task.CompletedTask;
    }

    public Task OnChartUpdatedAsync(ChartUpdatedEvent @event)
    {
        Console.WriteLine($"[CustomSubscriber] Chart updated: {@event.ChartId}");
        Console.WriteLine($"  New title: {@event.NewTitle}");
        Console.WriteLine($"  Modified fields: {string.Join(", ", @event.ModifiedFields ?? Array.Empty<string>())}");
        return Task.CompletedTask;
    }

    public Task OnChartDeletedAsync(ChartDeletedEvent @event)
    {
        Console.WriteLine($"[CustomSubscriber] Chart deleted: {@event.ChartId}");
        Console.WriteLine($"  Deleted by: {@event.DeletedBy}");
        return Task.CompletedTask;
    }

    public Task OnChartRenderedAsync(ChartRenderedEvent @event)
    {
        Console.WriteLine($"[CustomSubscriber] Chart rendered: {@event.ChartId}");
        Console.WriteLine($"  Dimensions: {@event.Width}x{@event.Height}");
        Console.WriteLine($"  Previous update: {@event.PreviousUpdateTime?.ToString("o") ?? "never"}");
        return Task.CompletedTask;
    }

    public Task OnChartExportedAsync(ChartExportedEvent @event)
    {
        Console.WriteLine($"[CustomSubscriber] Chart exported: {@event.ChartId}");
        Console.WriteLine($"  File path: {@event.FilePath}");
        return Task.CompletedTask;
    }

    public Task OnErrorAsync(ChartErrorEvent @event)
    {
        Console.WriteLine($"[CustomSubscriber] Chart error: {@event.ChartId}");
        Console.WriteLine($"  Error message: {@event.Message}");
        return Task.CompletedTask;
    }
}

// Usage example
public class Program
{
    public static async Task Main(string[] args)
    {
        // Create a custom subscriber
        var subscriber = new CustomChartEventSubscriber();
        
        Console.WriteLine($"Subscriber created with EventId: {subscriber.EventId}");
        Console.WriteLine($"Supported events: ChartCreated, ChartUpdated, ChartDeleted, ChartRendered, ChartExported, Error");
        
        // Simulate receiving events
        var createdEvent = new ChartCreatedEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            "line-chart-001"
        );
        
        await subscriber.OnChartCreatedAsync(createdEvent);
        
        var updatedEvent = new ChartUpdatedEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            "bar-chart-002",
            "Updated Bar Chart",
            new[] { "Title", "DataPoints" },
            DateTime.UtcNow.AddMinutes(-5)
        );
        
        await subscriber.OnChartUpdatedAsync(updatedEvent);
        
        var renderedEvent = new ChartRenderedEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            "pie-chart-003",
            1024,
            768
        );
        
        await subscriber.OnChartRenderedAsync(renderedEvent);
    }
}
```

## StringFormatBenchmarks

`StringFormatBenchmarks` is a benchmark suite that measures the performance of common string formatting operations used throughout the SkiaSharp chart engine. It focuses on hot paths for string formatting including camelCase/snake_case conversion, CSV generation, string repetition, and number formatting with units.

The benchmarks cover:
- **CamelCaseToTitleCase**: Converts camelCase strings to Title Case using `string.Create`
- **SnakeCaseToTitleCase**: Converts snake_case strings to Title Case using `string.Create`
- **ToCsvLine**: Generates CSV lines from arrays of values using pooled `StringBuilder`
- **Repeat**: Repeats strings multiple times efficiently
- **FormatNumberWithUnits**: Formats numbers with appropriate units (thousands, millions, billions)
- **FormatPercentage**: Formats numbers as percentages with specified decimal places

## MathHelperBenchmarks

`MathHelperBenchmarks` is a benchmark suite that measures the performance of mathematical operations used throughout the SkiaSharp chart engine. It focuses on hot paths for statistical calculations including min/max finding, averaging, and standard deviation computation. The benchmarks compare traditional `IEnumerable<T>`-based approaches against zero-allocation `ReadOnlySpan<T>`-based implementations to demonstrate performance improvements.

The benchmarks cover:
- **GetMinMax**: Finds minimum and maximum values in a dataset
- **Average**: Computes the arithmetic mean of values
- **StandardDeviation**: Calculates the standard deviation (measure of data dispersion)

```csharp
using System;
using System.Collections.Generic;
using SkiaSharpChartEngine.Benchmarks;
using SkiaSharpChartEngine.Utilities;

public class MathHelperBenchmarksExample
{
    public static void Main()
    {
        // Create sample data for analysis
        var random = new Random(42);
        var dataPoints = new float[1000];
        for (int i = 0; i < dataPoints.Length; i++)
        {
            dataPoints[i] = (float)(random.NextDouble() * 1000.0);
        }

        // Get min/max values using ReadOnlySpan (recommended for performance)
        var (min, max) = MathHelper.GetMinMax(dataPoints.AsSpan());
        Console.WriteLine($"Min: {min:F2}, Max: {max:F2}");
        // Example output: Min: 0.02, Max: 999.98

        // Calculate average using ReadOnlySpan
        float average = MathHelper.Average(dataPoints.AsSpan());
        Console.WriteLine($"Average: {average:F2}");
        // Example output: Average: 499.50

        // Calculate standard deviation using ReadOnlySpan
        float stdDev = MathHelper.StandardDeviation(dataPoints.AsSpan());
        Console.WriteLine($"Standard Deviation: {stdDev:F2}");
        // Example output: Standard Deviation: 288.67

        // For comparison: using IEnumerable (less efficient)
        var dataEnumerable = (IEnumerable<float>)dataPoints;
        var (minEnum, maxEnum) = MathHelper.GetMinMax(dataEnumerable);
        float averageEnum = MathHelper.Average(dataEnumerable);
        float stdDevEnum = MathHelper.StandardDeviation(dataEnumerable);
        
        Console.WriteLine($"\nIEnumerable results - Min: {minEnum:F2}, Max: {maxEnum:F2}, Avg: {averageEnum:F2}, StdDev: {stdDevEnum:F2}");
    }
}
```

## RenderCacheService

`RenderCacheService` is an in-memory cache service that stores rendered chart images to improve performance by avoiding redundant rendering operations. It maintains cache entries with image data, creation timestamps, and access counts, implementing a least-recently-used (LRU) eviction policy when the cache reaches its maximum size.

The cache is particularly useful for web applications where the same chart configurations are requested multiple times, reducing server load and improving response times for repeated requests.

```csharp
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SkiaSharpChartEngine.Services;

public class RenderCacheServiceExample
{
    public static void Main()
    {
        // Initialize cache service with logger
        var logger = new NullLogger<RenderCacheService>();
        var cacheService = new RenderCacheService(logger, maxCacheSize: 100);

        // Example 1: Check cache size and contents
        int cacheSize = cacheService.GetCacheSize();
        Console.WriteLine($"Initial cache size: {cacheSize}");
        Console.WriteLine($"Cache contains 'chart-123': {cacheService.Contains("chart-123")}");

        // Example 2: Add an entry to cache
        byte[] chartImageData = new byte[] { 0x89, 0x50, 0x4E, 0x47 }; // PNG header
        cacheService.Set("chart-123", chartImageData);
        Console.WriteLine("Added chart image to cache");

        // Example 3: Retrieve cached data
        byte[]? cachedData = cacheService.Get("chart-123");
        Console.WriteLine(cachedData != null
            ? $"Retrieved cached data: {cachedData.Length} bytes"
            : "Cache miss");

        // Example 4: List all cache keys
        var allKeys = cacheService.GetAllKeys();
        Console.WriteLine($"All cache keys: {string.Join(", ", allKeys)}");

        // Example 5: Remove a specific entry
        cacheService.Remove("chart-123");
        Console.WriteLine("Removed chart-123 from cache");

        // Example 6: Clear entire cache
        cacheService.Clear();
        Console.WriteLine($"Cache cleared. Size: {cacheService.GetCacheSize()}");
    }
}
```

## ReportSection

`ReportSection` represents a single section within a PDF report that can contain an optional heading, descriptive body text, and a chart visualization. Each section supports configurable layout options like image scaling behavior and automatic page breaks, making it easy to create multi-section reports with mixed text and chart content.

```csharp
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Reports;

// Create a simple text-only section
var textSection = new ReportSection
{
    Heading = "Executive Summary",
    BodyText = "This report contains quarterly performance metrics and key insights."
};

// Create a chart section with automatic page break
var chart = new Chart("quarterly-sales");
var series = new ChartSeries("Revenue")
{
    LineWidth = 2.5f,
    Color = "#2E86C1"
};
series.AddDataPoint(1.0, 100000.0);
series.AddDataPoint(2.0, 125000.0);
series.AddDataPoint(3.0, 150000.0);
series.AddDataPoint(4.0, 175000.0);
chart.AddSeries(series);

var chartSection = new ReportSection
{
    Heading = "Quarterly Revenue Analysis",
    BodyText = "Sales performance across all regions for Q2 2024.",
    Chart = chart,
    ImageFit = PdfImageFit.FitWidth,
    PageBreakBefore = true
};

// Create a section with original image size (may clip if too large)
var fullSizeSection = new ReportSection
{
    Heading = "High Resolution Chart",
    Chart = chart,
    ImageFit = PdfImageFit.Original
};
```

## ColorPalette

`ColorPalette` is a utility class for managing and cycling through color schemes for chart series. It provides predefined palettes for common charting scenarios (default, vibrant, pastel, monochrome, ocean) and allows custom palette creation. The palette automatically wraps around when accessing colors beyond its length, making it ideal for cycling through series colors in multi-series charts.

```csharp
using System;
using SkiaSharpChartEngine.Models;

public class ColorPaletteExample
{
    public static void Main()
    {
        // Example 1: Create a custom color palette
        var customPalette = new ColorPalette("Custom", new[] { "#FF5733", "#33FF57", "#3357FF", "#F3FF33" });
        Console.WriteLine($"Custom palette created with {customPalette.GetColorCount()} colors");
        
        // Example 2: Add colors dynamically
        customPalette.AddColor("#FF33F3");
        customPalette.AddColor("#33FFF3");
        Console.WriteLine($"Added 2 more colors, total: {customPalette.GetColorCount()}");
        
        // Example 3: Get colors by index (wraps around automatically)
        Console.WriteLine($"Color at index 0: {customPalette.GetColorAtIndex(0)}");
        Console.WriteLine($"Color at index 5: {customPalette.GetColorAtIndex(5)}"); // Wraps to index 0
        
        // Example 4: Use predefined palettes
        var defaultPalette = ColorPalette.CreateDefaultPalette();
        Console.WriteLine($"Default palette: {defaultPalette.Name} with {defaultPalette.GetColorCount()} colors");
        
        var vibrantPalette = ColorPalette.CreateVibrantPalette();
        Console.WriteLine($"Vibrant palette: {vibrantPalette.Name} with {vibrantPalette.GetColorCount()} colors");
        
        var pastelPalette = ColorPalette.CreatePastelPalette();
        Console.WriteLine($"Pastel palette: {pastelPalette.Name} with {pastelPalette.GetColorCount()} colors");
        
        var monochromePalette = ColorPalette.CreateMonochromePalette();
        Console.WriteLine($"Monochrome palette: {monochromePalette.Name} with {monochromePalette.GetColorCount()} colors");
        
        var oceanPalette = ColorPalette.CreateOceanPalette();
        Console.WriteLine($"Ocean palette: {oceanPalette.Name} with {oceanPalette.GetColorCount()} colors");
        
        // Example 5: Cycle through colors using color index
        int colorIndex = 0;
        Console.WriteLine("Cycling through colors:");
        for (int i = 0; i < 8; i++)
        {
            Console.WriteLine($"  Series {i + 1}: {customPalette.GetNextColor(ref colorIndex)}");
        }
        
        // Example 6: Access all colors
        Console.WriteLine($"\nAll colors in custom palette:");
        foreach (var color in customPalette.Colors)
        {
            Console.WriteLine($"  {color}");
        }
        
        // Example 7: ToString representation
        Console.WriteLine($"\nPalette info: {customPalette}");
    }
}
```

## CliOptions

`CliOptions` defines the configuration parameters for the SkiaSharp Chart Engine CLI. It aggregates command-line arguments, facilitating the management of input/output paths, rendering dimensions, export formats, caching strategies, and performance monitoring.

```csharp
using System;
using SkiaSharpChartEngine.Configuration;

public class CliOptionsExample
{
    public static void Main(string[] args)
    {
        // Parse command line arguments
        var options = CliOptions.Parse(args);

        if (options.ShowHelp)
        {
            CliOptions.DisplayHelp();
            return;
        }

        if (!options.IsValid)
        {
            Console.WriteLine("Invalid CLI configuration provided.");
            return;
        }

        // Access parsed options
        Console.WriteLine($"Input file: {options.InputFile}");
        Console.WriteLine($"Output directory: {options.OutputDirectory}");
        Console.WriteLine($"Dimensions: {options.Width}x{options.Height}");
        Console.WriteLine($"Enable cache: {options.EnableCache}");
        Console.WriteLine($"Cache size: {options.GetCacheSizeBytes} bytes");

        var formats = options.GetExportFormats;
        Console.WriteLine($"Export formats: {string.Join(", ", formats)}");
    }
}
```

## QuickStartExample

`QuickStartExample` provides a collection of practical examples demonstrating common usage patterns for the SkiaSharp chart engine. It showcases how to create various chart types, configure chart properties, render charts to images, export charts to files, validate chart configurations, work with color palettes, and perform asynchronous rendering operations.

```csharp
using System;
using System.Threading.Tasks;
using SkiaSharpChartEngine;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Utilities;

public class QuickStartExampleUsage
{
    public static void Main()
    {
        // Example 1: Create a simple line chart
        QuickStartExample.CreateSimpleLineChart();
        
        // Example 2: Create a multi-series chart with custom configuration
        QuickStartExample.CreateMultiSeriesChart();
        
        // Example 3: Export a chart to a file
        QuickStartExample.ExportChartToFile();
        
        // Example 4: Validate a chart before rendering
        QuickStartExample.ValidateChartBeforeRendering();
        
        // Example 5: Generate and visualize different color palettes
        QuickStartExample.GenerateAndVisualizePaletteColors();
        
        // Example 6: Perform asynchronous chart rendering
        QuickStartExample.RenderChartAsync().GetAwaiter().GetResult();
    }
}
```

```csharp
using System;
using SkiaSharpChartEngine.Benchmarks;
using SkiaSharpChartEngine.Utilities;

public class StringFormatBenchmarksExample
{
    public static void Main()
    {
        // CamelCaseToTitleCase example
        string camelCase = "salesPerformanceMonthlyDashboard";
        string titleCase = StringFormatHelper.CamelCaseToTitleCase(camelCase);
        Console.WriteLine($"CamelCase: {camelCase}");
        Console.WriteLine($"TitleCase: {titleCase}");
        // Output: TitleCase: Sales Performance Monthly Dashboard

        // SnakeCaseToTitleCase example
        string snakeCase = "sales_performance_monthly_dashboard";
        string titleCase2 = StringFormatHelper.SnakeCaseToTitleCase(snakeCase);
        Console.WriteLine($"SnakeCase: {snakeCase}");
        Console.WriteLine($"TitleCase: {titleCase2}");
        // Output: TitleCase: Sales Performance Monthly Dashboard

        // ToCsvLine example - format data for CSV export
        var csvValues = new object[] { "Monthly Revenue", 1234567.89, "North America", true, null };
        string csvLine = StringFormatHelper.ToCsvLine(csvValues);
        Console.WriteLine($"CSV Line: {csvLine}");
        // Output: CSV Line: "Monthly Revenue",1234567.89,North America,True,

        // Repeat example - create separator lines
        string separator = StringFormatHelper.Repeat("-", 50);
        Console.WriteLine(separator);
        // Output: --------------------------------------------------

        // FormatNumberWithUnits examples
        string formattedBillions = StringFormatHelper.FormatNumberWithUnits(1234567890);
        Console.WriteLine($"Billions: {formattedBillions}");
        // Output: Billions: 1.23B

        string formattedThousands = StringFormatHelper.FormatNumberWithUnits(4567.5);
        Console.WriteLine($"Thousands: {formattedThousands}");
        // Output: Thousands: 4.57K

        // FormatPercentage example
        string percentage = StringFormatHelper.FormatPercentage(73.456, 2);
        Console.WriteLine($"Percentage: {percentage}");
        // Output: Percentage: 73.46%
    }
}
```

## CacheKeyBenchmarks

`CacheKeyBenchmarks` is a benchmark suite that measures the performance of cache key generation operations used throughout the SkiaSharp chart engine. It focuses on hot paths for cache key assembly including SHA-256 hashing, frozen-dictionary lookups, and composite key construction.

The benchmarks cover:
- **BuildRenderKey**: Generates a render cache key for chart images with dimensions and format
- **BuildSeriesKey**: Creates a cache key for chart series based on chart and series names
- **BuildAxisKey**: Builds a cache key for axis configuration with min/max values and tick count
- **BuildConfigurationKey_Line**: Generates configuration keys for line charts using frozen dictionary lookups
- **BuildConfigurationKey_Bar**: Generates configuration keys for bar charts using frozen dictionary lookups
- **BuildCompositeKey**: Assembles composite cache keys from multiple parameters (no LINQ)

```csharp
using System;
using SkiaSharpChartEngine.Benchmarks;
using SkiaSharpChartEngine.Constants;

public class CacheKeyBenchmarksExample
{
    public static void Main()
    {
        // BuildRenderKey example - generate a key for a 1920x1080 PNG chart
        string renderKey = CacheKeyBuilder.BuildRenderKey(
            chartId: "sales-dashboard-2024",
            width: 1920,
            height: 1080,
            scale: 1.0f,
            format: "PNG"
        );
        Console.WriteLine($"Render key: {renderKey}");
        // Example output: Render key: 9a3b4c5d6e7f8g9h0i1j2k3l4m5n6o7p8

        // BuildSeriesKey example - create a key for a specific series in a chart
        string seriesKey = CacheKeyBuilder.BuildSeriesKey(
            chartId: "quarterly-revenue-chart",
            seriesName: "Q3 2024 Sales Performance"
        );
        Console.WriteLine($"Series key: {seriesKey}");
        // Example output: Series key: 2p3q4r5s6t7u8v9w0x1y2z3a4b5c6d7e8

        // BuildAxisKey example - generate a key for axis configuration
        string axisKey = CacheKeyBuilder.BuildAxisKey(
            minValue: 0f,
            maxValue: 1000f,
            tickCount: 10
        );
        Console.WriteLine($"Axis key: {axisKey}");
        // Example output: Axis key: 3a4b5c6d7e8f9g0h1i2j3k4l5m6n7

        // BuildConfigurationKey for LineChart - get configuration key from ChartType enum
        string lineConfigKey = CacheKeyBuilder.BuildConfigurationKey(ChartType.LineChart);
        Console.WriteLine($"Line chart config key: {lineConfigKey}");
        // Example output: Line chart config key: 4b5c6d7e8f9g0h1i2j3k4l5m6n7o8

        // BuildConfigurationKey for BarChart - get configuration key from ChartType enum
        string barConfigKey = CacheKeyBuilder.BuildConfigurationKey(ChartType.BarChart);
        Console.WriteLine($"Bar chart config key: {barConfigKey}");
        // Example output: Bar chart config key: 5c6d7e8f9g0h1i2j3k4l5m6n7o8p9

        // BuildCompositeKey example - assemble a composite key from multiple parameters
        string compositeKey = CacheKeyBuilder.BuildCompositeKey(
            chartId: "monthly-report-2024",
            width: 800,
            height: 600,
            format: "PNG",
            version: "v2.1.0"
        );
        Console.WriteLine($"Composite key: {compositeKey}");
        // Example output: Composite key: 6d7e8f9g0h1i2j3k4l5m6n7o8p9q0
    }
}
```

## PdfReportGeneratorTests

`PdfReportGeneratorTests` provides a comprehensive suite of unit tests for the `PdfReportGenerator` class, which generates PDF reports containing chart visualizations and formatted text content. The tests validate PDF generation with various section configurations, error handling for null inputs, and proper interaction with the chart rendering service.

The test suite covers:
- **Null argument validation**: Ensuring `GenerateAsync` and `GenerateToFileAsync` throw `ArgumentNullException` for invalid inputs
- **Empty sections handling**: Testing PDF generation with empty section lists
- **Text-only reports**: Validating PDF creation with only text content (headings and body text)
- **Chart integration**: Verifying chart rendering service is called for chart sections and multiple charts produce correct output
- **Error resilience**: Ensuring PDF generation succeeds even when chart rendering fails
- **File system operations**: Testing successful file writing with `GenerateToFileAsync`

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Reports;
using SkiaSharpChartEngine.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

public class PdfReportGeneratorTestsExample
{
    public static async Task Main()
    {
        // Initialize PDF report generator with dependencies
        var logger = new NullLogger<PdfReportGenerator>();
        var renderingService = new ChartRenderingService(
            logger,
            new ChartDataService(logger),
            new RenderCacheService()
        );
        var pdfGenerator = new PdfReportGenerator(renderingService, logger);

        // Example 1: Generate a PDF report with text-only sections
        var textSections = new List<ReportSection>
        {
            new ReportSection
            {
                Heading = "Executive Summary",
                BodyText = "This report contains quarterly performance metrics and analysis."
            },
            new ReportSection
            {
                Heading = "Key Findings",
                BodyText = "Revenue increased by 15% compared to last quarter. Customer satisfaction reached record high."
            }
        };

        var pdfBytes = await pdfGenerator.GenerateAsync(textSections);
        Console.WriteLine($"Generated PDF with text sections: {pdfBytes.Length} bytes");

        // Example 2: Generate a PDF report with chart sections
        var chart = new Chart("quarterly-sales");
        var salesSeries = new ChartSeries("Sales Revenue");
        salesSeries.AddDataPoint(1.0, 100000.0);
        salesSeries.AddDataPoint(2.0, 125000.0);
        salesSeries.AddDataPoint(3.0, 150000.0);
        salesSeries.AddDataPoint(4.0, 175000.0);
        chart.AddSeries(salesSeries);

        var chartSections = new List<ReportSection>
        {
            new ReportSection
            {
                Heading = "Quarterly Revenue Analysis",
                BodyText = "Sales performance across all regions.",
                Chart = chart
            },
            new ReportSection
            {
                Heading = "Market Share Distribution",
                BodyText = "Regional breakdown of market share.",
                Chart = CreatePieChart(),
                PageBreakBefore = true
            }
        };

        var chartPdfBytes = await pdfGenerator.GenerateAsync(chartSections);
        Console.WriteLine($"Generated PDF with chart sections: {chartPdfBytes.Length} bytes");

        // Example 3: Generate and save PDF to file
        var outputPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            $"report-{DateTime.Now:yyyyMMdd-HHmmss}.pdf"
        );

        await pdfGenerator.GenerateToFileAsync(outputPath, chartSections);
        Console.WriteLine($"PDF saved to: {outputPath}");
        Console.WriteLine($"File exists: {File.Exists(outputPath)}");

        // Cleanup
        if (File.Exists(outputPath))
        {
            File.Delete(outputPath);
        }
    }

    private static Chart CreatePieChart()
    {
        var chart = new Chart("market-share");
        var series = new ChartSeries("Market Share");
        series.AddDataPoint(1.0, 35.0);
        series.AddDataPoint(2.0, 25.0);
        series.AddDataPoint(3.0, 20.0);
        series.AddDataPoint(4.0, 20.0);
        chart.AddSeries(series);
        return chart;
    }
}
```

## MathHelperTests

`MathHelperTests` is a comprehensive unit test suite that validates the mathematical utility methods in the `MathHelper` class. These utilities provide essential chart rendering calculations including data normalization, statistical analysis, and value clamping operations. The tests cover edge cases, boundary conditions, and ensure mathematical correctness for chart visualization scenarios.

The test suite covers:
- **GetMinMax**: Validates minimum and maximum value extraction from collections
- **Normalize**: Tests normalization of values to [0, 1] range with special handling for equal bounds
- **Lerp**: Verifies linear interpolation with boundary clamping
- **StandardDeviation**: Ensures correct statistical calculations for data dispersion
- **GenerateAxisTicks**: Validates axis tick generation for chart rendering
- **Clamp**: Tests value clamping to specified minimum and maximum bounds

```csharp
using System;
using System.Collections.Generic;
using SkiaSharpChartEngine.Utilities;

public class MathHelperTestsExample
{
    public static void Main()
    {
        // Example 1: GetMinMax with a collection of values
        var values = new List<double> { 10.5, 25.3, 15.7, 30.1, 8.2 };
        var (min, max) = MathHelper.GetMinMax(values);
        Console.WriteLine($"Min: {min:F2}, Max: {max:F2}");
        // Output: Min: 8.20, Max: 30.10

        // Example 2: Normalize values to [0, 1] range
        var dataPoints = new List<(double X, double Y)>
        {
            (1.0, 100.0),
            (2.0, 200.0),
            (3.0, 150.0)
        };
        
        // Normalize X values
        MathHelper.Normalize(dataPoints, p => p.X);
        Console.WriteLine($"Normalized X - Min: {dataPoints[0].X:F4}, Max: {dataPoints[2].X:F4}");
        // Output: Normalized X - Min: 0.0000, Max: 1.0000
        
        // Normalize Y values
        MathHelper.Normalize(dataPoints, p => p.Y);
        Console.WriteLine($"Normalized Y - Min: {dataPoints[0].Y:F4}, Max: {dataPoints[1].Y:F4}");
        // Output: Normalized Y - Min: 0.0000, Max: 1.0000

        // Example 3: Linear interpolation (Lerp) with boundary clamping
        double start = 10.0;
        double end = 50.0;
        
        // Interpolate at midpoint (t = 0.5)
        double midpoint = MathHelper.Lerp(start, end, 0.5);
        Console.WriteLine($"Lerp midpoint: {midpoint:F2}");
        // Output: Lerp midpoint: 30.00
        
        // Interpolate with t > 1 (clamps to end value)
        double clamped = MathHelper.Lerp(start, end, 1.5);
        Console.WriteLine($"Lerp with t > 1: {clamped:F2}");
        // Output: Lerp with t > 1: 50.00
        
        // Interpolate with t = 0 (returns start value)
        double startValue = MathHelper.Lerp(start, end, 0.0);
        Console.WriteLine($"Lerp with t = 0: {startValue:F2}");
        // Output: Lerp with t = 0: 10.00

        // Example 4: Calculate standard deviation for statistical analysis
        var measurements = new List<double> { 9.8, 10.2, 9.9, 10.1, 10.0 };
        double stdDev = MathHelper.StandardDeviation(measurements);
        Console.WriteLine($"Standard deviation: {stdDev:F4}");
        // Output: Standard deviation: 0.1414

        // Example 5: Generate axis ticks for chart rendering
        var ticks = MathHelper.GenerateAxisTicks(0.0, 100.0, 11);
        Console.WriteLine($"Generated {ticks.Count} axis ticks");
        Console.WriteLine($"First tick: {ticks[0]:F2}, Last tick: {ticks[^1]:F2}");
        // Output: Generated 11 axis ticks
        // Output: First tick: 0.00, Last tick: 100.00

        // Example 6: Clamp values to ensure they stay within valid ranges
        double valueAboveMax = 150.0;
        double clampedValue = MathHelper.Clamp(valueAboveMax, 0.0, 100.0);
        Console.WriteLine($"Clamped value above max: {clampedValue:F2}");
        // Output: Clamped value above max: 100.00
        
        double valueBelowMin = -10.0;
        double clampedValue2 = MathHelper.Clamp(valueBelowMin, 0.0, 100.0);
        Console.WriteLine($"Clamped value below min: {clampedValue2:F2}");
        // Output: Clamped value below min: 0.00
    }
}
```

## HttpChartClient

`HttpChartClient` is an HTTP client for communicating with remote chart services. It provides methods for fetching charts, posting chart data, and downloading rendered chart images from a remote chart service endpoint. The client handles authentication headers, request serialization, and comprehensive error logging.

```csharp
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SkiaSharpChartEngine.Integration;
using SkiaSharpChartEngine.Models;

public class HttpChartClientExample
{
    public static async Task Main(string[] args)
    {
        // Initialize HttpChartClient with base URL and logger
        var logger = new NullLogger<HttpChartClient>();
        var serializer = new JsonChartSerializer();
        var httpChartClient = new HttpChartClient(
            "https://api.chart-service.example.com",
            logger,
            serializer
        );

        // Set authentication token for API access
        httpChartClient.SetAuthenticationToken("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...");

        // Set custom headers if needed
        httpChartClient.SetDefaultHeaders("X-API-Version", "v2.1");
        httpChartClient.SetDefaultHeaders("X-Request-ID", Guid.NewGuid().ToString());

        try
        {
            // Example 1: Fetch a chart by ID
            var chart = await httpChartClient.GetChartAsync("sales-dashboard-2024");
            Console.WriteLine(chart != null
                ? $"Fetched chart: {chart.Id} - {chart.Title}"
                : "Chart not found");

            // Example 2: Post a new chart to remote service
            var newChart = new Chart("monthly-report-q2")
            {
                Title = "Q2 2024 Monthly Report",
                Description = "Monthly sales performance report"
            };
            newChart.AddSeries(new ChartSeries("Revenue")
            {
                LineWidth = 2.5f,
                Color = "#2E86C1"
            });
            
            bool posted = await httpChartClient.PostChartAsync(newChart);
            Console.WriteLine($"Chart posted successfully: {posted}");

            // Example 3: Download rendered chart image
            var chartImageData = await httpChartClient.GetRenderedChartAsync(
                "sales-dashboard-2024",
                format: "png"
            );
            
            if (chartImageData != null)
            {
                Console.WriteLine($"Downloaded chart image: {chartImageData.Length} bytes");
                // Save to file or process the image data
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Chart service error: {ex.Message}");
        }
        finally
        {
            // Cleanup
            httpChartClient.Dispose();
        }
    }
}
```

## ExternalApiClient

`ExternalApiClient` is an HTTP client for integrating with external APIs. It provides a convenient wrapper around `HttpClient` with built-in retry logic, error handling, request/response serialization, and header management. The client supports common HTTP methods (GET, POST, PUT, DELETE) and automatically handles JSON serialization/deserialization.

```csharp
using System;
using System.Threading.Tasks;
using SkiaSharpChartEngine.Integration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

public class ExternalApiClientExample
{
    public static async Task Main(string[] args)
    {
        // Initialize HttpClient and logger
        var httpClient = new System.Net.Http.HttpClient();
        var logger = new NullLogger<ExternalApiClient>();
        
        // Create API client with 3 retries and 1 second base delay
        var apiClient = new ExternalApiClient(httpClient, logger, maxRetries: 3);
        
        // Set authorization header (e.g., Bearer token)
        apiClient.SetAuthorizationHeader("Bearer", "your-access-token-here");
        
        // Add custom headers
        apiClient.AddHeader("X-Custom-Header", "custom-value");
        apiClient.AddHeader("Accept", "application/json");
        
        try
        {
            // Example 1: GET request to fetch data
            var weatherData = await apiClient.GetAsync<WeatherResponse>(
                "https://api.weather.com/v1/current?location=NewYork"
            );
            Console.WriteLine($"Temperature: {weatherData?.Temperature}°C");
            
            // Example 2: POST request to create a resource
            var newChart = new { Name = "Sales Dashboard", Type = "line" };
            var createdChart = await apiClient.PostAsync<Chart>(
                "https://api.example.com/charts",
                newChart
            );
            Console.WriteLine($"Created chart with ID: {createdChart?.Id}");
            
            // Example 3: PUT request to update a resource
            var updateData = new { Title = "Updated Sales Dashboard" };
            var updatedChart = await apiClient.PutAsync<Chart>(
                $"https://api.example.com/charts/{createdChart?.Id}",
                updateData
            );
            Console.WriteLine($"Updated chart: {updatedChart?.Name}");
            
            // Example 4: DELETE request to remove a resource
            bool deleted = await apiClient.DeleteAsync(
                $"https://api.example.com/charts/{createdChart?.Id}"
            );
            Console.WriteLine($"Resource deleted: {deleted}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"API request failed: {ex.Message}");
        }
    }
}

// Example response model
public class WeatherResponse
{
    public double Temperature { get; set; }
    public double Humidity { get; set; }
    public string Location { get; set; }
}

// Example chart model
public class Chart
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
}
```

## ChartConfiguration

`ChartConfiguration` is a configuration class that defines the visual appearance and layout of charts rendered by the SkiaSharp chart engine. It controls chart margins, colors, axis labels, grid visibility, legend display, and scaling options for both X and Y axes. This configuration allows for consistent styling across multiple charts and easy customization of chart aesthetics.

## ConfigurationService

`ConfigurationService` is a service for managing chart configurations and templates in the SkiaSharp chart engine. It provides methods for retrieving, creating, saving, deleting, and listing chart configurations, as well as creating configurations from predefined templates for different chart types.

The service maintains an in-memory dictionary of configurations and supports dependency injection for logging. It includes built-in validation and handles common operations like retrieving default configurations, cloning configurations to prevent mutation, and managing template-based configurations for line, bar, pie, heatmap, area, scatter, and column charts.

```csharp
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Services;

public class ConfigurationServiceExample
{
    public static void Main()
    {
        // Initialize ConfigurationService with logger
        var logger = new NullLogger<ConfigurationService>();
        var configService = new ConfigurationService(logger);

        // Example 1: Create a new configuration
        var lineChartConfig = configService.CreateConfiguration(ChartType.LineChart);
        Console.WriteLine($"Created line chart config: {lineChartConfig.Name}");

        // Example 2: Save configuration
        configService.SaveConfiguration(lineChartConfig);
        Console.WriteLine("Configuration saved successfully");

        // Example 3: Retrieve saved configuration
        var retrievedConfig = configService.GetConfiguration("LineChart");
        Console.WriteLine(retrievedConfig != null 
            ? $"Retrieved config: {retrievedConfig.Name}"
            : "Configuration not found");

        // Example 4: List all configurations
        var allConfigs = configService.ListConfigurations();
        Console.WriteLine($"Total configurations: {allConfigs.Count}");
    }
}
```

## ChartEngineOptions

`ChartEngineOptions` is a configuration class that defines runtime settings for the SkiaSharp chart engine. It controls caching behavior, rendering performance, default chart dimensions, background styling, data validation, and custom settings for extended functionality. This class is typically configured during application startup when setting up the chart engine services.

The options include performance tuning parameters like `MaxConcurrentRenders` and `CacheSize`, rendering quality settings such as `UseAntiAliasing`, and data handling options like `MaxDataPointsPerSeries` and `ValidateDataOnLoad`. The `Validate()` method performs validation on all configuration values to ensure they meet minimum requirements.

```csharp
using System;
using Microsoft.Extensions.DependencyInjection;
using SkiaSharpChartEngine;
using SkiaSharpChartEngine.Configuration;

public class ChartEngineOptionsExample
{
    public static void Main()
    {
        // Example 1: Configure ChartEngineOptions with default values
        var options = new ChartEngineOptions
        {
            CacheSize = 100,
            EnableCaching = true,
            EnableLogging = true,
            MaxConcurrentRenders = Environment.ProcessorCount,
            DefaultChartWidth = 1024,
            DefaultChartHeight = 768,
            DefaultBackgroundColor = "#FFFFFF",
            UseAntiAliasing = true,
            MaxDataPointsPerSeries = 1000,
            MaxSeriesPerChart = 10,
            CacheExpirationTime = TimeSpan.FromHours(2),
            ValidateDataOnLoad = true
        };

        // Validate configuration
        options.Validate();
        Console.WriteLine("Chart engine options validated successfully");

        // Example 2: Configure via dependency injection (ASP.NET Core scenario)
        var services = new ServiceCollection();
        
        services.AddSkiaSharpChartEngine(options =>
        {
            options.CacheSize = 200;
            options.EnableCaching = true;
            options.MaxConcurrentRenders = 8;
            options.DefaultChartWidth = 1920;
            options.DefaultChartHeight = 1080;
            options.UseAntiAliasing = true;
            options.MaxDataPointsPerSeries = 5000;
            options.MaxSeriesPerChart = 20;
            options.CacheExpirationTime = TimeSpan.FromHours(1);
            options.ValidateDataOnLoad = true;
            
            // Add custom settings for application-specific features
            options.CustomSettings = new Dictionary<string, object>
            {
                ["enableAnimations"] = true,
                ["defaultFont"] = "Arial",
                ["enableTooltips"] = true
            };
        });

        var serviceProvider = services.BuildServiceProvider();
        Console.WriteLine("Chart engine configured via DI container");

        // Example 3: Access cache duration via seconds property
        var cacheOptions = new ChartEngineOptions
        {
            CacheExpirationTime = TimeSpan.FromMinutes(30)
        };
        
        // CacheDurationSeconds provides convenient access to TotalSeconds
        Console.WriteLine($"Cache duration in seconds: {cacheOptions.CacheDurationSeconds}");
        
        // Example 4: Configure for high-performance scenario
        var highPerformanceOptions = new ChartEngineOptions
        {
            CacheSize = 500,
            MaxConcurrentRenders = Environment.ProcessorCount * 2,
            MaxDataPointsPerSeries = 10000,
            MaxSeriesPerChart = 50,
            EnableCaching = true,
            CacheExpirationTime = TimeSpan.FromMinutes(15),
            ValidateDataOnLoad = false // Skip validation for performance-critical scenarios
        };

        highPerformanceOptions.Validate();
        Console.WriteLine("High-performance configuration validated");
    }
}
```

## SkiasharpChartEngineOptions

`SkiasharpChartEngineOptions` is a configuration class that defines fundamental properties for the SkiaSharp chart rendering engine. It manages rendering behavior (such as antialiasing and default background colors), default chart dimensions, caching settings, and data series limits. Developers can customize these options to control both output quality and engine resource utilization.

```csharp
using System;
using SkiaSharpChartEngine.Configuration;

public class SkiasharpChartEngineOptionsExample
{
    public static void Main()
    {
        // Instantiating and configuring SkiasharpChartEngineOptions
        var options = new SkiasharpChartEngineOptions
        {
            CacheEnabled = "true",
            CacheDurationSeconds = 3600,
            MaxConcurrentRenders = 10,
            DefaultChartWidth = 800,
            DefaultChartHeight = 600,
            DefaultBackgroundColor = "#FFFFFF",
            UseAntiAliasing = true,
            MaxDataPointsPerSeries = 1000,
            MaxSeriesPerChart = 10,
            ValidateDataOnLoad = true
        };

        Console.WriteLine($"Default Chart Dimensions: {options.DefaultChartWidth}x{options.DefaultChartHeight}");
        Console.WriteLine($"Cache Enabled: {options.CacheEnabled}, Duration: {options.CacheDurationSeconds}s");
    }
}
```

## RenderPipelineService

`RenderPipelineService` orchestrates the chart rendering pipeline with validation, caching, and processing stages. It implements the pipeline pattern to provide flexible rendering workflows that can be extended with custom stages for data validation, transformation, and rendering operations.

The service manages a sequence of pipeline stages that process chart data sequentially, tracking execution metrics and providing detailed results for each stage. It's designed for scenarios requiring multi-step chart processing with comprehensive error handling and performance monitoring.

```csharp
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Services;

public class RenderPipelineServiceExample
{
    public static async Task Main(string[] args)
    {
        // Initialize RenderPipelineService with logger
        var logger = new NullLogger<RenderPipelineService>();
        var pipelineService = new RenderPipelineService(logger);

        // Create a sample chart
        var chart = new Chart("sales-chart")
        {
            Title = "Quarterly Sales Performance"
        };

        var series = new ChartSeries("Revenue")
        {
            LineWidth = 2.5f,
            Color = "#2E86C1"
        };
        series.AddDataPoint(1.0, 100000.0);
        series.AddDataPoint(2.0, 125000.0);
        series.AddDataPoint(3.0, 150000.0);
        series.AddDataPoint(4.0, 175000.0);
        chart.AddSeries(series);

        // Example 1: Add custom pipeline stages
        pipelineService.AddStage(new DataValidationStage());
        pipelineService.AddStage(new ChartTransformationStage());

        // Example 2: Execute the rendering pipeline
        var result = await pipelineService.ExecuteAsync(chart);

        // Example 3: Check pipeline execution results
        Console.WriteLine($"Pipeline completed: {result.Success}");
        Console.WriteLine($"Chart ID: {result.ChartId}");
        Console.WriteLine($"Total duration: {result.TotalElapsedMs}ms");
        Console.WriteLine($"Started at: {result.StartedAt}");
        Console.WriteLine($"Completed at: {result.CompletedAt}");

        if (!result.Success)
        {
            Console.WriteLine($"Error: {result.Error}");
        }

        // Example 4: Access stage-specific results
        foreach (var stageResult in result.StageResults)
        {
            Console.WriteLine($"Stage '{stageResult.StageName}': {stageResult.Success} in {stageResult.ElapsedMs}ms");
            if (!stageResult.Success && stageResult.Message != null)
            {
                Console.WriteLine($"  Error: {stageResult.Message}");
            }
        }

        // Example 5: Get all pipeline stages
        var stages = pipelineService.GetStages();
        Console.WriteLine($"Pipeline has {stages.Count} stages");

        // Example 6: Clear all stages
        pipelineService.Clear();
        Console.WriteLine("All pipeline stages cleared");
    }
}

// Example pipeline stage implementation
public class DataValidationStage : IPipelineStage
{
    public string Name => "DataValidation";

    public async Task<PipelineStageResult> ExecuteAsync(Chart chart)
    {
        // Validate chart has data
        if (chart.Series.Count == 0)
        {
            return PipelineStageResult.Failure("Chart has no series data");
        }

        // Validate each series has data points
        foreach (var series in chart.Series)
        {
            if (series.DataPoints.Count == 0)
            {
                return PipelineStageResult.Failure($"Series '{series.Name}' has no data points");
            }
        }

        return PipelineStageResult.Success("Data validated successfully", chart);
    }
}

// Example pipeline stage for chart transformations
public class ChartTransformationStage : IPipelineStage
{
    public string Name => "ChartTransformation";

    public async Task<PipelineStageResult> ExecuteAsync(Chart chart)
    {
        // Apply chart transformations (e.g., normalize data, apply styles)
        foreach (var series in chart.Series)
        {
            // Example: Set consistent colors
            if (string.IsNullOrEmpty(series.Color))
            {
                series.Color = "#2E86C1";
            }
        }

        return PipelineStageResult.Success("Chart transformed successfully", chart);
    }
}
```

## ChartConfiguration
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Services;

public class ChartRenderingServiceExample
{
    public static void Main()
    {
        // Initialize ChartRenderingService with required dependencies
        var logger = new NullLogger<ChartRenderingService>();
        var dataService = new ChartDataService(logger);
        var cacheService = new RenderCacheService(logger, maxCacheSize: 100);
        
        var renderingService = new ChartRenderingService(logger, dataService, cacheService);

        // Example 1: Render chart to byte array (async)
        var chart = new Chart("sales-chart")
        {
            Title = "Quarterly Sales Performance",
            Configuration = new ChartConfiguration
            {
                Width = 800,
                Height = 600,
                BackgroundColor = "#FFFFFF",
                ShowGrid = true,
                ShowLegend = true
            }
        };
        
        var chartSeries = new ChartSeries("Revenue")
        {
            LineWidth = 2.5f,
            Color = "#2E86C1"
        };
        chartSeries.AddDataPoint(1.0, 100000.0);
        chartSeries.AddDataPoint(2.0, 125000.0);
        chartSeries.AddDataPoint(3.0, 150000.0);
        chartSeries.AddDataPoint(4.0, 175000.0);
        chart.AddSeries(chartSeries);

        // Render to byte array asynchronously
        var renderResult = await renderingService.RenderToByteArrayAsync(chart);
        
        if (renderResult.Success)
        {
            Console.WriteLine($"Chart rendered successfully: {renderResult.ImageData.Length} bytes");
            Console.WriteLine($"Render time: {renderResult.RenderTimeMs}ms");
        }

        // Example 2: Render chart to file (sync)
        var fileRenderResult = renderingService.RenderToFile(chart, "./output/quarterly-sales.png");
        
        if (fileRenderResult.Success)
        {
            Console.WriteLine($"Chart saved to file: {fileRenderResult.FilePath}");
        }

        // Example 3: Export chart with custom options (async)
        var exportOptions = new ExportOptions
        {
            Format = ExportFormat.SVG,
            OutputDirectory = "./output",
            FileName = "quarterly-sales"
        };
        
        var exportResult = await renderingService.RenderWithExportAsync(chart, exportOptions);
        
        if (exportResult.Success)
        {
            Console.WriteLine($"Chart exported as {exportResult.Format}: {exportResult.FilePath}");
        }

        // Example 4: Prewarm cache for faster subsequent renders
        renderingService.PrewarmCache(chart);
        Console.WriteLine("Cache prewarmed for chart");
    }
}
```

## ChartRenderingService

`ChartRenderingService` is the core rendering service that converts chart models into visual representations using SkiaSharp. It provides methods for rendering charts to byte arrays, files, and various export formats (PNG, JPEG, WEBP, SVG), with built-in caching for performance optimization.

The service supports both synchronous and asynchronous rendering operations, handles chart validation, manages an in-memory render cache to avoid redundant rendering operations, and provides detailed metrics about rendering performance. It's designed for both standalone usage and integration into ASP.NET Core applications via dependency injection.

## ChartDataService

`ChartDataService` is a core service that provides chart data validation, transformation, and normalization capabilities. It ensures data integrity by validating chart structures, series configurations, and individual data points, while also offering utilities for data transformation and filtering to prepare charts for rendering.

The service handles synchronous and asynchronous validation, calculates appropriate axis ranges for different scale types, normalizes data points to [0, 1] ranges, and provides transformation pipelines for chart data manipulation.

```csharp
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Services;

public class ChartDataServiceExample
{
    public static void Main()
    {
        // Initialize ChartDataService with logger
        var logger = new NullLogger<ChartDataService>();
        var chartDataService = new ChartDataService(logger);

        // Example 1: Validate a chart before rendering
        var chart = new Chart("sales-chart")
        {
            Title = "Quarterly Sales Performance"
        };

        var series = new ChartSeries("Revenue")
        {
            LineWidth = 2.5f,
            Color = "#2E86C1"
        };

        series.AddDataPoint(1.0, 100000.0);
        series.AddDataPoint(2.0, 125000.0);
        series.AddDataPoint(3.0, 150000.0);
        series.AddDataPoint(4.0, 175000.0);
        chart.AddSeries(series);

        // Validate chart structure and data
        try
        {
            chartDataService.ValidateChart(chart);
            Console.WriteLine("Chart validation passed successfully");
        }
        catch (InvalidChartDataException ex)
        {
            Console.WriteLine($"Validation failed: {ex.Message}");
        }

        // Example 2: Validate chart asynchronously
        var validationTask = chartDataService.ValidateChartAsync(chart);
        bool isValid = validationTask.Result;
        Console.WriteLine($"Async validation result: {isValid}");

        // Example 3: Calculate axis range for chart rendering
        var xValues = new List<double> { 1.0, 2.0, 3.0, 4.0 };
        var yValues = new List<double> { 100000.0, 125000.0, 150000.0, 175000.0 };

        var (xMin, xMax) = chartDataService.CalculateAxisRange(xValues, AxisScaleType.Linear);
        var (yMin, yMax) = chartDataService.CalculateAxisRange(yValues, AxisScaleType.Linear);

        Console.WriteLine($"X-axis range: [{xMin:F2}, {xMax:F2}]");
        Console.WriteLine($"Y-axis range: [{yMin:F2}, {yMax:F2}]");

        // Example 4: Normalize data points for consistent rendering
        var dataPoints = new List<DataPoint>
        {
            new DataPoint(1.0, 100000.0),
            new DataPoint(2.0, 125000.0),
            new DataPoint(3.0, 150000.0),
            new DataPoint(4.0, 175000.0)
        };

        chartDataService.NormalizeDataPoints(dataPoints);
        Console.WriteLine("Data points normalized to [0, 1] range");

        // Example 5: Transform chart data using a custom function
        Chart transformedChart = chartDataService.TransformChartData(
            chart,
            point => new DataPoint(point.X * 2, point.Y * 1.1) // Scale X by 2, Y by 1.1
        );
        Console.WriteLine("Chart data transformed successfully");

        // Example 6: Filter data points based on a condition
        var filteredPoints = chartDataService.FilterDataPoints(
            dataPoints,
            point => point.Y > 120000.0 // Only keep points above 120,000
        );
        Console.WriteLine($"Filtered {filteredPoints.Count} points above threshold");
    }
}
```

## ChartDiffService

`ChartDiffService` computes differences between chart versions for change tracking and auditing purposes. It identifies modifications to chart properties, series data, and configuration settings, making it ideal for version control, audit logging, and change visualization workflows.

The service compares two chart instances and generates a detailed diff report showing what changed between versions, including property names, old values, new values, and timestamps for each detected change.

```csharp
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Services;

public class ChartDiffServiceExample
{
    public static void Main()
    {
        // Initialize ChartDiffService with logger
        var logger = new NullLogger<ChartDiffService>();
        var diffService = new ChartDiffService(logger);

        // Create original chart
        var oldChart = new Chart("sales-chart")
        {
            Title = "Sales Performance Q1 2024",
            ChartType = ChartType.LineChart
        };

        var oldSeries = new ChartSeries("Revenue")
        {
            LineWidth = 2.5f,
            Color = "#2E86C1"
        };
        oldSeries.AddDataPoint(1.0, 100000.0);
        oldSeries.AddDataPoint(2.0, 125000.0);
        oldChart.AddSeries(oldSeries);

        // Create modified chart
        var newChart = new Chart("sales-chart")
        {
            Title = "Sales Performance Q2 2024",
            ChartType = ChartType.LineChart
        };

        var newSeries = new ChartSeries("Revenue")
        {
            LineWidth = 2.5f,
            Color = "#2E86C1"
        };
        newSeries.AddDataPoint(1.0, 100000.0);
        newSeries.AddDataPoint(2.0, 125000.0);
        newSeries.AddDataPoint(3.0, 150000.0); // Added Q3 data
        newSeries.AddDataPoint(4.0, 175000.0); // Added Q4 data
        newChart.AddSeries(newSeries);

        // Compute diff between charts
        var diff = diffService.ComputeDiff(oldChart, newChart);

        if (diff != null && diff.HasChanges)
        {
            Console.WriteLine($"Chart diff computed for chart: {diff.ChartId}");
            Console.WriteLine($"Changes detected: {diff.Changes.Count}");
            Console.WriteLine($"Computed at: {diff.ComputedAt}");
            Console.WriteLine();

            // Display detailed changes
            foreach (var change in diff.Changes)
            {
                Console.WriteLine($"Property: {change.Property}");
                Console.WriteLine($"  Old: {change.OldValue ?? "(null)"}");
                Console.WriteLine($"  New: {change.NewValue ?? "(null)"}");
                Console.WriteLine($"  Changed: {change.ChangedAt}");
                Console.WriteLine();
            }

            // Generate formatted diff report
            string report = diffService.GenerateDiffReport(diff);
            Console.WriteLine(report);
        }
        else
        {
            Console.WriteLine("No changes detected between charts.");
        }
    }
}
```

```csharp
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Services;

public class ChartRenderingServiceExample
{
    public static async Task Main(string[] args)
    {
        // Initialize ChartRenderingService with required dependencies
        var logger = new NullLogger<ChartRenderingService>();
        var dataService = new ChartDataService(logger);
        var cacheService = new RenderCacheService(logger, maxCacheSize: 100);
        
        var renderingService = new ChartRenderingService(logger, dataService, cacheService);

        // Example 1: Render chart to byte array (async)
        var chart = new Chart("sales-chart")
        {
            Title = "Quarterly Sales Performance",
            Configuration = new ChartConfiguration
            {
                Width = 800,
                Height = 600,
                BackgroundColor = "#FFFFFF",
                ShowGrid = true,
                ShowLegend = true
            }
        };
        
        var chartSeries = new ChartSeries("Revenue")
        {
            LineWidth = 2.5f,
            Color = "#2E86C1"
        };
        chartSeries.AddDataPoint(1.0, 100000.0);
        chartSeries.AddDataPoint(2.0, 125000.0);
        chartSeries.AddDataPoint(3.0, 150000.0);
        chartSeries.AddDataPoint(4.0, 175000.0);
        chart.AddSeries(chartSeries);

        // Render to byte array asynchronously
        var renderResult = await renderingService.RenderToByteArrayAsync(chart);
        
        if (renderResult.Success)
        {
            Console.WriteLine($"Chart rendered successfully: {renderResult.ImageData.Length} bytes");
            Console.WriteLine($"Render time: {renderResult.RenderTimeMs}ms");
        }

        // Example 2: Render chart to file (sync)
        var fileRenderResult = renderingService.RenderToFile(chart, "./output/quarterly-sales.png");
        
        if (fileRenderResult.Success)
        {
            Console.WriteLine($"Chart saved to file: {fileRenderResult.FilePath}");
        }

        // Example 3: Export chart with custom options (async)
        var exportOptions = new ExportOptions
        {
            Format = ExportFormat.SVG,
            OutputDirectory = "./output",
            FileName = "quarterly-sales"
        };
        
        var exportResult = await renderingService.RenderWithExportAsync(chart, exportOptions);
        
        if (exportResult.Success)
        {
            Console.WriteLine($"Chart exported as {exportResult.Format}: {exportResult.FilePath}");
        }

        // Example 4: Prewarm cache for faster subsequent renders
        renderingService.PrewarmCache(chart);
        Console.WriteLine("Cache prewarmed for chart");
    }
}
```

```csharp
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Services;

public class ConfigurationServiceExample
{
    public static void Main()
    {
        // Initialize ConfigurationService with logger
        var logger = new NullLogger<ConfigurationService>();
        var configService = new ConfigurationService(logger);

        // Example 1: Get default configuration
        var defaultConfig = configService.GetDefaultConfiguration();
        Console.WriteLine($"Default config - Title: {defaultConfig.Title}, Grid enabled: {defaultConfig.ShowGrid}");

        // Example 2: List all available configurations
        var configs = configService.ListConfigurations();
        Console.WriteLine($"Available configurations: {string.Join(", ", configs)}");

        // Example 3: Check if configuration exists
        bool exists = configService.ConfigurationExists("default_line");
        Console.WriteLine($"Configuration 'default_line' exists: {exists}");

        // Example 4: Get a specific configuration
        var lineConfig = configService.GetConfiguration("default_line");
        Console.WriteLine($"Retrieved config: {lineConfig.Title}");

        // Example 5: Create configuration from template
        var barConfig = configService.CreateConfigurationFromTemplate(ChartType.BarChart);
        Console.WriteLine($"Created bar config: {barConfig.Title}, X-axis type: {barConfig.XAxisScaleType}");

        // Example 6: Save a custom configuration
        var customConfig = new ChartConfiguration
        {
            Title = "Custom Sales Dashboard",
            XAxisLabel = "Quarter",
            YAxisLabel = "Revenue ($)",
            ShowGrid = true,
            ShowLegend = true,
            MarginTop = 50,
            MarginBottom = 70,
            MarginLeft = 70,
            MarginRight = 50,
            BackgroundColor = "#FFFFFF",
            GridColor = "#E0E0E0"
        };

        configService.SaveConfiguration("sales_dashboard", customConfig);
        Console.WriteLine("Custom configuration saved successfully");

        // Example 7: Verify configuration was saved
        bool configSaved = configService.ConfigurationExists("sales_dashboard");
        Console.WriteLine($"Configuration saved: {configSaved}");

        // Example 8: Delete a configuration
        configService.DeleteConfiguration("sales_dashboard");
        Console.WriteLine("Configuration deleted successfully");

        // Example 9: Verify configuration was deleted
        bool configDeleted = !configService.ConfigurationExists("sales_dashboard");
        Console.WriteLine($"Configuration deleted: {configDeleted}");
    }
}
```

## RenderMetrics

`RenderMetrics` is a metrics collection class that tracks and analyzes chart rendering performance statistics. It captures key performance indicators such as render time, image size, series count, data point count, cache usage, and export format. The class provides analytical methods to calculate throughput metrics like megabytes per second and data points per second, enabling performance optimization and benchmarking of chart rendering operations.

```csharp
using System;
using System.Collections.Generic;
using SkiaSharpChartEngine.Models;

public class RenderMetricsExample
{
    public static void Main()
    {
        // Example 1: Record basic rendering metrics
        var metrics = new RenderMetrics
        {
            ChartId = "quarterly-sales-chart",
            RenderTimeMs = 150,
            ImageSizeBytes = 45234,
            SeriesCount = 3,
            DataPointCount = 120,
            ExportFormat = ExportFormat.PNG,
            CacheSizeAtRenderTime = 1024,
            WasCached = false,
            AdditionalMetrics = new Dictionary<string, object>
            {
                ["cpuUsage"] = 45.2,
                ["memoryUsage"] = 128.5,
                ["renderEngine"] = "SkiaSharp"
            }
        };
        
        Console.WriteLine($"Render metrics: {metrics}");
        Console.WriteLine($"Chart: {metrics.ChartId}");
        Console.WriteLine($"Render time: {metrics.RenderTimeMs}ms");
        Console.WriteLine($"Image size: {metrics.ImageSizeBytes} bytes ({metrics.ImageSizeBytes / 1024.0:F2} KB)");
        Console.WriteLine($"Series count: {metrics.SeriesCount}");
        Console.WriteLine($"Data points: {metrics.DataPointCount}");
        
        // Example 2: Calculate performance metrics
        var mbPerSecond = metrics.GetMegabytesPerSecond();
        var pointsPerSecond = metrics.GetDataPointsPerSecond();
        
        Console.WriteLine($"Throughput: {mbPerSecond:F2} MB/s");
        Console.WriteLine($"Data points per second: {pointsPerSecond:F2}");
        
        // Example 3: Use with MetricsCollector for tracking multiple renders
        var collector = new MetricsCollector();
        collector.RecordMetrics(metrics);
        
        // Simulate another render
        var cachedMetrics = new RenderMetrics
        {
            ChartId = "quarterly-sales-chart",
            RenderTimeMs = 25,  // Much faster due to caching
            ImageSizeBytes = 45234,
            SeriesCount = 3,
            DataPointCount = 120,
            ExportFormat = ExportFormat.PNG,
            CacheSizeAtRenderTime = 1024,
            WasCached = true
        };
        
        collector.RecordMetrics(cachedMetrics);
        
        // Example 4: Analyze collected metrics
        Console.WriteLine($"\nCollected {collector.GetMetricsCount()} render metrics");
        Console.WriteLine($"Average render time: {collector.GetAverageRenderTimeMs():F2}ms");
        Console.WriteLine($"Average image size: {collector.GetAverageImageSizeBytes() / 1024.0:F2} KB");
        Console.WriteLine($"Cache hit count: {collector.GetCacheHitCount()}");
        Console.WriteLine($"Cache hit percentage: {collector.GetCacheHitPercentage():F1}%");
        
        // Example 5: Get last metrics
        var lastMetrics = collector.GetLastMetrics();
        Console.WriteLine($"\nLast render: {lastMetrics?.ChartId} in {lastMetrics?.RenderTimeMs}ms");
    }
}
```

```csharp
using SkiaSharpChartEngine.Models;

// Create a chart configuration with custom styling
var config = new ChartConfiguration
{
    Subtitle = "Quarterly Performance Report",
    XAxisLabel = "Quarter",
    YAxisLabel = "Revenue ($)",
    BackgroundColor = "#FFFFFF",
    GridColor = "#E0E0E0",
    AxisColor = "#333333",
    TextColor = "#333333",
    MarginTop = 40,
    MarginBottom = 60,
    MarginLeft = 60,
    MarginRight = 40,
    ShowLegend = true,
    ShowGrid = true,
    ShowAxisLabels = true,
    ShowDataPointLabels = true,
    XAxisScaleType = AxisScaleType.Linear,
    YAxisScaleType = AxisScaleType.Linear,
    XAxisMin = 0.5,
    XAxisMax = 4.5,
    YAxisMin = 0,
    YAxisMax = 200000
};

// Use the configuration when creating a chart
var chart = new Chart("sales-chart")
{
    Configuration = config
};
```

## ExportOptions

`ExportOptions` provides configuration for exporting charts to various image formats (PNG, JPEG, SVG, PDF, WEBP). It controls output quality, resolution (DPI), font embedding, aspect ratio preservation, and custom format-specific options. This class is used throughout the chart engine for saving rendered charts to files with consistent settings.

```csharp
using System;
using System.IO;
using SkiaSharpChartEngine.Models;

public class ExportOptionsExample
{
    public static void Main()
    {
        // Example 1: Create export options with default settings
        var defaultOptions = new ExportOptions("chart", ExportFormat.PNG);
        Console.WriteLine($"Default export: {defaultOptions}");
        Console.WriteLine($"Full path: {defaultOptions.GetFullPath()}");
        
        // Example 2: Configure high-quality PNG export with custom DPI
        var highQualityOptions = new ExportOptions(
            filename: "high-quality-chart",
            format: ExportFormat.PNG,
            dpi: 300,
            quality: 0.95f
        )
        {
            OutputDirectory = "./exports/",
            EmbedFonts = true,
            PreserveAspectRatio = true,
            CustomFormatOptions = new Dictionary<string, object>
            {
                ["compressionLevel"] = 6,
                ["includeAlpha"] = true
            }
        };
        
        Console.WriteLine($"High quality export: {highQualityOptions}");
        Console.WriteLine($"Full path: {highQualityOptions.GetFullPath()}");
        
        // Example 3: Export to JPEG with specific quality settings
        var jpegOptions = new ExportOptions("photo-chart", ExportFormat.JPEG, 150, 0.85f)
        {
            OutputDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "charts")
        };
        
        Console.WriteLine($"JPEG export: {jpegOptions}");
        
        // Example 4: Export to PDF format
        var pdfOptions = new ExportOptions("report-chart", ExportFormat.PDF)
        {
            DPI = 150,
            Quality = 0.9f,
            EmbedFonts = true
        };
        
        Console.WriteLine($"PDF export: {pdfOptions}");
        Console.WriteLine($"File extension: {ExportOptions.GetFileExtension(pdfOptions.Format)}");
        
        // Example 5: Clone and modify export options
        var clonedOptions = highQualityOptions.Clone();
        clonedOptions.Filename = "modified-chart";
        clonedOptions.Quality = 0.8f;
        
        Console.WriteLine($"Cloned options: {clonedOptions}");
        
        // Example 6: Validate export options before use
        try
        {
            defaultOptions.Validate();
            Console.WriteLine("✓ Export options are valid");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Validation failed: {ex.Message}");
        }
        
        // Example 7: Get file extension from format
        Console.WriteLine($"PNG extension: {ExportOptions.GetFileExtension(ExportFormat.PNG)}");
        Console.WriteLine($"SVG extension: {ExportOptions.GetFileExtension(ExportFormat.SVG)}");
        Console.WriteLine($"PDF extension: {ExportOptions.GetFileExtension(ExportFormat.PDF)}");
    }
}
```

## TooltipOptions

`TooltipOptions` controls the appearance and behavior of interactive tooltips when hovering over chart data points. It allows customization of colors, dimensions, hit-testing radius, and content formatting. Tooltips provide contextual information about the data point under the cursor, including X/Y values, series name, and custom formatted content.

```csharp
using System;
using SkiaSharpChartEngine.Models;

public class TooltipOptionsExample
{
    public static void Main()
    {
        // Create a chart with sample data
        var chart = new Chart("sales-performance-chart");
        var series = new ChartSeries("Revenue")
        {
            LineWidth = 2.5f,
            Color = "#2E86C1"
        };
        series.AddDataPoint(1.0, 100000.0);
        series.AddDataPoint(2.0, 125000.0);
        series.AddDataPoint(3.0, 150000.0);
        series.AddDataPoint(4.0, 175000.0);
        chart.AddSeries(series);

        // Configure tooltip appearance and behavior
        var tooltipOptions = new TooltipOptions
        {
            Enabled = true,
            BackgroundColor = "#2E86C1",
            BorderColor = "#1A5276",
            TextColor = "#FFFFFF",
            BorderWidth = 2f,
            Padding = 12f,
            BorderRadius = 8f,
            FontSize = 14f,
            HitRadius = 20f,
            ShadowOpacity = 0.3f,
            ContentTemplate = "{series}: {y:C0} in Q{x}"
        };

        // Use tooltip options when checking for tooltip hits
        var tooltipResult = chart.GetTooltipAt(
            pointerX: 200,
            pointerY: 300,
            canvasWidth: 800,
            canvasHeight: 600,
            options: tooltipOptions
        );

        if (tooltipResult.IsHit)
        {
            Console.WriteLine($"Tooltip: {tooltipResult.TooltipText}");
            Console.WriteLine($"Data point: X={tooltipResult.DataPoint?.X}, Y={tooltipResult.DataPoint?.Y}");
            Console.WriteLine($"Series: {tooltipResult.Series?.Name}");
        }

        // Clone tooltip options for reuse
        var clonedOptions = tooltipOptions.Clone();
        clonedOptions.BackgroundColor = "#E74C3C";
    }
}
```

## InteractivityExtensions

`InteractivityExtensions` provides extension methods for adding interactive features to charts, including tooltip hit-testing, zooming, panning, and viewport management. These methods allow you to implement rich user interactions like hover tooltips, zoom/pan gestures, and reset functionality in your chart applications.

The extension methods can be used with or without dependency injection - they include a built-in default service for standalone scenarios while also providing the `AddChartInteractivity()` method to register the service in your DI container.

```csharp
using System;
using SkiaSharp;
using SkiaSharpChartEngine.Extensions;
using SkiaSharpChartEngine.Models;

public class InteractivityExample
{
    public static void Main()
    {
        // Create a chart with sample data
        var chart = new Chart("sales-chart");
        var series = new ChartSeries("Revenue")
        {
            LineWidth = 2.5f,
            Color = "#2E86C1"
        };
        series.AddDataPoint(1.0, 100000.0);
        series.AddDataPoint(2.0, 125000.0);
        series.AddDataPoint(3.0, 150000.0);
        series.AddDataPoint(4.0, 175000.0);
        chart.AddSeries(series);

        // Example 1: Get tooltip at specific coordinates
        var tooltipResult = chart.GetTooltipAt(
            pointerX: 200,
            pointerY: 300,
            canvasWidth: 800,
            canvasHeight: 600,
            options: new TooltipOptions { HitRadius = 10 }
        );

        if (tooltipResult.IsHit)
        {
            Console.WriteLine($"Tooltip: {tooltipResult.TooltipText}");
            Console.WriteLine($"Data point: X={tooltipResult.DataPoint?.X}, Y={tooltipResult.DataPoint?.Y}");
        }

        // Example 2: Zoom in at a specific point (2x zoom)
        var currentViewport = chart.ResetViewport();
        var zoomedViewport = chart.ZoomAt(
            current: currentViewport,
            anchorX: 400,
            anchorY: 300,
            canvasWidth: 800,
            canvasHeight: 600,
            factor: 2.0
        );

        Console.WriteLine($"Zoomed viewport: {zoomedViewport.ZoomLevel:P0}");

        // Example 3: Pan the viewport by 50 pixels right
        var pannedViewport = chart.PanBy(
            current: zoomedViewport,
            deltaX: 50,
            deltaY: 0,
            canvasWidth: 800,
            canvasHeight: 600
        );

        Console.WriteLine($"Panned viewport: X offset={pannedViewport.XOffset}");

        // Example 4: Reset viewport to show all data
        var fullViewport = chart.ResetViewport();
        Console.WriteLine("Reset to full viewport");

        // Example 5: Async tooltip hit-testing
        var asyncTooltip = await chart.GetTooltipAtAsync(
            pointerX: 250,
            pointerY: 250,
            canvasWidth: 800,
            canvasHeight: 600
        );

        Console.WriteLine($"Async tooltip hit: {asyncTooltip.IsHit}");

        // Example 6: Using SKPoint overload for MAUI/Blazor coordinates
        var skPoint = new SKPoint(300, 200);
        var tooltipFromPoint = chart.GetTooltipAt(
            point: skPoint,
            canvasWidth: 800,
            canvasHeight: 600
        );
    }
}
```

## StringExtensions

`StringExtensions` provides a comprehensive set of utility extension methods for string manipulation and conversion. It includes methods for converting between different string formats (camelCase, snake_case, PascalCase, kebab-case), parsing and converting numeric values, truncating strings, checking string properties, and various text transformations.

```csharp
using System;
using SkiaSharpChartEngine.Extensions;

public class StringExtensionsExample
{
    public static void Main()
    {
        // Example 1: Convert between different string formats
        string camelCase = "salesPerformanceMonthlyDashboard";
        string kebabCase = camelCase.ToKebabCase();
        Console.WriteLine($"Kebab case: {kebabCase}");
        // Output: Kebab case: sales-performance-monthly-dashboard

        string snakeCase = "quarterly_revenue_report";
        string pascalCase = snakeCase.ToPascalCase();
        Console.WriteLine($"Pascal case: {pascalCase}");
        // Output: Pascal case: QuarterlyRevenueReport

        // Example 2: Parse strings to numeric values with fallback
        string numberString = "42.75";
        double parsedDouble = numberString.ToDoubleOrDefault();
        Console.WriteLine($"Parsed double: {parsedDouble}");
        // Output: Parsed double: 42.75

        string invalidNumber = "not-a-number";
        double defaultValue = invalidNumber.ToDoubleOrDefault(999.99);
        Console.WriteLine($"Invalid number fallback: {defaultValue}");
        // Output: Invalid number fallback: 999.99

        // Example 3: Truncate long strings with ellipsis
        string longText = "This is a very long text that needs to be truncated";
        string truncated = longText.Truncate(20);
        Console.WriteLine($"Truncated: {truncated}");
        // Output: Truncated: This is a very long...

        // Example 4: Check string properties
        string hexColor = "#FF5733";
        bool isValidHex = hexColor.IsValidHexColor();
        Console.WriteLine($"Is valid hex color: {isValidHex}");
        // Output: Is valid hex color: True

        string numericString = "12345";
        bool containsDigits = numericString.ContainsDigit();
        Console.WriteLine($"Contains digits: {containsDigits}");
        // Output: Contains digits: True

        // Example 5: Text transformations
        string mixedCase = "hElLo WoRlD";
        string capitalized = mixedCase.CapitalizeFirstLetter();
        Console.WriteLine($"Capitalized: {capitalized}");
        // Output: Capitalized: HElLo WoRlD

        string whitespaceText = "  Hello   World  ";
        string noWhitespace = whitespaceText.RemoveWhitespace();
        Console.WriteLine($"No whitespace: '{noWhitespace}'");
        // Output: No whitespace: 'HelloWorld'

        // Example 6: Extract numbers from strings
        string alphanumeric = "Product ID: 12345, Version: 2.1.0";
        string numbersOnly = alphanumeric.ExtractNumbers();
        Console.WriteLine($"Extracted numbers: {numbersOnly}");
        // Output: Extracted numbers: 12345210

        // Example 7: Repeat strings for padding or separators
        string separator = "=".Repeat(30);
        Console.WriteLine(separator);
        // Output: ================================

        // Example 8: Check if string is a palindrome
        string palindrome = "madam";
        bool isPalindrome = palindrome.IsPalindrome();
        Console.WriteLine($"Is palindrome: {isPalindrome}");
        // Output: Is palindrome: True

        string notPalindrome = "hello";
        bool isNotPalindrome = notPalindrome.IsPalindrome();
        Console.WriteLine($"Is palindrome: {isNotPalindrome}");
        // Output: Is palindrome: False
    }
}
```

## DataPointExtensions

`DataPointExtensions` provides utility methods for working with `DataPoint` objects, enabling common geometric and statistical operations on chart data points. It includes distance calculations, proximity checks, and transformation methods for both individual points and collections of points.

```csharp
using System;
using System.Collections.Generic;
using SkiaSharpChartEngine.Extensions;
using SkiaSharpChartEngine.Models;

public class DataPointExtensionsExample
{
    public static void Main()
    {
        // Create sample data points
        var pointA = new DataPoint(1.0, 2.0);
        var pointB = new DataPoint(4.0, 6.0);
        var pointC = new DataPoint(1.0, 2.0);
        
        // Example 1: Calculate Euclidean distance between two points
        double distance = pointA.GetDistance(pointB);
        Console.WriteLine($"Distance between A and B: {distance:F2}");
        // Output: Distance between A and B: 5.00
        
        // Example 2: Check if two points are near each other (within tolerance)
        bool isNear = pointA.IsNear(pointB, 0.1);
        Console.WriteLine($"Points A and B are near: {isNear}");
        // Output: Points A and B are near: False
        
        // Example 3: Offset a single data point by specified X and Y values
        var offsetPoint = pointA.Offset(3.0, 1.0);
        Console.WriteLine($"Offset point: [{offsetPoint.X}, {offsetPoint.Y}]");
        // Output: Offset point: [4, 3]
        
        // Example 4: Scale a single data point by specified factors
        var scaledPoint = pointA.Scale(2.0, 0.5);
        Console.WriteLine($"Scaled point: [{scaledPoint.X:F2}, {scaledPoint.Y:F2}]");
        // Output: Scaled point: [2.00, 1.00]
        
        // Example 5: Offset all points in a collection
        var points = new List<DataPoint> { pointA, pointB, new DataPoint(7.0, 8.0) };
        var offsetPoints = points.Offset(10.0, 5.0);
        Console.WriteLine("Offset points:");
        foreach (var p in offsetPoints)
        {
            Console.WriteLine($"  [{p.X}, {p.Y}]");
        }
        // Output: Offset points:
        //   [11, 7]
        //   [14, 11]
        //   [17, 13]
        
        // Example 6: Scale all points in a collection
        var scaledPoints = points.Scale(0.5, 2.0);
        Console.WriteLine("Scaled points:");
        foreach (var p in scaledPoints)
        {
            Console.WriteLine($"  [{p.X:F2}, {p.Y:F2}]");
        }
        // Output: Scaled points:
        //   [0.50, 4.00]
        //   [2.00, 12.00]
        //   [3.50, 16.00]
        
        // Example 7: Get bounds (min/max X and Y) for a collection of points
        var bounds = points.GetBounds();
        Console.WriteLine($"Bounds: X=[{bounds.minX:F2}, {bounds.maxX:F2}], Y=[{bounds.minY:F2}, {bounds.maxY:F2}]");
        // Output: Bounds: X=[1.00, 7.00], Y=[2.00, 8.00]
        
        // Example 8: Calculate average X coordinate across multiple points
        double avgX = points.GetAverageX();
        Console.WriteLine($"Average X: {avgX:F2}");
        // Output: Average X: 4.00
        
        // Example 9: Calculate average Y coordinate across multiple points
        double avgY = points.GetAverageY();
        Console.WriteLine($"Average Y: {avgY:F2}");
        // Output: Average Y: 5.33
    }
}
```

## ArgumentParser

`ArgumentParser` is a utility class for parsing command-line arguments into key-value pairs, supporting multiple input formats including `--key=value`, `--key value`, and `-k value`. It provides methods for parsing arguments, validating required parameters, retrieving values with defaults, and parsing comma-separated lists.

The parser handles flags (boolean arguments without values), required argument validation, and provides flexible value retrieval with default fallbacks. It's commonly used in CLI applications and console tools that need to process user input arguments.

```csharp
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SkiaSharpChartEngine.CLI;

public class ArgumentParserExample
{
    public static void Main(string[] args)
    {
        // Initialize argument parser with logger
        var logger = new NullLogger<ArgumentParser>();
        var argumentParser = new ArgumentParser(logger);

        // Example 1: Parse command line arguments
        var parsedArgs = argumentParser.Parse(new[] { 
            "--output=chart.png",
            "--width", "800",
            "-h", "600",
            "--format", "png",
            "--verbose"
        });

        Console.WriteLine("Parsed arguments:");
        foreach (var kvp in parsedArgs)
        {
            Console.WriteLine($"  {kvp.Key} = {kvp.Value}");
        }

        // Example 2: Validate required arguments
        bool hasRequired = argumentParser.ValidateRequired(
            parsedArgs,
            "output", "width", "height"
        );
        Console.WriteLine($"\nHas required arguments: {hasRequired}");

        // Example 3: Get argument value with default fallback
        string outputFile = argumentParser.GetValue(parsedArgs, "output", "default.png");
        int width = int.Parse(argumentParser.GetValue(parsedArgs, "width", "800"));
        int height = int.Parse(argumentParser.GetValue(parsedArgs, "height", "600"));
        string format = argumentParser.GetValue(parsedArgs, "format", "png");

        Console.WriteLine($"\nChart settings:");
        Console.WriteLine($"  Output file: {outputFile}");
        Console.WriteLine($"  Dimensions: {width}x{height}");
        Console.WriteLine($"  Format: {format}");

        // Example 4: Parse comma-separated list
        var tags = argumentParser.ParseList(parsedArgs, "tags");
        Console.WriteLine($"\nTags: {string.Join(", ", tags)}");

        // Example 5: Check for boolean flags
        bool isVerbose = argumentParser.GetValue(parsedArgs, "verbose") == "true";
        Console.WriteLine($"Verbose mode: {isVerbose}");
    }
}
```

## RequestValidationMiddleware

`RequestValidationMiddleware` is a middleware component that validates incoming HTTP requests before they are processed by the chart engine. It enforces security and data integrity by validating request headers, payload size limits, JSON schema compliance, and query parameters. This middleware helps prevent common web vulnerabilities and ensures that only properly formatted requests reach the chart rendering pipeline.

The middleware supports multiple content types (JSON, form data, CSV, XML) and provides configurable validation rules including maximum payload size limits and required field validation.

## PerformanceMonitoringMiddleware

`PerformanceMonitoringMiddleware` is a middleware component that monitors and tracks request performance metrics including execution time, memory usage, and operation latency. It helps identify performance bottlenecks, slow operations, and resource-intensive requests by providing detailed performance statistics and configurable slow operation alerts.

The middleware automatically tracks:
- Request execution time using high-precision stopwatch
- Memory allocation and usage during request processing
- Operation name and request correlation ID for traceability
- Configurable slow operation thresholds with automatic logging

```csharp
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SkiaSharpChartEngine.Middleware;

public class PerformanceMonitoringMiddlewareExample
{
    public static void Main()
    {
        // Initialize logger and middleware
        var logger = new NullLogger<PerformanceMonitoringMiddleware>();
        var performanceMiddleware = new PerformanceMonitoringMiddleware(logger, slowThresholdMs: 500);
        
        // Start monitoring a request
        var requestId = Guid.NewGuid().ToString();
        var operationName = "ChartRenderService.GetChartAsync";
        var context = performanceMiddleware.StartRequest(requestId, operationName);
        
        try
        {
            // Simulate request processing
            Console.WriteLine("Processing request...");
            System.Threading.Thread.Sleep(250); // Simulate work
            
            // End monitoring and log results
            performanceMiddleware.EndRequest(context);
            
            // Get performance statistics
            var stats = performanceMiddleware.GetStatistics(operationName);
            Console.WriteLine($"Performance stats for {operationName}:");
            Console.WriteLine($"  Average: {stats.AverageMs}ms");
            Console.WriteLine($"  Min: {stats.MinMs}ms");
            Console.WriteLine($"  Max: {stats.MaxMs}ms");
            Console.WriteLine($"  Total calls: {stats.TotalCalls}");
            Console.WriteLine($"  Collected at: {stats.CollectedAt}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Request failed: {ex.Message}");
            // Ensure context is ended even on failure
            performanceMiddleware.EndRequest(context);
        }
        
        // Update slow threshold dynamically
        performanceMiddleware.SetSlowThreshold(1000); // 1 second threshold
    }
}
```

## RateLimitingMiddleware

`RateLimitingMiddleware` is a token bucket-based rate limiting middleware that prevents abuse by limiting the number of requests individual clients can make within a specified time window. It uses a sliding window approach with configurable token refill rates to provide fair rate limiting while allowing bursts of activity.

The middleware tracks request counts per client using a token bucket algorithm, where each client gets a bucket of tokens that refill at regular intervals. When a client exceeds their token limit, subsequent requests are rejected until tokens become available again.

```csharp
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SkiaSharpChartEngine.Middleware;

public class RateLimitingMiddlewareExample
{
    public static void Main()
    {
        // Initialize rate limiting middleware with logger
        var logger = new NullLogger<RateLimitingMiddleware>();
        
        // Create rate limit policy with 100 requests per minute
        var policy = new RateLimitPolicy
        {
            MaxTokens = 100,
            RefillIntervalSeconds = 60
        };
        
        var rateLimiter = new RateLimitingMiddleware(logger, policy);
        
        // Example 1: Check if a request should be allowed
        string clientId = "user-123";
        bool isAllowed = rateLimiter.AllowRequest(clientId, out var rateLimitInfo);
        
        Console.WriteLine($"Request allowed: {isAllowed}");
        Console.WriteLine($"Available tokens: {rateLimitInfo?.AvailableTokens}/{rateLimitInfo?.MaxTokens}");
        Console.WriteLine($"Reset in: {rateLimitInfo?.SecondsUntilReset} seconds");
        
        // Example 2: Get current rate limit information
        var currentInfo = rateLimiter.GetRateLimitInfo(clientId);
        Console.WriteLine($"Current tokens: {currentInfo?.AvailableTokens}/{currentInfo?.MaxTokens}");
        
        // Example 3: Manually reset rate limit for a client (useful in testing)
        rateLimiter.ResetClientLimit(clientId);
        Console.WriteLine("Rate limit reset for client");
        
        // Example 4: Using with custom identifier extractor
        var policyWithExtractor = new RateLimitPolicy
        {
            MaxTokens = 50,
            RefillIntervalSeconds = 30,
            CustomIdentifierExtractor = () => Environment.UserName
        };
        
        var customRateLimiter = new RateLimitingMiddleware(logger, policyWithExtractor);
        bool customAllowed = customRateLimiter.AllowRequest("any-client", out _);
        Console.WriteLine($"Custom identifier extractor request allowed: {customAllowed}");
    }
}
```

## LoggingMiddleware

`LoggingMiddleware` is a middleware component that logs HTTP request and response details for debugging and monitoring purposes. It captures comprehensive information about each request including the HTTP method, path, query parameters, headers, request body, response status, response body, and execution timing. The middleware uses a unique trace identifier for each request to correlate logs across different components.

The middleware provides several logging methods:
- `LogRequest`: Logs basic request information
- `LogResponse`: Logs response information including status code and duration
- `LogRequestBody`: Logs the request body content
- `LogError`: Logs error details with stack traces

Example usage in an ASP.NET Core application:

```csharp
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SkiaSharpChartEngine.Middleware;
using SkiaSharpChartEngine.Models;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        // Add logging services
        builder.Logging.AddConsole();
        
        var app = builder.Build();
        
        // Use LoggingMiddleware before other middleware
        app.UseMiddleware<LoggingMiddleware>();
        
        // Example endpoint that uses the logging context
        app.MapGet("/api/charts/{id}", async (string id, HttpContext context) =>
        {
            // Access logging context populated by LoggingMiddleware
            var traceId = context.Items["TraceId"] as string;
            var method = context.Items["Method"] as string;
            var path = context.Items["Path"] as string;
            
            Console.WriteLine($"Processing request: {method} {path} (Trace: {traceId})");
            
            // Your chart rendering logic here
            return Results.Ok(new { ChartId = id, Status = "rendered" });
        });
        
        app.Run();
    }
}

// Example of using LoggingMiddleware with custom configuration
var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.AddDebug();
});

var loggingMiddleware = new LoggingMiddleware(
    async (context, next) => await next(context),
    loggerFactory.CreateLogger<LoggingMiddleware>()
);

// The middleware automatically captures:
// - TraceId: Unique identifier for the request
// - Method: HTTP method (GET, POST, etc.)
// - Path: Request path
// - QueryString: Query parameters
// - Headers: Request headers
// - RemoteIpAddress: Client IP address
// - Stopwatch: Execution timing
// - LoggingContext: Additional context data
```

## ErrorHandlingMiddleware

`ErrorHandlingMiddleware` is a global error handling middleware that catches exceptions and formats them consistently for API responses. It maps different exception types to appropriate HTTP status codes, logs errors appropriately, and wraps them in a standardized `ErrorResponse` format. This middleware ensures consistent error handling across the chart engine API.

```csharp
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SkiaSharpChartEngine.Middleware;
using SkiaSharpChartEngine.Exceptions;

public class ErrorHandlingMiddlewareExample
{
    public static void Main(string[] args)
    {
        // Initialize error handling middleware with logger
        var logger = new NullLogger<ErrorHandlingMiddleware>();
        var errorHandler = new ErrorHandlingMiddleware(logger);

        try
        {
            // Simulate an operation that might throw an exception
            ProcessChartOperation();
        }
        catch (Exception ex)
        {
            // Invoke the error handling middleware
            await errorHandler.InvokeAsync(ex);
        }
    }

    private static void ProcessChartOperation()
    {
        // This method might throw various exceptions
        throw new ArgumentNullException("chartData", "Chart data cannot be null");
    }
}

// Example of handling the MiddlewareException that gets thrown
public class ErrorHandler
{
    private readonly ILogger<ErrorHandler> _logger;

    public ErrorHandler(ILogger<ErrorHandler> logger)
    {
        _logger = logger;
    }

    public async Task HandleErrorAsync(Exception exception)
    {
        if (exception is MiddlewareException middlewareEx)
        {
            // Access the formatted error response
            var errorResponse = middlewareEx.ErrorResponse;

            Console.WriteLine($"Error occurred: {errorResponse.StatusCode} - {errorResponse.Message}");
            Console.WriteLine($"Details: {errorResponse.Details}");
            Console.WriteLine($"Exception Type: {errorResponse.ExceptionType}");
            Console.WriteLine($"Timestamp: {errorResponse.Timestamp}");
            Console.WriteLine($"Trace ID: {errorResponse.TraceId ?? "N/A"}");

            // Log the error appropriately
            _logger.LogError(exception, "Chart engine error handled: {StatusCode} - {Message}",
                errorResponse.StatusCode, errorResponse.Message);
        }
        else
        {
            // Handle non-middleware exceptions
            _logger.LogError(exception, "Unhandled exception");
        }
    }
}
```

```csharp
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SkiaSharpChartEngine.Middleware;

public class RequestValidationMiddlewareExample
{
    public static void Main()
    {
        // Initialize middleware with logger
        var logger = new NullLogger<RequestValidationMiddleware>();
        var validationMiddleware = new RequestValidationMiddleware(logger, maxPayloadSize: 5_242_880); // 5MB limit
        
        // Add custom allowed content type
        validationMiddleware.AddAllowedContentType("application/x-yaml");
        
        // Example 1: Validate request headers
        var headers = new Dictionary<string, string>
        {
            {"Content-Type", "application/json"},
            {"Authorization", "Bearer token123"},
            {"X-Request-ID", Guid.NewGuid().ToString()}
        };
        
        bool headersValid = validationMiddleware.ValidateHeaders(headers);
        Console.WriteLine($"Headers valid: {headersValid}");
        
        // Example 2: Validate payload size
        var smallPayload = new byte[1024]; // 1KB payload
        bool sizeValid = validationMiddleware.ValidatePayloadSize(smallPayload);
        Console.WriteLine($"Payload size valid: {sizeValid}");
        
        // Example 3: Validate JSON schema with required fields
        string jsonPayload = @"{
            \"chartType\": \"line\",
            \"dataPoints\": [
                {\"x\": 1, \"y\": 100},
                {\"x\": 2, \"y\": 200}
            ]
        }";
        
        bool schemaValid = validationMiddleware.ValidateJsonSchema(jsonPayload, "chartType", "dataPoints");
        Console.WriteLine($"JSON schema valid: {schemaValid}");
        
        // Example 4: Validate query parameters
        var queryParams = new Dictionary<string, string>
        {
            {"chartId", "sales-dashboard-2024"},
            {"width", "800"},
            {"height", "600"},
            {"format", "png"}
        };
        
        var validatedParams = validationMiddleware.ValidateQueryParameters(queryParams);
        Console.WriteLine($"Validated {validatedParams.Count} query parameters");
        
        // Example 5: Handle validation failure scenarios
        var invalidHeaders = new Dictionary<string, string>(); // Empty headers
        bool invalidResult = validationMiddleware.ValidateHeaders(invalidHeaders);
        Console.WriteLine($"Invalid headers result: {invalidResult}");
    }
}
```

## ChartRepository

`ChartRepository` is a repository class that provides data access and persistence operations for `Chart` entities. It abstracts the data layer and offers both synchronous and asynchronous methods for CRUD operations, enabling clean separation between business logic and data storage. The repository supports querying charts by ID, type, and search criteria, as well as checking for chart existence and counting charts.

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Repository;

public class ChartRepositoryExample
{
    public static async Task Main(string[] args)
    {
        // Initialize repository with data context
        var repository = new ChartRepository(new ChartDbContext());

        // Example 1: Check if a chart exists by ID
        bool exists = await repository.ExistsAsync("sales-dashboard-2024");
        Console.WriteLine($"Chart exists: {exists}");

        // Example 2: Get a chart by ID
        var chart = await repository.GetByIdAsync("sales-dashboard-2024");
        Console.WriteLine(chart != null
            ? $"Found chart: {chart.Id} - {chart.Title}"
            : "Chart not found");

        // Example 3: Get all charts
        var allCharts = await repository.GetAllAsync();
        Console.WriteLine($"Total charts: {allCharts.Count}");

        // Example 4: Get charts by type
        var lineCharts = await repository.GetByTypeAsync("line");
        Console.WriteLine($"Line charts: {lineCharts.Count}");

        // Example 5: Search charts by query
        var searchResults = await repository.SearchAsync("sales");
        Console.WriteLine($"Search results: {searchResults.Count}");

        // Example 6: Save a new chart
        var newChart = new Chart("monthly-report-q2")
        {
            Title = "Q2 2024 Monthly Report",
            Description = "Monthly sales performance report"
        };
        newChart.AddSeries(new ChartSeries("Revenue")
        {
            LineWidth = 2.5f,
            Color = "#2E86C1"
        });

        string chartId = await repository.SaveAsync(newChart);
        Console.WriteLine($"Saved chart with ID: {chartId}");

        // Example 7: Update an existing chart
        newChart.Title = "Q2 2024 Monthly Report - Updated";
        bool updated = await repository.UpdateAsync(newChart);
        Console.WriteLine($"Chart updated: {updated}");

        // Example 8: Delete a chart
        bool deleted = await repository.DeleteAsync("old-chart-id");
        Console.WriteLine($"Chart deleted: {deleted}");

        // Example 9: Get total chart count
        int chartCount = await repository.GetCountAsync();
        Console.WriteLine($"Total charts in repository: {chartCount}");
    }
}
```

## ChartExtensions

`ChartExtensions` provides a set of extension methods for enhancing and transforming `Chart` objects. These methods enable common chart operations like applying default configurations, calculating axis bounds, counting data points, checking for empty charts, applying color palettes, normalizing data across series, filtering series, and converting charts to builders for further customization.

The extension methods work with existing chart objects and return either the modified chart (for in-place operations) or a new chart instance (for functional-style operations), allowing for method chaining and immutable-style transformations.

```csharp
using System;
using SkiaSharpChartEngine.Extensions;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Utilities;

public class ChartExtensionsExample
{
    public static void Main()
    {
        // Create a chart with sample data
        var chart = new Chart("sales-performance-chart");
        
        var revenueSeries = new ChartSeries("Revenue")
        {
            LineWidth = 2.5f,
            Color = "#2E86C1"
        };
        revenueSeries.AddDataPoint(1.0, 100000.0);
        revenueSeries.AddDataPoint(2.0, 125000.0);
        revenueSeries.AddDataPoint(3.0, 150000.0);
        revenueSeries.AddDataPoint(4.0, 175000.0);
        
        var expensesSeries = new ChartSeries("Expenses")
        {
            LineWidth = 2.5f,
            Color = "#E74C3C"
        };
        expensesSeries.AddDataPoint(1.0, 85000.0);
        expensesSeries.AddDataPoint(2.0, 92000.0);
        expensesSeries.AddDataPoint(3.0, 98000.0);
        expensesSeries.AddDataPoint(4.0, 105000.0);
        
        chart.AddSeries(revenueSeries);
        chart.AddSeries(expensesSeries);

        // Example 1: Apply default configuration
        var configuredChart = chart.WithDefaultConfiguration();
        Console.WriteLine($"Chart configured with type: {configuredChart.Type}");

        // Example 2: Get axis bounds for the chart
        var (minX, maxX, minY, maxY) = chart.GetAxisBounds();
        Console.WriteLine($"Axis bounds - X: [{minX:F2}, {maxX:F2}], Y: [{minY:F2}, {maxY:F2}]");

        // Example 3: Count total data points
        int totalPoints = chart.GetTotalDataPoints();
        Console.WriteLine($"Total data points: {totalPoints}");

        // Example 4: Check if chart is empty
        bool isEmpty = chart.IsEmpty();
        Console.WriteLine($"Chart is empty: {isEmpty}");

        // Example 5: Apply a color palette
        var palette = new ColorPalette(new[] { "#2E86C1", "#E74C3C", "#27AE60", "#F39C12" });
        var chartWithPalette = chart.ApplyPalette(palette);
        Console.WriteLine($"Applied palette with {palette.GetColorCount()} colors");

        // Example 6: Normalize data across all series
        var normalizedChart = chart.NormalizeData(clone: true);
        Console.WriteLine("Data normalized to 0-1 range");

        // Example 7: Filter series (keep only series with high values)
        var highValueChart = chart.FilterSeries(
            series => series.DataPoints.Any(p => p.Y > 100000),
            clone: true
        );
        Console.WriteLine($"Filtered to {highValueChart.Series.Count} high-value series");

        // Example 8: Convert chart to builder for further customization
        var builder = chart.ToBuilder();
        Console.WriteLine($"Chart converted to builder with type: {builder.ChartType}");
    }
}
```

## CollectionExtensions

`CollectionExtensions` provides a set of utility extension methods for working with collections and sequences in a more convenient and expressive way. It includes methods for batching, filtering duplicates, checking for null/empty collections, getting random elements, shuffling, and various statistical operations.

```csharp
using System;
using System.Collections.Generic;
using SkiaSharpChartEngine.Extensions;

public class CollectionExtensionsExample
{
    public static void Main()
    {
        // Example 1: Batch a large collection into smaller chunks
        var numbers = Enumerable.Range(1, 25);
        var batches = numbers.Batch(5);
        
        Console.WriteLine("Batches:");
        foreach (var batch in batches)
        {
            Console.WriteLine($"  {string.Join(", ", batch)}");
        }
        // Output: Batches: 1, 2, 3, 4, 5
        //         Batches: 6, 7, 8, 9, 10
        //         ...
        
        // Example 2: Check if a collection is null or empty
        List<string>? nullList = null;
        List<string> emptyList = new();
        var listWithItems = new List<string> { "item1", "item2" };
        
        Console.WriteLine($"Null list is null or empty: {nullList.IsNullOrEmpty()}");
        Console.WriteLine($"Empty list is null or empty: {emptyList.IsNullOrEmpty()}");
        Console.WriteLine($"List with items is null or empty: {listWithItems.IsNullOrEmpty()}");
        
        // Example 3: Get first element or default value
        var emptyNumbers = new List<int>();
        int firstOrDefault = emptyNumbers.GetOrDefault(42);
        Console.WriteLine($"First or default for empty list: {firstOrDefault}");
        // Output: First or default for empty list: 42
        
        // Example 4: Shuffle a collection randomly
        var orderedNumbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        var shuffled = orderedNumbers.Shuffle();
        Console.WriteLine($"Original: {string.Join(", ", orderedNumbers)}");
        Console.WriteLine($"Shuffled: {string.Join(", ", shuffled)}");
        
        // Example 5: Get a random element from a collection
        var colors = new List<string> { "Red", "Green", "Blue", "Yellow", "Purple" };
        string randomColor = colors.GetRandom();
        Console.WriteLine($"Random color: {randomColor}");
        
        // Example 6: Find duplicate elements in a collection
        var dataWithDuplicates = new List<int> { 1, 2, 3, 2, 4, 5, 3, 6, 1 };
        var duplicates = dataWithDuplicates.FindDuplicates();
        Console.WriteLine($"Duplicates found: {string.Join(", ", duplicates)}");
        // Output: Duplicates found: 1, 2, 3
        
        // Example 7: DistinctBy - get distinct elements based on a key selector
        var people = new List<Person>
        {
            new Person("Alice", 25),
            new Person("Bob", 30),
            new Person("Alice", 28), // Same name, different age
            new Person("Charlie", 22)
        };
        
        var distinctByName = people.DistinctBy(p => p.Name);
        Console.WriteLine("People with distinct names:");
        foreach (var person in distinctByName)
        {
            Console.WriteLine($"  {person.Name} - {person.Age}");
        }
        
        // Example 8: ContainsAny - check if any element satisfies a condition
        var temperatures = new List<double> { 15.5, 18.2, 22.1, 19.8 };
        bool hasHighTemp = temperatures.ContainsAny(t => t > 20);
        Console.WriteLine($"Contains temperature > 20: {hasHighTemp}");
        
        // Example 9: SumBy - sum a specific property across all elements
        var products = new List<Product>
        {
            new Product("Laptop", 999.99m),
            new Product("Mouse", 29.99m),
            new Product("Keyboard", 79.99m)
        };
        decimal totalPrice = products.SumBy(p => p.Price);
        Console.WriteLine($"Total price: {totalPrice:C}");
        
        // Example 10: AverageOrDefault - compute average with fallback
        var measurements = new List<double> { 10.5, 12.3, 11.7, 13.1 };
        double avg = measurements.AverageOrDefault(x => x, -1);
        Console.WriteLine($"Average: {avg:F2}");
        
        // Example 11: ChunkBy - split collection by predicate
        var logEntries = new List<string> { "INFO", "DEBUG", "INFO", "WARN", "ERROR", "INFO" };
        var logChunks = logEntries.ChunkBy(entry => entry == "INFO");
        Console.WriteLine("Log chunks separated by INFO:");
        foreach (var chunk in logChunks)
        {
            Console.WriteLine($"  {string.Join(", ", chunk)}");
        }
        
        // Example 12: Interleave - merge multiple sequences alternately
        var sequence1 = new List<int> { 1, 2, 3 };
        var sequence2 = new List<int> { 4, 5, 6, 7 };
        var sequence3 = new List<int> { 8, 9 };
        
        var interleaved = CollectionExtensions.Interleave(sequence1, sequence2, sequence3);
        Console.WriteLine($"Interleaved: {string.Join(", ", interleaved)}");
        // Output: Interleaved: 1, 4, 8, 2, 5, 9, 3, 6, 7
    }
}

public class Person
{
    public string Name { get; }
    public int Age { get; }
    
    public Person(string name, int age)
    {
        Name = name;
        Age = age;
    }
}

public class Product
{
    public string Name { get; }
    public decimal Price { get; }
    
    public Product(string name, decimal price)
    {
        Name = name;
        Price = price;
    }
}
```

## AnimationSettings

`AnimationSettings` provides configuration for chart animations, controlling duration, frame rate, easing functions, and opacity transitions. It supports smooth visual transitions when charts load or update, with configurable timing curves and opacity effects for professional-looking animations.

```csharp
using System;
using SkiaSharpChartEngine.Models;

public class AnimationSettingsExample
{
    public static void Main()
    {
        // Example 1: Create a simple animation with custom duration
        var animationSettings = new AnimationSettings(durationMs: 1000) // 1 second animation
        {
            FrameRate = 30, // Lower frame rate for smoother performance
            EasingType = EasingFunction.EaseOutQuad,
            AnimateOnLoad = true,
            AnimateOnUpdate = true,
            StartOpacity = 0.3,
            EndOpacity = 1.0
        };

        Console.WriteLine($"Animation duration: {animationSettings.DurationMs}ms");
        Console.WriteLine($"Total frames: {animationSettings.GetTotalFrames()}");
        Console.WriteLine($"Configuration: {animationSettings}");

        // Example 2: Validate animation settings
        try
        {
            animationSettings.Validate();
            Console.WriteLine("✓ Animation settings are valid");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Validation failed: {ex.Message}");
        }

        // Example 3: Clone animation settings for reuse
        var clonedSettings = animationSettings.Clone();
        clonedSettings.DurationMs = 1500; // Modify cloned settings
        Console.WriteLine($"Original duration: {animationSettings.DurationMs}ms");
        Console.WriteLine($"Cloned duration: {clonedSettings.DurationMs}ms");

        // Example 4: Calculate animation progress
        int currentFrame = 15;
        double progress = animationSettings.GetProgress(currentFrame);
        Console.WriteLine($"Progress at frame {currentFrame}: {progress:P0}");

        // Example 5: Use different easing functions
        var linearAnimation = new AnimationSettings(800)
        {
            EasingType = EasingFunction.Linear
        };

        var easeInOutAnimation = new AnimationSettings(800)
        {
            EasingType = EasingFunction.EaseInOutCubic
        };

        Console.WriteLine($"Linear animation: {linearAnimation}");
        Console.WriteLine($"EaseInOutCubic animation: {easeInOutAnimation}");
    }
}
```

## DataPoint

`DataPoint` represents a single data point in a chart series. It contains the X/Y coordinates for rendering, optional label text, color styling, and extensible metadata. Data points support various states (normal, highlighted, hidden) and can be customized with custom radii and additional properties through the `Metadata` dictionary.

```csharp
using System;
using System.Collections.Generic;
using SkiaSharpChartEngine.Models;

public class DataPointExample
{
    public static void Main()
    {
        // Example 1: Create a basic data point
        var point1 = new DataPoint(1.0, 100.0);
        Console.WriteLine($"Basic point: {point1}");
        // Output: Basic point: DataPoint(X=1, Y=100, Label=)

        // Example 2: Create a data point with label and custom color
        var point2 = new DataPoint(2.0, 150.0, "Q2 2024", "#2E86C1");
        Console.WriteLine($"Styled point: {point2}");
        Console.WriteLine($"Color: {point2.Color}");
        // Output: Styled point: DataPoint(X=2, Y=150, Label=Q2 2024)
        // Output: Color: #2E86C1

        // Example 3: Create a data point with custom radius and metadata
        var point3 = new DataPoint(3.0, 200.0)
        {
            State = DataPointState.Highlighted,
            CustomRadius = 8.0,
            Metadata = new Dictionary<string, object>
            {
                ["unit"] = "USD",
                ["precision"] = 2,
                ["region"] = "North America"
            },
            Timestamp = DateTime.UtcNow
        };
        
        Console.WriteLine($"Point with metadata: X={point3.X}, Y={point3.Y}");
        Console.WriteLine($"State: {point3.State}");
        Console.WriteLine($"Custom radius: {point3.CustomRadius}");
        Console.WriteLine($"Metadata count: {point3.Metadata?.Count}");
        Console.WriteLine($"Timestamp: {point3.Timestamp?.ToString("o")}");
        
        // Example 4: Clone a data point for reuse
        var clonedPoint = point3.Clone();
        Console.WriteLine($"Cloned point is different instance: {point3 != clonedPoint}");
        Console.WriteLine($"Cloned metadata is independent: {point3.Metadata != clonedPoint.Metadata}");
        
        // Example 5: Use Value property (alias for Y)
        var point4 = new DataPoint(4.0, 250.0);
        Console.WriteLine($"Using Value property: {point4.Value}");
        // Output: Using Value property: 250
    }
}
```

## ChartModelsAndValidationTests

`ChartModelsAndValidationTests` is a comprehensive unit test suite that validates the behavior of core chart models and validation logic in the SkiaSharpChartEngine library. This test class ensures the correctness of fundamental components including `DataPoint`, `ChartSeries`, `Chart`, `ChartValidator`, `ColorHelper`, and their associated extension methods.

The tests cover:

```csharp
using System;
using System.Collections.Generic;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Utilities;
using SkiaSharpChartEngine.Extensions;

public class ChartModelsAndValidationTestsExample
{
    public static void Main()
    {
        // Example 1: Test DataPoint validation - setting X to NaN throws ArgumentException
        var point = new DataPoint(1.0, 2.0);
        try
        {
            point.X = double.NaN;
            Console.WriteLine("ERROR: Should have thrown ArgumentException");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"✓ DataPoint validation: {ex.Message}");
        }

        // Example 2: Test DataPoint cloning produces independent copy
        var original = new DataPoint(3.0, 7.5, "Q3", "#FF0000");
        original.Metadata = new Dictionary<string, object> { ["key"] = "value" };
        
        var clone = original.Clone();
        clone.X = 99.0;
        
        Console.WriteLine($"✓ Clone independent: Original.X={original.X}, Clone.X={clone.X}");

        // Example 3: Test ChartSeries data point count and Y-axis range
        var series = new ChartSeries("Revenue");
        series.AddDataPoint(1.0, 100.0);
        series.AddDataPoint(2.0, 150.0);
        series.AddDataPoint(3.0, 120.0);
        
        Console.WriteLine($"✓ Series data points: {series.GetDataPointCount()}");
        var (minY, maxY) = series.GetYAxisRange();
        Console.WriteLine($"✓ Y-axis range: [{minY}, {maxY}]");

        // Example 4: Test Chart validation
        var chart = new Chart("sales-chart");
        var validationResult = ChartValidator.ValidateChart(chart);
        Console.WriteLine($"✓ Chart validation valid: {validationResult.IsValid}");
        
        // Add a series to make it valid
        chart.AddSeries(series);
        validationResult = ChartValidator.ValidateChart(chart);
        Console.WriteLine($"✓ Chart with series valid: {validationResult.IsValid}");

        // Example 5: Test ColorHelper utilities
        string rgbRed = ColorHelper.HexToRgb("#FF0000");
        Console.WriteLine($"✓ Hex to RGB: {rgbRed}");
        
        string hexBlue = ColorHelper.RgbToHex(0, 0, 255);
        Console.WriteLine($"✓ RGB to Hex: {hexBlue}");
        
        bool isValidHex = ColorHelper.IsValidHexColor("#1F77B4");
        Console.WriteLine($"✓ Valid hex color: {isValidHex}");
        
        // Example 6: Test DataPoint extensions - offset and scale
        var dataPoint = new DataPoint(1.0, 2.0);
        var offsetPoint = dataPoint.Offset(3.0, 1.0);
        Console.WriteLine($"✓ Offset point: [{offsetPoint.X}, {offsetPoint.Y}]");
        
        var scaledPoint = dataPoint.Scale(2.0, 0.5);
        Console.WriteLine($"✓ Scaled point: [{scaledPoint.X:F2}, {scaledPoint.Y:F2}]");
        
        // Example 7: Test Euclidean distance calculation
        var pointA = new DataPoint(0.0, 0.0);
        var pointB = new DataPoint(3.0, 4.0);
        double distance = pointA.GetDistance(pointB);
        Console.WriteLine($"✓ Distance between (0,0) and (3,4): {distance:F4}");
    }
}
```

## Chart

The `Chart` class is the primary model in the SkiaSharp Chart Engine, representing a complete chart with its data series, visual configuration, metadata, and associated tags. Charts are the central data structure used throughout the rendering pipeline and can be created standalone or loaded from various data sources including JSON, CSV, or database records.

Charts support multiple chart types (line, bar, pie, etc.), multiple data series per chart, and comprehensive metadata for tracking creation/modification history, authorship, and categorization through tags.

```csharp
using System;
using System.Collections.Generic;
using SkiaSharpChartEngine.Models;

public class ChartExample
{
    public static void Main()
    {
        // Example 1: Create a basic line chart with sample data
        var lineChart = new Chart("sales-performance-2024")
        {
            Type = ChartType.LineChart,
            Title = "Quarterly Sales Performance"
        };
        
        var revenueSeries = new ChartSeries("Revenue")
        {
            LineWidth = 2.5f,
            Color = "#2E86C1"
        };
        revenueSeries.AddDataPoint(1.0, 100000.0);
        revenueSeries.AddDataPoint(2.0, 125000.0);
        revenueSeries.AddDataPoint(3.0, 150000.0);
        revenueSeries.AddDataPoint(4.0, 175000.0);
        lineChart.AddSeries(revenueSeries);
        
        Console.WriteLine($"Created chart: {lineChart.Title}");
        Console.WriteLine($"Chart ID: {lineChart.Id}");
        Console.WriteLine($"Chart Type: {lineChart.Type}");
        Console.WriteLine($"Series Count: {lineChart.GetSeriesCount()}");
        Console.WriteLine($"Total Data Points: {lineChart.GetTotalDataPoints()}");
        
        // Example 2: Create a bar chart with multiple series
        var barChart = new Chart("product-comparison", ChartType.BarChart)
        {
            Title = "Product Comparison by Region"
        };
        
        var northAmerica = new ChartSeries("North America")
        {
            Color = "#27AE60",
            BarWidth = 0.8f
        };
        northAmerica.AddDataPoint(1.0, 120000.0);
        northAmerica.AddDataPoint(2.0, 135000.0);
        
        var europe = new ChartSeries("Europe")
        {
            Color = "#E74C3C",
            BarWidth = 0.8f
        };
        europe.AddDataPoint(1.0, 95000.0);
        europe.AddDataPoint(2.0, 110000.0);
        
        barChart.AddSeries(northAmerica);
        barChart.AddSeries(europe);
        
        Console.WriteLine($"\nBar chart created with {barChart.GetSeriesCount()} series");
        
        // Example 3: Access chart metadata and tags
        var chartWithMetadata = new Chart("dashboard-template")
        {
            Type = ChartType.LineChart,
            CreatedBy = "system",
            IsTemplate = true
        };
        chartWithMetadata.Tags = new Dictionary<string, object>
        {
            ["category"] = "financial",
            ["frequency"] = "monthly",
            ["version"] = "1.0.0"
        };
        
        Console.WriteLine($"\nChart metadata:");
        Console.WriteLine($"  Created: {chartWithMetadata.CreatedAt}");
        Console.WriteLine($"  Created By: {chartWithMetadata.CreatedBy}");
        Console.WriteLine($"  Is Template: {chartWithMetadata.IsTemplate}");
        Console.WriteLine($"  Tags: {chartWithMetadata.Tags?.Count ?? 0} items");
        
        // Example 4: Manage series programmatically
        var dynamicChart = new Chart("dynamic-data");
        
        // Add multiple series
        for (int i = 0; i < 3; i++)
        {
            var series = new ChartSeries($"Series {i + 1}")
            {
                Color = $"#{(i * 50 + 100):X2}{(i * 100 + 50):X2}{(i * 150 + 200):X2}"
            };
            
            // Add 5 data points
            for (int j = 0; j < 5; j++)
            {
                series.AddDataPoint(j + 1, Random.Shared.Next(50, 200) * (i + 1));
            }
            
            dynamicChart.AddSeries(series);
        }
        
        Console.WriteLine($"\nDynamic chart created with {dynamicChart.GetSeriesCount()} series");
        Console.WriteLine($"Total data points: {dynamicChart.GetTotalDataPoints()}");
        
        // Example 5: Get chart data bounds for axis scaling
        var (minX, maxX, minY, maxY) = dynamicChart.GetDataBounds();
        Console.WriteLine($"\nChart data bounds:");
        Console.WriteLine($"  X range: [{minX:F2}, {maxX:F2}]");
        Console.WriteLine($"  Y range: [{minY:F2}, {maxY:F2}]");
        
        // Example 6: Find and remove a specific series
        var targetSeries = dynamicChart.GetSeriesByName("Series 2");
        if (targetSeries != null)
        {
            dynamicChart.RemoveSeriesByName("Series 2");
            Console.WriteLine($"\nRemoved 'Series 2', remaining series: {dynamicChart.GetSeriesCount()}");
        }
        
        // Example 7: Validate chart before rendering
        bool isValid = dynamicChart.ValidateForRendering();
        Console.WriteLine($"Chart validation: {(isValid ? "Valid" : "Invalid")}");
        
        // Example 8: Clone a chart for modification
        var clonedChart = dynamicChart.Clone();
        Console.WriteLine($"\nCloned chart - Original: {dynamicChart.GetTotalDataPoints()} points, Clone: {clonedChart.GetTotalDataPoints()} points");
        
        // Example 9: Clear all series
        var emptyChart = new Chart("empty-chart");
        Console.WriteLine($"\nEmpty chart series count: {emptyChart.GetSeriesCount()}");
        
        // Example 10: String representation
        Console.WriteLine($"\nChart ToString: {lineChart}");
    }
}
```

## ChartDataServiceTests

`ChartDataServiceTests` provides a comprehensive suite of unit tests for the `ChartDataService` class, which validates chart configurations, transforms and normalizes chart data, filters data points, and calculates axis ranges. The tests cover validation scenarios (null charts, empty series, invalid line widths), data transformation operations, normalization to [0, 1] intervals, and axis range calculations for both linear and logarithmic scales.

```csharp
using System;
using System.Collections.Generic;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

public class ChartDataServiceExample
{
    public static void Main()
    {
        // Initialize ChartDataService with logger
        var logger = new NullLogger<ChartDataService>();
        var chartDataService = new ChartDataService(logger);

        // Example 1: Validate a chart before processing
        var chart = new Chart("sales-performance");
        var revenueSeries = new ChartSeries("Revenue")
        {
            LineWidth = 2.0f,
            Color = "#2E86C1"
        };
        revenueSeries.AddDataPoint(1.0, 1000.0);
        revenueSeries.AddDataPoint(2.0, 1500.0);
        revenueSeries.AddDataPoint(3.0, 1200.0);
        chart.AddSeries(revenueSeries);

        // Validate chart configuration
        try
        {
            chartDataService.ValidateChart(chart);
            Console.WriteLine("✓ Chart validation passed");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Chart validation failed: {ex.Message}");
        }

        // Example 2: Calculate axis range for linear scale
        var yValues = new double[] { 1000.0, 1500.0, 1200.0, 800.0 };
        var (minValue, maxValue) = chartDataService.CalculateAxisRange(yValues, AxisScaleType.Linear);
        Console.WriteLine($"Linear axis range: [{minValue}, {maxValue}]");
        // Output: Linear axis range: [800, 1500]

        // Example 3: Calculate axis range for logarithmic scale
        var logValues = new double[] { 0.1, 1.0, 10.0, 100.0 };
        var (logMin, logMax) = chartDataService.CalculateAxisRange(logValues, AxisScaleType.Logarithmic);
        Console.WriteLine($"Logarithmic axis range: [{logMin}, {logMax}]");
        // Output: Logarithmic axis range: [1, 100]

        // Example 4: Filter data points with positive Y values
        var allPoints = new List<DataPoint>
        {
            new DataPoint(1.0, 1000.0),
            new DataPoint(2.0, -200.0),
            new DataPoint(3.0, 1500.0),
            new DataPoint(4.0, -500.0),
            new DataPoint(5.0, 1200.0)
        };
        var filteredPoints = chartDataService.FilterDataPoints(allPoints, p => p.Y > 0);
        Console.WriteLine($"Filtered points count: {filteredPoints.Count}");
        // Output: Filtered points count: 3

        // Example 5: Transform chart data (double all Y values)
        var transformedChart = chartDataService.TransformChartData(chart, 
            p => new DataPoint(p.X, p.Y * 2));
        Console.WriteLine($"Original first point Y: {chart.Series[0].DataPoints[0].Y}");
        Console.WriteLine($"Transformed first point Y: {transformedChart.Series[0].DataPoints[0].Y}");
        // Output: Original first point Y: 1000
        //         Transformed first point Y: 2000

        // Example 6: Normalize data points to [0, 1] range
        var normalizationPoints = new List<DataPoint>
        {
            new DataPoint(0.0, 100.0),
            new DataPoint(50.0, 500.0),
            new DataPoint(100.0, 900.0)
        };
        chartDataService.NormalizeDataPoints(normalizationPoints);
        Console.WriteLine($"Normalized first point: [{normalizationPoints[0].X:F4}, {normalizationPoints[0].Y:F4}]");
        Console.WriteLine($"Normalized last point: [{normalizationPoints[2].X:F4}, {normalizationPoints[2].Y:F4}]");
        // Output: Normalized first point: [0.0000, 0.0000]
        //         Normalized last point: [1.0000, 1.0000]
    }
}
```

## RenderCacheServiceTests

`RenderCacheServiceTests` provides a comprehensive suite of unit tests for the `RenderCacheService` class, which implements a thread-safe, LRU (Least Recently Used) cache for storing rendered chart images. The tests validate null safety, cache operations (get/set/remove/clear), cache eviction policies when maximum size is reached, and statistics tracking. The test suite covers edge cases including null keys, empty keys, non-existent keys, and various cache management scenarios.

```csharp
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SkiaSharpChartEngine.Services;

public class RenderCacheServiceTestsExample
{
  public static void Main()
  {
    // Initialize logger and cache service with small size for demonstration
    var logger = new NullLogger<RenderCacheService>();
    var cacheService = new RenderCacheService(logger, maxCacheSize: 5);

    // Example 1: Store and retrieve cached render results
    byte[] chartImageData = new byte[1024]; // Simulated chart image data
    cacheService.Set("chart-2024-q1", chartImageData);
    
    var cachedData = cacheService.Get("chart-2024-q1");
    Console.WriteLine($"Cache contains 'chart-2024-q1': {cachedData != null}");
    // Output: Cache contains 'chart-2024-q1': True

    // Example 2: Handle null keys gracefully
    try
    {
      var nullResult = cacheService.Get(null!);
      Console.WriteLine("✓ Get with null key handled gracefully");
    }
    catch
    {
      Console.WriteLine("✗ Get with null key threw exception");
    }

    // Example 3: Handle empty keys gracefully
    try
    {
      var emptyResult = cacheService.Get(" ");
      Console.WriteLine("✓ Get with empty key handled gracefully");
    }
    catch
    {
      Console.WriteLine("✗ Get with empty key threw exception");
    }

    // Example 4: Check cache contains key
    cacheService.Set("chart-2024-q2", new byte[512]);
    bool containsKey = cacheService.Contains("chart-2024-q2");
    Console.WriteLine($"Cache contains 'chart-2024-q2': {containsKey}");
    // Output: Cache contains 'chart-2024-q2': True

    // Example 5: Remove entry from cache
    cacheService.Remove("chart-2024-q2");
    bool removed = !cacheService.Contains("chart-2024-q2");
    Console.WriteLine($"Entry removed successfully: {removed}");
    // Output: Entry removed successfully: True

    // Example 6: Clear entire cache
    cacheService.Set("chart-1", new byte[256]);
    cacheService.Set("chart-2");
    cacheService.Set("chart-3", new byte[256]);
    
    int sizeBeforeClear = cacheService.GetCacheSize();
    cacheService.Clear();
    int sizeAfterClear = cacheService.GetCacheSize();
    
    Console.WriteLine($"Cache size before clear: {sizeBeforeClear}");
    Console.WriteLine($"Cache size after clear: {sizeAfterClear}");
    // Output: Cache size before clear: 3
    // Output: Cache size after clear: 0

    // Example 7: Cache eviction when maximum size is reached
    for (int i = 1; i <= 6; i++)
    {
      cacheService.Set($"chart-{i}", new byte[256]);
    }
    
    // The cache should have evicted the least recently used entry (chart-1)
    bool hasChart1 = cacheService.Contains("chart-1");
    bool hasChart6 = cacheService.Contains("chart-6");
    
    Console.WriteLine($"Cache contains chart-1 (should be false): {hasChart1}");
    Console.WriteLine($"Cache contains chart-6 (should be true): {hasChart6}");
    // Output: Cache contains chart-1 (should be false): False
    // Output: Cache contains chart-6 (should be true): True

    // Example 8: Get all cached keys
    cacheService.Set("chart-a", new byte[128]);
    cacheService.Set("chart-b", new byte[128]);
    cacheService.Set("chart-c", new byte[128]);
    
    var allKeys = cacheService.GetAllKeys();
    Console.WriteLine($"Total cached entries: {allKeys.Count()}");
    Console.WriteLine($"Keys: {string.Join(", ", allKeys)}");
    // Output: Total cached entries: 3
    // Output: Keys: chart-a, chart-b, chart-c
  }
}
```

## RenderResult

`RenderResult` represents the outcome of a chart rendering operation. It encapsulates success/failure status, rendered image data, file system information, timing metrics, error details, and custom metadata. This type is returned by all rendering methods and pipeline stages to provide comprehensive feedback about chart generation operations.

```csharp
using System;
using System.Collections.Generic;
using SkiaSharpChartEngine.Models;

public class RenderResultExample
{
    public static void Main()
    {
        // Example 1: Successful rendering with in-memory image data
        var imageData = new byte[1024 * 1024]; // 1MB of chart image data
        var successResult = RenderResult.CreateSuccess(
            chartId: "sales-dashboard-2024",
            imageData: imageData,
            renderTimeMs: 150,
            format: ExportFormat.Png
        );
        
        Console.WriteLine(successResult);
        Console.WriteLine($"Chart: {successResult.ChartId}");
        Console.WriteLine($"Success: {successResult.Success}");
        Console.WriteLine($"Image size: {successResult.ImageData?.Length} bytes");
        Console.WriteLine($"File size: {successResult.FileSizeBytes} bytes");
        Console.WriteLine($"Format: {successResult.ExportFormat}");
        Console.WriteLine($"Rendered at: {successResult.RenderedAt}");
        Console.WriteLine($"Render time: {successResult.RenderTimeMs}ms");
        
        // Example 2: Successful rendering with file output
        var fileResult = RenderResult.CreateSuccess(
            chartId: "quarterly-report",
            outputPath: "/tmp/quarterly-report.png",
            renderTimeMs: 210,
            format: ExportFormat.Png
        );
        
        Console.WriteLine($"\nFile output path: {fileResult.OutputPath}");
        Console.WriteLine($"File exists: {fileResult.ImageData != null}");
        
        // Example 3: Failed rendering with error details
        var errorResult = RenderResult.CreateFailure(
            chartId: "problem-chart",
            errorMessage: "Invalid chart configuration: series has no data points",
            exception: new InvalidOperationException("Series validation failed")
        );
        
        Console.WriteLine($"\nFailed render - Error: {errorResult.ErrorMessage}");
        Console.WriteLine($"Exception: {errorResult.Exception?.Message}");
        Console.WriteLine($"Success: {errorResult.Success}");
        
        // Example 4: Using custom metadata
        var metadataResult = new RenderResult("custom-chart", true)
        {
            RenderTimeMs = 85,
            ExportFormat = ExportFormat.Svg,
            Metadata = new Dictionary<string, object>
            {
                ["renderEngine"] = "SkiaSharp",
                ["quality"] = "high",
                ["dpi"] = 300,
                ["chartType"] = "line"
            }
        };
        
        Console.WriteLine($"\nMetadata count: {metadataResult.Metadata?.Count}");
        Console.WriteLine($"Render engine: {metadataResult.Metadata?["renderEngine"]}");
        
        // Example 5: Checking render time with both properties
        var result = new RenderResult("timing-test", true)
        {
            RenderTimeMs = 125
        };
        
        Console.WriteLine($"\nRender time (ms): {result.RenderTimeMs}");
        Console.WriteLine($"Render time (milliseconds): {result.RenderTimeMilliseconds}");
    }
}
```

## ExportServiceTests

`ExportServiceTests` provides a comprehensive suite of unit tests for the `ExportService` class, which handles chart export functionality including both synchronous and asynchronous export operations. The tests validate null argument validation, unsupported format handling, successful rendering delegation, error handling for infrastructure issues, cancellation support, and constructor dependency validation for the export service.

The test suite covers:
- **Async export methods**: Validating `ExportAsync` with null checks, unsupported formats, successful rendering delegation, failure scenarios, infrastructure errors, and cancellation support
- **Sync export methods**: Testing `Export` with null argument validation and successful rendering delegation
- **Format support**: Verifying `SupportsFormat` returns true for supported formats (PNG, SVG, JPEG, WEBP) and false for unsupported formats
- **Supported formats list**: Ensuring `GetSupportedFormats` returns all four supported formats in ascending order
- **Constructor validation**: Ensuring proper dependency injection validation for rendering service and logger

```csharp
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

public class ExportServiceTestsExample
{
    public static void Main()
    {
        // Initialize ExportService with dependencies
        var logger = new NullLogger<ExportService>();
        var renderingService = new ChartRenderingService(
            logger,
            new ChartDataService(logger),
            new RenderCacheService()
        );
        var exportService = new ExportService(renderingService, logger);

        // Create a test chart with data
        var chart = new Chart("sales-chart");
        var series = new ChartSeries("Revenue");
        series.AddDataPoint(1.0, 1000.0);
        series.AddDataPoint(2.0, 1500.0);
        series.AddDataPoint(3.0, 1200.0);
        chart.AddSeries(series);

        // Example 1: Check if PNG format is supported
        bool supportsPng = exportService.SupportsFormat(ExportFormat.PNG);
        Console.WriteLine($"PNG format supported: {supportsPng}");
        // Output: PNG format supported: True

        // Example 2: Get all supported formats
        var supportedFormats = exportService.GetSupportedFormats();
        Console.WriteLine($"Supported formats: {string.Join(", ", supportedFormats)}");
        // Output: Supported formats: JPEG, PNG, SVG, WEBP

        // Example 3: Export chart to file (async)
        var exportOptions = new ExportOptions
        {
            Format = ExportFormat.PNG,
            DirectoryPath = Path.GetTempPath(),
            FileName = "chart-export.png"
        };
        
        var asyncResult = await exportService.ExportAsync(chart, exportOptions);
        Console.WriteLine($"Async export success: {asyncResult.Success}");
        Console.WriteLine($"Error message: {asyncResult.ErrorMessage ?? "None"}");
        
        // Example 4: Export chart to file (sync)
        var syncResult = exportService.Export(chart, exportOptions);
        Console.WriteLine($"Sync export success: {syncResult.Success}");
        
        // Example 5: Export to SVG format
        var svgOptions = new ExportOptions
        {
            Format = ExportFormat.SVG,
            DirectoryPath = Path.GetTempPath(),
            FileName = "chart-export.svg"
        };
        
        var svgResult = await exportService.ExportAsync(chart, svgOptions);
        Console.WriteLine($"SVG export success: {svgResult.Success}");
        
        // Example 6: Handle unsupported format
        try
        {
            var invalidOptions = new ExportOptions { Format = (ExportFormat)999 };
            var invalidResult = exportService.Export(chart, invalidOptions);
            Console.WriteLine("ERROR: Should have thrown UnsupportedExportFormatException");
        }
        catch (UnsupportedExportFormatException ex)
        {
            Console.WriteLine($"✓ Correctly threw UnsupportedExportFormatException: {ex.Message}");
        }
    }
}
```

## ChartInteractionServiceTests

`ChartInteractionServiceTests` provides a comprehensive suite of unit tests for the `ChartInteractionService` class, which handles user interactions with chart elements such as clicks, hovers, selections, and context menu gestures. The tests validate edge cases including null inputs, missed interactions, and proper event raising, ensuring the service correctly processes user interactions and maintains selection state.

## ChartRenderingServiceTests

`ChartRenderingServiceTests` provides a comprehensive suite of unit tests for the `ChartRenderingService` class, which handles synchronous and asynchronous chart rendering to byte arrays and files in various image formats (PNG, SVG, JPEG, WebP). The tests validate null argument validation, caching behavior, file system operations, cancellation support, and constructor dependency validation for the chart rendering service.

The test suite covers:
- **Async rendering methods**: Validating `RenderToByteArrayAsync`, `RenderToFileAsync`, and `RenderWithExportAsync` with null checks, caching behavior, and cancellation support
- **Sync rendering methods**: Testing `RenderToByteArray` and `RenderToFile` with null argument validation
- **Cache pre-warming**: Verifying `PrewarmCache` populates the render cache correctly
- **Constructor validation**: Ensuring proper dependency injection validation for logger, data service, and cache service

```csharp
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

public class ChartRenderingServiceExample
{
    public static void Main()
    {
        // Initialize ChartRenderingService with dependencies
        var logger = new NullLogger<ChartRenderingService>();
        var dataService = new ChartDataService(logger);
        var cacheService = new RenderCacheService();
        var renderingService = new ChartRenderingService(logger, dataService, cacheService);

        // Create a simple line chart with data
        var chart = new Chart("sales-chart");
        var series = new ChartSeries("Revenue");
        series.AddDataPoint(1.0, 1000.0);
        series.AddDataPoint(2.0, 1500.0);
        series.AddDataPoint(3.0, 1200.0);
        chart.AddSeries(series);

        // Example 1: Render chart to byte array (async)
        var renderResult = await renderingService.RenderToByteArrayAsync(chart);
        Console.WriteLine($"Async render success: {renderResult.Success}");
        Console.WriteLine($"Render time: {renderResult.RenderTimeMilliseconds}ms");
        Console.WriteLine($"Image data size: {renderResult.ImageData?.Length ?? 0} bytes");

        // Example 2: Render chart to file (async)
        var filePath = Path.Combine(Path.GetTempPath(), "chart-example.png");
        var fileResult = await renderingService.RenderToFileAsync(chart, filePath);
        Console.WriteLine($"Async file render success: {fileResult.Success}");
        Console.WriteLine($"File exists: {File.Exists(filePath)}");

        // Example 3: Export with custom options (async)
        var exportOptions = new ExportOptions
        {
            Format = ExportFormat.SVG,
            DirectoryPath = Path.GetTempPath(),
            FileName = "chart-example.svg"
        };
        var exportResult = await renderingService.RenderWithExportAsync(chart, exportOptions);
        Console.WriteLine($"Export render success: {exportResult.Success}");

        // Example 4: Synchronous rendering
        var syncResult = renderingService.RenderToByteArray(chart);
        Console.WriteLine($"Sync render success: {syncResult.Success}");
        Console.WriteLine($"Sync image data size: {syncResult.ImageData?.Length ?? 0} bytes");

        // Example 5: Pre-warm the cache for faster subsequent renders
        renderingService.PrewarmCache(chart);
        Console.WriteLine("Cache pre-warmed");

        // Cleanup
        if (File.Exists(filePath)) File.Delete(filePath);
        if (File.Exists(exportOptions.FullPath)) File.Delete(exportOptions.FullPath);
    }
}
```

## ChartStreamingServiceTests

`ChartStreamingServiceTests` provides a comprehensive suite of unit tests for the `ChartStreamingService` class, which handles real-time data streaming for charts. The service maintains a buffer of streaming data points, applies windowing to limit data retention, and provides thread-safe snapshot generation for rendering. Tests cover registration, publishing, batch operations, window size enforcement, auto-series creation, and asynchronous flushing operations.

## WebhookHandler

`WebhookHandler` handles webhook subscriptions and deliveries, allowing external services to be notified of chart events. It implements the `IChartEventSubscriber` interface to receive chart events and deliver them via HTTP POST to registered webhook URLs.

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SkiaSharpChartEngine.Integration;
using SkiaSharpChartEngine.Events;

public class WebhookHandlerExample
{
    public static async Task Main(string[] args)
    {
        // Initialize webhook handler with logger
        var logger = new NullLogger<WebhookHandler>();
        var webhookHandler = new WebhookHandler(logger);

        // Register a webhook for chart created events
        string subscriptionId = webhookHandler.RegisterWebhook(
            "chart.created",
            "https://example.com/webhooks/chart-created",
            new WebhookOptions { Headers = new Dictionary<string, string> { { "Authorization", "Bearer token123" } } }
        );

        Console.WriteLine($"Registered webhook with ID: {subscriptionId}");

        // Get all subscriptions
        List<WebhookSubscription> subscriptions = webhookHandler.GetSubscriptions();
        Console.WriteLine($"Total subscriptions: {subscriptions.Count}");

        // Simulate a chart created event
        var chartCreatedEvent = new ChartCreatedEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            "chart-123"
        );

        await webhookHandler.OnChartCreatedAsync(chartCreatedEvent);
        Console.WriteLine("Chart created event processed and webhook delivered");

        // Check subscription details
        var subscription = subscriptions.Find(s => s.Id == subscriptionId);
        if (subscription != null)
        {
            Console.WriteLine($"Subscription status - Active: {subscription.IsActive}, Healthy: {subscription.IsHealthy}");
            Console.WriteLine($"Delivery count: {subscription.DeliveryCount}");
            Console.WriteLine($"Last delivery: {subscription.LastDeliveryAt}");
        }

        // Unregister the webhook
        bool unregistered = webhookHandler.UnregisterWebhook(subscriptionId);
        Console.WriteLine($"Webhook unregistered: {unregistered}");
    }
}
```

```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Services;
using SkiaSharpChartEngine.Streaming;

public class ChartStreamingServiceExample
{
    public static void Main()
    {
        // Initialize streaming service with rendering service
        var renderingService = new ChartRenderingService();
        var streamingService = new ChartStreamingService(renderingService);

        // Create a chart for streaming
        var chart = new Chart("temperature-stream");
        chart.AddSeries(new ChartSeries("Temperature"));
        chart.AddSeries(new ChartSeries("Humidity"));

        // Register the chart with default options
        streamingService.Register(chart);

        // Publish individual data points
        streamingService.Publish(chart.Id, new StreamDataPoint { SeriesName = "Temperature", X = 1, Y = 23.5 });
        streamingService.Publish(chart.Id, new StreamDataPoint { SeriesName = "Temperature", X = 2, Y = 24.1 });
        streamingService.Publish(chart.Id, new StreamDataPoint { SeriesName = "Humidity", X = 1, Y = 45.0 });

        // Publish batch of points
        var humidityPoints = Enumerable.Range(1, 10).Select(i => 
            new StreamDataPoint { SeriesName = "Humidity", X = i, Y = 35.0 + i * 2.5 });
        streamingService.PublishBatch(chart.Id, humidityPoints);

        // Get current snapshot for rendering
        var snapshot = streamingService.GetSnapshot(chart.Id);
        Console.WriteLine($"Chart {snapshot.Id} has {snapshot.Series.Count} series");
        Console.WriteLine($"Temperature points: {snapshot.GetSeriesByName("Temperature")?.GetDataPointCount()}");
        Console.WriteLine($"Humidity points: {snapshot.GetSeriesByName("Humidity")?.GetDataPointCount()}");

        // Configure window size to limit data retention
        var options = new StreamingChartOptions { WindowSize = 5 };
        streamingService.Register(chart, options);

        // Publish more points - oldest will be dropped when window exceeds size
        for (int i = 1; i <= 10; i++)
        {
            streamingService.Publish(chart.Id, new StreamDataPoint { SeriesName = "Temperature", X = i + 10, Y = 20.0 + i });
        }

        // Get updated snapshot
        var finalSnapshot = streamingService.GetSnapshot(chart.Id);
        Console.WriteLine($"After windowing, Temperature points: {finalSnapshot.GetSeriesByName("Temperature")?.GetDataPointCount()}");

        // Flush buffered points asynchronously
        var flushedCount = await streamingService.FlushAsync(chart.Id);
        Console.WriteLine($"Flushed {flushedCount} buffered points");

        // Unregister when chart is no longer needed
        streamingService.Unregister(chart.Id);
    }
}
```

## BatchProcessor

`BatchProcessor` processes items in batches, providing an efficient way to handle bulk operations with configurable batch sizing and timeout settings. It is designed to manage large collections by breaking them into manageable chunks, ensuring timely processing, and tracking metrics such as total items, processed items, failed items, and elapsed time.

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using SkiaSharpChartEngine.Utilities;

public class BatchProcessorExample
{
    public static async Task Main()
    {
        var logger = new NullLogger<BatchProcessor<int>>();
        var items = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

        // Define the batch processing logic
        Func<List<int>, Task> processor = async (batch) =>
        {
            Console.WriteLine($"Processing batch of {batch.Count} items...");
            await Task.Delay(100); // Simulate work
        };

        // Create processor with batch size of 3 and 1000ms timeout
        var batchProcessor = new BatchProcessor<int>(3, 1000, processor, logger);

        // Process items
        await batchProcessor.ProcessAsync(items);

        // Example: Process a single item
        await batchProcessor.ProcessSingleAsync(11);

        Console.WriteLine("Batch processing finished.");
    }
}
```

## RenderResultCacheTests

`RenderResultCacheTests` provides a comprehensive suite of unit tests for the `RenderResultCache` class, which implements a thread-safe, time-based cache for storing chart rendering results. The cache supports configurable maximum size limits, time-to-live (TTL) expiration, LRU (Least Recently Used) eviction policy, and detailed statistics tracking. Tests cover null safety, cache operations (add/get/remove/clear), TTL expiration, size limits, thread safety, and statistics reporting.

```csharp
using System;
using SkiaSharpChartEngine.Caching;
using SkiaSharpChartEngine.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

public class RenderResultCacheExample
{
    public static void Main()
    {
        // Initialize cache with logger and maximum size (100 MB default)
        var logger = new NullLogger<RenderResultCache>();
        var cache = new RenderResultCache(logger, maxSizeBytes: 104_857_600);

        // Create a test render result
        var renderResult = RenderResult.CreateSuccess(
            "line-chart-001",
            new byte[1024], // Image data
            renderTimeMilliseconds: 42,
            ExportFormat.PNG
        );

        // Example 1: Cache a render result with default TTL (1 hour)
        cache.Cache("render-key-001", renderResult);
        Console.WriteLine("Cached render result successfully");

        // Example 2: Retrieve cached result
        var cachedResult = cache.Get("render-key-001");
        if (cachedResult != null)
        {
            Console.WriteLine($"Retrieved cached result: ChartId={cachedResult.ChartId}, Size={cachedResult.ImageData?.Length} bytes");
        }

        // Example 3: Cache with custom TTL (5 minutes)
        var customTtl = TimeSpan.FromMinutes(5);
        cache.Cache("render-key-002", renderResult, customTtl);
        Console.WriteLine("Cached with 5-minute TTL");

        // Example 4: Remove a cached entry
        bool removed = cache.Remove("render-key-001");
        Console.WriteLine($"Entry removed: {removed}");

        // Example 5: Clear all cached entries
        cache.Clear();
        Console.WriteLine("Cache cleared");

        // Example 6: Get cache statistics
        var stats = cache.GetStatistics();
        Console.WriteLine($"Cache stats - Entries: {stats.TotalEntries}, Size: {stats.TotalSize} bytes, " +
                        $"Hits: {stats.TotalHits}, MaxSize: {stats.MaxSize} bytes");
    }
}
```

```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Services;
using SkiaSharpChartEngine.Streaming;

public class ChartStreamingServiceExample
{
    public static void Main()
    {
        // Initialize streaming service with rendering service
        var renderingService = new ChartRenderingService();
        var streamingService = new ChartStreamingService(renderingService);

        // Create a chart for streaming
        var chart = new Chart("temperature-stream");
        chart.AddSeries(new ChartSeries("Temperature"));
        chart.AddSeries(new ChartSeries("Humidity"));

        // Register the chart with default options
        streamingService.Register(chart);

        // Publish individual data points
        streamingService.Publish(chart.Id, new StreamDataPoint { SeriesName = "Temperature", X = 1, Y = 23.5 });
        streamingService.Publish(chart.Id, new StreamDataPoint { SeriesName = "Temperature", X = 2, Y = 24.1 });
        streamingService.Publish(chart.Id, new StreamDataPoint { SeriesName = "Humidity", X = 1, Y = 45.0 });

        // Publish batch of points
        var humidityPoints = Enumerable.Range(1, 10).Select(i =>
            new StreamDataPoint { SeriesName = "Humidity", X = i, Y = 35.0 + i * 2.5 });
        streamingService.PublishBatch(chart.Id, humidityPoints);

        // Get current snapshot for rendering
        var snapshot = streamingService.GetSnapshot(chart.Id);
        Console.WriteLine($"Chart {snapshot.Id} has {snapshot.Series.Count} series");
        Console.WriteLine($"Temperature points: {snapshot.GetSeriesByName("Temperature")?.GetDataPointCount()}");
        Console.WriteLine($"Humidity points: {snapshot.GetSeriesByName("Humidity")?.GetDataPointCount()}");

        // Configure window size to limit data retention
        var options = new StreamingChartOptions { WindowSize = 5 };
        streamingService.Register(chart, options);

        // Publish more points - oldest will be dropped when window exceeds size
        for (int i = 1; i <= 10; i++)
        {
            streamingService.Publish(chart.Id, new StreamDataPoint { SeriesName = "Temperature", X = i + 10, Y = 20.0 + i });
        }

        // Get updated snapshot
        var finalSnapshot = streamingService.GetSnapshot(chart.Id);
        Console.WriteLine($"After windowing, Temperature points: {finalSnapshot.GetSeriesByName("Temperature")?.GetDataPointCount()}");

        // Flush buffered points asynchronously
        var flushedCount = await streamingService.FlushAsync(chart.Id);
        Console.WriteLine($"Flushed {flushedCount} buffered points");

        // Unregister when chart is no longer needed
        streamingService.Unregister(chart.Id);
    }
}
```

```csharp
using System;
using System.Threading.Tasks;
using Xunit;
using SkiaSharpChartEngine;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

public class ChartInteractionServiceTestsExample
{
    public static void Main()
    {
        // Initialize the service with mock dependencies
        var interactivityService = new Mock<IInteractivityService>();
        var logger = new NullLogger<ChartInteractionService>();
        var interactionService = new ChartInteractionService(interactivityService.Object, logger);

        // Create a test chart with a single series
        var chart = new Chart("test-chart");
        var series = new ChartSeries("Sales Data");
        series.AddDataPoint(1.0, 100.0);
        series.AddDataPoint(2.0, 150.0);
        series.AddDataPoint(3.0, 200.0);
        chart.AddSeries(series);

        // Test 1: ProcessInteraction with null chart throws ArgumentNullException
        try
        {
            interactionService.ProcessInteraction(null!, ChartInteractionType.Click, 0, 0, 800, 600);
            Console.WriteLine("ERROR: Should have thrown ArgumentNullException");
        }
        catch (ArgumentNullException ex)
        {
            Console.WriteLine($"✓ Correctly threw ArgumentNullException: {ex.ParamName}");
        }

        // Test 2: ProcessInteraction_ClickOnDataPoint_RaisesClickedEvent
        var hitResult = new TooltipHitResult
        {
            IsHit = true,
            DataPoint = series.DataPoints[0],
            Series = series,
            SeriesIndex = 0,
            Region = ChartRegion.PlotArea,
            TooltipText = "x=1, y=100"
        };
        interactivityService.Setup(s => s.HitTest(chart, It.IsAny<float>(), It.IsAny<float>(),
            It.IsAny<float>(), It.IsAny<float>(), null, null)).Returns(hitResult);

        ChartInteractionEventArgs? clickEventArgs = null;
        interactionService.Clicked += (_, e) => clickEventArgs = e;

        var clickResult = interactionService.ProcessInteraction(
            chart, ChartInteractionType.Click, 100, 200, 800, 600);

        Console.WriteLine($"✓ Click interaction processed: Type={clickResult.InteractionType}, " +
            $"HasDataPoint={clickResult.HitDataPoint != null}");

        // Test 3: ProcessInteraction_HoverMiss_ReturnsNoHit
        interactivityService.Setup(s => s.HitTest(chart, It.IsAny<float>(), It.IsAny<float>(),
            It.IsAny<float>(), It.IsAny<float>(), null, null)).Returns(TooltipHitResult.Miss);

        var hoverResult = interactionService.ProcessInteraction(
            chart, ChartInteractionType.Hover, 0, 0, 800, 600);

        Console.WriteLine($"✓ Hover miss handled correctly: SeriesIndex={hoverResult.SeriesIndex}");

        // Test 4: ToggleSelection_HitDataPoint_SelectsAndRaisesEvent
        interactivityService.Setup(s => s.HitTest(chart, It.IsAny<float>(), It.IsAny<float>(),
            It.IsAny<float>(), It.IsAny<float>(), null, null)).Returns(hitResult);

        ChartSelectionChangedEventArgs? selectionArgs = null;
        interactionService.SelectionChanged += (_, e) => selectionArgs = e;

        var selected = interactionService.ToggleSelection(chart, 100, 200, 800, 600);
        Console.WriteLine($"✓ Selection toggled: Success={selected}, " +
            $"TotalSelected={selectionArgs?.TotalSelected}");

        // Test 5: ToggleSelection_SamePointTwice_DeselectionRemovesPoint
        interactionService.ToggleSelection(chart, 100, 200, 800, 600); // Deselect
        var selection = interactionService.GetSelection(chart);
        var totalPoints = selection.Values.Sum(pts => pts.Count);
        Console.WriteLine($"✓ Double toggle deselects: TotalPoints={totalPoints}");

        // Test 6: ClearSelection_AfterSelect_EmptiesSelection
        interactionService.ClearSelection(chart);
        var clearedSelection = interactionService.GetSelection(chart);
        Console.WriteLine($"✓ Selection cleared: IsEmpty={clearedSelection.IsEmpty}");

        // Test 7: ProcessInteractionAsync_WithCancellation_ThrowsOperationCancelled
        var cts = new CancellationTokenSource();
        cts.Cancel();
        
        try
        {
            await interactionService.ProcessInteractionAsync(
                chart, ChartInteractionType.Hover, 0, 0, 800, 600, cts.Token);
            Console.WriteLine("ERROR: Should have thrown OperationCanceledException");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("✓ Async cancellation handled correctly");
        }
    }
}
```


`ChartInteractionEventArgs` provides the event data for user interactions with chart elements such as clicks, hovers, selections, and context menu gestures. It contains detailed information about the interaction including the pointer position, the chart region affected, and any data points or series that were hit during the interaction.

```csharp
using System;
using System.Collections.Generic;
using SkiaSharpChartEngine.Events;
using SkiaSharpChartEngine.Models;

public class ChartInteractionHandler
{
    private readonly IChartInteractionService _interactionService;

    public ChartInteractionHandler(IChartInteractionService interactionService)
    {
        _interactionService = interactionService;
        _interactionService.Interaction += OnChartInteraction;
    }

    private void OnChartInteraction(object? sender, ChartInteractionEventArgs e)
    {
        Console.WriteLine($"Interaction at ({e.PointerX}, {e.PointerY})");
        Console.WriteLine($"Type: {e.InteractionType}");
        Console.WriteLine($"Region: {e.Region}");
        Console.WriteLine($"Series: {e.HitSeries?.Name ?? "None"}");
        Console.WriteLine($"DataPoint: {e.HitDataPoint?.Value ?? 0}");
        Console.WriteLine($"Tooltip: {e.TooltipText}");
        Console.WriteLine($"Timestamp: {e.Timestamp:O}");

        // Add custom metadata
        e.Metadata["userId"] = "user-123";
        e.Metadata["sessionId"] = Guid.NewGuid().ToString();

        // Handle selection-based interactions
        if (e.InteractionType == ChartInteractionType.Select && e.HitDataPoint != null)
        {
            Console.WriteLine($"Selected point: Series={e.SeriesIndex}, Value={e.HitDataPoint.Value}");
        }
    }
}

## DataAggregator

`DataAggregator` provides functionality for aggregating and analyzing data points in chart rendering scenarios. It supports bucket-based aggregation (grouping data into buckets), interval-based grouping (grouping by labels), and comprehensive statistical calculations including sum, average, min, max, median, standard deviation, and more.

The class is designed to process large datasets efficiently by reducing data volume while preserving key statistical properties for visualization purposes.

### Usage Example

```csharp
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Utilities;

// Initialize DataAggregator with logger
var logger = new NullLogger<DataAggregator>();
var aggregator = new DataAggregator(logger);

// Create sample data points
var dataPoints = new List<DataPoint>
{
    new DataPoint(1.0, 10.0),
    new DataPoint(2.0, 20.0),
    new DataPoint(3.0, 30.0),
    new DataPoint(4.0, 40.0),
    new DataPoint(5.0, 50.0)
};

// Example 1: Aggregate by count (bucket-based aggregation)
var aggregatedByCount = aggregator.AggregateByCount(dataPoints, 2, AggregationType.Average);
Console.WriteLine($"Aggregated into {aggregatedByCount.Count} buckets");
Console.WriteLine($"Bucket 1: {aggregatedByCount[0].Value:F2}"); // 30.00
Console.WriteLine($"Bucket 2: {aggregatedByCount[1].Value:F2}"); // 40.00

// Example 2: Group by interval (label-based aggregation)
var labeledPoints = new List<DataPoint>
{
    new DataPoint(1.0, 100.0) { Label = "Q1" },
    new DataPoint(2.0, 150.0) { Label = "Q1" },
    new DataPoint(3.0, 200.0) { Label = "Q2" },
    new DataPoint(4.0, 250.0) { Label = "Q2" }
};

var groupedByInterval = aggregator.AggregateByInterval(labeledPoints, AggregationType.Average);
Console.WriteLine($"Created {groupedByInterval.Count} groups");
Console.WriteLine($"Q1 points: {groupedByInterval["Q1"].Count}"); // 2
Console.WriteLine($"Q2 points: {groupedByInterval["Q2"].Count}"); // 2

// Example 3: Calculate comprehensive statistics
var statistics = aggregator.CalculateStatistics(dataPoints);
Console.WriteLine($"Count: {statistics.Count}");
Console.WriteLine($"Sum: {statistics.Sum:F2}");
Console.WriteLine($"Average: {statistics.Average:F2}");
Console.WriteLine($"Min: {statistics.Min:F2}");
Console.WriteLine($"Max: {statistics.Max:F2}");
Console.WriteLine($"Median: {statistics.Median:F2}");
Console.WriteLine($"Range: {statistics.Range:F2}");
Console.WriteLine($"Standard Deviation: {statistics.StandardDeviation:F4}");
Console.WriteLine($"Calculated at: {statistics.CalculatedAt:O}");
```

## DataAggregatorTests

`DataAggregatorTests` provides a comprehensive suite of unit tests for the `DataAggregator` class, which handles data aggregation operations including bucket-based aggregation, interval-based grouping, and statistical calculations. The tests validate edge cases, parameter validation, and correct computation across different aggregation types (Average, Sum, Min, Max, Median) to ensure accurate data summarization for chart rendering scenarios.

The test suite covers:

- **Bucket-based aggregation**: Validating `AggregateByCount` with null/empty inputs, zero/negative bucket counts, various aggregation types, and edge cases where bucket count exceeds data point count
- **Interval-based grouping**: Testing `AggregateByInterval` with null inputs, label-based grouping, and null label handling (groups as "unknown")
- **Statistical calculations**: Verifying `CalculateStatistics` returns null for null/empty inputs and computes Sum, Average, Min, Max, Median, Range, and Standard Deviation correctly
- **Error handling**: Ensuring proper exception throwing for invalid parameters and fallback behavior for unsupported aggregation types

```csharp
using System;
using System.Collections.Generic;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Utilities;

public class DataAggregatorTestsExample
{
    public static void Main()
    {
        // Initialize DataAggregator with logger
        var logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<DataAggregator>();
        var aggregator = new DataAggregator(logger);

        // Example 1: Aggregate data points by count into buckets using average aggregation
        var dataPoints = new List<DataPoint>
        {
            new DataPoint(1.0, 10.0),
            new DataPoint(2.0, 20.0),
            new DataPoint(3.0, 30.0),
            new DataPoint(4.0, 40.0)
        };

        var aggregated = aggregator.AggregateByCount(dataPoints, 2, AggregationType.Average);
        Console.WriteLine($"Aggregated {aggregated.Count} buckets");
        Console.WriteLine($"Bucket 1 average: {aggregated[0].Value:F2}");  // 15.00
        Console.WriteLine($"Bucket 2 average: {aggregated[1].Value:F2}");  // 35.00

        // Example 2: Group data points by label (interval-based aggregation)
        var labeledPoints = new List<DataPoint>
        {
            new DataPoint(1.0, 100.0) { Label = "Q1" },
            new DataPoint(2.0, 150.0) { Label = "Q1" },
            new DataPoint(3.0, 200.0) { Label = "Q2" },
            new DataPoint(4.0, 250.0) { Label = "Q2" }
        };

        var grouped = aggregator.AggregateByInterval(labeledPoints, AggregationType.Average);
        Console.WriteLine($"Groups created: {grouped.Count}");      // 2
        Console.WriteLine($"Q1 data points: {grouped["Q1"].Count}"); // 2
        Console.WriteLine($"Q2 data points: {grouped["Q2"].Count}"); // 2

        // Example 3: Calculate comprehensive statistics
        var stats = aggregator.CalculateStatistics(dataPoints);
        Console.WriteLine($"Statistics - Count: {stats?.Count}, Sum: {stats?.Sum:F2}");
        Console.WriteLine($"Min: {stats?.Min:F2}, Max: {stats?.Max:F2}");
        Console.WriteLine($"Average: {stats?.Average:F2}, Median: {stats?.Median:F2}");
        Console.WriteLine($"Range: {stats?.Range:F2}, StdDev: {stats?.StandardDeviation:F4}");
        Console.WriteLine($"Calculated at: {stats?.CalculatedAt:O}");
    }
}
```

## ChartRenderingIntegrationTests

`ChartRenderingIntegrationTests` validates the end-to-end rendering pipeline of the SkiaSharp chart engine, ensuring that charts are generated and exported correctly across various formats and configurations. It covers functional testing for rendering, exporting, caching, and parallel processing capabilities, providing confidence in the system's reliability under different scenarios.

```csharp
using System;
using System.Threading.Tasks;
using SkiaSharpChartEngine.Tests; 

public class ChartRenderingIntegrationTestsExample
{
    public static async Task Main()
    {
        // Setup 
        using var testEngine = new ChartRenderingIntegrationTests();

        // 1. Basic rendering tests
        testEngine.CanRenderSimpleLineChartToFile();
        
        var bytes = testEngine.CanRenderChartToByteArray();
        Console.WriteLine($"Rendered chart to byte array, size: {bytes.Length}");

        // 2. Async rendering
        await testEngine.CanRenderChartAsyncToFile();

        // 3. Export tests
        testEngine.CanExportChartAsPng();
        await testEngine.CanExportChartAsSvg();
        await testEngine.CanExportChartAsJpeg();
        await testEngine.CanExportChartAsWebP();

        // 4. Advanced rendering features
        testEngine.CanRenderMultiSeriesChart();
        testEngine.CanToggleSeriesVisibility();
        testEngine.CanRenderChartWithSingleDataPoint();

        // 5. Caching and configuration
        testEngine.CachingImprovesPerfomanceOnSecondRender();
        testEngine.DifferentChartsProduceDifferentCacheKeys();
        testEngine.CanApplyCustomConfiguration();
        testEngine.CanDisableGridAndLegend();

        // 6. Error handling
        testEngine.RenderingFailsWithMissingData();
        testEngine.ExportFailsWithInvalidPath();

        // 7. Parallel processing
        await testEngine.CanRenderMultipleChartsInParallel();
        await testEngine.CanExportMultipleChartsInParallel();
    }
}
## AlertingService

`AlertingService` monitors chart rendering operations and system conditions, triggering alerts when issues are detected. It maintains a registry of alert rules, tracks active alerts, and provides methods for acknowledging, querying, and clearing alerts. The service supports severity-based filtering and maintains a history of past alerts for auditing and analysis.

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Services;

public class AlertingServiceExample
{
    public static async Task Main()
    {
        // Initialize alerting service with logger
        var logger = new NullLogger<AlertingService>();
        var alertingService = new AlertingService(logger);

        // Example 1: Register a custom alert rule
        alertingService.RegisterRule(
            name: "HighMemoryUsage",
            condition: () => Environment.WorkingSet64 > 1024 * 1024 * 1024, // > 1GB
            message: "High memory usage detected",
            severity: AlertSeverity.Warning
        );

        // Example 2: Check all registered rules
        await alertingService.CheckAsync();

        // Example 3: Get active alerts
        var activeAlerts = alertingService.GetActiveAlerts();
        Console.WriteLine($"Active alerts: {activeAlerts.Count}");

        // Example 4: Acknowledge an alert
        if (activeAlerts.Count > 0)
        {
            bool acknowledged = alertingService.AcknowledgeAlert(activeAlerts[0].Id);
            Console.WriteLine($"Alert acknowledged: {acknowledged}");
        }

        // Example 5: Get alerts by severity
        var warningAlerts = alertingService.GetAlertsBySeverity(AlertSeverity.Warning);
        Console.WriteLine($"Warning alerts: {warningAlerts.Count}");

        // Example 6: Clear acknowledged alerts
        int clearedCount = alertingService.ClearAcknowledgedAlerts();
        Console.WriteLine($"Cleared {clearedCount} acknowledged alerts");

        // Example 7: Get alert history
        var alertHistory = alertingService.GetAlertHistory();
        Console.WriteLine($"Alert history entries: {alertHistory.Count}");

        // Example 8: Get alert statistics
        var statistics = alertingService.GetStatistics();
        Console.WriteLine($"Total alerts: {statistics.TotalAlerts}");
        Console.WriteLine($"Active alerts: {statistics.ActiveAlerts}");
        Console.WriteLine($"Critical alerts: {statistics.CriticalAlerts}");
        Console.WriteLine($"Warning alerts: {statistics.WarningAlerts}");
        Console.WriteLine($"Info alerts: {statistics.InfoAlerts}");
    }
}

// Example Alert class usage
public class AlertMonitor
{
    private readonly AlertingService _alertingService;
    private readonly ILogger _logger;

    public AlertMonitor(AlertingService alertingService, ILogger<AlertMonitor> logger)
    {
        _alertingService = alertingService;
        _logger = logger;
        
        // Subscribe to alert events
        _alertingService.AlertTriggered += OnAlertTriggered;
        _alertingService.AlertAcknowledged += OnAlertAcknowledged;
        _alertingService.AlertCleared += OnAlertCleared;
    }

    private void OnAlertTriggered(object sender, AlertEventArgs e)
    {
        _logger.LogWarning("ALERT TRIGGERED: [{Severity}] {Name} - {Message}", 
            e.Alert.Severity, e.Alert.Name, e.Alert.Message);
    }

    private void OnAlertAcknowledged(object sender, AlertEventArgs e)
    {
        _logger.LogInformation("ALERT ACKNOWLEDGED: {AlertId} by {AcknowledgedBy}", 
            e.Alert.Id, e.Alert.AcknowledgedBy);
    }

    private void OnAlertCleared(object sender, AlertEventArgs e)
    {
        _logger.LogInformation("ALERT CLEARED: {AlertId}", e.Alert.Id);
    }
}
```

## ExportService

`ExportService` provides functionality for exporting charts to various file formats including PNG, SVG, JPEG, and WEBP. It supports both asynchronous and synchronous export operations with comprehensive error handling and format validation. The service integrates with the chart rendering pipeline to produce high-quality chart exports suitable for reports, presentations, and web applications.

```csharp
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Services;
using SkiaSharpChartEngine.Constants;

public class ExportServiceExample
{
    public static async Task Main(string[] args)
    {
        // Initialize ExportService with required dependencies
        var logger = new NullLogger<ExportService>();
        var renderingService = new ChartRenderingService(
            logger,
            new ChartDataService(logger),
            new RenderCacheService(logger, maxCacheSize: 100)
        );
        
        var exportService = new ExportService(renderingService, logger);

        // Example 1: Check supported export formats
        var supportedFormats = exportService.GetSupportedFormats();
        Console.WriteLine("Supported export formats:");
        foreach (var format in supportedFormats)
        {
            Console.WriteLine($"- {format}");
        }

        // Example 2: Export chart asynchronously to PNG
        var chart = new Chart("sales-chart")
        {
            Title = "Quarterly Sales Performance"
        };
        
        var series = new ChartSeries("Revenue")
        {
            LineWidth = 2.5f,
            Color = "#2E86C1"
        };
        series.AddDataPoint(1.0, 100000.0);
        series.AddDataPoint(2.0, 125000.0);
        series.AddDataPoint(3.0, 150000.0);
        series.AddDataPoint(4.0, 175000.0);
        chart.AddSeries(series);

        var exportOptions = new ExportOptions
        {
            Format = ExportFormat.PNG,
            OutputPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "sales-chart.png"),
            Width = 1920,
            Height = 1080,
            Scale = 1.0f
        };

        var result = await exportService.ExportAsync(chart, exportOptions);
        
        if (result.Success)
        {
            Console.WriteLine($"Chart exported successfully to: {result.OutputPath}");
            Console.WriteLine($"File size: {new FileInfo(result.OutputPath).Length} bytes");
        }
        else
        {
            Console.WriteLine($"Export failed: {result.ErrorMessage}");
        }

        // Example 3: Export chart synchronously to SVG
        var svgOptions = new ExportOptions
        {
            Format = ExportFormat.SVG,
            OutputPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "sales-chart.svg"),
            Width = 800,
            Height = 600
        };

        var syncResult = exportService.Export(chart, svgOptions);
        
        if (syncResult.Success)
        {
            Console.WriteLine($"SVG export successful: {syncResult.OutputPath}");
        }

        // Example 4: Check if a format is supported
        bool supportsJpeg = exportService.SupportsFormat(ExportFormat.JPEG);
        Console.WriteLine($"JPEG format supported: {supportsJpeg}");
    }
}
```

## StringFormatHelper

`StringFormatHelper` provides a set of static utility methods for efficient string formatting, transformation, and sanitization, designed to minimize allocations in high-performance chart rendering scenarios. It includes methods for formatting numbers into human-readable units, converting between naming conventions, sanitizing input text, and generating CSV-formatted lines.

```csharp
using System;
using SkiaSharpChartEngine.Utilities;

public class StringFormatHelperExample
{
    public static void Main()
    {
        // Example 1: Formatting numbers and percentages
        Console.WriteLine(StringFormatHelper.FormatNumberWithUnits(1250000.5)); // "1.3M"
        Console.WriteLine(StringFormatHelper.FormatCurrency(99.99));            // "$99.99"
        Console.WriteLine(StringFormatHelper.FormatPercentage(0.75, 1));        // "75.0%"

        // Example 2: String transformations
        Console.WriteLine(StringFormatHelper.TruncateWithEllipsis("Very long chart label text", 15)); // "Very long..."
        Console.WriteLine(StringFormatHelper.CamelCaseToTitleCase("myChartLabel"));              // "My Chart Label"
        Console.WriteLine(StringFormatHelper.SnakeCaseToTitleCase("data_series_name"));          // "Data Series Name"

        // Example 3: Sanitization and alignment
        Console.WriteLine(StringFormatHelper.Sanitize("Hello <World>!", false)); // "Hello World"
        Console.WriteLine(StringFormatHelper.PadForAlignment("123", 5, '0', true)); // "00123"

        // Example 4: Formatting and utilities
        Console.WriteLine(StringFormatHelper.FormatTimespan(TimeSpan.FromSeconds(125))); // "2.1m"
        Console.WriteLine(StringFormatHelper.Repeat("-", 10));                          // "----------"
        Console.WriteLine(StringFormatHelper.ToCsvLine("Data", 100, true));             // "Data,100,True"
    }
}
```


## ChartStatistics

`ChartStatistics` provides comprehensive statistical analysis for chart data series, including descriptive statistics, trend analysis, and outlier detection. It calculates key metrics such as mean, median, mode, variance, standard deviation, quartiles, and skewness to help understand data distribution and identify anomalies.

```csharp
using System;
using System.Collections.Generic;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Utilities;

public class ChartStatisticsExample
{
    public static void Main()
    {
        // Create a sample data series
        var dataPoints = new List<DataPoint>
        {
            new DataPoint { Label = "Jan", Value = 120 },
            new DataPoint { Label = "Feb", Value = 150 },
            new DataPoint { Label = "Mar", Value = 130 },
            new DataPoint { Label = "Apr", Value = 180 },
            new DataPoint { Label = "May", Value = 200 },
            new DataPoint { Label = "Jun", Value = 170 }
        };

        var series = new ChartSeries
        {
            Name = "Monthly Sales",
            DataPoints = dataPoints
        };

        // Calculate statistics
        var chartStats = new ChartStatistics(null);
        var stats = chartStats.CalculateSeriesStatistics(series);
        
        Console.WriteLine($"Series: {stats.SeriesName}");
        Console.WriteLine($"Count: {stats.Count}");
        Console.WriteLine($"Sum: {stats.Sum:F2}");
        Console.WriteLine($"Mean: {stats.Mean:F2}");
        Console.WriteLine($"Median: {stats.Median:F2}");
        Console.WriteLine($"Mode: {stats.Mode:F2}");
        Console.WriteLine($"Min: {stats.Min}");
        Console.WriteLine($"Max: {stats.Max}");
        Console.WriteLine($"Range: {stats.Range}");
        Console.WriteLine($"Standard Deviation: {stats.StandardDeviation:F2}");
        Console.WriteLine($"Variance: {stats.Variance:F2}");
        Console.WriteLine($"Coefficient of Variation: {stats.CoefficientOfVariation:F2}");
        Console.WriteLine($"Q1: {stats.Q1:F2}");
        Console.WriteLine($"Q3: {stats.Q3:F2}");
        Console.WriteLine($"IQR: {stats.IQR:F2}");
        Console.WriteLine($"Skewness: {stats.Skewness:F2}");
        
        // Detect outliers
        var outliers = chartStats.DetectOutliers(series);
        Console.WriteLine($"\nOutliers detected: {outliers.Count}");
        foreach (var outlier in outliers)
        {
            Console.WriteLine($"  Index {outlier.Index}: {outlier.Value} (Label: {outlier.Label})");
        }
        
        // Calculate trend
        var trend = chartStats.CalculateTrend(series);
        Console.WriteLine($"\nTrend (slope): {trend:F4}");
    }
}
```

## CompressionHelper

`CompressionHelper` provides compression and decompression utilities for chart data and exports using GZip compression. It efficiently reduces storage and transmission size for binary data, strings, and chart exports while maintaining data integrity. The class calculates compression ratios and can detect GZip-compressed data.

```csharp
using System;
using System.IO;
using Microsoft.Extensions.Logging.Abstractions;
using SkiaSharpChartEngine.Utilities;

public class CompressionHelperExample
{
    public static async Task Main()
    {
        var logger = new NullLogger<CompressionHelper>();
        var compressionHelper = new CompressionHelper(logger);

        // Compress and decompress byte arrays
        var originalData = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 };
        var compressedData = await compressionHelper.CompressAsync(originalData);
        var decompressedData = await compressionHelper.DecompressAsync(compressedData);

        Console.WriteLine($"Original size: {originalData.Length} bytes");
        Console.WriteLine($"Compressed size: {compressedData.Length} bytes");
        Console.WriteLine($"Decompression successful: {decompressedData.SequenceEqual(originalData)}");

        // Calculate compression ratio
        var ratio = compressionHelper.CalculateCompressionRatio(originalData.Length, compressedData.Length);
        Console.WriteLine($"Compression ratio: {ratio:F1}%");

        // Check if data is GZip compressed
        Console.WriteLine($"Is GZip compressed: {compressionHelper.IsGZipCompressed(compressedData)}");

        // Compress and decompress strings
        var originalText = "This is a sample text that needs to be compressed for efficient storage and transmission";
        var compressedText = await compressionHelper.CompressStringAsync(originalText);
        var decompressedText = await compressionHelper.DecompressStringAsync(compressedText);

        Console.WriteLine($"Original text: {originalText}");
        Console.WriteLine($"Decompressed text: {decompressedText}");
        Console.WriteLine($"Text compression successful: {decompressedText == originalText}");
    }
}
```

## PerformanceOptimizer

`PerformanceOptimizer` provides tools to analyze chart performance and optimize rendering by downsampling data points and estimating memory usage. It identifies potential performance bottlenecks through detailed analysis, returning actionable recommendations based on data volume, series count, and chart dimensions.

```csharp
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging.Abstractions;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Utilities;

public class PerformanceOptimizerExample
{
    public static void Main()
    {
        var logger = new NullLogger<PerformanceOptimizer>();
        var optimizer = new PerformanceOptimizer(logger);

        // Create a sample chart
        var chart = new Chart("performance-chart-1");
        chart.Configuration = new ChartConfiguration { Width = 1920, Height = 1080 };
        var series = new ChartSeries("LargeSeries");
        for (int i = 0; i < 15000; i++)
            series.AddDataPoint(i, i * 2);
        chart.AddSeries(series);

        // 1. Analyze chart performance
        var analysis = optimizer.AnalyzeChart(chart);
        Console.WriteLine($"Analysis for {analysis.ChartId} (Points: {analysis.TotalDataPoints})");

        foreach (var rec in analysis.Recommendations)
        {
            Console.WriteLine($"[{rec.Severity}] {rec.Category}: {rec.Message}");
            Console.WriteLine($"   Action: {rec.Action}");
        }

        // 2. Downsample data points
        var downsampled = optimizer.Downsample(series.DataPoints, 1000);
        Console.WriteLine($"Downsampled from {series.DataPoints.Count} to {downsampled.Count} points");

        // 3. Estimate memory usage
        long memoryEstimate = optimizer.EstimateMemoryUsage(chart);
        Console.WriteLine($"Estimated memory: {memoryEstimate / 1024 / 1024} MB");
    }
}
```

## PerformanceMonitor

`PerformanceMonitor` tracks and analyzes performance metrics for chart operations, providing detailed statistics on execution time, success rates, and performance trends. It helps identify bottlenecks and optimize rendering performance by monitoring operations such as chart rendering, data processing, and export tasks.

```csharp
using System;
using Microsoft.Extensions.Logging.Abstractions;
using SkiaSharpChartEngine.Diagnostics;

public class PerformanceMonitorExample
{
    public static void Main()
    {
        // Create performance monitor with logger
        var logger = new NullLogger<PerformanceMonitor>();
        var performanceMonitor = new PerformanceMonitor(logger);

        // Simulate chart rendering operations
        Console.WriteLine("Simulating chart rendering operations...\n");

        // Record metrics for successful operations
        performanceMonitor.RecordMetric("Chart.Render", 45);
        performanceMonitor.RecordMetric("Chart.Render", 38);
        performanceMonitor.RecordMetric("Chart.Render", 52);
        performanceMonitor.RecordMetric("Chart.Render", 41);
        performanceMonitor.RecordMetric("Chart.Render", 49);

        // Record metrics for failed operations
        performanceMonitor.RecordMetric("Data.Load", 120, success: false);
        performanceMonitor.RecordMetric("Data.Load", 150, success: false);

        // Record metrics for export operations
        performanceMonitor.RecordMetric("Chart.Export.PNG", 185);
        performanceMonitor.RecordMetric("Chart.Export.PNG", 172);
        performanceMonitor.RecordMetric("Chart.Export.PDF", 245);

        // Get statistics for specific operations
        var renderStats = performanceMonitor.GetStatistics("Chart.Render");
        Console.WriteLine(renderStats?.ToString() ?? "No data");
        Console.WriteLine($"Total tracked operations: {performanceMonitor.GetTrackedOperationCount()}");
        Console.WriteLine($"Total metrics recorded: {performanceMonitor.GetTotalMetricCount()}");
        Console.WriteLine();

        // Get statistics for export operations
        var exportStats = performanceMonitor.GetStatistics("Chart.Export.PNG");
        Console.WriteLine(exportStats?.ToString() ?? "No data");
        Console.WriteLine();

        // Get statistics for failed operations
        var failedStats = performanceMonitor.GetStatistics("Data.Load");
        Console.WriteLine(failedStats?.ToString() ?? "No data");
        Console.WriteLine();

        // Get all statistics
        var allStats = performanceMonitor.GetAllStatistics();
        Console.WriteLine("All tracked operations:");
        foreach (var stat in allStats)
        {
            Console.WriteLine($"  - {stat}");
        }
        Console.WriteLine();

        // Clear metrics for specific operation
        performanceMonitor.ClearOperation("Chart.Export.PDF");
        Console.WriteLine($"Operations after clearing 'Chart.Export.PDF': {performanceMonitor.GetTrackedOperationCount()}");
        
        // Clear all metrics
        performanceMonitor.Clear();
        Console.WriteLine($"Operations after clearing all metrics: {performanceMonitor.GetTrackedOperationCount()}");
    }
}
```

## ChartValidator

`ChartValidator` is a comprehensive validation utility for SkiaSharp charts that provides static methods to validate charts, series, data points, and configurations. It returns detailed validation results with errors and warnings, making it ideal for debugging and ensuring chart integrity before rendering. The validator checks for null references, empty identifiers, invalid dimensions, malformed colors, and other common issues that could cause rendering problems.

```csharp
using System;
using Microsoft.Extensions.Logging.Abstractions;
using SkiaSharp;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Utilities;

public class ChartValidatorExample
{
    public static void Main()
    {
        // Create a valid chart
        var chart = new Chart("validation-example");
        chart.Configuration = new ChartConfiguration
        {
            Width = 800,
            Height = 600,
            BackgroundColor = "#ffffff",
            GridColor = "#cccccc",
            AxisColor = "#333333"
        };
        
        // Add a valid series
        var series = new ChartSeries("Temperature")
        {
            Color = "#ff0000",
            LineWidth = 2f
        };
        series.AddDataPoint(1.0, 25.5);
        series.AddDataPoint(2.0, 26.1);
        series.AddDataPoint(3.0, 27.8);
        chart.AddSeries(series);
        
        // Validate the chart
        var validationResult = ChartValidator.ValidateChart(chart);
        
        Console.WriteLine("Validation Result:");
        Console.WriteLine(validationResult);
        Console.WriteLine($"Is Valid: {validationResult.IsValid}");
        
        if (!validationResult.IsValid)
        {
            Console.WriteLine("\nErrors found:");
            foreach (var error in validationResult.Errors)
            {
                Console.WriteLine($"  - {error}");
            }
        }
        
        // Example with invalid data
        var invalidChart = new Chart(""); // Empty ID
        var invalidSeries = new ChartSeries(""); // Empty name
        invalidSeries.AddDataPoint(double.NaN, 10.0); // Invalid X value
        invalidChart.AddSeries(invalidSeries);
        
        var invalidResult = ChartValidator.ValidateChart(invalidChart);
        Console.WriteLine("\nInvalid chart validation:");
        Console.WriteLine(invalidResult);
        
        // Validate individual components
        var seriesResult = ChartValidator.ValidateSeries(series);
        Console.WriteLine("\nSeries validation:");
        Console.WriteLine(seriesResult);
        
        var dataPointResult = ChartValidator.ValidateDataPoint(series.DataPoints[0]);
        Console.WriteLine("\nDataPoint validation:");
        Console.WriteLine(dataPointResult);
        
        var configResult = ChartValidator.ValidateConfiguration(chart.Configuration);
        Console.WriteLine("\nConfiguration validation:");
        Console.WriteLine(configResult);
    }
}
```


## RenderQueueManager

`RenderQueueManager` is a thread-safe queue manager that handles chart rendering operations with priority-based execution. It prevents resource exhaustion by limiting the number of concurrent renders and provides comprehensive job management capabilities including enqueueing, dequeuing, cancellation, and status tracking. The manager uses a priority queue system where higher priority jobs are processed first, and automatically tracks job lifecycle from enqueueing through completion.


```csharp
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using SkiaSharp;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Utilities;

public class RenderQueueManagerExample
{
    public static async Task Main()
    {
        // Create a logger (can be null for simple scenarios)
        var logger = new NullLogger<RenderQueueManager>();
        
        // Create a render queue manager with max 2 concurrent renders
        var queueManager = new RenderQueueManager(logger, maxConcurrentRenders: 2);
        
        // Create sample charts
        var chart1 = new Chart("temperature-chart")
        {
            Configuration = new ChartConfiguration
            {
                Width = 800,
                Height = 600,
                BackgroundColor = "#ffffff"
            }
        };
        
        var chart2 = new Chart("pressure-chart")
        {
            Configuration = new ChartConfiguration
            {
                Width = 800,
                Height = 600,
                BackgroundColor = "#f8f9fa"
            }
        };
        
        // Enqueue jobs with different priorities
        string jobId1 = queueManager.Enqueue(chart1, priority: 1);
        string jobId2 = queueManager.Enqueue(chart2, priority: 2); // Higher priority
        string jobId3 = queueManager.Enqueue(chart1, priority: 0); // Lower priority
        
        Console.WriteLine($"Enqueued 3 jobs: {jobId1}, {jobId2}, {jobId3}");
        Console.WriteLine($"Queue size: {queueManager.GetQueueSize()}");
        
        // Process jobs (simulated)
        var job1 = queueManager.Dequeue();
        if (job1 != null)
        {
            Console.WriteLine($"Processing job: {job1.Id}, Priority: {job1.Priority}");
            queueManager.CompleteJob(job1.Id);
        }
        
        var job2 = queueManager.Dequeue();
        if (job2 != null)
        {
            Console.WriteLine($"Processing job: {job2.Id}, Priority: {job2.Priority}");
            queueManager.CompleteJob(job2.Id);
        }
        
        // Get status
        var status = queueManager.GetStatus();
        Console.WriteLine($"\nQueue Status:");
        Console.WriteLine($"  Queued: {status.QueuedCount}");
        Console.WriteLine($"  Active: {status.ActiveRenders}");
        Console.WriteLine($"  Max Concurrent: {status.MaxConcurrentRenders}");
        
        // Wait for queue to empty
        await queueManager.WaitForQueueEmptyAsync();
        Console.WriteLine("\nAll jobs completed!");
        
        // Cancel a job (example)
        string jobId4 = queueManager.Enqueue(chart2, priority: 3);
        bool cancelled = queueManager.CancelJob(jobId4);
        Console.WriteLine($"\nCancelled job {jobId4}: {cancelled}");
        
        // Check job status
        var jobStatus = queueManager.GetJobStatus(jobId1);
        Console.WriteLine($"Job {jobId1} status: {jobStatus}");
    }
}
```
