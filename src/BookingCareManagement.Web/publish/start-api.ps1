# BookingCare API Server Launcher
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "  BookingCare API Server" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "API Endpoints:" -ForegroundColor Yellow
Write-Host "  - HTTPS: https://localhost:7279" -ForegroundColor White
Write-Host "  - HTTP:  http://localhost:5088" -ForegroundColor White
Write-Host ""
Write-Host "Press Ctrl+C to stop the server" -ForegroundColor Gray
Write-Host ""

# Run the API
& ".\BookingCareManagement.exe" --urls "https://localhost:7279;http://localhost:5088"
