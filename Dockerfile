# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files first for better layer caching
COPY TradeWise-csharp.sln .
COPY TradeWiseBackend.Api/*.csproj ./TradeWiseBackend.Api/
COPY TradeWiseBackend.Bll/*.csproj ./TradeWiseBackend.Bll/
COPY TradeWiseBackend.Dal/*.csproj ./TradeWiseBackend.Dal/
COPY TradeWiseBackend.Domain/*.csproj ./TradeWiseBackend.Domain/

# Restore NuGet packages
RUN dotnet restore

# Copy all source code
COPY . .

# Build the application
WORKDIR /src/TradeWiseBackend.Api
RUN dotnet build -c Release --no-restore

# Publish the application
RUN dotnet publish -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install curl for health checks (optional but recommended)
RUN apt-get update && \
    apt-get install -y --no-install-recommends curl && \
    rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=build /app/publish .

# Health check (adjust endpoint as needed)
HEALTHCHECK --interval=30s --timeout=3s --start-period=30s --retries=3 \
    CMD curl -f http://localhost:80/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "TradeWiseBackend.Api.dll"]