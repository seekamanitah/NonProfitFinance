using Microsoft.AspNetCore.Mvc;
using NonProfitFinance.DTOs;
using NonProfitFinance.Services;

namespace NonProfitFinance.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FundsController : ControllerBase
{
    private readonly IFundService _fundService;

    public FundsController(IFundService fundService)
    {
        _fundService = fundService;
    }

    /// <summary>
    /// Get all funds.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<FundDto>>>> GetAll([FromQuery] bool includeInactive = false)
    {
        var funds = await _fundService.GetAllAsync(includeInactive);
        return Ok(new ApiResponse<List<FundDto>>(true, funds));
    }

    /// <summary>
    /// Get fund by ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<FundDto>>> GetById(int id)
    {
        var fund = await _fundService.GetByIdAsync(id);
        if (fund == null)
            return NotFound(new ApiResponse<FundDto>(false, null, "Fund not found"));

        return Ok(new ApiResponse<FundDto>(true, fund));
    }

    /// <summary>
    /// Get fund balance summary.
    /// </summary>
    [HttpGet("summary")]
    public async Task<ActionResult<ApiResponse<object>>> GetSummary()
    {
        var restricted = await _fundService.GetTotalRestrictedBalanceAsync();
        var unrestricted = await _fundService.GetTotalUnrestrictedBalanceAsync();

        return Ok(new ApiResponse<object>(true, new
        {
            TotalBalance = restricted + unrestricted,
            RestrictedBalance = restricted,
            UnrestrictedBalance = unrestricted,
            RestrictedPercentage = (restricted + unrestricted) > 0
                ? (restricted / (restricted + unrestricted)) * 100
                : 0
        }));
    }

    /// <summary>
    /// Create a new fund.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<FundDto>>> Create([FromBody] CreateFundRequest request)
    {
        var fund = await _fundService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = fund.Id },
            new ApiResponse<FundDto>(true, fund, "Fund created successfully"));
    }

    /// <summary>
    /// Update an existing fund.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<FundDto>>> Update(int id, [FromBody] UpdateFundRequest request)
    {
        var fund = await _fundService.UpdateAsync(id, request);
        if (fund == null)
            return NotFound(new ApiResponse<FundDto>(false, null, "Fund not found"));

        return Ok(new ApiResponse<FundDto>(true, fund, "Fund updated successfully"));
    }

    /// <summary>
    /// Delete a fund.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        try
        {
            var result = await _fundService.DeleteAsync(id);
            if (!result)
                return NotFound(new ApiResponse<bool>(false, false, "Fund not found"));

            return Ok(new ApiResponse<bool>(true, true, "Fund deleted successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse<bool>(false, false, ex.Message));
        }
    }

    /// <summary>
    /// Recalculate all fund balances from transactions.
    /// </summary>
    [HttpPost("recalculate")]
    public async Task<ActionResult<ApiResponse<bool>>> Recalculate()
    {
        await _fundService.RecalculateAllBalancesAsync();
        return Ok(new ApiResponse<bool>(true, true, "Fund balances recalculated"));
    }
}
