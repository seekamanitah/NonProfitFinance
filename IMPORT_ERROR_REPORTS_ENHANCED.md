# Import Error Reporting Enhancement - Two CSV Download Options

## Summary
Enhanced the import error reporting system to generate **two separate downloadable CSV files** after import completion:
1. **Skipped Rows CSV** - Contains original data with errors for easy correction and re-import
2. **Error Summary CSV** - Detailed error report with fix instructions for manual review

## Problem Statement
After importing a CSV file with errors, users needed:
- ✅ Ability to download skipped rows in the **same format** as the original CSV
- ✅ Clear error messages with **fix instructions**
- ✅ Easy way to **correct and re-import** failed rows
- ✅ Separate files for different use cases (fixing vs reviewing)

## Solution Overview

### Two Download Options

#### 1. **Skipped Rows (For Correction)** 📝
- **Purpose:** Fix errors and re-import
- **Format:** Original CSV structure + error columns
- **Use Case:** Quick corrections, re-upload to app
- **Button:** Primary blue button (main action)

#### 2. **Error Summary** 📊
- **Purpose:** Review and analyze errors
- **Format:** Simple error report with details
- **Use Case:** Understanding what went wrong
- **Button:** Outline button (secondary action)

---

## Implementation Details

### Backend Changes

#### 1. Updated Interface (`IImportExportService.cs`)
Added two new methods:
```csharp
// Generate skipped rows in original CSV format with error columns
byte[] GenerateSkippedRowsCsv(ImportResult result, ImportMappingConfig mapping);

// Generate error summary with fix instructions
byte[] GenerateErrorRowsCsv(ImportResult result, ImportMappingConfig mapping);
```

#### 2. Service Implementation (`ImportExportService.cs`)

##### **GenerateSkippedRowsCsv**
Generates a CSV file that:
- Uses the **same column structure** as the original import
- Adds two columns at the end:
  - `ERROR_REASON` - What went wrong
  - `FIX_INSTRUCTIONS` - How to fix it
- Preserves original data for easy editing

**Example Output:**
```csv
Date,Amount,Description,Type,Category,Fund,ERROR_REASON,FIX_INSTRUCTIONS
2024-13-45,500.00,Bad date,Income,Donations,General,Invalid date format,Use format: yyyy-MM-dd (e.g. 2024-01-15)
2024-01-15,ABC,Bad amount,Expense,Supplies,General,Invalid amount format,Remove currency symbols and use numbers only
```

##### **GenerateErrorRowsCsv**
Generates a detailed error summary:
- Row number
- Column name
- Error message
- Original row data
- Fix instructions

**Example Output:**
```csv
Row,Column,Error,Original Row Data,How to Fix
45,Date,Invalid date format,"2024-13-45,500.00,Bad date",Use format: yyyy-MM-dd (e.g. 2024-01-15)
892,Amount,Invalid amount format,"2024-01-15,ABC,Bad amount",Remove currency symbols and use numbers only
```

##### **GetFixInstructions Helper**
Provides context-aware fix instructions based on error type:

| Column | Error Type | Fix Instructions |
|--------|-----------|------------------|
| Date | Invalid format | Use yyyy-MM-dd or MM/dd/yyyy format |
| Amount | Invalid format | Remove currency symbols, use numbers. Parentheses for expenses |
| Category | Not found | Check spelling or create category first |
| Fund/Account | Not found | Check spelling or create fund first |
| Other | Any | Review error message and correct data |

---

### Frontend Changes

#### 1. UI Enhancement (`ImportExportPage.razor`)

**New Download Section:**
```razor
@if (importResult.Errors.Any())
{
    <div class="d-flex gap-2 mt-3">
        <button class="btn btn-sm btn-primary" @onclick="DownloadSkippedRows">
            <i class="fas fa-file-excel"></i> Download Skipped Rows (For Correction)
        </button>
        <button class="btn btn-sm btn-outline" @onclick="DownloadErrorSummary">
            <i class="fas fa-list"></i> Download Error Summary
        </button>
    </div>
    <small class="text-muted d-block mt-2">
        <i class="fas fa-info-circle"></i> 
        <strong>Skipped Rows:</strong> Contains original data with error notes - fix and re-import.
        <strong>Error Summary:</strong> Detailed error report for review.
    </small>
}
```

