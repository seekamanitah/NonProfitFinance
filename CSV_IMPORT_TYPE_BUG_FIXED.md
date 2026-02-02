# 🔧 CSV Import/Export Transaction Type Bug - FIXED!

## ❌ **The Problem**

When exporting transactions to CSV and then re-importing them:
- **All transactions showed as "Income"**
- **Expenses were not preserved**
- **Transfer types were lost**

### Root Cause

The CSV export includes a "Type" column (column 3) with values:
- `Income`
- `Expense`  
- `Transfer`

But the import page was **NOT reading the Type column**! It was passing `null` for the TypeColumn parameter, causing the import logic to fall back to:

```csharp
type = amount >= 0 ? TransactionType.Income : TransactionType.Expense;
```

Since all exported amounts are positive (no negative signs), everything defaulted to Income!

---

## ✅ **The Fix**

### Changes Made:

1. **Added Type Column Input Field** (ImportExportPage.razor)
   - New UI field: "Type Column (optional)"
   - Default value: `3` (matches export format)
   - Helper text: "Column with Income/Expense/Transfer"

2. **Updated Default Column Mappings**
   - `typeColumn = 3` (Type column in exported CSV)
   - `categoryColumn = 4` (Category column in exported CSV)

3. **Fixed Import Mapping**
   - PreviewImport now passes `typeColumn` to ImportMappingConfig
   - ExecuteImport now passes `typeColumn` to ImportMappingConfig

---

## 📊 CSV Format (Exported)

```
Date,Amount,Description,Type,Category,Fund,Donor,Grant,Payee,Tags,Reference,Reconciled
   0      1           2    3        4    5     6     7     8    9         10        11
```

**Column 3 = Type** ← This was being ignored!

---

## 🎯 How It Works Now

### Before (Broken):
```csharp
// Import ignored Type column
var mapping = new ImportMappingConfig(
    dateColumn: 0,
    amountColumn: 1,
    descriptionColumn: 2,
    categoryColumn: null,
    fundColumn: null,
    donorColumn: null,
    grantColumn: null,
    typeColumn: null,  // ❌ Always null!
    //...
);

// Result: All transactions default to Income (amount >= 0)
```

### After (Fixed):
```csharp
// Import reads Type column
var mapping = new ImportMappingConfig(
    dateColumn: 0,
    amountColumn: 1,
    descriptionColumn: 2,
    categoryColumn: 4,
    fundColumn: null,
    donorColumn: null,
    grantColumn: null,
    typeColumn: 3,  // ✅ Reads column 3 (Type)
    //...
);

// Result: Types preserved correctly!
```

---

## 🔍 Import Type Parsing Logic

The import now correctly reads the Type column:

```csharp
if (mapping.TypeColumn.HasValue && mapping.TypeColumn.Value < columns.Length)
{
    var typeStr = columns[mapping.TypeColumn.Value].ToLower();
    type = typeStr switch
    {
        "income" or "deposit" or "credit" => TransactionType.Income,
        "expense" or "withdrawal" or "debit" => TransactionType.Expense,
        "transfer" => TransactionType.Transfer,
        _ => amount >= 0 ? TransactionType.Income : TransactionType.Expense
    };
}
```

---

## ✅ Testing

### Test Case 1: Export & Re-import
1. Export transactions with mixed types (Income, Expense, Transfer)
2. Import the exported CSV
3. **Expected**: All transaction types preserved correctly ✅

### Test Case 2: Manual CSV
Create a CSV with:
```
Date,Amount,Description,Type,Category
2026-01-01,100.00,Test Income,Income,Donations
2026-01-02,50.00,Test Expense,Expense,Office Supplies
2026-01-03,200.00,Test Transfer,Transfer,General Fund
```

Import with:
- Date Column: 0
- Amount Column: 1
- Description Column: 2
- Type Column: 3
- Category Column: 4

**Expected**: All 3 types import correctly ✅

---

## 📝 User-Visible Changes

### Import Page UI:

**New Field Added:**
```
Type Column (optional)  [   3    ]
Column with Income/Expense/Transfer
```

**Updated Default Values:**
- Type Column: `3` (auto-filled)
- Category Column: `4` (auto-filled)

This matches the standard export format, so re-importing exported CSVs now works out-of-the-box!

---

## 🚀 Deployment

**Files Changed:**
- `Components/Pages/ImportExport/ImportExportPage.razor`

**Commit Message:**
```
fix: CSV import now correctly reads Transaction Type column

- Added Type Column mapping field with default value of 3
- Fixed import to pass typeColumn to ImportMappingConfig
- Updated Category Column default to 4 (matches export format)
- Resolves issue where all re-imported transactions showed as Income
- Export format: Date(0), Amount(1), Description(2), Type(3), Category(4)
```

---

## ✅ Status

**Fixed**: ✅  
**Tested**: ✅  
**Ready to Deploy**: ✅

### Expected Behavior After Fix:

| Scenario | Before Fix | After Fix |
|----------|-----------|-----------|
| Export 10 Income, 5 Expense | ✅ Exports correctly | ✅ Exports correctly |
| Re-import exported CSV | ❌ All show as Income | ✅ Types preserved! |
| Import custom CSV with Type column | ❌ Types ignored | ✅ Types read correctly |
| Import CSV without Type column | ✅ Defaults based on amount | ✅ Still works (fallback) |

---

**Bug Severity**: High (Data integrity issue)  
**Fix Complexity**: Low (UI + parameter passing)  
**Breaking Changes**: None (backward compatible)
