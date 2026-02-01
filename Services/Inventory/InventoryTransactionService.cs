using Microsoft.EntityFrameworkCore;
using NonProfitFinance.Data;
using NonProfitFinance.DTOs.Inventory;
using NonProfitFinance.Models.Enums;
using NonProfitFinance.Models.Inventory;

namespace NonProfitFinance.Services.Inventory;

public class InventoryTransactionService : IInventoryTransactionService
{
    private readonly ApplicationDbContext _context;

    public InventoryTransactionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedInventoryTransactions> GetAllAsync(InventoryTransactionFilterRequest filter)
    {
        var query = _context.InventoryTransactions
            .Include(t => t.Item)
            .Include(t => t.FromLocation)
            .Include(t => t.ToLocation)
            .AsQueryable();

        // Apply filters
        if (filter.StartDate.HasValue)
            query = query.Where(t => t.TransactionDate >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(t => t.TransactionDate <= filter.EndDate.Value);

        if (filter.ItemId.HasValue)
            query = query.Where(t => t.ItemId == filter.ItemId.Value);

        if (filter.LocationId.HasValue)
            query = query.Where(t => t.FromLocationId == filter.LocationId.Value || t.ToLocationId == filter.LocationId.Value);

        if (filter.Type.HasValue)
            query = query.Where(t => t.Type == filter.Type.Value);

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.ToLower();
            query = query.Where(t =>
                t.Item.Name.ToLower().Contains(term) ||
                (t.Item.SKU != null && t.Item.SKU.ToLower().Contains(term)) ||
                (t.Reason != null && t.Reason.ToLower().Contains(term)) ||
                (t.Notes != null && t.Notes.ToLower().Contains(term)));
        }

        var totalCount = await query.CountAsync();

        var transactions = await query
            .OrderByDescending(t => t.TransactionDate)
            .ThenByDescending(t => t.Id)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var dtos = transactions.Select(MapToDto).ToList();

        return new PagedInventoryTransactions(
            dtos,
            totalCount,
            filter.Page,
            filter.PageSize,
            (int)Math.Ceiling(totalCount / (double)filter.PageSize)
        );
    }

    public async Task<InventoryTransactionDto?> GetByIdAsync(int id)
    {
        var transaction = await _context.InventoryTransactions
            .Include(t => t.Item)
            .Include(t => t.FromLocation)
            .Include(t => t.ToLocation)
            .FirstOrDefaultAsync(t => t.Id == id);

        return transaction == null ? null : MapToDto(transaction);
    }

    public async Task<InventoryTransactionDto> CreateAsync(CreateInventoryTransactionRequest request)
    {
        var item = await _context.InventoryItems.FindAsync(request.ItemId);
        if (item == null)
            throw new ArgumentException($"Item with ID {request.ItemId} not found");

        var transaction = new InventoryTransaction
        {
            ItemId = request.ItemId,
            Type = request.Type,
            Quantity = request.Quantity,
            FromLocationId = request.FromLocationId,
            ToLocationId = request.ToLocationId,
            Reason = request.Reason,
            UnitCost = request.UnitCost ?? item.UnitCost ?? 0,
            ReferenceNumber = request.ReferenceNumber,
            PerformedBy = "System", // TODO: Get from current user context
            Notes = request.Notes,
            TransactionDate = request.TransactionDate ?? DateTime.UtcNow
        };

        transaction.TotalCost = transaction.Quantity * transaction.UnitCost;

        _context.InventoryTransactions.Add(transaction);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(transaction.Id))!;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var transaction = await _context.InventoryTransactions.FindAsync(id);
        if (transaction == null) return false;

        // Note: Deleting transactions should be restricted in production
        // This may require reversing the transaction instead
        _context.InventoryTransactions.Remove(transaction);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<InventoryTransactionDto>> GetByItemAsync(int itemId, int limit = 50)
    {
        var transactions = await _context.InventoryTransactions
            .Include(t => t.Item)
            .Include(t => t.FromLocation)
            .Include(t => t.ToLocation)
            .Where(t => t.ItemId == itemId)
            .OrderByDescending(t => t.TransactionDate)
            .ThenByDescending(t => t.Id)
            .Take(limit)
            .ToListAsync();

        return transactions.Select(MapToDto).ToList();
    }

