# Quick WiX Test Script
# Tests that WiX toolset is properly detected

$ErrorActionPreference = "Stop"

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "WiX Toolset Detection Test" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# Auto-detect WiX installation
$wixPath = $null
$wixSearchPaths = @(
    "C:\Program Files (x86)\WiX Toolset v3.14\bin",
    "C:\Program Files (x86)\WiX Toolset v3.11\bin",
    "C:\Program Files\WiX Toolset v3.14\bin",
    "C:\Program Files\WiX Toolset v3.11\bin"
)

Write-Host "Searching for WiX Toolset..." -ForegroundColor Yellow

foreach ($path in $wixSearchPaths) {
    Write-Host "  Checking: $path" -ForegroundColor Gray
    if (Test-Path "$path\candle.exe") {
        $wixPath = $path
        Write-Host "  [FOUND]`n" -ForegroundColor Green
        break
    } else {
        Write-Host "  [NOT FOUND]" -ForegroundColor DarkGray
    }
}

if (-not $wixPath) {
    Write-Host "`n[FAILED] WiX Toolset not found!" -ForegroundColor Red
    Write-Host "Install from: https://wixtoolset.org/`n" -ForegroundColor Yellow
    exit 1
}

# Get version info
Write-Host "========================================" -ForegroundColor Green
Write-Host "[SUCCESS] WiX Toolset Found!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host "Location: $wixPath" -ForegroundColor Cyan

# Check for required tools
$tools = @("candle.exe", "light.exe", "heat.exe")
Write-Host "`nVerifying required tools:" -ForegroundColor Yellow

foreach ($tool in $tools) {
    $toolPath = Join-Path $wixPath $tool
    if (Test-Path $toolPath) {
        Write-Host "  ✓ $tool" -ForegroundColor Green
    } else {
        Write-Host "  ✗ $tool [MISSING]" -ForegroundColor Red
    }
}

# Get candle version
Write-Host "`nVersion Information:" -ForegroundColor Yellow
try {
    $version = & "$wixPath\candle.exe" -? 2>&1 | Select-Object -First 3
    $version | ForEach-Object { Write-Host "  $_" -ForegroundColor Gray }
} catch {
    Write-Host "  Could not determine version" -ForegroundColor DarkGray
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "✓ All checks passed!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "`nYou can now run:" -ForegroundColor Yellow
Write-Host "  .\Build-Installer-Simple.ps1" -ForegroundColor White
Write-Host "  .\Build-Installer-Complete.ps1" -ForegroundColor White
Write-Host "`n"
