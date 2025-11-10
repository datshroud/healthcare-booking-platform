#!/usr/bin/env bash
set -euo pipefail

# Ensure Docker is running, start compose, wait SQL, and apply EF migrations. Does not start the web app.
ROOT_DIR="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT_DIR"

if ! docker info >/dev/null 2>&1; then
  echo "Docker daemon not available. Please start Docker Desktop or Docker Engine and re-run."
  exit 1
fi

echo "Starting DB container..."
if docker compose version >/dev/null 2>&1; then
  COMPOSE_CMD="docker compose"
else
  COMPOSE_CMD="docker-compose"
fi

$COMPOSE_CMD up -d

SA_PASSWORD=${SA_PASSWORD:-BookingCare123!}
if [ -f .env ]; then
  # shellcheck disable=SC1091
  . .env
  SA_PASSWORD=${SA_PASSWORD:-$SA_PASSWORD}
fi

echo "Waiting for SQL Server to be ready..."
ATTEMPTS=0
MAX=60
until docker exec bookingcare_mssql /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -Q "SELECT 1" >/dev/null 2>&1; do
  ATTEMPTS=$((ATTEMPTS+1))
  if [ $ATTEMPTS -ge $MAX ]; then
    echo "Timed out waiting for SQL Server" >&2
    exit 2
  fi
  echo "Waiting for SQL Server... ($ATTEMPTS/$MAX)"
  sleep 2
done

echo "Applying EF Core migrations..."
dotnet ef database update -p src/BookingCareManagement.Infrastructure -s src/BookingCareManagement.Web
