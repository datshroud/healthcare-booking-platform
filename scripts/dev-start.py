#!/usr/bin/env python3
"""
dev-start.py

Cross-platform Python script to start the development environment:
- ensures .env exists (copies from .env.example if missing)
- starts the DB container (docker compose up -d)
- waits until SQL Server accepts connections
- runs EF Core migrations (dotnet ef database update)
- starts the web app (dotnet run)

Usage:
  python scripts/dev-start.py

This is a convenience developer script. It requires Docker and the .NET SDK
installed and available on PATH.
"""
from __future__ import annotations

import os
import shutil
import subprocess
import sys
import time
from pathlib import Path


ROOT = Path(__file__).resolve().parent.parent


def find_compose_cmd() -> list[str]:
    # Prefer `docker compose` (v2+) then fallback to `docker-compose` (v1)
    if shutil.which("docker"):
        try:
            # check if `docker compose` is available
            subprocess.run(["docker", "compose", "version"], stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL, check=True)
            return ["docker", "compose"]
        except Exception:
            pass
    if shutil.which("docker-compose"):
        return ["docker-compose"]
    raise RuntimeError("Neither 'docker compose' nor 'docker-compose' found on PATH")


def load_dotenv(path: Path) -> dict[str, str]:
    env = {}
    if not path.exists():
        return env
    for line in path.read_text().splitlines():
        line = line.strip()
        if not line or line.startswith("#"):
            continue
        if "=" not in line:
            continue
        k, v = line.split("=", 1)
        env[k.strip()] = v.strip().strip('\"').strip("\'")
    return env


def run(cmd: list[str], check=True, capture_output=False, env=None) -> subprocess.CompletedProcess:
    print("$", " ".join(cmd))
    return subprocess.run(cmd, check=check, capture_output=capture_output, env=env)


def main() -> int:
    os.chdir(ROOT)

    env_example = ROOT / ".env.example"
    env_file = ROOT / ".env"
    if not env_file.exists() and env_example.exists():
        shutil.copy(env_example, env_file)
        print("Created .env from .env.example. Please review .env if needed.")

    env = load_dotenv(env_file)
    sa_password = env.get("SA_PASSWORD", "BookingCare123!")

    compose_base = find_compose_cmd()

    # Ensure Docker daemon is available. If not, try to start Docker Desktop on Windows and poll.
    def docker_available() -> bool:
        try:
            cp = subprocess.run(["docker", "info"], stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL, check=False)
            return cp.returncode == 0
        except Exception:
            return False

    if not docker_available():
        if os.name == "nt":
            # try common Docker Desktop locations
            possible = [
                os.path.join(os.environ.get("ProgramFiles", "C:\\Program Files"), "Docker", "Docker", "Docker Desktop.exe"),
                os.path.join(os.environ.get("ProgramFiles", "C:\\Program Files"), "Docker", "Docker Desktop.exe"),
            ]
            started = False
            for p in possible:
                if os.path.exists(p):
                    print(f"Docker daemon not available — attempting to start Docker Desktop at: {p}")
                    try:
                        # start without waiting
                        subprocess.Popen([p], shell=False)
                        started = True
                        break
                    except Exception as e:
                        print("Failed to start Docker Desktop:", e)
            if started:
                print("Waiting for Docker daemon to become available...")
                for i in range(60):
                    if docker_available():
                        print("Docker daemon is available.")
                        break
                    time.sleep(2)
                else:
                    print("Timed out waiting for Docker daemon. Please start Docker Desktop manually.", file=sys.stderr)
                    return 2
        else:
            print("Docker daemon not available. Please ensure Docker is running and accessible on PATH.", file=sys.stderr)
            return 2

    # Start DB
    try:
        run(compose_base + ["up", "-d"], check=True)
    except subprocess.CalledProcessError as e:
        print("Failed to start containers:", e, file=sys.stderr)
        return 1

    # Wait for SQL Server readiness
    print("Waiting for SQL Server to be ready (this may take 30-60s)...")
    max_attempts = 60
    attempt = 0
    while attempt < max_attempts:
        attempt += 1
        try:
            cp = run([
                "docker",
                "exec",
                "bookingcare_mssql",
                "/opt/mssql-tools/bin/sqlcmd",
                "-S",
                "localhost",
                "-U",
                "sa",
                "-P",
                sa_password,
                "-Q",
                "SELECT 1",
            ], check=False, capture_output=True)
            if cp.returncode == 0:
                print("SQL Server is ready.")
                break
        except FileNotFoundError:
            print("docker executable not found while waiting for SQL Server", file=sys.stderr)
            return 2
        time.sleep(2)
        print(f"Waiting... ({attempt}/{max_attempts})")
    else:
        print("Timed out waiting for SQL Server to be ready", file=sys.stderr)
        return 3

    # Run EF migrations
    try:
        run(["dotnet", "ef", "database", "update", "-p", "src/BookingCareManagement.Infrastructure", "-s", "src/BookingCareManagement.Web"], check=True)
    except subprocess.CalledProcessError as e:
        print("EF database update failed:", e, file=sys.stderr)
        return 4

    # Start the web app
    try:
        run(["dotnet", "run", "--project", "src/BookingCareManagement.Web"], check=True)
    except subprocess.CalledProcessError as e:
        print("Failed to start web app:", e, file=sys.stderr)
        return 5

    return 0


if __name__ == "__main__":
    sys.exit(main())
