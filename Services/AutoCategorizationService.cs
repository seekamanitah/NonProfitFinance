using Microsoft.EntityFrameworkCore;
using NonProfitFinance.Data;
using NonProfitFinance.Models;

namespace NonProfitFinance.Services;

public interface IAutoCategorizationService
{
    /// <summary>
    /// Get suggested category based on transaction details
    /// </summary>
    Task<int?> SuggestCategoryAsync(string? payee, string? description, decimal amount);
    
    /// <summary>
    /// Get all categorization rules
    /// </summary>
    Task<List<CategorizationRule>> GetAllRulesAsync();
    
    /// <summary>
    /// Create a new categorization rule
    /// </summary>
    Task<CategorizationRule> CreateRuleAsync(CategorizationRule rule);
    
    /// <summary>
    /// Update a categorization rule
    /// </summary>
    Task<CategorizationRule?> UpdateRuleAsync(int id, CategorizationRule rule);
    
    /// <summary>
    /// Delete a categorization rule
    /// </summary>
    Task<bool> DeleteRuleAsync(int id);
    
    /// <summary>
    /// Learn from existing transactions to create rules
    /// </summary>
    Task<int> LearnFromTransactionsAsync(int minimumOccurrences = 3);
}

public class AutoCategorizationService : IAutoCategorizationService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AutoCategorizationService> _logger;

    public AutoCategorizationService(
        ApplicationDbContext context,
        ILogger<AutoCategorizationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<int?> SuggestCategoryAsync(string? payee, string? description, decimal amount)
    {
        // Get active rules ordered by priority
        var rules = await _context.CategorizationRules
            .Where(r => r.IsActive)
            .OrderByDescending(r => r.Priority)
            .Include(r => r.Category)
            .ToListAsync();

        foreach (var rule in rules)
        {
            if (RuleMatches(rule, payee, description, amount))
            {
                _logger.LogInformation("Rule '{RuleName}' matched. Suggesting category: {Category}", 
                    rule.Name, rule.Category?.Name);
                return rule.CategoryId;
            }
        }

        // Fallback: Find most common category for this payee
        if (!string.IsNullOrWhiteSpace(payee))
        {
            var mostCommonCategory = await _context.Transactions
                .Where(t => t.Payee != null && t.Payee.Contains(payee))
                .GroupBy(t => t.CategoryId)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefaultAsync();

            if (mostCommonCategory > 0)
            {
                _logger.LogInformation("Using historical category for payee '{Payee}': {CategoryId}", 
                    payee, mostCommonCategory);
                return mostCommonCategory;
            }
        }

        return null;
    }

    private bool RuleMatches(CategorizationRule rule, string? payee, string? description, decimal amount)
    {
        var comparison = rule.CaseSensitive 
            ? StringComparison.Ordinal 
            : StringComparison.OrdinalIgnoreCase;

        return rule.MatchType switch
        {
            RuleMatchType.Payee => !string.IsNullOrWhiteSpace(payee) && 
                                   payee.Contains(rule.MatchPattern, comparison),
            
            RuleMatchType.Description => !string.IsNullOrWhiteSpace(description) && 
                                        description.Contains(rule.MatchPattern, comparison),
            
            RuleMatchType.AmountEquals => decimal.TryParse(rule.MatchPattern, out var targetAmount) && 
                                         amount == targetAmount,
            
            RuleMatchType.AmountGreaterThan => decimal.TryParse(rule.MatchPattern, out var minAmount) && 
                                              amount > minAmount,
            
            RuleMatchType.AmountLessThan => decimal.TryParse(rule.MatchPattern, out var maxAmount) && 
                                           amount < maxAmount,
            
            _ => false
        };
    }

    public async Task<List<CategorizationRule>> GetAllRulesAsync()
    {
        return await _context.CategorizationRules
            .Include(r => r.Category)
            .OrderByDescending(r => r.Priority)
            .ToListAsync();
    }

    public async Task<CategorizationRule> CreateRuleAsync(CategorizationRule rule)
    {
        rule.CreatedAt = DateTime.UtcNow;
        _context.CategorizationRules.Add(rule);
        await _context.SaveChangesAsync();
        return rule;
    }

    public async Task<CategorizationRule?> UpdateRuleAsync(int id, CategorizationRule rule)
    {
        var existing = await _context.CategorizationRules.FindAsync(id);
        if (existing == null) return null;

        existing.Name = rule.Name;
        existing.MatchType = rule.MatchType;
        existing.MatchPattern = rule.MatchPattern;
        existing.CaseSensitive = rule.CaseSensitive;
        existing.CategoryId = rule.CategoryId;
        existing.IsActive = rule.IsActive;
        existing.Priority = rule.Priority;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteRuleAsync(int id)
    {
        var rule = await _context.CategorizationRules.FindAsync(id);
        if (rule == null) return false;

        _context.CategorizationRules.Remove(rule);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> LearnFromTransactionsAsync(int minimumOccurrences = 3)
    {
        _logger.LogInformation("Learning categorization rules from existing transactions...");

        // Find common payee-category combinations
        var commonPatterns = await _context.Transactions
            .Where(t => t.Payee != null && t.CategoryId > 0)
            .GroupBy(t => new { t.Payee, t.CategoryId })
            .Where(g => g.Count() >= minimumOccurrences)
            .Select(g => new { g.Key.Payee, g.Key.CategoryId, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(50)
            .ToListAsync();

        int rulesCreated = 0;

        foreach (var pattern in commonPatterns)
        {
            // Check if rule already exists
            var exists = await _context.CategorizationRules
                .AnyAsync(r => r.MatchType == RuleMatchType.Payee && 
                              r.MatchPattern == pattern.Payee);

            if (!exists)
            {
                var rule = new CategorizationRule
                {
                    Name = $"Auto: {pattern.Payee}",
                    MatchType = RuleMatchType.Payee,
                    MatchPattern = pattern.Payee!,
                    CategoryId = pattern.CategoryId,
                    IsActive = true,
                    Priority = 50,
                    CreatedAt = DateTime.UtcNow
                };

                _context.CategorizationRules.Add(rule);
                rulesCreated++;
            }
        }

        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Created {RulesCount} new categorization rules", rulesCreated);
        return rulesCreated;
    }
}
