using NonProfitFinance.DTOs.Inventory;

namespace NonProfitFinance.Services.Inventory;

public interface IInventoryItemService
{
    // Basic CRUD
    Task<PagedInventoryItems> GetAllAsync(InventoryItemFilterRequest filter);
    Task<InventoryItemDto?> GetByIdAsync(int id);
    Task<InventoryItemDto> CreateAsync(CreateInventoryItemRequest request);
    Task<InventoryItemDto?> UpdateAsync(int id, UpdateInventoryItemRequest request);
    Task<bool> DeleteAsync(int id);

    // Search & Filtering
    Task<List<InventoryItemSummaryDto>> SearchAsync(string searchTerm, int maxResults = 10);
    Task<List<InventoryItemDto>> GetByCategoryAsync(int categoryId);
    Task<List<InventoryItemDto>> GetByLocationAsync(int locationId);
    Task<List<InventoryItemDto>> GetLowStockItemsAsync();
    Task<List<InventoryItemDto>> GetOutOfStockItemsAsync();
    Task<List<InventoryItemDto>> GetExpiringItemsAsync(int daysAhead = 30);

    // Stock Operations
    Task<bool> AdjustStockAsync(int itemId, decimal quantityChange, string reason, string? notes = null);
    Task<bool> TransferStockAsync(int itemId, int fromLocationId, int toLocationId, decimal quantity, string? notes = null);
    Task<bool> SetStockLevelAsync(int itemId, decimal newQuantity, string reason, string? notes = null);

    // Reporting & Analytics
    Task<StockLevelReportDto> GetStockLevelReportAsync(InventoryItemFilterRequest filter);
    Task<List<CategoryStockDto>> GetStockByCategoryAsync();
    Task<List<LocationStockDto>> GetStockByLocationAsync();
    Task<decimal> GetTotalInventoryValueAsync();

    // Validation
    Task<bool> ExistsAsync(int id);
    Task<bool> SKUExistsAsync(string sku, int? excludeItemId = null);
    Task<bool> BarcodeExistsAsync(string barcode, int? excludeItemId = null);
}
