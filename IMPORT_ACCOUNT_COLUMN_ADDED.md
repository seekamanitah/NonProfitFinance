# Import Account Column Selector Added

## Summary
Added Account (Fund) column selector to the CSV import settings, allowing users to map which column in their CSV contains fund/account information.

## Changes Made

### 1. UI Updates - `ImportExportPage.razor`
Added three new column selectors in the import mapping section:

**New Column Selectors:**
- **Account Column** (Fund) - Maps to fund/account name
- **Donor Column** - Maps to donor name (for income transactions)
- **Payee Column** - Maps to payee/vendor name

**UI Layout:**
```
Row 1:
- Date Column (required)
- Amount Column (required)
- Description Column (required)

Row 2:
- Type Column (optional) - Income/Expense/Transfer
- Category Column (optional) - Category name
- Account Column (optional) - Fund/Account name ⭐ NEW

Row 3:
- Donor Column (optional) - Donor name ⭐ NEW
- Payee Column (optional) - Payee/vendor name ⭐ NEW
- Date Format - Format dropdown
```

### 2. State Variables Added
```csharp
private int? fundColumn = 5;      // Default to column 5 (Fund in exported CSV)
private int? donorColumn = null;  // Donor column (optional)
private int? payeeColumn = null;  // Payee column (optional)
```

### 3. Mapping Configuration Updated
Both `PreviewImport` and `ExecuteImport` now pass the new columns:
```csharp
var mapping = new ImportMappingConfig(
    dateColumn, amountColumn, descriptionColumn,
    categoryColumn, fundColumn, donorColumn, null, typeColumn,
    payeeColumn, null, hasHeaderRow, dateFormat);
```

## Backend Support

The backend already fully supports these columns:
- ✅ `FundColumn` - Matches fund by name, links transaction to account
- ✅ `DonorColumn` - Matches donor by name, auto-creates if missing
- ✅ `PayeeColumn` - Sets payee field on transaction

## CSV Template

The import template already includes all these columns:
```csv
Date,Amount,Description,Type,Category,Fund,Donor,Grant ID,Payee,Tags
2024-01-15,500.00,Monthly donation,Income,Individual/Small Business,General Operating Fund,John Smith,,Donor,,
2024-01-16,-150.00,Office supplies,Expense,Office Supplies,General Operating Fund,,,Office Depot,supplies
```

## Column Mapping Reference

| Column Index | Field | Required | Description |
|--------------|-------|----------|-------------|
| 0 | Date | ✅ Yes | Transaction date |
| 1 | Amount | ✅ Yes | Amount (supports parentheses/+/-) |
| 2 | Description | ✅ Yes | Transaction description |
| 3 | Type | ⚪ Optional | Income/Expense/Transfer |
| 4 | Category | ⚪ Optional | Category name (auto-creates) |
| 5 | **Fund** | ⚪ Optional | **Account/Fund name** ⭐ |
| 6 | **Donor** | ⚪ Optional | **Donor name** ⭐ |
| 7 | Grant ID | ⚪ Optional | Grant reference |
| 8 | **Payee** | ⚪ Optional | **Payee/vendor name** ⭐ |
| 9 | Tags | ⚪ Optional | Comma-separated tags |

## Usage Example

### Import File with Account Column
```csv
Date,Amount,Description,Type,Category,Fund,Donor,Payee,Tags
2024-01-15,1000.00,Grant payment,Income,Grants,Restricted Equipment Fund,City Fire Dept,,grant,equipment
2024-01-16,-500.00,New turnout gear,Expense,Equipment,Restricted Equipment Fund,,FireGear Inc,gear,ppe
2024-01-17,250.00,Donation,Income,Donations,General Operating Fund,John Smith,,donation,monthly
```

### Column Mapping Settings
- Date Column: `0`
- Amount Column: `1`
- Description Column: `2`
- Type Column: `3`
- Category Column: `4`
- **Account Column: `5`** ⭐
- **Donor Column: `6`** ⭐
- **Payee Column: `7`** ⭐
- Tags Column: `8`

### What Happens During Import

1. **Account (Fund) Column:**
   - Looks up fund by name (case-insensitive)
   - If found: Links transaction to that fund
   - If not found: Transaction uses default/no fund
   - Fund balance updated automatically

2. **Donor Column:**
   - Looks up donor by name (case-insensitive)
   - If found: Links transaction to existing donor
   - If not found: **Auto-creates new donor** with that name
   - Updates donor contribution totals

3. **Payee Column:**
   - Sets the `Payee` field on the transaction
   - Used for checks, vendors, or who received/paid money
   - Available for search/filtering later

## Benefits

✅ **More accurate imports** - Transactions link to correct accounts
✅ **Fund accounting** - Track restricted vs unrestricted automatically
✅ **Auto-donor creation** - Import donations with donor info
✅ **Better tracking** - Payee field for vendor/recipient info
✅ **Backwards compatible** - All optional, works with old CSVs

## Testing

To test the new feature:

1. Navigate to **Settings > Import/Export**
2. Upload a CSV with fund/donor/payee columns
3. Set column numbers for Account, Donor, and Payee
4. Click **Preview** to verify mapping
5. Click **Import** to import with account linkage

**Sample Test CSV:**
```csv
Date,Amount,Description,Type,Category,Account,Donor
2024-01-15,1000.00,Monthly grant,Income,Grants,Equipment Fund,
2024-01-16,500.00,Donation,Income,Donations,General Fund,Jane Doe
2024-01-17,-250.00,Office supplies,Expense,Supplies,General Fund,
```

## Build Status
✅ **Build successful** - All changes compile without errors

---

**Date:** 2024-02-02
**Feature:** Import Account Column Selector
**Status:** ✅ Complete
