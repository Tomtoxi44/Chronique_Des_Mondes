namespace Cdm.Web.Services;

/// <summary>
/// Types of toast notifications.
/// </summary>
public enum ToastType
{
    Success,
    Info,
    Warning,
    Error
}

/// <summary>
/// Service for displaying toast notifications. Register as scoped.
/// </summary>
public class ToastService
{
    public event Action<string, ToastType>? OnShow;

    public void Show(string message, ToastType type = ToastType.Success)
        => OnShow?.Invoke(message, type);
}
