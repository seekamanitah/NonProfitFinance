namespace NonProfitFinance.Models;

/// <summary>
/// Represents a fund for nonprofit fund accounting.
/// Tracks restricted vs unrestricted funds for compliance.
/// </summary>
public class Fund
{
    public int Id { get; set; }

    /// <summary>
    /// Fund name (e.g., "General Operating", "Building Fund", "Scholarship Fund").
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Fund type: Unrestricted, Restricted, TemporarilyRestricted, PermanentlyRestricted.
    /// </summary>
    public FundType Type { get; set; }

    /// <summary>
    /// Starting balance when fund was created or at fiscal year start.
    /// </summary>
    public decimal StartingBalance { get; set; } = 0;

    /// <summary>
    /// Current fund balance (calculated from starting balance + transactions).
    /// </summary>
    public decimal Balance { get; set; } = 0;

    /// <summary>
    /// Description of fund purpose and restrictions.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Target balance or goal for the fund.
    /// </summary>
    public decimal? TargetBalance { get; set; }

    /// <summary>
    /// Whether the fund is active for new transactions.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// For restricted funds: expiration date if time-bound.
    /// </summary>
    public DateTime? RestrictionExpiryDate { get; set; }

    /// <summary>
    /// Transactions associated with this fund.
    /// </summary>
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// Concurrency token for optimistic locking.
    /// </summary>
    public uint RowVersion { get; set; }
}
