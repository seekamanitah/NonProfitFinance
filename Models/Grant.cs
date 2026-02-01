namespace NonProfitFinance.Models;

/// <summary>
/// Represents a grant received by the nonprofit.
/// Tracks grant lifecycle, amounts, restrictions, and compliance.
/// </summary>
public class Grant
{
    public int Id { get; set; }

    /// <summary>
    /// Grant name or title.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Organization or agency providing the grant.
    /// </summary>
    public required string GrantorName { get; set; }

    /// <summary>
    /// Total grant amount awarded.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Amount of the grant already spent/utilized.
    /// </summary>
    public decimal AmountUsed { get; set; } = 0;

    /// <summary>
    /// Grant start date / award date.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Grant end date / expiration.
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Application date.
    /// </summary>
    public DateTime? ApplicationDate { get; set; }

    /// <summary>
    /// Current status in the grant lifecycle.
    /// </summary>
    public GrantStatus Status { get; set; } = GrantStatus.Pending;

    /// <summary>
    /// Description of any restrictions on fund usage.
    /// </summary>
    public string? Restrictions { get; set; }

    /// <summary>
    /// Additional notes and compliance requirements.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Grant identification number from the grantor.
    /// </summary>
    public string? GrantNumber { get; set; }

    /// <summary>
    /// Contact person at the granting organization.
    /// </summary>
    public string? ContactPerson { get; set; }

    /// <summary>
    /// Contact email for grant administration.
    /// </summary>
    public string? ContactEmail { get; set; }

    /// <summary>
    /// Reporting requirements/schedule.
    /// </summary>
    public string? ReportingRequirements { get; set; }

    /// <summary>
    /// Next report due date.
    /// </summary>
    public DateTime? NextReportDueDate { get; set; }

    /// <summary>
    /// Transactions funded by this grant.
    /// </summary>
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    /// <summary>
    /// Remaining balance available.
    /// </summary>
    public decimal RemainingBalance => Amount - AmountUsed;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// Concurrency token for optimistic locking.
    /// </summary>
    public uint RowVersion { get; set; }
}
