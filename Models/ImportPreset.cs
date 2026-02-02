namespace NonProfitFinance.Models;

/// <summary>
/// Stores saved column mapping configurations for CSV imports.
/// Allows users to save presets for different import sources (e.g., QuickBooks, bank exports).
/// </summary>
public class ImportPreset
{
    public int Id { get; set; }

    /// <summary>
    /// User-friendly name for the preset (e.g., "QuickBooks Export", "Bank of America CSV").
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Optional description of what this preset is for.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Type of import this preset is for (e.g., "transactions", "donors").
    /// </summary>
    public string ImportType { get; set; } = "transactions";

    // Column mappings (0-based index, null if not mapped)
    public int DateColumn { get; set; }
    public int AmountColumn { get; set; }
    public int DescriptionColumn { get; set; }
    public int? TypeColumn { get; set; }
    public int? CategoryColumn { get; set; }
    public int? FundColumn { get; set; }
    public int? DonorColumn { get; set; }
    public int? GrantColumn { get; set; }
    public int? PayeeColumn { get; set; }
    public int? TagsColumn { get; set; }

    /// <summary>
    /// Whether the CSV file has a header row.
    /// </summary>
    public bool HasHeaderRow { get; set; } = true;

    /// <summary>
    /// Date format used in the CSV (e.g., "yyyy-MM-dd", "MM/dd/yyyy").
    /// </summary>
    public string DateFormat { get; set; } = "yyyy-MM-dd";

    /// <summary>
    /// Whether this is the default preset.
    /// </summary>
    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// When the preset was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the preset was last used.
    /// </summary>
    public DateTime? LastUsedAt { get; set; }
}
