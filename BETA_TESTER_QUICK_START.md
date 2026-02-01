# NonProfit Finance Manager - Beta Tester Quick Start

## Thank You for Beta Testing!

Your feedback is invaluable in making this the best financial management system for nonprofit organizations.

---

## Installation (5 minutes)

### Step 1: Download
- Download `NonProfitFinanceSetup.exe` from the link provided

### Step 2: Run Installer
1. **Right-click** `NonProfitFinanceSetup.exe`
2. Select **"Run as Administrator"**
3. Click **"Yes"** when prompted by Windows

### Step 3: Installation Wizard
1. **Welcome Screen** - Click Next
2. **License Agreement** - Read and accept
3. **Installation Folder** - Keep default or choose custom
4. **Install** - Click Install button

### Step 4: .NET Installation
- If you don't have .NET 10, the installer will download it automatically
- This takes 2-5 minutes (one-time only)
- Internet connection required

### Step 5: Complete
- Click Finish
- Application shortcut added to Desktop and Start Menu

---

## First Launch

### Open the Application
- Double-click **Desktop shortcut**
- Or: Start Menu → NonProfit Finance Manager

### First Time Setup
The application will:
1. Initialize the database (automatic)
2. Create default categories (automatic)
3. Show the Dashboard

**You're ready to test!**

---

## System Requirements

✅ **Operating System:**
- Windows 10 (64-bit) or newer
- Windows 11 (64-bit)

✅ **Hardware:**
- 4 GB RAM minimum (8 GB recommended)
- 500 MB free disk space
- 1280x720 screen resolution minimum

✅ **Software:**
- .NET 10 Runtime (installed automatically)
- Internet connection (first time only)

---

## Testing Focus Areas

### Priority 1: Core Features
- [ ] Dashboard displays correctly
- [ ] Create income transaction
- [ ] Create expense transaction
- [ ] View transaction list
- [ ] Categories load properly
- [ ] Reports generate

### Priority 2: Advanced Features
- [ ] Fund accounting
- [ ] Grants management
- [ ] Donor tracking
- [ ] Budget creation
- [ ] Form 990 generation
- [ ] Backup/Restore

### Priority 3: UI/UX
- [ ] Dark mode toggle
- [ ] Responsive design
- [ ] Navigation smooth
- [ ] Buttons responsive
- [ ] Forms validate correctly

---

## What to Test

### 1. Transactions ⭐ HIGH PRIORITY
```
Test Steps:
1. Click "+ Add Transaction"
2. Fill in:
   - Date: Today
   - Amount: $100.00
   - Category: Choose any
   - Description: "Test transaction"
3. Click Save
4. Verify it appears in list
```

**Report if:**
- Save button doesn't work
- Transaction doesn't appear
- Amount formats incorrectly
- Date picker has issues

### 2. Categories
```
Test Steps:
1. Go to Categories page
2. Click "Add Category"
3. Create Income category
4. Create Expense category
5. Try creating subcategory
```

**Report if:**
- Categories don't save
- Tree structure broken
- Colors don't display
- Icons missing

### 3. Reports
```
Test Steps:
1. Go to Reports
2. Select "Income Statement"
3. Set date range: Last 30 days
4. Click Generate
5. Try exporting to PDF
```

**Report if:**
- Report doesn't generate
- Data is incorrect
- PDF export fails
- Charts don't display

### 4. Dark Mode
```
Test Steps:
1. Click theme toggle (moon icon)
2. Switch between light/dark
3. Navigate different pages
4. Check all colors readable
```

**Report if:**
- Text unreadable
- Colors clash
- Icons don't change
- Pages stay light/dark when shouldn't

---

## How to Report Issues

### Bug Report Template

**Copy and paste this into email:**

```
Bug Report

Title: [Brief description]

Steps to Reproduce:
1. 
2. 
3. 

Expected Result:
[What should happen]

Actual Result:
[What actually happened]

Screenshots:
[Attach if possible]

System Info:
- Windows Version: [e.g., Windows 11]
- App Version: 1.0.0 Beta
- Occurred: [Date and time]

Severity:
[ ] Critical - App crashes/unusable
[ ] High - Feature doesn't work
[ ] Medium - Feature works but has issues
[ ] Low - Minor visual issue
```

### Where to Report

**Email:** support@yourorganization.com
**Subject:** [BETA] Bug Report - [Brief Description]

**Or use feedback form:**
https://forms.yourorg.com/beta-feedback

---

## Common Issues & Solutions

### Issue: "Cannot start application"
**Solution:**
1. Check if .NET 10 is installed:
   - Open PowerShell
   - Run: `dotnet --list-runtimes`
   - Should show: `Microsoft.AspNetCore.App 10.0.x`
