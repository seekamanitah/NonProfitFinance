using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NonProfitFinance.Models.Shared;

/// <summary>
/// Custom field definition (EAV pattern)
/// Allows adding custom fields to any entity type
/// </summary>
public class CustomField
{
    [Key]
    public int Id { get; set; }
    
    /// <summary>
    /// Entity type this field applies to (e.g., "InventoryItem", "Project")
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string EntityType { get; set; } = string.Empty;
    
    /// <summary>
    /// Field name
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Display label
    /// </summary>
    [MaxLength(200)]
    public string? Label { get; set; }
    
    /// <summary>
    /// Field description/help text
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Field data type
    /// </summary>
    [Required]
    public CustomFieldType FieldType { get; set; } = CustomFieldType.Text;
    
    /// <summary>
    /// Default value
    /// </summary>
    [MaxLength(1000)]
    public string? DefaultValue { get; set; }
    
    /// <summary>
    /// Options for Dropdown/MultiSelect (JSON array)
    /// </summary>
    [MaxLength(4000)]
    public string? Options { get; set; }
    
    /// <summary>
    /// Is this field required?
    /// </summary>
    public bool IsRequired { get; set; } = false;
    
    /// <summary>
    /// Display order
    /// </summary>
    public int DisplayOrder { get; set; }
    
    /// <summary>
    /// Show in list/table views?
    /// </summary>
    public bool ShowInList { get; set; } = false;
    
    /// <summary>
    /// Is searchable?
    /// </summary>
    public bool IsSearchable { get; set; } = false;
    
    /// <summary>
    /// Is this field active?
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Created date
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Navigation: Values for this field
    /// </summary>
    public ICollection<CustomFieldValue> Values { get; set; } = new List<CustomFieldValue>();
    
    /// <summary>
    /// Get display label (or name if no label)
    /// </summary>
    [NotMapped]
    public string DisplayLabel => Label ?? Name;
}

/// <summary>
/// Types of custom fields
/// </summary>
public enum CustomFieldType
{
    Text,
    TextArea,
    Number,
    Decimal,
    Currency,
    Date,
    DateTime,
    Checkbox,
    Dropdown,
    MultiSelect,
    Email,
    Phone,
    Url
}
