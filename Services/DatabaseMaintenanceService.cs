// Database Duplicate Checker and Fixer
// Add this as a maintenance utility in your app

using Microsoft.EntityFrameworkCore;
using NonProfitFinance.Data;

public class DatabaseMaintenanceService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DatabaseMaintenanceService> _logger;

    public DatabaseMaintenanceService(ApplicationDbContext context, ILogger<DatabaseMaintenanceService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<DuplicateCheckResult> CheckForDuplicatesAsync()
    {
        var result = new DuplicateCheckResult();

        // Check Categories
        var categoryDuplicates = await _context.Categories
            .GroupBy(c => c.Name.ToLower())
            .Where(g => g.Count() > 1)
            .Select(g => new DuplicateGroup
            {
                Name = g.Key,
                Count = g.Count(),
                Ids = g.Select(x => x.Id).ToList()
            })
            .ToListAsync();

        result.CategoryDuplicates = categoryDuplicates;

        // Check Funds
        var fundDuplicates = await _context.Funds
            .GroupBy(f => f.Name.ToLower())
            .Where(g => g.Count() > 1)
            .Select(g => new DuplicateGroup
            {
                Name = g.Key,
                Count = g.Count(),
                Ids = g.Select(x => x.Id).ToList()
            })
            .ToListAsync();

        result.FundDuplicates = fundDuplicates;

        // Check Donors
        var donorDuplicates = await _context.Donors
            .GroupBy(d => d.Name.ToLower())
            .Where(g => g.Count() > 1)
            .Select(g => new DuplicateGroup
            {
                Name = g.Key,
                Count = g.Count(),
                Ids = g.Select(x => x.Id).ToList()
            })
            .ToListAsync();

        result.DonorDuplicates = donorDuplicates;

        return result;
    }

    public async Task<FixResult> FixCategoryDuplicatesAsync()
    {
        var result = new FixResult();
        
        try
        {
            var duplicates = await _context.Categories
                .GroupBy(c => c.Name.ToLower())
                .Where(g => g.Count() > 1)
                .ToListAsync();

            foreach (var group in duplicates)
            {
                var categories = group.OrderBy(c => c.Id).ToList();
                var keepCategory = categories.First(); // Keep the oldest one
                var duplicatesToRemove = categories.Skip(1).ToList();

                _logger.LogInformation($"Merging duplicates for category '{keepCategory.Name}'. Keeping ID {keepCategory.Id}, removing {duplicatesToRemove.Count} duplicates.");

                // Update all transactions pointing to duplicates
                foreach (var duplicate in duplicatesToRemove)
                {
                    var transactions = await _context.Transactions
                        .Where(t => t.CategoryId == duplicate.Id)
                        .ToListAsync();

                    foreach (var transaction in transactions)
                    {
                        transaction.CategoryId = keepCategory.Id;
                        result.UpdatedTransactions++;
                    }

                    // Update budget line items
                    var budgetItems = await _context.BudgetLineItems
                        .Where(b => b.CategoryId == duplicate.Id)
                        .ToListAsync();

                    foreach (var item in budgetItems)
                    {
                        item.CategoryId = keepCategory.Id;
                        result.UpdatedBudgetItems++;
                    }

                    // Update categorization rules
                    var rules = await _context.CategorizationRules
                        .Where(r => r.CategoryId == duplicate.Id)
                        .ToListAsync();

                    foreach (var rule in rules)
                    {
                        rule.CategoryId = keepCategory.Id;
                        result.UpdatedRules++;
                    }

                    // Remove the duplicate category
                    _context.Categories.Remove(duplicate);
                    result.DeletedCategories++;
                }
            }

            await _context.SaveChangesAsync();
            result.Success = true;
            result.Message = $"Fixed {duplicates.Count} duplicate category groups";
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Message = $"Error: {ex.Message}";
            _logger.LogError(ex, "Failed to fix category duplicates");
        }

        return result;
    }
}

public class DuplicateCheckResult
{
    public List<DuplicateGroup> CategoryDuplicates { get; set; } = new();
    public List<DuplicateGroup> FundDuplicates { get; set; } = new();
    public List<DuplicateGroup> DonorDuplicates { get; set; } = new();

    public bool HasDuplicates => 
        CategoryDuplicates.Any() || FundDuplicates.Any() || DonorDuplicates.Any();
}

public class DuplicateGroup
{
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
    public List<int> Ids { get; set; } = new();
}

public class FixResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int DeletedCategories { get; set; }
    public int UpdatedTransactions { get; set; }
    public int UpdatedBudgetItems { get; set; }
    public int UpdatedRules { get; set; }
}
