using Microsoft.EntityFrameworkCore;
using NonProfitFinance.Data;
using NonProfitFinance.Models;

namespace NonProfitFinance.Services;

public class ImportPresetService : IImportPresetService
{
    private readonly ApplicationDbContext _context;

    public ImportPresetService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ImportPreset>> GetAllAsync(string? importType = null)
    {
        var query = _context.ImportPresets.AsQueryable();

        if (!string.IsNullOrEmpty(importType))
        {
            query = query.Where(p => p.ImportType == importType);
        }

        return await query
            .OrderByDescending(p => p.IsDefault)
            .ThenByDescending(p => p.LastUsedAt)
            .ThenBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<ImportPreset?> GetByIdAsync(int id)
    {
        return await _context.ImportPresets.FindAsync(id);
    }

    public async Task<ImportPreset?> GetDefaultAsync(string importType = "transactions")
    {
        return await _context.ImportPresets
            .Where(p => p.ImportType == importType && p.IsDefault)
            .FirstOrDefaultAsync();
    }

    public async Task<ImportPreset> CreateAsync(ImportPreset preset)
    {
        // If this is the first preset or marked as default, clear other defaults
        if (preset.IsDefault)
        {
            await ClearDefaultsAsync(preset.ImportType);
        }

        preset.CreatedAt = DateTime.UtcNow;
        _context.ImportPresets.Add(preset);
        await _context.SaveChangesAsync();
        return preset;
    }

    public async Task<ImportPreset?> UpdateAsync(int id, ImportPreset preset)
    {
        var existing = await _context.ImportPresets.FindAsync(id);
        if (existing == null) return null;

        // If setting as default, clear other defaults
        if (preset.IsDefault && !existing.IsDefault)
        {
            await ClearDefaultsAsync(preset.ImportType);
        }

        existing.Name = preset.Name;
        existing.Description = preset.Description;
        existing.ImportType = preset.ImportType;
        existing.DateColumn = preset.DateColumn;
        existing.AmountColumn = preset.AmountColumn;
        existing.DescriptionColumn = preset.DescriptionColumn;
        existing.TypeColumn = preset.TypeColumn;
        existing.CategoryColumn = preset.CategoryColumn;
        existing.FundColumn = preset.FundColumn;
        existing.DonorColumn = preset.DonorColumn;
        existing.GrantColumn = preset.GrantColumn;
        existing.PayeeColumn = preset.PayeeColumn;
        existing.TagsColumn = preset.TagsColumn;
        existing.HasHeaderRow = preset.HasHeaderRow;
        existing.DateFormat = preset.DateFormat;
        existing.IsDefault = preset.IsDefault;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var preset = await _context.ImportPresets.FindAsync(id);
        if (preset == null) return false;

        _context.ImportPresets.Remove(preset);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task SetAsDefaultAsync(int id)
    {
        var preset = await _context.ImportPresets.FindAsync(id);
        if (preset == null) return;

        await ClearDefaultsAsync(preset.ImportType);

        preset.IsDefault = true;
        await _context.SaveChangesAsync();
    }

    public async Task UpdateLastUsedAsync(int id)
    {
        var preset = await _context.ImportPresets.FindAsync(id);
        if (preset == null) return;

        preset.LastUsedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    private async Task ClearDefaultsAsync(string importType)
    {
        var defaults = await _context.ImportPresets
            .Where(p => p.ImportType == importType && p.IsDefault)
            .ToListAsync();

        foreach (var p in defaults)
        {
            p.IsDefault = false;
        }

        await _context.SaveChangesAsync();
    }
}
