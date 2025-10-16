using Microsoft.JSInterop;

namespace Cdm.Web.Services.Storage;

public interface ILocalStorageService
{
    Task<string?> GetItemAsync(string key);
    Task SetItemAsync(string key, string value);
    Task RemoveItemAsync(string key);
    Task ClearAsync();
}

public class LocalStorageService : ILocalStorageService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<LocalStorageService> _logger;
    
    public LocalStorageService(
        IJSRuntime jsRuntime,
        ILogger<LocalStorageService> logger)
    {
        _jsRuntime = jsRuntime;
        _logger = logger;
    }
    
    public async Task<string?> GetItemAsync(string key)
    {
        try
        {
            return await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting item from localStorage: {Key}", key);
            return null;
        }
    }
    
    public async Task SetItemAsync(string key, string value)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, value);
            _logger.LogDebug("Item set in localStorage: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting item in localStorage: {Key}", key);
        }
    }
    
    public async Task RemoveItemAsync(string key)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
            _logger.LogDebug("Item removed from localStorage: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing item from localStorage: {Key}", key);
        }
    }
    
    public async Task ClearAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.clear");
            _logger.LogInformation("localStorage cleared");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing localStorage");
        }
    }
}
