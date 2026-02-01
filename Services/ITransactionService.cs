using NonProfitFinance.DTOs;

namespace NonProfitFinance.Services;

public interface ITransactionService
{
    Task<PagedResult<TransactionDto>> GetAllAsync(TransactionFilterRequest filter);
    Task<TransactionDto?> GetByIdAsync(int id);
    Task<TransactionDto> CreateAsync(CreateTransactionRequest request);
    Task<TransactionDto?> UpdateAsync(int id, UpdateTransactionRequest request);
    Task<bool> DeleteAsync(int id);
    Task<List<TransactionDto>> GetRecentAsync(int count = 10);
    Task<List<TransactionDto>> GetByDonorAsync(int donorId);
    Task<List<TransactionDto>> GetByGrantAsync(int grantId);
    Task ProcessRecurringTransactionsAsync();
    
    /// <summary>
    /// Get distinct payees for auto-complete suggestions
    /// </summary>
    Task<List<PayeeSuggestion>> GetPayeeSuggestionsAsync(string searchTerm, int maxResults = 10);
    
    /// <summary>
    /// Get all distinct tags used in transactions
    /// </summary>
    Task<List<string>> GetDistinctTagsAsync();
    
    /// <summary>
    /// Check for potential duplicate transactions
    /// </summary>
    Task<List<DuplicateTransactionWarning>> CheckForDuplicatesAsync(DateTime date, decimal amount, string? payee);
    
    /// <summary>
    /// Restore a soft-deleted transaction
    /// </summary>
    Task<bool> RestoreAsync(int id);
    
    /// <summary>
    /// Get soft-deleted transactions for recovery
    /// </summary>
    Task<List<TransactionDto>> GetDeletedAsync(int maxCount = 50);
    
    /// <summary>
    /// Permanently delete a transaction (cannot be undone)
    /// </summary>
    Task<bool> PermanentDeleteAsync(int id);
}

public record PayeeSuggestion(
    string Payee,
    int UsageCount,
    int? LastCategoryId,
    string? LastCategoryName
);

public record DuplicateTransactionWarning(
    int ExistingTransactionId,
    DateTime Date,
    decimal Amount,
    string? Payee,
    string? Description
);
