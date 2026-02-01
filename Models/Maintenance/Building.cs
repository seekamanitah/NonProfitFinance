using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NonProfitFinance.Models.Enums;

namespace NonProfitFinance.Models.Maintenance;

/// <summary>
/// Building, facility, or location (hierarchical)
/// </summary>
public class Building
{
    [Key]
    public int Id { get; set; }
    
    /// <summary>
    /// Building/Location name
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Description
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Location type
    /// </summary>
    [Required]
    public LocationType Type { get; set; } = LocationType.Building;
    
    /// <summary>
    /// Building code
    /// </summary>
    [MaxLength(50)]
    public string? Code { get; set; }
    
    /// <summary>
    /// Parent building/location ID (for hierarchy)
    /// </summary>
    public int? ParentBuildingId { get; set; }
    
    /// <summary>
    /// Navigation property to parent building
    /// </summary>
    [ForeignKey(nameof(ParentBuildingId))]
    public Building? ParentBuilding { get; set; }
    
    /// <summary>
    /// Display order
    /// </summary>
    public int DisplayOrder { get; set; }
    
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
    /// Year built
    /// </summary>
    public int? YearBuilt { get; set; }
    
    /// <summary>
    /// Square footage
    /// </summary>
    public decimal? SquareFootage { get; set; }
    
    /// <summary>
    /// Primary contact person
    /// </summary>
    [MaxLength(200)]
    public string? ContactPerson { get; set; }
    
    /// <summary>
    /// Contact phone
    /// </summary>
    [MaxLength(20)]
    public string? ContactPhone { get; set; }
    
    /// <summary>
    /// Photo URL
    /// </summary>
    [MaxLength(500)]
    public string? PhotoUrl { get; set; }
    
    /// <summary>
    /// Notes
    /// </summary>
    [MaxLength(2000)]
    public string? Notes { get; set; }
    
    /// <summary>
    /// Is this location active?
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Is this demo data?
    /// </summary>
    public bool IsDemo { get; set; } = false;
    
    /// <summary>
    /// Created date
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Navigation: Child buildings/areas
    /// </summary>
    public ICollection<Building> ChildBuildings { get; set; } = new List<Building>();
    
    /// <summary>
    /// Navigation: Projects at this location
    /// </summary>
    public ICollection<Project> Projects { get; set; } = new List<Project>();
    
    /// <summary>
    /// Navigation: Service requests at this location
    /// </summary>
    public ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
    
    /// <summary>
    /// Navigation: Work orders at this location
    /// </summary>
    public ICollection<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();
    
    /// <summary>
    /// Get full location path
    /// </summary>
    [NotMapped]
    public string FullPath
    {
        get
        {
            var path = Name;
            var parent = ParentBuilding;
            while (parent != null)
            {
                path = $"{parent.Name} > {path}";
                parent = parent.ParentBuilding;
            }
            return path;
        }
    }
    
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
