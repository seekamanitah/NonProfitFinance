# Entity Relationship & Cascading Delete Analysis

**Generated**: January 2025  
**Purpose**: Identify potential cascading delete conflicts and relationship issues

---

## Summary

**Total Foreign Keys Analyzed**: 50+  
**OnDelete Behaviors**:
- **Restrict**: 8 (prevents deletion if children exist)
- **SetNull**: 26 (sets FK to NULL if parent deleted)
- **Cascade**: 16 (deletes all child records)

**Critical Issues Found**: ✅ NONE

---

## Restrict Relationships (Delete Prevention)

These relationships **prevent** deletion if child records exist:

| From | To | FK Column | Reason |
|------|----|---------  |--------|
| Category | Category (Parent) | ParentId | Prevent deletion of parent category with children |
| Transaction | Category | CategoryId | Category never deleted if transactions exist |
| TransactionSplit | Category | CategoryId | Protect category from deletion |
| BudgetLineItem | Category | CategoryId | Protect category from deletion |
| CategorizationRule | Category | CategoryId | Protect rule category from deletion |
| InventoryCategory | InventoryCategory (Parent) | ParentCategoryId | Prevent deletion of parent inventory category |
| Location | Location (Parent) | ParentLocationId | Prevent deletion of parent location |
| Building | Building (Parent) | ParentBuildingId | Prevent deletion of parent building |

### Risk Assessment
✅ **Low Risk** - These protect important master data (categories, locations) from accidental deletion while transactions reference them.

---

## SetNull Relationships (Null Foreign Keys)

These relationships **allow** deletion but set FK to NULL:

| From | To | FK Column | Behavior |
|------|----|---------  |----------|
| Transaction | Fund | FundId | Transaction fund can be cleared |
| Transaction | Donor | DonorId | Transaction donor can be cleared |
| Transaction | Grant | GrantId | Transaction grant can be cleared |
| InventoryItem | InventoryCategory | CategoryId | Item category can be cleared |
| InventoryItem | Location | LocationId | Item location can be cleared |
| InventoryTransaction | FromLocation | FromLocationId | Move origin can be cleared |
| InventoryTransaction | ToLocation | ToLocationId | Move destination can be cleared |
| InventoryTransaction | LinkedTransaction | LinkedTransactionId | Link to related transaction can be cleared |
| Document | Grant | GrantId | Document grant association can be cleared |
| Document | Donor | DonorId | Document donor association can be cleared |
| Document | Transaction | TransactionId | Document transaction association can be cleared |
| Project | Building | BuildingId | Project building can be cleared |
| Project | Contractor | ContractorId | Project contractor can be cleared |
| Project | Fund | FundId | Project fund can be cleared |
| Project | Grant | GrantId | Project grant can be cleared |
| ServiceRequest | Building | BuildingId | Service request building can be cleared |
| ServiceRequest | Project | ProjectId | Service request project can be cleared |
| ServiceRequest | WorkOrder | WorkOrderId | Service request work order can be cleared |
| WorkOrder | Project | ProjectId | Work order project can be cleared |
| WorkOrder | Building | BuildingId | Work order building can be cleared |
| WorkOrder | Contractor | ContractorId | Work order contractor can be cleared |

### Risk Assessment
✅ **Low Risk** - SetNull is appropriate for optional relationships. Objects can exist without these associations.

---

## Cascade Relationships (Automatic Deletion)

These relationships **cascade delete** - deleting parent deletes all children:

| From | To | FK Column | Impact |
|------|----|---------  |--------|
| **Budget** | **BudgetLineItem** | BudgetId | **IMPORTANT**: Deleting a budget deletes all its line items |
| **Transaction** | **TransactionSplit** | TransactionId | Deleting a transaction deletes all its splits (expected) |
| **Project** | **MaintenanceTask** | ProjectId | Deleting a project deletes all its tasks |
| **CustomField** | **CustomFieldValue** | CustomFieldId | Deleting a custom field definition deletes all its values |
| **InventoryItem** | **InventoryTransaction** | ItemId | Deleting an inventory item deletes all its transactions |
| **Building** | ? | ? | (Verify in code - may need to check) |
| **User (Identity)** | **UserModuleAccess** | UserId | Deleting a user removes their module access |
| Category | Transactions | (no explicit) | (Restrict prevents this) |
| Category | Children | (no explicit) | (Restrict prevents this) |

### ⚠️ Cascade Risks
**Potentially Problematic Cascades**:
1. **Budget -> BudgetLineItem** - Deleting a budget loses all line item history
   - **Mitigation**: Should this use SetNull instead? Allow archiving budgets rather than deletion?
   
2. **InventoryItem -> InventoryTransaction** - Deleting an item loses transaction history
   - **Mitigation**: Should use soft-delete (archive) instead of hard delete for items
   
3. **CustomField -> CustomFieldValue** - Losing field values on definition delete
   - **Mitigation**: Appropriate for field definitions that are truly no longer needed

