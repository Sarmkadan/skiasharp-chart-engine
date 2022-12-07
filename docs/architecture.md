# Architecture Overview

This document describes the high-level architecture, design patterns, and component interactions in the SkiaSharp Chart Engine.

## Design Philosophy

The library follows these core principles:

1. **Separation of Concerns** - Each component has a single responsibility
2. **Dependency Injection** - Loose coupling through abstraction
3. **Performance First** - Optimized rendering with intelligent caching
4. **Extensibility** - Pipeline-based architecture for custom renderers
5. **Async/Await** - Full async support throughout
6. **Immutability** - Data models where appropriate
7. **Fail-Fast** - Validation at entry points

## High-Level Architecture

```
┌────────────────────────────────────────────────────────────────┐
│                         ChartEngine                            │
│              (Public API, async & sync entry points)           │
└───────────────────────────┬──────────────────────────────────┘
                            │
        ┌───────────────────┼───────────────────┬────────────────┐
        │                   │                   │                │
    ┌───▼────┐      ┌──────▼──────┐      ┌─────▼────┐      ┌───▼────┐
    │Rendering│      │   Export    │      │Repository│      │  Data  │
    │Service  │      │   Service   │      │          │      │Service │
    └───┬────┘      └──────┬──────┘      └─────┬────┘      └───┬────┘
        │                   │                   │               │
        └───────────────────┴───────────────────┴───────────────┘
                            │
            ┌───────────────┼───────────────┐
            │               │               │
        ┌───▼────┐    ┌─────▼─────┐    ┌──▼──────┐
        │ Cache  │    │ Config    │    │Models & │
        │Service │    │ Service   │    │Types    │
        └────────┘    └───────────┘    └─────────┘
            │
        ┌───▼──────────────────────────────────┐
        │   SkiaSharp Rendering Backend        │
        │   (Canvas, Paint, Surface, Image)    │
        └───────────────────────────────────────┘
```

## Component Details

### 1. ChartEngine (Facade)

**Purpose**: Single entry point for all chart operations

**Responsibilities**:
- Create/manage service instances
- Coordinate between services
- Provide sync and async APIs
- Error handling and logging

**Key Methods**:
```csharp
RenderChart(chart)                    // Render to bytes
RenderChartAsync(chart, ct)           // Async render
ExportChart(chart, options)           // Export to file
ExportChartAsync(chart, options, ct)  // Async export
SaveChart(chart)                      // Store in repository
GetChart(id)                          // Retrieve from repository
UpdateChart(chart)                    // Update stored chart
DeleteChart(id)                       // Delete from repository
```

**Dependencies**:
- IChartRenderingService
- IExportService
- IChartRepository
- IChartDataService
- IConfigurationService
- ILogger

### 2. ChartRenderingService

**Purpose**: Convert chart models to raster images (PNG, JPEG, WebP)

**Responsibilities**:
- Draw chart elements (axes, grid, series, legend, title)
- Apply styling (colors, line widths, fonts)
- Optimize rendering for performance
- Manage SkiaSharp surface and canvas

**Pipeline**:
```
Chart Input
    │
    ├─► Validation
    ├─► Cache Check
    ├─► Rendering:
    │   ├─ Clear canvas
    │   ├─ Draw frame
    │   ├─ Draw grid
    │   ├─ Draw series (line paths, bars, points)
    │   ├─ Draw axes & labels
    │   ├─ Draw legend
    │   └─ Draw title
    ├─► Encode (PNG/JPEG/WebP)
    └─► Return byte array
```

**Performance Optimizations**:
- Bitmap caching
- Path reuse
- Paint object pooling
- Lazy grid calculation
- Batch rendering

### 3. ExportService

**Purpose**: Handle multi-format export (SVG, PDF, etc.)

**Responsibilities**:
- Format selection
- File I/O
- Format-specific encoding
- Directory creation
- Metadata writing

**Supported Formats**:
- PNG (via ChartRenderingService)
- JPEG (via SkiaSharp encoding)
- WebP (via SkiaSharp encoding)
- SVG (custom serialization)
- PDF (via SkiaSharp PDF backend)

### 4. ChartDataService

**Purpose**: Data validation and transformation

**Responsibilities**:
- Validate chart configuration
- Validate data points
- Check constraints (min/max series, data point counts)
- Type coercion and normalization
- Calculate data statistics

**Validation Rules**:
```
Chart:
  ├─ At least 1 series required
  ├─ At least 1 data point required
  └─ Configuration dimensions > 0

Series:
  ├─ Name length 1-100 chars
  ├─ Color valid hex format
  ├─ Line width 0.5-10
  └─ Max 500 data points per series

DataPoint:
  ├─ X and Y are finite numbers
  ├─ Not NaN or Infinity
  └─ Within reasonable ranges

Configuration:
  ├─ Dimensions >= 100x100
  ├─ Margins >= 0
  └─ Colors valid hex format
```

