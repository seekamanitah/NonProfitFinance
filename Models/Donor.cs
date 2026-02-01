namespace NonProfitFinance.Models;

/// <summary>
/// Represents a donor for tracking contributions and pledges.
/// </summary>
public class Donor
{
    public int Id { get; set; }

    /// <summary>
    /// Donor name (individual or organization).
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Donor type for categorization.
    /// </summary>
    public DonorType Type { get; set; } = DonorType.Individual;

    /// <summary>
    /// Email address for communication.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Phone number.
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Mailing address.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Additional notes about the donor.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Calculated total of all contributions.
    /// </summary>
    public decimal TotalContributions { get; set; } = 0;

    /// <summary>
    /// Date of first contribution.
    /// </summary>
    public DateTime? FirstContributionDate { get; set; }

    /// <summary>
    /// Date of most recent contribution.
    /// </summary>
    public DateTime? LastContributionDate { get; set; }

    /// <summary>
    /// Whether the donor wishes to remain anonymous in reports.
    /// </summary>
    public bool IsAnonymous { get; set; } = false;

    /// <summary>
    /// Whether the donor is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Transactions from this donor.
    /// </summary>
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
