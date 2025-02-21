# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:7.0-bullseye-slim AS build
WORKDIR /src

# Copy solution and project files first for better layer caching
COPY ["src/Backend.API/Backend.API.csproj", "Backend.API/"]
COPY ["src/Backend.Core/Backend.Core.csproj", "Backend.Core/"]
COPY ["src/Backend.Infrastructure/Backend.Infrastructure.csproj", "Backend.Infrastructure/"]

# Restore packages with specific version lock
RUN dotnet restore "Backend.API/Backend.API.csproj" --locked-mode

# Copy all source code
COPY ["src/Backend.API/", "Backend.API/"]
COPY ["src/Backend.Core/", "Backend.Core/"]
COPY ["src/Backend.Infrastructure/", "Backend.Infrastructure/"]

# Build and publish
WORKDIR "/src/Backend.API"
RUN dotnet build "Backend.API.csproj" -c Release -o /app/build \
    && dotnet test "Backend.API.csproj" -c Release --no-build \
    && dotnet publish "Backend.API.csproj" -c Release -o /app/publish \
    --no-restore \
    --self-contained true \
    /p:PublishTrimmed=true \
    /p:PublishSingleFile=true

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:7.0-bullseye-slim AS runtime

# Create non-root user and group
RUN groupadd -r appgroup && useradd -r -g appgroup appuser

# Set working directory
WORKDIR /app

# Copy published files from build stage
COPY --from=build /app/publish .

# Set appropriate file permissions
RUN chown -R appuser:appgroup /app \
    && chmod -R 500 /app \
    && chmod -R 600 /app/*.json

# Configure environment variables
ENV ASPNETCORE_URLS=http://+:80 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1 \
    DOTNET_CONSOLE_LOG_JSON=true \
    DOTNET_gcServer=1 \
    DOTNET_GCHeapHardLimit=1024MB

# Configure health check
HEALTHCHECK --interval=30s --timeout=5s --start-period=40s --retries=3 \
    CMD curl -f http://localhost/health || exit 1

# Expose port
EXPOSE 80

# Set security configurations
RUN chmod 500 /app/Backend.API
USER appuser:appgroup

# Set entry point with graceful shutdown
ENTRYPOINT ["./Backend.API"]

# Labels for container metadata
LABEL maintainer="Development Team" \
    description="Backend API service for Customer Management System" \
    version="1.0.0"

# Resource limits (to be enforced by container runtime)
# Memory: 1024M
# CPU: 1.0