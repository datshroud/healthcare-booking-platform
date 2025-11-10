@echo off
setlocal ENABLEDELAYEDEXPANSION

rem Ensure Docker and SQL Server are ready, then apply EF Core migrations (Windows CMD version).

set "SCRIPT_DIR=%~dp0"
pushd "%SCRIPT_DIR%.."

set "LOG=%SCRIPT_DIR%ensure-docker.log"
>"%LOG%" echo [%date% %time%] ensure-docker.cmd started
call :log "Working directory: %CD%"

set "DOCKER_EXE="
for /f "delims=" %%D in ('where docker 2^>nul') do (
    if not defined DOCKER_EXE (
        set "DOCKER_EXE=%%~fD"
        if /I not "%%~xD"==".exe" if exist "%%~fD.exe" set "DOCKER_EXE=%%~fD.exe"
    )
)

if not defined DOCKER_EXE (
    call :log "ERROR: 'docker' command not found. Install Docker Desktop and ensure it is on PATH."
    goto :fail
)

call :log "docker executable: %DOCKER_EXE%"

call :check_docker_ready
set "DOCKER_READY=%ERRORLEVEL%"
if not "%DOCKER_READY%"=="0" (
    call :log "Docker daemon not running; attempting to start it"
    call :start_docker
    if errorlevel 1 goto :fail
)

:after_docker_check
call :log "Docker available; continuing startup"

call :ensure_env
call :detect_compose
if errorlevel 1 goto :fail

call :log "Starting database container"
if "%COMPOSE_CMD%"=="docker" (
    docker compose up -d
) else (
    docker-compose up -d
)
if errorlevel 1 goto :fail

call :wait_sql_ready
if errorlevel 1 goto :fail

call :log "Applying EF Core migrations"
dotnet ef database update -p src\BookingCareManagement.Infrastructure -s src\BookingCareManagement.Web
if errorlevel 1 goto :fail

call :log "ensure-docker completed successfully"
popd
endlocal
exit /b 0

:fail
set "ERR=%ERRORLEVEL%"
call :log "ensure-docker failed with exit code %ERR%"
popd
endlocal
exit /b %ERR%

