using Microsoft.AspNetCore.Mvc;
using NonProfitFinance.DTOs;
using NonProfitFinance.Models;
using NonProfitFinance.Services;

namespace NonProfitFinance.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GrantsController : ControllerBase
{
    private readonly IGrantService _grantService;

    public GrantsController(IGrantService grantService)
    {
        _grantService = grantService;
    }

    /// <summary>
    /// Get all grants with optional status filter.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<GrantDto>>>> GetAll([FromQuery] GrantStatus? status = null)
    {
        var grants = await _grantService.GetAllAsync(status);
        return Ok(new ApiResponse<List<GrantDto>>(true, grants));
    }

    /// <summary>
    /// Get grant by ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<GrantDto>>> GetById(int id)
    {
        var grant = await _grantService.GetByIdAsync(id);
        if (grant == null)
            return NotFound(new ApiResponse<GrantDto>(false, null, "Grant not found"));

        return Ok(new ApiResponse<GrantDto>(true, grant));
    }

    /// <summary>
    /// Get grant usage/transaction history.
    /// </summary>
    [HttpGet("{id}/usage")]
    public async Task<ActionResult<ApiResponse<List<TransactionDto>>>> GetUsage(int id)
    {
        var transactions = await _grantService.GetUsageHistoryAsync(id);
        return Ok(new ApiResponse<List<TransactionDto>>(true, transactions));
    }

    /// <summary>
    /// Get grants expiring within specified days.
    /// </summary>
    [HttpGet("expiring")]
    public async Task<ActionResult<ApiResponse<List<GrantDto>>>> GetExpiring([FromQuery] int daysAhead = 30)
    {
        var grants = await _grantService.GetExpiringGrantsAsync(daysAhead);
        return Ok(new ApiResponse<List<GrantDto>>(true, grants));
    }

    /// <summary>
    /// Get grants with upcoming report deadlines.
    /// </summary>
    [HttpGet("upcoming-reports")]
    public async Task<ActionResult<ApiResponse<List<GrantDto>>>> GetUpcomingReports([FromQuery] int daysAhead = 14)
    {
        var grants = await _grantService.GetGrantsWithUpcomingReportsAsync(daysAhead);
        return Ok(new ApiResponse<List<GrantDto>>(true, grants));
    }

    /// <summary>
    /// Create a new grant.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<GrantDto>>> Create([FromBody] CreateGrantRequest request)
    {
        var grant = await _grantService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = grant.Id },
            new ApiResponse<GrantDto>(true, grant, "Grant created successfully"));
    }

    /// <summary>
    /// Update an existing grant.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<GrantDto>>> Update(int id, [FromBody] UpdateGrantRequest request)
    {
        var grant = await _grantService.UpdateAsync(id, request);
        if (grant == null)
            return NotFound(new ApiResponse<GrantDto>(false, null, "Grant not found"));

        return Ok(new ApiResponse<GrantDto>(true, grant, "Grant updated successfully"));
    }

    /// <summary>
    /// Delete a grant.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        try
        {
            var result = await _grantService.DeleteAsync(id);
            if (!result)
                return NotFound(new ApiResponse<bool>(false, false, "Grant not found"));

            return Ok(new ApiResponse<bool>(true, true, "Grant deleted successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse<bool>(false, false, ex.Message));
        }
    }
}
