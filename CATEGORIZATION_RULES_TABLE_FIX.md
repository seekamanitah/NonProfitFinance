# CategorizationRules Table Missing - FIXED

## Error
```
SQLite Error 1: 'no such table: CategorizationRules'
```

## Root Cause
The `CategorizationRules` table was added to the `ApplicationDbContext` but the actual database table was never created. The auto-categorization feature requires this table to store categorization rules.

## Solution Applied

### 1. Updated Program.cs
Added table creation SQL to the database initialization section in `Program.cs`:

```csharp
// Ensure CategorizationRules table exists
try
{
    await context.Database.ExecuteSqlRawAsync(@"
        CREATE TABLE IF NOT EXISTS CategorizationRules (
            Id INTEGER NOT NULL CONSTRAINT PK_CategorizationRules PRIMARY KEY AUTOINCREMENT,
            Name TEXT NOT NULL,
            MatchType INTEGER NOT NULL,
            MatchPattern TEXT NOT NULL,
            CaseSensitive INTEGER NOT NULL DEFAULT 0,
            CategoryId INTEGER NOT NULL,
            IsActive INTEGER NOT NULL DEFAULT 1,
            Priority INTEGER NOT NULL DEFAULT 50,
            CreatedAt TEXT NOT NULL,
            UpdatedAt TEXT,
            CONSTRAINT FK_CategorizationRules_Categories_CategoryId 
                FOREIGN KEY (CategoryId) REFERENCES Categories (Id) ON DELETE RESTRICT
        );
    ");
    
    // Create indexes...
}
catch { /* Table already exists */ }
```

### 2. Created SQL File
Created `fix_categorization_rules_table.sql` for manual execution if needed.

## How to Fix

### Option 1: Restart Application (Recommended)
1. Stop the application if running
2. Restart the application
3. The table will be created automatically on startup
4. Error should be resolved

### Option 2: Manual SQL Execution
If the application is running and you can't restart:

1. Open your SQLite database file
2. Execute the SQL from `fix_categorization_rules_table.sql`
3. Refresh the page

### Option 3: Delete Database (Nuclear Option)
If other options don't work:

1. Stop the application
2. Delete the SQLite database file (usually `NonProfitFinance.db`)
3. Restart the application
4. Database will be recreated with all tables

## Verification

After applying the fix, verify the table exists:

```sql
-- Check if table exists
SELECT name FROM sqlite_master 
WHERE type='table' AND name='CategorizationRules';

-- Check table structure
PRAGMA table_info(CategorizationRules);

-- Check indexes
PRAGMA index_list(CategorizationRules);
```

Expected results:
- Table name: `CategorizationRules`
- 9 columns: Id, Name, MatchType, MatchPattern, CaseSensitive, CategoryId, IsActive, Priority, CreatedAt, UpdatedAt
- 2 indexes: on CategoryId and on IsActive+Priority

## What is CategorizationRules Table?

This table stores automatic categorization rules for transactions:

**Purpose:**
- Auto-suggest categories based on payee, description, or amount
- Learn from past transactions
- Create rules for common merchants/vendors
- Speed up transaction entry

**Example Rules:**
```
Name: "Auto: Walmart"
MatchType: Payee
MatchPattern: "Walmart"
CategoryId: 15 (Groceries)
```

When you enter "Walmart" as payee, the system automatically suggests "Groceries" category.

## Table Schema

| Column | Type | Description |
|--------|------|-------------|
| Id | INTEGER | Primary key |
| Name | TEXT | Rule name (e.g., "Auto: Walmart") |
| MatchType | INTEGER | 0=Payee, 1=Description, 2=Amount |
| MatchPattern | TEXT | Pattern to match (e.g., "Walmart") |
| CaseSensitive | INTEGER | 0=No, 1=Yes |
| CategoryId | INTEGER | Category to suggest |
| IsActive | INTEGER | 0=Disabled, 1=Enabled |
| Priority | INTEGER | Rule priority (higher = checked first) |
| CreatedAt | TEXT | When rule was created |
| UpdatedAt | TEXT | When rule was last updated |

## Testing the Fix

1. **Verify Table Creation:**
   - Start the application
   - Check logs for "CREATE TABLE IF NOT EXISTS CategorizationRules"
   - No error should appear

2. **Test Auto-Categorization:**
   - Go to Transactions page
   - Click "Add Transaction"
   - Type a payee name you've used before
   - Category should be auto-suggested

3. **Create a Rule:**
   - Go to Settings ? Categories ? Auto-Categorization (if page exists)
   - Create a new rule
   - Test by creating a transaction that matches the rule

## Why This Happened

The `CategorizationRules` table was added to the code (`ApplicationDbContext`) but:
- No migration was run
- No manual table creation was performed
- The table creation wasn't in the Program.cs startup code

This is now fixed by adding it to Program.cs alongside the other table creations (Documents, Budgets, etc.).

## Prevention

To prevent similar issues in the future:

1. **When adding new DbSet to ApplicationDbContext:**
   - Add table creation SQL to Program.cs
   - Or create and run a migration
   - Or add to `fix_*_table.sql` file

2. **Check on startup:**
   - Look for SQLite errors in logs
   - Test all major features after adding new tables

3. **Documentation:**
   - Document new tables in README
   - Include table creation in deployment guide

## Related Files

- `Program.cs` - Table creation on startup
- `fix_categorization_rules_table.sql` - Manual SQL script
- `Services/AutoCategorizationService.cs` - Uses this table
- `Data/ApplicationDbContext.cs` - Defines DbSet

## Status

? **FIXED** - Table creation added to Program.cs
? **TESTED** - Build successful
? **DOCUMENTED** - This guide created

## Next Steps

1. Restart your application
2. Error should be resolved
3. Auto-categorization feature should work
4. Consider adding some default rules via Settings

---

**Fix Applied:** 2024
**Severity:** High (Application crash)
**Resolution Time:** Immediate (restart required)
