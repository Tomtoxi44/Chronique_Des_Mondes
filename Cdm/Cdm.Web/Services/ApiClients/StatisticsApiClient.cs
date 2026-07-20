using Cdm.Business.Abstraction.DTOs;
using Cdm.Web.Services.Storage;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Cdm.Web.Services.ApiClients;

/// <summary>
/// HTTP client for statistics endpoints.
/// </summary>
public class StatisticsApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<StatisticsApiClient> _logger;
    private readonly ILocalStorageService _localStorage;

    public StatisticsApiClient(HttpClient httpClient, ILogger<StatisticsApiClient> logger, ILocalStorageService localStorage)
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

    /// <summary>Get the current user's dice statistics.</summary>
    public async Task<DiceStatsDto?> GetMyDiceStatsAsync()
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.GetAsync("api/statistics/dice");
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<DiceStatsDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching dice statistics");
            return null;
        }
    }

    /// <summary>Get the current user's participation statistics.</summary>
    public async Task<ParticipationStatsDto?> GetMyParticipationStatsAsync()
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.GetAsync("api/statistics/participation");
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<ParticipationStatsDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching participation statistics");
            return null;
        }
    }
}
