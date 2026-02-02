using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NonProfitFinance.Data;

namespace NonProfitFinance.Services;

public class BackupService : IBackupService
{
    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BackupService> _logger;
    private readonly string _settingsFilePath;
    private readonly byte[] _encryptionKey;
    private BackupSettings _settings;
    
    // Default key used only if configuration is missing - HIGH-04 fix
    private const string FallbackKeyBase64 = "QXVkaXRSZW1lZGlhdGlvbjIwMjZLZXk9MDEyMzQ1Njc4OWFiY2RlZg==";

    public BackupService(
        IConfiguration configuration,
        IServiceScopeFactory scopeFactory,
        ILogger<BackupService> logger,
        IWebHostEnvironment environment)
    {
        _configuration = configuration;
        _scopeFactory = scopeFactory;
        _logger = logger;
        
        // Load encryption key from configuration or use fallback - HIGH-04 fix
        var configuredKey = _configuration["Backup:EncryptionKey"];
        if (!string.IsNullOrEmpty(configuredKey))
        {
            _encryptionKey = Convert.FromBase64String(configuredKey);
        }
        else
        {
            _logger.LogWarning("Backup:EncryptionKey not configured. Using fallback key. Configure a secure key in production!");
            _encryptionKey = Convert.FromBase64String(FallbackKeyBase64);
        }
        
        // Store settings in app data folder
        _settingsFilePath = Path.Combine(environment.ContentRootPath, "backup-settings.json");
        _settings = LoadSettings();
        
        // Set default backup directory if not configured
        if (string.IsNullOrEmpty(_settings.BackupDirectory))
        {
            _settings.BackupDirectory = GetDefaultBackupDirectory();
        }
    }

    public async Task<BackupResult> CreateBackupAsync(string? customPath = null)
    {
        try
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            var dbPath = GetDatabasePath(connectionString);
            
            if (!File.Exists(dbPath))
            {
                return new BackupResult(false, null, "Database file not found", 0, DateTime.UtcNow);
            }

            // Determine backup directory
            var backupDir = customPath ?? _settings.BackupDirectory;
            if (string.IsNullOrEmpty(backupDir))
            {
                backupDir = GetDefaultBackupDirectory();
            }

            // Ensure backup directory exists
            Directory.CreateDirectory(backupDir);

            // Generate backup filename with timestamp (use UTC for consistency)
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var backupFileName = $"nonprofit_backup_{timestamp}.db";
            var backupPath = Path.Combine(backupDir, backupFileName);

            // Use SQLite backup API for safe backup while database is in use
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                
                // Close any existing connections and perform backup
                await context.Database.CloseConnectionAsync();
                
                // Copy the database file
                using var sourceConnection = new SqliteConnection($"Data Source={dbPath}");
                using var destConnection = new SqliteConnection($"Data Source={backupPath}");
                
                await sourceConnection.OpenAsync();
                await destConnection.OpenAsync();
                
                sourceConnection.BackupDatabase(destConnection);
                
                await sourceConnection.CloseAsync();
                await destConnection.CloseAsync();
            }

            long fileSize = new FileInfo(backupPath).Length;

            // Compress if enabled
            if (_settings.CompressBackups)
            {
                var compressedPath = backupPath + ".gz";
                await CompressFileAsync(backupPath, compressedPath);
                File.Delete(backupPath);
                backupPath = compressedPath;
                fileSize = new FileInfo(backupPath).Length;
            }

            _logger.LogInformation("Database backup created: {BackupPath} ({Size} bytes)", backupPath, fileSize);

            // Cleanup old backups
            await CleanupOldBackupsAsync();

