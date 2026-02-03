# Complete Database Schema Analysis

**Generated**: January 2025  
**Status**: Comprehensive Review Complete  
**Found Issues**: None - All required entities have proper initialization

---

## Executive Summary

Review of `ApplicationDbContext.cs` reveals a well-structured database with **50+ entities** across 4 modules:
- **Financial Module**: Categories, Transactions, Funds, Donors, Grants, Documents, etc.
- **Inventory Module**: Items, Categories, Locations, Transactions
- **Maintenance Module**: Buildings, Projects, Work Orders, Service Requests
- **Shared Module**: Custom Fields, User Module Access, Audit Logs

### Critical Findings

✅ **All RowVersion-dependent entities are properly initialized in their services**
✅ **Soft-delete filter correctly configured for Transaction**
✅ **Entity relationships well-defined with appropriate OnDelete behaviors**
✅ **Performance indexes properly configured**

---

## Entities with RowVersion (Concurrency Token)

Only **3 entities** have RowVersion property for optimistic locking:

### 1. **Transaction**
- **DbContext Config** (Line 130-146):
  ```csharp
  entity.Property(t => t.RowVersion)
      .HasDefaultValue(0u)
      .IsConcurrencyToken();
  entity.HasQueryFilter(t => !t.IsDeleted);  // Soft delete filter
  ```
- **Service Initialization**: ✅ **TransactionService.CreateAsync** (initialized to 1)
- **Also Initialized In**: 
  - TransactionService.CreateTransferAsync (both income and expense sides)
  - ImportExportService (during import)
- **Soft Delete**: ✅ Transaction implements ISoftDelete interface

### 2. **Fund**
- **DbContext Config** (Line 168-175):
  ```csharp
  entity.Property(f => f.RowVersion)
      .HasDefaultValue(0u)
      .IsConcurrencyToken();
  ```
- **Service Initialization**: ✅ **FundService.CreateAsync** (initialized to 1)
- **Also Initialized In**:
  - ImportExportService (auto-create during import)

### 3. **Grant**
- **DbContext Config** (Line 207-213):
  ```csharp
  entity.Property(g => g.RowVersion)
      .HasDefaultValue(0u)
      .IsConcurrencyToken();
  ```
- **Service Initialization**: ✅ **GrantService.CreateAsync** (initialized to 1)

---

## Entities WITHOUT RowVersion

### Category
- **DbContext Config** (Line 73-87): No RowVersion property
- **Service**: CategoryService - Does NOT need RowVersion initialization
- **Note**: Self-referencing hierarchy with ParentId

### Donor
- **DbContext Config** (Line 176-197): No RowVersion property
- **Service**: DonorService - Does NOT need RowVersion initialization

### All Other Entities
All remaining entities (50+ others) do NOT have RowVersion property and do NOT need concurrency token initialization.

---

## Entity Relationships Summary

### Critical Foreign Keys with OnDelete Behaviors

**Restrict** (prevents deletion if child records exist):
- Category -> Parent (Category)
- Transaction -> Category
- Transaction -> TransactionSplit
- BudgetLineItem -> Category
- CustomField/Value relationships

**SetNull** (sets FK to NULL if parent deleted):
- Transaction -> Fund
- Transaction -> Donor
- Transaction -> Grant
- InventoryItem -> Category/Location
- Document -> Grant/Donor/Transaction
- Project -> Building/Contractor/Fund/Grant
- And others (26 total)

**Cascade** (deletes all child records):
- Budget -> BudgetLineItem
- TransactionSplit -> Transaction
- Project -> MaintenanceTask
- CustomField -> CustomFieldValue

---

## Soft Delete Configuration

### Transaction (Lines 142-146)
```csharp
entity.HasQueryFilter(t => !t.IsDeleted);  // Excluded by default
entity.HasIndex(t => t.IsDeleted);
```

**Implementation**: Transaction class implements ISoftDelete:
- `bool IsDeleted { get; set; } = false`
- `DateTime? DeletedAt { get; set; }`
- `string? DeletedBy { get; set; }`

**Impact**: 
- Queries automatically filter out deleted transactions
- Import operations must bypass soft-delete filter if restoring deleted items
- Reports should respect soft-delete flag

---

## Key Performance Indexes

