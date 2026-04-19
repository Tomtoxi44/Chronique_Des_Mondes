using Cdm.Business.Abstraction.DTOs;
using Cdm.Web.Services.Storage;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Cdm.Web.Services.ApiClients;

/// <summary>
/// HTTP client for session-related API calls.
/// </summary>
public class SessionApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SessionApiClient> _logger;
    private readonly ILocalStorageService _localStorage;

    public SessionApiClient(HttpClient httpClient, ILogger<SessionApiClient> logger, ILocalStorageService localStorage)
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

    /// <summary>Start a new session.</summary>
    public async Task<SessionDto?> StartSessionAsync(StartSessionDto dto)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.PostAsJsonAsync("api/sessions", dto);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<SessionDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting session");
            return null;
        }
    }

    /// <summary>Get a session by ID.</summary>
    public async Task<SessionDto?> GetSessionAsync(int sessionId)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.GetAsync($"api/sessions/{sessionId}");
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<SessionDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching session {SessionId}", sessionId);
            return null;
        }
    }

    /// <summary>Get the active session for a campaign.</summary>
    public async Task<SessionDto?> GetActiveSessionByCampaignAsync(int campaignId)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.GetAsync($"api/sessions/campaign/{campaignId}/active");
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<SessionDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching active session for campaign {CampaignId}", campaignId);
            return null;
        }
    }

    /// <summary>Get all sessions the user has been part of.</summary>
    public async Task<List<SessionDto>> GetMySessionsAsync()
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.GetAsync("api/sessions");
            if (!response.IsSuccessStatusCode) return new List<SessionDto>();
            return await response.Content.ReadFromJsonAsync<List<SessionDto>>() ?? new List<SessionDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching sessions");
            return new List<SessionDto>();
        }
    }

    /// <summary>End a session.</summary>
    public async Task<bool> EndSessionAsync(int sessionId)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.PutAsync($"api/sessions/{sessionId}/end", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ending session {SessionId}", sessionId);
            return false;
        }
    }

    /// <summary>Join a session as a player.</summary>
    public async Task<SessionDto?> JoinSessionAsync(int sessionId, int worldCharacterId)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.PostAsJsonAsync($"api/sessions/{sessionId}/join", new { WorldCharacterId = worldCharacterId });
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<SessionDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining session {SessionId}", sessionId);
            return null;
        }
    }

    /// <summary>Update the current chapter being played.</summary>
    public async Task<SessionDto?> UpdateCurrentChapterAsync(int sessionId, int? chapterId)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.PutAsJsonAsync($"api/sessions/{sessionId}/chapter", new { ChapterId = chapterId });
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<SessionDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating chapter for session {SessionId}", sessionId);
            return null;
        }
    }
}
