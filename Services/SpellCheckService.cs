using Microsoft.JSInterop;

namespace NonProfitFinance.Services;

/// <summary>
/// Service for managing spell checking functionality across the application
/// </summary>
public interface ISpellCheckService
{
    /// <summary>
    /// Initialize spell checker with custom dictionary
    /// </summary>
    Task InitializeAsync();
    
    /// <summary>
    /// Enable or disable spell checking
    /// </summary>
    Task SetEnabledAsync(bool enabled);
    
    /// <summary>
    /// Get current spell check settings
    /// </summary>
    SpellCheckSettings GetSettings();
    
    /// <summary>
    /// Save spell check settings
    /// </summary>
    Task SaveSettingsAsync(SpellCheckSettings settings);
    
    /// <summary>
    /// Add word to custom dictionary
    /// </summary>
    Task AddToDictionaryAsync(string word);
    
    /// <summary>
    /// Check if spell checking is enabled
    /// </summary>
    bool IsEnabled();
}

public class SpellCheckService : ISpellCheckService
{
    private readonly IJSRuntime _jsRuntime;
    private SpellCheckSettings _settings = new();
    
    // Common nonprofit/finance terms to add to custom dictionary
    private static readonly string[] CustomDictionary = new[]
    {
        // Nonprofit terms
        "nonprofit", "nonprofits", "501c3", "fundraiser", "fundraising", "grantor", "grantee",
        "donee", "unrestricted", "endowment", "bylaws",
        
        // Financial terms
        "reconciliation", "reimbursement", "budgeted", "unbudgeted", "subcategory", "subcategories",
        "payee", "payer", "backend",
        
        // Form/Document terms
        "reimbursable", "checkbox", "dropdown", "textarea",
        
        // Organization-specific (can be customized)
        "AFG", "FEMA", "SAFER", "SCBA", "PPE"
    };

    public SpellCheckService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task InitializeAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("spellCheck.initialize", _settings.Enabled, CustomDictionary);
        }
        catch
        {
            // JavaScript not ready or spell check initialization failed
        }
    }

    public async Task SetEnabledAsync(bool enabled)
    {
        _settings.Enabled = enabled;
        try
        {
            await _jsRuntime.InvokeVoidAsync("spellCheck.setEnabled", enabled);
        }
        catch
        {
            // JavaScript interop failed
        }
    }

    public SpellCheckSettings GetSettings() => _settings;

    public Task SaveSettingsAsync(SpellCheckSettings settings)
    {
        _settings = settings;
        // In production, save to local storage or user preferences
        return Task.CompletedTask;
    }

    public async Task AddToDictionaryAsync(string word)
    {
        if (string.IsNullOrWhiteSpace(word)) return;
        
        try
        {
            await _jsRuntime.InvokeVoidAsync("spellCheck.addToCustomDictionary", word.Trim().ToLower());
        }
        catch
        {
            // JavaScript interop failed
        }
    }

    public bool IsEnabled() => _settings.Enabled;
}

/// <summary>
/// Spell check configuration settings
/// </summary>
public class SpellCheckSettings
{
    /// <summary>
    /// Enable/disable spell checking globally
    /// </summary>
    public bool Enabled { get; set; } = true;
    
    /// <summary>
    /// Use browser native spell check
    /// </summary>
    public bool UseBrowserSpellCheck { get; set; } = true;
    
    /// <summary>
    /// Use enhanced spell check (Typo.js)
    /// </summary>
    public bool UseEnhancedSpellCheck { get; set; } = false;
    
    /// <summary>
    /// Underline misspelled words
    /// </summary>
    public bool UnderlineErrors { get; set; } = true;
    
    /// <summary>
    /// Language code (en_US, en_GB, etc.)
    /// </summary>
    public string Language { get; set; } = "en_US";
    
    /// <summary>
    /// Check spelling as user types (vs on submit)
    /// </summary>
    public bool CheckAsYouType { get; set; } = true;
}
