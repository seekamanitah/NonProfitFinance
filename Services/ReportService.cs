using Microsoft.EntityFrameworkCore;
using NonProfitFinance.Data;
using NonProfitFinance.DTOs;
using NonProfitFinance.Models;

namespace NonProfitFinance.Services;

public class ReportService : IReportService
{
    private readonly ApplicationDbContext _context;
    private readonly IFundService _fundService;
    private const decimal AuditThreshold = 1_000_000m; // $1M threshold for Form 990 audit

    public ReportService(ApplicationDbContext context, IFundService fundService)
    {
        _context = context;
        _fundService = fundService;
    }

    public async Task<DashboardMetricsDto> GetDashboardMetricsAsync()
    {
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        var startOfYear = new DateTime(now.Year, 1, 1);

        // Batch transaction calculations in single query
        var transactionSummary = await _context.Transactions
            .Where(t => t.Date >= startOfYear)
            .GroupBy(t => new { IsCurrentMonth = t.Date >= startOfMonth, t.Type })
            .Select(g => new
            {
                g.Key.IsCurrentMonth,
                g.Key.Type,
                Total = g.Sum(t => t.Amount)
            })
            .ToListAsync();

        var monthlyIncome = transactionSummary
            .Where(x => x.IsCurrentMonth && x.Type == TransactionType.Income)
            .Sum(x => x.Total);
        var monthlyExpenses = transactionSummary
            .Where(x => x.IsCurrentMonth && x.Type == TransactionType.Expense)
            .Sum(x => x.Total);
        var ytdIncome = transactionSummary
            .Where(x => x.Type == TransactionType.Income)
            .Sum(x => x.Total);
        var ytdExpenses = transactionSummary
            .Where(x => x.Type == TransactionType.Expense)
            .Sum(x => x.Total);

        // Fund balances
        var restrictedBalance = await _fundService.GetTotalRestrictedBalanceAsync();
        var unrestrictedBalance = await _fundService.GetTotalUnrestrictedBalanceAsync();
        var totalBalance = restrictedBalance + unrestrictedBalance;

        var restrictedPercentage = totalBalance > 0
            ? (restrictedBalance / totalBalance) * 100
            : 0;

        // Batch counts in single query
        var activeGrants = await _context.Grants
            .CountAsync(g => g.Status == GrantStatus.Active);

        var activeDonors = await _context.Donors
            .CountAsync(d => d.IsActive && d.LastContributionDate >= startOfYear);

        return new DashboardMetricsDto(
            monthlyIncome - monthlyExpenses,
            ytdIncome - ytdExpenses,
            totalBalance,
            restrictedPercentage,
            unrestrictedBalance,
            restrictedBalance,
            activeGrants,
            activeDonors,
            monthlyIncome,
            monthlyExpenses
        );
    }

    public async Task<IncomeExpenseSummaryDto> GetIncomeExpenseSummaryAsync(ReportFilterRequest filter)
    {
        var incomeCategories = await GetCategorySummariesAsync(
            filter.StartDate,
            filter.EndDate,
            CategoryType.Income,
            filter.CategoryId,
            filter.FundId,
            filter.DonorId,
            filter.GrantId,
            filter.IncludeSubcategories);

        var expenseCategories = await GetCategorySummariesAsync(
            filter.StartDate,
            filter.EndDate,
            CategoryType.Expense,
            filter.CategoryId,
            filter.FundId,
            filter.DonorId,
            filter.GrantId,
            filter.IncludeSubcategories);

        var totalIncome = incomeCategories.Sum(c => c.Amount);
        var totalExpenses = expenseCategories.Sum(c => c.Amount);

        return new IncomeExpenseSummaryDto(
            filter.StartDate,
            filter.EndDate,
            totalIncome,
            totalExpenses,
            totalIncome - totalExpenses,
            incomeCategories,
            expenseCategories
        );
    }

    public async Task<List<CategorySummaryDto>> GetCategoryBreakdownAsync(ReportFilterRequest filter)
    {
        var categoryType = filter.CategoryId.HasValue
            ? (await _context.Categories.FindAsync(filter.CategoryId.Value))?.Type ?? CategoryType.Expense
            : CategoryType.Expense;

        return await GetCategorySummariesAsync(
            filter.StartDate,
            filter.EndDate,
            categoryType,
            filter.CategoryId,
            filter.FundId,
            filter.DonorId,
            filter.GrantId,
            filter.IncludeSubcategories);
    }

