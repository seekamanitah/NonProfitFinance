using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NonProfitFinance.Models.Inventory;

/// <summary>
/// Storage location for inventory items (hierarchical)
/// </summary>
public class Location
{
    [Key]
    public int Id { get; set; }
    
    /// <summary>
    /// Location name (e.g., "Station 1", "Truck 2", "Storage Room")
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
    /// Location code/number
    /// </summary>
    [MaxLength(50)]
    public string? Code { get; set; }
    
    /// <summary>
    /// Parent location ID (for hierarchy)
    /// </summary>
    public int? ParentLocationId { get; set; }
    
    /// <summary>
    /// Navigation property to parent location
    /// </summary>
    [ForeignKey(nameof(ParentLocationId))]
    public Location? ParentLocation { get; set; }
    
    /// <summary>
    /// Display order
    /// </summary>
    public int DisplayOrder { get; set; }
    
    /// <summary>
    /// Physical address (if applicable)
    /// </summary>
    [MaxLength(500)]
    public string? Address { get; set; }
    
    /// <summary>
    /// Contact person for this location
    /// </summary>
    [MaxLength(200)]
    public string? ContactPerson { get; set; }
    
    /// <summary>
    /// Contact phone
    /// </summary>
    [MaxLength(20)]
    public string? ContactPhone { get; set; }
    
    /// <summary>
    /// Is this location active?
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
    /// Navigation: Child locations
    /// </summary>
    public ICollection<Location> SubLocations { get; set; } = new List<Location>();
    
    /// <summary>
    /// Navigation: Items at this location
    /// </summary>
    public ICollection<InventoryItem> Items { get; set; } = new List<InventoryItem>();
    
    /// <summary>
    /// Get full location path (e.g., "Station 1 > Truck 2 > Equipment Bay")
    /// </summary>
    [NotMapped]
    public string FullPath
    {
        get
        {
            var path = Name;
            var parent = ParentLocation;
            while (parent != null)
            {
                path = $"{parent.Name} > {path}";
                parent = parent.ParentLocation;
            }
            return path;
        }
    }
}
