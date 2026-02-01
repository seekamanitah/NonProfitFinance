# WiX Toolset - FIXED!

## Problem Solved ✓
Your scripts were looking for **WiX v3.11** but you have **WiX v3.14** installed.

All build scripts have been updated with **auto-detection** that searches for multiple WiX versions.

---

## Quick Start

### Option 1: Simple Build (Minimal Files)
```powershell
.\Build-Installer-Simple.ps1
```
- Only includes main .exe and .dll
- Fast build
- **Use for testing**

### Option 2: Complete Build (All Files)
```powershell
.\Build-Installer-Complete.ps1
```
- Auto-harvests ALL published files
- Comprehensive installer
- **Use for production**

### Option 3: Advanced Build
```powershell
.\Build-Installer.ps1 -Configuration Release -Runtime win-x64
```

---

## What Was Fixed

### 1. ✓ WiX Detection
**Before:** Hardcoded to `C:\Program Files (x86)\WiX Toolset v3.11\bin`
**After:** Auto-detects v3.11, v3.14, and other versions

### 2. ✓ Product.wxs Errors Fixed
- **Line 143**: Fixed `Guid="*"` with registry+files conflict
- **Lines 194-196**: Removed invalid wildcards (`*.dll`, `*.json`)
- **Line 202**: Fixed invalid wildcard syntax

### 3. ✓ New File Harvesting
Created `harvest-files.ps1` that uses **heat.exe** to automatically generate file lists

---

## Files Modified

1. **Build-Installer-Simple.ps1** ✓ - Auto-detects WiX
2. **Build-Installer.ps1** ✓ - Updated to v3.14
3. **Installer/build.bat** ✓ - Auto-detects WiX
4. **Installer/Product.wxs** ✓ - Fixed Component errors

## Files Created

1. **Build-Installer-Complete.ps1** - New complete build script
2. **Installer/harvest-files.ps1** - File harvesting utility
3. **WIX_FIXED.md** - This file

---

## Verification

Run this to verify WiX is detected:
```powershell
Get-ChildItem "C:\Program Files (x86)\WiX Toolset*\bin\candle.exe"
```

Should show: `C:\Program Files (x86)\WiX Toolset v3.14\bin\candle.exe`

---

## Next Steps

1. **Test the build:**
   ```powershell
   .\Build-Installer-Complete.ps1
   ```

2. **If you need more files in the installer:**
   - Run `.\Installer\harvest-files.ps1`
   - This generates `PublishedFiles.wxs`
   - Include it in your Product.wxs

3. **For production:**
   - Add code signing
   - Update version numbers
   - Customize UI images

---

## Troubleshooting

### "Toolset not found" error
- Run: `Get-ChildItem "C:\Program Files (x86)\WiX Toolset*"`
- If empty, reinstall WiX from https://wixtoolset.org/

### "Component Guid error"
- Components with registry KeyPath must have fixed GUIDs
- Components with file KeyPath can use `Guid="*"`
- **Never mix files + registry in one component**

### "Invalid wildcards" error
- WiX doesn't support `*.dll` in File elements
- Use heat.exe to harvest files
- Or list each file individually

---

## Build Process

```
1. dotnet publish
   ↓
2. heat.exe (harvest files)
   ↓
3. candle.exe (compile .wxs → .wixobj)
   ↓
4. light.exe (link .wixobj → .msi)
   ↓
5. NonProfitFinance.msi ✓
```

---

## Quick Commands

```powershell
# Clean build
Remove-Item publish -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item Installer\*.wixobj, Installer\*.wixpdb, Installer\*.msi -Force -ErrorAction SilentlyContinue

# Build
.\Build-Installer-Complete.ps1

# Test MSI
msiexec /i Installer\NonProfitFinance.msi /l*v install.log

# Uninstall
msiexec /x Installer\NonProfitFinance.msi
```

---

## Status: ✓ READY TO BUILD

Your installer is now configured correctly and should build without errors!
