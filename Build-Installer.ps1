param(
    [ValidateSet("Release", "Debug")]
    [string]$Configuration = "Release",
    
    [ValidateSet("win-x64", "win-x86", "win-arm64")]
    [string]$Runtime = "win-x64",
    
    [switch]$SelfContained = $false,
    [switch]$SingleFile = $false,
    [switch]$SkipPublish = $false,
    [switch]$SignInstaller = $false,
    [string]$CertificatePath = "",
    [string]$CertificatePassword = ""
)

$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

# Colors
function Write-Success { param($Message) Write-Host $Message -ForegroundColor Green }
function Write-Info { param($Message) Write-Host $Message -ForegroundColor Cyan }
function Write-Warning { param($Message) Write-Host $Message -ForegroundColor Yellow }
function Write-Error { param($Message) Write-Host $Message -ForegroundColor Red }

Write-Host @"
==========================================
NonProfit Finance Manager
MSI Installer Build Script
==========================================
"@ -ForegroundColor Cyan

# Configuration
$projectDir = Split-Path -Parent $PSScriptRoot
$publishDir = Join-Path $projectDir "publish"
$installerDir = Join-Path $projectDir "Installer"
$wixPath = "C:\Program Files (x86)\WiX Toolset v3.14\bin"

Write-Info "`nBuild Configuration:"
Write-Host "  Configuration: $Configuration"
Write-Host "  Runtime: $Runtime"
Write-Host "  Self-Contained: $SelfContained"
Write-Host "  Single File: $SingleFile"
Write-Host "  Project Dir: $projectDir"
Write-Host "  Publish Dir: $publishDir"

# Check WiX installation
if (-not (Test-Path "$wixPath\candle.exe")) {
    Write-Error "`nâŒ WiX Toolset not found!"
    Write-Host "   Download from: https://wixtoolset.org/" -ForegroundColor Yellow
    exit 1
}

Write-Success "`nâœ… WiX Toolset found"

# Step 1: Publish Application
if (-not $SkipPublish) {
    Write-Info "`n=========================================="
    Write-Info "Step 1: Publishing Application"
    Write-Info "=========================================="
    
    Push-Location $projectDir
    
    try {
        $publishArgs = @(
            "publish"
            "-c", $Configuration
            "-r", $Runtime
            "--self-contained:$SelfContained"
            "-o", $publishDir
        )
        
        if ($SingleFile) {
            $publishArgs += @(
                "-p:PublishSingleFile=true",
                "-p:IncludeNativeLibrariesForSelfExtract=true"
            )
        }
        
        Write-Host "  Running: dotnet $($publishArgs -join ' ')" -ForegroundColor Gray
        
        & dotnet @publishArgs
        
        if ($LASTEXITCODE -ne 0) {
            throw "Publish failed with exit code $LASTEXITCODE"
        }
        
        Write-Success "`nâœ… Publish completed successfully"
        
        # Show published files count
        $fileCount = (Get-ChildItem $publishDir -Recurse -File).Count
        Write-Host "   Published $fileCount files" -ForegroundColor Gray
        
    } finally {
        Pop-Location
    }
} else {
    Write-Warning "`nSkipping publish step (using existing publish folder)"
}

# Step 2: Build MSI
Write-Info "`n=========================================="
Write-Info "Step 2: Building MSI Package"
Write-Info "=========================================="

Push-Location $installerDir

try {
    # Clean previous builds
    Write-Host "  Cleaning previous builds..." -ForegroundColor Gray
    Get-ChildItem -Filter "*.wixobj" | Remove-Item -Force -ErrorAction SilentlyContinue
    Get-ChildItem -Filter "*.wixpdb" | Remove-Item -Force -ErrorAction SilentlyContinue
    Get-ChildItem -Filter "*.msi" | Remove-Item -Force -ErrorAction SilentlyContinue
    
    # Compile WiX
    Write-Host "  Compiling WiX source..." -ForegroundColor Gray
    
    $candleArgs = @(
        "Product.wxs"
        "-dPublishDir=$publishDir"
        "-ext", "WixUIExtension"
        "-ext", "WixUtilExtension"
    )
    
    & "$wixPath\candle.exe" @candleArgs
    
    if ($LASTEXITCODE -ne 0) {
        throw "WiX compile (candle) failed with exit code $LASTEXITCODE"
    }
    
    # Link MSI
    Write-Host "  Linking MSI package..." -ForegroundColor Gray
    
    $lightArgs = @(
        "Product.wixobj"
        "-ext", "WixUIExtension"
        "-ext", "WixUtilExtension"
        "-out", "NonProfitFinance.msi"
        "-sval"
    )
    
    & "$wixPath\light.exe" @lightArgs
    
    if ($LASTEXITCODE -ne 0) {
        throw "WiX link (light) failed with exit code $LASTEXITCODE"
    }
    
    Write-Success "`nâœ… MSI package created successfully"
    
    $msiPath = Join-Path $installerDir "NonProfitFinance.msi"
    $msiSize = (Get-Item $msiPath).Length / 1MB
    Write-Host ("   Size: {0:N2} MB" -f $msiSize) -ForegroundColor Gray
    
} finally {
    Pop-Location
}

