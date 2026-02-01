using NonProfitFinance.DTOs;

namespace NonProfitFinance.Services;

public interface IPdfExportService
{
    /// <summary>
    /// Generate a financial report PDF
    /// </summary>
    Task<byte[]> GenerateFinancialReportAsync(ReportRequest request);
    
    /// <summary>
    /// Generate a transaction list PDF
    /// </summary>
    Task<byte[]> GenerateTransactionReportAsync(TransactionFilterRequest filter);
    
    /// <summary>
    /// Generate a donor summary PDF
    /// </summary>
    Task<byte[]> GenerateDonorReportAsync();
    
    /// <summary>
    /// Generate a grant status PDF
    /// </summary>
    Task<byte[]> GenerateGrantReportAsync();
    
    /// <summary>
    /// Generate a category budget report PDF
    /// </summary>
    Task<byte[]> GenerateBudgetReportAsync(DateTime startDate, DateTime endDate);
}

public record ReportRequest(
    DateTime StartDate,
    DateTime EndDate,
    ReportType ReportType,
    ReportTheme Theme = ReportTheme.Modern,
    bool IncludeCharts = true,
    bool IncludeDetails = true,
    int? CategoryId = null,
    int? FundId = null
);

public enum ReportType
{
    IncomeStatement,
    ExpenseSummary,
    CategoryBreakdown,
    DonorSummary,
    GrantStatus,
    MonthlyTrend,
    AnnualSummary
}

public enum ReportTheme
{
    Modern,
    Classic,
    Dark,
    Light,
    FireDepartment
}
