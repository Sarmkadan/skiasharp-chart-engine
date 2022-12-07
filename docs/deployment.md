# Deployment Guide

This guide covers deploying the SkiaSharp Chart Engine in various production environments.

## System Requirements

### Minimum Requirements
- **OS**: Windows, Linux, macOS
- **Runtime**: .NET 10.0
- **RAM**: 512MB
- **CPU**: 2 cores
- **Disk**: 100MB

### Recommended for Production
- **OS**: Linux (Ubuntu 22.04+) or Windows Server 2022+
- **Runtime**: .NET 10.0 LTS
- **RAM**: 4GB+
- **CPU**: 4+ cores
- **Disk**: 500MB+
- **Network**: 100Mbps+

## Deployment Methods

### Method 1: Docker (Recommended)

#### Build Docker Image

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["skiasharp-chart-engine.csproj", "./"]
RUN dotnet restore "skiasharp-chart-engine.csproj"
COPY . .
RUN dotnet build "skiasharp-chart-engine.csproj" -c Release -o /app/build

FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app
COPY --from=build /app/build .
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000
ENTRYPOINT ["dotnet", "SkiaSharpChartEngine.dll"]
```

#### Run Container

```bash
# Build
docker build -t skiasharp-chart-engine:1.2.0 .

# Run standalone
docker run -d -p 5000:5000 \
  -e CACHE_DURATION=300 \
  -e MAX_CONCURRENT_RENDERS=10 \
  --name chart-engine \
  skiasharp-chart-engine:1.2.0

# Run with docker-compose
docker-compose up -d
```

#### Docker Compose

```yaml
version: '3.8'

services:
  chart-engine:
    build: .
    ports:
      - "5000:5000"
    environment:
      ASPNETCORE_URLS: http://+:5000
      CACHE_DURATION: 300
      MAX_CONCURRENT_RENDERS: 10
    volumes:
      - ./output:/app/output
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:5000/health"]
      interval: 30s
      timeout: 10s
      retries: 3
```

### Method 2: Linux Systemd Service

#### Create Service File

```ini
# /etc/systemd/system/chart-engine.service
[Unit]
Description=SkiaSharp Chart Engine
After=network.target

[Service]
Type=notify
User=chart-engine
WorkingDirectory=/opt/chart-engine
ExecStart=/usr/bin/dotnet /opt/chart-engine/SkiaSharpChartEngine.dll
Restart=on-failure
RestartSec=10
StandardOutput=journal
StandardError=journal
Environment="ASPNETCORE_URLS=http://+:5000"
Environment="CACHE_DURATION=300"

[Install]
WantedBy=multi-user.target
```

#### Install and Start

```bash
sudo systemctl daemon-reload
sudo systemctl enable chart-engine
sudo systemctl start chart-engine
sudo systemctl status chart-engine
```

### Method 3: Windows Service

#### Install as Service

```powershell
# Install
New-Service -Name "ChartEngine" `
  -BinaryPathName "C:\Apps\SkiaSharpChartEngine\SkiaSharpChartEngine.exe" `
  -StartupType Automatic

# Start
Start-Service -Name "ChartEngine"

# Check status
Get-Service -Name "ChartEngine"
```

### Method 4: Kubernetes

#### Docker Image

```bash
docker build -t myregistry.azurecr.io/chart-engine:1.2.0 .
docker push myregistry.azurecr.io/chart-engine:1.2.0
```

#### Kubernetes Deployment

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: chart-engine
spec:
  replicas: 3
  selector:
    matchLabels:
      app: chart-engine
  template:
    metadata:
      labels:
        app: chart-engine
    spec:
      containers:
      - name: chart-engine
        image: myregistry.azurecr.io/chart-engine:1.2.0
        ports:
        - containerPort: 5000
        env:
        - name: ASPNETCORE_URLS
          value: "http://+:5000"
        - name: CACHE_DURATION
          value: "300"
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"
        livenessProbe:
          httpGet:
            path: /health
            port: 5000
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health
            port: 5000
          initialDelaySeconds: 10
          periodSeconds: 5

---
apiVersion: v1
kind: Service
metadata:
  name: chart-engine-service
spec:
  selector:
    app: chart-engine
  type: LoadBalancer
  ports:
  - protocol: TCP
    port: 80
    targetPort: 5000
```

## Environment Variables

```bash
# Core Configuration
ASPNETCORE_URLS=http://+:5000           # Server URL binding
ASPNETCORE_ENVIRONMENT=Production       # Runtime environment

# Caching
CACHE_ENABLED=true                      # Enable caching
CACHE_DURATION=300                      # Cache TTL (seconds)

# Performance
MAX_CONCURRENT_RENDERS=10               # Concurrent render limit
BATCH_SIZE=5                            # Batch processing size

# Logging
LOG_LEVEL=Information                   # Minimum log level
LOG_FILE=/var/log/chart-engine.log      # Log file path

# Security
JWT_SECRET=your_secret_key              # JWT signing key
CORS_ORIGINS=*                          # CORS allowed origins

# Storage
OUTPUT_PATH=/app/output                 # Default output directory
TEMP_PATH=/tmp/chart-engine             # Temporary files path
```

## Performance Tuning

### Memory Optimization

