# Complete Build Script with File Harvesting
# Builds MSI installer for NonProfit Finance Manager

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "NonProfit Finance - Complete Build" -ForegroundColor Cyan  
Write-Host "========================================`n" -ForegroundColor Cyan

# Configuration
$projectDir = $PSScriptRoot
$publishDir = Join-Path $projectDir "publish"
$installerDir = Join-Path $projectDir "Installer"

# Auto-detect WiX installation
Write-Host "[1/4] Detecting WiX Toolset..." -ForegroundColor Yellow
$wixPath = $null
$wixSearchPaths = @(
    "C:\Program Files (x86)\WiX Toolset v3.14\bin",
    "C:\Program Files (x86)\WiX Toolset v3.11\bin",
    "C:\Program Files\WiX Toolset v3.11\bin",
    "C:\Program Files\WiX Toolset v3.14\bin"
)

foreach ($path in $wixSearchPaths) {
    if (Test-Path "$path\candle.exe") {
        $wixPath = $path
        break
    }
}

if (-not $wixPath) {
    Write-Host "[ERROR] WiX Toolset not found!" -ForegroundColor Red
    Write-Host "Install from: https://wixtoolset.org/" -ForegroundColor Yellow
    Write-Host "`nSearched paths:" -ForegroundColor Gray
    $wixSearchPaths | ForEach-Object { Write-Host "  - $_" -ForegroundColor Gray }
    Read-Host "Press Enter to exit"
    exit 1
}

Write-Host "[OK] WiX found at: $wixPath`n" -ForegroundColor Green

# Step 1: Publish
Write-Host "[2/4] Publishing Application..." -ForegroundColor Yellow
Push-Location $projectDir

try {
    dotnet publish -c Release -r win-x64 --self-contained false -o $publishDir
    
    if ($LASTEXITCODE -ne 0) {
        throw "Publish failed"
    }
    
    Write-Host "[OK] Publish completed`n" -ForegroundColor Green
    
} catch {
    Write-Host "[ERROR] $_" -ForegroundColor Red
    Pop-Location
    Read-Host "Press Enter to exit"
    exit 1
} finally {
    Pop-Location
}

# Step 2: Harvest Files (Optional - generates comprehensive file list)
Write-Host "[3/4] Harvesting Published Files..." -ForegroundColor Yellow
Push-Location $installerDir

try {
    Write-Host "Running heat.exe to generate file list..." -ForegroundColor Gray
    
    & "$wixPath\heat.exe" dir "$publishDir" `
        -cg PublishedFilesGroup `
        -gg `
        -sfrag `
        -srd `
        -scom `
        -sreg `
        -dr INSTALLFOLDER `
        -var var.PublishDir `
        -out "PublishedFiles.wxs" `
        2>&1 | Out-Null
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "[OK] Files harvested (PublishedFiles.wxs created)`n" -ForegroundColor Green
    } else {
        Write-Host "[WARNING] Heat.exe had issues, continuing with manual file list`n" -ForegroundColor Yellow
    }
    
} catch {
    Write-Host "[WARNING] File harvesting skipped: $_`n" -ForegroundColor Yellow
} finally {
    Pop-Location
}

# Step 3: Build MSI
Write-Host "[4/4] Building MSI..." -ForegroundColor Yellow
Push-Location $installerDir

try {
    Write-Host "Cleaning old builds..." -ForegroundColor Gray
    Get-ChildItem -Filter "*.wixobj" -ErrorAction SilentlyContinue | Remove-Item -Force
    Get-ChildItem -Filter "*.wixpdb" -ErrorAction SilentlyContinue | Remove-Item -Force
    Get-ChildItem -Filter "*.msi" -ErrorAction SilentlyContinue | Remove-Item -Force
    
    Write-Host "Compiling WiX (Product.wxs)..." -ForegroundColor Gray
    
    & "$wixPath\candle.exe" Product.wxs `
        -dPublishDir="$publishDir" `
        -ext WixUIExtension `
        -ext WixUtilExtension
    
    if ($LASTEXITCODE -ne 0) {
        throw "WiX compile failed"
    }
    
    Write-Host "Linking MSI..." -ForegroundColor Gray
    
    & "$wixPath\light.exe" Product.wixobj `
        -ext WixUIExtension `
        -ext WixUtilExtension `
        -out NonProfitFinance.msi `
        -sval
    
    if ($LASTEXITCODE -ne 0) {
        throw "WiX link failed"
    }
    
    Write-Host "`n========================================" -ForegroundColor Green
    Write-Host "[SUCCESS] MSI created successfully!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "`nInstaller: $installerDir\NonProfitFinance.msi`n" -ForegroundColor Cyan
    
} catch {
    Write-Host "`n========================================" -ForegroundColor Red
    Write-Host "[ERROR] Build failed: $_" -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
    Pop-Location
    Read-Host "Press Enter to exit"
    exit 1
} finally {
    Pop-Location
}

Write-Host "Press Enter to exit..." -ForegroundColor Gray
Read-Host
