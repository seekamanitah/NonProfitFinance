using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NonProfitFinance.Models.Enums;
using NonProfitFinance.Models.Shared;

namespace NonProfitFinance.Models.Inventory;

/// <summary>
/// Represents an inventory item
/// </summary>
public class InventoryItem
{
    [Key]
    public int Id { get; set; }
    
    /// <summary>
    /// Item name
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Item description
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    /// <summary>
    /// SKU or item number
    /// </summary>
    [MaxLength(100)]
    public string? SKU { get; set; }
    
    /// <summary>
    /// Barcode for scanning
    /// </summary>
    [MaxLength(100)]
    public string? Barcode { get; set; }
    
    /// <summary>
    /// Category ID
    /// </summary>
    public int? CategoryId { get; set; }
    
    /// <summary>
    /// Navigation property to category
    /// </summary>
    [ForeignKey(nameof(CategoryId))]
    public InventoryCategory? Category { get; set; }
    
    /// <summary>
    /// Current location ID
    /// </summary>
    public int? LocationId { get; set; }
    
    /// <summary>
    /// Navigation property to location
    /// </summary>
    [ForeignKey(nameof(LocationId))]
    public Location? Location { get; set; }
    
    /// <summary>
    /// Current quantity
    /// </summary>
    [Required]
    public decimal Quantity { get; set; }
    
    /// <summary>
    /// Unit of measure
    /// </summary>
    [Required]
    public UnitOfMeasure Unit { get; set; } = UnitOfMeasure.Each;
    
    /// <summary>
    /// Minimum quantity threshold for alerts
    /// </summary>
    public decimal? MinimumQuantity { get; set; }
    
    /// <summary>
    /// Maximum quantity (for storage limits)
    /// </summary>
    public decimal? MaximumQuantity { get; set; }
    
    /// <summary>
    /// Unit cost
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? UnitCost { get; set; }
    
    /// <summary>
    /// Current status
    /// </summary>
    [Required]
    public InventoryStatus Status { get; set; } = InventoryStatus.InStock;
    
    /// <summary>
    /// Item condition
    /// </summary>
    public ItemCondition? Condition { get; set; }
    
    /// <summary>
    /// Manufacturer
    /// </summary>
    [MaxLength(200)]
    public string? Manufacturer { get; set; }
    
    /// <summary>
    /// Model number
    /// </summary>
    [MaxLength(100)]
    public string? ModelNumber { get; set; }
    
    /// <summary>
    /// Serial number
    /// </summary>
    [MaxLength(100)]
    public string? SerialNumber { get; set; }
    
    /// <summary>
    /// Purchase date
    /// </summary>
    public DateTime? PurchaseDate { get; set; }
    
    /// <summary>
    /// Expiration date (for perishables)
    /// </summary>
    public DateTime? ExpirationDate { get; set; }
    
    /// <summary>
    /// Photo URL
    /// </summary>
    [MaxLength(500)]
    public string? PhotoUrl { get; set; }
    
    /// <summary>
    /// Notes
    /// </summary>
    [MaxLength(2000)]
    public string? Notes { get; set; }
    
    /// <summary>
    /// Is this item active?
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
    /// Created by user
    /// </summary>
    [MaxLength(100)]
    public string? CreatedBy { get; set; }
    
    /// <summary>
    /// Last updated date
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// Last updated by user
    /// </summary>
    [MaxLength(100)]
    public string? UpdatedBy { get; set; }
    
    /// <summary>
    /// Navigation: Transactions
    /// </summary>
    public ICollection<InventoryTransaction> Transactions { get; set; } = new List<InventoryTransaction>();
    
    /// <summary>
    /// Navigation: Custom field values
    /// </summary>
    public ICollection<CustomFieldValue> CustomFields { get; set; } = new List<CustomFieldValue>();
    
    /// <summary>
    /// Calculate total value
    /// </summary>
    [NotMapped]
    public decimal TotalValue => Quantity * (UnitCost ?? 0);
    
    /// <summary>
    /// Is quantity below minimum threshold?
    /// </summary>
    [NotMapped]
    public bool IsLowStock => MinimumQuantity.HasValue && Quantity <= MinimumQuantity.Value;
}
