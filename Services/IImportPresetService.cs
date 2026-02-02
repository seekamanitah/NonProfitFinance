using NonProfitFinance.Models;

namespace NonProfitFinance.Services;

public interface IImportPresetService
{
    /// <summary>
    /// Get all import presets, optionally filtered by type.
    /// </summary>
    Task<List<ImportPreset>> GetAllAsync(string? importType = null);

    /// <summary>
    /// Get a specific preset by ID.
    /// </summary>
    Task<ImportPreset?> GetByIdAsync(int id);

    /// <summary>
    /// Get the default preset for a given import type.
    /// </summary>
    Task<ImportPreset?> GetDefaultAsync(string importType = "transactions");

    /// <summary>
    /// Create a new preset.
    /// </summary>
    Task<ImportPreset> CreateAsync(ImportPreset preset);

    /// <summary>
    /// Update an existing preset.
    /// </summary>
    Task<ImportPreset?> UpdateAsync(int id, ImportPreset preset);

    /// <summary>
    /// Delete a preset.
    /// </summary>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Set a preset as the default.
    /// </summary>
    Task SetAsDefaultAsync(int id);

    /// <summary>
    /// Update the last used timestamp for a preset.
    /// </summary>
    Task UpdateLastUsedAsync(int id);
}
