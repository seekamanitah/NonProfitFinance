# Formula & Calculation Issues Audit

## Summary
This document identifies all discovered issues with balance calculations, formulas, and data relationships.

**Last Updated:** Audit Session - Deep Review

---

## CRITICAL ISSUES

### Issue #1: Transactions Without FundId Are Invisible to Dashboard ✅ FIXED
**Severity:** CRITICAL
**Location:** Import + TransactionService.CreateAsync
**Status:** FIXED - Auto-assigns to "General" fund in BOTH locations

**Problem:**
When importing or creating transactions without FundId:
- Transaction is created with FundId = null
- `UpdateFundBalanceAsync()` is NOT called
- Dashboard TotalCashBalance = SUM(Fund.Balance) which only includes transactions WITH FundId
- **Result:** Transactions don't show in dashboard totals

**Fixes Applied:**
1. ImportExportService auto-creates/uses "General" fund when no fund is specified
2. TransactionService.CreateAsync now auto-assigns to "General" fund for ALL transaction creation paths
3. QuickAddModal transactions are now properly assigned (via CreateAsync fix)

---

### Issue #2: Dashboard Balance vs Transaction List Mismatch ✅ RESOLVED
**Severity:** HIGH
**Location:** Dashboard.razor, TransactionList.razor
**Status:** RESOLVED via Issue #1 fix

**Problem:**
- Dashboard shows: `TotalCashBalance = SUM(Fund.Balance)`
- TransactionList shows: All transactions regardless of FundId
- Users see transactions in list but totals don't match

**Resolution:** Now that all transactions are assigned to a fund, totals will match.

---

### Issue #3: YTD Net Income Includes All Transactions (Including Inactive Funds)
**Severity:** MEDIUM
**Location:** ReportService.cs lines 27-49
**Status:** NOT FIXED (Lower priority - net income is still correct)

**Problem:**
Dashboard YTD Net Income is calculated from ALL transactions regardless of:
- Fund assignment
- Fund.IsActive status
- This may include transactions from inactive funds

**Impact:** May show different totals than expected if some funds are inactive.

**Recommended Fix:** Filter by active funds only, or show separate totals

---

### Issue #4: Transfer Transactions Inflate Chart/Dashboard Values ✅ FIXED
**Severity:** HIGH  
**Location:** Multiple services
**Status:** FIXED

**Problem:**
Transfers create 2 transactions with Type=Income and Type=Expense:
- Both were included in Income/Expense totals on charts and dashboard
- Net income was correct (Income - Expense = 0 for transfers)
- BUT individual Income and Expense values were inflated

**Example:**
- Transfer $500 from Fund A to Fund B
- Dashboard showed: Monthly Income +$500, Monthly Expense +$500 (from the transfer alone)
- Net = $0 (correct)
- But Income/Expense totals appeared $500 higher than actual

**Fixes Applied:**
Added `t.TransferPairId == null` filter to all these queries:
- ReportService.GetDashboardMetricsAsync() 
- ReportService.GetTrendDataAsync() 
- ReportService.GetCategorySummariesAsync()
- BudgetService.GetBudgetVsActualAsync() 
- CashFlowService.AddHistoricalProjectionsAsync()
- Form990Service.GetGrossRevenueAsync()
- Form990Service.GetPartIDataAsync()
- Form990Service.GetPartVIIIDataAsync()
- Form990Service.GetPartIXDataAsync()

---

### Issue #5: FundList vs Dashboard Total Mismatch ✅ FIXED
**Severity:** MEDIUM
**Location:** FundList.razor
**Status:** FIXED

**Problem:**
- FundList.razor included inactive funds in totals
- Dashboard excludes inactive funds
- Users saw different totals on different pages

**Fix Applied:**
```csharp
var activeFunds = funds.Where(f => f.IsActive).ToList();
unrestrictedTotal = activeFunds.Where(f => f.Type == FundType.Unrestricted).Sum(f => f.Balance);
```

---

### Issue #6: DateTime Timezone Mismatch
**Severity:** LOW
**Location:** ReportService.cs vs TransactionList.razor
**Status:** NOT FIXED (Low priority)

**Problem:**
- Dashboard uses `DateTime.UtcNow` for date calculations
- TransactionList uses `DateTime.Today` for filter defaults
- Transaction dates come from user input (local time)
- May cause off-by-one-day issues at day boundaries

