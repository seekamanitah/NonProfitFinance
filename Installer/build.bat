@echo off
echo ==========================================
echo NonProfit Finance Manager - Build Installer
echo ==========================================
echo.

REM Configuration
set PROJECT_DIR=%~dp0..
set PUBLISH_DIR=%PROJECT_DIR%\publish
set INSTALLER_DIR=%PROJECT_DIR%\Installer
set OUTPUT_DIR=%PROJECT_DIR%\Installer\Output

REM Auto-detect WiX installation
set WIX_PATH=
if exist "C:\Program Files (x86)\WiX Toolset v3.14\bin\candle.exe" (
    set WIX_PATH=C:\Program Files (x86)\WiX Toolset v3.14\bin
) else if exist "C:\Program Files (x86)\WiX Toolset v3.11\bin\candle.exe" (
    set WIX_PATH=C:\Program Files (x86)\WiX Toolset v3.11\bin
) else if exist "C:\Program Files\WiX Toolset v3.14\bin\candle.exe" (
    set WIX_PATH=C:\Program Files\WiX Toolset v3.14\bin
) else if exist "C:\Program Files\WiX Toolset v3.11\bin\candle.exe" (
    set WIX_PATH=C:\Program Files\WiX Toolset v3.11\bin
)

if "%WIX_PATH%"=="" (
    echo ERROR: WiX Toolset not found!
    echo Please install from: https://wixtoolset.org/
    pause
    exit /b 1
)

echo Found WiX Toolset at: %WIX_PATH%

echo Step 1: Publishing application...
echo ==========================================
cd /d "%PROJECT_DIR%"

REM Publish application
dotnet publish -c Release -r win-x64 --self-contained false -o "%PUBLISH_DIR%"

if %ERRORLEVEL% neq 0 (
    echo ERROR: Publish failed!
    pause
    exit /b 1
)

echo.
echo Step 2: Building MSI...
echo ==========================================
cd /d "%INSTALLER_DIR%"

REM Clean previous builds
if exist "*.wixobj" del /q *.wixobj
if exist "*.wixpdb" del /q *.wixpdb
if exist "*.msi" del /q *.msi
if exist "*.exe" del /q *.exe

REM Build MSI
"%WIX_PATH%\candle.exe" Product.wxs ^
    -dPublishDir="%PUBLISH_DIR%" ^
    -ext WixUIExtension ^
    -ext WixUtilExtension

if %ERRORLEVEL% neq 0 (
    echo ERROR: Candle (compile) failed!
    pause
    exit /b 1
)

"%WIX_PATH%\light.exe" Product.wixobj ^
    -ext WixUIExtension ^
    -ext WixUtilExtension ^
    -out NonProfitFinance.msi ^
    -sval

if %ERRORLEVEL% neq 0 (
    echo ERROR: Light (link) failed!
    pause
    exit /b 1
)

echo.
echo Step 3: Building Bootstrapper...
echo ==========================================

REM Build Bootstrapper
"%WIX_PATH%\candle.exe" Bundle.wxs ^
    -ext WixBalExtension ^
    -ext WixUtilExtension

if %ERRORLEVEL% neq 0 (
    echo ERROR: Bundle candle failed!
    pause
    exit /b 1
)

"%WIX_PATH%\light.exe" Bundle.wixobj ^
    -ext WixBalExtension ^
    -ext WixUtilExtension ^
    -out NonProfitFinanceSetup.exe ^
    -sval

if %ERRORLEVEL% neq 0 (
    echo ERROR: Bundle light failed!
    pause
    exit /b 1
)

echo.
echo ==========================================
echo ✅ BUILD SUCCESSFUL!
echo ==========================================
echo.
echo Output files:
echo - MSI: %INSTALLER_DIR%\NonProfitFinance.msi
echo - Setup: %INSTALLER_DIR%\NonProfitFinanceSetup.exe
echo.
echo File sizes:
dir "NonProfitFinance.msi" | find "NonProfitFinance.msi"
dir "NonProfitFinanceSetup.exe" | find "NonProfitFinanceSetup.exe"
echo.
echo Ready for distribution to beta testers!
echo.
pause
