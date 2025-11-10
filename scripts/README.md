Dev start scripts
=================

Two scripts help developers start the local environment without typing many commands:

- `scripts/dev-start.ps1` - PowerShell script for Windows (PowerShell Core/Windows PowerShell).
- `scripts/dev-start.sh`  - POSIX shell script for WSL/macOS/Linux.
- `scripts/dev-start.py`  - Cross-platform Python script alternative to start the dev environment.

What they do
-----------
- Ensure a `.env` file exists (copied from `.env.example` if missing).
- Start the database container with `docker compose up -d`.
- Wait until SQL Server is ready to accept connections.
- Run EF Core migrations: `dotnet ef database update -p src/BookingCareManagement.Infrastructure -s src/BookingCareManagement.Web`.
- Start the web app: `dotnet run --project src/BookingCareManagement.Web`.

Usage
-----
Windows (PowerShell):

```powershell
.\scripts\dev-start.ps1
```

WSL/macOS/Linux:

```bash
./scripts/dev-start.sh
```

Python (cross-platform):

```bash
python3 ./scripts/dev-start.py
```

Notes
-----
- The scripts assume Docker is installed and available on PATH.
- If you change SA_PASSWORD in `.env`, recreate the DB container with:
  ```
  docker compose down -v
  docker compose up -d
  ```
- Do not commit `.env` to git. Use `.env.example` as the template.
