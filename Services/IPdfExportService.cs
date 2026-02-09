using NonProfitFinance.DTOs;

namespace NonProfitFinance.Services;

public interface IPdfExportService
{
    /// <summary>
    /// Generate a comprehensive financial report PDF with configurable sections
    /// </summary>
    Task<byte[]> GenerateFinancialReportAsync(ReportRequest request);

    /// <summary>
    /// Generate a comprehensive financial report PDF with full section/filter options
    /// </summary>
    Task<byte[]> GenerateFinancialReportAsync(ReportRequest request, PdfReportOptions options);

    /// <summary>
    /// Generate a transaction list PDF with full filter support
    /// </summary>
    Task<byte[]> GenerateTransactionReportAsync(TransactionFilterRequest filter, ReportTheme theme = ReportTheme.Modern);

    /// <summary>
    /// Generate a donor summary PDF
    /// </summary>
    Task<byte[]> GenerateDonorReportAsync(ReportTheme theme = ReportTheme.Modern);

    /// <summary>
    /// Generate a grant status PDF
    /// </summary>
    Task<byte[]> GenerateGrantReportAsync(ReportTheme theme = ReportTheme.Modern);

    /// <summary>
    /// Generate a category budget report PDF
    /// </summary>
    Task<byte[]> GenerateBudgetReportAsync(DateTime startDate, DateTime endDate, ReportTheme theme = ReportTheme.Modern);

    /// <summary>
    /// Generate a fund summary PDF
    /// </summary>
    Task<byte[]> GenerateFundSummaryReportAsync(ReportTheme theme = ReportTheme.Modern);
}

public record ReportRequest(
    DateTime StartDate,
    DateTime EndDate,
    ReportType ReportType,
    ReportTheme Theme = ReportTheme.Modern,
    bool IncludeCharts = true,
    bool IncludeDetails = true,
    int? CategoryId = null,
    int? FundId = null,
    int? DonorId = null,
    int? GrantId = null
);

/// <summary>
/// Options for configuring which sections appear in a comprehensive PDF report
/// </summary>
public record PdfReportOptions(
    bool IncludeSummary = true,
    bool IncludeIncomeBreakdown = true,
    bool IncludeExpenseBreakdown = true,
    bool IncludeTrend = true,
    bool IncludeTransactions = false,
    bool IncludeDonors = false,
    bool IncludeGrants = false,
    bool IncludeBudget = false,
    bool IncludeFunds = false,
    bool LandscapeOrientation = false,
    string? CustomTitle = null
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
