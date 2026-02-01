using NonProfitFinance.DTOs;

namespace NonProfitFinance.Services;

public interface IDonorService
{
    Task<List<DonorDto>> GetAllAsync(bool includeInactive = false);
    Task<DonorDto?> GetByIdAsync(int id);
    Task<DonorDto> CreateAsync(CreateDonorRequest request);
    Task<DonorDto?> UpdateAsync(int id, UpdateDonorRequest request);
    Task<bool> DeleteAsync(int id);
    Task<List<TransactionDto>> GetContributionHistoryAsync(int donorId);
}
