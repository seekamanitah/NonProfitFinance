using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NonProfitFinance.DTOs;
using NonProfitFinance.Services;

namespace NonProfitFinance.Controllers;

/// <summary>
/// API controller for transaction management.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize] // HIGH-06 fix: Require authorization for all endpoints
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;
    private const int MaxPageSize = 100; // HIGH-05 fix: Enforce pagination limits

    public TransactionsController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    /// <summary>
    /// Get transactions with filtering and pagination.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<TransactionDto>>>> GetAll(
        [FromQuery] TransactionFilterRequest filter)
    {
        // HIGH-05 fix: Validate pagination limits before service call
        if (filter.PageSize > MaxPageSize)
        {
            return BadRequest(new ApiResponse<PagedResult<TransactionDto>>(
                false, null, $"PageSize cannot exceed {MaxPageSize}"));
        }

        var result = await _transactionService.GetAllAsync(filter);
        return Ok(new ApiResponse<PagedResult<TransactionDto>>(true, result));
    }

    /// <summary>
    /// Get recent transactions for dashboard.
    /// </summary>
    [HttpGet("recent")]
    public async Task<ActionResult<ApiResponse<List<TransactionDto>>>> GetRecent([FromQuery] int count = 10)
    {
        var transactions = await _transactionService.GetRecentAsync(count);
        return Ok(new ApiResponse<List<TransactionDto>>(true, transactions));
    }

    /// <summary>
    /// Get transaction by ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<TransactionDto>>> GetById(int id)
    {
        var transaction = await _transactionService.GetByIdAsync(id);
        if (transaction == null)
            return NotFound(new ApiResponse<TransactionDto>(false, null, "Transaction not found"));

        return Ok(new ApiResponse<TransactionDto>(true, transaction));
    }

    /// <summary>
    /// Create a new transaction.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<TransactionDto>>> Create([FromBody] CreateTransactionRequest request)
    {
        try
        {
            var transaction = await _transactionService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = transaction.Id },
                new ApiResponse<TransactionDto>(true, transaction, "Transaction created successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponse<TransactionDto>(false, null, ex.Message));
        }
    }

    /// <summary>
    /// Update an existing transaction.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<TransactionDto>>> Update(int id, [FromBody] UpdateTransactionRequest request)
    {
        try
        {
            var transaction = await _transactionService.UpdateAsync(id, request);
            if (transaction == null)
                return NotFound(new ApiResponse<TransactionDto>(false, null, "Transaction not found"));

            return Ok(new ApiResponse<TransactionDto>(true, transaction, "Transaction updated successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponse<TransactionDto>(false, null, ex.Message));
        }
    }

    /// <summary>
    /// Delete a transaction (soft delete - can be restored).
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        var result = await _transactionService.DeleteAsync(id);
        if (!result)
            return NotFound(new ApiResponse<bool>(false, false, "Transaction not found"));

        return Ok(new ApiResponse<bool>(true, true, "Transaction deleted successfully"));
    }

    /// <summary>
    /// Restore a soft-deleted transaction.
    /// </summary>
    [HttpPost("{id}/restore")]
    public async Task<ActionResult<ApiResponse<bool>>> Restore(int id)
    {
        var result = await _transactionService.RestoreAsync(id);
        if (!result)
            return NotFound(new ApiResponse<bool>(false, false, "Deleted transaction not found"));

        return Ok(new ApiResponse<bool>(true, true, "Transaction restored successfully"));
    }

    /// <summary>
    /// Get soft-deleted transactions for recovery.
    /// </summary>
    [HttpGet("deleted")]
    public async Task<ActionResult<ApiResponse<List<TransactionDto>>>> GetDeleted([FromQuery] int maxCount = 50)
    {
        var transactions = await _transactionService.GetDeletedAsync(maxCount);
        return Ok(new ApiResponse<List<TransactionDto>>(true, transactions));
    }

    /// <summary>
    /// Permanently delete a transaction (cannot be undone).
    /// </summary>
    [HttpDelete("{id}/permanent")]
    public async Task<ActionResult<ApiResponse<bool>>> PermanentDelete(int id)
    {
        var result = await _transactionService.PermanentDeleteAsync(id);
        if (!result)
            return NotFound(new ApiResponse<bool>(false, false, "Transaction not found"));

        return Ok(new ApiResponse<bool>(true, true, "Transaction permanently deleted"));
    }

    /// <summary>
    /// Process due recurring transactions.
    /// </summary>
    [HttpPost("process-recurring")]
    public async Task<ActionResult<ApiResponse<bool>>> ProcessRecurring()
    {
        await _transactionService.ProcessRecurringTransactionsAsync();
        return Ok(new ApiResponse<bool>(true, true, "Recurring transactions processed"));
    }
}
