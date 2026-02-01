using System.Globalization;

namespace NonProfitFinance.Helpers;

/// <summary>
/// Helper class for consistent currency and date formatting throughout the application.
/// Addresses audit finding UX-L02/L03 for format consistency.
/// </summary>
public static class FormatHelper
{
    // Default culture for US currency and date formatting
    private static readonly CultureInfo DefaultCulture = CultureInfo.GetCultureInfo("en-US");

    #region Currency Formatting

    /// <summary>
    /// Format as currency with cents (e.g., $1,234.56)
    /// </summary>
    public static string Currency(decimal amount) => amount.ToString("C2", DefaultCulture);

    /// <summary>
    /// Format as currency without cents (e.g., $1,235)
    /// </summary>
    public static string CurrencyWhole(decimal amount) => amount.ToString("C0", DefaultCulture);

    /// <summary>
    /// Format as currency with sign for positive/negative (e.g., +$100.00 or -$50.00)
    /// </summary>
    public static string CurrencyWithSign(decimal amount)
    {
        var prefix = amount >= 0 ? "+" : "";
        return prefix + amount.ToString("C2", DefaultCulture);
    }

    /// <summary>
    /// Format as compact currency (e.g., $1.2K, $1.5M)
    /// </summary>
    public static string CurrencyCompact(decimal amount)
    {
        return amount switch
        {
            >= 1_000_000_000 => $"${amount / 1_000_000_000:F1}B",
            >= 1_000_000 => $"${amount / 1_000_000:F1}M",
            >= 1_000 => $"${amount / 1_000:F1}K",
            _ => Currency(amount)
        };
    }

    #endregion

    #region Date Formatting

    /// <summary>
    /// Short date format (e.g., 1/29/2026)
    /// </summary>
    public static string ShortDate(DateTime date) => date.ToString("d", DefaultCulture);

    /// <summary>
    /// Medium date format (e.g., Jan 29, 2026)
    /// </summary>
    public static string MediumDate(DateTime date) => date.ToString("MMM d, yyyy", DefaultCulture);

    /// <summary>
    /// Long date format (e.g., January 29, 2026)
    /// </summary>
    public static string LongDate(DateTime date) => date.ToString("MMMM d, yyyy", DefaultCulture);

    /// <summary>
    /// Date with time (e.g., Jan 29, 2026 3:45 PM)
    /// </summary>
    public static string DateTime(DateTime date) => date.ToString("MMM d, yyyy h:mm tt", DefaultCulture);

    /// <summary>
    /// Time only (e.g., 3:45 PM)
    /// </summary>
    public static string TimeOnly(DateTime date) => date.ToString("h:mm tt", DefaultCulture);

    /// <summary>
    /// ISO date format for APIs (e.g., 2026-01-29)
    /// </summary>
    public static string IsoDate(DateTime date) => date.ToString("yyyy-MM-dd");

    /// <summary>
    /// Relative date (e.g., "Today", "Yesterday", "3 days ago")
    /// </summary>
    public static string RelativeDate(DateTime date)
    {
        var today = System.DateTime.Today;
        var diff = (today - date.Date).Days;

        return diff switch
        {
            0 => "Today",
            1 => "Yesterday",
            < 7 => $"{diff} days ago",
            < 14 => "Last week",
            < 30 => $"{diff / 7} weeks ago",
            < 60 => "Last month",
            < 365 => $"{diff / 30} months ago",
            _ => MediumDate(date)
        };
    }

    /// <summary>
    /// Month and year only (e.g., January 2026)
    /// </summary>
    public static string MonthYear(DateTime date) => date.ToString("MMMM yyyy", DefaultCulture);

    /// <summary>
    /// Month abbreviation and year (e.g., Jan 2026)
    /// </summary>
    public static string ShortMonthYear(DateTime date) => date.ToString("MMM yyyy", DefaultCulture);

    #endregion

    #region Number Formatting

    /// <summary>
    /// Format number with thousand separators
    /// </summary>
    public static string Number(decimal number, int decimals = 2) 
        => number.ToString($"N{decimals}", DefaultCulture);

    /// <summary>
    /// Format as percentage (e.g., 45.5%)
    /// </summary>
    public static string Percent(decimal value, int decimals = 1) 
        => (value / 100m).ToString($"P{decimals}", DefaultCulture);

    /// <summary>
    /// Format decimal as percentage (e.g., 0.455 -> 45.5%)
    /// </summary>
    public static string PercentFromDecimal(decimal value, int decimals = 1)
        => value.ToString($"P{decimals}", DefaultCulture);

    #endregion
}
