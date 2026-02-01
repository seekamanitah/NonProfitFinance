# ?? Transaction Transfer Feature - Fix Plan

**Issue ID:** TRANSFER-001  
**Priority:** HIGH  
**Module:** Financial Transactions  
**Discovered:** During inventory audit pre-check  
**Status:** ?? NOT IMPLEMENTED

---

## ?? Problem Statement

The **Transfer** transaction type tab exists in the UI but doesn't properly handle account-to-account transfers:

### Current Behavior (WRONG):
- User clicks "Transfer" tab
- Form shows single "Account" field
- User can't specify FROM and TO accounts
- Transfer doesn't create proper paired transactions

### Expected Behavior (CORRECT):
- User clicks "Transfer" tab
- Form shows "From Account" and "To Account" fields
- System creates TWO linked transactions:
  - Transaction 1: Expense from source account
  - Transaction 2: Income to destination account
- Both transactions linked via TransferPairId

---

## ?? Requirements

### UI Requirements:
1. When Transfer type selected:
   - Show "From Account" dropdown (required)
   - Show "To Account" dropdown (required)
   - Disable "Category" (or auto-set to "Transfer" category)
   - Disable "Donor" and "Grant" fields
   - Amount applies to both sides of transfer
   - Description applies to both

2. Validation:
   - From and To accounts must be different
   - Both accounts must be selected
   - Amount must be positive

### Backend Requirements:
1. Create TWO transactions on save:
   ```
   Transaction 1 (Debit/Expense):
   - Type: Expense
   - Amount: [amount]
   - FundId: [FromAccountId]
   - CategoryId: [Transfer Category ID]
   - TransferPairId: [guid]
   
   Transaction 2 (Credit/Income):
   - Type: Income
   - Amount: [amount]
   - FundId: [ToAccountId]
   - CategoryId: [Transfer Category ID]
   - TransferPairId: [guid] (same as Transaction 1)
   ```

2. Database:
   - Add `TransferPairId` (Guid?) to Transaction table
   - Add `ToFundId` (int?) to Transaction table for transfers
   - Create "Transfer" category in seed data

---

## ?? Implementation Steps

### Step 1: Database Migration (15 min)

**1.1 Add columns to Transaction table:**
```csharp
// In ApplicationDbContext.cs OnModelCreating or new migration
entity.Property(t => t.TransferPairId);
entity.Property(t => t.ToFundId);

entity.HasOne(t => t.ToFund)
    .WithMany()
    .HasForeignKey(t => t.ToFundId)
    .OnDelete(DeleteBehavior.Restrict);
```

**1.2 Create migration:**
```powershell
dotnet ef migrations add AddTransferFields
dotnet ef database update
```

**1.3 Update model:**
```csharp
// Models/Transaction.cs
public Guid? TransferPairId { get; set; }
public int? ToFundId { get; set; }
public virtual Fund? ToFund { get; set; }
```

---

### Step 2: Seed Transfer Category (5 min)

**Update DataSeeder.cs:**
```csharp
var transferCategory = new Category
{
    Name = "Transfer",
    Type = CategoryType.Both, // or create special type
    Description = "Internal transfers between accounts",
    Color = "#6B7280", // Gray color
    Icon = "exchange-alt"
};
```

---

### Step 3: Update DTO (10 min)

**DTOs/Dtos.cs:**
```csharp
public record TransactionDto(
    // ... existing fields ...
    int? ToFundId,
    string? ToFundName,
    Guid? TransferPairId
);

public record CreateTransactionRequest(
    // ... existing fields ...
    int? ToFundId, // For transfers
    bool IsTransfer
);
```

---

### Step 4: Update Service (30 min)

**Services/TransactionService.cs:**

