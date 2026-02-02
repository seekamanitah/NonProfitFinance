# Global Search Direct Filtering - Fixed

## Issue
When clicking a search result in the global search bar, it would navigate to the general page (e.g., `/transactions`) but wouldn't automatically filter to show the specific item. Users had to manually search again on that page.

## Solution
Updated the global search to pass search parameters in the URL, and updated all target pages to read and apply those parameters on initialization.

## Changes Made

### 1. GlobalSearch.razor
- **Changed URL format**: From `?id={id}` to `?search={searchTerm}`
- **Transactions**: Now uses transaction description in URL: `/transactions?search=Office%20Supplies`
- **Donors**: Uses donor name: `/donors?search=John%20Doe`
- **Grants**: Uses grant name: `/grants?search=FEMA%20AFG`
- **Categories**: Uses category name: `/categories?search=Utilities`

### 2. TransactionList.razor
- Added `System.Web` using statement for URL parsing
- Added `NavigationManager` injection
- Updated `OnInitializedAsync()` to:
  - Parse URL query parameters
  - Apply search term from `?search=` parameter
  - Expand date range to 1 year when coming from search (instead of default 1 month)
  - Automatically trigger search with the parameter

### 3. DonorList.razor
- Added `System.Web` using statement
- Added `NavigationManager` injection
- Updated `OnInitializedAsync()` to:
  - Read `?search=` parameter
  - Pre-fill the search box with the search term
  - Automatically filter donors

### 4. GrantList.razor
- Added `System.Web` using statement
- Added `NavigationManager` injection
- Updated `OnInitializedAsync()` to:
  - Read `?search=` parameter
  - Pre-fill the search box
  - Automatically filter grants

### 5. CategoryManager.razor
- Added `System.Web` using statement
- Added `NavigationManager` injection
- Updated `OnInitializedAsync()` to:
  - Read `?search=` parameter
  - Find matching category in all categories
  - Switch to correct tab (Income/Expense) if needed
  - Auto-select the matching category

## User Experience Improvements

### Before
1. Search "Office Supplies" in global search
2. Click the transaction result
3. Navigate to `/transactions`
4. See all transactions (unfiltered)
5. Must manually search again for "Office Supplies"

### After
1. Search "Office Supplies" in global search
2. Click the transaction result
3. Navigate to `/transactions?search=Office%20Supplies`
4. **Automatically see filtered results** showing only "Office Supplies" transactions
5. Search box is pre-filled with "Office Supplies"
6. Date range expanded to 1 year for better results

## Technical Notes

- Uses `System.Web.HttpUtility.ParseQueryString()` to parse URL parameters
- Uses `Uri.EscapeDataString()` to properly encode search terms in URLs
- Search terms are case-insensitive matches
- For categories, automatically switches to the correct Income/Expense tab
- Maintains existing filter functionality while adding search pre-fill

## Testing Checklist

- [x] Build successful
- [ ] Test transaction search and navigation
- [ ] Test donor search and navigation
- [ ] Test grant search and navigation
- [ ] Test category search and navigation (both Income and Expense)
- [ ] Verify search box is pre-filled
- [ ] Verify filters are applied automatically
- [ ] Verify URL parameters are properly encoded
- [ ] Test with special characters in search terms

## Related Files
- `Components/Shared/GlobalSearch.razor`
- `Components/Pages/Transactions/TransactionList.razor`
- `Components/Pages/Donors/DonorList.razor`
- `Components/Pages/Grants/GrantList.razor`
- `Components/Pages/Categories/CategoryManager.razor`
