namespace NonProfitFinance.Services;

/// <summary>
/// Placeholder implementation for bank connection service.
/// Replace with actual Plaid integration when ready.
/// </summary>
public class BankConnectionService : IBankConnectionService
{
    // In a full implementation, these would be stored in database
    private static readonly List<BankAccountDto> _connectedAccounts = new();
    private static readonly List<PendingBankTransactionDto> _pendingTransactions = new();

    public Task<List<BankAccountDto>> GetConnectedAccountsAsync()
    {
        return Task.FromResult(_connectedAccounts.ToList());
    }

    public Task<string> CreateLinkTokenAsync(string userId)
    {
        // In production, this would call Plaid's /link/token/create endpoint
        // Requires PLAID_CLIENT_ID, PLAID_SECRET from configuration
        throw new NotImplementedException(
            "Bank integration requires Plaid API credentials. " +
            "Add your PLAID_CLIENT_ID and PLAID_SECRET to appsettings.json and implement the Plaid SDK integration.");
    }

    public Task<BankConnectionResult> ExchangePublicTokenAsync(string publicToken)
    {
        // In production, this would call Plaid's /item/public_token/exchange endpoint
        throw new NotImplementedException(
            "Plaid integration not configured. See https://plaid.com/docs/link/");
    }

    public Task<SyncResult> SyncTransactionsAsync(int accountId)
    {
        // In production, this would call Plaid's /transactions/sync endpoint
        return Task.FromResult(new SyncResult(
            false, 0, 0,
            "Bank sync not configured. Connect a bank account first."));
    }

    public Task<SyncResult> SyncAllAccountsAsync()
    {
        if (!_connectedAccounts.Any())
        {
            return Task.FromResult(new SyncResult(
                false, 0, 0,
                "No bank accounts connected."));
        }

        // Would iterate through all accounts and sync
        return Task.FromResult(new SyncResult(true, 0, 0, null));
    }

    public Task DisconnectAccountAsync(int accountId)
    {
        var account = _connectedAccounts.FirstOrDefault(a => a.Id == accountId);
        if (account != null)
        {
            _connectedAccounts.Remove(account);
        }
        return Task.CompletedTask;
    }

    public Task<List<PendingBankTransactionDto>> GetPendingTransactionsAsync()
    {
        return Task.FromResult(_pendingTransactions.ToList());
    }

    public Task ApproveTransactionAsync(int pendingId, int categoryId, int? fundId = null)
    {
        var pending = _pendingTransactions.FirstOrDefault(p => p.Id == pendingId);
        if (pending != null)
        {
            _pendingTransactions.Remove(pending);
            // In production, would create a Transaction record
        }
        return Task.CompletedTask;
    }

    public Task DismissTransactionAsync(int pendingId)
    {
        var pending = _pendingTransactions.FirstOrDefault(p => p.Id == pendingId);
        if (pending != null)
        {
            _pendingTransactions.Remove(pending);
        }
        return Task.CompletedTask;
    }
}
