using NonProfitFinance.DTOs;
using NonProfitFinance.Models;

namespace NonProfitFinance.Services;

public interface IGrantService
{
    Task<List<GrantDto>> GetAllAsync(GrantStatus? status = null);
    Task<GrantDto?> GetByIdAsync(int id);
    Task<GrantDto> CreateAsync(CreateGrantRequest request);
    Task<GrantDto?> UpdateAsync(int id, UpdateGrantRequest request);
    Task<bool> DeleteAsync(int id);
    Task<List<TransactionDto>> GetUsageHistoryAsync(int grantId);
    Task<List<GrantDto>> GetExpiringGrantsAsync(int daysAhead = 30);
    Task<List<GrantDto>> GetGrantsWithUpcomingReportsAsync(int daysAhead = 14);
}
