using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NonProfitFinance.Models.Enums;

namespace NonProfitFinance.Models.Maintenance;

/// <summary>
/// Individual task within a maintenance project
/// </summary>
public class MaintenanceTask
{
    [Key]
    public int Id { get; set; }
    
    /// <summary>
    /// Parent project ID
    /// </summary>
    [Required]
    public int ProjectId { get; set; }
    
    /// <summary>
    /// Navigation property to project
    /// </summary>
    [ForeignKey(nameof(ProjectId))]
    public Project Project { get; set; } = null!;
    
    /// <summary>
    /// Task name/title
    /// </summary>
    [Required]
    [MaxLength(300)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Task description
    /// </summary>
    [MaxLength(2000)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Task sequence/order
    /// </summary>
    public int Sequence { get; set; }
    
    /// <summary>
    /// Current status
    /// </summary>
    [Required]
    public ProjectStatus Status { get; set; } = ProjectStatus.Planned;
    
    /// <summary>
    /// Priority level
    /// </summary>
    public Priority Priority { get; set; } = Priority.Medium;
    
    /// <summary>
    /// Assigned to (user name)
    /// </summary>
    [MaxLength(200)]
    public string? AssignedTo { get; set; }
    
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
    /// Actual cost
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? ActualCost { get; set; }
    
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
    /// Is this a milestone task?
    /// </summary>
    public bool IsMilestone { get; set; } = false;
    
    /// <summary>
    /// Notes
    /// </summary>
    [MaxLength(2000)]
    public string? Notes { get; set; }
    
    /// <summary>
    /// Is task completed?
    /// </summary>
    public bool IsCompleted { get; set; } = false;
    
    /// <summary>
    /// Is this demo data?
    /// </summary>
    public bool IsDemo { get; set; } = false;
    
    /// <summary>
    /// Created date
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Is task overdue?
    /// </summary>
    [NotMapped]
    public bool IsOverdue => DueDate.HasValue && 
                             DueDate.Value < DateTime.Today && 
                             !IsCompleted;
}