### Transaction Indexes (Critical for Reports/Queries)
- `Date` - Single column
- `Type` - Single column
- `CategoryId`, `FundId`, `DonorId`, `GrantId` - Single columns
- `ReferenceNumber`, `PONumber` - Single columns
- **Composite**: `(Date, Type)` - For report filtering
- **Composite**: `(FundId, Date)` - For fund balance queries

### Category Indexes
- **Unique Composite**: `(ParentId, Name)` - Ensures no duplicate names at same level

### Fund Indexes
- **Unique**: `Name` - Ensures unique fund names

### Document Indexes
- `OriginalFileName`, `Type`, `UploadedAt`, `IsArchived`

---

## Database Integrity Checklist

### RowVersion Initialization ✅
- [x] Transaction: InitializedService.CreateAsync
- [x] Transaction: Initialized in CreateTransferAsync
- [x] Transaction: Initialized in ImportExportService
- [x] Fund: Initialized in FundService.CreateAsync
- [x] Fund: Initialized in ImportExportService (auto-create)
- [x] Grant: Initialized in GrantService.CreateAsync

### Soft Delete Filter ✅
- [x] Transaction: Has global query filter (!t.IsDeleted)
- [x] Transaction: Index on IsDeleted column
- [x] Transaction: Implements ISoftDelete interface

### Foreign Key Relationships ✅
- [x] All foreign keys configured with appropriate OnDelete behaviors
- [x] No circular dependencies detected
- [x] Parent-child relationships properly defined

### Unique Constraints ✅
- [x] Category: (ParentId, Name) - Prevents duplicates at same level
- [x] Fund: Name - Prevents duplicate fund names
- [x] BudgetLineItem: (BudgetId, CategoryId) - One line per category per budget
- [x] CustomField: (EntityType, Name) - One field definition per entity type
- [x] UserModuleAccess: (UserId, ModuleName) - One access record per user/module

---

## Known Configuration Details

### Database Provider
- **SQLite** (not SQL Server)
- RowVersion uses simple uint counter: `HasDefaultValue(0u).IsConcurrencyToken()`
- Does NOT use `IsRowVersion()` because SQLite lacks auto row versioning

### Identity Integration
- Inherits from `IdentityDbContext<ApplicationUser>`
- Includes ASP.NET Core Identity tables automatically

### Audit Logging
- `AuditLog` table with indexes on:
  - EntityType
  - EntityId
  - Timestamp
  - Composite: (EntityType, EntityId)

---

## Recommended Next Steps

### If You're Experiencing Errors:

1. **Get Specific Error Details**
   - Capture full error messages with stack traces
   - Note which operation triggers the error (import, create, update, delete)
   - Check which entity type is involved

2. **Check for These Common Issues**
   - CSV import with missing required fields
   - Attempting to delete parent record with child records (Restrict behavior)
   - Accessing soft-deleted records without bypassing filter
   - Concurrent updates without proper RowVersion handling

3. **Test Import Workflow**
   - Verify CSV columns match expected format
   - Ensure CategoryId references exist in database
   - Check that Fund auto-creation works correctly
   - Confirm RowVersion initialization doesn't conflict with import

4. **Verify Theme Toggle**
   - Test in multiple browsers
   - Check browser console for JavaScript errors
   - Verify localStorage is available and working
   - Confirm theme.js initializes before components render

---

## Summary Table

| Entity | RowVersion | Soft Delete | Unique Constraints | Service Initialized |
|--------|------------|-------------|-------------------|-------------------|
| Transaction | Yes | Yes | None | ✅ TransactionService |
| Fund | Yes | No | Name | ✅ FundService |
| Grant | Yes | No | None | ✅ GrantService |
| Category | No | No | (ParentId, Name) | N/A |
| Donor | No | No | None | N/A |
| All Others | No | No | Varies | N/A |

---

## Database Health Conclusion

✅ **Schema is properly configured**
✅ **All required RowVersion initialization is in place**
✅ **Soft-delete filter is correct for Transaction**
✅ **Foreign key relationships are well-designed**
✅ **Indexes support key query patterns**

**If errors persist, they likely relate to:**
1. Data inconsistencies (referenced records don't exist)
2. Soft-delete filter unintentionally hiding needed records
3. Import format/mapping issues
4. JavaScript/theme initialization timing in Blazor
5. Concurrent update conflicts (handled by RowVersion conflict detection)

Request specific error details to diagnose further.
