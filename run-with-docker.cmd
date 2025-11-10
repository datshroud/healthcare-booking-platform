@echo off
REM Wrapper to ensure Docker + migrations then run the web app
SETLOCAL

REM Run the ensure script (CMD version) and then dotnet run
call "%~dp0scripts\ensure-docker.cmd"
IF ERRORLEVEL 1 (
  echo Ensure script failed. Aborting.
  EXIT /B 1
)

dotnet run --project "src\BookingCareManagement.Web"
