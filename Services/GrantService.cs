using Microsoft.EntityFrameworkCore;
using NonProfitFinance.Data;
using NonProfitFinance.DTOs;
using NonProfitFinance.Models;

namespace NonProfitFinance.Services;

public class GrantService : IGrantService
{
    private readonly ApplicationDbContext _context;
    private readonly ITransactionService _transactionService;

    public GrantService(ApplicationDbContext context, ITransactionService transactionService)
    {
        _context = context;
        _transactionService = transactionService;
    }

    public async Task<List<GrantDto>> GetAllAsync(GrantStatus? status = null)
    {
        var query = _context.Grants.AsQueryable();

        if (status.HasValue)
            query = query.Where(g => g.Status == status.Value);

        var grants = await query
            .OrderByDescending(g => g.StartDate)
            .ToListAsync();

        return grants.Select(MapToDto).ToList();
    }

    public async Task<GrantDto?> GetByIdAsync(int id)
    {
        var grant = await _context.Grants.FindAsync(id);
        return grant == null ? null : MapToDto(grant);
    }

    public async Task<GrantDto> CreateAsync(CreateGrantRequest request)
    {
        var grant = new Grant
        {
            Name = request.Name,
            GrantorName = request.GrantorName,
            Amount = request.Amount,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            ApplicationDate = request.ApplicationDate,
            Status = request.Status,
            Restrictions = request.Restrictions,
            Notes = request.Notes,
            GrantNumber = request.GrantNumber,
            ContactPerson = request.ContactPerson,
            ContactEmail = request.ContactEmail,
            ReportingRequirements = request.ReportingRequirements,
            NextReportDueDate = request.NextReportDueDate
        };

        _context.Grants.Add(grant);
        await _context.SaveChangesAsync();

        return MapToDto(grant);
    }

    public async Task<GrantDto?> UpdateAsync(int id, UpdateGrantRequest request)
    {
        var grant = await _context.Grants.FindAsync(id);
        if (grant == null) return null;

        grant.Name = request.Name;
        grant.GrantorName = request.GrantorName;
        grant.Amount = request.Amount;
        grant.StartDate = request.StartDate;
        grant.EndDate = request.EndDate;
        grant.Status = request.Status;
        grant.Restrictions = request.Restrictions;
        grant.Notes = request.Notes;
        grant.GrantNumber = request.GrantNumber;
        grant.ContactPerson = request.ContactPerson;
        grant.ContactEmail = request.ContactEmail;
        grant.ReportingRequirements = request.ReportingRequirements;
        grant.NextReportDueDate = request.NextReportDueDate;
        grant.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToDto(grant);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var grant = await _context.Grants.FindAsync(id);
        if (grant == null) return false;

        var hasTransactions = await _context.Transactions.AnyAsync(t => t.GrantId == id);
        if (hasTransactions)
            throw new InvalidOperationException("Cannot delete grant with associated transactions.");

        _context.Grants.Remove(grant);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<TransactionDto>> GetUsageHistoryAsync(int grantId)
    {
        return await _transactionService.GetByGrantAsync(grantId);
    }

    public async Task<List<GrantDto>> GetExpiringGrantsAsync(int daysAhead = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(daysAhead);

        var grants = await _context.Grants
            .Where(g => g.Status == GrantStatus.Active &&
                        g.EndDate.HasValue &&
                        g.EndDate.Value <= cutoffDate)
            .OrderBy(g => g.EndDate)
            .ToListAsync();

        return grants.Select(MapToDto).ToList();
    }

    public async Task<List<GrantDto>> GetGrantsWithUpcomingReportsAsync(int daysAhead = 14)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(daysAhead);

        var grants = await _context.Grants
            .Where(g => g.Status == GrantStatus.Active &&
                        g.NextReportDueDate.HasValue &&
                        g.NextReportDueDate.Value <= cutoffDate)
            .OrderBy(g => g.NextReportDueDate)
            .ToListAsync();

        return grants.Select(MapToDto).ToList();
    }

    private static GrantDto MapToDto(Grant grant)
    {
        return new GrantDto(
            grant.Id,
            grant.Name,
            grant.GrantorName,
            grant.Amount,
            grant.AmountUsed,
            grant.RemainingBalance,
            grant.StartDate,
            grant.EndDate,
            grant.ApplicationDate,
            grant.Status,
            grant.Restrictions,
            grant.Notes,
            grant.GrantNumber,
            grant.ContactPerson,
            grant.ContactEmail,
            grant.ReportingRequirements,
            grant.NextReportDueDate
        );
    }
}