# Step 3: Build Bootstrapper
Write-Info "`n=========================================="
Write-Info "Step 3: Building Bootstrapper"
Write-Info "=========================================="

Push-Location $installerDir

try {
    # Clean previous bundle builds
    Get-ChildItem -Filter "Bundle.wixobj" | Remove-Item -Force -ErrorAction SilentlyContinue
    Get-ChildItem -Filter "*.exe" | Remove-Item -Force -ErrorAction SilentlyContinue
    
    # Compile bundle
    Write-Host "  Compiling bundle..." -ForegroundColor Gray
    
    $bundleCandleArgs = @(
        "Bundle.wxs"
        "-ext", "WixBalExtension"
        "-ext", "WixUtilExtension"
    )
    
    & "$wixPath\candle.exe" @bundleCandleArgs
    
    if ($LASTEXITCODE -ne 0) {
        throw "Bundle compile failed with exit code $LASTEXITCODE"
    }
    
    # Link bundle
    Write-Host "  Linking bootstrapper..." -ForegroundColor Gray
    
    $bundleLightArgs = @(
        "Bundle.wixobj"
        "-ext", "WixBalExtension"
        "-ext", "WixUtilExtension"
        "-out", "NonProfitFinanceSetup.exe"
        "-sval"
    )
    
    & "$wixPath\light.exe" @bundleLightArgs
    
    if ($LASTEXITCODE -ne 0) {
        throw "Bundle link failed with exit code $LASTEXITCODE"
    }
    
    Write-Success "`nâœ… Bootstrapper created successfully"
    
    $setupPath = Join-Path $installerDir "NonProfitFinanceSetup.exe"
    $setupSize = (Get-Item $setupPath).Length / 1MB
    Write-Host ("   Size: {0:N2} MB" -f $setupSize) -ForegroundColor Gray
    
} finally {
    Pop-Location
}

# Step 4: Sign Installer (Optional)
if ($SignInstaller -and $CertificatePath) {
    Write-Info "`n=========================================="
    Write-Info "Step 4: Signing Installer"
    Write-Info "=========================================="
    
    $setupPath = Join-Path $installerDir "NonProfitFinanceSetup.exe"
    
    try {
        $signToolArgs = @(
            "sign"
            "/f", $CertificatePath
            "/t", "http://timestamp.digicert.com"
        )
        
        if ($CertificatePassword) {
            $signToolArgs += @("/p", $CertificatePassword)
        }
        
        $signToolArgs += $setupPath
        
        & signtool @signToolArgs
        
        if ($LASTEXITCODE -ne 0) {
            throw "Signing failed with exit code $LASTEXITCODE"
        }
        
        Write-Success "`nâœ… Installer signed successfully"
        
    } catch {
        Write-Warning "`nâš ï¸  Signing failed: $_"
    }
}

# Summary
Write-Host @"

==========================================
âœ… BUILD COMPLETED SUCCESSFULLY!
==========================================

Output Files:
"@ -ForegroundColor Green

$msiPath = Join-Path $installerDir "NonProfitFinance.msi"
$setupPath = Join-Path $installerDir "NonProfitFinanceSetup.exe"

if (Test-Path $msiPath) {
    $msiSize = (Get-Item $msiPath).Length / 1MB
    Write-Host ("  [MSI] Package: {0:N2} MB" -f $msiSize) -ForegroundColor Yellow
    Write-Host "     $msiPath" -ForegroundColor Gray
}

if (Test-Path $setupPath) {
    $setupSize = (Get-Item $setupPath).Length / 1MB
    Write-Host ("`n  [SETUP] Executable: {0:N2} MB" -f $setupSize) -ForegroundColor Yellow
    Write-Host "     $setupPath" -ForegroundColor Gray
}

Write-Host @"

Distribution:
  [EMAIL] Send NonProfitFinanceSetup.exe to beta testers
  [CLOUD] Upload to OneDrive/Dropbox/Google Drive
  [WEB]   Host on your website for download

Next Steps:
  1. Test installation on clean Windows 10/11 machine
  2. Verify .NET 10 auto-installation
  3. Check application functionality
  4. Distribute to beta testers

Documentation: INSTALLER_BUILD_GUIDE.md
"@ -ForegroundColor Cyan

Write-Host "`nPress any key to exit..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

