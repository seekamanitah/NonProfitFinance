using NonProfitFinance.DTOs;
using NonProfitFinance.Models;

namespace NonProfitFinance.Services;

public interface IFundService
{
    Task<List<FundDto>> GetAllAsync(bool includeInactive = false);
    Task<FundDto?> GetByIdAsync(int id);
    Task<FundDto> CreateAsync(CreateFundRequest request);
    Task<FundDto?> UpdateAsync(int id, UpdateFundRequest request);
    Task<bool> DeleteAsync(int id);
    Task RecalculateAllBalancesAsync();
    Task<decimal> GetTotalRestrictedBalanceAsync();
    Task<decimal> GetTotalUnrestrictedBalanceAsync();
}
