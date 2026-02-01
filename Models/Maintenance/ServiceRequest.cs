using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NonProfitFinance.Models.Enums;

namespace NonProfitFinance.Models.Maintenance;

/// <summary>
/// Service or repair request submitted by users
/// </summary>
public class ServiceRequest
{
    [Key]
    public int Id { get; set; }
    
    /// <summary>
    /// Request number (auto-generated)
    /// </summary>
    [MaxLength(50)]
    public string? RequestNumber { get; set; }
    
    /// <summary>
    /// Request title
    /// </summary>
    [Required]
    [MaxLength(300)]
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Detailed description of the issue
    /// </summary>
    [Required]
    [MaxLength(4000)]
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Type of request
    /// </summary>
    [Required]
    public ServiceRequestType Type { get; set; } = ServiceRequestType.Repair;
    
    /// <summary>
    /// Current status
    /// </summary>
    [Required]
    public ServiceRequestStatus Status { get; set; } = ServiceRequestStatus.Submitted;
    
    /// <summary>
    /// Priority/Urgency level
    /// </summary>
    [Required]
    public Priority Priority { get; set; } = Priority.Medium;
    
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
    /// Specific area/room within building
    /// </summary>
    [MaxLength(200)]
    public string? Area { get; set; }
    
    /// <summary>
    /// Equipment or asset involved
    /// </summary>
    [MaxLength(200)]
    public string? Equipment { get; set; }
    
    /// <summary>
    /// Submitted by user
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string SubmittedBy { get; set; } = string.Empty;
    
    /// <summary>
    /// Contact phone for requester
    /// </summary>
    [MaxLength(20)]
    public string? ContactPhone { get; set; }
    
    /// <summary>
    /// Contact email for requester
    /// </summary>
    [MaxLength(200)]
    public string? ContactEmail { get; set; }
    
    /// <summary>
    /// Submitted date
    /// </summary>
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Desired completion date
    /// </summary>
    public DateTime? RequestedCompletionDate { get; set; }
    
    /// <summary>
    /// Reviewed by
    /// </summary>
    [MaxLength(200)]
    public string? ReviewedBy { get; set; }
    
    /// <summary>
    /// Reviewed date
    /// </summary>
    public DateTime? ReviewedAt { get; set; }
    
    /// <summary>
    /// Assigned to (user or team)
    /// </summary>
    [MaxLength(200)]
    public string? AssignedTo { get; set; }
    
    /// <summary>
    /// Assigned date
    /// </summary>
    public DateTime? AssignedAt { get; set; }
    
    /// <summary>
    /// Linked project ID (if converted to project)
    /// </summary>
    public int? ProjectId { get; set; }
    
    /// <summary>
    /// Navigation property to project
    /// </summary>
    [ForeignKey(nameof(ProjectId))]
    public Project? Project { get; set; }
    
    /// <summary>
    /// Linked work order ID
    /// </summary>
    public int? WorkOrderId { get; set; }
    
    /// <summary>
    /// Navigation property to work order
    /// </summary>
    [ForeignKey(nameof(WorkOrderId))]
    public WorkOrder? WorkOrder { get; set; }
    
    /// <summary>
    /// Completed date
    /// </summary>
    public DateTime? CompletedAt { get; set; }
    
    /// <summary>
    /// Completed by
    /// </summary>
    [MaxLength(200)]
    public string? CompletedBy { get; set; }
    
    /// <summary>
    /// Resolution notes
    /// </summary>
    [MaxLength(2000)]
    public string? Resolution { get; set; }
    
    /// <summary>
    /// Photo/Attachment URL
    /// </summary>
    [MaxLength(500)]
    public string? PhotoUrl { get; set; }
    
    /// <summary>
    /// Internal notes (not visible to requester)
    /// </summary>
    [MaxLength(2000)]
    public string? InternalNotes { get; set; }
    
    /// <summary>
    /// Is this demo data?
    /// </summary>
    public bool IsDemo { get; set; } = false;
    
    /// <summary>
    /// Is urgent (priority is High, Critical, or Emergency)?
    /// </summary>
    [NotMapped]
    public bool IsUrgent => Priority >= Priority.High;
}