### 5. ConfigurationService

**Purpose**: Manage chart configuration and templates

**Responsibilities**:
- Provide default configuration
- Create templates for each chart type
- Validate configuration objects
- Manage color palettes and themes

**Built-in Templates**:
- LineChart template (trend visualization)
- BarChart template (categorical data)
- ColumnChart template (vertical bars)
- PieChart template (composition)
- HeatmapChart template (2D patterns)
- AreaChart template (filled trends)
- ScatterChart template (correlations)

### 6. RenderCacheService

**Purpose**: Cache rendered chart bytes to avoid redundant computation

**Responsibilities**:
- Store rendered output
- Cache invalidation on data changes
- Configurable cache duration
- Memory management
- Hit rate metrics

**Cache Key Generation**:
```csharp
key = $"chart_{chartId}_{lastModified:yyyyMMddHHmmss}"
```

**Cache Invalidation**:
- Automatic when `Chart.ModifiedAt` changes
- Manual via `PrewarmRenderCache()` 
- Configurable TTL (default 5 minutes)

### 7. ChartRepository

**Purpose**: Store and retrieve charts in memory

**Responsibilities**:
- CRUD operations on charts
- Thread-safe storage
- Querying by ID
- Listing all charts
- Batch operations

**Storage Model**:
```csharp
Dictionary<string, Chart> _charts;  // In-memory storage
ReaderWriterLockSlim _lock;          // Thread safety
```

**Methods**:
```csharp
Save(chart)                    // Insert or update
GetById(id)                    // Retrieve single
GetAll()                       // List all
Update(chart)                  // Update existing
Delete(id)                     // Remove by ID
GetByTag(tagKey, tagValue)     // Query by tags
```

## Data Models

### Chart
Root container for all chart data and configuration.

```csharp
public class Chart
{
    public string Id { get; set; }                    // Unique identifier
    public ChartType Type { get; set; }              // Line, Bar, Pie, etc.
    public List<ChartSeries> Series { get; }        // Data series
    public ChartConfiguration Configuration { get; } // Visual settings
    public DateTime CreatedAt { get; set; }         // Creation timestamp
    public DateTime? ModifiedAt { get; set; }       // Last modification
    public string? CreatedBy { get; set; }          // Creator identifier
    public bool IsTemplate { get; set; }            // Template flag
    public Dictionary<string, object>? Tags { get; set; }  // Metadata
}
```

### ChartSeries
Represents a single data series.

```csharp
public class ChartSeries
{
    public string Name { get; set; }                // Series name
    public string Color { get; set; }              // Hex color code
    public List<DataPoint> DataPoints { get; }    // X/Y values
    public float LineWidth { get; set; }          // Stroke width
    public bool IsVisible { get; set; }           // Visibility flag
}
```

### DataPoint
Individual X/Y coordinate pair.

```csharp
public class DataPoint
{
    public double X { get; set; }  // X coordinate
    public double Y { get; set; }  // Y coordinate
    public string? Label { get; set; }  // Optional label
}
```

### ChartConfiguration
Visual and structural settings.

```csharp
public class ChartConfiguration
{
    // Layout
    public int Width { get; set; }           // Canvas width
    public int Height { get; set; }          // Canvas height
    public int MarginTop { get; set; }       // Top margin
    public int MarginBottom { get; set; }    // Bottom margin
    public int MarginLeft { get; set; }      // Left margin
    public int MarginRight { get; set; }     // Right margin
    
    // Display
    public string Title { get; set; }        // Chart title
    public string XAxisLabel { get; set; }   // X-axis label
    public string YAxisLabel { get; set; }   // Y-axis label
    
    // Styling
    public string BackgroundColor { get; set; }  // Background hex
    public string AxisColor { get; set; }        // Axis hex
    public string GridColor { get; set; }        // Grid hex
    public string TextColor { get; set; }        // Text hex
    
    // Features
    public bool ShowGrid { get; set; }       // Grid visibility
    public bool ShowLegend { get; set; }     // Legend visibility
    public bool ShowAxisLabels { get; set; } // Axis labels visibility
    public bool AntiAlias { get; set; }      // Anti-aliasing
}
```

## Rendering Pipeline

The core rendering process follows this pipeline:

```
1. INPUT VALIDATION
   └─► Chart.ValidateForRendering()
   └─► Check series count > 0
   └─► Check data points exist

2. CACHE CHECK
   └─► Generate cache key
   └─► Check RenderCacheService
   └─► Return cached result if hit

3. SETUP
   └─► Create SKSurface (canvas)
   └─► Get canvas context
   └─► Calculate bounds

4. RENDERING STAGES
   └─► DrawChartFrame()     - Border/frame
   └─► DrawGrid()           - Grid lines
   └─► DrawSeries()         - Lines/bars/points
   └─► DrawAxes()           - Axis lines & labels
   └─► DrawLegend()         - Series legend
   └─► DrawTitle()          - Chart title

5. ENCODING
   └─► Create image snapshot
   └─► Encode to PNG/JPEG/WebP
   └─► Get byte array

6. CACHING
   └─► Store in RenderCacheService
   └─► Return RenderResult

7. CLEANUP
   └─► Dispose resources
   └─► Return success/failure
```

