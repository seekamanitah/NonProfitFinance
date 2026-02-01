using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NonProfitFinance.Models.Enums;

namespace NonProfitFinance.Models.Inventory;

/// <summary>
/// Tracks inventory movements and changes
/// </summary>
public class InventoryTransaction
{
    [Key]
    public int Id { get; set; }
    
    /// <summary>
    /// Item ID
    /// </summary>
    [Required]
    public int ItemId { get; set; }
    
    /// <summary>
    /// Navigation property to item
    /// </summary>
    [ForeignKey(nameof(ItemId))]
    public InventoryItem Item { get; set; } = null!;
    
    /// <summary>
    /// Transaction type
    /// </summary>
    [Required]
    public InventoryTransactionType Type { get; set; }
    
    /// <summary>
    /// Transaction date/time
    /// </summary>
    [Required]
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Quantity change (positive for additions, negative for removals)
    /// </summary>
    [Required]
    public decimal Quantity { get; set; }
    
    /// <summary>
    /// Unit of measure
    /// </summary>
    [Required]
    public UnitOfMeasure Unit { get; set; }
    
    /// <summary>
    /// From location ID (for transfers)
    /// </summary>
    public int? FromLocationId { get; set; }
    
    /// <summary>
    /// Navigation property to from location
    /// </summary>
    [ForeignKey(nameof(FromLocationId))]
    public Location? FromLocation { get; set; }
    
    /// <summary>
    /// To location ID (for transfers)
    /// </summary>
    public int? ToLocationId { get; set; }
    
    /// <summary>
    /// Navigation property to to location
    /// </summary>
    [ForeignKey(nameof(ToLocationId))]
    public Location? ToLocation { get; set; }
    
    /// <summary>
    /// Unit cost at time of transaction
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? UnitCost { get; set; }
    
    /// <summary>
    /// Total cost (Quantity * UnitCost)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? TotalCost { get; set; }
    
    /// <summary>
    /// Linked Financial transaction ID (for purchases)
    /// </summary>
    public int? LinkedTransactionId { get; set; }
    
    /// <summary>
    /// Navigation property to Financial transaction
    /// </summary>
    [ForeignKey(nameof(LinkedTransactionId))]
    public Transaction? LinkedTransaction { get; set; }
    
    /// <summary>
    /// Reference number (PO, invoice, etc.)
    /// </summary>
    [MaxLength(100)]
    public string? ReferenceNumber { get; set; }
    
    /// <summary>
    /// Reason for transaction
    /// </summary>
    [MaxLength(500)]
    public string? Reason { get; set; }
    
    /// <summary>
    /// Notes
    /// </summary>
    [MaxLength(2000)]
    public string? Notes { get; set; }
    
    /// <summary>
    /// Performed by user
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string PerformedBy { get; set; } = string.Empty;
    
    /// <summary>
    /// Is this demo data?
    /// </summary>
    public bool IsDemo { get; set; } = false;
}
