# Phase 3 Fix Plan - Adapt Services to Existing Models

**Strategy:** Modify Phase 3 services to work with existing Phase 1 model properties.  
**Do NOT modify:** Database schema, model classes, or anything that could affect the Financial module.

---

## ?? Core Principle
**Adapt the NEW code (Phase 3 services) to fit the EXISTING schema (Phase 1 models)**

---

## ?? Model Property Mappings

### InventoryItem Model - Actual Properties:
| DTO Property | Model Property | Type | Notes |
|--------------|----------------|------|-------|
| `UnitOfMeasure` | `Unit` | `UnitOfMeasure` (enum) | Convert enum to string |
| `MinimumStock` | `MinimumQuantity` | `decimal?` | Direct mapping |
| `MaximumStock` | `MaximumQuantity` | `decimal?` | Direct mapping |
| `ReorderPoint` | *(N/A - remove)* | - | Not in model, remove from DTOs/services |
| `ExpiryDate` | `ExpirationDate` | `DateTime?` | Direct mapping |
| `ImageUrl` | `PhotoUrl` | `string?` | Direct mapping |
| `Supplier` | *(N/A - remove)* | - | Not in model, remove from DTOs/services |

### InventoryTransaction Model - Actual Properties:
| DTO Property | Model Property | Type | Notes |
|--------------|----------------|------|-------|
| `UnitOfMeasure` | *(N/A - get from Item)* | - | Fetch from related InventoryItem |
| `RelatedFinancialTransactionId` | *(N/A - remove)* | - | Not in model, remove |
| `CreatedAt` | `CreatedAt` | `DateTime` | Already exists ? |

### InventoryCategory Model - Actual Properties:
| DTO Property | Model Property | Type | Notes |
|--------------|----------------|------|-------|
| `ColorCode` | *(N/A - remove)* | - | Not in model, remove |
| `Icon` | *(N/A - remove)* | - | Not in model, remove |
| `UpdatedAt` | `UpdatedAt` | `DateTime` | Already exists ? |

---

## ?? Required Changes

### 1?? Update InventoryItemService.cs

**File:** `Services/Inventory/InventoryItemService.cs`

#### Change 1: Fix CreateAsync mapping (line ~88-110)
```csharp
// OLD (incorrect):
UnitOfMeasure = request.UnitOfMeasure,
MinimumStock = request.MinimumStock,
MaximumStock = request.MaximumStock,
ReorderPoint = request.ReorderPoint,
ImageUrl = request.ImageUrl,
Supplier = request.Supplier,
ExpiryDate = request.ExpiryDate,

// NEW (correct):
Unit = Enum.TryParse<UnitOfMeasure>(request.UnitOfMeasure, out var unit) ? unit : UnitOfMeasure.Each,
MinimumQuantity = request.MinimumStock,
MaximumQuantity = request.MaximumStock,
PhotoUrl = request.ImageUrl,
ExpirationDate = request.ExpiryDate,
// Remove: ReorderPoint, Supplier
```

#### Change 2: Fix UpdateAsync mapping (line ~120-142)
```csharp
// OLD (incorrect):
item.UnitOfMeasure = request.UnitOfMeasure;
item.MinimumStock = request.MinimumStock;
item.MaximumStock = request.MaximumStock;
item.ReorderPoint = request.ReorderPoint;
item.ImageUrl = request.ImageUrl;
item.Supplier = request.Supplier;
item.ExpiryDate = request.ExpiryDate;

// NEW (correct):
item.Unit = Enum.TryParse<UnitOfMeasure>(request.UnitOfMeasure, out var unit) ? unit : UnitOfMeasure.Each;
item.MinimumQuantity = request.MinimumStock;
item.MaximumQuantity = request.MaximumStock;
item.PhotoUrl = request.ImageUrl;
item.ExpirationDate = request.ExpiryDate;
// Remove: ReorderPoint, Supplier
```

#### Change 3: Fix LowStock query (line ~36)
```csharp
// OLD:
query = query.Where(i => i.Quantity <= (i.MinimumStock ?? 0));

// NEW:
query = query.Where(i => i.Quantity <= (i.MinimumQuantity ?? 0));
```

