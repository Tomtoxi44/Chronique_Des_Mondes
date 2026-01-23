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
    private readonly IJSRuntime jsRuntime;
    private readonly ILogger<LocalStorageService> logger;
    
    public LocalStorageService(
        IJSRuntime jsRuntime,
        ILogger<LocalStorageService> logger)
    {
        this.jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    public async Task<string?> GetItemAsync(string key)
    {
        try
        {
            return await this.jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("JavaScript interop calls cannot be issued"))
        {
            // JS Interop not available during pre-rendering
            this.logger.LogDebug("JavaScript interop not available for localStorage.getItem during pre-rendering");
            return null;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error getting item from localStorage: {Key}", key);
            return null;
        }
    }
    
    public async Task SetItemAsync(string key, string value)
    {
        try
        {
            await this.jsRuntime.InvokeVoidAsync("localStorage.setItem", key, value);
            this.logger.LogDebug("Item set in localStorage: {Key}", key);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("JavaScript interop calls cannot be issued"))
        {
            // JS Interop not available during pre-rendering
            this.logger.LogDebug("JavaScript interop not available for localStorage.setItem during pre-rendering");
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error setting item in localStorage: {Key}", key);
        }
    }
    
    public async Task RemoveItemAsync(string key)
    {
        try
        {
            await this.jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
            this.logger.LogDebug("Item removed from localStorage: {Key}", key);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("JavaScript interop calls cannot be issued"))
        {
            // JS Interop not available during pre-rendering
            this.logger.LogDebug("JavaScript interop not available for localStorage.removeItem during pre-rendering");
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error removing item from localStorage: {Key}", key);
        }
    }
    
    public async Task ClearAsync()
    {
        try
        {
            await this.jsRuntime.InvokeVoidAsync("localStorage.clear");
            this.logger.LogInformation("localStorage cleared");
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error clearing localStorage");
        }
    }
}
