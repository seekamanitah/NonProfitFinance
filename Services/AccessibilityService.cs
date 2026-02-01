using Microsoft.JSInterop;

namespace NonProfitFinance.Services;

/// <summary>
/// Service for managing accessibility settings
/// </summary>
public class AccessibilityService : IAccessibilityService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<AccessibilityService> _logger;
    private float _uiScale = 1.0f;

    public float UiScale
    {
        get => _uiScale;
        set => _uiScale = Math.Clamp(value, 0.8f, 1.5f); // Limit between 80% and 150%
    }

    public AccessibilityService(IJSRuntime jsRuntime, ILogger<AccessibilityService> logger)
    {
        _jsRuntime = jsRuntime;
        _logger = logger;
    }

    public async Task LoadSettingsAsync()
    {
        try
        {
            var savedScale = await _jsRuntime.InvokeAsync<float?>("localStorage.getItem", "uiScale");
            if (savedScale.HasValue && savedScale.Value >= 0.8f && savedScale.Value <= 1.5f)
            {
                _uiScale = savedScale.Value;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading accessibility settings");
        }
    }

    public async Task SaveSettingsAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "uiScale", _uiScale);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving accessibility settings");
        }
    }
}
