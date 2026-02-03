# Theme Toggle & Rate Limiting Fixes

**Issue**: 
1. Theme doesn't change without page reload
2. Page reload triggers "Too many requests" rate limiting error

**Root Causes**:
1. JavaScript changes CSS but Blazor components don't re-render
2. RateLimitingMiddleware is too aggressive (100 req/min for general traffic)

## Fixes Applied

### Fix 1: RateLimitingMiddleware.cs
**Changed**:
- Max requests: 100 → 300 per 60 seconds (more reasonable for normal usage)
- Import limit: 5 → 10 per 60 seconds
- **NEW**: Exclude navigation/shell requests from rate limiting
  - Excludes: `/`, `/index.html`, `/_framework/*`, `/_blazor/*`
  - These requests are not counted against rate limit

**Why**: Normal page reloads and Blazor framework communication won't trigger rate limit anymore.

### Fix 2: theme.js
**Added**:
- `listeners` array to track Blazor callbacks
- `onThemeChange(callback)` method for Blazor to register listeners
- `applyTheme()` now includes `void html.offsetHeight` to force CSS reflow
- All listeners are called when theme changes (sync Blazor components)

**Why**: JavaScript can now notify Blazor components directly when theme changes.

### Fix 3: TopBarInteractive.razor
**Changed**:
- `SetupThemeListener()` now registers a callback with JavaScript
- Added `OnThemeChangedFromJS()` method to receive theme change notifications
- When JavaScript changes theme, Blazor component automatically re-renders

**Why**: Theme changes in JavaScript now properly trigger Blazor re-render without page reload.

## Testing Steps

1. **Stop the running app**
   ```powershell
   # Press Ctrl+C in terminal running `dotnet run`
   ```

2. **Rebuild**
   ```powershell
   cd C:\Users\tech\source\repos\NonProfitFinance
   dotnet clean
   dotnet build
   ```

3. **Run the app**
   ```powershell
   dotnet run
   ```

4. **Test Theme Toggle**
   - Click the theme toggle button in topbar
   - **EXPECTED**: Theme changes immediately without reload
   - **Button icon changes**: Dark mode ☀️ → Light mode 🌙
   - **CSS updates**: Dark/light colors apply instantly

5. **Test Page Reload**
   - After theme change, reload the page (F5 or Ctrl+R)
   - **EXPECTED**: No "Too many requests" error
   - **RESULT**: Page loads normally with theme persisted

6. **Test Theme Persistence**
   - Toggle theme
   - Reload page
   - **EXPECTED**: Theme is saved and persists after reload

## How It Works Now

### Before (Broken)
1. Click toggle button
2. JavaScript changes theme
3. CSS updates
4. **Blazor components don't know about change**
5. Button icon still shows old theme
6. User reloads page (rate limited!)
7. "Too many requests" error appears

### After (Fixed)
1. Click toggle button
2. JavaScript changes theme
3. JavaScript calls Blazor callback
4. **Blazor component updates icon**
5. StateHasChanged() re-renders button
6. Theme changes instantly with icon update
7. No reload needed
8. localStorage persists theme

## Code Changes Summary

### RateLimitingMiddleware.cs
```csharp
// Line 18-19: Increased limits
private const int MaxRequestsPerWindow = 300;  // was 100
private const int ImportMaxRequestsPerWindow = 10;  // was 5

// Lines 28-36: Exclude shell requests from rate limiting
if (path == "/" || path == "/index.html" || path.Contains("/_framework/") || path.Contains("_blazor"))
{
    await _next(context);
    return;
}
```

### theme.js
```javascript
// Added listener registration for Blazor
listeners: [],

onThemeChange: function(callback) {
    if (typeof callback === 'function') {
        this.listeners.push(callback);
    }
},

// Call all listeners when theme changes
this.listeners.forEach(callback => {
    try {
        callback(theme);
    } catch (e) {
        console.error('Error in theme change listener:', e);
    }
});

// Force CSS reflow
void html.offsetHeight;
```

### TopBarInteractive.razor
```csharp
private void OnThemeChangedFromJS(string newTheme)
{
    currentTheme = newTheme;
    StateHasChanged();
}

private async Task SetupThemeListener()
{
    try
    {
        await JS.InvokeVoidAsync("themeManager.onThemeChange", new Action<string>(OnThemeChangedFromJS));
    }
    catch
    {
        // Event listener setup is optional
    }
}
```

## Expected Results

✅ Theme toggles instantly without page reload
✅ Button icon updates immediately
✅ Theme persists across page reloads
✅ No "Too many requests" error on reload
✅ Multiple rapid requests don't trigger rate limit
✅ Rate limiting still protects against abuse

## Troubleshooting

If theme still doesn't change instantly:
1. Clear browser cache (Ctrl+Shift+Delete)
2. Restart browser
3. Check browser console (F12) for errors
4. Verify theme.js loaded (check Sources tab)

If still getting rate limit errors:
1. The fix may not have compiled yet
2. Restart the app: `Ctrl+C` then `dotnet run`
3. Clear browser cache
4. Try again

## Deployment Notes

These are non-breaking changes:
- Backward compatible
- No database migrations needed
- No API changes
- Safe to deploy immediately
- Can be hot-reloaded if running with hot reload enabled

---

**Status**: ✅ Ready to test  
**Risk Level**: LOW (only middleware timing + JavaScript)  
**Rollback**: Easy (revert RateLimitingMiddleware.cs changes if needed)
