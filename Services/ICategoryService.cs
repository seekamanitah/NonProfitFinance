using NonProfitFinance.DTOs;
using NonProfitFinance.Models;

namespace NonProfitFinance.Services;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllAsync(CategoryType? type = null, bool includeArchived = false);
    Task<List<CategoryDto>> GetTreeAsync(CategoryType type);
    Task<CategoryDto?> GetByIdAsync(int id);
    Task<CategoryDto> CreateAsync(CreateCategoryRequest request);
    Task<CategoryDto?> UpdateAsync(int id, UpdateCategoryRequest request);
    Task<bool> DeleteAsync(int id);
    Task<bool> ArchiveAsync(int id);
    Task<bool> RestoreAsync(int id);
    Task<bool> HasTransactionsAsync(int id);
}