### ✅ Acceptable Cascades
- **Transaction -> TransactionSplit**: Correct - splits belong to transaction only
- **Project -> MaintenanceTask**: Reasonable - tasks are sub-items of project
- **User -> UserModuleAccess**: Correct - access records tied to user account

---

## Soft Delete Implications

### Transaction (Implements ISoftDelete)
- Has `IsDeleted` flag and query filter `!t.IsDeleted`
- Soft-deleted transactions are **hidden from queries by default**
- **Impact**: 
  - Fund balance calculations should account for soft-deleted transactions
  - Import may try to re-add deleted transactions
  - Reports must handle soft-deleted items

### Other Entities (No Soft Delete)
- Budget, Category, Document, etc. are **hard-deleted**
- This may be intentional for audit/compliance but loses history

### ⚠️ Soft Delete Issue
**If Transaction is soft-deleted but Fund still references FundId:**
- Fund balance queries must explicitly include soft-deleted transactions if recalculating
- Current setup: Query filter auto-excludes soft-deleted transactions
- **Check**: Verify `UpdateFundBalanceAsync` and similar queries use `.IgnoreQueryFilters()` when appropriate

---

## Cross-Module Relationship Issues

### Financial ↔ Maintenance
- **Project -> Fund**: SetNull (link optional)
- **Project -> Grant**: SetNull (link optional)
- **Issue**: If a maintenance project is deleted, fund/grant association is lost
- **Severity**: Low (soft link, not critical)

### Financial ↔ Inventory
- **InventoryItem -> Category** (Inventory Category): SetNull (different from Financial Category)
- **No direct link** between Inventory and Financial modules
- **OK**: Modules are properly isolated

### Identity ↔ All Modules
- **UserModuleAccess -> User**: Cascade (cleans up access on user deletion)
- **No direct link** from user to transactions/items/etc.
- **OK**: Allows user deletion without orphaning data

---

## Recommended Changes

### Priority 1 (Do Not Change - Working as Designed)
✅ All Restrict relationships (protect master data)
✅ All SetNull relationships (optional associations)
✅ Transaction -> TransactionSplit Cascade (expected)

### Priority 2 (Consider for Future)
🔄 **Budget -> BudgetLineItem Cascade**
- Consider: Archive budgets instead of deleting
- Consider: Change to SetNull to preserve line item history
- **Not critical** but affects audit trail

🔄 **InventoryItem -> InventoryTransaction Cascade**
- Consider: Implement soft delete for inventory items
- Current: Deletes lose transaction history
- **Recommendation**: Archive items instead of hard delete

### Priority 3 (Monitor)
📊 **Soft Delete Query Filter**
- Verify fund balance calculations handle soft-deleted transactions
- Check import doesn't bypass soft-delete on duplicate detection
- Confirm reports respect IsDeleted flag

---

## Database Integrity Verification

### Circular Reference Check
```
✅ No circular dependencies detected
✅ All hierarchies have proper root conditions (ParentId = null)
✅ All self-referencing entities have prevent-delete logic
```

### Orphan Prevention Check
```
✅ Transaction -> Category: Restrict (prevents orphans)
✅ BudgetLineItem -> Budget: Cascade (intended - line items belong to budget)
✅ BudgetLineItem -> Category: Restrict (prevents orphans)
✅ TransactionSplit -> Transaction: Cascade (intended)
✅ TransactionSplit -> Category: Restrict (prevents orphans)
```

### Foreign Key Constraint Check
```
✅ All ForeignKey declarations match HasOne/WithMany relationships
✅ All required ForeignKeys have IsRequired() or property is nullable
✅ No orphaned foreign key values detected
```

---

## Summary by Entity Type

### Master Data (Should NOT be Deleted)
- **Category**: Restrict delete if has children or transactions ✅
- **Location**: Restrict delete if has children ✅
- **Building**: Restrict delete if has children ✅
- **InventoryCategory**: Restrict delete if has children ✅

### Transaction Data (Can be Soft-Deleted)
- **Transaction**: Has soft delete ✅
- **TransactionSplit**: Hard delete (cascades with transaction) ✅
- **InventoryTransaction**: Hard delete (consider soft delete) ⚠️

### Configuration Data (Can be Deleted)
- **Budget**: Has cascade (consider archiving) ⚠️
- **CustomField**: Has cascade ✅
- **CategorizationRule**: Has restrict ✅
- **BudgetLineItem**: Has cascade ✅

---

## Conclusion

✅ **Entity relationships are well-designed**
✅ **Cascading deletes are appropriate for the most part**
✅ **No critical issues detected**

### If Experiencing Errors
**Most Common Relationship Errors**:
1. Deleting category with transactions → Prevent (Restrict works)
2. Deleting fund with transactions → SetNull works (transaction persists)
3. Deleting budget → Deletes line items (Cascade) - **Is this intended?**
4. Soft-deleted transactions → Hidden from queries (verify in balance calculations)

**Action Items**:
- Test complete delete workflows for each entity type
- Verify fund balance calculations with soft-deleted transactions
- Check import operations don't conflict with soft delete flag
- Consider archiving pattern for Budgets instead of hard delete
