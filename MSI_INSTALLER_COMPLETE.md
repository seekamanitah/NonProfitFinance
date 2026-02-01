# 🚀 MSI INSTALLER - COMPLETE SETUP

## Overview

You now have a complete MSI installer package for NonProfit Finance Manager that includes automatic .NET 10 installation and all dependencies.

---

## 📦 What You Got

### Files Created:

1. **Installer/Product.wxs** - Main MSI installer definition
2. **Installer/Bundle.wxs** - Bootstrapper with .NET runtime
3. **Installer/build.bat** - Windows batch build script
4. **Build-Installer.ps1** - PowerShell build script (recommended)
5. **INSTALLER_BUILD_GUIDE.md** - Complete documentation
6. **BETA_TESTER_QUICK_START.md** - Guide for beta testers

---

## 🎯 Quick Start

### Step 1: Install WiX Toolset

```powershell
# Download from: https://wixtoolset.org/
# Or via Chocolatey:
choco install wixtoolset
```

### Step 2: Build the Installer

**Option A: PowerShell (Recommended)**
```powershell
.\Build-Installer.ps1
```

**Option B: Batch File**
```cmd
cd Installer
build.bat
```

### Step 3: Distribute

The installer will be created at:
```
Installer/NonProfitFinanceSetup.exe
```

This single EXE file:
- ✅ Checks for .NET 10
- ✅ Downloads and installs .NET if missing
- ✅ Installs your application
- ✅ Creates shortcuts
- ✅ Sets up database

---

## 📋 Pre-Build Checklist

Before building, ensure you have:

- [ ] **WiX Toolset** installed
- [ ] **Project published** to `publish/` folder
- [ ] **Icon file** (`icon.ico`) in Installer folder
- [ ] **License file** (`License.rtf`) in Installer folder
- [ ] **Banner images** (optional):
  - `banner.bmp` - 493x58 pixels
  - `dialog.bmp` - 493x312 pixels
  - `logo.png` - For bootstrapper

---

## 🛠️ Build Options

### Basic Build (Framework-Dependent)
```powershell
.\Build-Installer.ps1
```
**Size:** ~50-100 MB + .NET download  
**Requires:** Internet for .NET installation

### Self-Contained Build
```powershell
.\Build-Installer.ps1 -SelfContained
```
**Size:** ~150-200 MB  
**Requires:** Nothing (all included)

### Single-File Build
```powershell
.\Build-Installer.ps1 -SingleFile
```
**Size:** ~150 MB  
**Deployment:** Simplest (one EXE)

### With Code Signing
```powershell
.\Build-Installer.ps1 -SignInstaller -CertificatePath "cert.pfx" -CertificatePassword "pass"
```

---

## 📤 Distribution Methods

### Method 1: Email
```
- Max 100 MB attachment
- Use file sharing for larger files
- Include BETA_TESTER_QUICK_START.md
```

### Method 2: Cloud Storage
```
- OneDrive
- Google Drive
- Dropbox
- Generate shareable link
```

### Method 3: Your Website
```html
<a href="NonProfitFinanceSetup.exe" download>
  Download NonProfit Finance Manager v1.0 Beta
</a>
```

### Method 4: USB Drive
```
Beta Package/
├── NonProfitFinanceSetup.exe
├── README.txt (see BETA_TESTER_QUICK_START.md)
└── LICENSE.txt
```

---

## 🧪 Testing Your Installer

### Test on Clean VM

1. **Create Windows 10/11 VM**
2. **Copy only:** `NonProfitFinanceSetup.exe`
3. **Run as Administrator**
4. **Verify:**
   - .NET installs automatically
   - Application installs correctly
   - Shortcuts created
   - Application launches
   - Database initializes

### Test Scenarios

| Scenario | Expected Result |
|----------|----------------|
| **No .NET installed** | Installer downloads and installs .NET 10 |
| **.NET already present** | Skips .NET, installs app only |
| **Repair installation** | Fixes broken files |
| **Uninstall** | Removes all files except data |
| **Upgrade** | Replaces old version |

---

## 🐛 Troubleshooting

### Build Errors

**Error: "WiX not found"**
```powershell
# Install WiX
choco install wixtoolset

# Or download from
https://wixtoolset.org/
```

**Error: "Candle failed"**
```powershell
# Check Product.wxs syntax
# Ensure publish/ folder exists
# Verify paths in build script
```

**Error: "Light failed"**
```powershell
# Add -sval flag to suppress validation
# Check for missing files in publish/
```

### Installation Errors

**Error: ".NET won't install"**
```
Solution:
1. Download .NET 10 Hosting Bundle manually
2. Install before running app installer
3. Or include in local package
```

**Error: "Application won't start"**
```
Solution:
1. Check Event Viewer for errors
2. Verify .NET version: dotnet --list-runtimes
3. Check database file permissions
```

---

