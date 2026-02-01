using NonProfitFinance.Models;

namespace NonProfitFinance.Services;

/// <summary>
/// Service for managing organization settings
/// </summary>
public interface IOrganizationService
{
    /// <summary>
    /// Get current organization settings
    /// </summary>
    Task<OrganizationSettings> GetSettingsAsync();
    
    /// <summary>
    /// Save organization settings
    /// </summary>
    Task SaveSettingsAsync(OrganizationSettings settings);
    
    /// <summary>
    /// Check if organization settings have been configured
    /// </summary>
    Task<bool> IsConfiguredAsync();
}
