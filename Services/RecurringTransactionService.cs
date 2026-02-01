using Microsoft.EntityFrameworkCore;
using NonProfitFinance.Data;
using NonProfitFinance.DTOs;
using NonProfitFinance.Models;

namespace NonProfitFinance.Services;

public class RecurringTransactionService : IRecurringTransactionService
{
    private readonly ApplicationDbContext _context;
    private readonly ITransactionService _transactionService;

    // In-memory store for recurring templates (would be database in full implementation)
    private static readonly List<RecurringTemplate> _templates = new();
    private static int _nextId = 1;

    public RecurringTransactionService(ApplicationDbContext context, ITransactionService transactionService)
    {
        _context = context;
        _transactionService = transactionService;
    }

    public Task<List<RecurringTransactionDto>> GetAllAsync()
    {
        var categories = _context.Categories.ToDictionary(c => c.Id, c => c.Name);
        
        var dtos = _templates.Select(t => new RecurringTransactionDto(
            t.Id,
            t.Name,
            t.Amount,
            t.Type,
            t.CategoryId,
            categories.GetValueOrDefault(t.CategoryId),
            t.Pattern,
            t.Interval,
            t.StartDate,
            t.EndDate,
            t.NextOccurrence,
            t.LastProcessed,
            t.IsActive,
            t.TotalOccurrences
        )).ToList();

        return Task.FromResult(dtos);
    }

    public async Task<RecurringTransactionDto> CreateAsync(CreateRecurringTransactionRequest request)
    {
        var template = new RecurringTemplate
        {
            Id = _nextId++,
            Name = request.Name,
            Amount = request.Amount,
            Type = request.Type,
            CategoryId = request.CategoryId,
            Pattern = request.Pattern,
            Interval = request.Interval,
            StartDate = request.StartDate ?? DateTime.Today,
            EndDate = request.EndDate,
            FundId = request.FundId,
            DonorId = request.DonorId,
            GrantId = request.GrantId,
            Payee = request.Payee,
            Description = request.Description,
            IsActive = true,
            NextOccurrence = request.StartDate ?? DateTime.Today,
            TotalOccurrences = 0
        };

        _templates.Add(template);

        var category = await _context.Categories.FindAsync(request.CategoryId);

        return new RecurringTransactionDto(
            template.Id,
            template.Name,
            template.Amount,
            template.Type,
            template.CategoryId,
            category?.Name,
            template.Pattern,
            template.Interval,
            template.StartDate,
            template.EndDate,
            template.NextOccurrence,
            template.LastProcessed,
            template.IsActive,
            template.TotalOccurrences
        );
    }

    public async Task<RecurringTransactionDto?> UpdateAsync(int id, UpdateRecurringTransactionRequest request)
    {
        var template = _templates.FirstOrDefault(t => t.Id == id);
        if (template == null) return null;

        template.Name = request.Name;
        template.Amount = request.Amount;
        template.CategoryId = request.CategoryId;
        template.Pattern = request.Pattern;
        template.Interval = request.Interval;
        template.EndDate = request.EndDate;
        template.IsActive = request.IsActive;

        var category = await _context.Categories.FindAsync(request.CategoryId);

        return new RecurringTransactionDto(
            template.Id,
            template.Name,
            template.Amount,
            template.Type,
            template.CategoryId,
            category?.Name,
            template.Pattern,
            template.Interval,
            template.StartDate,
            template.EndDate,
            template.NextOccurrence,
            template.LastProcessed,
            template.IsActive,
            template.TotalOccurrences
        );
    }

    public Task<bool> DeleteAsync(int id)
    {
        var template = _templates.FirstOrDefault(t => t.Id == id);
        if (template == null) return Task.FromResult(false);
        
        _templates.Remove(template);
        return Task.FromResult(true);
    }

