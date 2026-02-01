using NonProfitFinance.DTOs.Inventory;

namespace NonProfitFinance.Services.Inventory;

public interface IInventoryCategoryService
{
    // Basic CRUD
    Task<List<InventoryCategoryDto>> GetAllAsync();
    Task<InventoryCategoryDto?> GetByIdAsync(int id);
    Task<InventoryCategoryDto> CreateAsync(CreateInventoryCategoryRequest request);
    Task<InventoryCategoryDto?> UpdateAsync(int id, UpdateInventoryCategoryRequest request);
    Task<bool> DeleteAsync(int id);

    // Hierarchical Operations
    Task<List<InventoryCategoryDto>> GetRootCategoriesAsync();
    Task<List<InventoryCategoryDto>> GetSubCategoriesAsync(int parentId);
    Task<List<InventoryCategoryDto>> GetCategoryTreeAsync();
    Task<bool> MoveCategoryAsync(int categoryId, int? newParentId);

    // Validation
    Task<bool> ExistsAsync(int id);
    Task<bool> HasSubCategoriesAsync(int id);
    Task<bool> HasItemsAsync(int id);
    Task<bool> CanDeleteAsync(int id);
}
