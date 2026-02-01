using NonProfitFinance.DTOs.Inventory;

namespace NonProfitFinance.Services.Inventory;

public interface IInventoryTransactionService
{
    // Basic CRUD
    Task<PagedInventoryTransactions> GetAllAsync(InventoryTransactionFilterRequest filter);
    Task<InventoryTransactionDto?> GetByIdAsync(int id);
    Task<InventoryTransactionDto> CreateAsync(CreateInventoryTransactionRequest request);
    Task<bool> DeleteAsync(int id);

    // Queries
    Task<List<InventoryTransactionDto>> GetByItemAsync(int itemId, int limit = 50);
    Task<List<InventoryTransactionDto>> GetByLocationAsync(int locationId, int limit = 50);
    Task<List<InventoryTransactionDto>> GetRecentAsync(int limit = 25);

    // Reports
    Task<List<InventoryUsageReportDto>> GetUsageReportAsync(DateTime startDate, DateTime endDate, int? itemId = null);
    Task<List<InventoryTransactionDto>> GetTransactionHistoryAsync(int itemId, DateTime? startDate = null, DateTime? endDate = null);

    // Validation
    Task<bool> ExistsAsync(int id);
}