---

### Issue #7: Transfer Transactions Show Twice in Transaction List
**Severity:** LOW (By Design)
**Location:** TransactionList.razor
**Status:** By Design - Consider UI enhancement

**Problem:**
Transfers create 2 transactions - both show in transaction list.

**Note:** This is by design but may need UI clarification

---

### Issue #8: Delete/Restore/Update Don't Handle Paired Transfers ✅ FIXED
**Severity:** CRITICAL
**Location:** TransactionService.cs
**Status:** FIXED

**Problem:**
When deleting, restoring, or updating transfer transactions:
- Only one leg of the transfer was affected
- Paired transaction was orphaned or inconsistent
- Fund balances became incorrect

**Fixes Applied:**
1. DeleteAsync now deletes both legs of transfer
2. RestoreAsync now restores both legs
3. PermanentDeleteAsync now permanently deletes both legs
4. UpdateAsync now syncs Amount, Date, ReferenceNumber on paired transactions

---

### Issue #9: QuickAddModal Creates Transactions Without FundId ✅ FIXED
**Severity:** HIGH
**Location:** Components/Shared/QuickAddModal.razor
**Status:** FIXED (via TransactionService.CreateAsync fix)

**Problem:**
QuickAddModal line 169 passed `null` for FundId when creating transactions.
These transactions were invisible to dashboard totals.

**Fix Applied:**
TransactionService.CreateAsync now auto-assigns to "General" fund when FundId is null.
This covers QuickAddModal and ALL other transaction creation paths.

---

## FORMULA VERIFICATION CHECKLIST

### Fund Balance Formula
```
Balance = StartingBalance + Income - Expense
```
✅ Formula correct in FundService.RecalculateAllBalancesAsync()
✅ Formula correct in FundService.UpdateAsync()
✅ Formula correct in TransactionService.UpdateFundBalanceAsync()

### Grant Usage Formula
```
AmountUsed = SUM(Expense transactions for this grant)
```
✅ Formula correct in TransactionService.UpdateGrantUsageAsync()

### Donor Total Formula
```
TotalDonated = SUM(Income transactions for this donor)
```
✅ Formula correct in TransactionService.UpdateDonorTotalsAsync()

### Dashboard Metrics
```
TotalCashBalance = SUM(Fund.Balance WHERE IsActive)
```
✅ Formula correct
✅ All transactions now have FundId (via auto-assignment)

---

## FIX STATUS SUMMARY

| Issue | Status | Priority |
|-------|--------|----------|
| #1 Null FundId on Import | ✅ FIXED | CRITICAL |
| #2 Dashboard/List Mismatch | ✅ RESOLVED | HIGH |
| #3 YTD includes inactive funds | Not Fixed | MEDIUM |
| #4 Transfers inflate charts | ✅ FIXED | HIGH |
| #5 FundList inactive funds | ✅ FIXED | MEDIUM |
| #6 DateTime timezone | Not Fixed | LOW |
| #7 Transfers show twice | By Design | LOW |
| #8 Transfer Delete/Restore/Update | ✅ FIXED | CRITICAL |
| #9 QuickAddModal null FundId | ✅ FIXED | HIGH |

---

## REMAINING ISSUES (Low Priority)

### Issue #3: YTD includes inactive funds
- Dashboard YTD calculations include all transactions regardless of fund IsActive status
- Impact is low because net income is still correct
- Consider adding fund filter if needed in future

### Issue #6: DateTime timezone
- Dashboard uses DateTime.UtcNow, other pages use DateTime.Today
- May cause off-by-one-day issues at midnight boundaries
- Low impact, can be addressed in future consistency pass

---

## VERIFICATION CHECKLIST

After fixes, verify:

1. [x] Import CSV without fund column → transactions assigned to General fund
2. [x] QuickAdd transactions → assigned to General fund via CreateAsync
3. [x] Dashboard TotalCashBalance excludes transfer transactions from income/expense
4. [x] Charts don't inflate income/expense with transfer amounts
5. [x] Form 990 calculations exclude transfer transactions
6. [x] Budget actual spending excludes transfer transactions
7. [x] Cash flow projections exclude transfer transactions
8. [x] Transfer delete/restore/update handles both legs

---

## Version
Created: January 2025
Last Updated: January 2025 - All critical and high priority issues fixed
