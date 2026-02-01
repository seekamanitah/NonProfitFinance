using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NonProfitFinance.Models.Maintenance;

/// <summary>
/// External contractor or vendor for maintenance work
/// </summary>
public class Contractor
{
    [Key]
    public int Id { get; set; }
    
    /// <summary>
    /// Company/Contractor name
    /// </summary>
    [Required]
    [MaxLength(300)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Contact person name
    /// </summary>
    [MaxLength(200)]
    public string? ContactName { get; set; }
    
    /// <summary>
    /// Primary phone
    /// </summary>
    [MaxLength(20)]
    public string? Phone { get; set; }
    
    /// <summary>
    /// Secondary phone
    /// </summary>
    [MaxLength(20)]
    public string? Phone2 { get; set; }
    
    /// <summary>
    /// Email address
    /// </summary>
    [MaxLength(200)]
    public string? Email { get; set; }
    
    /// <summary>
    /// Website
    /// </summary>
    [MaxLength(500)]
    public string? Website { get; set; }
    
    /// <summary>
    /// Street address
    /// </summary>
    [MaxLength(500)]
    public string? Address { get; set; }
    
    /// <summary>
    /// City
    /// </summary>
    [MaxLength(100)]
    public string? City { get; set; }
    
    /// <summary>
    /// State
    /// </summary>
    [MaxLength(50)]
    public string? State { get; set; }
    
    /// <summary>
    /// ZIP code
    /// </summary>
    [MaxLength(20)]
    public string? ZipCode { get; set; }
    
    /// <summary>
    /// Services provided (comma-separated or description)
    /// </summary>
    [MaxLength(1000)]
    public string? ServicesProvided { get; set; }
    
    /// <summary>
    /// License number
    /// </summary>
    [MaxLength(100)]
    public string? LicenseNumber { get; set; }
    
    /// <summary>
    /// Insurance policy number
    /// </summary>
    [MaxLength(100)]
    public string? InsurancePolicy { get; set; }
    
    /// <summary>
    /// Insurance expiration date
    /// </summary>
    public DateTime? InsuranceExpiration { get; set; }
    
    /// <summary>
    /// Hourly rate
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? HourlyRate { get; set; }
    
    /// <summary>
    /// Rating (1-5 stars)
    /// </summary>
    public int? Rating { get; set; }
    
    /// <summary>
    /// Notes
    /// </summary>
    [MaxLength(2000)]
    public string? Notes { get; set; }
    
    /// <summary>
    /// Is this contractor currently active/approved?
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Is this a preferred contractor?
    /// </summary>
    public bool IsPreferred { get; set; } = false;
    
    /// <summary>
    /// Is this demo data?
    /// </summary>
    public bool IsDemo { get; set; } = false;
    
    /// <summary>
    /// Created date
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Last updated date
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// Navigation: Projects assigned
    /// </summary>
    public ICollection<Project> Projects { get; set; } = new List<Project>();
    
    /// <summary>
    /// Navigation: Work orders assigned
    /// </summary>
    public ICollection<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();
    
    /// <summary>
    /// Is insurance expired?
    /// </summary>
    [NotMapped]
    public bool IsInsuranceExpired => InsuranceExpiration.HasValue && 
                                      InsuranceExpiration.Value < DateTime.Today;
    
    /// <summary>
    /// Get formatted address
    /// </summary>
    [NotMapped]
    public string? FormattedAddress
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Address)) return null;
            var parts = new List<string> { Address };
            if (!string.IsNullOrWhiteSpace(City)) parts.Add(City);
            if (!string.IsNullOrWhiteSpace(State)) parts.Add(State);
            if (!string.IsNullOrWhiteSpace(ZipCode)) parts.Add(ZipCode);
            return string.Join(", ", parts);
        }
    }
}