#### Change 4: Fix MapToDto method (line ~420+)
```csharp
// OLD:
item.UnitOfMeasure,
item.MinimumStock,
item.MaximumStock,
item.ReorderPoint,
item.ImageUrl,
item.Supplier,
item.ExpiryDate,

// NEW:
item.Unit.ToString(),
item.MinimumQuantity,
item.MaximumQuantity,
null, // ReorderPoint not in model
item.PhotoUrl,
item.ExpirationDate,
null, // Supplier not in model
```

#### Change 5: Fix SearchAsync MapToDto (line ~180-188)
```csharp
// Change:
i.UnitOfMeasure
// To:
i.Unit.ToString()
```

#### Change 6: Fix GetExpiringItemsAsync (line ~240-247)
```csharp
// OLD:
i.ExpiryDate.HasValue
i.ExpiryDate.Value

// NEW:
i.ExpirationDate.HasValue
i.ExpirationDate.Value
```

#### Change 7: Fix AdjustStockAsync (line ~255-275)
```csharp
// OLD:
item.Status = DetermineStatus(item.Quantity, item.MinimumStock, item.ExpiryDate);
UnitOfMeasure = item.UnitOfMeasure,

// NEW:
item.Status = DetermineStatus(item.Quantity, item.MinimumQuantity, item.ExpirationDate);
// Remove UnitOfMeasure from InventoryTransaction - not in model
```

#### Change 8: Fix TransferStockAsync (line ~280-300)
```csharp
// Remove:
UnitOfMeasure = item.UnitOfMeasure,
```

#### Change 9: Fix SetStockLevelAsync (line ~305-335)
```csharp
// Remove:
UnitOfMeasure = item.UnitOfMeasure,
```

#### Change 10: Fix DetermineStatus signature (line ~395)
```csharp
// No change needed - method signature is fine
```

---

### 2?? Update InventoryTransactionService.cs

**File:** `Services/Inventory/InventoryTransactionService.cs`

#### Add using directive at top:
```csharp
using NonProfitFinance.Models.Enums;
```

#### Change 1: Fix CreateAsync (line ~85-107)
```csharp
// OLD:
UnitOfMeasure = item.UnitOfMeasure,

// NEW:
// Remove UnitOfMeasure - not in model

// OLD:
RelatedFinancialTransactionId = request.RelatedFinancialTransactionId,

// NEW:
// Remove RelatedFinancialTransactionId - not in model
```

#### Change 2: Fix MapToDto (line ~220-250)
```csharp
// OLD:
transaction.UnitOfMeasure,
transaction.RelatedFinancialTransactionId,
transaction.CreatedAt

// NEW:
transaction.Item.Unit.ToString(), // Get from related Item
null, // RelatedFinancialTransactionId not in model
transaction.CreatedAt // This one exists ?
```

#### Change 3: Fix GetUsageReportAsync enum references (line ~180-195)
```csharp
// Already has correct enum names - just need using directive
// InventoryTransactionType.Addition
// InventoryTransactionType.Removal
```

---

### 3?? Update InventoryCategoryService.cs

**File:** `Services/Inventory/InventoryCategoryService.cs`

#### Change 1: Fix CreateAsync (line ~48-60)
```csharp
// OLD:
ColorCode = request.ColorCode,
Icon = request.Icon,

// NEW:
// Remove ColorCode and Icon - not in model
```

#### Change 2: Fix UpdateAsync (line ~72-86)
```csharp
// OLD:
category.ColorCode = request.ColorCode;
category.Icon = request.Icon;
category.UpdatedAt = DateTime.UtcNow;

// NEW:
// Remove ColorCode and Icon
category.UpdatedAt = DateTime.UtcNow; // This exists ?
```

#### Change 3: Fix DeleteAsync (line ~95-104)
```csharp
// UpdatedAt already exists - no change needed ?
```

#### Change 4: Fix MoveLocationAsync (line ~165-175)
```csharp
// UpdatedAt already exists - no change needed ?
```

