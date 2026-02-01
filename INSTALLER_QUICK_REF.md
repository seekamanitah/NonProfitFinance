# 🚀 QUICK REFERENCE - Build Installer in 5 Minutes

## Prerequisites (One-Time Setup)

```powershell
# Install WiX Toolset
choco install wixtoolset

# Verify installation
"C:\Program Files (x86)\WiX Toolset v3.11\bin\candle.exe" -?
```

---

## Build Commands

### PowerShell (Recommended)
```powershell
# Basic build (framework-dependent)
.\Build-Installer.ps1

# Self-contained (includes .NET)
.\Build-Installer.ps1 -SelfContained

# Debug build
.\Build-Installer.ps1 -Configuration Debug

# Skip publish step
.\Build-Installer.ps1 -SkipPublish
```

### Batch File
```cmd
cd Installer
build.bat
```

---

## Output Files

After successful build:
```
Installer/
├── NonProfitFinance.msi           ← MSI package
└── NonProfitFinanceSetup.exe      ← Distribute this!
```

---

## Distribution

### Send to Beta Testers:
```
📧 Email: Attach NonProfitFinanceSetup.exe
☁️  Cloud: Upload to Google Drive/OneDrive
💾 USB: Copy to USB drive
```

### Include:
- `NonProfitFinanceSetup.exe`
- `BETA_TESTER_QUICK_START.md` (renamed to README.txt)

---

## Installation (For Testers)

1. Right-click `NonProfitFinanceSetup.exe`
2. Select **"Run as Administrator"**
3. Follow wizard
4. .NET 10 installs automatically if needed
5. Application launches!

---

## Testing

```powershell
# Test on clean Windows 10/11 VM:
1. Copy NonProfitFinanceSetup.exe only
2. Run as Administrator
3. Verify:
   ✅ .NET installs
   ✅ App installs
   ✅ Shortcuts created
   ✅ App launches
   ✅ Features work
```

---

## Troubleshooting

| Problem | Solution |
|---------|----------|
| **WiX not found** | Install from https://wixtoolset.org/ |
| **Build fails** | Check publish/ folder exists |
| **.NET won't install** | Download manually from dot.net |
| **App won't start** | Check: `dotnet --list-runtimes` |

---

## File Sizes

| Type | Size |
|------|------|
| Setup EXE | ~5-10 MB |
| MSI (framework) | ~50-100 MB |
| MSI (self-contained) | ~150-200 MB |

---

## Quick Fixes

```powershell
# Rebuild clean
Remove-Item publish -Recurse -Force
Remove-Item Installer\*.wixobj, Installer\*.msi, Installer\*.exe
.\Build-Installer.ps1

# Test installation
.\Installer\NonProfitFinanceSetup.exe

# Check .NET version
dotnet --list-runtimes
```

---

## Support

- 📖 **Full Guide:** `INSTALLER_BUILD_GUIDE.md`
- 👥 **Beta Guide:** `BETA_TESTER_QUICK_START.md`
- 🌐 **WiX Docs:** https://wixtoolset.org/documentation/

---

**✅ You're ready to distribute your app!**
