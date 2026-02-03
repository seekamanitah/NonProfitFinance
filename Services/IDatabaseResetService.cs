namespace NonProfitFinance.Services;

/// <summary>
/// Service for safely resetting the database.
/// Handles deletion of all data with emergency backup creation.
/// </summary>
public interface IDatabaseResetService
{
    /// <summary>
    /// Creates an emergency backup and deletes all database data.
    /// </summary>
    /// <returns>Result with success status and backup file path</returns>
    Task<DatabaseResetResult> ResetDatabaseAsync();
}

/// <summary>
/// Result of database reset operation
/// </summary>
public class DatabaseResetResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? BackupFilePath { get; set; }
    public DateTime ResetTime { get; set; } = DateTime.UtcNow;
}
