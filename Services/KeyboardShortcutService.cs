using Microsoft.JSInterop;

namespace NonProfitFinance.Services;

/// <summary>
/// Service for managing keyboard shortcuts and accessibility features
/// </summary>
public interface IKeyboardShortcutService
{
    /// <summary>
    /// Initialize keyboard shortcuts
    /// </summary>
    Task InitializeAsync();
    
    /// <summary>
    /// Register a callback for a specific shortcut
    /// </summary>
    void RegisterShortcut(string key, Func<Task> callback);
    
    /// <summary>
    /// Handle keyboard event from JavaScript
    /// </summary>
    Task HandleKeyPress(string key, bool ctrl, bool alt, bool shift);
}

public class KeyboardShortcutService : IKeyboardShortcutService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly Dictionary<string, Func<Task>> _shortcuts = new();
    private DotNetObjectReference<KeyboardShortcutService>? _dotNetRef;

    public KeyboardShortcutService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task InitializeAsync()
    {
        _dotNetRef = DotNetObjectReference.Create(this);
        await _jsRuntime.InvokeVoidAsync("keyboardShortcuts.initialize", _dotNetRef);
    }

    public void RegisterShortcut(string key, Func<Task> callback)
    {
        _shortcuts[key.ToLower()] = callback;
    }

    [JSInvokable]
    public async Task HandleKeyPress(string key, bool ctrl, bool alt, bool shift)
    {
        var shortcutKey = BuildShortcutKey(key, ctrl, alt, shift);
        
        if (_shortcuts.TryGetValue(shortcutKey, out var callback))
        {
            await callback();
        }
    }

    private static string BuildShortcutKey(string key, bool ctrl, bool alt, bool shift)
    {
        var parts = new List<string>();
        if (ctrl) parts.Add("ctrl");
        if (alt) parts.Add("alt");
        if (shift) parts.Add("shift");
        parts.Add(key.ToLower());
        return string.Join("+", parts);
    }

    public void Dispose()
    {
        _dotNetRef?.Dispose();
    }
}

