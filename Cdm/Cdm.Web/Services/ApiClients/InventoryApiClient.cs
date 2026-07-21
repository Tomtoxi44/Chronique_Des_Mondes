using Cdm.Business.Abstraction.DTOs;
using Cdm.Web.Services.Storage;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Cdm.Web.Services.ApiClients;

/// <summary>
/// HTTP client for the unified (all game systems) character inventory endpoints.
/// </summary>
public class InventoryApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<InventoryApiClient> _logger;
    private readonly ILocalStorageService _localStorage;

    public InventoryApiClient(HttpClient httpClient, ILogger<InventoryApiClient> logger, ILocalStorageService localStorage)
    {
        _httpClient = httpClient;
        _logger = logger;
        _localStorage = localStorage;
    }

    private async Task AddAuthHeaderAsync()
    {
        var token = await _localStorage.GetItemAsync("auth_token");
        if (!string.IsNullOrEmpty(token))
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<List<InventoryItemDto>> GetForCharacterAsync(int worldCharacterId)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.GetAsync($"api/world-characters/{worldCharacterId}/inventory");
            if (!response.IsSuccessStatusCode) return new();
            return await response.Content.ReadFromJsonAsync<List<InventoryItemDto>>() ?? new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching inventory for world character {WorldCharacterId}", worldCharacterId);
            return new();
        }
    }

    /// <summary>GM read-only view of a player's inventory (GM owns the world).</summary>
    public async Task<List<InventoryItemDto>> GetForCharacterAsGmAsync(int worldCharacterId)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.GetAsync($"api/world-characters/{worldCharacterId}/inventory/gm");
            if (!response.IsSuccessStatusCode) return new();
            return await response.Content.ReadFromJsonAsync<List<InventoryItemDto>>() ?? new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching GM inventory view for world character {WorldCharacterId}", worldCharacterId);
            return new();
        }
    }

    public async Task<InventoryItemDto?> AddAsync(int worldCharacterId, CreateInventoryItemDto dto)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.PostAsJsonAsync($"api/world-characters/{worldCharacterId}/inventory", dto);
            return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<InventoryItemDto>() : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding inventory item to world character {WorldCharacterId}", worldCharacterId);
            return null;
        }
    }

    public async Task<InventoryItemDto?> UpdateAsync(int itemId, CreateInventoryItemDto dto)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.PutAsJsonAsync($"api/inventory/{itemId}", dto);
            return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<InventoryItemDto>() : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating inventory item {ItemId}", itemId);
            return null;
        }
    }

    public async Task<bool> DeleteAsync(int itemId)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.DeleteAsync($"api/inventory/{itemId}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting inventory item {ItemId}", itemId);
            return false;
        }
    }
}
