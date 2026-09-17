# ── Build Stage ──────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY SmartInventory.Domain/SmartInventory.Domain.csproj SmartInventory.Domain/
COPY SmartInventory.Application/SmartInventory.Application.csproj SmartInventory.Application/
COPY SmartInventory.Infrastructure/SmartInventory.Infrastructure.csproj SmartInventory.Infrastructure/
COPY SmartInventory.API/SmartInventory.API.csproj SmartInventory.API/

RUN dotnet restore SmartInventory.API/SmartInventory.API.csproj

# Copy source code and build/publish Release
COPY . .
RUN dotnet publish SmartInventory.API/SmartInventory.API.csproj -c Release -o /app/publish --no-restore

# ── Runtime Stage ───────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# Run as non-root user (built-in 'app' user in .NET 10 runtime images)
USER app

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "SmartInventory.API.dll"]
