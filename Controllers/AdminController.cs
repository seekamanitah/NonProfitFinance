using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NonProfitFinance.Services;

namespace NonProfitFinance.Controllers;

/// <summary>
/// Admin operations controller - dangerous operations like database reset
/// </summary>
[ApiController]
[Route("api/admin")]
[Authorize] // Require authentication
public class AdminController : ControllerBase
{
    private readonly IDatabaseResetService _databaseResetService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        IDatabaseResetService databaseResetService,
        ILogger<AdminController> logger)
    {
        _databaseResetService = databaseResetService;
        _logger = logger;
    }

    /// <summary>
    /// Reset the entire database (dangerous operation)
    /// Requires authentication. Creates emergency backup before reset.
    /// </summary>
    [HttpPost("reset-database")]
    // Temporarily allow any authenticated user (in development)
    // TODO: Re-enable role check for production: [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ResetDatabase()
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name ?? "unknown";
            _logger.LogWarning("Database reset initiated by user: {UserId}", userId);

            var result = await _databaseResetService.ResetDatabaseAsync();

            if (result.Success)
            {
                return Ok(new
                {
                    success = true,
                    message = result.Message,
                    backupFilePath = result.BackupFilePath,
                    resetTime = result.ResetTime
                });
            }
            else
            {
                return BadRequest(new
                {
                    success = false,
                    message = result.Message,
                    backupFilePath = result.BackupFilePath
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during database reset");
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                success = false,
                message = "An unexpected error occurred during reset",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Check if current user is admin (for UI permission checks)
    /// </summary>
    [HttpGet("is-admin")]
    [Authorize]
    public IActionResult IsAdmin()
    {
        var isAdmin = User.IsInRole("Admin");
        return Ok(new { isAdmin });
    }
}
