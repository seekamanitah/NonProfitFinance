using Microsoft.EntityFrameworkCore;
using NonProfitFinance.Data;
using NonProfitFinance.Models;

namespace NonProfitFinance.Services;

/// <summary>
/// Service for safely resetting the database.
/// Creates emergency backup before deleting all data.
/// </summary>
public class DatabaseResetService : IDatabaseResetService
{
    private readonly ApplicationDbContext _context;
    private readonly IBackupService _backupService;
    private readonly IAuditService _auditService;
    private readonly ILogger<DatabaseResetService> _logger;

    public DatabaseResetService(
        ApplicationDbContext context,
        IBackupService backupService,
        IAuditService auditService,
        ILogger<DatabaseResetService> logger)
    {
        _context = context;
        _backupService = backupService;
        _auditService = auditService;
        _logger = logger;
    }

    /// <summary>
    /// Safely resets the database by:
    /// 1. Creating an emergency backup
    /// 2. Deleting all data through services (respecting relationships)
    /// 3. Logging the operation
    /// </summary>
    public async Task<DatabaseResetResult> ResetDatabaseAsync()
    {
        var result = new DatabaseResetResult();

        try
        {
            // Step 1: Create emergency backup
            _logger.LogWarning("Starting database reset - creating emergency backup");
            var backupResult = await _backupService.CreateBackupAsync();

            if (!backupResult.Success)
            {
                result.Success = false;
                result.Message = $"Failed to create backup: {backupResult.Error}. Database reset cancelled for safety.";
                _logger.LogError("Failed to create backup before reset: {Error}", backupResult.Error);
                return result;
            }

            result.BackupFilePath = backupResult.FilePath;
            _logger.LogInformation("Emergency backup created: {BackupPath}", backupResult.FilePath);

            // Step 2: Delete all data in reverse dependency order
            // We need to delete in order to respect foreign key constraints
            _logger.LogInformation("Deleting all application data");

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Delete in order of dependencies (children first, parents last)
                
                // Inventory module
                await _context.InventoryTransactions.ExecuteDeleteAsync();
                await _context.InventoryItems.ExecuteDeleteAsync();
                await _context.Locations.ExecuteDeleteAsync();
                await _context.InventoryCategories.ExecuteDeleteAsync();

                // Maintenance module
                await _context.ServiceRequests.ExecuteDeleteAsync();
                await _context.MaintenanceTasks.ExecuteDeleteAsync();
                await _context.WorkOrders.ExecuteDeleteAsync();
                await _context.Projects.ExecuteDeleteAsync();
                await _context.Contractors.ExecuteDeleteAsync();
                await _context.Buildings.ExecuteDeleteAsync();

                // Financial module - dependent data
                await _context.Documents.ExecuteDeleteAsync();
                await _context.TransactionSplits.ExecuteDeleteAsync();
                await _context.Transactions.ExecuteDeleteAsync();
                await _context.BudgetLineItems.ExecuteDeleteAsync();
                await _context.Budgets.ExecuteDeleteAsync();
                await _context.CategorizationRules.ExecuteDeleteAsync();

                // Financial module - core data
                await _context.Grants.ExecuteDeleteAsync();
                await _context.Funds.ExecuteDeleteAsync();
                await _context.Donors.ExecuteDeleteAsync();
                await _context.Categories.ExecuteDeleteAsync();

                // Shared module
                await _context.CustomFieldValues.ExecuteDeleteAsync();
                await _context.CustomFields.ExecuteDeleteAsync();

                // Note: UserModuleAccess is tied to Identity users, leave those alone

                // Save the changes
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("All application data deleted successfully");

                // Step 3: Log the reset operation
                await _auditService.LogAsync(
                    AuditAction.Delete,
                    "System",
                    0,
                    "CRITICAL: Database completely reset by user. Emergency backup created.",
                    oldValues: new { BackupFile = backupResult.FilePath, ResetTime = DateTime.UtcNow }
                );

                result.Success = true;
                result.Message = $"Database reset complete! Emergency backup saved to: {backupResult.FilePath}. All data has been permanently deleted.";
                _logger.LogWarning("Database reset completed successfully. Backup: {BackupPath}", backupResult.FilePath);

                return result;
            }
            catch (Exception innerEx)
            {
                await transaction.RollbackAsync();
                result.Success = false;
                result.Message = $"Error deleting data: {innerEx.Message}";
                _logger.LogError(innerEx, "Error during data deletion in reset operation");
                return result;
            }
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Message = $"Reset operation failed: {ex.Message}";
            _logger.LogError(ex, "Database reset operation failed");
            return result;
        }
    }
}
