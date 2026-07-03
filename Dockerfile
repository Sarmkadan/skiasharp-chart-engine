# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# Dockerfile for SkiaSharp Chart Engine
# =============================================================================

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

# Copy project file and solution
COPY ["skiasharp-chart-engine.csproj", "./"]
COPY ["*.sln", "./"]

# Restore dependencies
RUN dotnet restore "skiasharp-chart-engine.csproj"

# Copy source code (after restore to leverage Docker cache)
COPY ["./", "./"]

# Build application
RUN dotnet build "skiasharp-chart-engine.csproj" -c Release -o /app/build \
--no-restore

# Publish application
RUN dotnet publish "skiasharp-chart-engine.csproj" -c Release -o /app/publish \
--no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine
WORKDIR /app

# Create non-root user
RUN adduser -D -u 1000 appuser

# Install required runtime dependencies for SkiaSharp
RUN apk add --no-cache icu-libs krb5-libs libgcc libintl libssl1.1 libstdc++ zlib \
  harfbuzz \
  libpng \
  libjpeg-turbo \
  freetype \
  lcms2

# Copy published application
COPY --from=build /app/publish .

# Create output directory with proper permissions
RUN mkdir -p /app/output && \
mkdir -p /app/cache && \
mkdir -p /app/config && \
chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Expose port 8080
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=30s --retries=3 \
CMD wget --no-verbose --tries=1 --spider http://localhost:8080/health || exit 1

# Environment variables
ENV ASPNETCORE_URLS=http://+:8080 \
  DOTNET_RUNNING_IN_CONTAINER=true \
  CACHE_DURATION=300 \
  MAX_CONCURRENT_RENDERS=10 \
  ASPNETCORE_ENVIRONMENT=Production

# Start application
ENTRYPOINT ["dotnet", "SkiaSharpChartEngine.dll"]