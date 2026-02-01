using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NonProfitFinance.Data;
using NonProfitFinance.DTOs;
using NonProfitFinance.Models;

namespace NonProfitFinance.Services;

public class TransactionService : ITransactionService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TransactionService> _logger;
    private readonly IAuditService _auditService;

    public TransactionService(
        ApplicationDbContext context, 
        ILogger<TransactionService> logger,
        IAuditService auditService)
    {
        _context = context;
        _logger = logger;
        _auditService = auditService;
    }

    private const int MaxPageSize = 100;
    private const int DefaultPageSize = 50;

    public async Task<PagedResult<TransactionDto>> GetAllAsync(TransactionFilterRequest filter)
    {
        // Enforce pagination limits to prevent DoS
        var pageSize = Math.Min(Math.Max(filter.PageSize, 1), MaxPageSize);
        var page = Math.Max(filter.Page, 1);

        var query = _context.Transactions
            .Include(t => t.Category)
            .Include(t => t.Fund)
            .Include(t => t.Donor)
            .Include(t => t.Grant)
            .Include(t => t.Splits).ThenInclude(s => s.Category)
            .AsQueryable();

        // Apply filters
        if (filter.StartDate.HasValue)
            query = query.Where(t => t.Date >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(t => t.Date <= filter.EndDate.Value);

        if (filter.CategoryId.HasValue)
            query = query.Where(t => t.CategoryId == filter.CategoryId.Value ||
                                     t.Splits.Any(s => s.CategoryId == filter.CategoryId.Value));

        if (filter.FundId.HasValue)
            query = query.Where(t => t.FundId == filter.FundId.Value);

        if (filter.DonorId.HasValue)
            query = query.Where(t => t.DonorId == filter.DonorId.Value);

        if (filter.GrantId.HasValue)
            query = query.Where(t => t.GrantId == filter.GrantId.Value);

        if (filter.Type.HasValue)
            query = query.Where(t => t.Type == filter.Type.Value);

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.ToLower();
            query = query.Where(t =>
                (t.Description != null && t.Description.ToLower().Contains(term)) ||
                (t.Payee != null && t.Payee.ToLower().Contains(term)) ||
                (t.Tags != null && t.Tags.ToLower().Contains(term)));
        }

        // Filter by tags (comma-separated)
        if (!string.IsNullOrWhiteSpace(filter.Tags))
        {
            var tagList = filter.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim().ToLower())
                .Where(t => !string.IsNullOrEmpty(t))
                .ToList();
            
            if (tagList.Any())
            {
                query = query.Where(t => t.Tags != null && 
                    tagList.Any(tag => t.Tags.ToLower().Contains(tag)));
            }
        }

        var totalCount = await query.CountAsync();

        // Use projection to avoid N+1 queries - select only needed fields
        var transactions = await query
            .OrderByDescending(t => t.Date)
            .ThenByDescending(t => t.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TransactionDto(
                t.Id,
                t.Date,
                t.Amount,
                t.Description,
                t.Type,
                t.CategoryId,
                t.Category != null ? t.Category.Name : null,
                t.FundType,
                t.FundId,
                t.Fund != null ? t.Fund.Name : null,
                t.ToFundId,
                t.ToFund != null ? t.ToFund.Name : null,
                t.TransferPairId,
                t.DonorId,
                t.Donor != null ? t.Donor.Name : null,
                t.GrantId,
                t.Grant != null ? t.Grant.Name : null,
                t.Payee,
                t.Tags,
                t.ReferenceNumber,
                t.PONumber,
                t.IsRecurring,
                t.IsReconciled,
                t.Splits.Select(s => new TransactionSplitDto(
                    s.Id,
                    s.CategoryId,
                    s.Category != null ? s.Category.Name : null,
                    s.Amount,
                    s.Description
                )).ToList()
            ))
            .ToListAsync();

        return new PagedResult<TransactionDto>(
            transactions,
            totalCount,
            page,
            pageSize,
            (int)Math.Ceiling(totalCount / (double)pageSize)
        );
    }

    public async Task<TransactionDto?> GetByIdAsync(int id)
    {
        var transaction = await _context.Transactions
            .Include(t => t.Category)
            .Include(t => t.Fund)
            .Include(t => t.Donor)
            .Include(t => t.Grant)
            .Include(t => t.Splits).ThenInclude(s => s.Category)
            .FirstOrDefaultAsync(t => t.Id == id);

        return transaction == null ? null : MapToDto(transaction);
    }

    public async Task<TransactionDto> CreateAsync(CreateTransactionRequest request)
    {
        _logger.LogInformation("Creating transaction: Type={Type}, Amount={Amount}, Category={CategoryId}", 
            request.Type, request.Amount, request.CategoryId);

        // Handle transfers specially - create paired transactions
        if (request.Type == TransactionType.Transfer)
        {
            _logger.LogInformation("Creating transfer from Fund {FromFund} to Fund {ToFund}", 
                request.FundId, request.ToFundId);
            return await CreateTransferAsync(request);
        }

        // Validate grant spending doesn't exceed remaining balance
        if (request.GrantId.HasValue && request.Type == TransactionType.Expense)
        {
            var grant = await _context.Grants.FindAsync(request.GrantId.Value);
            if (grant != null)
            {
                var remainingBalance = grant.Amount - grant.AmountUsed;
                if (request.Amount > remainingBalance)
                {
                    throw new InvalidOperationException(
                        $"Expense amount (${request.Amount:N2}) exceeds grant remaining balance (${remainingBalance:N2}). " +
                        $"Grant '{grant.Name}' has ${grant.AmountUsed:N2} used of ${grant.Amount:N2} total.");
                }
            }
        }

        // Validate fund balance for expenses
        if (request.FundId.HasValue && request.Type == TransactionType.Expense)
        {
            var fund = await _context.Funds.FindAsync(request.FundId.Value);
            if (fund != null && fund.Balance < request.Amount)
            {
                // Warning only - don't prevent, but log
                // Organizations may allow temporary negative balances
            }
        }

        var transaction = new Transaction
        {
            Date = request.Date,
            Amount = request.Amount,
            Description = request.Description,
            Type = request.Type,
            CategoryId = request.CategoryId,
            FundType = request.FundType,
            FundId = request.FundId,
            DonorId = request.DonorId,
            GrantId = request.GrantId,
            Payee = request.Payee,
            Tags = request.Tags,
            ReferenceNumber = request.ReferenceNumber,
            PONumber = request.PONumber,
            IsRecurring = request.IsRecurring,
            RecurrencePattern = request.RecurrencePattern
        };

        // Calculate next recurrence date if recurring
        if (request.IsRecurring && !string.IsNullOrEmpty(request.RecurrencePattern))
        {
            transaction.NextRecurrenceDate = CalculateNextRecurrence(request.Date, request.RecurrencePattern);
        }

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        // Add splits if provided (with validation)
        if (request.Splits?.Any() == true)
        {
            // Validate split amounts equal transaction total
            var splitTotal = request.Splits.Sum(s => s.Amount);
            if (Math.Abs(splitTotal - request.Amount) > 0.01m)
            {
                throw new InvalidOperationException(
                    $"Split amounts (${splitTotal:N2}) must equal transaction amount (${request.Amount:N2})");
            }

            foreach (var split in request.Splits)
            {
                _context.TransactionSplits.Add(new TransactionSplit
                {
                    TransactionId = transaction.Id,
                    CategoryId = split.CategoryId,
                    Amount = split.Amount,
                    Description = split.Description
                });
            }
            await _context.SaveChangesAsync();
        }

        // Update donor totals if applicable
        if (request.DonorId.HasValue && request.Type == TransactionType.Income)
        {
            await UpdateDonorTotalsAsync(request.DonorId.Value);
        }

        // Update grant usage if applicable
        if (request.GrantId.HasValue)
        {
            await UpdateGrantUsageAsync(request.GrantId.Value);
        }

        // Update fund balance
        if (request.FundId.HasValue)
        {
            await UpdateFundBalanceAsync(request.FundId.Value);
        }

        // Audit log
        await _auditService.LogAsync(
            AuditAction.Create,
            "Transaction",
            transaction.Id,
            $"Created {request.Type} transaction for ${request.Amount:N2}",
            newValues: new { request.Amount, request.Type, request.CategoryId, request.FundId, request.Description }
        );

        return (await GetByIdAsync(transaction.Id))!;
    }

    private async Task<TransactionDto> CreateTransferAsync(CreateTransactionRequest request)
    {
        // Validation
        if (!request.FundId.HasValue || !request.ToFundId.HasValue)
            throw new InvalidOperationException("Both source and destination accounts required for transfers");

        if (request.FundId == request.ToFundId)
            throw new InvalidOperationException("Cannot transfer to the same account");

        // Use explicit transaction for atomicity
        await using var dbTransaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            // Get fund names for descriptions
            var fromFund = await _context.Funds.FindAsync(request.FundId.Value);
            var toFund = await _context.Funds.FindAsync(request.ToFundId.Value);

            if (fromFund == null || toFund == null)
                throw new InvalidOperationException("Invalid account selection");

            // Find or create Transfer category
            var transferCategory = await _context.Categories
                .FirstOrDefaultAsync(c => c.Name == "Transfer" && c.Type == CategoryType.Expense);

            if (transferCategory == null)
            {
                transferCategory = new Category
                {
                    Name = "Transfer",
                    Type = CategoryType.Expense,
                    Description = "Internal transfers between accounts",
                    Color = "#6B7280"
                };
                _context.Categories.Add(transferCategory);
                await _context.SaveChangesAsync();
            }

            var transferPairId = Guid.NewGuid();

            // Transaction 1: Debit from source (Expense)
            var expenseTransaction = new Transaction
            {
                Date = request.Date,
                Amount = request.Amount,
                Description = request.Description ?? $"Transfer to {toFund.Name}",
                Type = TransactionType.Expense,
                CategoryId = transferCategory.Id,
                FundType = FundType.Unrestricted,
                FundId = request.FundId,
                ToFundId = request.ToFundId,
                TransferPairId = transferPairId,
                ReferenceNumber = request.ReferenceNumber,
                Tags = "Transfer"
            };

            // Transaction 2: Credit to destination (Income)
            var incomeTransaction = new Transaction
            {
                Date = request.Date,
                Amount = request.Amount,
                Description = request.Description ?? $"Transfer from {fromFund.Name}",
                Type = TransactionType.Income,
                CategoryId = transferCategory.Id,
                FundType = FundType.Unrestricted,
                FundId = request.ToFundId,
                ToFundId = request.FundId,
                TransferPairId = transferPairId,
                ReferenceNumber = request.ReferenceNumber,
                Tags = "Transfer"
            };

            _context.Transactions.AddRange(expenseTransaction, incomeTransaction);
            await _context.SaveChangesAsync();

            // Update both fund balances
            await UpdateFundBalanceAsync(request.FundId.Value);
            await UpdateFundBalanceAsync(request.ToFundId.Value);

            // Commit transaction
            await dbTransaction.CommitAsync();

            return (await GetByIdAsync(expenseTransaction.Id))!;
        }
        catch
        {
            await dbTransaction.RollbackAsync();
            throw;
        }
    }

    public async Task<TransactionDto?> UpdateAsync(int id, UpdateTransactionRequest request)
    {
        var transaction = await _context.Transactions
            .Include(t => t.Splits)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (transaction == null) return null;

        var oldDonorId = transaction.DonorId;
        var oldGrantId = transaction.GrantId;
        var oldFundId = transaction.FundId;

        transaction.Date = request.Date;
        transaction.Amount = request.Amount;
        transaction.Description = request.Description;
        transaction.Type = request.Type;
        transaction.CategoryId = request.CategoryId;
        transaction.FundType = request.FundType;
        transaction.FundId = request.FundId;
        transaction.DonorId = request.DonorId;
        transaction.GrantId = request.GrantId;
        transaction.Payee = request.Payee;
        transaction.Tags = request.Tags;
        transaction.ReferenceNumber = request.ReferenceNumber;
        transaction.PONumber = request.PONumber;
        transaction.IsReconciled = request.IsReconciled;
        transaction.UpdatedAt = DateTime.UtcNow;

        // Update splits
        _context.TransactionSplits.RemoveRange(transaction.Splits);
        if (request.Splits?.Any() == true)
        {
            foreach (var split in request.Splits)
            {
                _context.TransactionSplits.Add(new TransactionSplit
                {
                    TransactionId = transaction.Id,
                    CategoryId = split.CategoryId,
                    Amount = split.Amount,
                    Description = split.Description
                });
            }
        }

        await _context.SaveChangesAsync();

        // Update affected totals
        if (oldDonorId.HasValue) await UpdateDonorTotalsAsync(oldDonorId.Value);
        if (request.DonorId.HasValue && request.DonorId != oldDonorId) await UpdateDonorTotalsAsync(request.DonorId.Value);

        if (oldGrantId.HasValue) await UpdateGrantUsageAsync(oldGrantId.Value);
        if (request.GrantId.HasValue && request.GrantId != oldGrantId) await UpdateGrantUsageAsync(request.GrantId.Value);

        if (oldFundId.HasValue) await UpdateFundBalanceAsync(oldFundId.Value);
        if (request.FundId.HasValue && request.FundId != oldFundId) await UpdateFundBalanceAsync(request.FundId.Value);

        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        // Use IgnoreQueryFilters to find even soft-deleted records
        var transaction = await _context.Transactions
            .IgnoreQueryFilters()
            .Include(t => t.Splits)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (transaction == null) return false;

        var donorId = transaction.DonorId;
        var grantId = transaction.GrantId;
        var fundId = transaction.FundId;

        // Soft delete instead of hard delete
        transaction.SoftDelete();
        transaction.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();

        // Update affected totals
        if (donorId.HasValue) await UpdateDonorTotalsAsync(donorId.Value);
        if (grantId.HasValue) await UpdateGrantUsageAsync(grantId.Value);
        if (fundId.HasValue) await UpdateFundBalanceAsync(fundId.Value);

        _logger.LogInformation("Transaction {Id} soft-deleted", id);
        
        // Audit log
        await _auditService.LogAsync(
            AuditAction.Delete,
            "Transaction",
            id,
            $"Soft-deleted transaction #{id}",
            oldValues: new { transaction.Amount, transaction.Type, transaction.Description }
        );
        
        return true;
    }

    public async Task<bool> RestoreAsync(int id)
    {
        var transaction = await _context.Transactions
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == id && t.IsDeleted);

        if (transaction == null) return false;

        transaction.Restore();
        transaction.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();

        // Recalculate affected totals
        if (transaction.DonorId.HasValue) await UpdateDonorTotalsAsync(transaction.DonorId.Value);
        if (transaction.GrantId.HasValue) await UpdateGrantUsageAsync(transaction.GrantId.Value);
        if (transaction.FundId.HasValue) await UpdateFundBalanceAsync(transaction.FundId.Value);

        _logger.LogInformation("Transaction {Id} restored from soft-delete", id);
        return true;
    }

    public async Task<List<TransactionDto>> GetDeletedAsync(int maxCount = 50)
    {
        var transactions = await _context.Transactions
            .IgnoreQueryFilters()
            .Where(t => t.IsDeleted)
            .Include(t => t.Category)
            .Include(t => t.Fund)
            .OrderByDescending(t => t.DeletedAt)
            .Take(maxCount)
            .ToListAsync();

        return transactions.Select(MapToDto).ToList();
    }

    public async Task<bool> PermanentDeleteAsync(int id)
    {
        var transaction = await _context.Transactions
            .IgnoreQueryFilters()
            .Include(t => t.Splits)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (transaction == null) return false;

        _context.TransactionSplits.RemoveRange(transaction.Splits);
        _context.Transactions.Remove(transaction);
        await _context.SaveChangesAsync();

        _logger.LogWarning("Transaction {Id} permanently deleted", id);
        return true;
    }

    public async Task<List<TransactionDto>> GetRecentAsync(int count = 10)
    {
        var transactions = await _context.Transactions
            .Include(t => t.Category)
            .Include(t => t.Fund)
            .Include(t => t.Donor)
            .Include(t => t.Grant)
            .OrderByDescending(t => t.Date)
            .ThenByDescending(t => t.Id)
            .Take(count)
            .ToListAsync();

        return transactions.Select(MapToDto).ToList();
    }

    public async Task<List<TransactionDto>> GetByDonorAsync(int donorId)
    {
        var transactions = await _context.Transactions
            .Include(t => t.Category)
            .Include(t => t.Fund)
            .Where(t => t.DonorId == donorId)
            .OrderByDescending(t => t.Date)
            .ToListAsync();

        return transactions.Select(MapToDto).ToList();
    }

    public async Task<List<TransactionDto>> GetByGrantAsync(int grantId)
    {
        var transactions = await _context.Transactions
            .Include(t => t.Category)
            .Include(t => t.Fund)
            .Where(t => t.GrantId == grantId)
            .OrderByDescending(t => t.Date)
            .ToListAsync();

        return transactions.Select(MapToDto).ToList();
    }

    public async Task ProcessRecurringTransactionsAsync()
    {
        var today = DateTime.UtcNow.Date;
        var recurringTransactions = await _context.Transactions
            .Where(t => t.IsRecurring &&
                        t.NextRecurrenceDate.HasValue &&
                        t.NextRecurrenceDate.Value.Date <= today)
            .ToListAsync();

        foreach (var template in recurringTransactions)
        {
            // Create new transaction from template
            var newTransaction = new Transaction
            {
                Date = template.NextRecurrenceDate!.Value,
                Amount = template.Amount,
                Description = template.Description,
                Type = template.Type,
                CategoryId = template.CategoryId,
                FundType = template.FundType,
                FundId = template.FundId,
                DonorId = template.DonorId,
                GrantId = template.GrantId,
                Payee = template.Payee,
                Tags = template.Tags
            };

            _context.Transactions.Add(newTransaction);

            // Update next recurrence date
            template.NextRecurrenceDate = CalculateNextRecurrence(
                template.NextRecurrenceDate.Value,
                template.RecurrencePattern!);
        }

        await _context.SaveChangesAsync();
    }

    private static DateTime CalculateNextRecurrence(DateTime currentDate, string pattern)
    {
        return pattern.ToLower() switch
        {
            "daily" => currentDate.AddDays(1),
            "weekly" => currentDate.AddDays(7),
            "biweekly" => currentDate.AddDays(14),
            "monthly" => currentDate.AddMonths(1),
            "quarterly" => currentDate.AddMonths(3),
            "yearly" => currentDate.AddYears(1),
            _ => currentDate.AddMonths(1)
        };
    }

    private async Task UpdateDonorTotalsAsync(int donorId)
    {
        var donor = await _context.Donors.FindAsync(donorId);
        if (donor == null) return;

        var contributions = await _context.Transactions
            .Where(t => t.DonorId == donorId && t.Type == TransactionType.Income)
            .ToListAsync();

        donor.TotalContributions = contributions.Sum(t => t.Amount);
        donor.FirstContributionDate = contributions.Min(t => (DateTime?)t.Date);
        donor.LastContributionDate = contributions.Max(t => (DateTime?)t.Date);
        donor.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    private async Task UpdateGrantUsageAsync(int grantId)
    {
        var grant = await _context.Grants.FindAsync(grantId);
        if (grant == null) return;

        grant.AmountUsed = await _context.Transactions
            .Where(t => t.GrantId == grantId && t.Type == TransactionType.Expense)
            .SumAsync(t => t.Amount);

        grant.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    private async Task UpdateFundBalanceAsync(int fundId)
    {
        var fund = await _context.Funds.FindAsync(fundId);
        if (fund == null) return;

        var income = await _context.Transactions
            .Where(t => t.FundId == fundId && t.Type == TransactionType.Income)
            .SumAsync(t => t.Amount);

        var expenses = await _context.Transactions
            .Where(t => t.FundId == fundId && t.Type == TransactionType.Expense)
            .SumAsync(t => t.Amount);

        // Balance = Starting Balance + Income - Expenses
        fund.Balance = fund.StartingBalance + income - expenses;
        fund.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    private static TransactionDto MapToDto(Transaction t)
    {
        return new TransactionDto(
            t.Id,
            t.Date,
            t.Amount,
            t.Description,
            t.Type,
            t.CategoryId,
            t.Category?.Name,
            t.FundType,
            t.FundId,
            t.Fund?.Name,
            t.ToFundId,
            t.ToFund?.Name,
            t.TransferPairId,
            t.DonorId,
            t.Donor?.Name,
            t.GrantId,
            t.Grant?.Name,
            t.Payee,
            t.Tags,
            t.ReferenceNumber,
            t.PONumber,
            t.IsRecurring,
            t.IsReconciled,
            t.Splits?.Select(s => new TransactionSplitDto(
                s.Id,
                s.CategoryId,
                s.Category?.Name,
                s.Amount,
                s.Description
            )).ToList()
        );
    }

    public async Task<List<PayeeSuggestion>> GetPayeeSuggestionsAsync(string searchTerm, int maxResults = 10)
    {
        if (string.IsNullOrWhiteSpace(searchTerm) || searchTerm.Length < 2)
        {
            return new List<PayeeSuggestion>();
        }

        var term = searchTerm.ToLower();
        
        var payees = await _context.Transactions
            .Include(t => t.Category)
            .Where(t => t.Payee != null && t.Payee.ToLower().Contains(term))
            .GroupBy(t => t.Payee)
            .Select(g => new
            {
                Payee = g.Key,
                Count = g.Count(),
                LastTransaction = g.OrderByDescending(t => t.Date).First()
            })
            .OrderByDescending(x => x.Count)
            .Take(maxResults)
            .ToListAsync();

        return payees.Select(p => new PayeeSuggestion(
            p.Payee!,
            p.Count,
            p.LastTransaction.CategoryId,
            p.LastTransaction.Category?.Name
        )).ToList();
    }

    public async Task<List<string>> GetDistinctTagsAsync()
    {
        var allTags = await _context.Transactions
            .Where(t => t.Tags != null && t.Tags != "")
            .Select(t => t.Tags)
            .ToListAsync();

        var distinctTags = allTags
            .SelectMany(tags => tags!.Split(',', StringSplitOptions.RemoveEmptyEntries))
            .Select(tag => tag.Trim())
            .Where(tag => !string.IsNullOrEmpty(tag))
            .Distinct()
            .OrderBy(tag => tag)
            .ToList();

        return distinctTags;
    }

    public async Task<List<DuplicateTransactionWarning>> CheckForDuplicatesAsync(
        DateTime date, decimal amount, string? payee)
    {
        // Look for transactions with same date and amount within 24 hours
        var startDate = date.Date;
        var endDate = date.Date.AddDays(1);

        var query = _context.Transactions
            .Where(t => t.Date >= startDate && t.Date < endDate)
            .Where(t => t.Amount == amount);

        // If payee provided, also match on payee
        if (!string.IsNullOrWhiteSpace(payee))
        {
            var payeeLower = payee.ToLower();
            query = query.Where(t => t.Payee != null && t.Payee.ToLower() == payeeLower);
        }

        var potentialDuplicates = await query
            .Select(t => new DuplicateTransactionWarning(
                t.Id,
                t.Date,
                t.Amount,
                t.Payee,
                t.Description
            ))
            .Take(5)
            .ToListAsync();

        return potentialDuplicates;
    }
}
