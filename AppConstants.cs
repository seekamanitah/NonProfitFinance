namespace NonProfitFinance;

/// <summary>
/// Application-wide constants to avoid magic strings.
/// LOW-02 fix: Centralized constants for type-safe references.
/// </summary>
public static class AppConstants
{
    /// <summary>
    /// Category names used throughout the application.
    /// </summary>
    public static class CategoryNames
    {
        public const string Transfer = "Transfer";
        public const string GeneralIncome = "General Income";
        public const string GeneralExpense = "General Expense";
        public const string Donation = "Donation";
        public const string Grant = "Grant Income";
        public const string Salary = "Salary & Wages";
        public const string Utilities = "Utilities";
        public const string Supplies = "Supplies";
        public const string Equipment = "Equipment";
        public const string Rent = "Rent";
        public const string Insurance = "Insurance";
        public const string Other = "Other";
    }

    /// <summary>
    /// Fund names used for default funds.
    /// </summary>
    public static class FundNames
    {
        public const string GeneralOperating = "General Operating";
        public const string Building = "Building Fund";
        public const string Emergency = "Emergency Reserve";
        public const string Restricted = "Restricted Donations";
    }

    /// <summary>
    /// Status values for various entities.
    /// </summary>
    public static class Status
    {
        public const string Active = "Active";
        public const string Inactive = "Inactive";
        public const string Pending = "Pending";
        public const string Completed = "Completed";
        public const string Cancelled = "Cancelled";
        public const string Draft = "Draft";
        public const string Submitted = "Submitted";
        public const string Approved = "Approved";
        public const string Rejected = "Rejected";
    }

    /// <summary>
    /// Recurrence patterns for recurring transactions.
    /// </summary>
    public static class RecurrencePatterns
    {
        public const string Daily = "Daily";
        public const string Weekly = "Weekly";
        public const string BiWeekly = "BiWeekly";
        public const string Monthly = "Monthly";
        public const string Quarterly = "Quarterly";
        public const string Annually = "Annually";
    }

    /// <summary>
    /// Date formats used throughout the application.
    /// LOW-07 fix: Standardized date formats.
    /// </summary>
    public static class DateFormats
    {
        public const string Display = "MMM dd, yyyy";
        public const string ShortDisplay = "MM/dd/yyyy";
        public const string Iso = "yyyy-MM-dd";
        public const string MonthYear = "MMM yyyy";
        public const string FullDateTime = "MMM dd, yyyy h:mm tt";
    }

    /// <summary>
    /// Currency formats used throughout the application.
    /// LOW-06 fix: Standardized currency formats.
    /// </summary>
    public static class CurrencyFormats
    {
        public const string Default = "C2";
        public const string NoDecimals = "C0";
        public const string WithSymbol = "${0:N2}";
    }

    /// <summary>
    /// Pagination defaults.
    /// </summary>
    public static class Pagination
    {
        public const int DefaultPageSize = 25;
        public const int MaxPageSize = 100;
        public const int SmallPageSize = 10;
        public const int LargePageSize = 50;
    }

    /// <summary>
    /// File upload constraints.
    /// </summary>
    public static class FileUpload
    {
        public const long MaxFileSizeBytes = 50 * 1024 * 1024; // 50 MB
        public const long MaxFileSizeMB = 50;
        public static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
        public static readonly string[] AllowedDocumentExtensions = { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".csv", ".txt" };
    }

    /// <summary>
    /// Audit action names.
    /// </summary>
    public static class AuditActions
    {
        public const string Create = "Create";
        public const string Update = "Update";
        public const string Delete = "Delete";
        public const string Login = "Login";
        public const string Logout = "Logout";
        public const string Export = "Export";
        public const string Import = "Import";
        public const string BulkImport = "BulkImport";
    }

    /// <summary>
    /// Entity names for audit logging.
    /// </summary>
    public static class EntityNames
    {
        public const string Transaction = "Transaction";
        public const string Category = "Category";
        public const string Fund = "Fund";
        public const string Donor = "Donor";
        public const string Grant = "Grant";
        public const string Document = "Document";
        public const string Budget = "Budget";
        public const string User = "User";
    }
}
