using NonProfitFinance.DTOs;

namespace NonProfitFinance.Services;

public interface IBudgetService
{
    Task<List<BudgetDto>> GetAllAsync();
    Task<BudgetDto?> GetByIdAsync(int id);
    Task<BudgetDto?> GetByYearAsync(int fiscalYear);
    Task<BudgetDto> CreateAsync(CreateBudgetRequest request);
    Task<BudgetDto?> UpdateAsync(int id, UpdateBudgetRequest request);
    Task<bool> DeleteAsync(int id);
    Task<BudgetDto> GetBudgetVsActualAsync(int fiscalYear);
}
