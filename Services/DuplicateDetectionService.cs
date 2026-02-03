using Microsoft.EntityFrameworkCore;
using NonProfitFinance.Data;
using NonProfitFinance.DTOs;
using NonProfitFinance.Models;

namespace NonProfitFinance.Services;

public class DuplicateDetectionService : IDuplicateDetectionService
{
    private readonly ApplicationDbContext _context;
    private readonly ITransactionService _transactionService;
    private readonly ILogger<DuplicateDetectionService> _logger;
    
    // In-memory storage for dismissed pairs (in production, this would be in the database)
    private static readonly HashSet<string> _dismissedPairs = new();

    public DuplicateDetectionService(
        ApplicationDbContext context,
        ITransactionService transactionService,
        ILogger<DuplicateDetectionService> logger)
    {
        _context = context;
        _transactionService = transactionService;
        _logger = logger;
    }

    public async Task<List<DuplicateTransactionMatch>> FindDuplicatesAsync(DuplicateSearchCriteria? criteria = null)
    {
        criteria ??= new DuplicateSearchCriteria();
        var duplicates = new List<DuplicateTransactionMatch>();
        
        try
        {
            // Get transactions within date range
            var query = _context.Transactions
                .Include(t => t.Category)
                .Include(t => t.Fund)
                .Include(t => t.Donor)
                .Include(t => t.Grant)
                .Where(t => !t.IsDeleted);

            if (criteria.StartDate.HasValue)
                query = query.Where(t => t.Date >= criteria.StartDate.Value);
            if (criteria.EndDate.HasValue)
                query = query.Where(t => t.Date <= criteria.EndDate.Value);

            var transactions = await query
                .OrderBy(t => t.Date)
                .ThenBy(t => t.Amount)
                .ToListAsync();

            // Group by potential duplicates: same amount, similar date
            var potentialDuplicates = new Dictionary<string, List<Transaction>>();

            foreach (var tx in transactions)
            {
                // Create a grouping key based on amount (rounded) and approximate date
                var amountKey = Math.Round(tx.Amount, 2).ToString("F2");
                var dateKey = tx.Date.ToString("yyyy-MM");
                var typeKey = tx.Type.ToString();
                var key = $"{amountKey}|{dateKey}|{typeKey}";

                if (!potentialDuplicates.ContainsKey(key))
                    potentialDuplicates[key] = new List<Transaction>();
                
                potentialDuplicates[key].Add(tx);
            }

            // Analyze groups with more than one transaction
            foreach (var group in potentialDuplicates.Values.Where(g => g.Count > 1))
            {
                for (int i = 0; i < group.Count; i++)
                {
                    for (int j = i + 1; j < group.Count; j++)
                    {
                        var tx1 = group[i];
                        var tx2 = group[j];

                        // Skip if already dismissed
                        if (await IsDismissedPairAsync(tx1.Id, tx2.Id))
                            continue;

                        var match = AnalyzeMatch(tx1, tx2, criteria);
                        
                        if (match != null && match.MatchType <= criteria.MinimumMatchType)
                        {
                            duplicates.Add(match);
                        }
                    }
                }
            }

            // Sort by similarity score descending
            return duplicates
                .OrderByDescending(d => d.SimilarityScore)
                .ThenBy(d => d.MatchType)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding duplicates");
            return duplicates;
        }
    }

    private DuplicateTransactionMatch? AnalyzeMatch(
        Transaction tx1, 
        Transaction tx2, 
        DuplicateSearchCriteria criteria)
    {
        var matchingCriteria = new List<string>();
        decimal score = 0;

        // Check amount match (exact or within tolerance)
        if (tx1.Amount == tx2.Amount)
        {
            matchingCriteria.Add("Exact amount match");
            score += 30;
        }
        else if (criteria.AmountTolerancePercent > 0)
        {
            var tolerance = tx1.Amount * (criteria.AmountTolerancePercent / 100);
            if (Math.Abs(tx1.Amount - tx2.Amount) <= tolerance)
            {
                matchingCriteria.Add($"Amount within {criteria.AmountTolerancePercent}% tolerance");
                score += 20;
            }
        }

        // Check date proximity
        var daysDiff = Math.Abs((tx1.Date - tx2.Date).Days);
        if (daysDiff == 0)
        {
            matchingCriteria.Add("Same date");
            score += 25;
        }
        else if (daysDiff <= criteria.DateRangeDays)
        {
            matchingCriteria.Add($"Dates within {daysDiff} days");
            score += 15;
        }
        else
        {
            // Dates too far apart, likely not duplicates
            return null;
        }

        // Check transaction type
        if (tx1.Type == tx2.Type)
        {
            matchingCriteria.Add("Same transaction type");
            score += 10;
        }
        else
        {
            // Different types can't be duplicates
            return null;
        }

        // Check fund match
        if (criteria.MatchAccount && tx1.FundId == tx2.FundId)
        {
            matchingCriteria.Add("Same fund");
            score += 15;
        }

        // Check payee similarity
        if (criteria.MatchPayee && !string.IsNullOrEmpty(tx1.Payee) && !string.IsNullOrEmpty(tx2.Payee))
        {
            var payeeSimilarity = CalculateStringSimilarity(tx1.Payee, tx2.Payee);
            if (payeeSimilarity >= 0.8m)
            {
                matchingCriteria.Add($"Payee similarity: {payeeSimilarity:P0}");
                score += 10 * payeeSimilarity;
            }
        }

        // Check description similarity
        if (criteria.MatchDescription && !string.IsNullOrEmpty(tx1.Description) && !string.IsNullOrEmpty(tx2.Description))
        {
            var descSimilarity = CalculateStringSimilarity(tx1.Description, tx2.Description);
            if (descSimilarity >= 0.7m)
            {
                matchingCriteria.Add($"Description similarity: {descSimilarity:P0}");
                score += 10 * descSimilarity;
            }
        }

        // Check category match
        if (criteria.MatchCategory && tx1.CategoryId == tx2.CategoryId)
        {
            matchingCriteria.Add("Same category");
            score += 5;
        }

        // Determine match type based on score
        DuplicateMatchType matchType;
        if (score >= 80)
            matchType = DuplicateMatchType.Exact;
        else if (score >= 50)
            matchType = DuplicateMatchType.Likely;
        else if (score >= 30)
            matchType = DuplicateMatchType.Possible;
        else
            return null; // Not enough similarity

        // Convert to DTOs
        var dto1 = MapToDto(tx1);
        var dto2 = MapToDto(tx2);

        return new DuplicateTransactionMatch(
            tx1.Id,
            tx2.Id,
            dto1,
            dto2,
            Math.Min(score, 100),
            matchingCriteria,
            matchType
        );
    }

