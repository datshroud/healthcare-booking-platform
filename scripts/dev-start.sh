#!/usr/bin/env bash
set -euo pipefail

# dev-start.sh - start DB (docker compose), wait for readiness, apply EF migrations, run app
# Run from repository root: ./scripts/dev-start.sh

ROOT_DIR="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT_DIR"

if [ ! -f .env ] && [ -f .env.example ]; then
  cp .env.example .env
  echo "Created .env from .env.example. Edit .env before continuing if needed."
fi

# load .env (simple KEY=VALUE)
set -a
if [ -f .env ]; then
  # shellcheck disable=SC1091
  . .env
fi
set +a

COMPOSE_CMD=""
if docker compose version >/dev/null 2>&1; then
  COMPOSE_CMD="docker compose"
elif command -v docker-compose >/dev/null 2>&1; then
  COMPOSE_CMD="docker-compose"
else
  echo "docker compose or docker-compose not found on PATH" >&2
  exit 1
fi

echo "Using compose: $COMPOSE_CMD"

echo "Starting database container..."
$COMPOSE_CMD up -d

SA_PASSWORD=${SA_PASSWORD:-BookingCare123!}

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

echo "Starting web app..."
dotnet run --project src/BookingCareManagement.Web
