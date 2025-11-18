# ==============================
# Multi-stage Dockerfile for BookingCareManagement.Web (.NET 9)
# ==============================
# Build stage (restore + publish)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files separately for layer caching
COPY src/BookingCareManagement.Web/BookingCareManagement.csproj src/BookingCareManagement.Web/
COPY src/BookingCareManagement.Application/BookingCareManagement.Application.csproj src/BookingCareManagement.Application/
COPY src/BookingCareManagement.Domain/BookingCareManagement.Domain.csproj src/BookingCareManagement.Domain/
COPY src/BookingCareManagement.Infrastructure/BookingCareManagement.Infrastructure.csproj src/BookingCareManagement.Infrastructure/

# Restore
RUN dotnet restore src/BookingCareManagement.Web/BookingCareManagement.csproj

# Copy the rest of the source
COPY . .

# Publish (no apphost for smaller image)
RUN dotnet publish src/BookingCareManagement.Web/BookingCareManagement.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Environment configuration
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production

# Copy published output
COPY --from=build /app/publish .

# Expose HTTP port (Traefik / reverse proxy will handle HTTPS)
EXPOSE 8080

# Optional healthcheck hitting root (adjust if you add /health)
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 CMD wget -qO- http://localhost:8080/_routes || exit 1

ENTRYPOINT ["dotnet", "BookingCareManagement.dll"]