    private static decimal CalculateStringSimilarity(string s1, string s2)
    {
        if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2))
            return 0;

        s1 = s1.ToLowerInvariant().Trim();
        s2 = s2.ToLowerInvariant().Trim();

        if (s1 == s2)
            return 1.0m;

        // Check if one contains the other
        if (s1.Contains(s2) || s2.Contains(s1))
            return 0.9m;

        // Calculate Levenshtein distance ratio
        var distance = LevenshteinDistance(s1, s2);
        var maxLen = Math.Max(s1.Length, s2.Length);
        
        return 1.0m - ((decimal)distance / maxLen);
    }

    private static int LevenshteinDistance(string s1, string s2)
    {
        var m = s1.Length;
        var n = s2.Length;
        var d = new int[m + 1, n + 1];

        for (int i = 0; i <= m; i++) d[i, 0] = i;
        for (int j = 0; j <= n; j++) d[0, j] = j;

        for (int i = 1; i <= m; i++)
        {
            for (int j = 1; j <= n; j++)
            {
                var cost = (s1[i - 1] == s2[j - 1]) ? 0 : 1;
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }
        }

        return d[m, n];
    }

    private static TransactionDto MapToDto(Transaction tx)
    {
        return new TransactionDto(
            tx.Id,
            tx.Date,
            tx.Amount,
            tx.Description,
            tx.Type,
            tx.CategoryId,
            tx.Category?.Name,
            tx.FundType,
            tx.FundId,
            tx.Fund?.Name,
            tx.ToFundId,
            null, // ToFundName
            tx.TransferPairId,
            tx.DonorId,
            tx.Donor?.Name,
            tx.GrantId,
            tx.Grant?.Name,
            tx.Payee,
            tx.Tags,
            tx.ReferenceNumber,
            tx.PONumber,
            tx.IsRecurring,
            tx.IsReconciled,
            null // Splits
        );
    }

    public async Task ResolveDuplicateAsync(DuplicateTransactionMatch match, DuplicateResolution resolution)
    {
        switch (resolution)
        {
            case DuplicateResolution.Keep:
            case DuplicateResolution.Dismiss:
                await DismissDuplicatePairAsync(match.Transaction1Id, match.Transaction2Id);
                break;
                
            case DuplicateResolution.Delete1:
                await _transactionService.DeleteAsync(match.Transaction1Id);
                break;
                
            case DuplicateResolution.Delete2:
                await _transactionService.DeleteAsync(match.Transaction2Id);
                break;
                
            case DuplicateResolution.MergeInto1:
                // Keep transaction 1, delete transaction 2
                await _transactionService.DeleteAsync(match.Transaction2Id);
                _logger.LogInformation("Merged transaction {Tx2} into {Tx1}", 
                    match.Transaction2Id, match.Transaction1Id);
                break;
                
            case DuplicateResolution.MergeInto2:
                // Keep transaction 2, delete transaction 1
                await _transactionService.DeleteAsync(match.Transaction1Id);
                _logger.LogInformation("Merged transaction {Tx1} into {Tx2}", 
                    match.Transaction1Id, match.Transaction2Id);
                break;
        }
    }

    public async Task<int> GetDuplicateCountAsync()
    {
        var duplicates = await FindDuplicatesAsync(new DuplicateSearchCriteria(
            DateRangeDays: 3,
            MinimumMatchType: DuplicateMatchType.Likely
        ));
        return duplicates.Count;
    }

    public Task DismissDuplicatePairAsync(int transaction1Id, int transaction2Id)
    {
        var key = GetPairKey(transaction1Id, transaction2Id);
        _dismissedPairs.Add(key);
        return Task.CompletedTask;
    }

    public Task<bool> IsDismissedPairAsync(int transaction1Id, int transaction2Id)
    {
        var key = GetPairKey(transaction1Id, transaction2Id);
        return Task.FromResult(_dismissedPairs.Contains(key));
    }

    private static string GetPairKey(int id1, int id2)
    {
        // Always use consistent ordering for the key
        return id1 < id2 ? $"{id1}|{id2}" : $"{id2}|{id1}";
    }
}
