# BitmapHelper

The `BitmapHelper` class is a utility provider within the `skiasharp-chart-engine` project, designed to facilitate common image processing and analysis tasks. It offers a suite of static methods for validating image formats, performing dimensional calculations, converting resolution metrics, and managing image scaling, ensuring consistent bitmap handling throughout the rendering pipeline.

## API

### IsPngValid
`public static bool IsPngValid(string filePath)`
Validates whether the file at the specified path is a valid PNG image. Returns `true` if the file header and structure conform to the PNG specification; otherwise, `false`. Throws `FileNotFoundException` if the file does not exist.

### IsJpegValid
`public static bool IsJpegValid(string filePath)`
Validates whether the file at the specified path is a valid JPEG image. Returns `true` if the file is a valid JPEG; otherwise, `false`. Throws `FileNotFoundException` if the file does not exist.

### GetPngDimensions
`public static (int Width, int Height)? GetPngDimensions(string filePath)`
Retrieves the pixel dimensions of a PNG image without loading the entire file into memory. Returns a tuple containing `Width` and `Height` if successful, or `null` if the file is not a valid PNG.

### EstimateFileSize
`public static long EstimateFileSize(int width, int height, string format, float quality = 0.8f)`
Estimates the file size in bytes for an image with given dimensions and format. The `format` parameter accepts standard identifiers (e.g., "png", "jpeg"). The `quality` parameter is used specifically for lossy formats like JPEG.

### DetectFormat
`public static string? DetectFormat(string filePath)`
Analyzes the header of the file at the specified path to detect its image format. Returns the format identifier (e.g., "png", "jpeg") as a string, or `null` if the format cannot be identified.

### GetDpiFromPpi
`public static int GetDpiFromPpi(int ppi)`
Converts Pixels Per Inch (PPI) to Dots Per Inch (DPI). Returns the equivalent DPI value as an integer.

### ConvertDpiToPpi
`public static float ConvertDpiToPpi(float dpi)`
Converts Dots Per Inch (DPI) to Pixels Per Inch (PPI). Returns the equivalent PPI value as a float.

### GetPhysicalSize
`public static (double WidthInches, double HeightInches) GetPhysicalSize(int widthPixels, int heightPixels, float dpi)`
Calculates the physical dimensions of an image in inches based on its pixel dimensions and a given DPI resolution.

### GetPixelDimensions
`public static (int WidthPixels, int HeightPixels) GetPixelDimensions(double widthInches, double heightInches, float dpi)`
Calculates the pixel dimensions required for an image to achieve a specific physical size in inches at a target DPI resolution.

### ValidateDimensions
`public static bool ValidateDimensions(int width, int height, int maxWidth, int maxHeight)`
Checks if an image's dimensions fall within defined maximum bounds. Returns `true` if both `width` and `height` are less than or equal to their respective maximums; otherwise, `false`.

### GetAspectRatio
`public static float GetAspectRatio(int width, int height)`
Calculates the aspect ratio of an image as the quotient of width divided by height.

### ScaleMaintainingRatio
`public static (int ScaledWidth, int ScaledHeight) ScaleMaintainingRatio(int width, int height, int maxDimension)`
Calculates new dimensions for an image that scales it to fit within a square box defined by `maxDimension`, while maintaining the original aspect ratio.

## Usage

```csharp
// Example 1: Validating an image and checking its dimensions
string path = "chart.png";
if (BitmapHelper.IsPngValid(path))
{
    var dimensions = BitmapHelper.GetPngDimensions(path);
    if (dimensions.HasValue)
    {
        Console.WriteLine($"Image size: {dimensions.Value.Width}x{dimensions.Value.Height}");
    }
}

// Example 2: Calculating physical size for printing
int pixelsWidth = 1920;
int pixelsHeight = 1080;
float dpi = 300f;
var physicalSize = BitmapHelper.GetPhysicalSize(pixelsWidth, pixelsHeight, dpi);
Console.WriteLine($"Physical size: {physicalSize.WidthInches}x{physicalSize.HeightInches} inches");
```

## Notes

*   **Thread Safety:** All members of `BitmapHelper` are `static` and stateless. They are thread-safe and can be called concurrently from multiple threads without locking.
*   **File Access:** Methods accepting a `filePath` perform file I/O operations. Ensure that the calling application has appropriate filesystem permissions. If an invalid path is provided, standard .NET `IOException` or `FileNotFoundException` types may be thrown.
*   **Input Validation:** While some methods return `null` or `false` for invalid formats, it is recommended to verify file existence and accessibility prior to calling these methods to minimize exception handling overhead.
*   **Floating Point Precision:** Calculations involving DPI and physical sizes may be subject to minor floating-point rounding errors. Use these values for display and estimation purposes rather than for high-precision manufacturing or engineering specifications.
