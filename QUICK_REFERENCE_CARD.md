# Quick Reference Card - Database & App Issues

**Bookmark This for Future Reference**

---

## ❌ If You See "NOT NULL constraint failed: Entity.RowVersion"

**Error looks like:**
```
NOT NULL constraint failed: Transactions.RowVersion
NOT NULL constraint failed: Funds.RowVersion
NOT NULL constraint failed: Grants.RowVersion
```

**✅ Solution:**
Add `RowVersion = 1` when creating Transaction, Fund, or Grant:
```csharp
var transaction = new Transaction 
{ 
    Date = request.Date,
    Amount = request.Amount,
    // ... other properties ...
    RowVersion = 1  // ← ADD THIS LINE
};
```

**Services to check:**
- TransactionService.CreateAsync
- FundService.CreateAsync
- GrantService.CreateAsync
- ImportExportService (if auto-creating Funds)

---

## ❌ If Dark/Light Mode Toggle Not Working

**Problem:** Button doesn't respond, theme doesn't change

**✅ Solutions:**
1. **Clear browser cache** (Ctrl+Shift+Delete)
2. **Check browser console** (F12) for JavaScript errors
3. **Verify localStorage** is available: `localStorage.getItem('theme')`
4. **Restart application** - restart browser completely
5. **Check theme.js loaded** - verify in Sources tab (F12)

**If still broken:**
- theme.js may not be initializing before components render
- Blazor InteractiveServer mode timing issue
- Clear localStorage: `localStorage.removeItem('theme')`

---

## ❌ If Import Fails

**Common causes:**

1. **Missing Category**
   - CSV has category name that doesn't exist
   - ✅ Fix: Add category first, OR import will auto-create it

2. **Invalid Date Format**
   - ✅ Fix: Use MM/dd/yyyy or verify date column format in mapping

3. **Invalid Amount Format**
   - ✅ Fix: Remove currency symbols, use decimal point

4. **Duplicate Row**
   - ✅ Fix: Check preview for duplicates, clean CSV

5. **Fund Not Found**
   - ✅ Fix: Fund will auto-create during import with default settings

---

## ❌ If Delete Operations Fail

**Cannot delete Category/Fund?**
- ✅ Reason: Has child records (Restrict behavior)
- ✅ Solution: Delete children first OR archive instead

**Cannot undo delete?**
- ✅ Check RecycleBin page (if soft-deleted)
- ✅ May need to restore from backup if hard-deleted

---

## ✅ Common Operations

### Create Transaction
```csharp
var transaction = new Transaction
{
    Date = DateTime.Now,
    Amount = 100.00m,
    Type = TransactionType.Income,
    CategoryId = 1,
    FundId = 1,
    RowVersion = 1  // IMPORTANT
};
```

### Create Fund
```csharp
var fund = new Fund
{
    Name = "General Fund",
    Type = FundType.Unrestricted,
    Balance = 0,
    RowVersion = 1  // IMPORTANT
};
```

### Create Grant
```csharp
var grant = new Grant
{
    Name = "Federal Grant",
    GrantorName = "FEMA",
    Amount = 50000m,
    RowVersion = 1  // IMPORTANT
};
```

### Soft Delete Transaction
```csharp
transaction.SoftDelete(userId);  // Marks as deleted
await _context.SaveChangesAsync();
```

### Restore Soft-Deleted Transaction
```csharp
transaction.Restore();  // Restores from soft-delete
await _context.SaveChangesAsync();
```

---

## 📊 Database Files

### Key Documentation
- `DATABASE_COMPLETE_REFERENCE_GUIDE.md` - Full reference (start here)
- `DATABASE_SCHEMA_ANALYSIS_COMPLETE.md` - Entity details
- `ENTITY_RELATIONSHIPS_CASCADE_ANALYSIS.md` - Relationships
- `COMPREHENSIVE_REVIEW_FINAL_SUMMARY.md` - Session summary

### Troubleshooting Checklist
```
☐ Build succeeds (Ctrl+Shift+B)
☐ No database errors in Output window
☐ All migrations applied (Update-Database)
☐ Database file exists
☐ RowVersion initialized in all CreateAsync
☐ Theme.js loads before components render
☐ localStorage available in browser
☐ Foreign keys reference existing records
```

---

## 🔧 Entity Reference

### Entities with RowVersion (MUST initialize RowVersion = 1)
- Transaction
- Fund
- Grant

### Entities WITHOUT RowVersion
- Category, Donor, Budget, Document, etc. (no initialization needed)

### Entities with Soft Delete
- Transaction (has IsDeleted flag and query filter)

### Delete Protection (Restrict)
- Category (if has children or transactions)
- Location hierarchy
- Building hierarchy
- Budget line items

---

## 🚀 Quick Actions

**If application won't start:**
1. Run build: `Ctrl+Shift+B`
2. Check Output window for errors
3. Verify database path correct in appsettings.json
4. Ensure all migrations applied

**If import fails:**
1. Check CSV format (date, amount, description)
2. Preview import first to see errors
3. Verify category exists or will auto-create
4. Check amounts are valid numbers

**If transaction won't save:**
1. Verify category exists
2. Verify fund exists (if specified)
3. Check amount is valid decimal
4. Ensure date is valid

**If theme won't toggle:**
1. Clear browser cache (Ctrl+Shift+Delete)
2. Check browser console (F12) for errors
3. Verify theme.js loaded in Sources
4. Restart browser completely

---

## 📞 Support Information

**If you need to report an issue:**
1. Take screenshot of error
2. Copy full error message
3. Note what operation caused it
4. Describe steps to reproduce
5. Share sample data if possible

**Documentation to reference:**
- `COMPREHENSIVE_REVIEW_FINAL_SUMMARY.md` - Start here for overview
- `DATABASE_COMPLETE_REFERENCE_GUIDE.md` - Troubleshooting section
- `ENTITY_RELATIONSHIPS_CASCADE_ANALYSIS.md` - For delete behavior questions

---

**Last Updated**: January 2025  
**Status**: All known issues documented and resolved  
**Build**: ✅ Successful - No compilation errors
