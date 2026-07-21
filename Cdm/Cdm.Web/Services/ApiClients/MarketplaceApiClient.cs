using Cdm.Business.Abstraction.DTOs;
using Cdm.Common.Enums;
using Cdm.Web.Services.Storage;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Cdm.Web.Services.ApiClients;

/// <summary>
/// HTTP client for the marketplace endpoints (shared worlds, campaigns and base characters).
/// </summary>
public class MarketplaceApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<MarketplaceApiClient> _logger;
    private readonly ILocalStorageService _localStorage;

    public MarketplaceApiClient(HttpClient httpClient, ILogger<MarketplaceApiClient> logger, ILocalStorageService localStorage)
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

    // ── Browse ───────────────────────────────────────────────────────────

    public Task<List<MarketplaceEntryDto>> GetSharedWorldsAsync(GameType? gameType = null, string? search = null)
        => GetSharedAsync("worlds", gameType, search);

    public Task<List<MarketplaceEntryDto>> GetSharedCampaignsAsync(GameType? gameType = null, string? search = null)
        => GetSharedAsync("campaigns", gameType, search);

    public Task<List<MarketplaceEntryDto>> GetSharedCharactersAsync(string? search = null)
        => GetSharedAsync("characters", null, search);

    private async Task<List<MarketplaceEntryDto>> GetSharedAsync(string kind, GameType? gameType, string? search)
    {
        try
        {
            await AddAuthHeaderAsync();
            var query = new List<string>();
            if (gameType.HasValue) query.Add($"gameType={(int)gameType.Value}");
            if (!string.IsNullOrWhiteSpace(search)) query.Add($"search={Uri.EscapeDataString(search)}");
            var url = $"api/marketplace/{kind}" + (query.Count > 0 ? "?" + string.Join("&", query) : string.Empty);
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return new();
            return await response.Content.ReadFromJsonAsync<List<MarketplaceEntryDto>>() ?? new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching shared {Kind}", kind);
            return new();
        }
    }

    // ── Import ───────────────────────────────────────────────────────────

    public Task<(bool Success, string? Error)> ImportWorldAsync(int id)
        => ImportAsync($"api/marketplace/worlds/{id}/import");

    public Task<(bool Success, string? Error)> ImportCampaignAsync(int id, int targetWorldId)
        => ImportAsync($"api/marketplace/campaigns/{id}/import?targetWorldId={targetWorldId}");

    public Task<(bool Success, string? Error)> ImportCharacterAsync(int id)
        => ImportAsync($"api/marketplace/characters/{id}/import");

    private async Task<(bool Success, string? Error)> ImportAsync(string url)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.PostAsync(url, null);
            if (response.IsSuccessStatusCode) return (true, null);
            var problem = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            return (false, problem?.Error ?? "Import impossible.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing from {Url}", url);
            return (false, "Import impossible.");
        }
    }

    // ── Share toggles ────────────────────────────────────────────────────

    public Task<bool> SetWorldSharedAsync(int id, bool shared)
        => SetSharedAsync($"api/marketplace/worlds/{id}/share", shared);

    public Task<bool> SetCampaignSharedAsync(int id, bool shared)
        => SetSharedAsync($"api/marketplace/campaigns/{id}/share", shared);

    public Task<bool> SetCharacterSharedAsync(int id, bool shared)
        => SetSharedAsync($"api/marketplace/characters/{id}/share", shared);

    private async Task<bool> SetSharedAsync(string url, bool shared)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.PutAsync($"{url}?shared={shared.ToString().ToLowerInvariant()}", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sharing via {Url}", url);
            return false;
        }
    }

    private sealed class ErrorResponse
    {
        public string? Error { get; set; }
    }
}