```csharp
public async Task<TransactionDto> CreateAsync(CreateTransactionRequest request)
{
    if (request.Type == TransactionType.Transfer)
    {
        return await CreateTransferAsync(request);
    }
    
    // Existing single transaction logic
    // ...
}

private async Task<TransactionDto> CreateTransferAsync(CreateTransactionRequest request)
{
    // Validation
    if (!request.FundId.HasValue || !request.ToFundId.HasValue)
        throw new ValidationException("Both From and To accounts required for transfers");
    
    if (request.FundId == request.ToFundId)
        throw new ValidationException("Cannot transfer to the same account");
    
    var transferPairId = Guid.NewGuid();
    var transferCategory = await _context.Categories
        .FirstOrDefaultAsync(c => c.Name == "Transfer");
    
    if (transferCategory == null)
        throw new InvalidOperationException("Transfer category not found");
    
    // Transaction 1: Debit from source (Expense)
    var expenseTransaction = new Transaction
    {
        Type = TransactionType.Expense,
        Amount = request.Amount,
        Date = request.Date,
        Description = $"Transfer to {request.ToFundName}",
        CategoryId = transferCategory.Id,
        FundId = request.FundId,
        ToFundId = request.ToFundId,
        TransferPairId = transferPairId,
        ReferenceNumber = request.ReferenceNumber,
        FundType = FundType.Unrestricted
    };
    
    // Transaction 2: Credit to destination (Income)
    var incomeTransaction = new Transaction
    {
        Type = TransactionType.Income,
        Amount = request.Amount,
        Date = request.Date,
        Description = $"Transfer from {request.FromFundName}",
        CategoryId = transferCategory.Id,
        FundId = request.ToFundId,
        ToFundId = request.FundId,
        TransferPairId = transferPairId,
        ReferenceNumber = request.ReferenceNumber,
        FundType = FundType.Unrestricted
    };
    
    _context.Transactions.AddRange(expenseTransaction, incomeTransaction);
    await _context.SaveChangesAsync();
    
    // Return the expense transaction (source side)
    return await GetByIdAsync(expenseTransaction.Id);
}
```

---

### Step 5: Update UI Form (45 min)

**Components/Pages/Transactions/TransactionForm.razor:**

**5.1 Add state variables:**
```csharp
private int? toAccountId;
private bool isTransfer => model.Type == TransactionType.Transfer;
```

**5.2 Conditional rendering for Account/Transfer fields:**
```razor
@if (model.Type == TransactionType.Transfer)
{
    <!-- Transfer-specific fields -->
    <div class="form-row">
        <div class="form-group">
            <label class="form-label">From Account *</label>
            <SearchableSelect TItem="FundDto" TValue="int?"
                            Items="Funds"
                            @bind-SelectedValue="model.FundId"
                            ValueSelector="@(f => (int?)f.Id)"
                            TextSelector="@(f => f.Name)"
                            Placeholder="Select source account..."
                            NullOptionText="Select account..." />
            <ValidationMessage For="@(() => model.FundId)" />
        </div>
        
        <div class="form-group">
            <label class="form-label">To Account *</label>
            <SearchableSelect TItem="FundDto" TValue="int?"
                            Items="Funds"
                            @bind-SelectedValue="toAccountId"
                            ValueSelector="@(f => (int?)f.Id)"
                            TextSelector="@(f => f.Name)"
                            Placeholder="Select destination account..."
                            NullOptionText="Select account..." />
            <ValidationMessage For="@(() => toAccountId)" />
        </div>
    </div>
    
    <div class="alert alert-info">
        <i class="fas fa-info-circle"></i>
        <strong>Transfer Note:</strong> This will create two linked transactions: 
        one withdrawal from the source account and one deposit to the destination account.
    </div>
}
else
{
    <!-- Normal single account field for Income/Expense -->
    <div class="form-row">
        <div class="form-group">
            <label class="form-label">Category *</label>
            <SearchableSelect TItem="CategoryDto" TValue="int"
                            Items="FilteredCategories"
                            @bind-SelectedValue="model.CategoryId"
                            ... />
        </div>
        
        <div class="form-group">
            <label class="form-label">Account</label>
            <SearchableSelect TItem="FundDto" TValue="int?"
                            Items="Funds"
                            @bind-SelectedValue="model.FundId"
                            ... />
        </div>
    </div>
}
```

