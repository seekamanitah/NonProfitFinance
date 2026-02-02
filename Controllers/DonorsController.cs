using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NonProfitFinance.DTOs;
using NonProfitFinance.Services;

namespace NonProfitFinance.Controllers;

/// <summary>
/// API controller for donor management.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize] // HIGH-06 fix: Require authorization
public class DonorsController : ControllerBase
{
    private readonly IDonorService _donorService;

    public DonorsController(IDonorService donorService)
    {
        _donorService = donorService;
    }

    /// <summary>
    /// Get all donors.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<DonorDto>>>> GetAll([FromQuery] bool includeInactive = false)
    {
        var donors = await _donorService.GetAllAsync(includeInactive);
        return Ok(new ApiResponse<List<DonorDto>>(true, donors));
    }

    /// <summary>
    /// Get donor by ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<DonorDto>>> GetById(int id)
    {
        var donor = await _donorService.GetByIdAsync(id);
        if (donor == null)
            return NotFound(new ApiResponse<DonorDto>(false, null, "Donor not found"));

        return Ok(new ApiResponse<DonorDto>(true, donor));
    }

    /// <summary>
    /// Get donor contribution history.
    /// </summary>
    [HttpGet("{id}/contributions")]
    public async Task<ActionResult<ApiResponse<List<TransactionDto>>>> GetContributions(int id)
    {
        var contributions = await _donorService.GetContributionHistoryAsync(id);
        return Ok(new ApiResponse<List<TransactionDto>>(true, contributions));
    }

    /// <summary>
    /// Create a new donor.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<DonorDto>>> Create([FromBody] CreateDonorRequest request)
    {
        var donor = await _donorService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = donor.Id },
            new ApiResponse<DonorDto>(true, donor, "Donor created successfully"));
    }

    /// <summary>
    /// Update an existing donor.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<DonorDto>>> Update(int id, [FromBody] UpdateDonorRequest request)
    {
        var donor = await _donorService.UpdateAsync(id, request);
        if (donor == null)
            return NotFound(new ApiResponse<DonorDto>(false, null, "Donor not found"));

        return Ok(new ApiResponse<DonorDto>(true, donor, "Donor updated successfully"));
    }

    /// <summary>
    /// Delete a donor.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        try
        {
            var result = await _donorService.DeleteAsync(id);
            if (!result)
                return NotFound(new ApiResponse<bool>(false, false, "Donor not found"));

            return Ok(new ApiResponse<bool>(true, true, "Donor deleted successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse<bool>(false, false, ex.Message));
        }
    }
}
