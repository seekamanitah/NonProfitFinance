using Microsoft.EntityFrameworkCore;
using NonProfitFinance.Data;
using NonProfitFinance.DTOs;
using NonProfitFinance.Models;

namespace NonProfitFinance.Services;

public class BudgetService : IBudgetService
{
    private readonly ApplicationDbContext _context;

    public BudgetService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<BudgetDto>> GetAllAsync()
    {
        // Since we don't have a Budget table yet, we'll generate budget data from categories
        var currentYear = DateTime.Today.Year;
        var budget = await GetBudgetVsActualAsync(currentYear);
        return new List<BudgetDto> { budget };
    }

    public async Task<BudgetDto?> GetByIdAsync(int id)
    {
        return await GetBudgetVsActualAsync(DateTime.Today.Year);
    }

    public async Task<BudgetDto?> GetByYearAsync(int fiscalYear)
    {
        return await GetBudgetVsActualAsync(fiscalYear);
    }

    public Task<BudgetDto> CreateAsync(CreateBudgetRequest request)
    {
        // In a full implementation, this would save to a Budget table
        throw new NotImplementedException("Budget creation requires Budget table setup");
    }

    public Task<BudgetDto?> UpdateAsync(int id, UpdateBudgetRequest request)
    {
        throw new NotImplementedException("Budget update requires Budget table setup");
    }

    public Task<bool> DeleteAsync(int id)
    {
        throw new NotImplementedException("Budget deletion requires Budget table setup");
    }

    public async Task<BudgetDto> GetBudgetVsActualAsync(int fiscalYear)
    {
        var startDate = new DateTime(fiscalYear, 1, 1);
        var endDate = new DateTime(fiscalYear, 12, 31);

        // Get categories with budget limits (expense categories)
        var categories = await _context.Categories
            .Where(c => c.Type == CategoryType.Expense && c.BudgetLimit.HasValue && c.BudgetLimit > 0)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();

        // Get actual spending by category
        var actualByCategory = await _context.Transactions
            .Where(t => t.Type == TransactionType.Expense && t.Date >= startDate && t.Date <= endDate)
            .GroupBy(t => t.CategoryId)
            .Select(g => new { CategoryId = g.Key, Amount = g.Sum(t => t.Amount) })
            .ToDictionaryAsync(x => x.CategoryId, x => x.Amount);

        var lineItems = categories.Select(c =>
        {
            var budgetAmount = c.BudgetLimit ?? 0;
            var actualAmount = actualByCategory.GetValueOrDefault(c.Id, 0);
            var variance = budgetAmount - actualAmount;
            var percentUsed = budgetAmount > 0 ? (actualAmount / budgetAmount) * 100 : 0;

            return new BudgetLineItemDto(
                c.Id,
                c.Id,
                c.Name,
                c.Color,
                budgetAmount,
                actualAmount,
                variance,
                percentUsed
            );
        }).ToList();

        var totalBudget = lineItems.Sum(l => l.BudgetAmount);
        var totalSpent = lineItems.Sum(l => l.ActualAmount);
        var remaining = totalBudget - totalSpent;
        var percentageUsed = totalBudget > 0 ? (totalSpent / totalBudget) * 100 : 0;

        return new BudgetDto(
            fiscalYear,
            $"FY {fiscalYear} Budget",
            fiscalYear,
            totalBudget,
            totalSpent,
            remaining,
            percentageUsed,
            lineItems,
            true
        );
    }
}
