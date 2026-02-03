using NonProfitFinance.Models;

namespace NonProfitFinance.Services;

public interface IReportPresetService
{
    Task<List<ReportPreset>> GetAllPresetsAsync(string? reportType = null);
    Task<ReportPreset?> GetPresetByIdAsync(int id);
    Task<ReportPreset?> GetDefaultPresetAsync(string reportType);
    Task<ReportPreset> SavePresetAsync(ReportPreset preset);
    Task DeletePresetAsync(int id);
    Task SetDefaultPresetAsync(int id);
}