2. If not listed, run installer again

### Issue: "Database error"
**Solution:**
1. Close application
2. Delete: `C:\Program Files\Your Organization\NonProfit Finance Manager\NonProfitFinance.db`
3. Restart application (will recreate database)

### Issue: "Installer won't run"
**Solution:**
1. Right-click installer
2. Properties
3. Unblock checkbox (if present)
4. Run as Administrator

### Issue: "Application slow"
**Solution:**
1. Check available RAM
2. Close other applications
3. Restart computer
4. Report if persists

---

## Testing Checklist

Use this to track your testing:

### Installation Testing
- [ ] Downloaded installer successfully
- [ ] Ran as administrator
- [ ] Accepted license
- [ ] .NET installed automatically
- [ ] Desktop shortcut created
- [ ] Start menu shortcut created
- [ ] Application launches

### Core Functionality
- [ ] Dashboard loads
- [ ] Can create transaction
- [ ] Can edit transaction
- [ ] Can delete transaction
- [ ] Can view transaction list
- [ ] Pagination works
- [ ] Search works
- [ ] Filters work

### Categories
- [ ] Can create category
- [ ] Can edit category
- [ ] Can delete category
- [ ] Subcategories work
- [ ] Color picker works
- [ ] Icon selection works

### Reports
- [ ] Income statement generates
- [ ] Balance sheet generates
- [ ] Cash flow generates
- [ ] Custom date ranges work
- [ ] PDF export works
- [ ] Charts display correctly

### Advanced Features
- [ ] Funds page works
- [ ] Grants page works
- [ ] Donors page works
- [ ] Budgets page works
- [ ] Documents page works
- [ ] Form 990 page works

### Settings
- [ ] Organization settings save
- [ ] Dark mode toggle works
- [ ] Backup creates successfully
- [ ] Restore works
- [ ] Import/Export works

### UI/UX
- [ ] All buttons clickable
- [ ] Forms validate
- [ ] Error messages clear
- [ ] Navigation smooth
- [ ] Icons load properly
- [ ] Tables responsive
- [ ] Modals function correctly

---

## Feedback We Need

### What's Working Well ✅
- Which features do you love?
- What's intuitive and easy?
- What saves you time?

### What Needs Improvement ⚠️
- What's confusing?
- What's slow or clunky?
- What's missing?

### Feature Requests 💡
- What would make this better?
- What features do you need?
- What similar apps do you compare this to?

---

## Beta Testing Timeline

**Week 1-2:** Core Features Testing
- Transactions, Categories, Basic Reports

**Week 3-4:** Advanced Features
- Funds, Grants, Donors, Budgets

**Week 5-6:** Polish & Bug Fixes
- UI improvements, Performance tuning

**Week 7:** Final Testing
- Regression testing, Sign-off

---

## Uninstalling (If Needed)

### Method 1: Settings
1. Windows Settings
2. Apps
3. Find "NonProfit Finance Manager"
4. Click Uninstall

### Method 2: Control Panel
1. Control Panel
2. Programs and Features
3. Right-click "NonProfit Finance Manager"
4. Uninstall

**Note:** Your data will be preserved in case you reinstall

---

## Getting Help

### Resources
- **User Guide:** Help menu in application
- **Video Tutorials:** https://youtube.com/yourorg
- **FAQ:** https://yourorg.com/faq
- **Email Support:** support@yourorg.com

### Support Hours
- Monday-Friday: 9 AM - 5 PM EST
- Response time: Within 24 hours

### Beta Tester Community
- Slack: yourorg.slack.com/beta-testers
- Discord: discord.gg/yourorg-beta
- Forum: forum.yourorg.com/beta

---

## Thank You! 🎉

Your participation in this beta test helps us create better software for nonprofit organizations everywhere.

**Every bug you find makes the product better.**
**Every suggestion helps us improve.**
**Every minute you test is valuable.**

**We appreciate you!**

---

## Quick Reference Card

| Action | How To |
|--------|--------|
| **Add Transaction** | Dashboard → + Add Transaction |
| **View All Transactions** | Sidebar → Transactions |
| **Create Category** | Sidebar → Categories → + Add |
| **Generate Report** | Sidebar → Reports → Choose type |
| **Toggle Dark Mode** | Top-right → Moon/Sun icon |
| **Backup Data** | Settings → Backup & Restore → Create Backup |
| **Get Help** | Help menu (?) in top-right |

---

**Version:** 1.0.0 Beta  
**Updated:** January 29, 2026  
**Contact:** support@yourorganization.com
