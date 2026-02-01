# Organization Settings Enhancement Plan

## Overview
Add comprehensive organization information fields that can be used throughout the app on reports, forms, and documents.

## New Fields to Add

### Contact Information:
- Organization Name (existing)
- EIN/Tax ID (existing)  
- Street Address
- City
- State (existing, make editable)
- ZIP Code
- Phone Number
- Fax Number (optional)
- Email Address
- Website URL

### Additional Details:
- Mission Statement
- Fiscal Year Start (existing)
- Incorporation Date
- 501(c)(3) Designation Date
- Board President Name
- Executive Director Name
- Contact Person Name
- Contact Person Title

## Files to Modify

1. **Models/OrganizationSettings.cs** (NEW) - Model for organization data
2. **Components/Pages/Settings/SettingsPage.razor** - Update UI with new fields
3. **Services/IOrganizationService.cs** (NEW) - Service interface
4. **Services/OrganizationService.cs** (NEW) - Service implementation
5. **Program.cs** - Register service
6. **Services/PdfExportService.cs** - Use org data in PDF headers/footers
7. **Services/Form990Service.cs** - Use org data in Form 990
8. **Services/PrintableFormsService.cs** - Use org data in printable forms
9. **Components/Pages/Compliance/Form990Page.razor** - Display org info
10. **Services/ReportService.cs** - Include org info in reports

## Implementation Steps

1. Create OrganizationSettings model with all fields
2. Create service to save/load settings (using JSON file or database)
3. Update SettingsPage UI with organized sections
4. Update all services that generate PDFs/forms to include org data
5. Add org info to report headers/footers
6. Test all forms and reports show correct org information

---

Status: Planning
Estimated Time: 2-3 hours
