using NonProfitFinance.Models;

namespace NonProfitFinance.Services;

public interface IScheduledReportService
{
    Task<List<ScheduledReport>> GetAllAsync();
    Task<ScheduledReport?> GetByIdAsync(int id);
    Task<List<ScheduledReport>> GetDueReportsAsync();
    Task<ScheduledReport> SaveAsync(ScheduledReport report);
    Task DeleteAsync(int id);
    Task<bool> ToggleActiveAsync(int id);
    Task RecordRunAsync(int id, bool success, string? message = null);
    Task<(DateTime Start, DateTime End)> GetPeriodDatesAsync(ReportPeriodType periodType);
}
