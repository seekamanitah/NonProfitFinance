using Microsoft.AspNetCore.Identity;

namespace NonProfitFinance.Models;

/// <summary>
/// Custom application user extending IdentityUser with nonprofit-specific properties.
/// </summary>
public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? OrganizationRole { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; } = true;
}
