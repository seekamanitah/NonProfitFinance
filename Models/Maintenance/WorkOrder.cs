using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NonProfitFinance.Models.Enums;

namespace NonProfitFinance.Models.Maintenance;

/// <summary>
/// Work order for maintenance/repair work
/// </summary>
public class WorkOrder
{
    [Key]
    public int Id { get; set; }
    
    /// <summary>
    /// Work order number (auto-generated)
    /// </summary>
    [MaxLength(50)]
    public string? WorkOrderNumber { get; set; }
    
    /// <summary>
    /// Work order title
    /// </summary>
    [Required]
    [MaxLength(300)]
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Work description
    /// </summary>
    [Required]
    [MaxLength(4000)]
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Current status
    /// </summary>
    [Required]
    public WorkOrderStatus Status { get; set; } = WorkOrderStatus.Created;
    
    /// <summary>
    /// Priority level
    /// </summary>
    [Required]
    public Priority Priority { get; set; } = Priority.Medium;
    
    /// <summary>
    /// Parent project ID (if part of project)
    /// </summary>
    public int? ProjectId { get; set; }
    
    /// <summary>
    /// Navigation property to project
    /// </summary>
    [ForeignKey(nameof(ProjectId))]
    public Project? Project { get; set; }
    
    /// <summary>
    /// Building/Location ID
    /// </summary>
    public int? BuildingId { get; set; }
    
    /// <summary>
    /// Navigation property to building
    /// </summary>
    [ForeignKey(nameof(BuildingId))]
    public Building? Building { get; set; }
    
    /// <summary>
    /// Specific area within building
    /// </summary>
    [MaxLength(200)]
    public string? Area { get; set; }
    
    /// <summary>
    /// Assigned to (staff member)
    /// </summary>
    [MaxLength(200)]
    public string? AssignedTo { get; set; }
    
    /// <summary>
    /// Contractor ID (if external)
    /// </summary>
    public int? ContractorId { get; set; }
    
    /// <summary>
    /// Navigation property to contractor
    /// </summary>
    [ForeignKey(nameof(ContractorId))]
    public Contractor? Contractor { get; set; }
    
    /// <summary>
    /// Scheduled start date
    /// </summary>
    public DateTime? ScheduledDate { get; set; }
    
    /// <summary>
    /// Due date
    /// </summary>
    public DateTime? DueDate { get; set; }
    
    /// <summary>
    /// Actual start date
    /// </summary>
    public DateTime? StartedAt { get; set; }
    
    /// <summary>
    /// Completion date
    /// </summary>
    public DateTime? CompletedAt { get; set; }
    
    /// <summary>
    /// Verified by (supervisor)
    /// </summary>
    [MaxLength(200)]
    public string? VerifiedBy { get; set; }
    
    /// <summary>
    /// Verified date
    /// </summary>
    public DateTime? VerifiedAt { get; set; }
    
    /// <summary>
    /// Estimated hours
    /// </summary>
    public decimal? EstimatedHours { get; set; }
    
    /// <summary>
    /// Actual hours spent
    /// </summary>
    public decimal? ActualHours { get; set; }
    
    /// <summary>
    /// Estimated cost
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? EstimatedCost { get; set; }
    
    /// <summary>
    /// Labor cost
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? LaborCost { get; set; }
    
    /// <summary>
    /// Materials cost
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? MaterialsCost { get; set; }
    
    /// <summary>
    /// Other costs
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? OtherCost { get; set; }
    
    /// <summary>
    /// Materials/Parts used
    /// </summary>
    [MaxLength(2000)]
    public string? MaterialsUsed { get; set; }
    
    /// <summary>
    /// Completion notes
    /// </summary>
    [MaxLength(2000)]
    public string? CompletionNotes { get; set; }
    
    /// <summary>
    /// Before photo URL
    /// </summary>
    [MaxLength(500)]
    public string? BeforePhotoUrl { get; set; }
    
    /// <summary>
    /// After photo URL
    /// </summary>
    [MaxLength(500)]
    public string? AfterPhotoUrl { get; set; }
    
    /// <summary>
    /// Is recurring work?
    /// </summary>
    public bool IsRecurring { get; set; } = false;
    
    /// <summary>
    /// Recurrence frequency
    /// </summary>
    public MaintenanceFrequency? RecurrenceFrequency { get; set; }
    
    /// <summary>
    /// Is this demo data?
    /// </summary>
    public bool IsDemo { get; set; } = false;
    
    /// <summary>
    /// Created date
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Created by
    /// </summary>
    [MaxLength(100)]
    public string? CreatedBy { get; set; }
    
    /// <summary>
    /// Total cost (Labor + Materials + Other)
    /// </summary>
    [NotMapped]
    public decimal TotalCost => (LaborCost ?? 0) + (MaterialsCost ?? 0) + (OtherCost ?? 0);
    
    /// <summary>
    /// Is overdue?
    /// </summary>
    [NotMapped]
    public bool IsOverdue => DueDate.HasValue && 
                             DueDate.Value < DateTime.Today && 
                             Status != WorkOrderStatus.Completed &&
                             Status != WorkOrderStatus.Verified &&
                             Status != WorkOrderStatus.Closed;
}
