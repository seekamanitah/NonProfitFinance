namespace NonProfitFinance.Models;

/// <summary>
/// Organization settings and information used throughout the application
/// </summary>
public class OrganizationSettings
{
    /// <summary>
    /// Legal organization name
    /// </summary>
    public string OrganizationName { get; set; } = string.Empty;
    
    /// <summary>
    /// Employer Identification Number (EIN) / Tax ID
    /// </summary>
    public string EIN { get; set; } = string.Empty;
    
    /// <summary>
    /// Street address line 1
    /// </summary>
    public string AddressLine1 { get; set; } = string.Empty;
    
    /// <summary>
    /// Street address line 2 (suite, unit, etc.)
    /// </summary>
    public string? AddressLine2 { get; set; }
    
    /// <summary>
    /// City
    /// </summary>
    public string City { get; set; } = string.Empty;
    
    /// <summary>
    /// State/Province
    /// </summary>
    public string State { get; set; } = "Tennessee";
    
    /// <summary>
    /// ZIP/Postal Code
    /// </summary>
    public string ZipCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Primary phone number
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Fax number (optional)
    /// </summary>
    public string? FaxNumber { get; set; }
    
    /// <summary>
    /// Primary email address
    /// </summary>
    public string EmailAddress { get; set; } = string.Empty;
    
    /// <summary>
    /// Website URL
    /// </summary>
    public string? Website { get; set; }
    
    /// <summary>
    /// Organization mission statement
    /// </summary>
    public string? MissionStatement { get; set; }
    
    /// <summary>
    /// Fiscal year start month (1-12)
    /// </summary>
    public int FiscalYearStartMonth { get; set; } = 1;
    
    /// <summary>
    /// Date of incorporation
    /// </summary>
    public DateTime? IncorporationDate { get; set; }
    
    /// <summary>
    /// Date of 501(c)(3) designation
    /// </summary>
    public DateTime? NonProfitDesignationDate { get; set; }
    
    /// <summary>
    /// Name of board president/chairman
    /// </summary>
    public string? BoardPresidentName { get; set; }
    
    /// <summary>
    /// Name of executive director/CEO
    /// </summary>
    public string? ExecutiveDirectorName { get; set; }
    
    /// <summary>
    /// Primary contact person name
    /// </summary>
    public string? ContactPersonName { get; set; }
    
    /// <summary>
    /// Primary contact person title
    /// </summary>
    public string? ContactPersonTitle { get; set; }
    
    /// <summary>
    /// Get formatted full address
    /// </summary>
    public string GetFullAddress()
    {
        var parts = new List<string>();
        
        if (!string.IsNullOrWhiteSpace(AddressLine1))
            parts.Add(AddressLine1);
        
        if (!string.IsNullOrWhiteSpace(AddressLine2))
            parts.Add(AddressLine2);
        
        var cityStateZip = new List<string>();
        if (!string.IsNullOrWhiteSpace(City))
            cityStateZip.Add(City);
        if (!string.IsNullOrWhiteSpace(State))
            cityStateZip.Add(State);
        if (!string.IsNullOrWhiteSpace(ZipCode))
            cityStateZip.Add(ZipCode);
        
        if (cityStateZip.Any())
            parts.Add(string.Join(", ", cityStateZip.Take(2)) + 
                     (cityStateZip.Count > 2 ? " " + cityStateZip.Last() : ""));
        
        return string.Join("\n", parts);
    }
    
    /// <summary>
    /// Get formatted phone number for display
    /// </summary>
    public string GetFormattedPhone()
    {
        if (string.IsNullOrWhiteSpace(PhoneNumber))
            return string.Empty;
        
        // Remove non-numeric characters
        var digits = new string(PhoneNumber.Where(char.IsDigit).ToArray());
        
        if (digits.Length == 10)
            return $"({digits.Substring(0, 3)}) {digits.Substring(3, 3)}-{digits.Substring(6)}";
        
        return PhoneNumber; // Return as-is if not 10 digits
    }
    
    /// <summary>
    /// Get formatted EIN for display
    /// </summary>
    public string GetFormattedEIN()
    {
        if (string.IsNullOrWhiteSpace(EIN))
            return string.Empty;
        
        // Remove non-numeric characters
        var digits = new string(EIN.Where(char.IsDigit).ToArray());
        
        if (digits.Length == 9)
            return $"{digits.Substring(0, 2)}-{digits.Substring(2)}";
        
        return EIN; // Return as-is if not 9 digits
    }

    /// <summary>
    /// Get the start date of the current fiscal year
    /// </summary>
    public DateTime GetCurrentFiscalYearStart()
    {
        var now = DateTime.UtcNow;
        var fiscalYearStart = new DateTime(now.Year, FiscalYearStartMonth, 1);
        
        // If we haven't reached the fiscal year start month yet, use previous year
        if (now < fiscalYearStart)
            fiscalYearStart = fiscalYearStart.AddYears(-1);
        
        return fiscalYearStart;
    }

    /// <summary>
    /// Get the end date of the current fiscal year
    /// </summary>
    public DateTime GetCurrentFiscalYearEnd()
    {
        return GetCurrentFiscalYearStart().AddYears(1).AddDays(-1);
    }

    /// <summary>
    /// Get the fiscal year number for a given date
    /// </summary>
    public int GetFiscalYear(DateTime date)
    {
        // If fiscal year starts in Jan, use calendar year
        if (FiscalYearStartMonth == 1)
            return date.Year;
        
        // Otherwise, fiscal year is based on which year the start falls in
        // e.g., July 2024 to June 2025 would be FY 2025
        return date.Month >= FiscalYearStartMonth ? date.Year + 1 : date.Year;
    }

    /// <summary>
    /// Get fiscal year label (e.g., "FY 2024" or "FY 2024-25")
    /// </summary>
    public string GetFiscalYearLabel(DateTime date)
    {
        var fiscalYear = GetFiscalYear(date);
        
        if (FiscalYearStartMonth == 1)
            return $"FY {fiscalYear}";
        
        return $"FY {fiscalYear - 1}-{fiscalYear % 100:D2}";
    }
}
