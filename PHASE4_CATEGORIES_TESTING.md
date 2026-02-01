# Phase 4 Categories Page - Testing Guide

## ? Build Status: **SUCCESSFUL**

The Categories management page is now ready to test!

---

## ?? How to Test

### Step 1: Clean & Run Database
If you haven't already, reset the database to include inventory seed data:

```powershell
# Delete database
Remove-Item "nonprofit.db*" -Force

# Run the app (migrations will apply automatically)
dotnet run
```

### Step 2: Navigate to Categories Page

1. **Start the application**: Press `F5` in Visual Studio or run `dotnet run`
2. **Open browser**: Navigate to `https://localhost:5001`
3. **Go to Landing Page**: Click on `/landing` or the home page
4. **Click Inventory Tile**: Click the "?? Inventory" tile
5. **Click Categories**: In the left sidebar, click "??? Categories"

**Direct URL:** `https://localhost:5001/inventory/categories`

---

## ? Test Checklist

### Page Loading
- [ ] Page loads without errors
- [ ] Tree view shows on the left
- [ ] Details panel shows on the right (empty state initially)
- [ ] "Add Category" button visible in header

### View Categories
- [ ] See 10 seeded categories in tree:
  - Fire Equipment
  - Medical Equipment
  - Safety Equipment
  - Office Supplies
  - Cleaning Supplies
  - Training Materials
  - Hand Tools
  - Power Tools
  - Vehicle Parts
  - Vehicle Accessories

### Select Category
- [ ] Click a category in the tree
- [ ] Details panel updates on the right
- [ ] Shows category name and description
- [ ] Shows stats (Items: 0, Status: Active)
- [ ] Action buttons appear (Edit, Add Sub-Category, View Items, Delete)

### Add Root Category
1. Click "? Add Category" button in header
2. Modal opens
3. Fill in:
   - Name: "Test Category"
   - Description: "Test description"
   - Parent: -- Root Category --
4. Click "?? Save"
5. Modal closes
6. Tree refreshes
7. New category appears in list

### Add Sub-Category
1. Select a category from the tree
2. Click "? Add Sub-Category" button in details panel
3. Modal opens with parent pre-selected
4. Fill in:
   - Name: "Sub Test"
   - Description: "Sub category test"
5. Click "?? Save"
6. Tree refreshes
7. Expand parent to see sub-category

### Edit Category
1. Select a category
2. Click "?? Edit" button
3. Modal opens with data pre-filled
4. Change name to "Updated Name"
5. Toggle "Active" checkbox
6. Click "?? Save"
7. Tree refreshes
8. Changes appear

### Delete Category
1. Select a category with:
   - 0 items
   - 0 sub-categories
2. Click "??? Delete" button
3. Category is deleted
4. Tree refreshes
5. Category removed

**Note:** Categories with items or sub-categories cannot be deleted (button won't show)

### View Items
1. Select a category
2. Click "?? View Items" button
3. Navigates to `/inventory/items?category={id}`
4. Items list filters by that category

### Tree Interaction
- [ ] Click expand button (?) to expand categories with sub-categories
- [ ] Click collapse button (?) to collapse expanded categories
- [ ] Selected category highlights
- [ ] Item count shows next to each category name

### Responsive Design
1. Resize browser to mobile width
2. Tree and details should stack vertically
3. All features still accessible

---

## ?? Expected Issues (Known)

### ? Working As Designed:
1. **Delete button hidden** - If category has items or sub-categories, delete button doesn't show (intentional)
2. **No confirmation dialog** - Delete happens immediately (marked as TODO in code)
3. **Item count is 0** - Until you add items, all categories show 0 items

### ?? Should NOT Happen:
- Page crashes or errors
- Modal doesn't open/close
- Changes don't save
- Tree doesn't refresh
- Navigation doesn't work

---

## ?? Expected UI Structure

```
???????????????????????????????????????????????????????
? ?? Inventory Categories           [? Add] [??]     ?
???????????????????????????????????????????????????????
? Category Tree    ? Category Details                 ?
?                  ?                                  ?
? ??? Fire Equipment? Fire Equipment                   ?
? ??? Medical Eq... ? Firefighting equipment and tools ?
? ??? Safety Eq...  ?                                  ?
? ??? Office Sup... ? Items: 0                         ?
? ??? Cleaning...   ? Status: Active                   ?
? ??? Training...   ?                                  ?
? ??? Hand Tools    ? [?? Edit]                        ?
? ??? Power Tools   ? [? Add Sub-Category]            ?
? ??? Vehicle Parts ? [?? View Items]                  ?
? ??? Vehicle Acc...? [??? Delete]                      ?
???????????????????????????????????????????????????????
```

---

## ?? Troubleshooting

### Categories don't show
**Fix:** Database needs seed data. Stop app, delete `nonprofit.db`, restart.

### Modal doesn't open
**Fix:** Check browser console for JavaScript errors. Refresh page.

### Changes don't save
**Fix:** Check that database connection is working. Look at console output for error messages.

### Tree looks broken
**Fix:** Clear browser cache and hard refresh (`Ctrl + Shift + R`)

---

## ?? Next Steps After Testing

Once Categories page works:

1. **? Phase 4.1 Complete** - Categories management
2. **?? Phase 4.2 Next** - Locations management page (similar to categories)
3. **?? Phase 4.3** - Transactions & movements page
4. **?? Phase 4.4** - Reports page
5. **?? Phase 4.5** - Settings page

---

## ?? Test Results Template

Fill this out after testing:

```
Date: _____________
Tester: ___________

? Page Loading: PASS / FAIL
? View Categories: PASS / FAIL
? Select Category: PASS / FAIL
? Add Category: PASS / FAIL
? Add Sub-Category: PASS / FAIL
? Edit Category: PASS / FAIL
? Delete Category: PASS / FAIL
? View Items: PASS / FAIL
? Tree Interaction: PASS / FAIL
? Responsive: PASS / FAIL

Overall: PASS / FAIL

Issues Found:
1. _____________________
2. _____________________
3. _____________________

Notes:
_______________________
_______________________
```

---

**Ready to test! ??**
