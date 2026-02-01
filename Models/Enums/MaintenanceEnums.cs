namespace NonProfitFinance.Models.Enums;

/// <summary>
/// Building maintenance module enumerations
/// </summary>

/// <summary>
/// Type of maintenance project
/// </summary>
public enum ProjectType
{
    Repair,
    Maintenance,
    Upgrade,
    Installation,
    Inspection,
    Emergency,
    Renovation
}

/// <summary>
/// Status of project
/// </summary>
public enum ProjectStatus
{
    Planned,
    Approved,
    InProgress,
    OnHold,
    Completed,
    Cancelled,
    Overdue
}

/// <summary>
/// Priority level
/// </summary>
public enum Priority
{
    Low,
    Medium,
    High,
    Critical,
    Emergency
}

/// <summary>
/// Type of service request
/// </summary>
public enum ServiceRequestType
{
    Repair,
    Maintenance,
    Inspection,
    Installation,
    Removal,
    Cleaning,
    Other
}

/// <summary>
/// Status of service request
/// </summary>
public enum ServiceRequestStatus
{
    Submitted,
    UnderReview,
    Approved,
    Assigned,
    InProgress,
    Completed,
    Rejected,
    Cancelled
}

/// <summary>
/// Frequency of preventive maintenance
/// </summary>
public enum MaintenanceFrequency
{
    Daily,
    Weekly,
    Biweekly,
    Monthly,
    Quarterly,
    Semiannually,
    Annually,
    AsNeeded
}

/// <summary>
/// Type of building/location
/// </summary>
public enum LocationType
{
    Station,
    Facility,
    Building,
    Floor,
    Room,
    Area,
    Equipment
}

/// <summary>
/// Status of work order
/// </summary>
public enum WorkOrderStatus
{
    Created,
    Assigned,
    InProgress,
    Paused,
    Completed,
    Verified,
    Closed
}
