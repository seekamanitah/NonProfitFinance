using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NonProfitFinance.Data;
using NonProfitFinance.Models;

namespace NonProfitFinance.Services;

public class ReportCustomizationService : IReportCustomizationService
{
    private readonly ApplicationDbContext _context;

    public ReportCustomizationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<ReportColumnConfig> GetDefaultColumns(CustomizableReportType reportType)
    {
        return reportType switch
        {
            CustomizableReportType.TransactionList => GetTransactionListColumns(),
            CustomizableReportType.IncomeExpenseSummary => GetIncomeExpenseColumns(),
            CustomizableReportType.CategoryBreakdown => GetCategoryBreakdownColumns(),
            CustomizableReportType.FundSummary => GetFundSummaryColumns(),
            CustomizableReportType.DonorReport => GetDonorReportColumns(),
            CustomizableReportType.GrantReport => GetGrantReportColumns(),
            CustomizableReportType.BudgetVsActual => GetBudgetColumns(),
            CustomizableReportType.CashFlow => GetCashFlowColumns(),
            CustomizableReportType.AuditLog => GetAuditLogColumns(),
            CustomizableReportType.RecycleBin => GetRecycleBinColumns(),
            _ => []
        };
    }

    public async Task<ReportColumnSettings> GetSettingsAsync(CustomizableReportType reportType, string? userId = null)
    {
        var settings = await _context.ReportColumnSettings
            .Where(s => s.ReportType == reportType && s.UserId == userId && s.IsDefault)
            .FirstOrDefaultAsync();

        if (settings != null)
            return settings;

        // Return default settings
        var defaultColumns = GetDefaultColumns(reportType);
        return new ReportColumnSettings
        {
            ReportType = reportType,
            UserId = userId,
            ColumnsJson = JsonSerializer.Serialize(defaultColumns),
            IsDefault = true
        };
    }

    public async Task<ReportColumnSettings> SaveSettingsAsync(ReportColumnSettings settings)
    {
        settings.UpdatedAt = DateTime.UtcNow;

        if (settings.Id == 0)
        {
            settings.CreatedAt = DateTime.UtcNow;
            _context.ReportColumnSettings.Add(settings);
        }
        else
        {
            _context.ReportColumnSettings.Update(settings);
        }

        await _context.SaveChangesAsync();
        return settings;
    }

    public async Task<List<ReportColumnSettings>> GetPresetsAsync(CustomizableReportType reportType, string? userId = null)
    {
        return await _context.ReportColumnSettings
            .Where(s => s.ReportType == reportType && (s.UserId == userId || s.UserId == null))
            .OrderByDescending(s => s.IsDefault)
            .ThenBy(s => s.PresetName)
            .ToListAsync();
    }

