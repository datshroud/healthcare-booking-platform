<#
Ensure Docker and SQL Server are available, start compose and apply EF migrations.
This script DOES NOT start the web app; it leaves the app to be started by the caller (IDE or manual dotnet run).
#>
Set-StrictMode -Version Latest
$scriptDir = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent
Set-Location -Path $scriptDir
Set-Location -Path ..

$logPath = Join-Path $scriptDir 'ensure-docker.log'
function Write-Log {
    param($message)
    $timestamp = (Get-Date).ToString('yyyy-MM-dd HH:mm:ss')
    $line = "[$timestamp] $message"
    Add-Content -Path $logPath -Value $line
}

try {
    Set-Content -Path $logPath -Value "[$((Get-Date).ToString('yyyy-MM-dd HH:mm:ss'))] ensure-docker.ps1 started"
    Write-Log "Script working directory: $(Get-Location)"
    function Test-Docker {
        try { docker info > $null 2>&1; return $true } catch { return $false }
    }

    function Start-DockerDesktop {
        param(
            [switch] $Force
        )

        if (Test-Docker) { return $true }

    Write-Log "Docker daemon not available. Attempting to start Docker Desktop"
    Write-Host "Docker daemon not available. Attempting to start Docker Desktop..." -ForegroundColor Yellow

        $started = $false

        # 1. Try Windows service if installed (requires admin rights)
        $svc = Get-Service -Name 'com.docker.service' -ErrorAction SilentlyContinue
        if (-not $svc) { $svc = Get-Service -Name 'docker' -ErrorAction SilentlyContinue }
        if ($svc) {
            try {
                if ($svc.Status -ne 'Running') {
                    Write-Log ("Starting Docker service {0}" -f $svc.Name)
                    Write-Host ("Starting Docker service '{0}'..." -f $svc.Name)
                    Start-Service -InputObject $svc -ErrorAction Stop
                }
                Start-Sleep -Seconds 2
                if (Test-Docker) {
                    Write-Log "Docker daemon became ready via service"
                    Write-Host "Docker daemon is up (service)." -ForegroundColor Green
                    return $true
                }
            }
            catch {
                Write-Log ("Failed to start service: {0}" -f $_.Exception.Message)
                Write-Host ("Failed to start Docker service: {0}" -f $_.Exception.Message) -ForegroundColor Yellow
            }
        }

        # 2. Try launching Docker Desktop executable from common locations.
        $paths = @(
            "$env:ProgramFiles\\Docker\\Docker\\Docker Desktop.exe",
            "$env:ProgramFiles(x86)\\Docker\\Docker\\Docker Desktop.exe",
            "$env:LOCALAPPDATA\\Programs\\Docker\\Docker\\Docker Desktop.exe",
            "C:\\Program Files\\Docker\\Docker\\Docker Desktop.exe",
            "C:\\Program Files (x86)\\Docker\\Docker\\Docker Desktop.exe"
        ) | Where-Object { $_ -and (Test-Path $_) }

        foreach ($exe in $paths) {
            Write-Log ("Launching Docker Desktop via {0}" -f $exe)
            Write-Host ("Launching Docker Desktop via '{0}'" -f $exe)
            try {
                # Use cmd /c start so Docker Desktop can display UI if needed and return immediately.
                $quoted = '"' + $exe + '"'
                Start-Process -FilePath "cmd.exe" -ArgumentList '/c', 'start', '""', $quoted -WindowStyle Hidden -ErrorAction Stop
                $started = $true
                break
            }
            catch {
                Write-Log ("Failed to launch {0}: {1}" -f $exe, $_.Exception.Message)
                Write-Host ("Failed to launch '{0}': {1}" -f $exe, $_.Exception.Message) -ForegroundColor Yellow
            }
        }

        if (-not $started -and $Force) {
            foreach ($exe in $paths) {
                try {
                    Write-Log ("Retrying with elevation for {0}" -f $exe)
                    Write-Host ("Retrying with elevation: {0}" -f $exe)
                    Start-Process -FilePath $exe -Verb RunAs -ErrorAction Stop
                    $started = $true
                    break
                }
                catch {
                    Write-Log ("Failed elevated launch {0}: {1}" -f $exe, $_.Exception.Message)
                    Write-Host ("Failed elevated launch for '{0}': {1}" -f $exe, $_.Exception.Message) -ForegroundColor Yellow
                }
            }
        }

        if (-not $started) {
            Write-Log "Could not automatically launch Docker Desktop"
            Write-Host "Could not automatically launch Docker Desktop. Please start it manually." -ForegroundColor Red
            return $false
        }

        Write-Log "Waiting for Docker daemon to be ready"
        Write-Host "Waiting for Docker daemon to be ready..."
        for ($i = 1; $i -le 90; $i++) {
            if (Test-Docker) {
                Write-Log "Docker daemon reported ready"
                Write-Host "Docker daemon is ready." -ForegroundColor Green
                return $true
            }
            Start-Sleep -Seconds 2
            Write-Host ("Waiting for Docker... ({0}/90)" -f $i)
            Write-Log ("Waiting loop iteration {0}" -f $i)
        }

        throw 'Timed out waiting for Docker daemon.'
    }

    if (-not (Test-Docker)) {
        Write-Log "Docker not ready on initial check"
        if (-not (Start-DockerDesktop)) {
            Write-Log "Start-DockerDesktop returned false"
            throw 'Docker daemon not available.'
        }
    }

    function Get-ComposeCommand {
        if (Get-Command "docker" -ErrorAction SilentlyContinue) {
            try {
                docker compose version > $null 2>&1
                return @{ Cmd = 'docker'; Args = @('compose') }
            }
            catch { }
        }

        if (Get-Command "docker-compose" -ErrorAction SilentlyContinue) {
            return @{ Cmd = 'docker-compose'; Args = @() }
        }

        throw 'Could not find docker compose command.'
    }

    $compose = Get-ComposeCommand
    Write-Log ("Compose command: {0} {1}" -f $compose.Cmd, ($compose.Args -join ' '))
    $composeDisplay = ($compose.Args.Count -gt 0) ? ("{0} {1}" -f $compose.Cmd, ($compose.Args -join ' ')) : $compose.Cmd
    Write-Host ("Using compose command: {0}" -f $composeDisplay)

    # Ensure .env exists
    if (-not (Test-Path '.env') -and (Test-Path '.env.example')) {
        Copy-Item '.env.example' '.env'
        Write-Log "Created .env from .env.example"
        Write-Host "Created .env from .env.example" -ForegroundColor Yellow
    }

    function Get-EnvVars {
        param($path)
        $dict = @{}
        if (-not (Test-Path $path)) { return $dict }
        Get-Content $path | ForEach-Object {
            $line = $_.Trim()
            if (-not $line -or $line.StartsWith('#')) { return }
            $parts = $line -split '=', 2
            if ($parts.Length -eq 2) {
                $dict[$parts[0].Trim()] = $parts[1].Trim()
            }
        }
        return $dict
    }

    Write-Log "Loading environment variables"
    $envVars = Get-EnvVars '.env'
    $script:saPassword = $envVars['SA_PASSWORD']
    if (-not $script:saPassword) { $script:saPassword = 'BookingCare123!' }

    Write-Log "Executing docker compose up -d"
    Write-Host "Starting database container (docker compose up -d)..."
    & $compose.Cmd @($compose.Args + @('up', '-d'))

    $script:dbPort = 1433
    if ($envVars.ContainsKey('MSSQL_PORT')) {
        [int]::TryParse($envVars['MSSQL_PORT'], [ref]$parsed) | Out-Null
        if ($parsed) { $script:dbPort = $parsed }
    }

    function Test-SqlReady {
        param(
            [int] $Port
        )

        $ready = $false
        $sqlcmdExists = $false

        try {
            docker exec bookingcare_mssql test -x /opt/mssql-tools/bin/sqlcmd > $null 2>&1
            $sqlcmdExists = ($LASTEXITCODE -eq 0)
        } catch { }

        if ($sqlcmdExists) {
            try {
                docker exec bookingcare_mssql /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $script:saPassword -Q "SELECT 1" > $null 2>&1
                if ($LASTEXITCODE -eq 0) { return @{ Ready = $true; Sqlcmd = $true; Tcp = $false; Tnc = $false; Logs = $false } }
            } catch { }
        }
        else {
            try {
                $tcp = New-Object System.Net.Sockets.TcpClient
                $async = $tcp.BeginConnect('127.0.0.1', $Port, $null, $null)
                $connected = $async.AsyncWaitHandle.WaitOne(2000)
                if ($connected -and $tcp.Connected) { $tcp.Close(); $ready = $true }
            } catch { }

            if (-not $ready) {
                try {
                    $tnc = Test-NetConnection -ComputerName '127.0.0.1' -Port $Port -WarningAction SilentlyContinue
                    if ($tnc -and $tnc.TcpTestSucceeded) { $ready = $true }
                } catch { }
            }

            $logsReady = $false
            try {
                $logs = docker logs --tail 100 bookingcare_mssql 2>$null
                if ($logs -and ($logs -match 'SQL Server is now ready for client connections')) { $logsReady = $true; $ready = $true }
            } catch { }

            return @{ Ready = $ready; Sqlcmd = $false; Tcp = $ready; Tnc = $ready; Logs = $logsReady }
        }

        return @{ Ready = $false; Sqlcmd = $sqlcmdExists; Tcp = $false; Tnc = $false; Logs = $false }
    }

    Write-Log "Waiting for SQL Server readiness"
    Write-Host "Waiting for SQL Server to be ready (this may take 20-60s)..."
    $maxAttempts = 90
    for ($attempt = 1; $attempt -le $maxAttempts; $attempt++) {
        $status = Test-SqlReady -Port $script:dbPort
        if ($status.Ready) {
            Write-Log "SQL Server reported ready"
            Write-Host "SQL Server is ready." -ForegroundColor Green
            break
        }

        Write-Log ("Attempt {0}: sqlcmd={1} tcp={2} logs={3}" -f $attempt, $status.Sqlcmd, $status.Tcp, $status.Logs)
        Write-Host ("Attempt {0}/{1} — sqlcmd={2} tcp={3} logs={4}" -f $attempt, $maxAttempts, $status.Sqlcmd, $status.Tcp, $status.Logs)
        Start-Sleep -Seconds 3
    }

    if ($attempt -gt $maxAttempts) {
        Write-Log "Timed out waiting for SQL Server"
        Write-Host "Timed out waiting for SQL Server. Dumping diagnostics..." -ForegroundColor Red
        try { docker ps -a --filter "name=bookingcare_mssql" } catch { }
        try { docker logs --tail 200 bookingcare_mssql } catch { }
        throw 'Timed out waiting for SQL Server to accept connections.'
    }

    Write-Log "Running dotnet ef database update"
    Write-Host "Applying EF Core migrations..."
    dotnet ef database update -p src/BookingCareManagement.Infrastructure -s src/BookingCareManagement.Web

    Write-Log "Ensure script completed successfully"
    Write-Host "Ensure script completed successfully." -ForegroundColor Green

} catch {
    Write-Log ("ERROR: {0}" -f $_.Exception.Message)
    Write-Host "ERROR in ensure-docker.ps1: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host $_.Exception.ToString()
    exit 1
} finally {
    Write-Log "ensure-docker.ps1 finished"
}
