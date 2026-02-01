namespace NonProfitFinance.Models;

/// <summary>
/// Saved report configuration preset
/// </summary>
public class ReportPreset
{
    public int Id { get; set; }
    
    /// <summary>
    /// Preset name
    /// </summary>
    public string Name { get; set; } = "";
    
    /// <summary>
    /// Report type (Financial, Transaction, Donor, Grant, etc.)
    /// </summary>
    public string ReportType { get; set; } = "";
    
    /// <summary>
    /// Start date period type (ThisMonth, ThisQuarter, YTD, Custom, etc.)
    /// </summary>
    public string? PeriodType { get; set; }
    
    /// <summary>
    /// Custom start date (if PeriodType = Custom)
    /// </summary>
    public DateTime? StartDate { get; set; }
    
    /// <summary>
    /// Custom end date (if PeriodType = Custom)
    /// </summary>
    public DateTime? EndDate { get; set; }
    
    /// <summary>
    /// Filter type (All, Account, Category, Donor, Grant)
    /// </summary>
    public string? FilterType { get; set; }
    
    /// <summary>
    /// Selected account ID
    /// </summary>
    public int? AccountId { get; set; }
    
    /// <summary>
    /// Selected category ID
    /// </summary>
    public int? CategoryId { get; set; }
    
    /// <summary>
    /// Selected donor ID
    /// </summary>
    public int? DonorId { get; set; }
    
    /// <summary>
    /// Selected grant ID
    /// </summary>
    public int? GrantId { get; set; }
    
    /// <summary>
    /// PDF theme
    /// </summary>
    public string? PdfTheme { get; set; }
    
    /// <summary>
    /// Include details flag
    /// </summary>
    public bool IncludeDetails { get; set; } = true;
    
    /// <summary>
    /// Is default preset
    /// </summary>
    public bool IsDefault { get; set; }
    
    /// <summary>
    /// Created date
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Updated date
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
