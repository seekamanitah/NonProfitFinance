using NonProfitFinance.DTOs.Inventory;
using NonProfitFinance.Models.Enums;

namespace NonProfitFinance.Services.Inventory;

public interface ILocationService
{
    // Basic CRUD
    Task<List<LocationDto>> GetAllAsync();
    Task<LocationDto?> GetByIdAsync(int id);
    Task<LocationDto> CreateAsync(CreateLocationRequest request);
    Task<LocationDto?> UpdateAsync(int id, UpdateLocationRequest request);
    Task<bool> DeleteAsync(int id);

    // Hierarchical Operations
    Task<List<LocationDto>> GetRootLocationsAsync();
    Task<List<LocationDto>> GetSubLocationsAsync(int parentId);
    Task<List<LocationDto>> GetLocationTreeAsync();
    Task<bool> MoveLocationAsync(int locationId, int? newParentId);

    // Location Queries
    Task<List<LocationDto>> GetByTypeAsync(LocationType type);
    Task<List<InventoryItemDto>> GetItemsAtLocationAsync(int locationId);

    // Validation
    Task<bool> ExistsAsync(int id);
    Task<bool> HasSubLocationsAsync(int id);
    Task<bool> HasItemsAsync(int id);
    Task<bool> CanDeleteAsync(int id);
}
