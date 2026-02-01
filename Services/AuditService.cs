using Microsoft.EntityFrameworkCore;
using NonProfitFinance.Data;
using NonProfitFinance.Models;
using System.Text.Json;

namespace NonProfitFinance.Services;

public interface IAuditService
{
    /// <summary>
    /// Log an audit entry for an action
    /// </summary>
    Task LogAsync(string action, string entityType, int entityId, string? description = null,
        object? oldValues = null, object? newValues = null, string? additionalInfo = null);
    
    /// <summary>
    /// Get audit logs with filtering
    /// </summary>
    Task<List<AuditLogDto>> GetLogsAsync(AuditLogFilterRequest filter);
    
    /// <summary>
    /// Get audit history for a specific entity
    /// </summary>
    Task<List<AuditLogDto>> GetEntityHistoryAsync(string entityType, int entityId);
}

public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuditService> _logger;

    public AuditService(
        ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuditService> logger)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task LogAsync(string action, string entityType, int entityId, string? description = null,
        object? oldValues = null, object? newValues = null, string? additionalInfo = null)
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            
            var auditLog = new AuditLog
            {
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Description = description,
                OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
                NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null,
                UserId = httpContext?.User?.Identity?.Name,
                UserName = httpContext?.User?.Identity?.Name ?? "System",
                IpAddress = httpContext?.Connection?.RemoteIpAddress?.ToString(),
                Timestamp = DateTime.UtcNow,
                AdditionalInfo = additionalInfo
            };

            _context.Set<AuditLog>().Add(auditLog);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Don't fail the main operation if audit logging fails
            _logger.LogError(ex, "Failed to create audit log for {Action} on {EntityType} {EntityId}",
                action, entityType, entityId);
        }
    }

    public async Task<List<AuditLogDto>> GetLogsAsync(AuditLogFilterRequest filter)
    {
        var query = _context.Set<AuditLog>().AsQueryable();

        if (!string.IsNullOrEmpty(filter.EntityType))
            query = query.Where(a => a.EntityType == filter.EntityType);

        if (filter.EntityId.HasValue)
            query = query.Where(a => a.EntityId == filter.EntityId.Value);

        if (!string.IsNullOrEmpty(filter.Action))
            query = query.Where(a => a.Action == filter.Action);

        if (!string.IsNullOrEmpty(filter.UserId))
            query = query.Where(a => a.UserId == filter.UserId);

        if (filter.StartDate.HasValue)
            query = query.Where(a => a.Timestamp >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(a => a.Timestamp <= filter.EndDate.Value);

        var logs = await query
            .OrderByDescending(a => a.Timestamp)
            .Take(filter.MaxResults)
            .Select(a => new AuditLogDto(
                a.Id,
                a.Action,
                a.EntityType,
                a.EntityId,
                a.Description,
                a.UserName,
                a.Timestamp,
                a.IpAddress
            ))
            .ToListAsync();

        return logs;
    }

    public async Task<List<AuditLogDto>> GetEntityHistoryAsync(string entityType, int entityId)
    {
        return await _context.Set<AuditLog>()
            .Where(a => a.EntityType == entityType && a.EntityId == entityId)
            .OrderByDescending(a => a.Timestamp)
            .Select(a => new AuditLogDto(
                a.Id,
                a.Action,
                a.EntityType,
                a.EntityId,
                a.Description,
                a.UserName,
                a.Timestamp,
                a.IpAddress
            ))
            .ToListAsync();
    }
}

public record AuditLogDto(
    int Id,
    string Action,
    string EntityType,
    int EntityId,
    string? Description,
    string? UserName,
    DateTime Timestamp,
    string? IpAddress
);

public record AuditLogFilterRequest(
    string? EntityType = null,
    int? EntityId = null,
    string? Action = null,
    string? UserId = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    int MaxResults = 100
);
