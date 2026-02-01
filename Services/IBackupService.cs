namespace NonProfitFinance.Services;

public interface IBackupService
{
    /// <summary>
    /// Create a backup of the database immediately
    /// </summary>
    Task<BackupResult> CreateBackupAsync(string? customPath = null);
    
    /// <summary>
    /// Get list of existing backups
    /// </summary>
    Task<List<BackupInfo>> GetBackupsAsync();
    
    /// <summary>
    /// Restore database from a backup file
    /// </summary>
    Task<BackupResult> RestoreFromBackupAsync(string backupPath);
    
    /// <summary>
    /// Delete a specific backup
    /// </summary>
    Task<bool> DeleteBackupAsync(string backupPath);
    
    /// <summary>
    /// Get current backup settings
    /// </summary>
    BackupSettings GetSettings();
    
    /// <summary>
    /// Update backup settings
    /// </summary>
    Task SaveSettingsAsync(BackupSettings settings);
    
    /// <summary>
    /// Clean up old backups based on retention policy
    /// </summary>
    Task<int> CleanupOldBackupsAsync();
    
    /// <summary>
    /// Get the default backup directory
    /// </summary>
    string GetDefaultBackupDirectory();
}

public record BackupResult(
    bool Success,
    string? FilePath,
    string? Error,
    long FileSizeBytes,
    DateTime Timestamp
);

public record BackupInfo(
    string FileName,
    string FullPath,
    DateTime CreatedAt,
    long SizeBytes,
    bool IsAutomatic
);

public class BackupSettings
{
    /// <summary>
    /// Enable automatic scheduled backups
    /// </summary>
    public bool AutoBackupEnabled { get; set; } = false;
    
    /// <summary>
    /// Backup schedule type
    /// </summary>
    public BackupSchedule Schedule { get; set; } = BackupSchedule.Daily;
    
    /// <summary>
    /// Time of day to run automatic backups (24-hour format)
    /// </summary>
    public TimeSpan BackupTime { get; set; } = new TimeSpan(2, 0, 0); // 2:00 AM
    
    /// <summary>
    /// Directory to store backups
    /// </summary>
    public string BackupDirectory { get; set; } = "";
    
    /// <summary>
    /// Number of backups to retain (0 = unlimited)
    /// </summary>
    public int RetentionCount { get; set; } = 10;
    
    /// <summary>
    /// Days to retain backups (0 = unlimited)
    /// </summary>
    public int RetentionDays { get; set; } = 30;
    
    /// <summary>
    /// Last automatic backup timestamp
    /// </summary>
    public DateTime? LastAutoBackup { get; set; }
    
    /// <summary>
    /// Compress backups using gzip
    /// </summary>
    public bool CompressBackups { get; set; } = true;
}

public enum BackupSchedule
{
    Hourly,
    Daily,
    Weekly,
    Monthly
}
