using Microsoft.EntityFrameworkCore;
using NonProfitFinance.Data;

namespace NonProfitFinance.Services;

public interface IPONumberService
{
    /// <summary>
    /// Generate the next PO number based on configured format
    /// </summary>
    Task<string> GenerateNextPONumberAsync();

    /// <summary>
    /// Validate if a PO number is unique
    /// </summary>
    Task<bool> IsPONumberUniqueAsync(string poNumber);

    /// <summary>
    /// Get or set PO number format pattern
    /// </summary>
    string GetPONumberFormat();
    
    /// <summary>
    /// Set custom PO number format
    /// </summary>
    void SetPONumberFormat(string format);
}

public class PONumberService : IPONumberService
{
    private readonly ApplicationDbContext _context;
    private string _format = "PO-{YYYY}-{####}"; // Default format

    public PONumberService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> GenerateNextPONumberAsync()
    {
        var format = GetPONumberFormat();
        var year = DateTime.UtcNow.Year;
        
        // Get the highest number for current year
        var prefix = format.Replace("{YYYY}", year.ToString()).Split(new[] { "{####}" }, StringSplitOptions.None)[0];
        
        var lastPO = await _context.Transactions
            .Where(t => t.PONumber != null && t.PONumber.StartsWith(prefix))
            .OrderByDescending(t => t.PONumber)
            .Select(t => t.PONumber)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        
        if (lastPO != null)
        {
            // Extract number from last PO
            var numberPart = lastPO.Replace(prefix, "");
            if (int.TryParse(numberPart, out int currentNumber))
            {
                nextNumber = currentNumber + 1;
            }
        }

        // Generate new PO number
        var poNumber = format
            .Replace("{YYYY}", year.ToString())
            .Replace("{YY}", year.ToString().Substring(2))
            .Replace("{MM}", DateTime.UtcNow.Month.ToString("D2"))
            .Replace("{####}", nextNumber.ToString("D4"))
            .Replace("{###}", nextNumber.ToString("D3"));

        return poNumber;
    }

    public async Task<bool> IsPONumberUniqueAsync(string poNumber)
    {
        if (string.IsNullOrWhiteSpace(poNumber))
            return true;

        return !await _context.Transactions.AnyAsync(t => t.PONumber == poNumber);
    }

    public string GetPONumberFormat()
    {
        // In a real implementation, this would be stored in settings/database
        // For now, return the configured format
        return _format;
    }

    public void SetPONumberFormat(string format)
    {
        // Validate format contains required placeholders
        if (string.IsNullOrWhiteSpace(format))
            throw new ArgumentException("Format cannot be empty");

        _format = format;
    }
}

// Add PONumberSettings model for configuration
public class PONumberSettings
{
    public string Format { get; set; } = "PO-{YYYY}-{####}";
    public bool AutoGenerate { get; set; } = true;
    public bool AllowManualEntry { get; set; } = true;
    public bool RequireUnique { get; set; } = true;
}

/*
 * Format Placeholders:
 * {YYYY} - Full year (2024)
 * {YY}   - Short year (24)
 * {MM}   - Month (01-12)
 * {####} - 4-digit sequential number
 * {###}  - 3-digit sequential number
 * 
 * Example formats:
 * PO-{YYYY}-{####}     -> PO-2024-0001
 * PO{YY}{MM}-{###}     -> PO2401-001
 * {YYYY}-{MM}-{####}   -> 2024-01-0001
 * PO-{####}            -> PO-0001 (simple)
 */
