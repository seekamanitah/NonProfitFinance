# 🔧 Import Failure Fix - Duplicate Categories

## Problem
Import fails with error: **"An item with the same key has already been added. Key: dinners"**

**Root Cause:** Your database has duplicate category names (case-insensitive), causing the import dictionary creation to fail.

---

## ✅ **IMMEDIATE FIX** (Already Applied)

The import service has been updated to handle duplicates gracefully:

**File:** `Services/ImportExportService.cs` (line 128)

**Before:**
```csharp
var existingCategories = await _context.Categories
    .ToDictionaryAsync(c => c.Name.ToLower(), c => c.Id);
```

**After:**
```csharp
var existingCategories = (await _context.Categories.ToListAsync())
    .GroupBy(c => c.Name.ToLower())
    .ToDictionary(g => g.Key, g => g.First().Id);
```

This uses `GroupBy` to handle duplicates by taking the first occurrence.

---

## 🔍 **Check for Duplicates**

### Option 1: SQL Query (Quick)
Open your database in **DB Browser for SQLite** and run:

```sql
SELECT 
    Name,
    COUNT(*) as DuplicateCount,
    GROUP_CONCAT(Id) as CategoryIds
FROM Categories
GROUP BY LOWER(Name)
HAVING COUNT(*) > 1;
```

### Option 2: PowerShell Script
```powershell
.\Check-DatabaseDuplicates.ps1
```

---

## 🛠️ **Fix Duplicates Permanently**

### Method 1: Use SQL Script (Recommended)
1. Open `fix_duplicate_categories.sql` in DB Browser for SQLite
2. Review what will be kept/deleted (queries 1-2)
3. Execute the update/delete queries (steps 2-5)
4. Verify with final query (step 6)

### Method 2: Use Maintenance Service (Code-based)
1. Register the service in `Program.cs`:
```csharp
builder.Services.AddScoped<DatabaseMaintenanceService>();
```

2. Add a maintenance page or run from startup:
```csharp
// Check for duplicates
var maintenanceService = app.Services
    .CreateScope().ServiceProvider
    .GetRequiredService<DatabaseMaintenanceService>();

var duplicates = await maintenanceService.CheckForDuplicatesAsync();
if (duplicates.HasDuplicates)
{
    logger.LogWarning($"Found {duplicates.CategoryDuplicates.Count} duplicate category groups");
    // Optionally auto-fix:
    // await maintenanceService.FixCategoryDuplicatesAsync();
}
```

---

## 🎯 **Test Import After Fix**

1. **Rebuild and restart** your application
2. Navigate to **Import/Export** page
3. Try importing your CSV file again
4. Import should now complete successfully

---

## 📋 **Common Duplicate Scenarios**

### Why duplicates occur:
1. **Case variations:** "Dinners" vs "dinners" vs "DINNERS"
2. **Whitespace:** "Food " vs "Food"
3. **Data seeding:** Running seed multiple times
4. **Manual entry:** Users creating similar categories

### Prevention:
Add unique index (case-insensitive):
```csharp
// In ApplicationDbContext.OnModelCreating:
modelBuilder.Entity<Category>()
    .HasIndex(c => c.Name)
    .IsUnique()
    .HasFilter(null); // Add COLLATE NOCASE for SQLite
```

---

## ✅ **Quick Test**

Run this after fixing:
```sql
-- Should return 0 rows
SELECT Name, COUNT(*) 
FROM Categories 
GROUP BY LOWER(Name) 
HAVING COUNT(*) > 1;
```

---

## 📝 **Files Created**

| File | Purpose |
|------|---------|
| `fix_duplicate_categories.sql` | SQL script to merge duplicates |
| `Check-DatabaseDuplicates.ps1` | PowerShell diagnostic tool |
| `Services/DatabaseMaintenanceService.cs` | C# maintenance utility |
| `IMPORT_DUPLICATE_FIX.md` | This guide |

---

## 🚨 **If Import Still Fails**

1. **Check the logs** for the specific duplicate key
2. **Verify the fix** was applied (check line 128 in ImportExportService.cs)
3. **Restart the application** to reload the service
4. **Check for duplicates** in Funds and Donors tables too
5. **Review CSV file** for malformed data

---

## 💡 **Next Steps**

After import works:
1. ✅ Clean up remaining duplicates
2. ✅ Add unique constraints to prevent future duplicates
3. ✅ Add validation in UI when creating categories
4. ✅ Add duplicate detection before insert

---

**Status:** ✅ Import service fixed - ready to test!
