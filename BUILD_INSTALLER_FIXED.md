# Build Installer - Fixed and Ready

## Issue Fixed
**Problem:** Emoji characters in PowerShell script caused parsing errors  
**Solution:** Replaced all emojis with ASCII text like `[OK]`, `[ERROR]`, `[MSI]`, etc.

---

## Quick Start

### Option 1: Simple Build Script (Recommended for First Time)
```powershell
.\Build-Installer-Simple.ps1
```

This script:
- ✅ No emojis (pure ASCII)
- ✅ Step-by-step output
- ✅ Clear error messages
- ✅ Pauses at end so you can see results

### Option 2: Full Build Script
```powershell
.\Build-Installer.ps1
```

More features but same result.

---

## Prerequisites

**Before running, ensure:**

1. **WiX Toolset installed:**
   ```powershell
   choco install wixtoolset
   ```
   Or download from: https://wixtoolset.org/

2. **Project builds successfully:**
   ```powershell
   dotnet build
   ```

---

## What Happens When You Run It

### Step 1: Publish (1-2 minutes)
```
Publishing application...
  ✓ Compiling code
  ✓ Copying files to publish/
  ✓ ~50-100 MB output
```

### Step 2: Build MSI (30 seconds)
```
Building MSI package...
  ✓ Compiling Product.wxs
  ✓ Linking installer
  ✓ Output: NonProfitFinance.msi
```

### Step 3: Build Bootstrapper (30 seconds)
```
Building bootstrapper...
  ✓ Compiling Bundle.wxs
  ✓ Linking setup executable
  ✓ Output: NonProfitFinanceSetup.exe
```

---

## Output Files

After successful build, you'll find:

```
Installer/
├── NonProfitFinance.msi           (50-100 MB)
└── NonProfitFinanceSetup.exe      (5-10 MB) ← Distribute this!
```

---

## Distribute to Beta Testers

**Send them only:**
- `NonProfitFinanceSetup.exe`
- `BETA_TESTER_QUICK_START.md` (renamed to README.txt)

**They need:**
- Windows 10/11 (64-bit)
- Run as Administrator
- That's it! (NET 10 installs automatically)

---

## Testing Before Distribution

1. **Copy to clean Windows VM**
2. **Run as Administrator**
3. **Verify:**
   - .NET 10 downloads/installs
   - Application installs
   - Shortcuts created
   - App launches
   - Database initializes

---

## Troubleshooting

### Error: "WiX not found"
```powershell
# Install WiX
choco install wixtoolset

# Verify
Test-Path "C:\Program Files (x86)\WiX Toolset v3.11\bin\candle.exe"
```

### Error: "Publish failed"
```powershell
# Clean and rebuild
dotnet clean
dotnet build
```

### Error: "Candle failed"
```
Check:
- publish/ folder exists
- Product.wxs has no syntax errors
- All paths correct
```

### Error: "Light failed"
```
Common causes:
- Missing files in publish/
- Duplicate component IDs
- Invalid WiX syntax

Solution: Check error message for specific file/line
```

---

## Build Options

### Framework-Dependent (Default)
```powershell
.\Build-Installer-Simple.ps1
```
- Size: ~50-100 MB
- Requires: .NET 10 (installed automatically)

### Self-Contained
```powershell
# Edit line in script:
--self-contained false  →  --self-contained true
```
- Size: ~150-200 MB
- Requires: Nothing (all included)

---

## File Sizes Reference

| Component | Size | Notes |
|-----------|------|-------|
| Setup EXE | 5-10 MB | Bootstrapper |
| MSI (framework) | 50-100 MB | Needs .NET |
| MSI (self-contained) | 150-200 MB | All included |
| .NET 10 download | ~150 MB | One-time |

---

## Success Checklist

Your build is ready when you see:

```
================================
BUILD COMPLETED SUCCESSFULLY!
================================

MSI Package: 87.45 MB
  Location: C:\...\NonProfitFinance.msi

Setup Executable: 8.23 MB
  Location: C:\...\NonProfitFinanceSetup.exe

Next Steps:
  1. Test on clean Windows VM
  2. Distribute NonProfitFinanceSetup.exe
  3. Include BETA_TESTER_QUICK_START.md
```

---

## Quick Commands

```powershell
# Clean everything and rebuild
Remove-Item publish -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item Installer\*.wixobj, Installer\*.msi, Installer\*.exe -Force -ErrorAction SilentlyContinue
.\Build-Installer-Simple.ps1

# Check file sizes
Get-ChildItem Installer\*.msi, Installer\*.exe | ForEach-Object { "{0}: {1:N2} MB" -f $_.Name, ($_.Length/1MB) }

# Test .NET detection
dotnet --list-runtimes
```

---

## Ready to Go!

✅ Scripts fixed (no emoji issues)  
✅ Build instructions clear  
✅ Two script options available  
✅ Troubleshooting guide included  

**You're ready to build and distribute your installer!**

Run: `.\Build-Installer-Simple.ps1`
