# Quick Build Script (No Emojis)
# For Windows PowerShell Compatibility

$ErrorActionPreference = "Stop"

Write-Host "================================" -ForegroundColor Cyan
Write-Host "NonProfit Finance - Build Installer" -ForegroundColor Cyan  
Write-Host "================================`n" -ForegroundColor Cyan

# Configuration
$projectDir = $PSScriptRoot
$publishDir = Join-Path $projectDir "publish"
$installerDir = Join-Path $projectDir "Installer"

Write-Host "Checking WiX Toolset..." -ForegroundColor Yellow

# Auto-detect WiX installation
$wixPath = $null
$wixSearchPaths = @(
    "C:\Program Files (x86)\WiX Toolset v3.14\bin",
    "C:\Program Files (x86)\WiX Toolset v3.11\bin",
    "C:\Program Files\WiX Toolset v3.14\bin",
    "C:\Program Files\WiX Toolset v3.11\bin"
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
    Write-Host "Searched paths:" -ForegroundColor Gray
    $wixSearchPaths | ForEach-Object { Write-Host "  - $_" -ForegroundColor Gray }
    Read-Host "Press Enter to exit"
    exit 1
}

Write-Host "[OK] WiX Toolset found`n" -ForegroundColor Green

# Step 1: Publish
Write-Host "================================" -ForegroundColor Cyan
Write-Host "Step 1: Publishing Application" -ForegroundColor Cyan
Write-Host "================================`n" -ForegroundColor Cyan

Push-Location $projectDir

try {
    Write-Host "Running: dotnet publish..." -ForegroundColor Gray
    
    dotnet publish -c Release -r win-x64 --self-contained false -o $publishDir
    
    if ($LASTEXITCODE -ne 0) {
        throw "Publish failed"
    }
    
    Write-Host "`n[OK] Publish completed`n" -ForegroundColor Green
    
} catch {
    Write-Host "[ERROR] $_" -ForegroundColor Red
    Pop-Location
    Read-Host "Press Enter to exit"
    exit 1
} finally {
    Pop-Location
}

# Step 2: Build MSI
Write-Host "================================" -ForegroundColor Cyan
Write-Host "Step 2: Building MSI" -ForegroundColor Cyan
Write-Host "================================`n" -ForegroundColor Cyan

Push-Location $installerDir

try {
    Write-Host "Cleaning old builds..." -ForegroundColor Gray
    Get-ChildItem -Filter "*.wixobj" -ErrorAction SilentlyContinue | Remove-Item -Force
    Get-ChildItem -Filter "*.wixpdb" -ErrorAction SilentlyContinue | Remove-Item -Force
    Get-ChildItem -Filter "*.msi" -ErrorAction SilentlyContinue | Remove-Item -Force
    
    Write-Host "Compiling WiX..." -ForegroundColor Gray
    
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
    
    Write-Host "`n[OK] MSI created successfully`n" -ForegroundColor Green
    
} catch {
    Write-Host "[ERROR] $_" -ForegroundColor Red
    Pop-Location
    Read-Host "Press Enter to exit"
    exit 1
} finally {
    Pop-Location
}

# Step 3: Build Bootstrapper
Write-Host "================================" -ForegroundColor Cyan
Write-Host "Step 3: Building Bootstrapper" -ForegroundColor Cyan
Write-Host "================================`n" -ForegroundColor Cyan

Push-Location $installerDir

try {
    Get-ChildItem -Filter "Bundle.wixobj" -ErrorAction SilentlyContinue | Remove-Item -Force
    Get-ChildItem -Filter "*.exe" -ErrorAction SilentlyContinue | Remove-Item -Force
    
    Write-Host "Compiling bundle..." -ForegroundColor Gray
    
    & "$wixPath\candle.exe" Bundle.wxs `
        -ext WixBalExtension `
        -ext WixUtilExtension
    
    if ($LASTEXITCODE -ne 0) {
        throw "Bundle compile failed"
    }
    
    Write-Host "Linking bootstrapper..." -ForegroundColor Gray
    
    & "$wixPath\light.exe" Bundle.wixobj `
        -ext WixBalExtension `
        -ext WixUtilExtension `
        -out NonProfitFinanceSetup.exe `
        -sval
    
    if ($LASTEXITCODE -ne 0) {
        throw "Bundle link failed"
    }
    
    Write-Host "`n[OK] Bootstrapper created`n" -ForegroundColor Green
    
} catch {
    Write-Host "[ERROR] $_" -ForegroundColor Red
    Pop-Location
    Read-Host "Press Enter to exit"
    exit 1
} finally {
    Pop-Location
}

# Summary
Write-Host "`n================================" -ForegroundColor Green
Write-Host "BUILD COMPLETED SUCCESSFULLY!" -ForegroundColor Green
Write-Host "================================`n" -ForegroundColor Green

$msiPath = Join-Path $installerDir "NonProfitFinance.msi"
$setupPath = Join-Path $installerDir "NonProfitFinanceSetup.exe"

if (Test-Path $msiPath) {
    $msiSize = (Get-Item $msiPath).Length / 1MB
    Write-Host ("MSI Package: {0:N2} MB" -f $msiSize) -ForegroundColor Yellow
    Write-Host "  Location: $msiPath`n" -ForegroundColor Gray
}

if (Test-Path $setupPath) {
    $setupSize = (Get-Item $setupPath).Length / 1MB
    Write-Host ("Setup Executable: {0:N2} MB" -f $setupSize) -ForegroundColor Yellow
    Write-Host "  Location: $setupPath`n" -ForegroundColor Gray
}

Write-Host "Next Steps:" -ForegroundColor Cyan
Write-Host "  1. Test on clean Windows VM" -ForegroundColor White
Write-Host "  2. Distribute NonProfitFinanceSetup.exe to beta testers" -ForegroundColor White
Write-Host "  3. Include BETA_TESTER_QUICK_START.md" -ForegroundColor White

Write-Host "`nPress Enter to exit..." -ForegroundColor Gray
Read-Host
