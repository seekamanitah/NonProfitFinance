using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NonProfitFinance.Models;

/// <summary>
/// Audit log entry for tracking all CRUD operations on financial data.
/// Provides complete audit trail for nonprofit compliance requirements.
/// </summary>
public class AuditLog
{
    [Key]
    public int Id { get; set; }
    
    /// <summary>
    /// Type of action performed (Create, Update, Delete, Restore, etc.)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Action { get; set; } = string.Empty;
    
    /// <summary>
    /// Name of the entity type affected (Transaction, Fund, Grant, etc.)
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string EntityType { get; set; } = string.Empty;
    
    /// <summary>
    /// Primary key of the affected entity
    /// </summary>
    public int EntityId { get; set; }
    
    /// <summary>
    /// Description of what changed
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }
    
    /// <summary>
    /// JSON snapshot of old values (for updates/deletes)
    /// </summary>
    public string? OldValues { get; set; }
    
    /// <summary>
    /// JSON snapshot of new values (for creates/updates)
    /// </summary>
    public string? NewValues { get; set; }
    
    /// <summary>
    /// User who performed the action
    /// </summary>
    [MaxLength(256)]
    public string? UserId { get; set; }
    
    /// <summary>
    /// Username for display
    /// </summary>
    [MaxLength(256)]
    public string? UserName { get; set; }
    
    /// <summary>
    /// IP address of the request
    /// </summary>
    [MaxLength(45)]
    public string? IpAddress { get; set; }
    
    /// <summary>
    /// Timestamp of the action (UTC)
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Additional context or notes
    /// </summary>
    [MaxLength(1000)]
    public string? AdditionalInfo { get; set; }
}

/// <summary>
/// Common audit action types
/// </summary>
public static class AuditAction
{
    public const string Create = "Create";
    public const string Update = "Update";
    public const string Delete = "Delete";
    public const string Restore = "Restore";
    public const string PermanentDelete = "PermanentDelete";
    public const string Archive = "Archive";
    public const string Login = "Login";
    public const string Logout = "Logout";
    public const string Export = "Export";
    public const string Import = "Import";
    public const string Backup = "Backup";
    public const string Transfer = "Transfer";
}
