# NonProfit Finance - Foundation Database Reference

## Purpose
This document serves as the authoritative reference for understanding the database structure, entity relationships, and calculation formulas used throughout the application. Use this as a baseline for debugging, planning fixes, and ensuring consistency.

---

## 1. Entity Relationship Diagram (Text)

```
┌─────────────────────────────────────────────────────────────────┐
│                         CORE ENTITIES                           │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌──────────────┐      ┌──────────────┐      ┌──────────────┐  │
│  │   Category   │      │    Fund      │      │    Donor     │  │
│  │──────────────│      │──────────────│      │──────────────│  │
│  │ Id (PK)      │      │ Id (PK)      │      │ Id (PK)      │  │
│  │ Name         │      │ Name         │      │ Name         │  │
│  │ Type         │      │ Type         │      │ TotalDonated │  │
│  │ ParentId(FK) │←─┐   │ StartingBal  │      │ LastContrib  │  │
│  │ Children     │──┘   │ Balance      │      └──────┬───────┘  │
│  └──────┬───────┘      └──────┬───────┘             │          │
│         │                     │                      │          │
│         │    ┌────────────────┼──────────────────────┤          │
│         │    │                │                      │          │
│         ▼    ▼                ▼                      ▼          │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │                     TRANSACTION                          │   │
│  │─────────────────────────────────────────────────────────│   │
│  │ Id (PK)                                                  │   │
│  │ Date          - When transaction occurred                │   │
│  │ Amount        - Transaction amount (ALWAYS POSITIVE)     │   │
│  │ Type          - Income | Expense | Transfer              │   │
│  │ Description   - User-provided memo                       │   │
│  │ CategoryId    - FK to Category (required)                │   │
│  │ FundId        - FK to Fund (optional - source fund)      │   │
│  │ ToFundId      - FK to Fund (transfers only - dest fund)  │   │
│  │ DonorId       - FK to Donor (optional)                   │   │
│  │ GrantId       - FK to Grant (optional)                   │   │
│  │ TransferPairId- Guid linking paired transfer transactions│   │
│  │ IsDeleted     - Soft delete flag                         │   │
│  └─────────────────────────────────────────────────────────┘   │
│                              │                                  │
│                              ▼                                  │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │                  TRANSACTION_SPLIT                       │   │
│  │─────────────────────────────────────────────────────────│   │
│  │ Id (PK)                                                  │   │
│  │ TransactionId - FK to Transaction                        │   │
│  │ CategoryId    - FK to Category                           │   │
│  │ Amount        - Split amount                             │   │
│  │ Description   - Optional split description               │   │
│  └─────────────────────────────────────────────────────────┘   │
│                                                                 │
│  ┌──────────────┐                                              │
│  │    Grant     │                                              │
│  │──────────────│                                              │
│  │ Id (PK)      │                                              │
│  │ Name         │                                              │
│  │ Amount       │ - Total grant amount                         │
│  │ AmountUsed   │ - CALCULATED from expense transactions       │
│  │ Status       │ - Pending|Active|Completed|Expired|Rejected  │
│  └──────────────┘                                              │
└─────────────────────────────────────────────────────────────────┘
```

---

## 2. Table Definitions

### 2.1 Transaction Table
| Column | Type | Nullable | Description |
|--------|------|----------|-------------|
| Id | int | No | Primary key (auto-increment) |
| Date | DateTime | No | Transaction date |
| Amount | decimal(18,2) | No | **ALWAYS POSITIVE** - Type determines direction |
| Description | string(500) | Yes | User memo |
| Type | enum | No | Income=0, Expense=1, Transfer=2 |
| CategoryId | int | No | FK to Categories |
| FundType | enum | No | Unrestricted=0, Restricted=1, etc. |
| FundId | int | Yes | FK to Funds (source account) |
| ToFundId | int | Yes | FK to Funds (transfer destination) |
| TransferPairId | Guid | Yes | Links paired transfer transactions |
| DonorId | int | Yes | FK to Donors |
| GrantId | int | Yes | FK to Grants |
| Payee | string(200) | Yes | Vendor/recipient name |
| Tags | string(500) | Yes | Comma-separated tags |
| ReferenceNumber | string | Yes | Check number, etc. |
| PONumber | string | Yes | Purchase order number |
| IsRecurring | bool | No | Template for recurring |
| IsReconciled | bool | No | Bank reconciliation flag |
| IsDeleted | bool | No | Soft delete flag (default filter) |
| RowVersion | uint | No | Optimistic concurrency token |