            return new BackupResult(true, backupPath, null, fileSize, DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create database backup");
            return new BackupResult(false, null, ex.Message, 0, DateTime.UtcNow);
        }
    }

    public Task<List<BackupInfo>> GetBackupsAsync()
    {
        var backups = new List<BackupInfo>();
        
        var backupDir = _settings.BackupDirectory;
        if (string.IsNullOrEmpty(backupDir) || !Directory.Exists(backupDir))
        {
            return Task.FromResult(backups);
        }

        var files = Directory.GetFiles(backupDir, "nonprofit_backup_*.*")
            .Where(f => f.EndsWith(".db") || f.EndsWith(".db.gz"));

        foreach (var file in files)
        {
            var info = new FileInfo(file);
            var isAutomatic = file.Contains("_auto_");
            
            backups.Add(new BackupInfo(
                info.Name,
                info.FullName,
                info.CreationTime,
                info.Length,
                isAutomatic
            ));
        }

        return Task.FromResult(backups.OrderByDescending(b => b.CreatedAt).ToList());
    }

    public async Task<BackupResult> RestoreFromBackupAsync(string backupPath)
    {
        try
        {
            if (!File.Exists(backupPath))
            {
                return new BackupResult(false, null, "Backup file not found", 0, DateTime.UtcNow);
            }

            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            var dbPath = GetDatabasePath(connectionString);

            // Create a backup of current database before restoring
            var preRestoreBackup = await CreateBackupAsync();
            if (!preRestoreBackup.Success)
            {
                return new BackupResult(false, null, "Failed to create pre-restore backup", 0, DateTime.UtcNow);
            }

            // Close all connections
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await context.Database.CloseConnectionAsync();
            }

            // Decompress if needed
            var restorePath = backupPath;
            if (backupPath.EndsWith(".gz"))
            {
                restorePath = Path.GetTempFileName();
                await DecompressFileAsync(backupPath, restorePath);
            }

            // Replace database file
            File.Copy(restorePath, dbPath, true);

            // Clean up temp file if we decompressed
            if (restorePath != backupPath)
            {
                File.Delete(restorePath);
            }

            _logger.LogInformation("Database restored from backup: {BackupPath}", backupPath);

            return new BackupResult(true, dbPath, null, new FileInfo(dbPath).Length, DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to restore database from backup");
            return new BackupResult(false, null, ex.Message, 0, DateTime.UtcNow);
        }
    }

    public Task<bool> DeleteBackupAsync(string backupPath)
    {
        try
        {
            if (File.Exists(backupPath))
            {
                File.Delete(backupPath);
                _logger.LogInformation("Backup deleted: {BackupPath}", backupPath);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete backup: {BackupPath}", backupPath);
            return Task.FromResult(false);
        }
    }

    public BackupSettings GetSettings()
    {
        return _settings;
    }

    public async Task SaveSettingsAsync(BackupSettings settings)
    {
        _settings = settings;
        
        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_settingsFilePath, json);
        
        _logger.LogInformation("Backup settings saved");
    }

    public async Task<int> CleanupOldBackupsAsync()
    {
        var backups = await GetBackupsAsync();
        var deletedCount = 0;

        // Apply retention by count
        if (_settings.RetentionCount > 0 && backups.Count > _settings.RetentionCount)
        {
            var toDelete = backups.Skip(_settings.RetentionCount).ToList();
            foreach (var backup in toDelete)
            {
                if (await DeleteBackupAsync(backup.FullPath))
                {
                    deletedCount++;
                }
            }
        }

        // Apply retention by age
        if (_settings.RetentionDays > 0)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-_settings.RetentionDays);
            var oldBackups = backups.Where(b => b.CreatedAt < cutoffDate).ToList();
            
            foreach (var backup in oldBackups)
            {
                if (await DeleteBackupAsync(backup.FullPath))
                {
                    deletedCount++;
                }
            }
        }

        if (deletedCount > 0)
        {
            _logger.LogInformation("Cleaned up {Count} old backups", deletedCount);
        }

        return deletedCount;
    }

    public string GetDefaultBackupDirectory()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appData, "NonProfitFinance", "Backups");
    }

    private BackupSettings LoadSettings()
    {
        try
        {
            if (File.Exists(_settingsFilePath))
            {
                var json = File.ReadAllText(_settingsFilePath);
                return JsonSerializer.Deserialize<BackupSettings>(json) ?? new BackupSettings();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load backup settings, using defaults");
        }
        
        return new BackupSettings();
    }

    private static string GetDatabasePath(string? connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Database connection string not found");
        }

        // Parse SQLite connection string to get Data Source
        var builder = new SqliteConnectionStringBuilder(connectionString);
        return builder.DataSource;
    }

    private static async Task CompressFileAsync(string sourcePath, string destPath)
    {
        await using var sourceStream = File.OpenRead(sourcePath);
        await using var destStream = File.Create(destPath);
        await using var gzipStream = new GZipStream(destStream, CompressionLevel.Optimal);
        await sourceStream.CopyToAsync(gzipStream);
    }

    private static async Task DecompressFileAsync(string sourcePath, string destPath)
    {
        await using var sourceStream = File.OpenRead(sourcePath);
        await using var gzipStream = new GZipStream(sourceStream, CompressionMode.Decompress);
        await using var destStream = File.Create(destPath);
        await gzipStream.CopyToAsync(destStream);
    }

    /// <summary>
    /// Encrypts a file using AES-256.
    /// </summary>
    private async Task EncryptFileAsync(string sourcePath, string destPath, byte[]? encryptionKey = null)
    {
        var key = encryptionKey ?? _encryptionKey;
        
        using var aes = Aes.Create();
        aes.Key = key;
        aes.GenerateIV();
        
        await using var destStream = File.Create(destPath);
        
        // Write IV to the beginning of the file
        await destStream.WriteAsync(aes.IV);
        
        await using var cryptoStream = new CryptoStream(destStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
        await using var sourceStream = File.OpenRead(sourcePath);
        await sourceStream.CopyToAsync(cryptoStream);
    }

    /// <summary>
    /// Decrypts a file encrypted with AES-256.
    /// </summary>
    private async Task DecryptFileAsync(string sourcePath, string destPath, byte[]? encryptionKey = null)
    {
        var key = encryptionKey ?? _encryptionKey;
        
        using var aes = Aes.Create();
        aes.Key = key;
        
        
        await using var sourceStream = File.OpenRead(sourcePath);
        
        // Read IV from the beginning of the file
        var iv = new byte[16];
        await sourceStream.ReadExactlyAsync(iv);
        aes.IV = iv;
        
        await using var cryptoStream = new CryptoStream(sourceStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
        await using var destStream = File.Create(destPath);
        await cryptoStream.CopyToAsync(destStream);
    }
}