    public async Task<RecurringProcessResult> ProcessDueTransactionsAsync()
    {
        var today = DateTime.Today;
        var processed = 0;
        var success = 0;
        var failed = 0;
        var errors = new List<string>();
        var createdIds = new List<int>();

        var dueTemplates = _templates
            .Where(t => t.IsActive && t.NextOccurrence <= today && (t.EndDate == null || t.EndDate >= today))
            .ToList();

        foreach (var template in dueTemplates)
        {
            processed++;
            try
            {
                var request = new CreateTransactionRequest(
                    template.NextOccurrence ?? today,
                    template.Amount,
                    template.Description ?? template.Name,
                    template.Type,
                    template.CategoryId,
                    FundType.Unrestricted,
                    template.FundId,
                    null, // ToFundId
                    template.DonorId,
                    template.GrantId,
                    template.Payee,
                    "recurring",
                    null, // ReferenceNumber
                    null, // PONumber
                    false, // IsRecurring (this is a generated transaction, not a template)
                    null // RecurrencePattern
                );

                var tx = await _transactionService.CreateAsync(request);
                createdIds.Add(tx.Id);

                template.LastProcessed = today;
                template.NextOccurrence = CalculateNextOccurrence(template.NextOccurrence ?? today, template.Pattern, template.Interval);
                template.TotalOccurrences++;
                success++;
            }
            catch (Exception ex)
            {
                failed++;
                errors.Add($"Failed to process '{template.Name}': {ex.Message}");
            }
        }

        return new RecurringProcessResult(processed, success, failed, errors, createdIds);
    }

    public Task<List<UpcomingRecurringDto>> GetUpcomingAsync(int daysAhead = 30)
    {
        var today = DateTime.Today;
        var endDate = today.AddDays(daysAhead);
        var categories = _context.Categories.ToDictionary(c => c.Id, c => c.Name);

        var upcoming = _templates
            .Where(t => t.IsActive && t.NextOccurrence.HasValue && 
                        t.NextOccurrence >= today && t.NextOccurrence <= endDate)
            .Select(t => new UpcomingRecurringDto(
                t.Id,
                t.Name,
                t.Amount,
                t.Type,
                categories.GetValueOrDefault(t.CategoryId),
                t.NextOccurrence!.Value,
                (t.NextOccurrence.Value - today).Days
            ))
            .OrderBy(u => u.NextDate)
            .ToList();

        return Task.FromResult(upcoming);
    }

    public Task SkipNextOccurrenceAsync(int id)
    {
        var template = _templates.FirstOrDefault(t => t.Id == id);
        if (template != null && template.NextOccurrence.HasValue)
        {
            template.NextOccurrence = CalculateNextOccurrence(
                template.NextOccurrence.Value, template.Pattern, template.Interval);
        }
        return Task.CompletedTask;
    }

    private static DateTime CalculateNextOccurrence(DateTime current, RecurrencePattern pattern, int interval)
    {
        return pattern switch
        {
            RecurrencePattern.Daily => current.AddDays(interval),
            RecurrencePattern.Weekly => current.AddDays(7 * interval),
            RecurrencePattern.BiWeekly => current.AddDays(14 * interval),
            RecurrencePattern.Monthly => current.AddMonths(interval),
            RecurrencePattern.Quarterly => current.AddMonths(3 * interval),
            RecurrencePattern.Yearly => current.AddYears(interval),
            _ => current.AddMonths(interval)
        };
    }

    // Internal template class
    private class RecurringTemplate
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public decimal Amount { get; set; }
        public TransactionType Type { get; set; }
        public int CategoryId { get; set; }
        public RecurrencePattern Pattern { get; set; }
        public int Interval { get; set; } = 1;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? NextOccurrence { get; set; }
        public DateTime? LastProcessed { get; set; }
        public int? FundId { get; set; }
        public int? DonorId { get; set; }
        public int? GrantId { get; set; }
        public string? Payee { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public int TotalOccurrences { get; set; }
    }
}
