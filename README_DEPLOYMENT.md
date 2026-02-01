# NonProfit Finance Application - Deployment Packages

## ?? Two Deployment Options Available

### Option 1: Self-Contained Package (RECOMMENDED for Windows 11)
**File:** `NonProfitFinance_SELFCONTAINED_20260128_225836.zip` (64 MB)

? **NO .NET INSTALLATION REQUIRED**
? **100% Standalone**
? **Windows 11 Compatible**
? **Just Extract & Run**

#### How to Run (Self-Contained):
1. **Extract ZIP** to any folder (e.g., `C:\NonProfitFinance`)
2. **Double-click** `NonProfitFinance.exe`
3. **Browser opens automatically** at `https://localhost:5001`
4. **Done!** No installation needed.

#### What's Included:
- ? Complete .NET 10 Runtime
- ? All application files
- ? All dependencies
- ? Ready to run immediately

---

### Option 2: Source Code Package (For Developers)
**File:** `NonProfitFinance_20260128_225044.zip` (5 MB)

? **Requires .NET 10 SDK Installation**
? **Smaller file size**
? **Full source code**
? **Can be modified/customized**

#### How to Run (Source Code):
1. **Install .NET 10 SDK** from https://dotnet.microsoft.com/download/dotnet/10.0
2. **Extract ZIP** to a folder
3. **Open Terminal** in that folder
4. **Run:**
   ```powershell
   dotnet restore
   dotnet build
   dotnet run
   ```
5. **Browser opens** at `https://localhost:5001`

---

## ?? Which Package Should You Use?

| Scenario | Recommended Package |
|----------|-------------------|
| **Just want to use the app** | Self-Contained (64 MB) |
| **Testing on Windows 11** | Self-Contained (64 MB) |
| **No .NET installed** | Self-Contained (64 MB) |
| **Want to modify code** | Source Code (5 MB) |
| **Developer/Programmer** | Source Code (5 MB) |

---

## ?? System Requirements

### For Self-Contained Package:
- ? **Windows 11** (64-bit)
- ? **~500 MB free disk space**
- ? **No other requirements**

### For Source Code Package:
- ? **Windows 11** (64-bit)
- ? **.NET 10 SDK** (download separately)
- ? **Visual Studio 2022** (optional, for GUI development)
- ? **~1 GB free disk space**

---

## ?? First Time Setup (Both Packages)

After running the application for the first time:

1. **Database Created:** `NonProfitFinance.db` automatically created
2. **Default Categories:** Automatically seeded
3. **Demo Data:** Go to Settings ? Demo Data ? Click "Load Demo Data"
4. **Create First Budget:** Go to Budgets ? Click "Create Budget"

---

## ?? File Structure (Self-Contained)

```
NonProfitFinance_Standalone/
??? NonProfitFinance.exe        ? Double-click to run
??? NonProfitFinance.dll
??? appsettings.json            ? Configuration
??? wwwroot/                    ? Static files (CSS, JS)
??? *.dll                       ? Runtime libraries
??? NonProfitFinance.db         ? Created on first run
```

---

## ?? Configuration (Optional)

Edit `appsettings.json` to change:
- Database location
- Logging level
- Port numbers
- Backup directory

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=NonProfitFinance.db"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

---

## ?? Accessing the Application

After starting:
- **HTTPS:** `https://localhost:5001`
- **HTTP:** `http://localhost:5000`

The browser opens automatically to the correct URL.

---

## ?? Data Storage

All data stored in `NonProfitFinance.db` (SQLite database)

**Backup:**
- Go to Settings ? Backup
- Click "Create Backup Now"
- Default location: `%AppData%\NonProfitFinance\Backups`

---

## ?? Troubleshooting

### Self-Contained Package Issues:

**"Windows protected your PC" message:**
- Click "More info"
- Click "Run anyway"
- (This is normal for unsigned executables)

**Port already in use:**
- Edit `appsettings.json`
- Change port numbers in `urls` section

**Browser doesn't open:**
- Manually navigate to `https://localhost:5001`

### Source Code Package Issues:

**Build errors:**
- Ensure .NET 10 SDK is installed
- Run `dotnet restore` first
- Check internet connection (downloads NuGet packages)

**"SDK not found":**
- Install .NET 10 SDK from Microsoft
- Restart terminal after installation

---

## ?? Key Features

? **Transaction Management** - Income/expenses with categories
? **Accounts (Funds)** - Restricted/unrestricted tracking
? **Donors** - Contribution tracking & reporting
? **Grants** - Application tracking & usage monitoring
? **Budgets** - Budget vs actual with alerts
? **Reports** - Financial reports with PDF/Excel export
? **Cash Flow Forecast** - 90-day projections
? **Form 990** - IRS reporting assistance
? **Compliance** - Audit alerts & reminders
? **Demo Data** - Load sample data for testing
? **Dark Mode** - Light/dark theme support
? **Backups** - Automatic and manual database backups

---

## ?? Support

For issues or questions:
1. Check this README
2. Review the application's built-in help
3. Check logs in: `%AppData%\NonProfitFinance\Logs`

---

## ?? Security Notes

- Database contains sensitive financial data
- Keep regular backups
- Database file is NOT encrypted by default
- Restrict folder permissions as needed
- For production use, consider:
  - Moving database to secure location
  - Implementing user authentication
  - Using HTTPS with valid certificate

---

## ?? Version Information

- **Application Version:** 1.0.0
- **Build Date:** January 28, 2025
- **Framework:** .NET 10
- **Database:** SQLite 3
- **UI Framework:** Blazor Server

---

## ? Quick Start Checklist

**Self-Contained Package:**
- [ ] Extract ZIP file
- [ ] Double-click `NonProfitFinance.exe`
- [ ] Browser opens automatically
- [ ] Load demo data from Settings
- [ ] Start using the application!

**Source Code Package:**
- [ ] Install .NET 10 SDK
- [ ] Extract ZIP file
- [ ] Open terminal in folder
- [ ] Run `dotnet restore`
- [ ] Run `dotnet build`
- [ ] Run `dotnet run`
- [ ] Browser opens automatically
- [ ] Load demo data from Settings

---

**Both packages are on your Desktop and ready to use!**

?? **Happy Financial Management!** ??
