<#
Starts the development environment for BookingCareManagement:
- ensures .env exists (copies from .env.example if missing)
- starts the DB container (docker compose up -d)
- waits until SQL Server accepts connections
- runs EF Core migrations (dotnet ef database update)
- starts the web app (dotnet run)

Run from repository root (PowerShell):
  .\scripts\dev-start.ps1
#>

Set-StrictMode -Version Latest
$scriptDir = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent
Set-Location -Path $scriptDir
Set-Location -Path ..

function Get-ComposeCmd {
    # Return an object with Cmd and Arg so PowerShell can invoke multi-word commands correctly.
    if (Get-Command "docker" -ErrorAction SilentlyContinue) {
        try {
            docker compose version > $null 2>&1
            return @{ Cmd = 'docker'; Arg = 'compose' }
        }
        catch { }
    }
    if (Get-Command "docker-compose" -ErrorAction SilentlyContinue) {
        return @{ Cmd = 'docker-compose'; Arg = $null }
    }
    throw 'Neither "docker compose" nor "docker-compose" found on PATH.'
}

# ensure .env exists
if (-not (Test-Path .env)) {
    if (Test-Path .env.example) {
        Copy-Item .env.example .env
        Write-Host "Created .env from .env.example. Please review .env before continuing." -ForegroundColor Yellow
    }
}

# load simple KEY=VALUE pairs from .env
function Load-EnvFile($path) {
    $dict = @{}
    if (-not (Test-Path $path)) { return $dict }
    Get-Content $path | ForEach-Object {
        $_ = $_.Trim()
        if ($_.StartsWith('#') -or $_ -eq '') { return }
        $parts = $_ -split '=', 2
        if ($parts.Count -eq 2) { $dict[$parts[0].Trim()] = $parts[1].Trim() }
    }
    return $dict
}


$envVars = Load-EnvFile '.env'
$sa = $envVars['SA_PASSWORD'] 
if (-not $sa) { $sa = 'BookingCare123!' }

$composeObj = Get-ComposeCmd
if ($composeObj.Arg) { Write-Host "Using compose command: $($composeObj.Cmd) $($composeObj.Arg)" }
else { Write-Host "Using compose command: $($composeObj.Cmd)" }

# If Docker daemon isn't available, try to start Docker Desktop on Windows
function Test-Docker {
    try {
        docker info > $null 2>&1
        return $true
    }
    catch { return $false }
}

if (-not (Test-Docker)) {
    Write-Host "Docker daemon not available. Attempting to start Docker Desktop (Windows)..." -ForegroundColor Yellow
    $possible = @("$env:ProgramFiles\Docker\Docker\Docker Desktop.exe", "$env:ProgramFiles\Docker\Docker Desktop.exe")
    $started = $false
    foreach ($p in $possible) {
        if (Test-Path $p) {
            Write-Host "Starting Docker Desktop: $p"
            Start-Process -FilePath $p -WindowStyle Hidden
            $started = $true
            break
        }
    }
    if ($started) {
        Write-Host "Waiting for Docker daemon to be ready..."
        $attempt = 0
        while ($attempt -lt 60) {
            if (Test-Docker) { Write-Host "Docker is ready." -ForegroundColor Green; break }
            Start-Sleep -Seconds 2
            $attempt++
            Write-Host "Waiting for docker... ($attempt/60)"
        }
        if ($attempt -ge 60) { throw 'Timed out waiting for Docker daemon.' }
    }
    else {
        Write-Host "Could not find Docker Desktop executable. Please start Docker manually." -ForegroundColor Red
        throw 'Docker daemon not available.'
    }
}

Write-Host "Starting database container..."
if ($composeObj.Arg) {
    & $composeObj.Cmd $composeObj.Arg 'up' '-d'
}
else {
    & $composeObj.Cmd 'up' '-d'
}

