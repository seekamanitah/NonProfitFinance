using Microsoft.EntityFrameworkCore;
using NonProfitFinance.Data;
using NonProfitFinance.DTOs.Inventory;
using NonProfitFinance.Models.Enums;
using NonProfitFinance.Models.Inventory;

namespace NonProfitFinance.Services.Inventory;

public class InventoryItemService : IInventoryItemService
{
    private readonly ApplicationDbContext _context;

    public InventoryItemService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedInventoryItems> GetAllAsync(InventoryItemFilterRequest filter)
    {
        var query = _context.InventoryItems
            .Include(i => i.Category)
            .Include(i => i.Location)
            .AsQueryable();

        // Apply filters
        if (filter.CategoryId.HasValue)
            query = query.Where(i => i.CategoryId == filter.CategoryId.Value);

        if (filter.LocationId.HasValue)
            query = query.Where(i => i.LocationId == filter.LocationId.Value);

        if (filter.Status.HasValue)
            query = query.Where(i => i.Status == filter.Status.Value);

        if (filter.LowStock == true)
            query = query.Where(i => i.Quantity <= (i.MinimumQuantity ?? 0));

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.ToLower();
            query = query.Where(i =>
                i.Name.ToLower().Contains(term) ||
                (i.SKU != null && i.SKU.ToLower().Contains(term)) ||
                (i.Description != null && i.Description.ToLower().Contains(term)) ||
                (i.Barcode != null && i.Barcode.ToLower().Contains(term)));
        }

        // Apply sorting
        query = filter.SortBy?.ToLower() switch
        {
            "name" => filter.SortDescending ? query.OrderByDescending(i => i.Name) : query.OrderBy(i => i.Name),
            "quantity" => filter.SortDescending ? query.OrderByDescending(i => i.Quantity) : query.OrderBy(i => i.Quantity),
            "value" => filter.SortDescending ? query.OrderByDescending(i => i.Quantity * i.UnitCost) : query.OrderBy(i => i.Quantity * i.UnitCost),
            "category" => filter.SortDescending ? query.OrderByDescending(i => i.Category!.Name) : query.OrderBy(i => i.Category!.Name),
            _ => query.OrderBy(i => i.Name)
        };

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var dtos = items.Select(MapToDto).ToList();

