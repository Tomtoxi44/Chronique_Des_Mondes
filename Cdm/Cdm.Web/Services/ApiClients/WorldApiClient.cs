using Cdm.Business.Abstraction.DTOs;
using Cdm.Common.Enums;
using System.Net.Http.Json;

namespace Cdm.Web.Services.ApiClients;

public class WorldApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WorldApiClient> _logger;

    public WorldApiClient(HttpClient httpClient, ILogger<WorldApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Get all worlds owned by the authenticated user
    /// </summary>
    public async Task<List<WorldDto>> GetMyWorldsAsync()
    {
        try
        {
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

    /// <summary>
    /// Create a new world
    /// </summary>
    public async Task<WorldDto?> CreateWorldAsync(CreateWorldRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/worlds", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<WorldDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating world");
            return null;
        }
    }

    /// <summary>
    /// Update an existing world
    /// </summary>
    public async Task<WorldDto?> UpdateWorldAsync(int id, UpdateWorldRequest request)
    {
        try
        {
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
    /// Upload an image for a world
    /// </summary>
    public async Task<string?> UploadWorldImageAsync(int id, MultipartFormDataContent content)
    {
        try
        {
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
public record UpdateWorldRequest(string Name, string Description, bool IsActive);
public record UploadImageResponse(string ImageUrl);
