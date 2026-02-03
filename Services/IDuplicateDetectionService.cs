using NonProfitFinance.DTOs;

namespace NonProfitFinance.Services;

/// <summary>
/// Represents a potential duplicate transaction pair with similarity analysis
/// </summary>
public record DuplicateTransactionMatch(
    int Transaction1Id,
    int Transaction2Id,
    TransactionDto Transaction1,
    TransactionDto Transaction2,
    decimal SimilarityScore,
    List<string> MatchingCriteria,
    DuplicateMatchType MatchType
);

public enum DuplicateMatchType
{
    Exact,      // All key fields match exactly
    Likely,     // High similarity (amount, date within range, similar description)
    Possible    // Some matching criteria
}

public enum DuplicateResolution
{
    Keep,       // Keep both transactions
    MergeInto1, // Merge Transaction2 into Transaction1
    MergeInto2, // Merge Transaction1 into Transaction2
    Delete1,    // Delete Transaction1
    Delete2,    // Delete Transaction2
    Dismiss     // Mark as reviewed, not duplicates
}

public interface IDuplicateDetectionService
{
    /// <summary>
    /// Scans all transactions for potential duplicates
    /// </summary>
    Task<List<DuplicateTransactionMatch>> FindDuplicatesAsync(DuplicateSearchCriteria? criteria = null);
    
    /// <summary>
    /// Resolves a duplicate match according to the specified resolution
    /// </summary>
    Task ResolveDuplicateAsync(DuplicateTransactionMatch match, DuplicateResolution resolution);
    
    /// <summary>
    /// Gets count of potential duplicates for dashboard display
    /// </summary>
    Task<int> GetDuplicateCountAsync();
    
    /// <summary>
    /// Marks a pair as reviewed (not duplicates)
    /// </summary>
    Task DismissDuplicatePairAsync(int transaction1Id, int transaction2Id);
    
    /// <summary>
    /// Checks if a specific transaction has been dismissed as duplicate with another
    /// </summary>
    Task<bool> IsDismissedPairAsync(int transaction1Id, int transaction2Id);
}

public record DuplicateSearchCriteria(
    int DateRangeDays = 3,
    decimal AmountTolerancePercent = 0m,
    bool MatchPayee = true,
    bool MatchDescription = true,
    bool MatchCategory = false,
    bool MatchAccount = true,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    DuplicateMatchType MinimumMatchType = DuplicateMatchType.Possible
);
