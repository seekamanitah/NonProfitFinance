using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NonProfitFinance.Models.Shared;

/// <summary>
/// Custom field value (EAV pattern)
/// Stores actual values for custom fields
/// </summary>
public class CustomFieldValue
{
    [Key]
    public int Id { get; set; }
    
    /// <summary>
    /// Custom field ID
    /// </summary>
    [Required]
    public int CustomFieldId { get; set; }
    
    /// <summary>
    /// Navigation property to custom field
    /// </summary>
    [ForeignKey(nameof(CustomFieldId))]
    public CustomField CustomField { get; set; } = null!;
    
    /// <summary>
    /// Entity type (matches CustomField.EntityType)
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string EntityType { get; set; } = string.Empty;
    
    /// <summary>
    /// Entity ID (the ID of the record this value belongs to)
    /// </summary>
    [Required]
    public int EntityId { get; set; }
    
    /// <summary>
    /// String value (for text, dropdown, etc.)
    /// </summary>
    [MaxLength(4000)]
    public string? StringValue { get; set; }
    
    /// <summary>
    /// Numeric value (for number, decimal, currency)
    /// </summary>
    [Column(TypeName = "decimal(18,6)")]
    public decimal? NumericValue { get; set; }
    
    /// <summary>
    /// Date value
    /// </summary>
    public DateTime? DateValue { get; set; }
    
    /// <summary>
    /// Boolean value
    /// </summary>
    public bool? BooleanValue { get; set; }
    
    /// <summary>
    /// Created date
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Last updated date
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// Get the value as object based on field type
    /// </summary>
    [NotMapped]
    public object? Value
    {
        get
        {
            if (CustomField == null) return StringValue;
            
            return CustomField.FieldType switch
            {
                CustomFieldType.Checkbox => BooleanValue,
                CustomFieldType.Number or 
                CustomFieldType.Decimal or 
                CustomFieldType.Currency => NumericValue,
                CustomFieldType.Date or CustomFieldType.DateTime => DateValue,
                _ => StringValue
            };
        }
    }
    
    /// <summary>
    /// Get value as display string
    /// </summary>
    [NotMapped]
    public string DisplayValue
    {
        get
        {
            if (CustomField == null) return StringValue ?? string.Empty;
            
            return CustomField.FieldType switch
            {
                CustomFieldType.Checkbox => BooleanValue == true ? "Yes" : "No",
                CustomFieldType.Currency => NumericValue?.ToString("C") ?? string.Empty,
                CustomFieldType.Number => NumericValue?.ToString("N0") ?? string.Empty,
                CustomFieldType.Decimal => NumericValue?.ToString("N2") ?? string.Empty,
                CustomFieldType.Date => DateValue?.ToString("d") ?? string.Empty,
                CustomFieldType.DateTime => DateValue?.ToString("g") ?? string.Empty,
                _ => StringValue ?? string.Empty
            };
        }
    }
}
