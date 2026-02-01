namespace NonProfitFinance.Models;

/// <summary>
/// Represents a financial transaction (income, expense, or transfer).
/// Supports splits, recurring items, fund tracking, and donor/grant associations.
/// </summary>
public class Transaction : ISoftDelete
{
    public int Id { get; set; }

    /// <summary>
    /// Date when the transaction occurred.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Transaction amount (positive for income, negative for expense, or use Type).
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Description or memo for the transaction.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Transaction type: Income, Expense, or Transfer.
    /// </summary>
    public TransactionType Type { get; set; }

    /// <summary>
    /// Primary category for the transaction.
    /// </summary>
    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    /// <summary>
    /// Fund type for nonprofit compliance (restricted/unrestricted).
    /// </summary>
    public FundType FundType { get; set; } = FundType.Unrestricted;

    /// <summary>
    /// Optional associated fund.
    /// </summary>
    public int? FundId { get; set; }
    public Fund? Fund { get; set; }

    /// <summary>
    /// For transfers: destination fund ID.
    /// </summary>
    public int? ToFundId { get; set; }
    public Fund? ToFund { get; set; }

    /// <summary>
    /// For transfers: links paired transactions together.
    /// </summary>
    public Guid? TransferPairId { get; set; }

    /// <summary>
    /// Optional associated donor (for contributions).
    /// </summary>
    public int? DonorId { get; set; }
    public Donor? Donor { get; set; }

    /// <summary>
    /// Optional associated grant.
    /// </summary>
    public int? GrantId { get; set; }
    public Grant? Grant { get; set; }

    /// <summary>
    /// Optional associated maintenance project (for project expenses).
    /// </summary>
    public int? ProjectId { get; set; }

    /// <summary>
    /// Payee or payer name.
    /// </summary>
    public string? Payee { get; set; }

    /// <summary>
    /// Comma-separated tags for additional categorization.
    /// </summary>
    public string? Tags { get; set; }

    /// <summary>
    /// Check number or reference number.
    /// </summary>
    public string? ReferenceNumber { get; set; }

    /// <summary>
    /// Purchase Order number (auto-generated or manual).
    /// </summary>
    public string? PONumber { get; set; }

    /// <summary>
    /// Path to receipt image file.
    /// </summary>
    public string? ReceiptPath { get; set; }

    /// <summary>
    /// Whether this is a recurring transaction template.
    /// </summary>
    public bool IsRecurring { get; set; } = false;

    /// <summary>
    /// Recurrence pattern (e.g., Monthly, Weekly) if recurring.
    /// </summary>
    public string? RecurrencePattern { get; set; }

    /// <summary>
    /// Next scheduled date for recurring transactions.
    /// </summary>
    public DateTime? NextRecurrenceDate { get; set; }

    /// <summary>
    /// Transaction splits for allocating across multiple categories.
    /// </summary>
    public ICollection<TransactionSplit> Splits { get; set; } = new List<TransactionSplit>();

    /// <summary>
    /// Bank sync identifier for avoiding duplicate imports.
    /// </summary>
    public string? ExternalId { get; set; }

    /// <summary>
    /// Whether this transaction has been reconciled.
    /// </summary>
    public bool IsReconciled { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// Concurrency token for optimistic locking.
    /// </summary>
    public uint RowVersion { get; set; }
    
    // ISoftDelete implementation
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}
