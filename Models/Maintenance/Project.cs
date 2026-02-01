using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NonProfitFinance.Models.Enums;

namespace NonProfitFinance.Models.Maintenance;

/// <summary>
/// Maintenance or improvement project
/// </summary>
public class Project
{
    [Key]
    public int Id { get; set; }
    
    /// <summary>
    /// Project name/title
    /// </summary>
    [Required]
    [MaxLength(300)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Project description
    /// </summary>
    [MaxLength(2000)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Project type
    /// </summary>
    [Required]
    public ProjectType Type { get; set; } = ProjectType.Maintenance;
    
    /// <summary>
    /// Current status
    /// </summary>
    [Required]
    public ProjectStatus Status { get; set; } = ProjectStatus.Planned;
    
    /// <summary>
    /// Priority level
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
    /// Specific area within building
    /// </summary>
    [MaxLength(200)]
    public string? Area { get; set; }
    
    /// <summary>
    /// Start date (planned)
    /// </summary>
    public DateTime? PlannedStartDate { get; set; }
    
    /// <summary>
    /// End date (planned)
    /// </summary>
    public DateTime? PlannedEndDate { get; set; }
    
    /// <summary>
    /// Actual start date
    /// </summary>
    public DateTime? ActualStartDate { get; set; }
    
    /// <summary>
    /// Actual end date
    /// </summary>
    public DateTime? ActualEndDate { get; set; }
    
    /// <summary>
    /// Due date
    /// </summary>
    public DateTime? DueDate { get; set; }
    
    /// <summary>
    /// Assigned to (user or team name)
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
    /// Estimated cost
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal CostEstimate { get; set; }
    
    /// <summary>
    /// Actual cost spent
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal ActualCost { get; set; }
    
    /// <summary>
    /// Budget amount allocated
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? BudgetAmount { get; set; }
    
    /// <summary>
    /// Linked Fund ID (for fund accounting)
    /// </summary>
    public int? FundId { get; set; }
    
    /// <summary>
    /// Navigation property to fund
    /// </summary>
    [ForeignKey(nameof(FundId))]
    public Fund? Fund { get; set; }
    
    /// <summary>
    /// Linked Grant ID (if grant-funded)
    /// </summary>
    public int? GrantId { get; set; }
    
    /// <summary>
    /// Navigation property to grant
    /// </summary>
    [ForeignKey(nameof(GrantId))]
    public Grant? Grant { get; set; }
    
    /// <summary>
    /// Completion percentage (0-100)
    /// </summary>
    public int CompletionPercentage { get; set; }
    
    /// <summary>
    /// Notes
    /// </summary>
    [MaxLength(4000)]
    public string? Notes { get; set; }
    
    /// <summary>
    /// Is this project active?
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
    /// Navigation: Tasks
    /// </summary>
    public ICollection<MaintenanceTask> Tasks { get; set; } = new List<MaintenanceTask>();
    
    /// <summary>
    /// Navigation: Work orders
    /// </summary>
    public ICollection<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();
    
    /// <summary>
    /// Navigation: Linked expenses (Financial transactions)
    /// </summary>
    public ICollection<Transaction> LinkedExpenses { get; set; } = new List<Transaction>();
    
    /// <summary>
    /// Is project overdue?
    /// </summary>
    [NotMapped]
    public bool IsOverdue => DueDate.HasValue && 
                             DueDate.Value < DateTime.Today && 
                             Status != ProjectStatus.Completed && 
                             Status != ProjectStatus.Cancelled;
    
    /// <summary>
    /// Cost variance (Actual - Estimate)
    /// </summary>
    [NotMapped]
    public decimal CostVariance => ActualCost - CostEstimate;
    
    /// <summary>
    /// Is over budget?
    /// </summary>
    [NotMapped]
    public bool IsOverBudget => ActualCost > CostEstimate;
}
