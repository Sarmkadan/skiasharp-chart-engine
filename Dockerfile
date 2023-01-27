# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project file
COPY ["skiasharp-chart-engine.csproj", "./"]

# Restore dependencies
RUN dotnet restore "skiasharp-chart-engine.csproj"

# Copy remaining source code
COPY . .

# Build application
RUN dotnet build "skiasharp-chart-engine.csproj" -c Release -o /app/build

# Publish application
RUN dotnet publish "skiasharp-chart-engine.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app

# Create non-root user
RUN useradd -m -u 1000 appuser

# Copy published application
COPY --from=build /app/publish .

# Create output directory with proper permissions
RUN mkdir -p /app/output && chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Expose port
EXPOSE 5000

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:5000/health || exit 1

# Environment variables
ENV ASPNETCORE_URLS=http://+:5000
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV CACHE_DURATION=300
ENV MAX_CONCURRENT_RENDERS=10

# Start application
ENTRYPOINT ["dotnet", "SkiaSharpChartEngine.dll"]
