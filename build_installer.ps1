# NonProfit Finance Installer Configuration
# PowerShell script to create Windows installer with auto-update support

param(
    [string]$AppVersion = "1.0.0",
    [string]$OutputDir = "bin\Setup",
    [string]$StandaloneDir = "C:\Users\tech\Desktop\NonProfitFinance_Standalone"
)

# Configuration
$AppName = "NonProfit Finance"
$AppPublisher = "Fire Department"
$InstallerName = "NonProfitFinance_Setup_v$AppVersion.exe"

Write-Host "========================================" -ForegroundColor Green
Write-Host "NonProfit Finance Installer Generator" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""

# Step 1: Verify Inno Setup installation
Write-Host "Step 1: Checking Inno Setup..." -ForegroundColor Yellow
$InnoSetupPath = "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"

if (-not (Test-Path $InnoSetupPath)) {
    Write-Host "[ERROR] Inno Setup 6 not found!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please download and install Inno Setup from:" -ForegroundColor Red
    Write-Host "https://jrsoftware.org/isdl.php" -ForegroundColor Cyan
    Write-Host ""
    exit 1
}
Write-Host "[OK] Inno Setup found" -ForegroundColor Green

# Step 2: Verify self-contained deployment
Write-Host ""
Write-Host "Step 2: Verifying self-contained deployment..." -ForegroundColor Yellow

if (-not (Test-Path $StandaloneDir)) {
    Write-Host "[ERROR] Self-contained package not found at: $StandaloneDir" -ForegroundColor Red
    exit 1
}
Write-Host "[OK] Self-contained package found" -ForegroundColor Green

# Step 3: Create output directory
Write-Host ""
Write-Host "Step 3: Creating output directory..." -ForegroundColor Yellow

if (-not (Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
}
Write-Host "[OK] Output directory: $OutputDir" -ForegroundColor Green

# Step 4: Generate installer script
Write-Host ""
Write-Host "Step 4: Generating Inno Setup script..." -ForegroundColor Yellow

$setupScript = @"
[Setup]
AppName=$AppName
AppVersion=$AppVersion
AppPublisher=$AppPublisher
DefaultDirName={autopf}\NonProfitFinance
DefaultGroupName=$AppName
AllowNoIcons=yes
OutputBaseFilename=$($InstallerName -replace '\.exe$', '')
OutputDir=$OutputDir
Compression=lzma
SolidCompression=yes
PrivilegesRequired=lowest
ArchitecturesInstallIn64BitMode=x64
ArchitecturesAllowed=x64
WizardStyle=modern
ShowLanguageDialog=no
SetupLogging=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create desktop shortcut"; Flags: unchecked

[Files]
Source: "$StandaloneDir\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\$AppName"; Filename: "{app}\NonProfitFinance.exe"
Name: "{autodesktop}\$AppName"; Filename: "{app}\NonProfitFinance.exe"; Tasks: desktopicon
Name: "{group}\Uninstall $AppName"; Filename: "{uninstallexe}"

[Run]
Filename: "{app}\NonProfitFinance.exe"; Description: "Launch $AppName"; Flags: nowait postinstall skipifsilent

[Code]
procedure InitializeWizard;
begin
  WizardForm.WelcomeLabel2.Caption := 
    'This will install $AppName v$AppVersion on your computer.' + #13#13 +
    'System Requirements:' + #13 +
    '- Windows 11 (64-bit)' + #13 +
    '- 500 MB free disk space' + #13#13 +
    'Click Next to continue.';
end;
"@

$setupScript | Out-File -FilePath "setup.iss" -Encoding UTF8
Write-Host "[OK] Setup script generated" -ForegroundColor Green

# Step 5: Compile installer
Write-Host ""
Write-Host "Step 5: Compiling installer (this may take a minute)..." -ForegroundColor Yellow

& $InnoSetupPath "setup.iss"

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "[SUCCESS] Installer created!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Output file: $OutputDir\$InstallerName" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "You can now:" -ForegroundColor Yellow
    Write-Host "1. Test the installer by running it" -ForegroundColor White
    Write-Host "2. Distribute to end users" -ForegroundColor White
    Write-Host "3. Host for auto-updates" -ForegroundColor White
    Write-Host ""
} else {
    Write-Host ""
    Write-Host "[ERROR] Installer compilation failed!" -ForegroundColor Red
    Write-Host ""
    exit 1
}
