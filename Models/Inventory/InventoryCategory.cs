using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NonProfitFinance.Models.Inventory;

/// <summary>
/// Hierarchical category for inventory items
/// </summary>
public class InventoryCategory
{
    [Key]
    public int Id { get; set; }
    
    /// <summary>
    /// Category name
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Description
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Parent category ID (for hierarchy)
    /// </summary>
    public int? ParentCategoryId { get; set; }
    
    /// <summary>
    /// Navigation property to parent category
    /// </summary>
    [ForeignKey(nameof(ParentCategoryId))]
    public InventoryCategory? ParentCategory { get; set; }
    
    /// <summary>
    /// Display order
    /// </summary>
    public int DisplayOrder { get; set; }
    
    /// <summary>
    /// Color code for UI
    /// </summary>
    [MaxLength(7)]
    public string? Color { get; set; }
    
    /// <summary>
    /// Icon class (e.g., Font Awesome)
    /// </summary>
    [MaxLength(100)]
    public string? Icon { get; set; }
    
    /// <summary>
    /// Is this category active?
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Is this demo data?
    /// </summary>
    public bool IsDemo { get; set; } = false;
    
    /// <summary>
    /// Created date
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Navigation: Child categories
    /// </summary>
    public ICollection<InventoryCategory> SubCategories { get; set; } = new List<InventoryCategory>();
    
    /// <summary>
    /// Navigation: Items in this category
    /// </summary>
    public ICollection<InventoryItem> Items { get; set; } = new List<InventoryItem>();
    
    /// <summary>
    /// Get full category path (e.g., "Equipment > Tools > Hand Tools")
    /// </summary>
    [NotMapped]
    public string FullPath
    {
        get
        {
            var path = Name;
            var parent = ParentCategory;
            while (parent != null)
            {
                path = $"{parent.Name} > {path}";
                parent = parent.ParentCategory;
            }
            return path;
        }
    }
}
