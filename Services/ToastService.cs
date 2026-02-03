using Microsoft.AspNetCore.Components;

namespace NonProfitFinance.Services;

/// <summary>
/// Service for displaying toast notifications throughout the application.
/// </summary>
public interface IToastService
{
    event Action<ToastMessage>? OnShow;
    event Action<Guid>? OnRemove;
    
    void ShowSuccess(string message, string? title = null, int duration = 5000);
    void ShowError(string message, string? title = null, int duration = 8000);
    void ShowWarning(string message, string? title = null, int duration = 6000);
    void ShowInfo(string message, string? title = null, int duration = 5000);
    void Show(ToastMessage toast);
    void Remove(Guid id);
    void Clear();
}

public class ToastService : IToastService
{
    public event Action<ToastMessage>? OnShow;
    public event Action<Guid>? OnRemove;
    
    public void ShowSuccess(string message, string? title = null, int duration = 5000)
    {
        Show(new ToastMessage(ToastType.Success, message, title ?? "Success", duration));
    }
    
    public void ShowError(string message, string? title = null, int duration = 8000)
    {
        Show(new ToastMessage(ToastType.Error, message, title ?? "Error", duration));
    }
    
    public void ShowWarning(string message, string? title = null, int duration = 6000)
    {
        Show(new ToastMessage(ToastType.Warning, message, title ?? "Warning", duration));
    }
    
    public void ShowInfo(string message, string? title = null, int duration = 5000)
    {
        Show(new ToastMessage(ToastType.Info, message, title ?? "Info", duration));
    }
    
    public void Show(ToastMessage toast)
    {
        OnShow?.Invoke(toast);
    }
    
    public void Remove(Guid id)
    {
        OnRemove?.Invoke(id);
    }
    
    public void Clear()
    {
        // Signal to clear all toasts
        OnRemove?.Invoke(Guid.Empty);
    }
}

public enum ToastType
{
    Success,
    Error,
    Warning,
    Info
}

public class ToastMessage
{
    public Guid Id { get; } = Guid.NewGuid();
    public ToastType Type { get; }
    public string Message { get; }
    public string Title { get; }
    public int Duration { get; }
    public DateTime CreatedAt { get; } = DateTime.Now;
    
    public ToastMessage(ToastType type, string message, string title, int duration = 5000)
    {
        Type = type;
        Message = message;
        Title = title;
        Duration = duration;
    }
    
    public string Icon => Type switch
    {
        ToastType.Success => "fa-check-circle",
        ToastType.Error => "fa-times-circle",
        ToastType.Warning => "fa-exclamation-triangle",
        ToastType.Info => "fa-info-circle",
        _ => "fa-bell"
    };
    
    public string ColorClass => Type switch
    {
        ToastType.Success => "toast-success",
        ToastType.Error => "toast-error",
        ToastType.Warning => "toast-warning",
        ToastType.Info => "toast-info",
        _ => "toast-info"
    };
}
