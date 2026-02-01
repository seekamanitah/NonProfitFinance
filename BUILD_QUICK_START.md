# 🚀 INSTALLER BUILD - QUICK START

## ✅ WiX Issue FIXED!

The `.NET Framework` error is resolved. Here's how to build:

---

## 1️⃣ **One-Command Build**

```powershell
.\Build-Installer-Complete.ps1
```

**This does everything:**
- ✅ Detects WiX Toolset
- ✅ Publishes your app
- ✅ Harvests files
- ✅ Compiles WiX
- ✅ Creates MSI

---

## 2️⃣ **Manual Build** (if you prefer)

### Step 1: Publish
```powershell
dotnet publish -c Release -r win-x64 --self-contained false -o publish
```

### Step 2: Build Installer
```powershell
.\Build-Installer-Simple.ps1
```

### Output:
```
Installer\NonProfitFinance.msi
```

---

## 3️⃣ **Test the MSI**

```powershell
# Install
msiexec /i Installer\NonProfitFinance.msi

# OR install with logging
msiexec /i Installer\NonProfitFinance.msi /l*v install.log

# Uninstall
msiexec /x Installer\NonProfitFinance.msi
```

---

## 📋 **Pre-Build Checklist**

- ✅ WiX Toolset v3.14 installed
- ✅ .NET 10 SDK installed
- ✅ Project builds without errors: `dotnet build`
- ✅ All files you need are referenced in `Product.wxs`

---

## 🔍 **Troubleshooting**

### "WiX not found"
```powershell
.\Test-WixSetup.ps1
```

### "Cannot find file"
- Make sure `dotnet publish` completed successfully
- Check that `publish\` folder exists and has your files

### "Component GUID error"
- Already fixed in Product.wxs
- Use `Guid="*"` for file-only components
- Use fixed GUID if component has registry keys

---

## 📦 **What Gets Installed**

- `C:\Program Files\Your Organization\NonProfit Finance Manager\`
  - NonProfitFinance.exe
  - NonProfitFinance.dll
  - appsettings.json
  - All dependencies

- Start Menu shortcut
- Desktop shortcut (optional)

---

## ⚙️ **Build Options**

```powershell
# Simple build (minimal files)
.\Build-Installer-Simple.ps1

# Complete build (all files)
.\Build-Installer-Complete.ps1

# Advanced options
.\Build-Installer.ps1 -Configuration Release -Runtime win-x64
```

---

## 🎯 **Common Commands**

```powershell
# Clean everything
Remove-Item publish -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item Installer\*.wixobj,Installer\*.wixpdb,Installer\*.msi -Force -ErrorAction SilentlyContinue

# Build fresh
.\Build-Installer-Complete.ps1

# Check WiX version
& "C:\Program Files (x86)\WiX Toolset v3.14\bin\candle.exe" -?

# Test compile only (no linking)
cd Installer
& "C:\Program Files (x86)\WiX Toolset v3.14\bin\candle.exe" Product.wxs -dPublishDir="..\publish"
```

---

## 📝 **Notes**

### .NET 10 Runtime
Your installer does NOT include .NET 10 runtime.
Users must install it separately from:
https://dotnet.microsoft.com/download/dotnet/10.0

### File Harvesting
`Build-Installer-Complete.ps1` uses `heat.exe` to automatically include all published files.

### Customization
Edit `Product.wxs` to:
- Change product name/version
- Add/remove files
- Modify install location
- Customize UI images

---

## ✅ Status

| Component | Status |
|-----------|--------|
| WiX Detection | ✅ Working |
| .NET Framework Error | ✅ Fixed |
| Product.wxs | ✅ Valid |
| Build Scripts | ✅ Ready |
| Documentation | ✅ Complete |

---

## 🎉 You're Ready!

Run this now:
```powershell
.\Build-Installer-Complete.ps1
```

Wait for:
```
[SUCCESS] MSI created successfully!
Installer: Installer\NonProfitFinance.msi
```

Done! 🎊
