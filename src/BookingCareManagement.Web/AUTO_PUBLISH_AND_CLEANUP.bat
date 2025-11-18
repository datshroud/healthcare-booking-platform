@echo off
color 0B
title Auto Publish and Cleanup

echo =====================================
echo   Auto Publish ^& Cleanup Script
echo =====================================
echo.

echo [Step 1/3] Publishing API...
cd ..
dotnet publish -c Release -o .\publish

if errorlevel 1 (
    echo.
    echo [ERROR] Publish failed!
    pause
    exit /b 1
)

echo.
echo [Step 2/3] Running cleanup...
cd publish
call CLEANUP_PUBLISH.bat

echo.
echo [Step 3/3] Verifying files...
dir /b | find /c /v "" > temp.txt
set /p filecount=<temp.txt
del temp.txt

echo.
echo =====================================
echo   Publish Complete!
echo =====================================
echo.
echo Total files in publish folder: %filecount%
echo Location: %cd%
echo.
echo To run API:
echo   1. Double-click: START_API_SERVER.bat
echo   2. Or run: BookingCareManagement.exe
echo.
pause