```csharp
// Configure service with memory limits
services.AddSkiaSharpChartEngine(options =>
{
    options.CacheMaxSizeBytes = 1024 * 1024 * 100;  // 100MB cache
    options.MaxConcurrentRenders = 10;
    options.CacheDurationSeconds = 300;
});
```

### Connection Pooling

```csharp
// For database connections (if using external storage)
services.AddDbContext<ChartContext>(options =>
{
    options.UseSqlServer(connectionString,
        sqlOptions => sqlOptions.MaxPoolSize(20));
});
```

### Load Balancing

```nginx
# nginx configuration
upstream chart_backend {
    server localhost:5000 weight=3;
    server localhost:5001 weight=3;
    server localhost:5002 weight=3;
}

server {
    listen 80;
    location / {
        proxy_pass http://chart_backend;
        proxy_http_version 1.1;
        proxy_set_header Connection "";
        proxy_buffering off;
    }
}
```

## Monitoring

### Health Check Endpoint

```csharp
// In Program.cs
app.MapGet("/health", async (ChartEngine engine) =>
{
    // Verify core functionality
    var testChart = new Chart(ChartType.LineChart);
    testChart.Configuration.Title = "Health Check";
    
    var series = new ChartSeries("Test", "#FF6B6B");
    series.AddDataPoint(1, 10);
    testChart.AddSeries(series);
    
    var result = await engine.RenderChartAsync(testChart);
    
    return result.IsSuccessful ? Results.Ok() : Results.StatusCode(503);
});
```

### Prometheus Metrics

```csharp
// Enable metrics collection
services.AddSingleton<MetricsCollector>();

app.Use(async (context, next) =>
{
    var metrics = context.RequestServices.GetRequiredService<MetricsCollector>();
    metrics.RecordRequest(context.Request.Path);
    await next();
});

// Expose metrics endpoint
app.MapGet("/metrics", (MetricsCollector metrics) =>
{
    return metrics.GetPrometheusMetrics();
});
```

### Log Aggregation

```csharp
// Configure Serilog for centralized logging
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddFile("/var/log/chart-engine/app-{Date}.log");
});

// Or use structured logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("/var/log/chart-engine/app-{Date}.log",
        rollingInterval: RollingInterval.Day)
    .CreateLogger();
```

## Security

### HTTPS

```csharp
// Program.cs
app.UseHttpsRedirection();
app.UseHsts();
```

```bash
# Docker
docker run -d -p 443:443 \
  -v /path/to/cert.pem:/app/cert.pem \
  -v /path/to/key.pem:/app/key.pem \
  -e ASPNETCORE_Kestrel__Certificates__Default__Path=/app/cert.pem \
  -e ASPNETCORE_Kestrel__Certificates__Default__KeyPath=/app/key.pem \
  chart-engine:latest
```

### Rate Limiting

```csharp
// Add rate limiting middleware
app.UseRateLimiting();

services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
        httpContext => RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));
});
```

### CORS

```csharp
// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("production", builder =>
        builder.WithOrigins("https://yourdomain.com")
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials());
});

app.UseCors("production");
```

## Backup & Recovery

### Database Backup

```bash
# If using SQL Server
sqlcmd -S localhost -U sa -P YourPassword -Q "BACKUP DATABASE ChartDb TO DISK='/backups/chartdb.bak'"

# Scheduled backup (cron)
0 2 * * * /scripts/backup-database.sh
```

### File Backups

```bash
# Backup output directory
rsync -av /app/output/ /backups/chart-engine/

# S3 backup
aws s3 sync /app/output s3://my-bucket/chart-engine/
```

## Scaling

### Horizontal Scaling

```yaml
# kubernetes HPA
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: chart-engine-hpa
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: chart-engine
  minReplicas: 2
  maxReplicas: 10
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 70
  - type: Resource
    resource:
      name: memory
      target:
        type: Utilization
        averageUtilization: 80
```

### Caching Strategy

- Local cache (fast, per-instance)
- Distributed cache (Redis, shared)
- CDN (HTTP caching headers)

```csharp
// Redis distributed caching
services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = Configuration.GetConnectionString("Redis");
});
```

## Troubleshooting

### High Memory Usage

1. Reduce cache size
2. Limit concurrent renders
3. Clear old cached data
4. Monitor GC pressure

### Slow Rendering

1. Check CPU usage
2. Review chart complexity
3. Enable render caching
4. Consider GPU rendering

### Port Already in Use

```bash
# Linux/macOS
lsof -i :5000
kill -9 <PID>

# Windows
netstat -ano | findstr :5000
taskkill /PID <PID> /F
```

## Rollback Procedure

```bash
# Tag previous version
docker tag chart-engine:previous chart-engine:current

# Run previous version
docker-compose down
docker-compose up -d

# Verify health
curl http://localhost:5000/health
```

## Monitoring Checklist

- [ ] Health check passes
- [ ] Disk space adequate
- [ ] Memory usage stable
- [ ] CPU utilization normal
- [ ] Error rate low
- [ ] Response times acceptable
- [ ] Cache hit rate good
- [ ] Logs being collected
- [ ] Backups running
- [ ] Security policies enforced

---

For detailed environment-specific guidance, consult your infrastructure team or review cloud provider documentation.
