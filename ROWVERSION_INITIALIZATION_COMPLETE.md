# Import RowVersion Errors - Complete Fix

## Problem
During CSV import, multiple database constraint errors were occurring:
- `SQLite Error 19: 'NOT NULL constraint failed: Transactions.RowVersion'`
- `SQLite Error 19: 'NOT NULL constraint failed: Funds.RowVersion'` 
- `SQLite Error 19: 'NOT NULL constraint failed: Grants.RowVersion'`

These errors occurred because entities with `RowVersion` (concurrency tokens) were not initializing this required field when created.

## Root Cause
Several services create entities that have `RowVersion` as a NOT NULL column, but weren't initializing it:
1. **TransactionService** - Creates Transaction entities during import
2. **FundService** - Creates Fund entities through form UI and import
3. **GrantService** - Creates Grant entities through form UI
4. **ImportExportService** - Auto-creates Funds during CSV import (already fixed in previous session)

## Solution Implemented

### Files Modified

#### 1. Services/TransactionService.cs
**Location:** CreateAsync method (lines ~200-225)
- Added `RowVersion = 1` to Transaction object initialization
- Applied to both regular transactions AND transfer transactions (lines ~330-355)
- Ensures all Transaction objects have the required concurrency token

```csharp
var transaction = new Transaction
{
    Date = request.Date,
    Amount = request.Amount,
    // ... other properties ...
    RowVersion = 1  // Initialize concurrency token
};
```

#### 2. Services/FundService.cs
**Location:** CreateAsync method (lines ~36-55)
- Added `RowVersion = 1` to Fund object initialization
- Ensures Fund entities created through the service have the token

```csharp
var fund = new Fund
{
    Name = request.Name,
    // ... other properties ...
    RowVersion = 1  // Initialize concurrency token
};
```

#### 3. Services/GrantService.cs
**Location:** CreateAsync method (lines ~38-63)
- Added `RowVersion = 1` to Grant object initialization
- Ensures Grant entities created through the service have the token

```csharp
var grant = new Grant
{
    Name = request.Name,
    // ... other properties ...
    RowVersion = 1  // Initialize concurrency token
};
```

#### 4. Services/ImportExportService.cs
**Previously Fixed**
- Already initialized `RowVersion = 1` for Funds created during import

## Why RowVersion is Required

The `RowVersion` property is a concurrency token used for optimistic locking in Entity Framework Core:
- Prevents lost updates when multiple users modify the same entity
- Automatically incremented on each update
- Required at database level (NOT NULL constraint)
- Must be initialized to at least 1 for new entities

## Affected Entities

| Entity | HasRowVersion | Fixed | Location |
|--------|---------------|-------|----------|
| Transaction | ✅ Yes | ✅ Yes | TransactionService.cs |
| Fund | ✅ Yes | ✅ Yes | FundService.cs |
| Grant | ✅ Yes | ✅ Yes | GrantService.cs |
| Category | ❌ No | N/A | CategoryService.cs |
| Donor | ❌ No | N/A | DonorService.cs |
| TransactionSplit | ❌ No | N/A | TransactionService.cs |

## Testing the Fix

### 1. CSV Import Test
```
1. Go to Import/Export page
2. Select a CSV file with transaction data
3. Map columns appropriately
4. Click Import
5. Verify: No RowVersion constraint errors
```

### 2. Manual Entity Creation Tests
```
# Test Transaction Creation
- Create new transaction via form
- Verify it saves without RowVersion errors

# Test Fund Creation  
- Create new fund via form
- Verify it saves without RowVersion errors

# Test Grant Creation
- Create new grant via form
- Verify it saves without RowVersion errors
```

### 3. Database Verification
```sql
-- Verify RowVersion values are set
SELECT Id, RowVersion FROM Transactions LIMIT 5;
SELECT Id, RowVersion FROM Funds LIMIT 5;
SELECT Id, RowVersion FROM Grants LIMIT 5;

-- All should show RowVersion >= 1
```

## Verification Steps

1. **Build Success**: ✅ Complete - No compilation errors
2. **Import Test**: Try importing a CSV file
3. **Create Operations**: Test creating transactions, funds, and grants
4. **Database Check**: Verify RowVersion is set for all entities

## Related Issues Fixed

This fix complements the previous RowVersion initialization fixes:
- Fund import fix (ImportExportService)
- Donor creation fix (auto-created without RowVersion in Category creation)
- Theme toggle functionality

## Notes

- `RowVersion = 1` is the standard initialization value
- EF Core automatically increments on updates
- This is a database-level requirement, not just an application-level concern
- All services follow the same pattern for consistency

## Files Summary

```
Services/TransactionService.cs  - 2 locations (regular + transfer transactions)
Services/FundService.cs         - 1 location (CreateAsync)
Services/GrantService.cs        - 1 location (CreateAsync)
Services/ImportExportService.cs - Previously fixed (Fund creation during import)
```

## Build Status
✅ Build successful - No errors or warnings related to this change
