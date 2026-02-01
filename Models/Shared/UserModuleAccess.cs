using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NonProfitFinance.Models.Shared;

/// <summary>
/// User access permissions per module
/// Allows secondary authentication per module
/// </summary>
public class UserModuleAccess
{
    [Key]
    public int Id { get; set; }
    
    /// <summary>
    /// User ID
    /// </summary>
    [Required]
    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// Navigation property to user
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public ApplicationUser? User { get; set; }
    
    /// <summary>
    /// Module name (e.g., "Financial", "Inventory", "Maintenance")
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string ModuleName { get; set; } = string.Empty;
    
    /// <summary>
    /// Has access to this module?
    /// </summary>
    public bool HasAccess { get; set; } = false;
    
    /// <summary>
    /// Access level within module
    /// </summary>
    public ModuleAccessLevel AccessLevel { get; set; } = ModuleAccessLevel.ReadOnly;
    
    /// <summary>
    /// Secondary PIN/password for module (hashed)
    /// </summary>
    [MaxLength(256)]
    public string? SecondaryPasswordHash { get; set; }
    
    /// <summary>
    /// Is secondary authentication required?
    /// </summary>
    public bool RequiresSecondaryAuth { get; set; } = false;
    
    /// <summary>
    /// Last access date
    /// </summary>
    public DateTime? LastAccessAt { get; set; }
    
    /// <summary>
    /// Access granted by
    /// </summary>
    [MaxLength(200)]
    public string? GrantedBy { get; set; }
    
    /// <summary>
    /// Access granted date
    /// </summary>
    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Access expires on (null = never)
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
    
    /// <summary>
    /// Is access currently active?
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Notes
    /// </summary>
    [MaxLength(500)]
    public string? Notes { get; set; }
    
    /// <summary>
    /// Is access expired?
    /// </summary>
    [NotMapped]
    public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow;
    
    /// <summary>
    /// Is access valid (active and not expired)?
    /// </summary>
    [NotMapped]
    public bool IsValid => IsActive && HasAccess && !IsExpired;
}

/// <summary>
/// Module access levels
/// </summary>
public enum ModuleAccessLevel
{
    /// <summary>
    /// Can view only
    /// </summary>
    ReadOnly = 0,
    
    /// <summary>
    /// Can view and edit
    /// </summary>
    ReadWrite = 1,
    
    /// <summary>
    /// Full access including delete
    /// </summary>
    FullAccess = 2,
    
    /// <summary>
    /// Administrative access
    /// </summary>
    Admin = 3
}

/// <summary>
/// Module names constants
/// </summary>
public static class ModuleNames
{
    public const string Financial = "Financial";
    public const string Inventory = "Inventory";
    public const string Maintenance = "Maintenance";
    public const string Admin = "Admin";
}
