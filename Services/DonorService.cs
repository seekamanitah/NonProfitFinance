using Microsoft.EntityFrameworkCore;
using NonProfitFinance.Data;
using NonProfitFinance.DTOs;
using NonProfitFinance.Models;

namespace NonProfitFinance.Services;

public class DonorService : IDonorService
{
    private readonly ApplicationDbContext _context;
    private readonly ITransactionService _transactionService;

    public DonorService(ApplicationDbContext context, ITransactionService transactionService)
    {
        _context = context;
        _transactionService = transactionService;
    }

    public async Task<List<DonorDto>> GetAllAsync(bool includeInactive = false)
    {
        var query = _context.Donors.AsQueryable();

        if (!includeInactive)
            query = query.Where(d => d.IsActive);

        var donors = await query
            .OrderBy(d => d.Name)
            .ToListAsync();

        return donors.Select(MapToDto).ToList();
    }

    public async Task<DonorDto?> GetByIdAsync(int id)
    {
        var donor = await _context.Donors.FindAsync(id);
        return donor == null ? null : MapToDto(donor);
    }

    public async Task<DonorDto> CreateAsync(CreateDonorRequest request)
    {
        var donor = new Donor
        {
            Name = request.Name,
            Type = request.Type,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address,
            Notes = request.Notes,
            IsAnonymous = request.IsAnonymous
        };

        _context.Donors.Add(donor);
        await _context.SaveChangesAsync();

        return MapToDto(donor);
    }

    public async Task<DonorDto?> UpdateAsync(int id, UpdateDonorRequest request)
    {
        var donor = await _context.Donors.FindAsync(id);
        if (donor == null) return null;

        donor.Name = request.Name;
        donor.Type = request.Type;
        donor.Email = request.Email;
        donor.Phone = request.Phone;
        donor.Address = request.Address;
        donor.Notes = request.Notes;
        donor.IsAnonymous = request.IsAnonymous;
        donor.IsActive = request.IsActive;
        donor.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToDto(donor);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var donor = await _context.Donors.FindAsync(id);
        if (donor == null) return false;

        // Check for associated transactions
        var hasTransactions = await _context.Transactions.AnyAsync(t => t.DonorId == id);
        if (hasTransactions)
            throw new InvalidOperationException("Cannot delete donor with associated transactions. Deactivate instead.");

        _context.Donors.Remove(donor);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<TransactionDto>> GetContributionHistoryAsync(int donorId)
    {
        return await _transactionService.GetByDonorAsync(donorId);
    }

    private static DonorDto MapToDto(Donor donor)
    {
        return new DonorDto(
            donor.Id,
            donor.Name,
            donor.Type,
            donor.Email,
            donor.Phone,
            donor.Address,
            donor.Notes,
            donor.TotalContributions,
            donor.FirstContributionDate,
            donor.LastContributionDate,
            donor.IsAnonymous,
            donor.IsActive
        );
    }
}