## Coordinate System

The rendering uses this coordinate transformation:

```
Chart Data Space (X/Y values)
         │
         ├─► Calculate bounds: (minX, maxX, minY, maxY)
         │
         ├─► Normalize to 0-1 range
         │
         ├─► Map to canvas space
         │
Canvas Pixel Space (0-width, 0-height)
```

**Transformation Formula**:
```csharp
var canvasX = marginLeft + ((dataX - minX) / xRange) * canvasWidth;
var canvasY = canvasHeight - marginBottom - ((dataY - minY) / yRange) * canvasHeight;
```

## Thread Safety

All services are thread-safe:

```
ChartEngine
    ├─► Stateless (safe for concurrent calls)
    │
ChartRepository
    ├─► ReaderWriterLockSlim for concurrent reads
    │
RenderCacheService
    ├─► ConcurrentDictionary for thread-safe cache
    │
SkiaSharp Objects
    └─► Created per-render (no sharing)
```

**Safe Usage**:
```csharp
var engine = ChartEngine.Create();
var tasks = Enumerable.Range(0, 10)
    .Select(async i => await engine.RenderChartAsync(charts[i]))
    .ToList();
await Task.WhenAll(tasks);  // Safe: can render in parallel
```

## Dependency Injection

The library uses Microsoft.Extensions.DependencyInjection:

```csharp
services.AddSkiaSharpChartEngine(options =>
{
    options.CacheEnabled = true;
    options.CacheDurationSeconds = 300;
    options.MaxConcurrentRenders = 10;
});
```

**Registered Services**:
- IChartRenderingService (singleton)
- IExportService (singleton)
- IChartDataService (singleton)
- IChartRepository (singleton)
- IConfigurationService (singleton)
- IRenderCacheService (singleton)
- ILogger<T> (Microsoft.Extensions.Logging)

## Performance Characteristics

### Memory Usage
- Per-render: ~50MB (800x600 chart)
- Per-cached-chart: ~2-5MB
- Configuration objects: <1KB

### Rendering Time (800x600)
- Simple chart (1 series, 10 points): ~10ms
- Medium chart (3 series, 100 points): ~50ms
- Complex chart (5 series, 500 points): ~200ms
- With caching: <1ms (cache hit)

### CPU Usage
- Single-threaded rendering: Fully utilized
- Multi-chart rendering: Scales linearly
- I/O bound export: Non-blocking async

## Extensibility Points

### Custom Renderer
Implement `IChartRenderer`:
```csharp
public interface IChartRenderer
{
    RenderResult Render(Chart chart);
}
```

### Custom Cache
Implement `IRenderCacheService`:
```csharp
public interface IRenderCacheService
{
    void Set(string key, byte[] value);
    byte[]? Get(string key);
    void Clear();
}
```

### Custom Data Service
Implement `IChartDataService`:
```csharp
public interface IChartDataService
{
    void ValidateChart(Chart chart);
    void ValidateDataPoint(DataPoint point);
}
```

## Design Patterns Used

1. **Facade** - ChartEngine hides complexity
2. **Factory** - Service creation through DI
3. **Strategy** - Different renderers for each chart type
4. **Observer** - Event system for lifecycle events
5. **Singleton** - Services registered as singletons
6. **Decorator** - Middleware for cross-cutting concerns
7. **Template Method** - Rendering pipeline
8. **Builder** - ChartBuilder fluent API
9. **Repository** - Data access abstraction
10. **Dependency Injection** - Loose coupling

## Error Handling

All operations return `RenderResult` with error details:

```csharp
public class RenderResult
{
    public bool IsSuccessful { get; }
    public string ChartId { get; }
    public object? Data { get; }
    public long RenderTimeMs { get; }
    public ExportFormat Format { get; }
    public string? ErrorMessage { get; }
    public Exception? Exception { get; }
}
```

Exceptions are caught and wrapped:
```
ChartEngine
  └─► Catch all exceptions
  └─► Log error
  └─► Create failure RenderResult
  └─► Return to caller (no throw)
```

## Future Enhancements

Potential areas for extension:

1. **Distributed Caching** - Redis/Memcached backend
2. **GPU Rendering** - Direct3D/Metal acceleration
3. **Custom Renderers** - More chart types
4. **Animation System** - Built-in animations
5. **Data Binding** - Direct data source binding
6. **Web Components** - JavaScript interop for Blazor
7. **Streaming** - Incremental render output
8. **Clustering** - Web farm support

---

This architecture enables high performance, scalability, and extensibility while maintaining clean code and separation of concerns.
