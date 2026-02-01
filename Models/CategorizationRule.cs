namespace NonProfitFinance.Models;

/// <summary>
/// Auto-categorization rule for transactions
/// </summary>
public class CategorizationRule
{
    public int Id { get; set; }
    
    /// <summary>
    /// Rule name
    /// </summary>
    public string Name { get; set; } = "";
    
    /// <summary>
    /// Match type: Payee, Description, Amount
    /// </summary>
    public RuleMatchType MatchType { get; set; }
    
    /// <summary>
    /// Match pattern (text to search for)
    /// </summary>
    public string MatchPattern { get; set; } = "";
    
    /// <summary>
    /// Case sensitive matching
    /// </summary>
    public bool CaseSensitive { get; set; }
    
    /// <summary>
    /// Target category ID
    /// </summary>
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
    
    /// <summary>
    /// Rule is active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Rule priority (higher = applied first)
    /// </summary>
    public int Priority { get; set; }
    
    /// <summary>
    /// Created date
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Last updated date
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}

public enum RuleMatchType
{
    Payee,
    Description,
    AmountEquals,
    AmountGreaterThan,
    AmountLessThan
}
