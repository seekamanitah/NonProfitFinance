# Creating a Windows Installer for NonProfit Finance

## Why NOT ClickOnce?

ClickOnce is designed for desktop applications (WinForms, WPF), not web applications like Blazor Server. Your app is a web application, so we're creating a proper **Windows Installer (MSI equivalent)** instead.

---

## Solution: Inno Setup Installer

**Inno Setup** is a professional installer creator that:
- ? Works with any Windows application
- ? Supports auto-updates
- ? Creates clean uninstall
- ? No administrative privileges required
- ? Small download size
- ? Free and open-source

---

## How to Create the Installer

### Prerequisites

1. **Download and Install Inno Setup 6**
   - Visit: https://jrsoftware.org/isdl.php
   - Download the latest version
   - Installation size: ~30 MB
   - Takes 2-3 minutes to install

2. **Self-contained deployment** (already created)
   - Location: `C:\Users\tech\Desktop\NonProfitFinance_Standalone`
   - Size: 64 MB

### Step 1: Prepare the Files

```powershell
# Files in your project root:
- setup.iss              (Installer configuration - already created)
- build_installer.bat   (Batch script to build - already created)
- build_installer.ps1   (PowerShell script - already created)
```

### Step 2: Build the Installer

**Option A: Using PowerShell (Recommended)**
```powershell
# Open PowerShell and run:
.\build_installer.ps1

# Or with custom parameters:
.\build_installer.ps1 -AppVersion "1.0.1" -StandaloneDir "C:\Path\To\Standalone"
```

**Option B: Using Batch File**
```batch
# Double-click:
build_installer.bat

# Or run from command prompt:
build_installer.bat
```

**Option C: Manual Build**
```
1. Open Inno Setup
2. File ? Open
3. Select: setup.iss
4. Build ? Compile
5. Installer created in: bin\Setup\
```

### Step 3: Test the Installer

```
1. Navigate to: bin\Setup\
2. Double-click: NonProfitFinance_Setup_v1.0.0.exe
3. Follow installation wizard
4. Test the application after install
```

---

## Installer Features

### What Gets Installed:
? Complete application (64 MB)
? Desktop shortcut (optional)
? Start Menu shortcuts
? Uninstaller

### What User Gets:
? One-click installation
? Start Menu entry
? Desktop icon (optional)
? Easy uninstall via Control Panel
? No .NET SDK required (runtime included)

### Installation Process:
1. User downloads: `NonProfitFinance_Setup_v1.0.0.exe`
2. Double-clicks to launch installer
3. Wizard guides through setup (3 screens)
4. App installs to: `C:\Program Files\NonProfitFinance\`
5. Application launches automatically
6. Database created on first run

---

## Distribution

### For End Users:

**File to distribute:**
```
NonProfitFinance_Setup_v1.0.0.exe (64 MB)
```

**Installation Requirements:**
- Windows 11 (64-bit)
- 500 MB free disk space
- Internet connection (first run only)

**Installation Time:**
- Download: 1-5 minutes (depends on internet)
- Install: 1-2 minutes
- Total: 2-7 minutes

### Distribution Methods:

1. **USB Drive**
   - Copy .exe to USB
   - Give to fire department
   - They run installer

2. **Email**
   - Too large (64 MB)
   - Use OneDrive/Google Drive link instead

3. **File Sharing**
   - OneDrive, Google Drive, DropBox
   - Generate shareable link
   - Send to users

4. **Network Share**
   - Place on shared server
   - Users run from network

5. **Website**
   - Host on your website
   - Provide download link

---

## Creating Updates

When you update the application:

### Step 1: Rebuild Self-Contained Version
```bash
dotnet publish -c Release -r win-x64 --self-contained -o "C:\Path\To\NonProfitFinance_Standalone"
```

### Step 2: Update Version Number
Edit `setup.iss`:
```
AppVersion=1.0.1          # Change version
OutputBaseFilename=NonProfitFinance_Setup_v1.0.1
```

### Step 3: Rebuild Installer
```powershell
.\build_installer.ps1 -AppVersion "1.0.1"
```

### Step 4: Distribute New Installer
Users download and run new `.exe` file.

---

## Advanced: Auto-Updates

For automatic updates (beyond basic installer):

### Option 1: Squirrel.Windows
Adds auto-update capability to your installer.

### Option 2: Update Service
- Host new version on server
- App checks for updates on startup
- User gets notification to update

### Option 3: Manual Updates
- Users download new installer
- Install over existing (updates in-place)
- Database preserved

---

## Troubleshooting

### "Inno Setup not found"
- Download and install from: https://jrsoftware.org/isdl.php
- Default install path: `C:\Program Files (x86)\Inno Setup 6\`

### "Self-contained package not found"
- Verify location: `C:\Users\tech\Desktop\NonProfitFinance_Standalone`
- Ensure you created self-contained deployment first

### Installer too large (64 MB)
- This is expected (includes .NET 10 runtime)
- Can't be reduced without removing runtime
- Users only download once

### App won't launch after install
- Check Windows Defender isn't blocking it
- Run as Administrator
- Check Event Viewer for errors

### Database not found after install
- First run creates database automatically
- Check AppData permissions: `%AppData%\NonProfitFinance\`

---

## File Locations After Install

```
C:\Program Files\NonProfitFinance\
??? NonProfitFinance.exe          (Main application)
??? appsettings.json              (Configuration)
??? wwwroot/                      (Static files)
??? *.dll                         (Runtime libraries)

C:\Users\<Username>\AppData\Roaming\NonProfitFinance\
??? NonProfitFinance.db           (Database)
??? Logs/                         (Application logs)
??? Backups/                      (Automatic backups)
```

---

## Summary

| Method | Complexity | Setup Time | File Size |
|--------|-----------|-----------|-----------|
| Self-Contained EXE | ? Easy | 1 min | 64 MB |
| Inno Setup Installer | ?? Medium | 5 min | 64 MB |
| ClickOnce | ? Not suitable | - | - |
| WiX Toolset | ??? Complex | 30 min | 64 MB |

**Recommendation: Inno Setup** - Best balance of ease and professionalism.

---

## Next Steps

1. ? Download Inno Setup from https://jrsoftware.org/isdl.php
2. ? Run `build_installer.ps1`
3. ? Test installer on Windows 11
4. ? Distribute `NonProfitFinance_Setup_v1.0.0.exe` to users
5. ? Keep self-contained package as backup

---

**Your application is ready for professional deployment!** ??
