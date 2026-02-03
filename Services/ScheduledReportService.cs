using Microsoft.EntityFrameworkCore;
using NonProfitFinance.Data;
using NonProfitFinance.Models;

namespace NonProfitFinance.Services;

public class ScheduledReportService : IScheduledReportService
{
    private readonly ApplicationDbContext _context;

    public ScheduledReportService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ScheduledReport>> GetAllAsync()
    {
        return await _context.ScheduledReports
            .OrderBy(r => r.Name)
            .ToListAsync();
    }

    public async Task<ScheduledReport?> GetByIdAsync(int id)
    {
        return await _context.ScheduledReports.FindAsync(id);
    }

    public async Task<List<ScheduledReport>> GetDueReportsAsync()
    {
        var now = DateTime.UtcNow;
        return await _context.ScheduledReports
            .Where(r => r.IsActive && r.NextRunAt <= now)
            .ToListAsync();
    }

    public async Task<ScheduledReport> SaveAsync(ScheduledReport report)
    {
        // Calculate next run time
        report.NextRunAt = CalculateNextRunTime(report);

        if (report.Id == 0)
        {
            report.CreatedAt = DateTime.UtcNow;
            _context.ScheduledReports.Add(report);
        }
        else
        {
            report.UpdatedAt = DateTime.UtcNow;
            _context.ScheduledReports.Update(report);
        }

        await _context.SaveChangesAsync();
        return report;
    }