        return new PagedInventoryItems(
            dtos,
            totalCount,
            filter.Page,
            filter.PageSize,
            (int)Math.Ceiling(totalCount / (double)filter.PageSize)
        );
    }

    public async Task<InventoryItemDto?> GetByIdAsync(int id)
    {
        var item = await _context.InventoryItems
            .Include(i => i.Category)
            .Include(i => i.Location)
            .FirstOrDefaultAsync(i => i.Id == id);

        return item == null ? null : MapToDto(item);
    }

    public async Task<InventoryItemDto> CreateAsync(CreateInventoryItemRequest request)
    {
        var item = new InventoryItem
        {
            Name = request.Name,
            SKU = request.SKU,
            Description = request.Description,
            CategoryId = request.CategoryId,
            LocationId = request.LocationId,
            Quantity = request.Quantity,
            Unit = Enum.TryParse<UnitOfMeasure>(request.UnitOfMeasure, out var unit) ? unit : UnitOfMeasure.Each,
            MinimumQuantity = request.MinimumStock,
            MaximumQuantity = request.MaximumStock,
            UnitCost = request.UnitCost,
            PhotoUrl = request.ImageUrl,
            Barcode = request.Barcode,
            Manufacturer = request.Manufacturer,
            PurchaseDate = request.PurchaseDate,
            ExpirationDate = request.ExpiryDate,
            Notes = request.Notes,
            Status = DetermineStatus(request.Quantity, request.MinimumStock, request.ExpiryDate),
            IsActive = true
        };

        _context.InventoryItems.Add(item);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(item.Id))!;
    }

    public async Task<InventoryItemDto?> UpdateAsync(int id, UpdateInventoryItemRequest request)
    {
        var item = await _context.InventoryItems.FindAsync(id);
        if (item == null) return null;

        item.Name = request.Name;
        item.SKU = request.SKU;
        item.Description = request.Description;
        item.CategoryId = request.CategoryId;
        item.LocationId = request.LocationId;
        item.Quantity = request.Quantity;
        item.Unit = Enum.TryParse<UnitOfMeasure>(request.UnitOfMeasure, out var unit) ? unit : UnitOfMeasure.Each;
        item.MinimumQuantity = request.MinimumStock;
        item.MaximumQuantity = request.MaximumStock;
        item.UnitCost = request.UnitCost;
        item.PhotoUrl = request.ImageUrl;
        item.Barcode = request.Barcode;
        item.Manufacturer = request.Manufacturer;
        item.PurchaseDate = request.PurchaseDate;
        item.ExpirationDate = request.ExpiryDate;
        item.Notes = request.Notes;
        item.IsActive = request.IsActive;
        item.Status = DetermineStatus(request.Quantity, request.MinimumStock, request.ExpiryDate);
        item.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var item = await _context.InventoryItems.FindAsync(id);
        if (item == null) return false;

        // Soft delete
        item.IsActive = false;
        item.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<InventoryItemSummaryDto>> SearchAsync(string searchTerm, int maxResults = 10)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return new List<InventoryItemSummaryDto>();

        var term = searchTerm.ToLower();
        var items = await _context.InventoryItems
            .Include(i => i.Category)
            .Where(i => i.IsActive &&
                       (i.Name.ToLower().Contains(term) ||
                        (i.SKU != null && i.SKU.ToLower().Contains(term)) ||
                        (i.Barcode != null && i.Barcode.ToLower().Contains(term))))
            .Take(maxResults)
            .ToListAsync();

        return items.Select(i => new InventoryItemSummaryDto(
            i.Id,
            i.Name,
            i.SKU,
            i.CategoryId,
            i.Category?.Name,
            i.Quantity,
            i.Unit.ToString(),
            i.Quantity * (i.UnitCost ?? 0),
            i.Status
        )).ToList();
    }

    public async Task<List<InventoryItemDto>> GetByCategoryAsync(int categoryId)
    {
        var items = await _context.InventoryItems
            .Include(i => i.Category)
            .Include(i => i.Location)
            .Where(i => i.CategoryId == categoryId && i.IsActive)
            .OrderBy(i => i.Name)
            .ToListAsync();

        return items.Select(MapToDto).ToList();
    }

    public async Task<List<InventoryItemDto>> GetByLocationAsync(int locationId)
    {
        var items = await _context.InventoryItems
            .Include(i => i.Category)
            .Include(i => i.Location)
            .Where(i => i.LocationId == locationId && i.IsActive)
            .OrderBy(i => i.Name)
            .ToListAsync();

        return items.Select(MapToDto).ToList();
    }

    public async Task<List<InventoryItemDto>> GetLowStockItemsAsync()
    {
        var items = await _context.InventoryItems
            .Include(i => i.Category)
            .Include(i => i.Location)
            .Where(i => i.IsActive && i.Status == InventoryStatus.LowStock)
            .OrderBy(i => i.Quantity)
            .ToListAsync();

        return items.Select(MapToDto).ToList();
    }

    public async Task<List<InventoryItemDto>> GetOutOfStockItemsAsync()
    {
        var items = await _context.InventoryItems
            .Include(i => i.Category)
            .Include(i => i.Location)
            .Where(i => i.IsActive && i.Status == InventoryStatus.OutOfStock)
            .OrderBy(i => i.Name)
            .ToListAsync();

        return items.Select(MapToDto).ToList();
    }

    public async Task<List<InventoryItemDto>> GetExpiringItemsAsync(int daysAhead = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(daysAhead);
        var items = await _context.InventoryItems
            .Include(i => i.Category)
            .Include(i => i.Location)
            .Where(i => i.IsActive &&
                       i.ExpirationDate.HasValue &&
                       i.ExpirationDate.Value <= cutoffDate)
            .OrderBy(i => i.ExpirationDate)
            .ToListAsync();

        return items.Select(MapToDto).ToList();
    }

    public async Task<bool> AdjustStockAsync(int itemId, decimal quantityChange, string reason, string? notes = null)
    {
        var item = await _context.InventoryItems.FindAsync(itemId);
        if (item == null) return false;

        var oldQuantity = item.Quantity;
        item.Quantity += quantityChange;
        item.Status = DetermineStatus(item.Quantity, item.MinimumQuantity, item.ExpirationDate);
        item.UpdatedAt = DateTime.UtcNow;

        // Record transaction
        var transaction = new InventoryTransaction
        {
            ItemId = itemId,
            Type = quantityChange > 0 ? InventoryTransactionType.Purchase : InventoryTransactionType.Use,
            Quantity = Math.Abs(quantityChange),
            ToLocationId = item.LocationId,
            Reason = reason,
            Notes = notes,
            TransactionDate = DateTime.UtcNow
        };

        _context.InventoryTransactions.Add(transaction);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> TransferStockAsync(int itemId, int fromLocationId, int toLocationId, decimal quantity, string? notes = null)
    {
        var item = await _context.InventoryItems.FindAsync(itemId);
        if (item == null) return false;

        // Record transaction
        var transaction = new InventoryTransaction
        {
            ItemId = itemId,
            Type = InventoryTransactionType.Transfer,
            Quantity = quantity,
            FromLocationId = fromLocationId,
            ToLocationId = toLocationId,
            Reason = "Stock Transfer",
            Notes = notes,
            TransactionDate = DateTime.UtcNow
        };

        _context.InventoryTransactions.Add(transaction);

        // Update item location if this is its current location
        if (item.LocationId == fromLocationId)
        {
            item.LocationId = toLocationId;
            item.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SetStockLevelAsync(int itemId, decimal newQuantity, string reason, string? notes = null)
    {
        var item = await _context.InventoryItems.FindAsync(itemId);
        if (item == null) return false;

        var quantityChange = newQuantity - item.Quantity;
        item.Quantity = newQuantity;
        item.Status = DetermineStatus(item.Quantity, item.MinimumQuantity, item.ExpirationDate);
        item.UpdatedAt = DateTime.UtcNow;

        // Record transaction
        var transaction = new InventoryTransaction
        {
            ItemId = itemId,
            Type = InventoryTransactionType.Adjustment,
            Quantity = Math.Abs(quantityChange),
            ToLocationId = item.LocationId,
            Reason = reason,
            Notes = notes,
            TransactionDate = DateTime.UtcNow
        };

        _context.InventoryTransactions.Add(transaction);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<StockLevelReportDto> GetStockLevelReportAsync(InventoryItemFilterRequest filter)
    {
        var pagedResult = await GetAllAsync(filter);
        
        return new StockLevelReportDto(
            pagedResult.Items,
            pagedResult.Items.Sum(i => i.TotalValue),
            pagedResult.Items.Count(i => i.Status == InventoryStatus.LowStock),
            pagedResult.Items.Count(i => i.Status == InventoryStatus.OutOfStock)
        );
    }

    public async Task<List<CategoryStockDto>> GetStockByCategoryAsync()
    {
        // Get aggregated data by category
        var result = await _context.InventoryItems
            .Where(i => i.IsActive && i.CategoryId.HasValue)
            .GroupBy(i => i.CategoryId)
            .Select(g => new 
            {
                CategoryId = g.Key!.Value,
                ItemCount = g.Count(),
                TotalValue = g.Sum(i => i.Quantity * (i.UnitCost ?? 0)),
                LowStockCount = g.Count(i => i.Status == InventoryStatus.LowStock)
            })
            .ToListAsync();

        // Get category names in a separate query
        var categoryIds = result.Select(r => r.CategoryId).ToList();
        var categories = await _context.InventoryCategories
            .Where(c => categoryIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Name);

        // Combine results
        return result
            .Select(r => new CategoryStockDto(
                r.CategoryId,
                categories.TryGetValue(r.CategoryId, out var name) ? name : "Unknown",
                r.ItemCount,
                r.TotalValue,
                r.LowStockCount
            ))
            .OrderByDescending(c => c.TotalValue)
            .ToList();
    }

    public async Task<List<LocationStockDto>> GetStockByLocationAsync()
    {
        // Get aggregated data by location
        var result = await _context.InventoryItems
            .Where(i => i.IsActive && i.LocationId.HasValue)
            .GroupBy(i => i.LocationId)
            .Select(g => new 
            {
                LocationId = g.Key!.Value,
                ItemCount = g.Count(),
                TotalValue = g.Sum(i => i.Quantity * (i.UnitCost ?? 0))
            })
            .ToListAsync();

        // Get location names in a separate query
        var locationIds = result.Select(r => r.LocationId).ToList();
        var locations = await _context.Locations
            .Where(l => locationIds.Contains(l.Id))
            .ToDictionaryAsync(l => l.Id, l => l.Name);

        // Combine results
        return result
            .Select(r => new LocationStockDto(
                r.LocationId,
                locations.TryGetValue(r.LocationId, out var name) ? name : "Unknown",
                r.ItemCount,
                r.TotalValue
            ))
            .OrderByDescending(l => l.TotalValue)
            .ToList();
    }

    public async Task<decimal> GetTotalInventoryValueAsync()
    {
        return await _context.InventoryItems
            .Where(i => i.IsActive)
            .SumAsync(i => i.Quantity * (i.UnitCost ?? 0));
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.InventoryItems.AnyAsync(i => i.Id == id);
    }

    public async Task<bool> SKUExistsAsync(string sku, int? excludeItemId = null)
    {
        if (string.IsNullOrWhiteSpace(sku)) return false;

        var query = _context.InventoryItems.Where(i => i.SKU == sku);
        if (excludeItemId.HasValue)
            query = query.Where(i => i.Id != excludeItemId.Value);

        return await query.AnyAsync();
    }

    public async Task<bool> BarcodeExistsAsync(string barcode, int? excludeItemId = null)
    {
        if (string.IsNullOrWhiteSpace(barcode)) return false;

        var query = _context.InventoryItems.Where(i => i.Barcode == barcode);
        if (excludeItemId.HasValue)
            query = query.Where(i => i.Id != excludeItemId.Value);

        return await query.AnyAsync();
    }

    private static InventoryStatus DetermineStatus(decimal quantity, decimal? minimumStock, DateTime? expiryDate)
    {
        // Check expiry first (use Discontinued for expired items)
        if (expiryDate.HasValue && expiryDate.Value <= DateTime.UtcNow)
            return InventoryStatus.Discontinued;

        // Check stock levels
        if (quantity <= 0)
            return InventoryStatus.OutOfStock;

        if (minimumStock.HasValue && quantity <= minimumStock.Value)
            return InventoryStatus.LowStock;

        return InventoryStatus.InStock;
    }

    private static InventoryItemDto MapToDto(InventoryItem item)
    {
        return new InventoryItemDto(
            item.Id,
            item.Name,
            item.SKU,
            item.Description,
            item.CategoryId,
            item.Category?.Name,
            item.LocationId,
            item.Location?.Name,
            item.Quantity,
            item.Unit.ToString(),
            item.MinimumQuantity,
            item.MaximumQuantity,
            null, // ReorderPoint not in model
            item.UnitCost ?? 0,
            item.Quantity * (item.UnitCost ?? 0),
            item.Status,
            item.PhotoUrl,
            item.Barcode,
            item.Manufacturer,
            null, // Supplier not in model
            item.PurchaseDate,
            item.ExpirationDate,
            item.Notes,
            item.IsActive,
            item.CreatedAt,
            item.UpdatedAt ?? item.CreatedAt // Use CreatedAt if UpdatedAt is null
        );
    }
}
