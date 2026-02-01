# ?? IMPLEMENTATION PLAN - Three Major Features

## Status: IN PROGRESS

---

## 1. ? PO Number System - STARTED

### What's Done:
- ? Added `PONumber` field to Transaction model
- ? Created `IPONumberService` and `PONumberService`
- ? Registered service in Program.cs
- ? Added `PONumber` to TransactionDto
- ? Added `PONumber` to CreateTransactionRequest
- ? Added `PONumber` to UpdateTransactionRequest

### Still Need To Do:
- [ ] Update TransactionService to map PONumber
- [ ] Add PO number to Transaction Form UI
- [ ] Add auto-generate button
- [ ] Add manual override capability
- [ ] Add PO number to Global Search
- [ ] Add PO number to Reimbursement forms
- [ ] Add PO number to Check Request forms
- [ ] Add PO number settings page
- [ ] Add database migration

### Features:
- **Format:** User-configurable (default: PO-{YYYY}-{####})
- **Auto-generate:** Sequential numbers per year
- **Unique:** Validation to prevent duplicates
- **Override:** Can be manually set
- **Searchable:** Included in global search

---

## 2. ? Fix Global Search - NOT STARTED

### Issue:
Global search returns no visible results and appears not to be responding

### Debugging Steps:
1. Check if PerformSearch() is being called
2. Verify search query is being set
3. Check if SearchAll() is executing
4. Verify results are being populated
5. Check if showResults is true
6. Verify StateHasChanged() is being called

### Likely Causes:
- Debounce timer not triggering
- Results not rendering due to CSS/DOM issues
- Services not returning data
- StateHasChanged() not being called at right time

---

## 3. ? Transfer Transactions - NOT STARTED

### Issues:
1. Transfer transaction page doesn't show "To" and "From" accounts
2. Should show existing accounts in dropdowns

### Implementation:
- [ ] Check if TransactionForm handles Type=Transfer
- [ ] Add "From Account" dropdown (source)
- [ ] Add "To Account" dropdown (destination)
- [ ] Populate dropdowns with existing accounts
- [ ] Update transaction creation logic for transfers
- [ ] Show both accounts in transaction list for transfers

---

## Files Modified So Far:

1. ? `Models/Transaction.cs` - Added PONumber field
2. ? `Services/PONumberService.cs` - Created (NEW)
3. ? `Program.cs` - Registered PONumberService
4. ? `DTOs/Dtos.cs` - Added PONumber to all DTOs

## Files Still To Modify:

### For PO Numbers:
- `Services/TransactionService.cs` - Map PONumber
- `Components/Pages/Transactions/TransactionForm.razor` - Add PO field
- `Components/Shared/GlobalSearch.razor` - Add PO search
- `Components/Pages/Forms/PrintableFormsPage.razor` - Add PO to forms

### For Global Search Fix:
- `Components/Shared/GlobalSearch.razor` - Debug and fix

### For Transfer Transactions:
- `Components/Pages/Transactions/TransactionForm.razor` - Add To/From fields
- `Services/TransactionService.cs` - Handle transfer logic

---

## Estimated Time:
- PO Numbers: 2 hours (30% done)
- Global Search Fix: 30 minutes (0% done)
- Transfer Transactions: 1 hour (0% done)

**Total:** ~3.5 hours remaining
**Completion:** 10% done

---

## Next Steps:
1. Fix Global Search (quick win)
2. Complete PO Number integration
3. Fix Transfer Transactions
4. Test everything
5. Build and verify

