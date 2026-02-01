# AccessibilitySettingsPage Fix

## Issue
```
InvalidOperationException: Object of type 'NonProfitFinance.Components.Shared.PageHeader' 
does not have a property matching the name 'Icon'.
```

## Root Cause
The `AccessibilitySettingsPage.razor` was trying to use a `PageHeader` component with an `Icon` parameter:

```razor
<PageHeader Title="Accessibility Settings" 
            Icon="fa-universal-access"  <!-- This parameter doesn't exist -->
            Subtitle="Configure OCR and Text-to-Speech features" />
```

But the `PageHeader` component only has these parameters:
- `Title` (string)
- `Subtitle` (string, optional)
- `ChildContent` (RenderFragment, optional)

**NO Icon parameter!**

## Fix Applied
Removed the `PageHeader` component usage and replaced with direct HTML that includes the icon:

```razor
<div class="page-header">
    <div class="page-header-left">
        <h1><i class="fas fa-universal-access"></i> Accessibility Settings</h1>
        <p class="text-muted" style="margin: 0.25rem 0 0 0;">
            Configure OCR and Text-to-Speech features
        </p>
    </div>
</div>
```

## Alternative Fix (Not Implemented)
Could also add Icon parameter to PageHeader component:

```csharp
@code {
    [Parameter]
    public string Title { get; set; } = "";
    
    [Parameter]
    public string? Icon { get; set; }  // ADD THIS
    
    [Parameter]
    public string? Subtitle { get; set; }
    
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
```

Then use it in template:
```razor
<h1>
    @if (!string.IsNullOrEmpty(Icon))
    {
        <i class="fas fa-@Icon"></i>
    }
    @Title
</h1>
```

But since only one page needs this, direct HTML is simpler.

## Testing
1. Hot reload or restart app
2. Navigate to `/accessibility-settings`
3. Page should load without error
4. Title should show with accessibility icon

## Build Status
? **Build Successful**

## Files Modified
- `Components/Pages/Settings/AccessibilitySettingsPage.razor`

---

**Issue:** SEC-014 (New)  
**Severity:** Low (Development Error)  
**Status:** FIXED  
**Time to Fix:** 2 minutes
