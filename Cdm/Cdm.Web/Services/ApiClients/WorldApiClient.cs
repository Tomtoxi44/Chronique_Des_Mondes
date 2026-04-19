using Cdm.Business.Abstraction.DTOs;
using Cdm.Common.Enums;
using Cdm.Web.Services.ApiClients.Base;
using Cdm.Web.Services.Storage;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Cdm.Web.Services.ApiClients;

public class WorldApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WorldApiClient> _logger;
    private readonly ILocalStorageService _localStorage;

    public WorldApiClient(HttpClient httpClient, ILogger<WorldApiClient> logger, ILocalStorageService localStorage)
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

    /// <summary>
    /// Get all worlds owned by the authenticated user
    /// </summary>
    public async Task<List<WorldDto>> GetMyWorldsAsync()
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.GetAsync("api/worlds");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<WorldDto>>() ?? new List<WorldDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching my worlds");
            return new List<WorldDto>();
        }
    }

    /// <summary>
    /// Get all worlds where the user participates (GM + player)
    /// </summary>
    public async Task<List<WorldDto>> GetAllMyWorldsAsync()
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.GetAsync("api/worlds/all");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<WorldDto>>() ?? new List<WorldDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all my worlds");
            return new List<WorldDto>();
        }
    }

    /// <summary>
    /// Get a world by ID
    /// </summary>
    public async Task<WorldDto?> GetWorldByIdAsync(int id)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.GetAsync($"api/worlds/{id}");
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("World {WorldId} not found or access denied", id);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<WorldDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching world {WorldId}", id);
            return null;
        }
    }

    /// <summary>
    /// Get characters assigned to a world
    /// </summary>
    public async Task<List<object>> GetWorldCharactersAsync(int worldId)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.GetAsync($"api/worlds/{worldId}/characters");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<object>>() ?? new List<object>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching characters for world {WorldId}", worldId);
            return new List<object>();
        }
    }

    private async Task<ApiException> ReadApiErrorAsync(HttpResponseMessage response)
    {
        var statusCode = (int)response.StatusCode;
        var content = await response.Content.ReadAsStringAsync();
        try
        {
            var err = JsonSerializer.Deserialize<ErrorResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return new ApiException(statusCode, err?.Error ?? content);
        }
        catch
        {
            return new ApiException(statusCode, string.IsNullOrWhiteSpace(content) ? "Erreur serveur" : content);
        }
    }

    /// <summary>
    /// Create a new world
    /// </summary>
    public async Task<WorldDto?> CreateWorldAsync(CreateWorldRequest request)
    {
        await AddAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync("api/worlds", request);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Error creating world: {StatusCode}", response.StatusCode);
            throw await ReadApiErrorAsync(response);
        }
        return await response.Content.ReadFromJsonAsync<WorldDto>();
    }

    /// <summary>
    /// Update an existing world
    /// </summary>
    public async Task<WorldDto?> UpdateWorldAsync(int id, UpdateWorldRequest request)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.PutAsJsonAsync($"api/worlds/{id}", request);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to update world {WorldId}", id);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<WorldDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating world {WorldId}", id);
            return null;
        }
    }

    /// <summary>
    /// Delete a world (soft delete)
    /// </summary>
    public async Task<bool> DeleteWorldAsync(int id)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.DeleteAsync($"api/worlds/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting world {WorldId}", id);
            return false;
        }
    }

    /// <summary>
    /// Get characters assigned to a world (typed)
    /// </summary>
    public async Task<List<WorldCharacterDto>> GetWorldCharactersTypedAsync(int worldId)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.GetAsync($"api/worlds/{worldId}/characters");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<WorldCharacterDto>>() ?? new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching characters for world {WorldId}", worldId);
            return new();
        }
    }

    /// <summary>
    /// Generate (or refresh) the invite token for a world.
    /// </summary>
    public async Task<string?> GenerateWorldInviteTokenAsync(int worldId)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.PostAsync($"api/worlds/{worldId}/invite", null);
            if (!response.IsSuccessStatusCode) return null;
            var result = await response.Content.ReadFromJsonAsync<InviteTokenResponse>();
            return result?.InviteToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating invite token for world {WorldId}", worldId);
            return null;
        }
    }

    /// <summary>
    /// Get world info by invite token (public, no auth required).
    /// </summary>
    public async Task<WorldDto?> GetWorldByInviteTokenAsync(string token)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/worlds/join/{token}");
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<WorldDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting world by invite token");
            return null;
        }
    }

    /// <summary>
    /// Join a world with a character using an invite token.
    /// </summary>
    public async Task<WorldCharacterDto?> JoinWorldAsync(string token, int characterId)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.PostAsJsonAsync("api/worlds/join", new { InviteToken = token, CharacterId = characterId });
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<WorldCharacterDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining world");
            return null;
        }
    }

    /// <summary>
    /// Remove a character from a world.
    /// </summary>
    public async Task<bool> RemoveCharacterFromWorldAsync(int worldId, int characterId)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.DeleteAsync($"api/worlds/{worldId}/characters/{characterId}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing character {CharacterId} from world {WorldId}", characterId, worldId);
            return false;
        }
    }

    /// <summary>
    /// Get the current user's world character for a specific world.
    /// </summary>
    public async Task<WorldCharacterDto?> GetMyWorldCharacterAsync(int worldId)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.GetAsync($"api/worlds/{worldId}/my-character");
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<WorldCharacterDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching world character for world {WorldId}", worldId);
            return null;
        }
    }

    /// <summary>
    /// Update the current user's world character profile.
    /// </summary>
    public async Task<WorldCharacterDto?> UpdateMyWorldCharacterAsync(int worldId, UpdateWorldCharacterProfileRequest request)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.PutAsJsonAsync($"api/worlds/{worldId}/my-character", request);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<WorldCharacterDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating world character for world {WorldId}", worldId);
            return null;
        }
    }

    /// <summary>
    /// Get campaigns for a world (accessible by GM and members).
    /// </summary>
    public async Task<List<CampaignDto>> GetWorldCampaignsAsync(int worldId)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.GetAsync($"api/worlds/{worldId}/campaigns");
            if (!response.IsSuccessStatusCode) return new List<CampaignDto>();
            return await response.Content.ReadFromJsonAsync<List<CampaignDto>>() ?? new List<CampaignDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching campaigns for world {WorldId}", worldId);
            return new List<CampaignDto>();
        }
    }

    /// <summary>
    /// Upload an image for a world
    /// </summary>
    public async Task<string?> UploadWorldImageAsync(int id, MultipartFormDataContent content)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.PostAsync($"api/worlds/{id}/upload-image", content);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<UploadImageResponse>();
            return result?.ImageUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image for world {WorldId}", id);
            return null;
        }
    }
}

// Request DTOs (to match backend)
public record CreateWorldRequest(string Name, string Description, GameType GameType);
public record UpdateWorldRequest(string Name, string Description, bool IsActive, GameType GameType = GameType.Generic);
public record UploadImageResponse(string ImageUrl);
public record InviteTokenResponse(string InviteToken);
public record UpdateWorldCharacterProfileRequest(int? Level, int? CurrentHealth, int? MaxHealth, string? GameSpecificData);
file record ErrorResponse(string? Error);