#### Change 5: Fix MapToDto (line ~205-220)
```csharp
// OLD:
category.ColorCode,
category.Icon,

// NEW:
null, // ColorCode not in model
null, // Icon not in model
```

---

### 4?? Update DTOs (Remove unused properties)

**File:** `DTOs/Inventory/InventoryDtos.cs`

#### Change InventoryItemDto to make optional:
```csharp
// Change these to nullable:
decimal? ReorderPoint,  // Make nullable - not in model
string? Supplier,       // Make nullable - not in model
```

#### Change CategoryDto:
```csharp
// Make these nullable:
string? ColorCode,      // Make nullable - not in model
string? Icon,           // Make nullable - not in model
```

#### Change TransactionDto:
```csharp
// Make nullable:
int? RelatedFinancialTransactionId,  // Make nullable - not in model
```

---

### 5?? Fix ItemsList.razor Filter Constructor

**File:** `Components/Pages/Inventory/ItemsList.razor` (line ~307)

```csharp
// OLD (object initializer - doesn't work with records):
var filter = new InventoryItemFilterRequest
{
    SearchTerm = searchTerm,
    ...
};

// NEW (positional constructor):
var filter = new InventoryItemFilterRequest(
    CategoryId: int.TryParse(selectedCategoryId, out var catId) ? catId : null,
    LocationId: int.TryParse(selectedLocationId, out var locId) ? locId : null,
    Status: Enum.TryParse<InventoryStatus>(selectedStatus, out var status) ? status : null,
    LowStock: null,
    SearchTerm: searchTerm,
    SortBy: sortBy,
    SortDescending: sortDescending,
    Page: currentPage,
    PageSize: pageSize
);
```

---

### 6?? Fix ItemForm.razor Remove Unused Fields

**File:** `Components/Pages/Inventory/ItemForm.razor`

#### Remove from form (lines to delete):
1. **ReorderPoint field** (around line ~120-125)
2. **Supplier field** (around line ~140-145)

These fields don't exist in the model, so remove them from the UI.

---

### 7?? Fix InventoryDashboard UnitOfMeasure Display

**File:** `Components/Pages/Inventory/InventoryDashboard.razor`

No changes needed - DTOs already handle the conversion.

---

## ? Testing Checklist

After making changes:

1. ? **Build** - `dotnet build` (should succeed)
2. ? **Run** - Start application
3. ? **Dashboard** - Navigate to `/inventory` - should load
4. ? **Items List** - Navigate to `/inventory/items` - should load
5. ? **Add Item** - Click "Add Item" - form should work
6. ? **Create Item** - Fill form and save - should create
7. ? **Edit Item** - Click edit on an item - should load
8. ? **Update Item** - Modify and save - should update
9. ? **Delete Item** - Delete an item - should work
10. ? **Filters** - Test category/location/status filters

---

## ?? Impact Assessment

| Component | Changes | Risk | Test Priority |
|-----------|---------|------|---------------|
| InventoryItemService | 10 changes | ?? Medium | ??? High |
| InventoryTransactionService | 3 changes | ?? Low | ?? Medium |
| InventoryCategoryService | 5 changes | ?? Low | ?? Medium |
| DTOs | Make properties nullable | ?? Low | ? Low |
| ItemsList.razor | 1 change | ?? Low | ??? High |
| ItemForm.razor | Remove 2 fields | ?? Low | ?? Medium |
| **Database** | ? NO CHANGES | ? Zero | - |
| **Phase 1 Models** | ? NO CHANGES | ? Zero | - |

---

## ?? Time Estimate

- Service fixes: **20 minutes**
- DTO updates: **5 minutes**
- UI fixes: **5 minutes**
- Testing: **10 minutes**
- **Total: ~40 minutes**

---

## ?? Success Criteria

- ? Solution builds without errors
- ? Inventory dashboard loads
- ? Can create/edit/delete items
- ? Filters work correctly
- ? No changes to database or Phase 1 models
- ? Financial module still works (no regression)

---

**Status:** Ready to implement - Services will adapt to existing schema.
