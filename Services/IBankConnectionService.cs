namespace NonProfitFinance.Services;

public interface IBankConnectionService
{
    /// <summary>
    /// Get all connected bank accounts
    /// </summary>
    Task<List<BankAccountDto>> GetConnectedAccountsAsync();
    
    /// <summary>
    /// Create a link token for Plaid integration
    /// </summary>
    Task<string> CreateLinkTokenAsync(string userId);
    
    /// <summary>
    /// Exchange public token for access token after user completes Plaid Link
    /// </summary>
    Task<BankConnectionResult> ExchangePublicTokenAsync(string publicToken);
    
    /// <summary>
    /// Sync transactions from connected bank accounts
    /// </summary>
    Task<SyncResult> SyncTransactionsAsync(int accountId);
    
    /// <summary>
    /// Sync all connected accounts
    /// </summary>
    Task<SyncResult> SyncAllAccountsAsync();
    
    /// <summary>
    /// Disconnect a bank account
    /// </summary>
    Task DisconnectAccountAsync(int accountId);
    
    /// <summary>
    /// Get pending transactions for review/categorization
    /// </summary>
    Task<List<PendingBankTransactionDto>> GetPendingTransactionsAsync();
    
    /// <summary>
    /// Approve and import a pending transaction
    /// </summary>
    Task ApproveTransactionAsync(int pendingId, int categoryId, int? fundId = null);
    
    /// <summary>
    /// Dismiss a pending transaction
    /// </summary>
    Task DismissTransactionAsync(int pendingId);
}

public record BankAccountDto(
    int Id,
    string InstitutionName,
    string AccountName,
    string AccountMask,
    string AccountType,
    decimal? CurrentBalance,
    DateTime? LastSynced,
    bool IsActive
);

public record BankConnectionResult(
    bool Success,
    string? Error,
    List<BankAccountDto> Accounts
);

public record SyncResult(
    bool Success,
    int NewTransactions,
    int UpdatedTransactions,
    string? Error
);

public record PendingBankTransactionDto(
    int Id,
    int AccountId,
    string AccountName,
    DateTime Date,
    decimal Amount,
    string? Description,
    string? MerchantName,
    string? SuggestedCategory,
    int? SuggestedCategoryId,
    bool IsIncome
);
