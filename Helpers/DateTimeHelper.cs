namespace NonProfitFinance.Helpers;

/// <summary>
/// Helper class for consistent DateTime handling across the application.
/// All database storage should use UTC. Display should convert to local time.
/// </summary>
public static class DateTimeHelper
{
    /// <summary>
    /// Gets current UTC time. Use this instead of DateTime.Now or DateTime.UtcNow.
    /// </summary>
    public static DateTime UtcNow => DateTime.UtcNow;

    /// <summary>
    /// Gets current date in UTC. Use for date-only comparisons.
    /// </summary>
    public static DateTime UtcToday => DateTime.UtcNow.Date;

    /// <summary>
    /// Converts a UTC DateTime to local time for display.
    /// </summary>
    public static DateTime ToLocalTime(DateTime utcDateTime)
    {
        if (utcDateTime.Kind == DateTimeKind.Utc)
            return utcDateTime.ToLocalTime();
        
        // Assume it's UTC if unspecified
        if (utcDateTime.Kind == DateTimeKind.Unspecified)
            return DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc).ToLocalTime();
        
        return utcDateTime;
    }

    /// <summary>
    /// Converts a local DateTime to UTC for storage.
    /// </summary>
    public static DateTime ToUtc(DateTime localDateTime)
    {
        if (localDateTime.Kind == DateTimeKind.Local)
            return localDateTime.ToUniversalTime();
        
        if (localDateTime.Kind == DateTimeKind.Utc)
            return localDateTime;
        
        // Assume local if unspecified
        return DateTime.SpecifyKind(localDateTime, DateTimeKind.Local).ToUniversalTime();
    }

    /// <summary>
    /// Formats a UTC DateTime for display in local time.
    /// </summary>
    public static string FormatLocalDate(DateTime utcDateTime, string format = "MMM d, yyyy")
    {
        return ToLocalTime(utcDateTime).ToString(format);
    }

    /// <summary>
    /// Formats a UTC DateTime with time for display.
    /// </summary>
    public static string FormatLocalDateTime(DateTime utcDateTime, string format = "MMM d, yyyy h:mm tt")
    {
        return ToLocalTime(utcDateTime).ToString(format);
    }

    /// <summary>
    /// Gets the start of the current month in UTC.
    /// </summary>
    public static DateTime StartOfMonth => new DateTime(UtcNow.Year, UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// Gets the start of the current year in UTC.
    /// </summary>
    public static DateTime StartOfYear => new DateTime(UtcNow.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// Gets the end of the current month in UTC.
    /// </summary>
    public static DateTime EndOfMonth => StartOfMonth.AddMonths(1).AddTicks(-1);

    /// <summary>
    /// Gets the end of the current year in UTC.
    /// </summary>
    public static DateTime EndOfYear => new DateTime(UtcNow.Year, 12, 31, 23, 59, 59, 999, DateTimeKind.Utc);
}