### 2.2 Fund Table (Accounts)
| Column | Type | Nullable | Description |
|--------|------|----------|-------------|
| Id | int | No | Primary key |
| Name | string(100) | No | Unique fund name |
| Type | enum | No | Unrestricted, Restricted, etc. |
| StartingBalance | decimal(18,2) | No | Opening balance |
| Balance | decimal(18,2) | No | **CALCULATED** current balance |
| Description | string(500) | Yes | Fund purpose |
| TargetBalance | decimal | Yes | Goal amount |
| IsActive | bool | No | Available for new transactions |
| RowVersion | uint | No | Concurrency token |

### 2.3 Category Table
| Column | Type | Nullable | Description |
|--------|------|----------|-------------|
| Id | int | No | Primary key |
| Name | string(100) | No | Unique within parent |
| Type | enum | No | Income=0, Expense=1 |
| ParentId | int | Yes | FK to Categories (self-reference) |
| IsArchived | bool | No | Soft archive flag |
| BudgetLimit | decimal | Yes | Monthly budget cap |
| SortOrder | int | No | Display ordering |

---

## 3. Foreign Key Relationships

### 3.1 Transaction Relationships
```
Transaction.CategoryId → Category.Id (Restrict on delete)
Transaction.FundId → Fund.Id (Set Null on delete)
Transaction.ToFundId → Fund.Id (Set Null on delete)
Transaction.DonorId → Donor.Id (Set Null on delete)
Transaction.GrantId → Grant.Id (Set Null on delete)
```

### 3.2 Category Self-Reference
```
Category.ParentId → Category.Id (Restrict on delete)
```

### 3.3 TransactionSplit Relationships
```
TransactionSplit.TransactionId → Transaction.Id (Cascade delete)
TransactionSplit.CategoryId → Category.Id (Restrict on delete)
```

---

## 4. CRITICAL: Calculation Formulas

### 4.1 Fund (Account) Balance Calculation
**Location:** `FundService.cs`, `TransactionService.UpdateFundBalanceAsync()`

```
Fund.Balance = Fund.StartingBalance 
             + SUM(Transaction.Amount WHERE FundId = Fund.Id AND Type = Income)
             - SUM(Transaction.Amount WHERE FundId = Fund.Id AND Type = Expense)
```

**IMPORTANT NOTES:**
- Transaction.Amount is **ALWAYS POSITIVE**
- Transaction.Type determines whether to ADD or SUBTRACT
- Transfers create TWO transactions: one Expense from source, one Income to destination
- Soft-deleted transactions (IsDeleted=true) are EXCLUDED by query filter

### 4.2 Grant AmountUsed Calculation
**Location:** `TransactionService.UpdateGrantUsageAsync()`

```
Grant.AmountUsed = SUM(Transaction.Amount 
                       WHERE GrantId = Grant.Id AND Type = Expense)
```

### 4.3 Donor TotalDonated Calculation
**Location:** `TransactionService.UpdateDonorTotalsAsync()`

```
Donor.TotalDonated = SUM(Transaction.Amount 
                         WHERE DonorId = Donor.Id AND Type = Income)
```

### 4.4 Dashboard Total Cash Balance
**Location:** `ReportService.GetDashboardMetricsAsync()`

```
TotalCashBalance = SUM(Fund.Balance WHERE Fund.IsActive = true)
```

Which expands to:
```
TotalCashBalance = SUM(
    Fund.StartingBalance 
    + Income Transactions 
    - Expense Transactions
) FOR ALL ACTIVE FUNDS
```

