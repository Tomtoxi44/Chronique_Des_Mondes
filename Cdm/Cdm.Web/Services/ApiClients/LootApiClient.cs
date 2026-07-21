using Cdm.Business.Abstraction.DTOs;
using Cdm.Web.Services.Storage;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Cdm.Web.Services.ApiClients;

/// <summary>
/// HTTP client for campaign loot endpoints (GM preparation + out-of-session distribution).
/// In-session distribution goes through the SignalR hub, not this client.
/// </summary>
public class LootApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<LootApiClient> _logger;
    private readonly ILocalStorageService _localStorage;

    public LootApiClient(HttpClient httpClient, ILogger<LootApiClient> logger, ILocalStorageService localStorage)
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

    public async Task<List<CampaignLootDto>> GetCampaignLootAsync(int campaignId)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.GetAsync($"api/campaigns/{campaignId}/loot");
            if (!response.IsSuccessStatusCode) return new();
            return await response.Content.ReadFromJsonAsync<List<CampaignLootDto>>() ?? new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching loot for campaign {CampaignId}", campaignId);
            return new();
        }
    }

    public async Task<CampaignLootDto?> CreateAsync(int campaignId, CreateLootDto dto)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.PostAsJsonAsync($"api/campaigns/{campaignId}/loot", dto);
            return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<CampaignLootDto>() : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating loot for campaign {CampaignId}", campaignId);
            return null;
        }
    }

    public async Task<CampaignLootDto?> UpdateAsync(int lootId, CreateLootDto dto)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.PutAsJsonAsync($"api/loot/{lootId}", dto);
            return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<CampaignLootDto>() : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating loot {LootId}", lootId);
            return null;
        }
    }

    public async Task<bool> DeleteAsync(int lootId)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.DeleteAsync($"api/loot/{lootId}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting loot {LootId}", lootId);
            return false;
        }
    }

    public async Task<(bool Success, string? Error)> DistributeAsync(int lootId, int worldCharacterId, int? sessionId = null)
    {
        try
        {
            await AddAuthHeaderAsync();
            var url = $"api/loot/{lootId}/distribute?worldCharacterId={worldCharacterId}";
            if (sessionId.HasValue) url += $"&sessionId={sessionId.Value}";
            var response = await _httpClient.PostAsync(url, null);
            if (response.IsSuccessStatusCode) return (true, null);
            var problem = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            return (false, problem?.Error ?? "Distribution impossible.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error distributing loot {LootId}", lootId);
            return (false, "Distribution impossible.");
        }
    }

    private sealed class ErrorResponse
    {
        public string? Error { get; set; }
    }
}
