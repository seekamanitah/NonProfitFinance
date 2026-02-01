namespace NonProfitFinance.Services;

/// <summary>
/// Background service that runs scheduled database backups based on configured settings
/// </summary>
public class BackupHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BackupHostedService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5);

    public BackupHostedService(
        IServiceScopeFactory scopeFactory,
        ILogger<BackupHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Backup scheduler service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAndRunScheduledBackupAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in backup scheduler");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("Backup scheduler service stopped");
    }

    private async Task CheckAndRunScheduledBackupAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var backupService = scope.ServiceProvider.GetRequiredService<IBackupService>();
        
        var settings = backupService.GetSettings();
        
        if (!settings.AutoBackupEnabled)
        {
            return;
        }

        var now = DateTime.UtcNow;
        var shouldBackup = ShouldRunBackup(settings, now);

        if (shouldBackup)
        {
            _logger.LogInformation("Starting scheduled automatic backup");
            
            // Update the backup directory to indicate it's automatic
            var result = await backupService.CreateBackupAsync();
            
            if (result.Success)
            {
                // Rename to indicate automatic backup
                if (!string.IsNullOrEmpty(result.FilePath) && File.Exists(result.FilePath))
                {
                    var dir = Path.GetDirectoryName(result.FilePath)!;
                    var ext = Path.GetExtension(result.FilePath);
                    var name = Path.GetFileNameWithoutExtension(result.FilePath);
                    var newPath = Path.Combine(dir, $"{name}_auto{ext}");
                    
                    if (File.Exists(newPath))
                    {
                        File.Delete(newPath);
                    }
                    File.Move(result.FilePath, newPath);
                }

                // Update last backup time
                settings.LastAutoBackup = now;
                await backupService.SaveSettingsAsync(settings);
                
                _logger.LogInformation("Automatic backup completed successfully");
            }
            else
            {
                _logger.LogError("Automatic backup failed: {Error}", result.Error);
            }
        }
    }

    private static bool ShouldRunBackup(BackupSettings settings, DateTime now)
    {
        // Check if we're at or past the scheduled backup time
        var currentTime = now.TimeOfDay;
        var scheduledTime = settings.BackupTime;
        
        // Only proceed if we're within the check window of the scheduled time
        if (currentTime < scheduledTime || currentTime > scheduledTime.Add(TimeSpan.FromMinutes(10)))
        {
            return false;
        }

        // Check if we've already done a backup today (or in the current period)
        if (settings.LastAutoBackup.HasValue)
        {
            var lastBackup = settings.LastAutoBackup.Value;
            
            switch (settings.Schedule)
            {
                case BackupSchedule.Hourly:
                    if (now - lastBackup < TimeSpan.FromHours(1))
                        return false;
                    break;
                    
                case BackupSchedule.Daily:
                    if (lastBackup.Date == now.Date)
                        return false;
                    break;
                    
                case BackupSchedule.Weekly:
                    var startOfWeek = now.Date.AddDays(-(int)now.DayOfWeek);
                    if (lastBackup.Date >= startOfWeek)
                        return false;
                    break;
                    
                case BackupSchedule.Monthly:
                    if (lastBackup.Year == now.Year && lastBackup.Month == now.Month)
                        return false;
                    break;
            }
        }

        return true;
    }
}