:log
setlocal ENABLEDELAYEDEXPANSION
set "MSG=%~1"
echo(!MSG!
>>"%LOG%" echo [%date% %time%] !MSG!
endlocal
exit /b 0

:check_docker_ready
call :log "Checking docker info"
set "_DOCKER_TMP=%SCRIPT_DIR%docker-info.tmp"
del /F /Q "!_DOCKER_TMP!" >nul 2>&1
"%DOCKER_EXE%" info >"!_DOCKER_TMP!" 2>&1
set "EL=!ERRORLEVEL!"
call :log "docker info exit code !EL!"
if "!EL!"=="0" (
    del /F /Q "!_DOCKER_TMP!" >nul 2>&1
    exit /b 0
)
call :log "docker info failed; dumping captured output"
if exist "!_DOCKER_TMP!" type "!_DOCKER_TMP!" >>"%LOG%"
del /F /Q "!_DOCKER_TMP!" >nul 2>&1
exit /b 1

:wait_docker_daemon
call :log "Waiting for docker service to report ready"
for /L %%I in (1,1,90) do (
    "%DOCKER_EXE%" info >nul 2>&1
    if not errorlevel 1 (
        call :log "Docker daemon is ready"
        exit /b 0
    )
    timeout /t 2 /nobreak >nul
    call :log "Waiting for Docker daemon (%%I/90)"
)
call :log "Timed out waiting for Docker daemon"
exit /b 1

:start_docker
set "STARTED="
set "SERVICE_ATTEMPTED="
for %%S in (com.docker.service docker) do (
    sc query %%S >nul 2>&1
    if not errorlevel 1 (
        set "SERVICE_ATTEMPTED=1"
        for /f "tokens=3 delims=: " %%T in ('sc query %%S ^| find "STATE"') do (
            if /I not "%%T"=="RUNNING" (
                call :log "Attempting to start service %%S"
                net start %%S >nul 2>&1
            ) else (
                call :log "Service %%S already running"
            )
        )
    )
)

if defined SERVICE_ATTEMPTED (
    call :wait_docker_daemon
    if not errorlevel 1 exit /b 0
)

for %%P in ("%ProgramFiles%\Docker\Docker\Docker Desktop.exe" "C:\Program Files\Docker\Docker\Docker Desktop.exe" "C:\Program Files (x86)\Docker\Docker\Docker Desktop.exe" "%ProgramFiles(x86)%\Docker\Docker\Docker Desktop.exe" "%LOCALAPPDATA%\Programs\Docker\Docker\Docker Desktop.exe") do (
    if exist %%~fP (
        call :log "Launching Docker Desktop: %%~fP"
        start "" "%%~fP"
        set "STARTED=1"
        goto after_launch
    )
)

:after_launch
if defined STARTED (
    call :wait_docker_daemon
    exit /b %ERRORLEVEL%
)

call :log "Could not find Docker Desktop executable; please start Docker manually"
exit /b 1

:ensure_env
call :log "Ensuring .env exists"
if not exist .env (
    if exist .env.example (
        copy /Y .env.example .env >nul
        call :log "Created .env from .env.example"
    )
)

set "SA_PASSWORD=BookingCare123^!"
set "MSSQL_PORT=1433"
if exist .env (
    call :log "Loading values from .env"
    for /f "usebackq tokens=1,* delims==" %%A in (".env") do (
        set "KEY=%%A"
        set "VALUE=%%B"
        if defined VALUE (
            set "VALUE=!VALUE!"
            if "!VALUE:~0,1!"=="'" set "VALUE=!VALUE:~1!"
            if "!VALUE:~-1!"=="'" set "VALUE=!VALUE:~0,-1!"
            if "!VALUE:~0,1!"=="\"" set "VALUE=!VALUE:~1!"
            if "!VALUE:~-1!"=="\"" set "VALUE=!VALUE:~0,-1!"
            set "VALUE=!VALUE:!=^!!"
        )
        if /I "!KEY!"=="SA_PASSWORD" set "SA_PASSWORD=!VALUE!"
        if /I "!KEY!"=="MSSQL_PORT" set "MSSQL_PORT=!VALUE!"
    )
)
call :log "Using MSSQL_PORT: !MSSQL_PORT!"
exit /b 0

:detect_compose
set "COMPOSE_CMD="
call :log "Detecting docker compose availability"
docker compose version >nul 2>&1
if not errorlevel 1 (
    set "COMPOSE_CMD=docker"
    call :log "Using 'docker compose'"
    exit /b 0
)

docker-compose --version >nul 2>&1
if errorlevel 1 (
    call :log "docker compose command not found"
    exit /b 1
)

set "COMPOSE_CMD=docker-compose"
call :log "Using legacy docker-compose"
exit /b 0

:wait_sql_ready
for /L %%I in (1,1,90) do (
    docker exec bookingcare_mssql /bin/sh -c "test -x /opt/mssql-tools/bin/sqlcmd" >nul 2>&1
    if not errorlevel 1 (
        docker exec bookingcare_mssql /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "!SA_PASSWORD!" -Q "SELECT 1" >nul 2>&1
        if not errorlevel 1 (
            call :log "SQL Server responded to sqlcmd"
            exit /b 0
        )
    )

    docker logs --tail 50 bookingcare_mssql 2>nul | find "SQL Server is now ready for client connections" >nul
    if not errorlevel 1 (
        call :log "SQL Server ready based on container logs"
        exit /b 0
    )

    call :log "Waiting for SQL Server (%%I/90)"
    timeout /t 3 /nobreak >nul
)

call :log "Timed out waiting for SQL Server readiness"
exit /b 1