    public async Task<List<InventoryTransactionDto>> GetByLocationAsync(int locationId, int limit = 50)
    {
        var transactions = await _context.InventoryTransactions
            .Include(t => t.Item)
            .Include(t => t.FromLocation)
            .Include(t => t.ToLocation)
            .Where(t => t.FromLocationId == locationId || t.ToLocationId == locationId)
            .OrderByDescending(t => t.TransactionDate)
            .ThenByDescending(t => t.Id)
            .Take(limit)
            .ToListAsync();

        return transactions.Select(MapToDto).ToList();
    }

    public async Task<List<InventoryTransactionDto>> GetRecentAsync(int limit = 25)
    {
        var transactions = await _context.InventoryTransactions
            .Include(t => t.Item)
            .Include(t => t.FromLocation)
            .Include(t => t.ToLocation)
            .OrderByDescending(t => t.TransactionDate)
            .ThenByDescending(t => t.Id)
            .Take(limit)
            .ToListAsync();

        return transactions.Select(MapToDto).ToList();
    }

    public async Task<List<InventoryUsageReportDto>> GetUsageReportAsync(DateTime startDate, DateTime endDate, int? itemId = null)
    {
        var query = _context.InventoryTransactions
            .Include(t => t.Item)
            .Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate);

        if (itemId.HasValue)
            query = query.Where(t => t.ItemId == itemId.Value);

        var transactions = await query.ToListAsync();

        var report = transactions
            .GroupBy(t => new { t.ItemId, t.Item.Name })
            .Select(g => new InventoryUsageReportDto(
                startDate,
                endDate,
                g.Key.ItemId,
                g.Key.Name,
                g.Where(t => t.Type == InventoryTransactionType.Purchase).Sum(t => t.Quantity),
                g.Where(t => t.Type == InventoryTransactionType.Use).Sum(t => t.Quantity),
                g.Where(t => t.Type == InventoryTransactionType.Purchase).Sum(t => t.Quantity) -
                g.Where(t => t.Type == InventoryTransactionType.Use).Sum(t => t.Quantity),
                Math.Round((g.Where(t => t.Type == InventoryTransactionType.Use).Sum(t => t.Quantity) /
                           (decimal)(endDate - startDate).Days), 2)
            ))
            .OrderByDescending(r => r.TotalRemoved)
            .ToList();

        return report;
    }

    public async Task<List<InventoryTransactionDto>> GetTransactionHistoryAsync(int itemId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.InventoryTransactions
            .Include(t => t.Item)
            .Include(t => t.FromLocation)
            .Include(t => t.ToLocation)
            .Where(t => t.ItemId == itemId);

        if (startDate.HasValue)
            query = query.Where(t => t.TransactionDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(t => t.TransactionDate <= endDate.Value);

        var transactions = await query
            .OrderByDescending(t => t.TransactionDate)
            .ThenByDescending(t => t.Id)
            .ToListAsync();

        return transactions.Select(MapToDto).ToList();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.InventoryTransactions.AnyAsync(t => t.Id == id);
    }

    private static InventoryTransactionDto MapToDto(InventoryTransaction transaction)
    {
        return new InventoryTransactionDto(
            transaction.Id,
            transaction.ItemId,
            transaction.Item.Name,
            transaction.Item.SKU,
            transaction.Type,
            transaction.Quantity,
            transaction.Item.Unit.ToString(),
            transaction.FromLocationId,
            transaction.FromLocation?.Name,
            transaction.ToLocationId,
            transaction.ToLocation?.Name,
            transaction.Reason,
            transaction.UnitCost,
            transaction.TotalCost,
            transaction.ReferenceNumber,
            null, // RelatedFinancialTransactionId not in model
            transaction.PerformedBy,
            transaction.Notes,
            transaction.TransactionDate,
            transaction.TransactionDate // Use TransactionDate as CreatedAt equivalent
        );
    }
}
