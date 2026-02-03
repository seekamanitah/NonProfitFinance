namespace NonProfitFinance.Models;

/// <summary>
/// Stores user preferences for report column visibility and ordering.
/// Allows customization of what data appears in reports and exports.
/// </summary>
public class ReportColumnSettings
{
    public int Id { get; set; }
    
    /// <summary>
    /// Type of report these settings apply to.
    /// </summary>
    public CustomizableReportType ReportType { get; set; }
    
    /// <summary>
    /// Optional user-specific settings (null = default/global settings).
    /// </summary>
    public string? UserId { get; set; }
    
    /// <summary>
    /// JSON array of column configurations.
    /// </summary>
    public string ColumnsJson { get; set; } = "[]";
    
    /// <summary>
    /// Whether to include summary totals in exports.
    /// </summary>
    public bool IncludeSummaryTotals { get; set; } = true;
    
    /// <summary>
    /// Whether to include charts in PDF exports.
    /// </summary>
    public bool IncludeCharts { get; set; } = true;
    
    /// <summary>
    /// Whether to include organization header in exports.
    /// </summary>
    public bool IncludeOrganizationHeader { get; set; } = true;
    
    /// <summary>
    /// Date format preference for exports.
    /// </summary>
    public string DateFormat { get; set; } = "MM/dd/yyyy";
    
    /// <summary>
    /// Currency format preference.
    /// </summary>
    public string CurrencyFormat { get; set; } = "C2";
    
    /// <summary>
    /// Page orientation for PDF exports.
    /// </summary>
    public PageOrientation PageOrientation { get; set; } = PageOrientation.Portrait;
    
    /// <summary>
    /// Paper size for PDF exports.
    /// </summary>
    public PaperSize PaperSize { get; set; } = PaperSize.Letter;
    
    /// <summary>
    /// Whether this is the default/favorite preset for this report type.
    /// </summary>
    public bool IsDefault { get; set; }
    
    /// <summary>
    /// User-friendly name for saved presets.
    /// </summary>
    public string? PresetName { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Types of reports that can be customized.
/// </summary>
public enum CustomizableReportType
{
    TransactionList,
    IncomeExpenseSummary,
    CategoryBreakdown,
    FundSummary,
    DonorReport,
    GrantReport,
    BudgetVsActual,
    CashFlow,
    Form990,
    AuditLog,
    RecycleBin
}

/// <summary>
/// Page orientation for PDF exports.
/// </summary>
public enum PageOrientation
{
    Portrait,
    Landscape
}

/// <summary>
/// Standard paper sizes for PDF exports.
/// </summary>
public enum PaperSize
{
    Letter,      // 8.5" x 11"
    Legal,       // 8.5" x 14"
    A4,          // 210mm x 297mm
    Tabloid      // 11" x 17"
}

/// <summary>
/// Configuration for a single report column.
/// </summary>
public class ReportColumnConfig
{
    /// <summary>
    /// Unique identifier for the column (e.g., "Date", "Amount", "Description").
    /// </summary>
    public string ColumnId { get; set; } = "";
    
    /// <summary>
    /// Display name for the column header.
    /// </summary>
    public string DisplayName { get; set; } = "";
    
    /// <summary>
    /// Whether the column is visible in the report.
    /// </summary>
    public bool IsVisible { get; set; } = true;
    
    /// <summary>
    /// Display order (lower = first).
    /// </summary>
    public int Order { get; set; }
    
    /// <summary>
    /// Column width in pixels (0 = auto).
    /// </summary>
    public int Width { get; set; }
    
    /// <summary>
    /// Text alignment for the column.
    /// </summary>
    public ColumnAlignment Alignment { get; set; } = ColumnAlignment.Left;
    
    /// <summary>
    /// Whether to include this column in exports.
    /// </summary>
    public bool IncludeInExport { get; set; } = true;
}

/// <summary>
/// Text alignment options for report columns.
/// </summary>
public enum ColumnAlignment
{
    Left,
    Center,
    Right
}
