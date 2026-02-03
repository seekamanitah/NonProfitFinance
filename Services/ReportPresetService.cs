using Microsoft.EntityFrameworkCore;
using NonProfitFinance.Data;
using NonProfitFinance.Models;

namespace NonProfitFinance.Services;

public class ReportPresetService : IReportPresetService
{
    private readonly ApplicationDbContext _context;

    public ReportPresetService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ReportPreset>> GetAllPresetsAsync(string? reportType = null)
    {
        var query = _context.ReportPresets.AsQueryable();

        if (!string.IsNullOrEmpty(reportType))
        {
            query = query.Where(p => p.ReportType == reportType);
        }

        return await query
            .OrderByDescending(p => p.IsDefault)
            .ThenBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<ReportPreset?> GetPresetByIdAsync(int id)
    {
        return await _context.ReportPresets.FindAsync(id);
    }

    public async Task<ReportPreset?> GetDefaultPresetAsync(string reportType)
    {
        return await _context.ReportPresets
            .Where(p => p.ReportType == reportType && p.IsDefault)
            .FirstOrDefaultAsync();
    }

    public async Task<ReportPreset> SavePresetAsync(ReportPreset preset)
    {
        if (preset.IsDefault)
        {
            // Clear other defaults for this report type
            var existingDefaults = await _context.ReportPresets
                .Where(p => p.ReportType == preset.ReportType && p.IsDefault && p.Id != preset.Id)
                .ToListAsync();

            foreach (var existing in existingDefaults)
            {
                existing.IsDefault = false;
            }
        }

        if (preset.Id == 0)
        {
            preset.CreatedAt = DateTime.UtcNow;
            _context.ReportPresets.Add(preset);
        }
        else
        {
            preset.UpdatedAt = DateTime.UtcNow;
            _context.ReportPresets.Update(preset);
        }

        await _context.SaveChangesAsync();
        return preset;
    }

    public async Task DeletePresetAsync(int id)
    {
        var preset = await _context.ReportPresets.FindAsync(id);
        if (preset != null)
        {
            _context.ReportPresets.Remove(preset);
            await _context.SaveChangesAsync();
        }
    }

    public async Task SetDefaultPresetAsync(int id)
    {
        var preset = await _context.ReportPresets.FindAsync(id);
        if (preset == null) return;

        // Clear other defaults for this report type
        var existingDefaults = await _context.ReportPresets
            .Where(p => p.ReportType == preset.ReportType && p.IsDefault)
            .ToListAsync();

        foreach (var existing in existingDefaults)
        {
            existing.IsDefault = false;
        }

        preset.IsDefault = true;
        preset.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }
}
