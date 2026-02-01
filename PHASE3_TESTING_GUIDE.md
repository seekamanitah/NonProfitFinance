# Phase 3 - Quick Test Guide

## ?? How to Test the Inventory Module

### Step 1: Run the Application
```bash
dotnet run
```

### Step 2: Navigate to Inventory
1. Open browser to: `https://localhost:5001` (or your port)
2. Go to Landing Page: `/` or `/landing`
3. Click the **Inventory** tile (??)

### Step 3: Test Dashboard
**URL:** `/inventory` or `/inventory/dashboard`

**What to Check:**
- ? Metrics cards display (Total Items, Low Stock, Out of Stock, Total Value)
- ? Stock by Category chart shows
- ? Stock Status breakdown displays
- ? Low Stock Alerts section (if any items)
- ? Recent Transactions list
- ? "Add Item" button works
- ? "Record Movement" button works

### Step 4: Test Items List
**URL:** `/inventory/items`

**What to Check:**
- ? Table loads with items
- ? Search box filters items
- ? Category dropdown filters
- ? Location dropdown filters
- ? Status dropdown filters
- ? "Clear" button resets filters
- ? Sorting works (click column headers)
- ? Pagination works
- ? "Add Item" button opens form
- ? Edit icon (??) opens edit form
- ? Delete icon (???) deletes item

### Step 5: Test Add Item Form
**URL:** `/inventory/items/new`

**What to Test:**
- ? Form loads
- ? All fields available:
  - Name (required)
  - SKU
  - Description
  - Category (dropdown)
  - Location (dropdown)
  - Quantity (required)
  - Unit of Measure (required)
  - Unit Cost (required)
  - Minimum Stock Level
  - Maximum Stock Level
  - Manufacturer
  - Purchase Date
  - Expiry Date
  - Image URL
  - Barcode
  - Notes
- ? "Save" button creates item
- ? "Cancel" button returns to list
- ? Redirects to items list after save

### Step 6: Test Edit Item
**URL:** `/inventory/items/{id}/edit`

**What to Test:**
- ? Form loads with existing data
- ? All fields are populated
- ? Can modify values
- ? "Save" updates item
- ? "Cancel" returns to list
- ? "Active" checkbox works (if editing)

### Step 7: Test Navigation
**What to Check:**
- ? Left sidebar shows:
  - ?? Home (back to landing)
  - ?? Dashboard
  - ?? Items
  - ??? Categories (placeholder)
  - ?? Locations (placeholder)
  - ?? Transactions (placeholder)
  - ?? Reports (placeholder)
  - ?? Settings (placeholder)
- ? Quick action buttons:
  - ? Add Item
  - ?? Record Movement

### Step 8: Test Dark Mode
**How:** Click theme toggle in top bar

**What to Check:**
- ? Dashboard switches to dark theme
- ? Items list switches to dark theme
- ? Forms switch to dark theme
- ? All text remains readable
- ? Charts adjust colors

### Step 9: Test Responsive Design
**How:** Resize browser window or test on mobile

**What to Check:**
- ? Sidebar collapses on small screens
- ? Tables become scrollable
- ? Metrics cards stack vertically
- ? Forms remain usable
- ? Navigation accessible

---

## ?? Known Issues (By Design)

### Expected Behaviors:
1. **Location Type** - All locations show as "Facility" (model doesn't have Type property)
2. **No ReorderPoint** - Field not in form (not in Phase 1 model)
3. **No Supplier** - Field not in form (not in Phase 1 model)
4. **Categories/Locations pages** - Show "Coming Soon" (Phase 4 features)
5. **Transactions page** - Shows "Coming Soon" (Phase 4 features)
6. **Reports page** - Shows "Coming Soon" (Phase 4 features)

---

## ? Success Criteria

If all these work, Phase 3 is successful:
- [ ] Can navigate to Inventory module
- [ ] Dashboard loads with data
- [ ] Can view items list
- [ ] Can add new item
- [ ] Can edit existing item
- [ ] Can delete item
- [ ] Filters work on items list
- [ ] Pagination works
- [ ] Dark mode works
- [ ] No console errors
- [ ] No build errors

---

## ?? Troubleshooting

### Dashboard shows zero items
**Solution:** Add some items first using "Add Item" button

### Categories/Locations dropdown empty
**Solution:** Need to add these via Data Seeder or manually in Phase 4

### Can't create item
**Check:** All required fields filled (Name, Quantity, Unit of Measure, Unit Cost)

### Page not found
**Check:** Make sure you're using correct URLs:
- `/inventory` or `/inventory/dashboard` - Dashboard
- `/inventory/items` - Items list
- `/inventory/items/new` - Add item form

### Styling looks wrong
**Check:** Make sure `inventory.css` is loaded in `Components/App.razor`

---

## ?? Expected Screenshots

### Dashboard
```
???????????????????????????????????????????????
? ?? Inventory Dashboard           [? Add]  ?
???????????????????????????????????????????????
? ?????????? ?????????? ?????????? ??????????
? ? Total  ? ? Low    ? ?Out of  ? ? Total ??
? ? Items  ? ? Stock  ? ?Stock   ? ? Value ??
? ?   0    ? ?   0    ? ?   0    ? ?  $0   ??
? ?????????? ?????????? ?????????? ??????????
?                                             ?
? Stock by Category          Stock Status     ?
? [Chart]                    [Breakdown]      ?
???????????????????????????????????????????????
```

### Items List
```
???????????????????????????????????????????????
? ?? Inventory Items                [? Add]  ?
???????????????????????????????????????????????
? [Search...] [Category?] [Location?] [??]   ?
?                                             ?
? Name     SKU   Category Location Qty Status ?
? ??????????????????????????????????????????  ?
? Item 1   001   Office   Room A   10  ?     ?
? Item 2   002   Tools    Truck    5   ??     ?
?                                             ?
? [? Previous]  Page 1 of 1  [Next ?]        ?
???????????????????????????????????????????????
```

---

## ?? Next Steps After Testing

If everything works:
1. ? Mark Phase 3 complete
2. ?? Document any bugs found
3. ?? Optionally start Phase 4 (Categories, Locations, Transactions pages)
4. ?? Add sample data via DataSeeder
5. ?? Customize colors/theme if desired

---

**Happy Testing! ??**
