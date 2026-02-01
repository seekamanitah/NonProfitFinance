namespace NonProfitFinance.Services;

/// <summary>
/// Service for managing accessibility settings like UI scaling
/// </summary>
public interface IAccessibilityService
{
    /// <summary>
    /// UI scale factor (0.8 = 80%, 1.0 = 100%, 1.5 = 150%)
    /// </summary>
    float UiScale { get; set; }
    
    /// <summary>
    /// Load saved accessibility settings
    /// </summary>
    Task LoadSettingsAsync();
    
    /// <summary>
    /// Save accessibility settings
    /// </summary>
    Task SaveSettingsAsync();
}
