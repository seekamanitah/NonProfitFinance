namespace NonProfitFinance.Models;

/// <summary>
/// Represents a split portion of a transaction across multiple categories.
/// Used when a single transaction needs to be allocated to different categories.
/// </summary>
public class TransactionSplit
{
    public int Id { get; set; }

    /// <summary>
    /// Parent transaction that this split belongs to.
    /// </summary>
    public int TransactionId { get; set; }
    public Transaction? Transaction { get; set; }

    /// <summary>
    /// Category for this portion of the split.
    /// </summary>
    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    /// <summary>
    /// Amount allocated to this category.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Optional description specific to this split portion.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Percentage of total (for display purposes, calculated from amounts).
    /// </summary>
    public decimal? Percentage { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
