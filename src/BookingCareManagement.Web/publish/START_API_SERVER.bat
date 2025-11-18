@echo off
color 0A
title BookingCare API Server

echo =====================================
echo   BookingCare API Server
echo =====================================
echo.
echo API Endpoints:
echo   - HTTPS: https://localhost:7279
echo   - HTTP:  http://localhost:5088
echo.
echo Database: BookingCareDb (localhost:1433)
echo.
echo Press Ctrl+C to stop the server
echo =====================================
echo.

cd /d "%~dp0"
BookingCareManagement.exe --urls "https://localhost:7279;http://localhost:5088"

if errorlevel 1 (
    echo.
    echo ERROR: Failed to start API server!
    echo Check:
    echo   1. Database connection
    echo   2. Port availability
    echo   3. appsettings.json configuration
    echo.
    pause
)