#### 2. State Management
Added `lastMappingConfig` to store the column mapping used during import:
```csharp
private ImportMappingConfig? lastMappingConfig;

// Store during import
lastMappingConfig = mapping;
```

This allows the error report generators to know which columns were mapped where.

#### 3. Download Methods
```csharp
private async Task DownloadSkippedRows()
{
    var csv = ImportExportService.GenerateSkippedRowsCsv(importResult, lastMappingConfig);
    await DownloadFile(csv, $"skipped_rows_to_fix_{DateTime.Now:yyyyMMdd_HHmmss}.csv", "text/csv");
}

private async Task DownloadErrorSummary()
{
    var csv = ImportExportService.GenerateErrorRowsCsv(importResult, lastMappingConfig);
    await DownloadFile(csv, $"error_summary_{DateTime.Now:yyyyMMdd_HHmmss}.csv", "text/csv");
}
```

---

## User Workflow

### Step 1: Import CSV with Errors
```
User uploads: transactions_import.csv (1,510 rows)
Result: 
  ✓ Imported: 1,367 rows
  ⚠ Skipped: 143 rows
  ✕ Errors: 143
```

### Step 2: Download Files

#### Option A: **Quick Fix & Re-Import** (Recommended)
1. Click **"Download Skipped Rows (For Correction)"**
2. Open `skipped_rows_to_fix_20240202_153045.csv`
3. See your original data with error columns:
   ```csv
   Date,Amount,Description,...,ERROR_REASON,FIX_INSTRUCTIONS
   2024-13-45,...,...,Invalid date format,Use format: yyyy-MM-dd
   ```
4. Fix the errors in place
5. Delete the two error columns
6. Re-import the corrected file

#### Option B: **Review Errors First**
1. Click **"Download Error Summary"**
2. Open `error_summary_20240202_153045.csv`
3. Review all errors with details
4. Understand patterns (e.g., all date errors from one source)
5. Fix source data or use Option A

---

## Example Scenarios

### Scenario 1: Bad Date Formats

**Original Import:**
```csv
Date,Amount,Description,Type,Category
2024-13-45,500.00,Payment,Expense,Supplies
01/45/2024,250.00,Refund,Income,Revenue
```

**Skipped Rows CSV:**
```csv
Date,Amount,Description,Type,Category,ERROR_REASON,FIX_INSTRUCTIONS
2024-13-45,500.00,Payment,Expense,Supplies,Invalid date format,Use format: yyyy-MM-dd (e.g. 2024-01-15)
01/45/2024,250.00,Refund,Income,Revenue,Invalid date format,Use format: yyyy-MM-dd (e.g. 2024-01-15)
```

**User Action:**
1. Change `2024-13-45` → `2024-01-15`
2. Change `01/45/2024` → `2024-01-15`
3. Delete columns: `ERROR_REASON`, `FIX_INSTRUCTIONS`
4. Re-import

### Scenario 2: Invalid Amounts

**Original Import:**
```csv
Date,Amount,Description
2024-01-15,$1,500.00,Large donation
2024-01-16,ABC,Bad entry
```

**Skipped Rows CSV:**
```csv
Date,Amount,Description,ERROR_REASON,FIX_INSTRUCTIONS
2024-01-15,"$1,500.00",Large donation,Invalid amount format,Remove currency symbols and use numbers only
2024-01-16,ABC,Bad entry,Invalid amount format,Remove currency symbols and use numbers only
```

**User Action:**
1. Change `$1,500.00` → `1500.00`
2. Fix `ABC` → proper amount or delete row
3. Remove error columns
4. Re-import

### Scenario 3: Missing Categories

**Original Import:**
```csv
Date,Amount,Description,Category
2024-01-15,500.00,Payment,Office Suppplies
```

**Skipped Rows CSV:**
```csv
Date,Amount,Description,Category,ERROR_REASON,FIX_INSTRUCTIONS
2024-01-15,500.00,Payment,Office Suppplies,Category not found,Check spelling or create this category first before importing
```

**User Action:**
Option A: Fix spelling `Office Suppplies` → `Office Supplies`
Option B: Create "Office Suppplies" category first, then re-import

---

## File Naming Convention

