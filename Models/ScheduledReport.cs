namespace NonProfitFinance.Models;

/// <summary>
/// Scheduled report configuration for automated report generation and delivery
/// </summary>
public class ScheduledReport
{
    public int Id { get; set; }

    /// <summary>
    /// Report name/description
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Report type to generate
    /// </summary>
    public ScheduledReportType ReportType { get; set; }

    /// <summary>
    /// Report format (PDF, Excel, Both)
    /// </summary>
    public ReportFormat Format { get; set; } = ReportFormat.Pdf;

    /// <summary>
    /// Schedule frequency
    /// </summary>
    public ScheduleFrequency Frequency { get; set; } = ScheduleFrequency.Monthly;

    /// <summary>
    /// Day of month to run (1-31, for monthly/yearly)
    /// </summary>
    public int? DayOfMonth { get; set; }

    /// <summary>
    /// Day of week to run (0-6, for weekly)
    /// </summary>
    public DayOfWeek? DayOfWeek { get; set; }

    /// <summary>
    /// Time of day to run (e.g., "08:00")
    /// </summary>
    public TimeOnly TimeOfDay { get; set; } = new TimeOnly(8, 0);

    /// <summary>
    /// Period type for the report data
    /// </summary>
    public ReportPeriodType PeriodType { get; set; } = ReportPeriodType.PreviousMonth;

    /// <summary>
    /// Optional filter by fund/account
    /// </summary>
    public int? FundId { get; set; }

    /// <summary>
    /// Email addresses for delivery (comma-separated)
    /// </summary>
    public string? EmailRecipients { get; set; }

    /// <summary>
    /// Save to Documents folder
    /// </summary>
    public bool SaveToDocuments { get; set; } = true;

    /// <summary>
    /// PDF theme to use
    /// </summary>
    public string PdfTheme { get; set; } = "Modern";

    /// <summary>
    /// Include detailed breakdown
    /// </summary>
    public bool IncludeDetails { get; set; } = true;

    /// <summary>
    /// Include charts in report
    /// </summary>
    public bool IncludeCharts { get; set; } = true;

    /// <summary>
    /// Is this schedule active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Last time this report was run
    /// </summary>
    public DateTime? LastRunAt { get; set; }

    /// <summary>
    /// Next scheduled run time
    /// </summary>
    public DateTime? NextRunAt { get; set; }

    /// <summary>
    /// Last run status
    /// </summary>
    public string? LastRunStatus { get; set; }

    /// <summary>
    /// Number of consecutive failures
    /// </summary>
    public int FailureCount { get; set; }

    /// <summary>
    /// Created date
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Updated date
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}

public enum ScheduledReportType
{
    FinancialSummary,
    IncomeStatement,
    ExpenseReport,
    TransactionListing,
    DonorReport,
    GrantStatus,
    BudgetVsActual,
    CashFlow,
    Form990Preview
}

public enum ReportFormat
{
    Pdf,
    Excel,
    Both
}

public enum ScheduleFrequency
{
    Daily,
    Weekly,
    Monthly,
    Quarterly,
    Yearly
}

public enum ReportPeriodType
{
    PreviousDay,
    PreviousWeek,
    PreviousMonth,
    PreviousQuarter,
    PreviousYear,
    MonthToDate,
    QuarterToDate,
    YearToDate,
    Last30Days,
    Last90Days,
    Last12Months
}