    public async Task DeleteAsync(int id)
    {
        var report = await _context.ScheduledReports.FindAsync(id);
        if (report != null)
        {
            _context.ScheduledReports.Remove(report);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ToggleActiveAsync(int id)
    {
        var report = await _context.ScheduledReports.FindAsync(id);
        if (report == null) return false;

        report.IsActive = !report.IsActive;
        if (report.IsActive)
        {
            report.NextRunAt = CalculateNextRunTime(report);
            report.FailureCount = 0;
        }

        report.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return report.IsActive;
    }

    public async Task RecordRunAsync(int id, bool success, string? message = null)
    {
        var report = await _context.ScheduledReports.FindAsync(id);
        if (report == null) return;

        report.LastRunAt = DateTime.UtcNow;
        report.LastRunStatus = success ? "Success" : $"Failed: {message}";

        if (success)
        {
            report.FailureCount = 0;
            report.NextRunAt = CalculateNextRunTime(report);
        }
        else
        {
            report.FailureCount++;
            // Disable after 5 consecutive failures
            if (report.FailureCount >= 5)
            {
                report.IsActive = false;
                report.LastRunStatus = "Disabled after 5 failures: " + message;
            }
            else
            {
                // Retry in 1 hour
                report.NextRunAt = DateTime.UtcNow.AddHours(1);
            }
        }

        report.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public Task<(DateTime Start, DateTime End)> GetPeriodDatesAsync(ReportPeriodType periodType)
    {
        var today = DateTime.Today;
        DateTime start, end;

        switch (periodType)
        {
            case ReportPeriodType.PreviousDay:
                start = today.AddDays(-1);
                end = today.AddDays(-1);
                break;

            case ReportPeriodType.PreviousWeek:
                var lastSunday = today.AddDays(-(int)today.DayOfWeek - 7);
                start = lastSunday;
                end = lastSunday.AddDays(6);
                break;

            case ReportPeriodType.PreviousMonth:
                var lastMonth = today.AddMonths(-1);
                start = new DateTime(lastMonth.Year, lastMonth.Month, 1);
                end = start.AddMonths(1).AddDays(-1);
                break;

            case ReportPeriodType.PreviousQuarter:
                var currentQuarter = (today.Month - 1) / 3;
                var prevQuarterStart = currentQuarter == 0
                    ? new DateTime(today.Year - 1, 10, 1)
                    : new DateTime(today.Year, (currentQuarter - 1) * 3 + 1, 1);
                start = prevQuarterStart;
                end = prevQuarterStart.AddMonths(3).AddDays(-1);
                break;

            case ReportPeriodType.PreviousYear:
                start = new DateTime(today.Year - 1, 1, 1);
                end = new DateTime(today.Year - 1, 12, 31);
                break;

            case ReportPeriodType.MonthToDate:
                start = new DateTime(today.Year, today.Month, 1);
                end = today;
                break;

            case ReportPeriodType.QuarterToDate:
                var quarterStart = new DateTime(today.Year, ((today.Month - 1) / 3) * 3 + 1, 1);
                start = quarterStart;
                end = today;
                break;

            case ReportPeriodType.YearToDate:
                start = new DateTime(today.Year, 1, 1);
                end = today;
                break;

            case ReportPeriodType.Last30Days:
                start = today.AddDays(-29);
                end = today;
                break;

            case ReportPeriodType.Last90Days:
                start = today.AddDays(-89);
                end = today;
                break;

            case ReportPeriodType.Last12Months:
                start = today.AddMonths(-11).AddDays(1 - today.Day);
                end = today;
                break;

            default:
                start = today.AddMonths(-1);
                end = today;
                break;
        }

        return Task.FromResult((start, end));
    }

    private DateTime CalculateNextRunTime(ScheduledReport report)
    {
        var now = DateTime.UtcNow;
        var today = now.Date;
        var runTime = report.TimeOfDay.ToTimeSpan();

        DateTime nextRun;

        switch (report.Frequency)
        {
            case ScheduleFrequency.Daily:
                nextRun = today.Add(runTime);
                if (nextRun <= now)
                    nextRun = nextRun.AddDays(1);
                break;

            case ScheduleFrequency.Weekly:
                var targetDay = report.DayOfWeek ?? DayOfWeek.Monday;
                var daysUntilTarget = ((int)targetDay - (int)now.DayOfWeek + 7) % 7;
                if (daysUntilTarget == 0 && now.TimeOfDay >= runTime)
                    daysUntilTarget = 7;
                nextRun = today.AddDays(daysUntilTarget).Add(runTime);
                break;

            case ScheduleFrequency.Monthly:
                var dayOfMonth = Math.Min(report.DayOfMonth ?? 1, DateTime.DaysInMonth(today.Year, today.Month));
                nextRun = new DateTime(today.Year, today.Month, dayOfMonth).Add(runTime);
                if (nextRun <= now)
                {
                    var nextMonth = today.AddMonths(1);
                    dayOfMonth = Math.Min(report.DayOfMonth ?? 1, DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month));
                    nextRun = new DateTime(nextMonth.Year, nextMonth.Month, dayOfMonth).Add(runTime);
                }
                break;

            case ScheduleFrequency.Quarterly:
                var currentQStart = new DateTime(today.Year, ((today.Month - 1) / 3) * 3 + 1, 1);
                var qDayOfMonth = Math.Min(report.DayOfMonth ?? 1, DateTime.DaysInMonth(currentQStart.Year, currentQStart.Month));
                nextRun = new DateTime(currentQStart.Year, currentQStart.Month, qDayOfMonth).Add(runTime);
                if (nextRun <= now)
                {
                    var nextQStart = currentQStart.AddMonths(3);
                    qDayOfMonth = Math.Min(report.DayOfMonth ?? 1, DateTime.DaysInMonth(nextQStart.Year, nextQStart.Month));
                    nextRun = new DateTime(nextQStart.Year, nextQStart.Month, qDayOfMonth).Add(runTime);
                }
                break;

            case ScheduleFrequency.Yearly:
                var yearDayOfMonth = Math.Min(report.DayOfMonth ?? 1, 28); // Safe for February
                nextRun = new DateTime(today.Year, 1, yearDayOfMonth).Add(runTime);
                if (nextRun <= now)
                {
                    nextRun = new DateTime(today.Year + 1, 1, yearDayOfMonth).Add(runTime);
                }
                break;

            default:
                nextRun = now.AddDays(1);
                break;
        }

        return nextRun;
    }
}