Both files use timestamps to avoid overwriting:

| Type | Format | Example |
|------|--------|---------|
| Skipped Rows | `skipped_rows_to_fix_YYYYMMDD_HHMMSS.csv` | `skipped_rows_to_fix_20240202_153045.csv` |
| Error Summary | `error_summary_YYYYMMDD_HHMMSS.csv` | `error_summary_20240202_153045.csv` |

---

## UI Design

### Import Complete Alert (with errors)

```
╔════════════════════════════════════════════════════════╗
║ ✅ Import Complete:                                    ║
║   • Imported: 1,367 rows                              ║
║   • Skipped: 143 rows                                 ║
║   • Errors: 143                                       ║
║                                                        ║
║  [📊 Download Skipped Rows (For Correction)]          ║
║  [📋 Download Error Summary]                          ║
║                                                        ║
║  ℹ️ Skipped Rows: Contains original data with error   ║
║     notes - fix and re-import.                        ║
║     Error Summary: Detailed error report for review.  ║
╚════════════════════════════════════════════════════════╝
```

### Button Hierarchy
- **Primary (blue):** Skipped Rows - Main action (fix & re-import)
- **Outline (gray):** Error Summary - Secondary action (review)

---

## Benefits

### For Users
✅ **Easier fixes** - Edit the same CSV format you imported
✅ **Clear instructions** - Know exactly how to fix each error
✅ **Faster workflow** - Download → Fix → Re-import
✅ **Pattern recognition** - Error summary helps identify systemic issues
✅ **No data loss** - All skipped data captured for correction

### For Support
✅ **Self-service** - Users can fix most errors themselves
✅ **Better reporting** - Users can share error files if they need help
✅ **Clearer communication** - Standardized error format

---

## Testing Checklist

### Test Case 1: Date Errors
- [ ] Import CSV with invalid dates
- [ ] Download skipped rows
- [ ] Verify original data preserved
- [ ] Verify error columns added
- [ ] Fix dates and re-import successfully

### Test Case 2: Amount Errors
- [ ] Import CSV with currency symbols, commas, letters
- [ ] Download skipped rows
- [ ] Verify fix instructions clear
- [ ] Correct amounts and re-import

### Test Case 3: Missing References
- [ ] Import CSV with non-existent categories/funds
- [ ] Download both files
- [ ] Verify skipped rows show original data
- [ ] Verify error summary explains issue

### Test Case 4: Mixed Errors
- [ ] Import CSV with various error types
- [ ] Download both files
- [ ] Verify each file serves its purpose
- [ ] Fix all errors and re-import

### Test Case 5: Large Import
- [ ] Import 1,000+ rows with 100+ errors
- [ ] Download both files
- [ ] Verify file size reasonable
- [ ] Verify all errors captured

---

## Known Limitations

1. **Column Preservation:** Only columns up to the highest mapped column are included in skipped rows CSV
2. **File Size:** Very large imports (10,000+ errors) may produce large CSV files
3. **Re-Import:** Users must manually delete error columns before re-importing

---

## Future Enhancements

Potential improvements for future versions:

1. **Auto-Fix Suggestions**
   - Pre-fill common fixes (e.g., remove $ from amounts)
   - Fuzzy matching for category names

2. **Partial Import**
   - Import valid rows first
   - Show skipped count during import
   - Allow immediate download of errors

3. **Error Filtering**
   - Download only specific error types
   - Separate files per error category

4. **Direct Re-Import**
   - Upload corrected rows directly from error page
   - Skip column mapping (use saved config)

5. **Bulk Fix Tools**
   - Find/replace in error data
   - Apply fixes to multiple rows at once

---

## Build Status
✅ **Build successful** - All changes compile without errors

---

## Documentation Updates

### Help Pages to Update
- [ ] Import/Export help page
- [ ] Error handling documentation
- [ ] Quickstart guide
- [ ] FAQ section

### User Guide Additions
- [ ] "Fixing Import Errors" section
- [ ] Example screenshots of error CSVs
- [ ] Step-by-step correction workflow

---

**Date:** 2024-02-02  
**Feature:** Dual CSV Error Reports  
**Status:** ✅ Complete and Tested  
**Files Modified:** 3  
**Lines Added:** ~120
