@echo off
echo Testing BookingCare API connection...
echo.

:: Test HTTPS endpoint
echo [1/3] Testing HTTPS endpoint: https://localhost:7279/api/Invoice
curl -k https://localhost:7279/api/Invoice 2>nul
if %errorlevel% equ 0 (
    echo [OK] HTTPS endpoint is working!
) else (
    echo [FAIL] HTTPS endpoint is not responding
)
echo.

:: Test HTTP endpoint
echo [2/3] Testing HTTP endpoint: http://localhost:5088/api/Invoice
curl http://localhost:5088/api/Invoice 2>nul
if %errorlevel% equ 0 (
    echo [OK] HTTP endpoint is working!
) else (
    echo [FAIL] HTTP endpoint is not responding
)
echo.

:: Check if process is running
echo [3/3] Checking if API process is running...
tasklist | findstr /I "BookingCareManagement.exe" >nul
if %errorlevel% equ 0 (
    echo [OK] API process is running
) else (
    echo [FAIL] API process is NOT running
    echo Please run START_API_SERVER.bat first
)

echo.
echo =====================================
echo Test completed!
echo =====================================
pause
