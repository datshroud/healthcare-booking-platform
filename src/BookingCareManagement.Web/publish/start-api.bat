@echo off
echo Starting BookingCare API Server...
echo API will be available at: https://localhost:7279
echo Press Ctrl+C to stop the server
echo.
BookingCareManagement.exe --urls "https://localhost:7279;http://localhost:5088"
pause
