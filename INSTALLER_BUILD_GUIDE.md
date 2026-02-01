# NonProfit Finance Manager - MSI Installer Build Guide

## Prerequisites

### 1. Install WiX Toolset
```powershell
# Download and install WiX Toolset v3.11+
# From: https://wixtoolset.org/releases/

# Or via Chocolatey
choco install wixtoolset

# Or via dotnet tool
dotnet tool install --global wix
```

### 2. Install WiX Extensions
```powershell
# .NET SDK Extensions
dotnet add package WixSharp
```

## Build Steps

### Step 1: Publish the Application

```powershell
# Navigate to project directory
cd C:\Users\tech\source\repos\NonProfitFinance

# Publish for Windows x64 (self-contained)
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false -o ./publish

# OR Publish framework-dependent (requires .NET runtime)
dotnet publish -c Release -r win-x64 --self-contained false -o ./publish
```

### Step 2: Prepare Installer Files

Create these folders:
```
NonProfitFinance/
├── Installer/
│   ├── Product.wxs          # Main installer definition
│   ├── Bundle.wxs           # Bootstrapper with .NET
│   ├── License.rtf          # License agreement
│   ├── banner.bmp           # 493x58 banner image
│   ├── dialog.bmp           # 493x312 dialog image
│   ├── logo.png             # Logo for bootstrapper
│   ├── icon.ico             # Application icon
│   └── build.bat            # Build script
├── publish/                 # Published app output
```

### Step 3: Create Required Assets

**License.rtf:**
```rtf
{\rtf1\ansi\deff0
{\fonttbl{\f0 Times New Roman;}}
\f0\fs24
END USER LICENSE AGREEMENT\par
\par
1. Grant of License\par
This software is provided for nonprofit organizations...\par
\par
2. Restrictions\par
You may not reverse engineer...\par
}
```

**icon.ico:**
- Create a 256x256 icon file
- Convert from PNG if needed using online tools

### Step 4: Build the MSI

```powershell
# Navigate to Installer folder
cd Installer

# Build the MSI
candle.exe Product.wxs -ext WixUIExtension -ext WixUtilExtension
light.exe Product.wixobj -ext WixUIExtension -ext WixUtilExtension -out NonProfitFinance.msi

# Build the Bootstrapper (with .NET bundle)
candle.exe Bundle.wxs -ext WixBalExtension -ext WixUtilExtension
light.exe Bundle.wixobj -ext WixBalExtension -ext WixUtilExtension -out NonProfitFinanceSetup.exe
```

### Step 5: Using the Build Script

**build.bat:**
```batch
@echo off
echo Building NonProfit Finance Manager Installer...

REM Set paths
set WIX_PATH="C:\Program Files (x86)\WiX Toolset v3.11\bin"
set PUBLISH_DIR="..\publish"

REM Build MSI
echo.
echo Building MSI...
%WIX_PATH%\candle.exe Product.wxs -dPublishDir=%PUBLISH_DIR% -ext WixUIExtension -ext WixUtilExtension
%WIX_PATH%\light.exe Product.wixobj -ext WixUIExtension -ext WixUtilExtension -out NonProfitFinance.msi -sval

REM Build Bundle
echo.
echo Building Bootstrapper...
%WIX_PATH%\candle.exe Bundle.wxs -ext WixBalExtension -ext WixUtilExtension
%WIX_PATH%\light.exe Bundle.wixobj -ext WixBalExtension -ext WixUtilExtension -out NonProfitFinanceSetup.exe -sval

echo.
echo Build complete!
echo Setup file: NonProfitFinanceSetup.exe
pause
```

Run:
```powershell
.\build.bat
```

## Alternative: Simple MSI (No Bootstrapper)

If you want a simpler MSI without .NET bundled:

### build-simple.bat:
```batch
@echo off
set WIX_PATH="C:\Program Files (x86)\WiX Toolset v3.11\bin"
set PUBLISH_DIR="..\publish"

%WIX_PATH%\candle.exe Product.wxs -dPublishDir=%PUBLISH_DIR% -ext WixUIExtension
%WIX_PATH%\light.exe Product.wixobj -ext WixUIExtension -out NonProfitFinance.msi -sval

echo MSI created: NonProfitFinance.msi
pause
```

## Using PowerShell Build Script

**Build-Installer.ps1:**
```powershell
param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [switch]$SelfContained = $false
)

$ErrorActionPreference = "Stop"

Write-Host "Building NonProfit Finance Manager Installer" -ForegroundColor Green

# Step 1: Publish
Write-Host "`nStep 1: Publishing application..." -ForegroundColor Cyan
$publishArgs = @(
    "publish"
    "-c", $Configuration
    "-r", $Runtime
    "--self-contained", $SelfContained
    "-o", "./publish"
)

