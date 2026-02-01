[Setup]
AppName=NonProfit Finance
AppVersion=1.0.0
AppPublisher=Fire Department
DefaultDirName={autopf}\NonProfitFinance
DefaultGroupName=NonProfit Finance
AllowNoIcons=yes
OutputBaseFilename=NonProfitFinance_Setup_v1.0.0
OutputDir=bin\Setup
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
Source: "C:\Users\tech\Desktop\NonProfitFinance_Standalone\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\NonProfit Finance"; Filename: "{app}\NonProfitFinance.exe"
Name: "{autodesktop}\NonProfit Finance"; Filename: "{app}\NonProfitFinance.exe"; Tasks: desktopicon
Name: "{group}\Uninstall NonProfit Finance"; Filename: "{uninstallexe}"

[Run]
Filename: "{app}\NonProfitFinance.exe"; Description: "Launch NonProfit Finance"; Flags: nowait postinstall skipifsilent

[Code]
procedure InitializeWizard;
begin
  WizardForm.WelcomeLabel2.Caption := 
    'This will install NonProfit Finance v1.0.0 on your computer.' + #13#13 +
    'System Requirements:' + #13 +
    '- Windows 11 (64-bit)' + #13 +
    '- 500 MB free disk space' + #13#13 +
    'Click Next to continue.';
end;
