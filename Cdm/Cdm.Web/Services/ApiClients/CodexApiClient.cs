using Cdm.Business.Abstraction.DTOs;
using Cdm.Common.Enums;
using Cdm.Web.Services.Storage;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Cdm.Web.Services.ApiClients;

/// <summary>
/// HTTP client for the codex endpoints.
/// </summary>
public class CodexApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CodexApiClient> _logger;
    private readonly ILocalStorageService _localStorage;

    public CodexApiClient(HttpClient httpClient, ILogger<CodexApiClient> logger, ILocalStorageService localStorage)
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

    public async Task<List<CodexItemDto>> GetMyItemsAsync(GameType? gameType = null)
    {
        try
        {
            await AddAuthHeaderAsync();
            var url = gameType.HasValue ? $"api/codex?gameType={(int)gameType.Value}" : "api/codex";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return new();
            return await response.Content.ReadFromJsonAsync<List<CodexItemDto>>() ?? new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching codex items");
            return new();
        }
    }

    public async Task<CodexItemDto?> CreateAsync(CreateCodexItemDto dto)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.PostAsJsonAsync("api/codex", dto);
            return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<CodexItemDto>() : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating codex item");
            return null;
        }
    }

    public async Task<CodexItemDto?> UpdateAsync(int id, CreateCodexItemDto dto)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.PutAsJsonAsync($"api/codex/{id}", dto);
            return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<CodexItemDto>() : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating codex item {Id}", id);
            return null;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.DeleteAsync($"api/codex/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting codex item {Id}", id);
            return false;
        }
    }

    public async Task<bool> SetSharedAsync(int id, bool shared)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.PutAsync($"api/codex/{id}/share?shared={shared.ToString().ToLowerInvariant()}", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sharing codex item {Id}", id);
            return false;
        }
    }

    public async Task<List<CodexItemDto>> GetMarketplaceItemsAsync(GameType? gameType = null, string? search = null)
    {
        try
        {
            await AddAuthHeaderAsync();
            var query = new List<string>();
            if (gameType.HasValue) query.Add($"gameType={(int)gameType.Value}");
            if (!string.IsNullOrWhiteSpace(search)) query.Add($"search={Uri.EscapeDataString(search)}");
            var url = "api/marketplace/items" + (query.Count > 0 ? "?" + string.Join("&", query) : string.Empty);
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return new();
            return await response.Content.ReadFromJsonAsync<List<CodexItemDto>>() ?? new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching marketplace items");
            return new();
        }
    }

    public async Task<CodexItemDto?> ImportItemAsync(int id)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.PostAsync($"api/marketplace/items/{id}/import", null);
            return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<CodexItemDto>() : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing marketplace item {Id}", id);
            return null;
        }
    }

    /// <summary>
    /// Lists the user's world characters compatible with an item: a specific game type,
    /// or all characters when <paramref name="gameType"/> is null (generic items go anywhere).
    /// </summary>
    public async Task<List<WorldCharacterDto>> GetCompatibleCharactersAsync(GameType? gameType)
    {
        try
        {
            await AddAuthHeaderAsync();
            var url = gameType.HasValue
                ? $"api/characters/world-characters?gameType={(int)gameType.Value}"
                : "api/characters/world-characters";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return new();
            return await response.Content.ReadFromJsonAsync<List<WorldCharacterDto>>() ?? new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching compatible characters");
            return new();
        }
    }

    public async Task<(bool Success, string? Error)> AddToCharacterAsync(int codexItemId, int worldCharacterId)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.PostAsync($"api/codex/{codexItemId}/add-to-character/{worldCharacterId}", null);
            if (response.IsSuccessStatusCode) return (true, null);
            var problem = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            return (false, problem?.Error ?? "Ajout impossible.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding codex item {Id} to character {WcId}", codexItemId, worldCharacterId);
            return (false, "Ajout impossible.");
        }
    }

    private sealed class ErrorResponse
    {
        public string? Error { get; set; }
    }
}
