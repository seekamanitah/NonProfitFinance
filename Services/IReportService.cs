using NonProfitFinance.DTOs;

namespace NonProfitFinance.Services;

public interface IReportService
{
    Task<DashboardMetricsDto> GetDashboardMetricsAsync();
    Task<IncomeExpenseSummaryDto> GetIncomeExpenseSummaryAsync(ReportFilterRequest filter);
    Task<List<CategorySummaryDto>> GetCategoryBreakdownAsync(ReportFilterRequest filter);
    Task<List<TrendDataDto>> GetTrendDataAsync(DateTime startDate, DateTime endDate, string interval = "monthly");
    Task<decimal> GetYtdRevenueAsync();
    Task<bool> IsApproachingAuditThresholdAsync();
}
