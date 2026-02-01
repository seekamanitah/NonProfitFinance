using Microsoft.EntityFrameworkCore;
using NonProfitFinance.Data;
using NonProfitFinance.Models;

namespace NonProfitFinance.Services;

public class ComplianceService : IComplianceService
{
    private readonly ApplicationDbContext _context;
    private const decimal AuditThreshold = 1_000_000m;

    public ComplianceService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ComplianceReminderDto>> GetUpcomingRemindersAsync(int daysAhead = 90)
    {
        var reminders = new List<ComplianceReminderDto>();
        
        reminders.AddRange(await GetTNFilingRemindersAsync());
        reminders.AddRange(await GetForm990RemindersAsync());
        reminders.AddRange(await GetGrantReportingRemindersAsync());
        
        return reminders
            .Where(r => r.DaysUntilDue <= daysAhead && !r.IsDismissed)
            .OrderBy(r => r.DueDate)
            .ToList();
    }

    public Task<List<ComplianceReminderDto>> GetTNFilingRemindersAsync()
    {
        var reminders = new List<ComplianceReminderDto>();
        var today = DateTime.Today;
        var currentYear = today.Year;

        // TN Annual Report - Due by April 1 each year
        var annualReportDue = new DateTime(currentYear, 4, 1);
        if (annualReportDue < today)
            annualReportDue = annualReportDue.AddYears(1);

        var daysUntilAnnual = (annualReportDue - today).Days;
        reminders.Add(new ComplianceReminderDto(
            $"tn-annual-{annualReportDue.Year}",
            "TN Secretary of State Annual Report",
            $"File annual report with Tennessee Secretary of State. Due by {annualReportDue:MMMM d, yyyy}.",
            ComplianceType.TNSecretaryOfState,
            annualReportDue,
            daysUntilAnnual,
            GetPriority(daysUntilAnnual),
            "https://tnbear.tn.gov/",
            false
        ));

        // TN Charitable Solicitation Registration - Due annually
        var charitableRegDue = new DateTime(currentYear, 8, 31);
        if (charitableRegDue < today)
            charitableRegDue = charitableRegDue.AddYears(1);

        var daysUntilCharitable = (charitableRegDue - today).Days;
        reminders.Add(new ComplianceReminderDto(
            $"tn-charitable-{charitableRegDue.Year}",
            "TN Charitable Solicitation Registration",
            $"Renew charitable solicitation registration. Due by {charitableRegDue:MMMM d, yyyy}.",
            ComplianceType.TNSecretaryOfState,
            charitableRegDue,
            daysUntilCharitable,
            GetPriority(daysUntilCharitable),
            "https://sos.tn.gov/charitable-solicitations",
            false
        ));

        return Task.FromResult(reminders);
    }

    public Task<List<ComplianceReminderDto>> GetForm990RemindersAsync()
    {
        var reminders = new List<ComplianceReminderDto>();
        var today = DateTime.Today;
        
        // Form 990 is due on the 15th day of the 5th month after fiscal year end
        // Assuming calendar year (Dec 31), due May 15
        var fiscalYearEnd = new DateTime(today.Year - 1, 12, 31);
        var form990Due = fiscalYearEnd.AddMonths(5).AddDays(-fiscalYearEnd.Day + 15);
        
        if (form990Due < today)
        {
            fiscalYearEnd = new DateTime(today.Year, 12, 31);
            form990Due = fiscalYearEnd.AddMonths(5).AddDays(-fiscalYearEnd.Day + 15);
        }

        var daysUntilDue = (form990Due - today).Days;
        reminders.Add(new ComplianceReminderDto(
            $"form990-{fiscalYearEnd.Year}",
            $"IRS Form 990 - Tax Year {fiscalYearEnd.Year}",
            $"File Form 990 with the IRS. Due by {form990Due:MMMM d, yyyy}. Extension available (Form 8868).",
            ComplianceType.Form990,
            form990Due,
            daysUntilDue,
            GetPriority(daysUntilDue),
            "/compliance/form990",
            false
        ));

        return Task.FromResult(reminders);
    }

    public async Task<List<ComplianceReminderDto>> GetGrantReportingRemindersAsync()
    {
        var reminders = new List<ComplianceReminderDto>();
        var today = DateTime.Today;

        var grantsWithReports = await _context.Grants
            .Where(g => g.Status == GrantStatus.Active && g.NextReportDueDate.HasValue)
            .ToListAsync();

        foreach (var grant in grantsWithReports)
        {
            var dueDate = grant.NextReportDueDate!.Value;
            var daysUntilDue = (dueDate - today).Days;

            reminders.Add(new ComplianceReminderDto(
                $"grant-report-{grant.Id}",
                $"Grant Report: {grant.Name}",
                $"Submit required report to {grant.GrantorName}. Due by {dueDate:MMMM d, yyyy}.",
                ComplianceType.GrantReport,
                dueDate,
                daysUntilDue,
                GetPriority(daysUntilDue),
                $"/grants",
                false
            ));
        }

        // Also check for expiring grants
        var expiringGrants = await _context.Grants
            .Where(g => g.Status == GrantStatus.Active && g.EndDate.HasValue && g.EndDate <= today.AddDays(60))
            .ToListAsync();

        foreach (var grant in expiringGrants)
        {
            var endDate = grant.EndDate!.Value;
            var daysUntil = (endDate - today).Days;

            reminders.Add(new ComplianceReminderDto(
                $"grant-expiry-{grant.Id}",
                $"Grant Expiring: {grant.Name}",
                $"Grant from {grant.GrantorName} expires on {endDate:MMMM d, yyyy}. Remaining: {grant.RemainingBalance:C}",
                ComplianceType.GrantReport,
                endDate,
                daysUntil,
                daysUntil <= 30 ? ReminderPriority.High : ReminderPriority.Medium,
                $"/grants",
                false
            ));
        }

        return reminders;
    }

    public async Task<AuditThresholdStatusDto> CheckAuditThresholdAsync()
    {
        var startOfYear = new DateTime(DateTime.Today.Year, 1, 1);
        
        var ytdRevenue = await _context.Transactions
            .Where(t => t.Type == TransactionType.Income && t.Date >= startOfYear)
            .SumAsync(t => t.Amount);

        var percentage = (ytdRevenue / AuditThreshold) * 100;
        var isApproaching = percentage >= 75;
        var hasExceeded = ytdRevenue >= AuditThreshold;

        var message = hasExceeded 
            ? "Your organization has exceeded $1M in revenue and requires an independent audit."
            : isApproaching 
                ? $"Warning: Your organization is at {percentage:N0}% of the $1M audit threshold."
                : $"Current revenue is {percentage:N0}% of the $1M audit threshold.";

        return new AuditThresholdStatusDto(
            ytdRevenue,
            AuditThreshold,
            percentage,
            isApproaching,
            hasExceeded,
            message
        );
    }

    public Task DismissReminderAsync(string reminderId, DateTime? snoozeUntil = null)
    {
        // In a full implementation, this would save to a dismissals table
        // For now, just return completed
        return Task.CompletedTask;
    }

    private static ReminderPriority GetPriority(int daysUntilDue)
    {
        if (daysUntilDue <= 7) return ReminderPriority.Critical;
        if (daysUntilDue <= 30) return ReminderPriority.High;
        if (daysUntilDue <= 60) return ReminderPriority.Medium;
        return ReminderPriority.Low;
    }
}
