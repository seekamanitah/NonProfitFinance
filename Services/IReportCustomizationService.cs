using NonProfitFinance.Models;

namespace NonProfitFinance.Services;

public interface IReportCustomizationService
{
    /// <summary>
    /// Get default column configurations for a report type.
    /// </summary>
    List<ReportColumnConfig> GetDefaultColumns(CustomizableReportType reportType);
    
    /// <summary>
    /// Get user's saved column settings for a report type.
    /// Falls back to defaults if none exist.
    /// </summary>
    Task<ReportColumnSettings> GetSettingsAsync(CustomizableReportType reportType, string? userId = null);
    
    /// <summary>
    /// Save column settings for a report type.
    /// </summary>
    Task<ReportColumnSettings> SaveSettingsAsync(ReportColumnSettings settings);
    
    /// <summary>
    /// Get all saved presets for a report type.
    /// </summary>
    Task<List<ReportColumnSettings>> GetPresetsAsync(CustomizableReportType reportType, string? userId = null);
    
    /// <summary>
    /// Delete a saved preset.
    /// </summary>
    Task<bool> DeletePresetAsync(int id);
    
    /// <summary>
    /// Set a preset as the default for a report type.
    /// </summary>
    Task SetDefaultPresetAsync(int id, CustomizableReportType reportType, string? userId = null);
}
