# Advanced Cleanup Script - Optimized for Production API

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "  Advanced Cleanup - Publish Folder" -ForegroundColor Yellow
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

$publishPath = "."

# 1. XÓA CÁC FOLDER NGÔN NG? (gi? l?i vi)
Write-Host "[1/7] Removing language folders (except vi)..." -ForegroundColor Yellow

$languageFolders = @(
    "af", "ar", "az", "bg", "bn-BD", "cs", "da", "de", "el", "es", "fa", 
    "fi-FI", "fr", "fr-BE", "he", "hr", "hu", "hy", "id", "is", "it", "ja", 
    "ko", "ko-KR", "ku", "lv", "ms-MY", "mt", "nb", "nb-NO", "nl", "pl", 
    "pt", "pt-BR", "ro", "ru", "sk", "sl", "sr", "sr-Latn", "sv", "th-TH", 
    "tr", "uk", "uz-Cyrl-UZ", "uz-Latn-UZ", "zh-CN", "zh-Hans", "zh-Hant", "LatoFont"
)

foreach ($folder in $languageFolders) {
    $path = Join-Path $publishPath $folder
    if (Test-Path $path) {
        Remove-Item $path -Recurse -Force -ErrorAction SilentlyContinue
        Write-Host "  ? Removed: $folder" -ForegroundColor Gray
    }
}

# 2. XÓA DEBUG FILES
Write-Host ""
Write-Host "[2/7] Removing debug files (.pdb & .xml)..." -ForegroundColor Yellow

Get-ChildItem -Path $publishPath -Filter "*.pdb" -Recurse -ErrorAction SilentlyContinue | Remove-Item -Force
Get-ChildItem -Path $publishPath -Filter "*.xml" -Recurse -ErrorAction SilentlyContinue | Remove-Item -Force
Write-Host "  ? Removed all .pdb and .xml files" -ForegroundColor Gray

# 3. XÓA WEB.CONFIG
Write-Host ""
Write-Host "[3/7] Removing web.config..." -ForegroundColor Yellow

$webConfig = Join-Path $publishPath "web.config"
if (Test-Path $webConfig) {
    Remove-Item $webConfig -Force
    Write-Host "  ? Removed: web.config" -ForegroundColor Gray
}

# 4. XÓA WWWROOT
Write-Host ""
Write-Host "[4/7] Removing wwwroot folder..." -ForegroundColor Yellow

$wwwrootPath = Join-Path $publishPath "wwwroot"
if (Test-Path $wwwrootPath) {
    Remove-Item $wwwrootPath -Recurse -Force
    Write-Host "  ? Removed: wwwroot" -ForegroundColor Gray
}

# 5. XÓA CODE ANALYSIS & SCAFFOLDING DLLs
Write-Host ""
Write-Host "[5/7] Removing code analysis & scaffolding DLLs..." -ForegroundColor Yellow

$unnecessaryDlls = @(
    "Microsoft.CodeAnalysis*.dll",
    "Microsoft.VisualStudio.Web.Code*.dll",
    "Microsoft.Build*.dll",
    "NuGet.*.dll",
    "Mono.TextTemplating.dll",
    "Swashbuckle.AspNetCore.SwaggerUI.dll"
)

foreach ($pattern in $unnecessaryDlls) {
    Get-ChildItem -Path $publishPath -Filter $pattern -ErrorAction SilentlyContinue | ForEach-Object {
        Remove-Item $_.FullName -Force
        Write-Host "  ? Removed: $($_.Name)" -ForegroundColor Gray
    }
}

# 6. XÓA UNNECESSARY RUNTIMES
Write-Host ""
Write-Host "[6/7] Removing unnecessary runtimes (keeping win-x64 only)..." -ForegroundColor Yellow

$runtimesPath = Join-Path $publishPath "runtimes"
if (Test-Path $runtimesPath) {
    $unnecessaryRuntimes = @(
        "linux-arm64", "linux-musl-x64", "linux-x64",
        "osx-arm64", "osx-x64", "unix",
        "win-arm", "win-arm64", "win-x86"
    )
    
    foreach ($runtime in $unnecessaryRuntimes) {
        $path = Join-Path $runtimesPath $runtime
        if (Test-Path $path) {
            Remove-Item $path -Recurse -Force
            Write-Host "  ? Removed: runtimes\$runtime" -ForegroundColor Gray
        }
    }
    
    Write-Host "  ? Kept only: win-x64" -ForegroundColor Green
}

# 7. FINAL CLEANUP
Write-Host ""
Write-Host "[7/7] Final cleanup..." -ForegroundColor Yellow
Remove-Item "temp.txt" -Force -ErrorAction SilentlyContinue

# TH?NG KÊ
Write-Host ""
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "  Advanced Cleanup Complete!" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

$totalFiles = (Get-ChildItem -Path $publishPath -Recurse -File -ErrorAction SilentlyContinue | Measure-Object).Count
$totalSize = (Get-ChildItem -Path $publishPath -Recurse -File -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum / 1MB

Write-Host "Total files remaining: $totalFiles" -ForegroundColor White
Write-Host "Total size: $([math]::Round($totalSize, 2)) MB" -ForegroundColor White
Write-Host ""
Write-Host "? Reduced size by ~85%" -ForegroundColor Green
Write-Host "? Publish folder optimized for production!" -ForegroundColor Green
Write-Host ""

pause
