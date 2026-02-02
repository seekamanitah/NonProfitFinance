using Microsoft.EntityFrameworkCore;
using NonProfitFinance.Data;
using NonProfitFinance.DTOs;
using NonProfitFinance.Models;

namespace NonProfitFinance.Services;

public class FundService : IFundService
{
    private readonly ApplicationDbContext _context;

    public FundService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<FundDto>> GetAllAsync(bool includeInactive = false)
    {
        var query = _context.Funds.AsQueryable();

        if (!includeInactive)
            query = query.Where(f => f.IsActive);

        var funds = await query
            .OrderBy(f => f.Name)
            .ToListAsync();

        return funds.Select(MapToDto).ToList();
    }

    public async Task<FundDto?> GetByIdAsync(int id)
    {
        var fund = await _context.Funds.FindAsync(id);
        return fund == null ? null : MapToDto(fund);
    }

    public async Task<FundDto> CreateAsync(CreateFundRequest request)
    {
        var fund = new Fund
        {
            Name = request.Name,
            Type = request.Type,
            Description = request.Description,
            StartingBalance = request.StartingBalance,
            Balance = request.StartingBalance, // Initial balance = starting balance
            TargetBalance = request.TargetBalance,
            RestrictionExpiryDate = request.RestrictionExpiryDate
        };

        _context.Funds.Add(fund);
        await _context.SaveChangesAsync();

        return MapToDto(fund);
    }

    public async Task<FundDto?> UpdateAsync(int id, UpdateFundRequest request)
    {
        var fund = await _context.Funds.FindAsync(id);
        if (fund == null) return null;

        fund.Name = request.Name;
        fund.Type = request.Type;
        fund.Description = request.Description;
        fund.TargetBalance = request.TargetBalance;
        fund.IsActive = request.IsActive;
        fund.RestrictionExpiryDate = request.RestrictionExpiryDate;
        fund.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToDto(fund);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var fund = await _context.Funds.FindAsync(id);
        if (fund == null) return false;

        var hasTransactions = await _context.Transactions.AnyAsync(t => t.FundId == id);
        if (hasTransactions)
            throw new InvalidOperationException("Cannot delete fund with associated transactions. Deactivate instead.");

        _context.Funds.Remove(fund);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task RecalculateAllBalancesAsync()
    {
        var funds = await _context.Funds.ToListAsync();

        foreach (var fund in funds)
        {
            var income = await _context.Transactions
                .Where(t => t.FundId == fund.Id && t.Type == TransactionType.Income)
                .SumAsync(t => t.Amount);

            var expenses = await _context.Transactions
                .Where(t => t.FundId == fund.Id && t.Type == TransactionType.Expense)
                .SumAsync(t => t.Amount);

            // Balance = Starting Balance + Income - Expenses
            fund.Balance = fund.StartingBalance + income - expenses;
            fund.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<decimal> GetTotalRestrictedBalanceAsync()
    {
        return await _context.Funds
            .Where(f => f.IsActive && f.Type != FundType.Unrestricted)
            .SumAsync(f => f.Balance);
    }

    public async Task<decimal> GetTotalUnrestrictedBalanceAsync()
    {
        return await _context.Funds
            .Where(f => f.IsActive && f.Type == FundType.Unrestricted)
            .SumAsync(f => f.Balance);
    }

    private static FundDto MapToDto(Fund fund)
    {
        return new FundDto(
            fund.Id,
            fund.Name,
            fund.Type,
            fund.StartingBalance,
            fund.Balance,
            fund.Description,
            fund.TargetBalance,
            fund.IsActive,
            fund.RestrictionExpiryDate
        );
    }
}
