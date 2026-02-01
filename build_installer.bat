@echo off
REM NonProfit Finance Installer Build Script
REM This script creates a Windows installer using Inno Setup

echo.
echo ========================================
echo NonProfit Finance Installer Builder
echo ========================================
echo.

REM Check if Inno Setup is installed
if not exist "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" (
    echo.
    echo ERROR: Inno Setup 6 is not installed!
    echo.
    echo Please download and install Inno Setup from:
    echo https://jrsoftware.org/isdl.php
    echo.
    echo After installation, run this script again.
    pause
    exit /b 1
)

echo Step 1: Checking if self-contained package exists...
if not exist "C:\Users\tech\Desktop\NonProfitFinance_Standalone" (
    echo.
    echo ERROR: Self-contained package not found!
    echo Expected location: C:\Users\tech\Desktop\NonProfitFinance_Standalone
    echo.
    echo Please ensure the self-contained deployment exists.
    pause
    exit /b 1
)
echo [OK] Self-contained package found

echo.
echo Step 2: Creating output directory...
if not exist "bin\Setup" mkdir bin\Setup
echo [OK] Output directory ready

echo.
echo Step 3: Compiling installer with Inno Setup...
"C:\Program Files (x86)\Inno Setup 6\ISCC.exe" setup.iss

if %ERRORLEVEL% == 0 (
    echo.
    echo [SUCCESS] Installer created successfully!
    echo.
    echo Output file: bin\Setup\NonProfitFinance_Setup_v1.0.0.exe
    echo.
    echo You can now distribute this installer to end users.
    echo.
) else (
    echo.
    echo [ERROR] Installer creation failed!
    echo Please check the error messages above.
    echo.
)

pause
