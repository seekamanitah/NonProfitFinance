namespace NonProfitFinance.Models;

/// <summary>
/// Interface for entities that support soft delete.
/// Entities implementing this interface will be filtered out by default
/// and can be restored if needed.
/// </summary>
public interface ISoftDelete
{
    /// <summary>
    /// Indicates whether the entity has been soft-deleted.
    /// </summary>
    bool IsDeleted { get; set; }
    
    /// <summary>
    /// The date and time when the entity was deleted.
    /// </summary>
    DateTime? DeletedAt { get; set; }
    
    /// <summary>
    /// The user who deleted the entity (for audit trail).
    /// </summary>
    string? DeletedBy { get; set; }
}

/// <summary>
/// Extension methods for soft delete operations.
/// </summary>
public static class SoftDeleteExtensions
{
    /// <summary>
    /// Marks an entity as deleted without removing it from the database.
    /// </summary>
    public static void SoftDelete(this ISoftDelete entity, string? deletedBy = null)
    {
        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        entity.DeletedBy = deletedBy;
    }
    
    /// <summary>
    /// Restores a soft-deleted entity.
    /// </summary>
    public static void Restore(this ISoftDelete entity)
    {
        entity.IsDeleted = false;
        entity.DeletedAt = null;
        entity.DeletedBy = null;
    }
}