$WriteHostMsg = "Waiting for SQL Server to be ready (this may take 20-60s)..."
Write-Host $WriteHostMsg
$maxAttempts = 60
$attempt = 0
while ($attempt -lt $maxAttempts) {
    $ready = $false
    try {
        # Prefer sqlcmd inside the container if present
        try { docker exec bookingcare_mssql test -x /opt/mssql-tools/bin/sqlcmd > $null 2>&1; $sqlcmdExists = ($LASTEXITCODE -eq 0) } catch { $sqlcmdExists = $false }

        if ($sqlcmdExists) {
            try {
                docker exec bookingcare_mssql /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "$sa" -Q "SELECT 1" > $null 2>&1
                if ($LASTEXITCODE -eq 0) { $ready = $true }
            } catch { }
        }
        else {
            # Fall back to a TCP connect test from host to mapped port
            $port = 1433
            if ($envVars.ContainsKey('MSSQL_PORT')) { [int]::TryParse($envVars['MSSQL_PORT'], [ref]$parsed) | Out-Null; if ($parsed) { $port = $parsed } }

            try {
                $tcp = New-Object System.Net.Sockets.TcpClient
                $async = $tcp.BeginConnect('127.0.0.1', $port, $null, $null)
                $connected = $async.AsyncWaitHandle.WaitOne(2000)
                if ($connected -and $tcp.Connected) { $tcp.Close(); $ready = $true }
            } catch { }

            if (-not $ready) {
                try {
                    $tnc = Test-NetConnection -ComputerName '127.0.0.1' -Port $port -WarningAction SilentlyContinue
                    if ($tnc -and $tnc.TcpTestSucceeded) { $ready = $true }
                } catch { }
            }
        
        # Also check container logs for the standard readiness message as a final fallback
        try {
            $logs = docker logs --tail 100 bookingcare_mssql 2>$null
            if ($logs -and ($logs -match 'SQL Server is now ready for client connections')) { $ready = $true }
        } catch { }
        }
    }
    catch {
        # ignore transient errors
    }

    # Final safety: check container logs again outside the try/catch in case earlier checks failed
    try {
        $allLogs = docker logs --tail 200 bookingcare_mssql 2>$null
        if ($allLogs -and ($allLogs -match 'SQL Server is now ready for client connections')) { $ready = $true }
    } catch { }

    if ($ready) { Write-Host "SQL Server is ready." -ForegroundColor Green; break }

    # Print per-attempt diagnostics to help debug why readiness wasn't detected
    $sqlcmdExists = ($sqlcmdExists -eq $true)
    $tcpConnected = $false
    $tncSucceeded = $false
    $logsReady = $false
    try {
        if (-not $sqlcmdExists) {
            try {
                $tcp = New-Object System.Net.Sockets.TcpClient
                $async = $tcp.BeginConnect('127.0.0.1', $port, $null, $null)
                $connected = $async.AsyncWaitHandle.WaitOne(1000)
                if ($connected -and $tcp.Connected) { $tcpConnected = $true; $tcp.Close() }
            } catch { }
            try { $tnc = Test-NetConnection -ComputerName '127.0.0.1' -Port $port -WarningAction SilentlyContinue; if ($tnc -and $tnc.TcpTestSucceeded) { $tncSucceeded = $true } } catch { }
        }
        try { $logs = docker logs --tail 20 bookingcare_mssql 2>$null; if ($logs -and ($logs -match 'SQL Server is now ready for client connections')) { $logsReady = $true } } catch { }
    } catch { }

    Write-Host "Attempt $($attempt+1)/$maxAttempts — sqlcmdExists=$sqlcmdExists tcp=$tcpConnected tnc=$tncSucceeded logsReady=$logsReady"

    $attempt++
    Start-Sleep -Seconds 4
    Write-Host "Waiting... ($attempt/$maxAttempts)"
}
if ($attempt -ge $maxAttempts) {
    Write-Host "Timed out waiting for SQL Server to be ready. Gathering diagnostics..." -ForegroundColor Red

    Write-Host "`nContainer status (docker ps -a --filter name=bookingcare_mssql):" -ForegroundColor Yellow
    try { docker ps -a --filter "name=bookingcare_mssql" } catch {}

    Write-Host "`nCompose logs for service 'mssql' (last 200 lines):" -ForegroundColor Yellow
    try {
        if ($composeObj.Arg) {
            & $composeObj.Cmd $composeObj.Arg 'logs' 'mssql' '--no-color' '--tail' '200'
        }
        else {
            & $composeObj.Cmd 'logs' 'mssql' '--no-color' '--tail' '200'
        }
    }
    catch {}

    Write-Host "`nDocker logs for container 'bookingcare_mssql' (tail 200):" -ForegroundColor Yellow
    try { docker logs --tail 200 bookingcare_mssql } catch {}

    Write-Host "`nIf you changed SA_PASSWORD in .env after creating the container, recreate the container with:`n  docker compose down -v && docker compose up -d" -ForegroundColor Yellow

    throw 'Timed out waiting for SQL Server to be ready. See diagnostics above.'
}

Write-Host "Applying EF Core migrations..."
dotnet ef database update -p src/BookingCareManagement.Infrastructure -s src/BookingCareManagement.Web

Write-Host "Starting web app..."
dotnet run --project src/BookingCareManagement.Web
