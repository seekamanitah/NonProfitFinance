using Microsoft.AspNetCore.Mvc;
using NonProfitFinance.DTOs;
using NonProfitFinance.Services;

namespace NonProfitFinance.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    /// <summary>
    /// Get dashboard metrics (monthly/YTD income/expenses, balances, counts).
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<ActionResult<ApiResponse<DashboardMetricsDto>>> GetDashboard()
    {
        var metrics = await _reportService.GetDashboardMetricsAsync();
        return Ok(new ApiResponse<DashboardMetricsDto>(true, metrics));
    }

    /// <summary>
    /// Get income/expense summary for a period.
    /// </summary>
    [HttpGet("income-expense")]
    public async Task<ActionResult<ApiResponse<IncomeExpenseSummaryDto>>> GetIncomeExpenseSummary(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] int? categoryId = null,
        [FromQuery] int? fundId = null,
        [FromQuery] int? donorId = null,
        [FromQuery] int? grantId = null,
        [FromQuery] bool includeSubcategories = true)
    {
        var filter = new ReportFilterRequest(startDate, endDate, categoryId, fundId, donorId, grantId, includeSubcategories);
        var summary = await _reportService.GetIncomeExpenseSummaryAsync(filter);
        return Ok(new ApiResponse<IncomeExpenseSummaryDto>(true, summary));
    }

    /// <summary>
    /// Get category breakdown for charts.
    /// </summary>
    [HttpGet("category-breakdown")]
    public async Task<ActionResult<ApiResponse<List<CategorySummaryDto>>>> GetCategoryBreakdown(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] int? categoryId = null,
        [FromQuery] int? fundId = null,
        [FromQuery] int? donorId = null,
        [FromQuery] int? grantId = null,
        [FromQuery] bool includeSubcategories = true)
    {
        var filter = new ReportFilterRequest(startDate, endDate, categoryId, fundId, donorId, grantId, includeSubcategories);
        var breakdown = await _reportService.GetCategoryBreakdownAsync(filter);
        return Ok(new ApiResponse<List<CategorySummaryDto>>(true, breakdown));
    }

    /// <summary>
    /// Get trend data for line/area charts.
    /// </summary>
    [HttpGet("trends")]
    public async Task<ActionResult<ApiResponse<List<TrendDataDto>>>> GetTrends(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] string interval = "monthly")
    {
        var trends = await _reportService.GetTrendDataAsync(startDate, endDate, interval);
        return Ok(new ApiResponse<List<TrendDataDto>>(true, trends));
    }

    /// <summary>
    /// Get YTD revenue total (for Form 990 audit threshold monitoring).
    /// </summary>
    [HttpGet("ytd-revenue")]
    public async Task<ActionResult<ApiResponse<object>>> GetYtdRevenue()
    {
        var revenue = await _reportService.GetYtdRevenueAsync();
        var approachingThreshold = await _reportService.IsApproachingAuditThresholdAsync();

        return Ok(new ApiResponse<object>(true, new
        {
            YtdRevenue = revenue,
            AuditThreshold = 1_000_000m,
            IsApproachingThreshold = approachingThreshold,
            PercentageOfThreshold = (revenue / 1_000_000m) * 100
        }));
    }
}