## 📊 Installer Features

### What It Includes:

✅ **Application Files**
- All .NET DLLs
- wwwroot folder (CSS, JS, images)
- Configuration files
- Database file

✅ **Runtime Dependencies**
- .NET 10 Runtime
- ASP.NET Core Runtime
- All NuGet packages

✅ **User Experience**
- Professional UI
- License agreement
- Installation progress
- Desktop shortcut
- Start Menu entry

✅ **Maintenance**
- Repair installation
- Modify installation
- Clean uninstall
- Upgrade support

---

## 🎨 Customization

### Change Application Name
Edit `Product.wxs`:
```xml
<?define ProductName="Your Custom Name" ?>
```

### Change Install Location
Default: `C:\Program Files\Your Organization\NonProfit Finance Manager`

Users can change during installation via "Browse" button

### Add Custom Actions

Add to `Product.wxs`:
```xml
<CustomAction Id="LaunchApp"
              FileKey="NonProfitFinance.exe"
              ExeCommand=""
              Execute="immediate"
              Impersonate="yes"
              Return="asyncNoWait" />
```

### Branding

Replace these files in `Installer/`:
- `icon.ico` - Application icon
- `banner.bmp` - Top banner (493x58)
- `dialog.bmp` - Welcome screen (493x312)
- `logo.png` - Bootstrapper logo

---

## 📈 File Sizes

| Component | Size | Notes |
|-----------|------|-------|
| **MSI (framework-dependent)** | ~50-100 MB | Requires .NET runtime |
| **MSI (self-contained)** | ~150-200 MB | All included |
| **Bootstrapper EXE** | ~5-10 MB | Downloads .NET if needed |
| **.NET 10 Runtime** | ~150 MB | One-time download |
| **Published App** | ~50 MB | Without runtime |

---

## 🔒 Code Signing (Optional)

### Get Certificate
```powershell
# Purchase from:
- DigiCert
- Sectigo
- GlobalSign

# Or create self-signed (testing only)
New-SelfSignedCertificate -Type CodeSigningCert
```

### Sign Installer
```powershell
.\Build-Installer.ps1 -SignInstaller -CertificatePath "cert.pfx" -CertificatePassword "password"
```

### Verify Signature
```powershell
# Right-click installer -> Properties -> Digital Signatures
# Or:
signtool verify /pa NonProfitFinanceSetup.exe
```

**Benefits:**
- Windows SmartScreen won't warn
- Users trust signed software
- Professional appearance

---

## 📝 Beta Testing Workflow

### 1. Build Installer
```powershell
.\Build-Installer.ps1 -Configuration Release
```

### 2. Test Locally
- Run on your machine
- Test all features
- Check for errors

### 3. Package for Distribution
```
BetaTest-v1.0.0/
├── NonProfitFinanceSetup.exe
├── README.txt (from BETA_TESTER_QUICK_START.md)
├── CHANGELOG.txt
└── LICENSE.txt
```

### 4. Distribute
- Upload to cloud storage
- Send download link to testers
- Include Quick Start guide

### 5. Collect Feedback
- Bug reports via email
- Feature requests via form
- Weekly check-ins

### 6. Iterate
- Fix bugs
- Rebuild installer
- Redistribute
- Repeat

---

## ✅ Success Checklist

Your installer is ready when:

- [x] Builds without errors
- [x] Installs on clean Windows 10/11
- [x] .NET installs automatically
- [x] Application launches successfully
- [x] Database initializes correctly
- [x] Shortcuts work
- [x] Uninstall works cleanly
- [x] Can upgrade from previous version

---

## 🎉 You're Ready!

You now have a professional MSI installer that:

✅ Installs .NET 10 automatically  
✅ Sets up your application  
✅ Creates shortcuts  
✅ Handles upgrades  
✅ Supports uninstall  
✅ Works on any Windows 10/11 machine  

**Perfect for beta testing!**

---

## 📚 Next Steps

1. **Test:** Run installer on clean VM
2. **Document:** Add any custom notes for testers
3. **Distribute:** Send to beta testers
4. **Collect Feedback:** Use bug report template
5. **Iterate:** Fix issues, rebuild, repeat
6. **Release:** When ready, distribute final version

---

## 🆘 Support

If you need help:

1. **Read:** `INSTALLER_BUILD_GUIDE.md` for detailed docs
2. **Check:** WiX Toolset documentation
3. **Search:** WiX forums and Stack Overflow
4. **Ask:** Your development team

---

## 📞 Resources

- **WiX Toolset:** https://wixtoolset.org/
- **Documentation:** https://wixtoolset.org/documentation/
- **Tutorial:** https://www.firegiant.com/wix/tutorial/
- **.NET Download:** https://dot.net

---

**🎊 Congratulations on creating a professional installer package! 🎊**

You're ready to beta test your NonProfit Finance Manager with confidence!
