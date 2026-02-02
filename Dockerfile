# ================================================================
# NonProfit Finance - Multi-stage Docker Build
# ================================================================

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy the entire project structure (including Components, Models, Services, DTOs)
COPY [".", "./"]

# Restore NuGet packages
RUN dotnet restore "NonProfitFinance.csproj"

# Build the project
RUN dotnet build "NonProfitFinance.csproj" -c Release -o /app/build

# ================================================================
# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "NonProfitFinance.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# ================================================================
# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Install dependencies for OCR and health checks
RUN apt-get update && apt-get install -y \
    tesseract-ocr \
    libtesseract-dev \
    libleptonica-dev \
    curl \
    ca-certificates \
    && rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=publish /app/publish .

# Create directories for data persistence
RUN mkdir -p /app/data /app/backups /app/documents && \
    chmod 777 /app/data /app/backups /app/documents

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Set non-root user for security (optional but recommended)
# RUN useradd -m appuser && chown -R appuser:appuser /app
# USER appuser

ENTRYPOINT ["dotnet", "NonProfitFinance.dll"]