    public async Task<bool> DeletePresetAsync(int id)
    {
        var preset = await _context.ReportColumnSettings.FindAsync(id);
        if (preset == null) return false;

        _context.ReportColumnSettings.Remove(preset);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task SetDefaultPresetAsync(int id, CustomizableReportType reportType, string? userId = null)
    {
        // Clear existing defaults for this report type
        var existingDefaults = await _context.ReportColumnSettings
            .Where(s => s.ReportType == reportType && s.UserId == userId && s.IsDefault)
            .ToListAsync();

        foreach (var existing in existingDefaults)
        {
            existing.IsDefault = false;
        }

        // Set new default
        var newDefault = await _context.ReportColumnSettings.FindAsync(id);
        if (newDefault != null)
        {
            newDefault.IsDefault = true;
        }

        await _context.SaveChangesAsync();
    }

    #region Default Column Definitions

    private static List<ReportColumnConfig> GetTransactionListColumns() =>
    [
        new() { ColumnId = "Date", DisplayName = "Date", Order = 1, Alignment = ColumnAlignment.Left },
        new() { ColumnId = "Type", DisplayName = "Type", Order = 2, Alignment = ColumnAlignment.Center },
        new() { ColumnId = "Amount", DisplayName = "Amount", Order = 3, Alignment = ColumnAlignment.Right },
        new() { ColumnId = "Category", DisplayName = "Category", Order = 4, Alignment = ColumnAlignment.Left },
        new() { ColumnId = "Account", DisplayName = "Account", Order = 5, Alignment = ColumnAlignment.Left },
        new() { ColumnId = "Description", DisplayName = "Description", Order = 6, Alignment = ColumnAlignment.Left },
        new() { ColumnId = "Payee", DisplayName = "Payee/Payer", Order = 7, Alignment = ColumnAlignment.Left },
        new() { ColumnId = "Reference", DisplayName = "Reference #", Order = 8, Alignment = ColumnAlignment.Left, IsVisible = false },
        new() { ColumnId = "Tags", DisplayName = "Tags", Order = 9, Alignment = ColumnAlignment.Left, IsVisible = false },
        new() { ColumnId = "Donor", DisplayName = "Donor", Order = 10, Alignment = ColumnAlignment.Left, IsVisible = false },
        new() { ColumnId = "Grant", DisplayName = "Grant", Order = 11, Alignment = ColumnAlignment.Left, IsVisible = false },
        new() { ColumnId = "FundType", DisplayName = "Fund Type", Order = 12, Alignment = ColumnAlignment.Left, IsVisible = false }
    ];

    private static List<ReportColumnConfig> GetIncomeExpenseColumns() =>
    [
        new() { ColumnId = "Category", DisplayName = "Category", Order = 1, Alignment = ColumnAlignment.Left },
        new() { ColumnId = "Amount", DisplayName = "Amount", Order = 2, Alignment = ColumnAlignment.Right },
        new() { ColumnId = "Percentage", DisplayName = "% of Total", Order = 3, Alignment = ColumnAlignment.Right },
        new() { ColumnId = "TransactionCount", DisplayName = "# Transactions", Order = 4, Alignment = ColumnAlignment.Center },
        new() { ColumnId = "AvgAmount", DisplayName = "Avg Amount", Order = 5, Alignment = ColumnAlignment.Right, IsVisible = false }
    ];

    private static List<ReportColumnConfig> GetCategoryBreakdownColumns() =>
    [
        new() { ColumnId = "Category", DisplayName = "Category", Order = 1, Alignment = ColumnAlignment.Left },
        new() { ColumnId = "ParentCategory", DisplayName = "Parent Category", Order = 2, Alignment = ColumnAlignment.Left },
        new() { ColumnId = "Amount", DisplayName = "Amount", Order = 3, Alignment = ColumnAlignment.Right },
        new() { ColumnId = "Percentage", DisplayName = "% of Total", Order = 4, Alignment = ColumnAlignment.Right },
        new() { ColumnId = "Budget", DisplayName = "Budget", Order = 5, Alignment = ColumnAlignment.Right, IsVisible = false },
        new() { ColumnId = "Variance", DisplayName = "Variance", Order = 6, Alignment = ColumnAlignment.Right, IsVisible = false }
    ];

    private static List<ReportColumnConfig> GetFundSummaryColumns() =>
    [
        new() { ColumnId = "FundName", DisplayName = "Account Name", Order = 1, Alignment = ColumnAlignment.Left },
        new() { ColumnId = "Type", DisplayName = "Type", Order = 2, Alignment = ColumnAlignment.Left },
        new() { ColumnId = "StartingBalance", DisplayName = "Starting Balance", Order = 3, Alignment = ColumnAlignment.Right },
        new() { ColumnId = "TotalIncome", DisplayName = "Total Income", Order = 4, Alignment = ColumnAlignment.Right },
        new() { ColumnId = "TotalExpenses", DisplayName = "Total Expenses", Order = 5, Alignment = ColumnAlignment.Right },
        new() { ColumnId = "CurrentBalance", DisplayName = "Current Balance", Order = 6, Alignment = ColumnAlignment.Right },
        new() { ColumnId = "TargetBalance", DisplayName = "Target Balance", Order = 7, Alignment = ColumnAlignment.Right, IsVisible = false },
        new() { ColumnId = "IsActive", DisplayName = "Status", Order = 8, Alignment = ColumnAlignment.Center }
    ];

    private static List<ReportColumnConfig> GetDonorReportColumns() =>
    [
        new() { ColumnId = "DonorName", DisplayName = "Donor Name", Order = 1, Alignment = ColumnAlignment.Left },
        new() { ColumnId = "Type", DisplayName = "Donor Type", Order = 2, Alignment = ColumnAlignment.Left },
        new() { ColumnId = "TotalDonated", DisplayName = "Total Donated", Order = 3, Alignment = ColumnAlignment.Right },
        new() { ColumnId = "DonationCount", DisplayName = "# Donations", Order = 4, Alignment = ColumnAlignment.Center },
        new() { ColumnId = "FirstDonation", DisplayName = "First Donation", Order = 5, Alignment = ColumnAlignment.Left },
        new() { ColumnId = "LastDonation", DisplayName = "Last Donation", Order = 6, Alignment = ColumnAlignment.Left },
        new() { ColumnId = "AvgDonation", DisplayName = "Avg Donation", Order = 7, Alignment = ColumnAlignment.Right },
        new() { ColumnId = "Email", DisplayName = "Email", Order = 8, Alignment = ColumnAlignment.Left, IsVisible = false },
        new() { ColumnId = "Phone", DisplayName = "Phone", Order = 9, Alignment = ColumnAlignment.Left, IsVisible = false },
        new() { ColumnId = "Address", DisplayName = "Address", Order = 10, Alignment = ColumnAlignment.Left, IsVisible = false }
    ];

    private static List<ReportColumnConfig> GetGrantReportColumns() =>
    [
        new() { ColumnId = "GrantName", DisplayName = "Grant Name", Order = 1, Alignment = ColumnAlignment.Left },
        new() { ColumnId = "Grantor", DisplayName = "Grantor", Order = 2, Alignment = ColumnAlignment.Left },
        new() { ColumnId = "Amount", DisplayName = "Total Amount", Order = 3, Alignment = ColumnAlignment.Right },
        new() { ColumnId = "AmountUsed", DisplayName = "Amount Used", Order = 4, Alignment = ColumnAlignment.Right },
        new() { ColumnId = "Remaining", DisplayName = "Remaining", Order = 5, Alignment = ColumnAlignment.Right },
        new() { ColumnId = "PercentUsed", DisplayName = "% Used", Order = 6, Alignment = ColumnAlignment.Right },
        new() { ColumnId = "StartDate", DisplayName = "Start Date", Order = 7, Alignment = ColumnAlignment.Left },
        new() { ColumnId = "EndDate", DisplayName = "End Date", Order = 8, Alignment = ColumnAlignment.Left },
        new() { ColumnId = "Status", DisplayName = "Status", Order = 9, Alignment = ColumnAlignment.Center },
        new() { ColumnId = "GrantNumber", DisplayName = "Grant #", Order = 10, Alignment = ColumnAlignment.Left, IsVisible = false }
    ];

    private static List<ReportColumnConfig> GetBudgetColumns() =>
    [
        new() { ColumnId = "Category", DisplayName = "Category", Order = 1, Alignment = ColumnAlignment.Left },
        new() { ColumnId = "BudgetAmount", DisplayName = "Budget", Order = 2, Alignment = ColumnAlignment.Right },
        new() { ColumnId = "ActualAmount", DisplayName = "Actual", Order = 3, Alignment = ColumnAlignment.Right },
        new() { ColumnId = "Variance", DisplayName = "Variance", Order = 4, Alignment = ColumnAlignment.Right },
        new() { ColumnId = "PercentUsed", DisplayName = "% Used", Order = 5, Alignment = ColumnAlignment.Right },
        new() { ColumnId = "Status", DisplayName = "Status", Order = 6, Alignment = ColumnAlignment.Center }
    ];

    private static List<ReportColumnConfig> GetCashFlowColumns() =>
    [
        new() { ColumnId = "Date", DisplayName = "Date", Order = 1, Alignment = ColumnAlignment.Left },
        new() { ColumnId = "Description", DisplayName = "Description", Order = 2, Alignment = ColumnAlignment.Left },
        new() { ColumnId = "Type", DisplayName = "Type", Order = 3, Alignment = ColumnAlignment.Center },
        new() { ColumnId = "Amount", DisplayName = "Amount", Order = 4, Alignment = ColumnAlignment.Right },
        new() { ColumnId = "RunningBalance", DisplayName = "Balance", Order = 5, Alignment = ColumnAlignment.Right },
        new() { ColumnId = "Source", DisplayName = "Source", Order = 6, Alignment = ColumnAlignment.Left }
    ];

    private static List<ReportColumnConfig> GetAuditLogColumns() =>
    [
        new() { ColumnId = "Timestamp", DisplayName = "Timestamp", Order = 1, Alignment = ColumnAlignment.Left },
        new() { ColumnId = "Action", DisplayName = "Action", Order = 2, Alignment = ColumnAlignment.Center },
        new() { ColumnId = "EntityType", DisplayName = "Entity Type", Order = 3, Alignment = ColumnAlignment.Left },
        new() { ColumnId = "EntityId", DisplayName = "Entity ID", Order = 4, Alignment = ColumnAlignment.Center },
        new() { ColumnId = "Description", DisplayName = "Description", Order = 5, Alignment = ColumnAlignment.Left },
        new() { ColumnId = "User", DisplayName = "User", Order = 6, Alignment = ColumnAlignment.Left, IsVisible = false },
        new() { ColumnId = "OldValues", DisplayName = "Old Values", Order = 7, Alignment = ColumnAlignment.Left, IsVisible = false },
        new() { ColumnId = "NewValues", DisplayName = "New Values", Order = 8, Alignment = ColumnAlignment.Left, IsVisible = false }
    ];

    private static List<ReportColumnConfig> GetRecycleBinColumns() =>
    [
        new() { ColumnId = "DeletedAt", DisplayName = "Deleted At", Order = 1, Alignment = ColumnAlignment.Left },
        new() { ColumnId = "Type", DisplayName = "Type", Order = 2, Alignment = ColumnAlignment.Center },
        new() { ColumnId = "Date", DisplayName = "Original Date", Order = 3, Alignment = ColumnAlignment.Left },
        new() { ColumnId = "Amount", DisplayName = "Amount", Order = 4, Alignment = ColumnAlignment.Right },
        new() { ColumnId = "Description", DisplayName = "Description", Order = 5, Alignment = ColumnAlignment.Left },
        new() { ColumnId = "Category", DisplayName = "Category", Order = 6, Alignment = ColumnAlignment.Left }
    ];

    #endregion
}
