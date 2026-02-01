using System.Text.Json;
using NonProfitFinance.Models;

namespace NonProfitFinance.Services;

/// <summary>
/// Service for managing organization settings
/// Stores settings in a JSON file in the application data directory
/// </summary>
public class OrganizationService : IOrganizationService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<OrganizationService> _logger;
    private readonly string _settingsFilePath;
    private OrganizationSettings? _cachedSettings;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public OrganizationService(IWebHostEnvironment environment, ILogger<OrganizationService> logger)
    {
        _environment = environment;
        _logger = logger;
        
        // Store settings in the application's content root
        var settingsDirectory = Path.Combine(environment.ContentRootPath, "AppData");
        if (!Directory.Exists(settingsDirectory))
        {
            Directory.CreateDirectory(settingsDirectory);
        }
        
        _settingsFilePath = Path.Combine(settingsDirectory, "organization-settings.json");
    }

    public async Task<OrganizationSettings> GetSettingsAsync()
    {
        await _lock.WaitAsync();
        try
        {
            if (_cachedSettings != null)
                return _cachedSettings;

            if (File.Exists(_settingsFilePath))
            {
                var json = await File.ReadAllTextAsync(_settingsFilePath);
                _cachedSettings = JsonSerializer.Deserialize<OrganizationSettings>(json);
                
                if (_cachedSettings != null)
                {
                    _logger.LogInformation("Loaded organization settings from {Path}", _settingsFilePath);
                    return _cachedSettings;
                }
            }

            // Return default settings if file doesn't exist
            _cachedSettings = new OrganizationSettings
            {
                OrganizationName = "Your Organization Name",
                State = "Tennessee"
            };
            
            return _cachedSettings;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading organization settings");
            return new OrganizationSettings();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task SaveSettingsAsync(OrganizationSettings settings)
    {
        await _lock.WaitAsync();
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            
            var json = JsonSerializer.Serialize(settings, options);
            await File.WriteAllTextAsync(_settingsFilePath, json);
            
            _cachedSettings = settings;
            
            _logger.LogInformation("Saved organization settings to {Path}", _settingsFilePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving organization settings");
            throw;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<bool> IsConfiguredAsync()
    {
        var settings = await GetSettingsAsync();
        
        // Consider configured if at least org name and EIN are set
        return !string.IsNullOrWhiteSpace(settings.OrganizationName) &&
               settings.OrganizationName != "Your Organization Name" &&
               !string.IsNullOrWhiteSpace(settings.EIN);
    }
}
