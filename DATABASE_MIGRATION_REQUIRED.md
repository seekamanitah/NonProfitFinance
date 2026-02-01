# ?? Database Migration Required

**Date:** 2024-01-29  
**Issue:** SQLite Error - Column `t.ProjectId` doesn't exist  
**Status:** ?? **ACTION REQUIRED**

---

## ?? Problem

The application is trying to query a `ProjectId` column in the `Transactions` table that doesn't exist yet because the database migration hasn't been applied.

**Error:**
```
SQLite Error 1: 'no such column: t.ProjectId'
```

---

## ? What Was Fixed

### 1. Added ProjectId to Transaction Model
Added the missing `ProjectId` property to link transactions to maintenance projects:

```csharp
// Models/Transaction.cs
public int? ProjectId { get; set; }
```

### 2. Created Migration
Successfully created migration:
```
Migrations/20260129084932_AddInventoryAndMaintenanceModules.cs
```

This migration will:
- Add `ProjectId` column to Transactions table
- Add `PONumber` column to Transactions table (if not exists)
- Create all Inventory module tables
- Create all Maintenance module tables
- Create all Shared module tables (CustomFields, UserModuleAccess)

---

## ?? Next Steps - ACTION REQUIRED

### Step 1: Stop the Debugger
**The application is currently running**, which prevents the migration from being applied.

**In Visual Studio:**
- Click the **Stop** button (Shift+F5)
- Or close the browser window and stop debugging

### Step 2: Apply the Migration
Once the app is stopped, run this command in the terminal:

```bash
dotnet ef database update
```

This will:
- Add all new tables for Inventory and Maintenance modules
- Add ProjectId column to Transactions table
- Update the database schema

### Step 3: Restart the Application
After the migration completes successfully:
- Press **F5** to start debugging again
- The error should be resolved
- The landing page will load correctly

---

## ?? Migration Contents

The migration adds:

### New Tables:
- **Inventory Module:** (4 tables)
  - InventoryItems
  - InventoryCategories
  - Locations
  - InventoryTransactions

- **Maintenance Module:** (6 tables)
  - Buildings
  - Contractors
  - Projects
  - MaintenanceTasks
  - ServiceRequests
  - WorkOrders

- **Shared:** (3 tables)
  - CustomFields
  - CustomFieldValues
  - UserModuleAccess

### Modified Tables:
- **Transactions:**
  - Add ProjectId column (nullable INT)
  - Add PONumber column (nullable TEXT) if not exists

### Total: 13 new tables + 2 column additions

---

## ?? Verification

After applying the migration, verify it worked:

```bash
# Check migration status
dotnet ef migrations list

# Should show:
# 20260129084932_AddInventoryAndMaintenanceModules (Applied)
```

---

## ?? If Migration Fails

If you encounter errors during migration:

### Option 1: Check Database Lock
```bash
# Make sure no other process is using the database
# Close any SQLite tools, DB browsers, or other connections
```

### Option 2: Backup First (Recommended)
```bash
# Copy the database file before migration
copy nonprofitfinance.db nonprofitfinance.db.backup
```

### Option 3: Manual SQL (Last Resort)
If EF migration fails, you can manually add the column:

```sql
-- Add ProjectId column
ALTER TABLE Transactions ADD COLUMN ProjectId INTEGER NULL;

-- Add index
CREATE INDEX IX_Transactions_ProjectId ON Transactions(ProjectId);
```

---

## ? Success Indicators

After migration succeeds, you should see:

1. **? No SQLite errors** when loading the dashboard
2. **? Landing page loads** without errors
3. **? All module placeholders** work (Inventory, Maintenance)
4. **? Financial module** continues to work normally

---

## ?? Summary

**What happened:**
- Phase 1 & 2 added new models and relationships
- Migration was created but not applied
- Database is out of sync with code
- Application crashes on startup

**Resolution:**
1. Stop debugger
2. Run: `dotnet ef database update`
3. Restart application
4. Everything should work!

**Status:** ?? **BLOCKED** - Waiting for migration to be applied

---

**Next Command After Stopping Debugger:**
```bash
dotnet ef database update
```
