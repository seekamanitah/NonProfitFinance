# ?? Inventory Module - Quick Test Script

**Quick 15-Minute Smoke Test**  
Use this for rapid verification after fixes are applied.

---

## ? Pre-Test Setup

1. **Stop debugger** (Shift+F5)
2. **Rebuild solution:**
   ```
   dotnet clean
   dotnet build
   ```
3. **Start application** (F5)
4. **Load demo data** on landing page

---

## ?? 5-Minute Critical Path Test

### Test 1: Dashboard (2 min)
```
? Navigate to /inventory
? Verify 4 metric cards display
? Check category breakdown chart shows data
? Verify low stock alerts section visible
? Click "Add Item" button ? goes to form
? Go back to dashboard
```

### Test 2: Items List (1 min)
```
? Click "Items" in sidebar
? Verify items table displays
? Type in search box ? table filters
? Click category dropdown ? select one ? filters
? Clear filters ? shows all items
```

### Test 3: Add New Item (2 min)
```
? Click "Add Item" button
? Fill in:
   - Name: "Test Fire Extinguisher"
   - SKU: "TEST-001"
   - Category: Select "Safety Equipment"
   - Location: Select any location
   - Quantity: 10
   - Unit Cost: 25.00
? Click Save
? Verify redirects to items list
? Search for "Test Fire" ? should find new item
```

---

## ?? 10-Minute Extended Test

### Test 4: Categories (2 min)
```
? Click "Categories" in sidebar
? Verify tree displays with demo categories
? Expand/collapse category nodes
? Click "Add Category"
? Enter name: "Test Category"
? Save ? appears in tree
? Click edit ? change name ? Save
? Delete test category
```

### Test 5: Locations (2 min)
```
? Click "Locations" in sidebar
? Verify location hierarchy displays
? Add new location at root level
? Add child location under it
? Verify parent-child relationship
? Delete test locations
```

### Test 6: Transactions (2 min)
```
? Click "Transactions" in sidebar
? Verify list of transactions from demo data
? Click "Record Transaction" or similar
? Select transaction type: "Use"
? Select an item
? Enter quantity: 2
? Enter reason: "Testing"
? Save ? transaction recorded
? Verify item quantity decreased
```

### Test 7: Edit Item (2 min)
```
? Go back to Items list
? Click Edit on any item
? Change quantity from 10 to 15
? Save
? Verify quantity updated in list
? Check dashboard metrics updated
```

### Test 8: Stock Alerts (2 min)
```
? Edit an item
? Set Quantity to 1
? Set Minimum Stock to 5
? Save
? Go to dashboard
? Verify item appears in Low Stock Alerts
? Low Stock count increased by 1
```

---

## ?? Known Issues Check

After each test, verify these fixes were applied:

### ? Fix #1: Routing Conflict
```
? /inventory route works (no ambiguous match error)
? Only InventoryDashboard handles the route
? InventoryPlaceholder.razor was deleted
```

### ? Fix #2: EF Core Query Translation
```
? Dashboard loads without InvalidOperationException
? Category breakdown chart displays correctly
? Location breakdown works (if visible on page)
? No LINQ translation errors in console
```

### ? Fix #3: PageHeader Parameter
```
? Dashboard loads without "Icon" parameter error
? Add Item and Record Movement buttons display
? PageHeader component renders correctly
```

---

## ?? Quick Issue Log

Use this if you find problems:

```
[ ] Issue: _______________________
    Page: _______________________
    Error: _______________________
    Priority: High / Med / Low
    
[ ] Issue: _______________________
    Page: _______________________
    Error: _______________________
    Priority: High / Med / Low
```

---

## ? Success Criteria

**Module is READY if:**
- ? All 8 tests pass without errors
- ? No console errors during testing
- ? Data persists correctly to database
- ? All 3 known fixes are working
- ? Navigation between pages works
- ? CRUD operations complete successfully

**Status After Testing:**
- ?? **PASS** - Ready for full audit
- ?? **MINOR ISSUES** - Fix and retest
- ?? **MAJOR ISSUES** - Needs significant work

---

## ?? Next Steps

### If Test PASSES:
1. Proceed with full audit (INVENTORY_MODULE_COMPLETE_AUDIT.md)
2. Test edge cases and advanced features
3. Performance testing with larger datasets
4. Accessibility audit

### If Test FAILS:
1. Document all errors
2. Check console for error messages
3. Review recent code changes
4. Apply fixes
5. Restart this quick test

---

**Quick Test Completed:** ___/___/___  
**Result:** ?? PASS / ?? MINOR / ?? FAIL  
**Time Taken:** ___ minutes  
**Notes:** ___________________________

---

## ?? Critical Validation Points

While testing, specifically verify:

1. **Database reads work** (items, categories, locations load)
2. **Database writes work** (create, update, delete persist)
3. **Relationships intact** (items link to categories/locations)
4. **Calculations correct** (total value, stock status)
5. **Navigation smooth** (all links work, no 404s)
6. **UI responsive** (loads fast, no frozen screens)
7. **Errors handled** (validation messages, not crashes)

---

**Document:** Quick Test Script v1.0  
**For:** Inventory Module  
**Updated:** 2026-01-29
