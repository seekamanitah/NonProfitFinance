# ?? Quick Reference: Using Organization Settings

## For Developers

### Import the Service:
```csharp
@inject IOrganizationService OrganizationService
```

### Get Settings:
```csharp
var orgSettings = await OrganizationService.GetSettingsAsync();
```

### Use in Code:
```csharp
// Get formatted values
var orgName = orgSettings.OrganizationName;
var address = orgSettings.GetFullAddress();
var phone = orgSettings.GetFormattedPhone();
var ein = orgSettings.GetFormattedEIN();

// Check if configured
var isSetup = await OrganizationService.IsConfiguredAsync();
```

---

## Common Use Cases

### 1. PDF Headers
```csharp
doc.Header().Text(orgSettings.OrganizationName).Bold();
doc.Header().Text(orgSettings.GetFullAddress());
doc.Header().Text($"Phone: {orgSettings.GetFormattedPhone()}");
doc.Header().Text($"Email: {orgSettings.EmailAddress}");
```

### 2. Form 990
```csharp
form.OrganizationName = orgSettings.OrganizationName;
form.EIN = orgSettings.EIN;
form.Address = orgSettings.AddressLine1;
form.City = orgSettings.City;
form.State = orgSettings.State;
form.ZipCode = orgSettings.ZipCode;
```

### 3. Printable Forms
```csharp
var letterhead = $@"
{orgSettings.OrganizationName}
{orgSettings.GetFullAddress()}
Phone: {orgSettings.GetFormattedPhone()}
Email: {orgSettings.EmailAddress}
";
```

### 4. Email Templates
```csharp
var footer = $@"
---
{orgSettings.OrganizationName}
{orgSettings.ContactPersonName}, {orgSettings.ContactPersonTitle}
{orgSettings.PhoneNumber} | {orgSettings.EmailAddress}
{orgSettings.Website}
";
```

### 5. Report Headers
```csharp
var header = new ReportHeader
{
    OrganizationName = orgSettings.OrganizationName,
    EIN = orgSettings.GetFormattedEIN(),
    FiscalYear = GetFiscalYear(orgSettings.FiscalYearStartMonth),
    ContactInfo = $"{orgSettings.PhoneNumber} • {orgSettings.EmailAddress}"
};
```

---

## Available Fields

### Contact:
- `OrganizationName`
- `AddressLine1`, `AddressLine2`
- `City`, `State`, `ZipCode`
- `PhoneNumber`, `FaxNumber`
- `EmailAddress`, `Website`

### Details:
- `EIN`
- `MissionStatement`
- `FiscalYearStartMonth`
- `IncorporationDate`
- `NonProfitDesignationDate`

### Leadership:
- `BoardPresidentName`
- `ExecutiveDirectorName`
- `ContactPersonName`
- `ContactPersonTitle`

### Helper Methods:
- `GetFullAddress()` - Multi-line formatted address
- `GetFormattedPhone()` - (615) 555-1234
- `GetFormattedEIN()` - 12-3456789

---

## Tips

1. **Always check for null/empty** when using optional fields
2. **Use formatted methods** for display (GetFormattedPhone, etc.)
3. **Cache settings** if used multiple times in same operation
4. **Check IsConfiguredAsync()** before generating official documents
5. **Inject at service level**, not individual methods

---

## Example Service Update

```csharp
public class MyService
{
    private readonly IOrganizationService _organizationService;
    
    public MyService(IOrganizationService organizationService)
    {
        _organizationService = organizationService;
    }
    
    public async Task<string> GenerateReport()
    {
        var org = await _organizationService.GetSettingsAsync();
        
        return $@"
# {org.OrganizationName}
EIN: {org.GetFormattedEIN()}

{org.GetFullAddress()}
Phone: {org.GetFormattedPhone()}

## Report Content...
        ";
    }
}
```

---

**See:** `ORGANIZATION_SETTINGS_COMPLETE.md` for full documentation