**5.3 Hide irrelevant fields for transfers:**
```razor
@if (model.Type != TransactionType.Transfer)
{
    <!-- Donor field (Income only) -->
    <!-- Grant field -->
    <!-- Other fields not relevant to transfers -->
}
```

**5.4 Validation in submit:**
```csharp
private async Task HandleSubmit()
{
    if (model.Type == TransactionType.Transfer)
    {
        if (!model.FundId.HasValue || !toAccountId.HasValue)
        {
            errorMessage = "Both From and To accounts are required for transfers";
            return;
        }
        
        if (model.FundId == toAccountId)
        {
            errorMessage = "Cannot transfer to the same account";
            return;
        }
        
        // Set ToFundId for transfer
        model.ToFundId = toAccountId;
        model.IsTransfer = true;
    }
    
    // Call service
    await TransactionService.CreateAsync(model);
}
```

---

### Step 6: Update Transaction List Display (20 min)

**Components/Pages/Transactions/TransactionList.razor:**

Show transfer pairs together:
```razor
@if (transaction.TransferPairId.HasValue)
{
    <div class="transaction-transfer-pair">
        <i class="fas fa-exchange-alt"></i>
        <strong>Transfer:</strong>
        @transaction.FundName ? @transaction.ToFundName
        <span class="badge badge-secondary">Paired</span>
    </div>
}
```

---

### Step 7: Reports & Analytics (15 min)

**Update Report Service:**
- Exclude internal transfers from income/expense totals (optional)
- OR include them with special indicator
- Cash flow report should net out transfers

---

## ? Testing Checklist

### Unit Tests:
- [ ] Create transfer with valid accounts
- [ ] Reject transfer with same from/to account
- [ ] Reject transfer without from account
- [ ] Reject transfer without to account
- [ ] Verify two transactions created
- [ ] Verify TransferPairId matches
- [ ] Verify amounts match on both sides

### UI Tests:
- [ ] Transfer tab displays from/to fields
- [ ] Cannot select same account twice
- [ ] Validation messages show correctly
- [ ] Transfer creates paired transactions
- [ ] Transaction list shows transfer pairs
- [ ] Reports handle transfers correctly

### Edge Cases:
- [ ] Transfer to restricted fund
- [ ] Transfer large amount
- [ ] Transfer with zero balance in source
- [ ] Delete one side of transfer (should warn/prevent)
- [ ] Edit transfer (should update both sides or warn)

---

## ?? Potential Gotchas

1. **Deleting Transfers:** Must handle both transactions
2. **Editing Transfers:** Should update both or prevent editing
3. **Account Balance:** Ensure source has sufficient funds
4. **Reporting:** Don't double-count transfers in net income/expense
5. **Budget Impact:** Transfers shouldn't affect budget vs actual
6. **Fund Restrictions:** Validate transfer between restricted/unrestricted

---

## ?? Estimated Time

| Task | Time |
|------|------|
| Database migration | 15 min |
| Seed data | 5 min |
| DTOs | 10 min |
| Service logic | 30 min |
| UI form changes | 45 min |
| List display | 20 min |
| Reports update | 15 min |
| Testing | 60 min |
| **TOTAL** | **~3 hours** |

---

## ?? Acceptance Criteria

**Feature is complete when:**

1. ? User can select different from/to accounts on transfer tab
2. ? System creates two linked transactions
3. ? Both transactions have matching TransferPairId
4. ? Transaction list shows transfers clearly
5. ? Reports don't double-count transfers
6. ? Cannot delete one side without the other
7. ? All validations working
8. ? Tests pass

---

## ?? Related Issues

- None currently, but may discover during inventory audit:
  - Similar transfer needs for inventory between locations
  - Budget transfers between categories

---

**Fix Plan Created:** 2026-01-29  
**Priority:** HIGH (but after inventory audit)  
**Assigned To:** TBD  
**Target:** Next sprint after inventory module stable
