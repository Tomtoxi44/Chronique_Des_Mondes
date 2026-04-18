using Cdm.Business.Abstraction.DTOs;
using Cdm.Common.Enums;
using Cdm.Web.Services.Storage;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Cdm.Web.Services.ApiClients;

public class CampaignApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CampaignApiClient> _logger;
    private readonly ILocalStorageService _localStorage;

    public CampaignApiClient(HttpClient httpClient, ILogger<CampaignApiClient> logger, ILocalStorageService localStorage)
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
    /// Get all campaigns for the authenticated user
    /// </summary>
    public async Task<List<CampaignDto>> GetMyCampaignsAsync()
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.GetAsync("api/campaigns/my");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<CampaignDto>>() ?? new List<CampaignDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching my campaigns");
            return new List<CampaignDto>();
        }
    }

    /// <summary>
    /// Get a campaign by ID
    /// </summary>
    public async Task<CampaignDto?> GetCampaignByIdAsync(int id)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.GetAsync($"api/campaigns/{id}");
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Campaign {CampaignId} not found or access denied", id);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<CampaignDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching campaign {CampaignId}", id);
            return null;
        }
    }

    /// <summary>
    /// Update an existing campaign
    /// </summary>
    public async Task<CampaignDto?> UpdateCampaignAsync(int id, UpdateCampaignRequest request)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.PutAsJsonAsync($"api/campaigns/{id}", request);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to update campaign {CampaignId}", id);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<CampaignDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating campaign {CampaignId}", id);
            return null;
        }
    }

    /// <summary>
    /// Delete a campaign
    /// </summary>
    public async Task<bool> DeleteCampaignAsync(int id)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.DeleteAsync($"api/campaigns/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting campaign {CampaignId}", id);
            return false;
        }
    }

    /// <summary>
    /// Create a new campaign
    /// </summary>
    public async Task<CampaignDto?> CreateCampaignAsync(CreateCampaignDto dto)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.PostAsJsonAsync("api/campaigns", dto);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to create campaign. Status: {StatusCode}", response.StatusCode);
                return null;
            }
            return await response.Content.ReadFromJsonAsync<CampaignDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating campaign");
            return null;
        }
    }

    /// <summary>
    /// Generate (or regenerate) an invite token for a campaign
    /// </summary>
    public async Task<string?> GenerateInviteTokenAsync(int campaignId)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.PostAsync($"api/campaigns/{campaignId}/invite", null);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to generate invite token for campaign {CampaignId}", campaignId);
                return null;
            }
            var result = await response.Content.ReadFromJsonAsync<InviteTokenResponse>();
            return result?.InviteToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating invite token for campaign {CampaignId}", campaignId);
            return null;
        }
    }

    /// <summary>
    /// Join a campaign using an invite token
    /// </summary>
    public async Task<CampaignDto?> JoinCampaignAsync(string inviteToken)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.PostAsJsonAsync("api/campaigns/join", new { InviteToken = inviteToken });
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to join campaign with token. Status: {StatusCode}", response.StatusCode);
                return null;
            }
            return await response.Content.ReadFromJsonAsync<CampaignDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining campaign with token");
            return null;
        }
    }

    /// <summary>
    /// Update the status of a campaign
    /// </summary>
    public async Task<CampaignDto?> UpdateCampaignStatusAsync(int campaignId, CampaignStatus status)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.PatchAsJsonAsync($"api/campaigns/{campaignId}/status", new { Status = status });
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to update status for campaign {CampaignId}", campaignId);
                return null;
            }
            return await response.Content.ReadFromJsonAsync<CampaignDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating status for campaign {CampaignId}", campaignId);
            return null;
        }
    }
}

// Request DTOs
public record UpdateCampaignRequest(
    string Name, 
    string? Description, 
    Visibility Visibility, 
    int MaxPlayers, 
    CampaignStatus Status);

file record InviteTokenResponse(string InviteToken);