    private async Task<List<CategorySummaryDto>> GetCategorySummariesAsync(
        DateTime startDate,
        DateTime endDate,
        CategoryType type,
        int? filterCategoryId,
        int? fundId,
        int? donorId,
        int? grantId,
        bool includeSubcategories)
    {
        var query = _context.Transactions
            .Include(t => t.Category)
            .Where(t => t.Date >= startDate && t.Date <= endDate)
            .Where(t => t.Category!.Type == type);

        if (fundId.HasValue)
            query = query.Where(t => t.FundId == fundId.Value);
        
        if (donorId.HasValue)
            query = query.Where(t => t.DonorId == donorId.Value);
        
        if (grantId.HasValue)
            query = query.Where(t => t.GrantId == grantId.Value);

        if (filterCategoryId.HasValue)
        {
            if (includeSubcategories)
            {
                var categoryIds = await GetCategoryAndDescendantIdsAsync(filterCategoryId.Value);
                query = query.Where(t => categoryIds.Contains(t.CategoryId));
            }
            else
            {
                query = query.Where(t => t.CategoryId == filterCategoryId.Value);
            }
        }

        var transactions = await query.ToListAsync();

        // Group by top-level category
        var topLevelCategories = await _context.Categories
            .Where(c => c.Type == type && c.ParentId == null && !c.IsArchived)
            .ToListAsync();

        var allCategories = await _context.Categories
            .Where(c => c.Type == type && !c.IsArchived)
            .ToListAsync();

        var total = transactions.Sum(t => t.Amount);
        var summaries = new List<CategorySummaryDto>();

        foreach (var topCategory in topLevelCategories)
        {
            var categoryIds = GetDescendantIds(topCategory.Id, allCategories);
            categoryIds.Add(topCategory.Id);

            var categoryAmount = transactions
                .Where(t => categoryIds.Contains(t.CategoryId))
                .Sum(t => t.Amount);

            if (categoryAmount > 0)
            {
                var subcategories = includeSubcategories
                    ? GetSubcategorySummaries(topCategory.Id, allCategories, transactions, categoryAmount)
                    : null;

                summaries.Add(new CategorySummaryDto(
                    topCategory.Id,
                    topCategory.Name,
                    topCategory.Color,
                    categoryAmount,
                    total > 0 ? (categoryAmount / total) * 100 : 0,
                    subcategories?.Count > 0 ? subcategories : null
                ));
            }
        }

        return summaries.OrderByDescending(s => s.Amount).ToList();
    }

    private List<CategorySummaryDto> GetSubcategorySummaries(
        int parentId,
        List<Category> allCategories,
        List<Transaction> transactions,
        decimal parentTotal)
    {
        var children = allCategories.Where(c => c.ParentId == parentId).ToList();
        var summaries = new List<CategorySummaryDto>();

        foreach (var child in children)
        {
            var childIds = GetDescendantIds(child.Id, allCategories);
            childIds.Add(child.Id);

            var amount = transactions
                .Where(t => childIds.Contains(t.CategoryId))
                .Sum(t => t.Amount);

            if (amount > 0)
            {
                summaries.Add(new CategorySummaryDto(
                    child.Id,
                    child.Name,
                    child.Color,
                    amount,
                    parentTotal > 0 ? (amount / parentTotal) * 100 : 0,
                    null
                ));
            }
        }

        return summaries.OrderByDescending(s => s.Amount).ToList();
    }

    private async Task<List<int>> GetCategoryAndDescendantIdsAsync(int categoryId)
    {
        var allCategories = await _context.Categories.ToListAsync();
        var ids = GetDescendantIds(categoryId, allCategories);
        ids.Add(categoryId);
        return ids;
    }

    private static List<int> GetDescendantIds(int parentId, List<Category> allCategories)
    {
        var descendants = new List<int>();
        var children = allCategories.Where(c => c.ParentId == parentId).ToList();

        foreach (var child in children)
        {
            descendants.Add(child.Id);
            descendants.AddRange(GetDescendantIds(child.Id, allCategories));
        }

        return descendants;
    }

    public async Task<List<TrendDataDto>> GetTrendDataAsync(DateTime startDate, DateTime endDate, string interval = "monthly")
    {
        var transactions = await _context.Transactions
            .Where(t => t.Date >= startDate && t.Date <= endDate)
            .ToListAsync();

        var periods = GeneratePeriods(startDate, endDate, interval);
        var trends = new List<TrendDataDto>();

        foreach (var period in periods)
        {
            var periodTransactions = transactions
                .Where(t => t.Date >= period.Start && t.Date < period.End)
                .ToList();

            var income = periodTransactions
                .Where(t => t.Type == TransactionType.Income)
                .Sum(t => t.Amount);

            var expenses = periodTransactions
                .Where(t => t.Type == TransactionType.Expense)
                .Sum(t => t.Amount);

            trends.Add(new TrendDataDto(
                period.Start,
                income,
                expenses,
                income - expenses
            ));
        }

        return trends;
    }

    private static List<(DateTime Start, DateTime End)> GeneratePeriods(DateTime startDate, DateTime endDate, string interval)
    {
        var periods = new List<(DateTime Start, DateTime End)>();
        var current = startDate;

        while (current < endDate)
        {
            var next = interval.ToLower() switch
            {
                "daily" => current.AddDays(1),
                "weekly" => current.AddDays(7),
                "monthly" => current.AddMonths(1),
                "quarterly" => current.AddMonths(3),
                "yearly" => current.AddYears(1),
                _ => current.AddMonths(1)
            };

            periods.Add((current, next > endDate ? endDate : next));
            current = next;
        }

        return periods;
    }

    public async Task<decimal> GetYtdRevenueAsync()
    {
        var startOfYear = new DateTime(DateTime.UtcNow.Year, 1, 1);

        return await _context.Transactions
            .Where(t => t.Date >= startOfYear && t.Type == TransactionType.Income)
            .SumAsync(t => t.Amount);
    }

    public async Task<bool> IsApproachingAuditThresholdAsync()
    {
        var ytdRevenue = await GetYtdRevenueAsync();
        return ytdRevenue >= AuditThreshold * 0.8m; // Alert at 80% of threshold
    }
}
