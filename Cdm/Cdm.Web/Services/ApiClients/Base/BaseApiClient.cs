using System.Net.Http.Json;
using System.Text.Json;
using Cdm.Web.Services.Storage;

namespace Cdm.Web.Services.ApiClients.Base;

public abstract class BaseApiClient
{
    protected readonly HttpClient _httpClient;
    protected readonly ILogger _logger;
    protected readonly ILocalStorageService _localStorage;
    
    protected BaseApiClient(
        HttpClient httpClient,
        ILogger logger,
        ILocalStorageService localStorage)
    {
        _httpClient = httpClient;
        _logger = logger;
        _localStorage = localStorage;
    }
    
    protected async Task AddAuthorizationHeaderAsync()
    {
        var token = await _localStorage.GetItemAsync("auth_token");
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
    }
    
    protected async Task<T?> GetAsync<T>(string endpoint)
    {
        try
        {
            await AddAuthorizationHeaderAsync();
            
            _logger.LogDebug("GET {Endpoint}", endpoint);
            
            var response = await _httpClient.GetAsync(endpoint);
            
            if (!response.IsSuccessStatusCode)
            {
                await HandleErrorResponseAsync(response);
            }
            
            return await response.Content.ReadFromJsonAsync<T>();
        }
        catch (ApiException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error for GET {Endpoint}", endpoint);
            throw new ApiException(500, "Une erreur inattendue s'est produite");
        }
    }
    
    protected async Task<TResponse?> PostAsync<TRequest, TResponse>(
        string endpoint, 
        TRequest data)
    {
        try
        {
            await AddAuthorizationHeaderAsync();
            
            _logger.LogDebug("POST {Endpoint}", endpoint);
            
            var response = await _httpClient.PostAsJsonAsync(endpoint, data);
            
            if (!response.IsSuccessStatusCode)
            {
                await HandleErrorResponseAsync(response);
            }
            
            return await response.Content.ReadFromJsonAsync<TResponse>();
        }
        catch (ApiException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error for POST {Endpoint}", endpoint);
            throw new ApiException(500, "Une erreur inattendue s'est produite");
        }
    }
    
    protected async Task<TResponse?> PutAsync<TRequest, TResponse>(
        string endpoint, 
        TRequest data)
    {
        try
        {
            await AddAuthorizationHeaderAsync();
            
            _logger.LogDebug("PUT {Endpoint}", endpoint);
            
            var response = await _httpClient.PutAsJsonAsync(endpoint, data);
            
            if (!response.IsSuccessStatusCode)
            {
                await HandleErrorResponseAsync(response);
            }
            
            return await response.Content.ReadFromJsonAsync<TResponse>();
        }
        catch (ApiException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error for PUT {Endpoint}", endpoint);
            throw new ApiException(500, "Une erreur inattendue s'est produite");
        }
    }
    
    protected async Task<bool> DeleteAsync(string endpoint)
    {
        try
        {
            await AddAuthorizationHeaderAsync();
            
            _logger.LogDebug("DELETE {Endpoint}", endpoint);
            
            var response = await _httpClient.DeleteAsync(endpoint);
            
            if (!response.IsSuccessStatusCode)
            {
                await HandleErrorResponseAsync(response);
            }
            
            return response.IsSuccessStatusCode;
        }
        catch (ApiException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error for DELETE {Endpoint}", endpoint);
            return false;
        }
    }
    
    private async Task HandleErrorResponseAsync(HttpResponseMessage response)
    {
        var statusCode = (int)response.StatusCode;
        var content = await response.Content.ReadAsStringAsync();
        
        _logger.LogError("API Error {StatusCode}: {Content}", statusCode, content);
        
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            
            var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(content, options);
            
            throw new ApiException(
                statusCode,
                errorResponse?.Error ?? "Une erreur s'est produite",
                errorResponse?.ErrorCode,
                errorResponse?.ValidationErrors
            );
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize error response, raw content: {Content}", content);
            throw new ApiException(statusCode, content);
        }
    }
}

public class ErrorResponse
{
    public string Error { get; set; } = string.Empty;
    public string? ErrorCode { get; set; }
    public Dictionary<string, string[]>? ValidationErrors { get; set; }
}