### 4.5 Dashboard YTD Net Income
```
YTD Net Income = SUM(Amount WHERE Type=Income AND Date >= Jan 1)
               - SUM(Amount WHERE Type=Expense AND Date >= Jan 1)
```

---

## 5. Transfer Transaction Logic

Transfers create **TWO linked transactions**:

### Source Transaction (Expense):
- Type = Expense
- FundId = Source Account
- ToFundId = Destination Account  
- Amount = Transfer Amount
- TransferPairId = Shared GUID
- Category = "Transfer" (auto-created)

### Destination Transaction (Income):
- Type = Income
- FundId = Destination Account
- ToFundId = null
- Amount = Transfer Amount
- TransferPairId = Same GUID as source
- Category = "Transfer" (auto-created)

**Effect on Balances:**
- Source Fund: Balance decreases by Amount (Expense)
- Destination Fund: Balance increases by Amount (Income)
- NET change across all funds = $0

---

## 6. Query Filters (Global)

### 6.1 Soft Delete Filter on Transactions
```csharp
entity.HasQueryFilter(t => !t.IsDeleted);
```

**Effect:** All queries automatically exclude deleted transactions unless `.IgnoreQueryFilters()` is used.

---

## 7. Known Issues & Discrepancies

### Issue 1: Import Type Detection
**Symptom:** Imported transactions may all be marked as same type
**Root Cause:** TypeColumn detection priority over amount sign
**Fix Location:** `ImportExportService.cs` lines 241-255

### Issue 2: Fund Balance Not Updating After Import
**Symptom:** Dashboard shows wrong balance after CSV import
**Root Cause:** `UpdateFundBalanceAsync()` may not be called after batch imports
**Fix Location:** Verify import calls `UpdateFundBalanceAsync()` for each affected fund

### Issue 3: Transfer Balance Double-Count
**Symptom:** Transfers counted as both income and expense on same fund
**Root Cause:** Transfer creates 2 transactions; if both have same FundId, doubled
**Verification:** Ensure source transaction has FundId=source, dest has FundId=dest

---

## 8. Service Responsibility Map

| Service | Responsibilities |
|---------|-----------------|
| TransactionService | Create/Update/Delete transactions, update related balances |
| FundService | CRUD funds, RecalculateAllBalances |
| CategoryService | CRUD categories, hierarchy management |
| DonorService | CRUD donors, total calculations |
| GrantService | CRUD grants, usage tracking |
| ReportService | Dashboard metrics, reports, trend data |
| ImportExportService | CSV/Excel import, column mapping |

---

## 9. Balance Update Trigger Points

Fund balances are recalculated when:

1. **Transaction Created** → `UpdateFundBalanceAsync(fundId)`
2. **Transaction Updated** → `UpdateFundBalanceAsync(oldFundId)` + `UpdateFundBalanceAsync(newFundId)`
3. **Transaction Deleted** → `UpdateFundBalanceAsync(fundId)`
4. **Fund Starting Balance Changed** → Recalculate in `UpdateAsync()`
5. **Manual Recalculation** → `RecalculateAllBalancesAsync()`

---

## 10. Debugging Checklist

When balance discrepancies occur:

1. **Check Transaction Types:** Verify Income/Expense is set correctly
2. **Check Fund Assignment:** Verify FundId is set on transactions
3. **Check Soft Deletes:** Use `.IgnoreQueryFilters()` to see deleted records
4. **Run Recalculation:** Call `FundService.RecalculateAllBalancesAsync()`
5. **Verify Import Mapping:** Check if TypeColumn is mapped or amount sign detection is used

---

## 11. Database Indexes

### Transaction Indexes (Performance)
- Date
- Type
- CategoryId
- FundId
- DonorId
- GrantId
- Payee
- ReferenceNumber
- PONumber
- (Date, Type) - Composite for reports
- (FundId, Date) - Composite for fund balance queries
- IsDeleted

### Category Indexes
- (ParentId, Name) - Unique composite

### Fund Indexes
- Name - Unique

---

## Version History
- Created: January 2025
- Purpose: Foundation reference for formula and calculation auditing
