using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NonProfitFinance.DTOs;
using NonProfitFinance.Models;
using NonProfitFinance.Services;

namespace NonProfitFinance.Controllers;

/// <summary>
/// API controller for category management.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize] // HIGH-06 fix: Require authorization
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    /// <summary>
    /// Get all categories with optional filtering.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<CategoryDto>>>> GetAll(
        [FromQuery] CategoryType? type = null,
        [FromQuery] bool includeArchived = false)
    {
        var categories = await _categoryService.GetAllAsync(type, includeArchived);
        return Ok(new ApiResponse<List<CategoryDto>>(true, categories));
    }

    /// <summary>
    /// Get category hierarchy tree by type.
    /// </summary>
    [HttpGet("tree/{type}")]
    public async Task<ActionResult<ApiResponse<List<CategoryDto>>>> GetTree(CategoryType type)
    {
        var tree = await _categoryService.GetTreeAsync(type);
        return Ok(new ApiResponse<List<CategoryDto>>(true, tree));
    }

    /// <summary>
    /// Get category by ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> GetById(int id)
    {
        var category = await _categoryService.GetByIdAsync(id);
        if (category == null)
            return NotFound(new ApiResponse<CategoryDto>(false, null, "Category not found"));

        return Ok(new ApiResponse<CategoryDto>(true, category));
    }

    /// <summary>
    /// Create a new category.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> Create([FromBody] CreateCategoryRequest request)
    {
        try
        {
            var category = await _categoryService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = category.Id },
                new ApiResponse<CategoryDto>(true, category, "Category created successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse<CategoryDto>(false, null, ex.Message));
        }
    }

    /// <summary>
    /// Update an existing category.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> Update(int id, [FromBody] UpdateCategoryRequest request)
    {
        try
        {
            var category = await _categoryService.UpdateAsync(id, request);
            if (category == null)
                return NotFound(new ApiResponse<CategoryDto>(false, null, "Category not found"));

            return Ok(new ApiResponse<CategoryDto>(true, category, "Category updated successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse<CategoryDto>(false, null, ex.Message));
        }
    }

    /// <summary>
    /// Delete a category (only if no transactions linked).
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        try
        {
            var result = await _categoryService.DeleteAsync(id);
            if (!result)
                return NotFound(new ApiResponse<bool>(false, false, "Category not found"));

            return Ok(new ApiResponse<bool>(true, true, "Category deleted successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse<bool>(false, false, ex.Message));
        }
    }

    /// <summary>
    /// Archive a category (soft delete).
    /// </summary>
    [HttpPost("{id}/archive")]
    public async Task<ActionResult<ApiResponse<bool>>> Archive(int id)
    {
        var result = await _categoryService.ArchiveAsync(id);
        if (!result)
            return NotFound(new ApiResponse<bool>(false, false, "Category not found"));

        return Ok(new ApiResponse<bool>(true, true, "Category archived successfully"));
    }

    /// <summary>
    /// Restore an archived category.
    /// </summary>
    [HttpPost("{id}/restore")]
    public async Task<ActionResult<ApiResponse<bool>>> Restore(int id)
    {
        var result = await _categoryService.RestoreAsync(id);
        if (!result)
            return NotFound(new ApiResponse<bool>(false, false, "Category not found"));

        return Ok(new ApiResponse<bool>(true, true, "Category restored successfully"));
    }
}
