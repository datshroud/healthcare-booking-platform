@echo off
color 0E
echo =====================================
echo   Smart Cleanup - Publish Folder
echo =====================================
echo.

echo [1/6] Removing language folders (except vi)...
for /d %%i in (af ar az bg cs da de el es fa fi-FI fr he hr hu id is it ja ko lv ms-MY mt nb nl pl pt ro ru sk sl sr sv th-TH tr uk uz-* zh-*) do (
    if exist "%%i" (
        rd /s /q "%%i" 2>nul
        echo   ? Removed: %%i
    )
)

echo.
echo [2/6] Removing debug files (.pdb ^& .xml)...
del /s /q *.pdb >nul 2>&1
del /s /q *.xml >nul 2>&1
del /s /q BookingCareManagement*.pdb >nul 2>&1
del /s /q BookingCareManagement.xml >nul 2>&1

echo.
echo [3/6] Removing web.config...
if exist "web.config" del /q "web.config"

echo.
echo [4/6] Removing wwwroot folder...
if exist "wwwroot" rd /s /q "wwwroot"

echo.
echo [5/6] Removing code analysis ^& scaffolding DLLs (SAFE)...
del /q Microsoft.CodeAnalysis*.dll >nul 2>&1
del /q Microsoft.VisualStudio.Web.Code*.dll >nul 2>&1
del /q Microsoft.Build*.dll >nul 2>&1
del /q NuGet.*.dll >nul 2>&1
del /q Mono.TextTemplating.dll >nul 2>&1
echo   NOTE: Keeping Swagger DLLs (required by API)

echo.
echo [6/6] Removing unnecessary runtimes...
cd runtimes 2>nul
if not errorlevel 1 (
    rd /s /q linux-arm64 2>nul
    rd /s /q linux-musl-x64 2>nul
    rd /s /q linux-x64 2>nul
    rd /s /q osx-arm64 2>nul
    rd /s /q osx-x64 2>nul
    rd /s /q unix 2>nul
    rd /s /q win-arm 2>nul
    rd /s /q win-arm64 2>nul
    rd /s /q win-x86 2>nul
    echo   ? Kept only: win-x64
    cd ..
)

echo.
echo =====================================
echo   Smart Cleanup Complete!
echo =====================================
echo.

:: Count files
for /f %%a in ('dir /b /s ^| find /c /v ""') do set filecount=%%a

echo Total files remaining: %filecount%
echo.
echo ? Publish folder optimized!
echo ? All required DLLs preserved
echo.
pause
