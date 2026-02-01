namespace NonProfitFinance.Services;

public interface IComplianceService
{
    /// <summary>
    /// Get all upcoming compliance deadlines
    /// </summary>
    Task<List<ComplianceReminderDto>> GetUpcomingRemindersAsync(int daysAhead = 90);
    
    /// <summary>
    /// Get TN Secretary of State filing deadlines
    /// </summary>
    Task<List<ComplianceReminderDto>> GetTNFilingRemindersAsync();
    
    /// <summary>
    /// Get Form 990 deadlines
    /// </summary>
    Task<List<ComplianceReminderDto>> GetForm990RemindersAsync();
    
    /// <summary>
    /// Get grant reporting deadlines
    /// </summary>
    Task<List<ComplianceReminderDto>> GetGrantReportingRemindersAsync();
    
    /// <summary>
    /// Check if organization is approaching audit threshold
    /// </summary>
    Task<AuditThresholdStatusDto> CheckAuditThresholdAsync();
    
    /// <summary>
    /// Dismiss or snooze a reminder
    /// </summary>
    Task DismissReminderAsync(string reminderId, DateTime? snoozeUntil = null);
}

public record ComplianceReminderDto(
    string Id,
    string Title,
    string Description,
    ComplianceType Type,
    DateTime DueDate,
    int DaysUntilDue,
    ReminderPriority Priority,
    string? ActionUrl,
    bool IsDismissed
);

public record AuditThresholdStatusDto(
    decimal CurrentYtdRevenue,
    decimal AuditThreshold,
    decimal PercentageOfThreshold,
    bool IsApproaching,
    bool HasExceeded,
    string Message
);

public enum ComplianceType
{
    TNSecretaryOfState,
    Form990,
    GrantReport,
    AuditRequirement,
    InsuranceRenewal,
    LicenseRenewal,
    Other
}

public enum ReminderPriority
{
    Low,
    Medium,
    High,
    Critical
}