dotnet @publishArgs

if ($LASTEXITCODE -ne 0) {
    throw "Publish failed"
}

# Step 2: Build MSI
Write-Host "`nStep 2: Building MSI..." -ForegroundColor Cyan
cd Installer

$wixPath = "C:\Program Files (x86)\WiX Toolset v3.11\bin"

& "$wixPath\candle.exe" Product.wxs `
    -dPublishDir="..\publish" `
    -ext WixUIExtension `
    -ext WixUtilExtension

& "$wixPath\light.exe" Product.wixobj `
    -ext WixUIExtension `
    -ext WixUtilExtension `
    -out NonProfitFinance.msi `
    -sval

# Step 3: Build Bundle
Write-Host "`nStep 3: Building Bootstrapper..." -ForegroundColor Cyan

& "$wixPath\candle.exe" Bundle.wxs `
    -ext WixBalExtension `
    -ext WixUtilExtension

& "$wixPath\light.exe" Bundle.wixobj `
    -ext WixBalExtension `
    -ext WixUtilExtension `
    -out NonProfitFinanceSetup.exe `
    -sval

Write-Host "`n✅ Build complete!" -ForegroundColor Green
Write-Host "Setup file: Installer\NonProfitFinanceSetup.exe" -ForegroundColor Yellow

cd ..
```

Run:
```powershell
.\Build-Installer.ps1 -SelfContained
```

## Testing the Installer

### On Test Machine:

1. **Copy files to test machine:**
   ```
   NonProfitFinanceSetup.exe
   ```

2. **Run as Administrator:**
   ```
   Right-click -> Run as Administrator
   ```

3. **Installation will:**
   - Check for .NET 10 Runtime
   - Download/install if missing
   - Install application to Program Files
   - Create Start Menu shortcut
   - Create Desktop shortcut
   - Initialize database

4. **Launch:**
   - From Start Menu
   - From Desktop shortcut
   - Or navigate to: `C:\Program Files\Your Organization\NonProfit Finance Manager\NonProfitFinance.exe`

## Uninstall

```
Settings -> Apps -> NonProfit Finance Manager -> Uninstall
```

Or via command line:
```powershell
msiexec /x {PRODUCT-GUID} /qn
```

## Troubleshooting

### Issue: "WiX not found"
**Solution:** Install WiX Toolset from https://wixtoolset.org/

### Issue: ".NET not installing"
**Solution:** 
- Download .NET 10 Hosting Bundle manually
- Include in installer package
- Update DownloadUrl in Bundle.wxs

### Issue: "Database error on first run"
**Solution:**
- Ensure database file is in publish folder
- Check appsettings.json connection string
- Grant write permissions to installation folder

### Issue: "Application won't start"
**Solution:**
```powershell
# Check .NET installation
dotnet --list-runtimes

# Should show: Microsoft.AspNetCore.App 10.0.x
```

## Distribution to Beta Testers

### Package Contents:
```
BetaTest-v1.0.0/
├── NonProfitFinanceSetup.exe    # Main installer
├── README.txt                   # Installation instructions
├── LICENSE.txt                  # License
└── CHANGELOG.txt                # Version notes
```

### README.txt for Testers:
```
NonProfit Finance Manager - Beta Test v1.0.0
=============================================

INSTALLATION:
1. Run NonProfitFinanceSetup.exe as Administrator
2. Follow the installation wizard
3. The installer will automatically install .NET 10 if needed
4. Launch from Start Menu or Desktop shortcut

SYSTEM REQUIREMENTS:
- Windows 10/11 (64-bit)
- 4GB RAM minimum
- 500MB disk space
- Internet connection (first run only)

BETA TESTING:
- Report bugs to: support@yourorg.com
- Feedback form: https://forms.yourorg.com/beta
- Known issues: See CHANGELOG.txt

UNINSTALL:
Settings -> Apps -> NonProfit Finance Manager -> Uninstall
```

## Signing the Installer (Recommended)

```powershell
# Sign with certificate
signtool sign /f certificate.pfx /p password /t http://timestamp.digicert.com NonProfitFinanceSetup.exe

# Verify signature
signtool verify /pa NonProfitFinanceSetup.exe
```

## File Sizes

Approximate sizes:
- MSI (framework-dependent): ~50-100 MB
- MSI (self-contained): ~150-200 MB
- Bootstrapper EXE: ~5-10 MB
- .NET 10 Runtime download: ~150 MB

## Success!

You now have a professional MSI installer with:
✅ .NET 10 Runtime bundled
✅ Automatic dependency installation
✅ Start Menu and Desktop shortcuts
✅ Proper uninstall support
✅ Professional UI
✅ Ready for beta testing
