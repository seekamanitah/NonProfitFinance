using NonProfitFinance.DTOs;
using NonProfitFinance.Models;

namespace NonProfitFinance.Services;

public interface IRecurringTransactionService
{
    /// <summary>
    /// Get all recurring transaction templates
    /// </summary>
    Task<List<RecurringTransactionDto>> GetAllAsync();
    
    /// <summary>
    /// Create a new recurring transaction template
    /// </summary>
    Task<RecurringTransactionDto> CreateAsync(CreateRecurringTransactionRequest request);
    
    /// <summary>
    /// Update a recurring transaction template
    /// </summary>
    Task<RecurringTransactionDto?> UpdateAsync(int id, UpdateRecurringTransactionRequest request);
    
    /// <summary>
    /// Delete a recurring transaction template
    /// </summary>
    Task<bool> DeleteAsync(int id);
    
    /// <summary>
    /// Process all due recurring transactions and create actual transactions
    /// </summary>
    Task<RecurringProcessResult> ProcessDueTransactionsAsync();
    
    /// <summary>
    /// Get upcoming recurring transactions
    /// </summary>
    Task<List<UpcomingRecurringDto>> GetUpcomingAsync(int daysAhead = 30);
    
    /// <summary>
    /// Skip the next occurrence of a recurring transaction
    /// </summary>
    Task SkipNextOccurrenceAsync(int id);
}

public record RecurringTransactionDto(
    int Id,
    string Name,
    decimal Amount,
    TransactionType Type,
    int CategoryId,
    string? CategoryName,
    RecurrencePattern Pattern,
    int Interval,
    DateTime StartDate,
    DateTime? EndDate,
    DateTime? NextOccurrence,
    DateTime? LastProcessed,
    bool IsActive,
    int TotalOccurrences
);

public record CreateRecurringTransactionRequest(
    string Name,
    decimal Amount,
    TransactionType Type,
    int CategoryId,
    RecurrencePattern Pattern,
    int Interval = 1,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    int? FundId = null,
    int? DonorId = null,
    int? GrantId = null,
    string? Payee = null,
    string? Description = null
);

public record UpdateRecurringTransactionRequest(
    string Name,
    decimal Amount,
    int CategoryId,
    RecurrencePattern Pattern,
    int Interval,
    DateTime? EndDate,
    bool IsActive
);

public record RecurringProcessResult(
    int TotalProcessed,
    int SuccessCount,
    int FailedCount,
    List<string> Errors,
    List<int> CreatedTransactionIds
);

public record UpcomingRecurringDto(
    int RecurringId,
    string Name,
    decimal Amount,
    TransactionType Type,
    string? CategoryName,
    DateTime NextDate,
    int DaysUntil
);

public enum RecurrencePattern
{
    Daily,
    Weekly,
    BiWeekly,
    Monthly,
    Quarterly,
    Yearly
}
