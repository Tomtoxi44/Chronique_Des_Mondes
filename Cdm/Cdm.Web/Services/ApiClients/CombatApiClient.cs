// -----------------------------------------------------------------------
// <copyright file="CombatApiClient.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Web.Services.ApiClients;

using Cdm.Business.Abstraction.DTOs;
using Cdm.Web.Services.Storage;
using System.Net.Http.Headers;
using System.Net.Http.Json;

/// <summary>
/// API client for combat-related operations.
/// </summary>
public class CombatApiClient
{
    private readonly HttpClient httpClient;
    private readonly ILogger<CombatApiClient> logger;
    private readonly ILocalStorageService localStorage;

    /// <summary>
    /// Initializes a new instance of the <see cref="CombatApiClient"/> class.
    /// </summary>
    public CombatApiClient(HttpClient httpClient, ILogger<CombatApiClient> logger, ILocalStorageService localStorage)
    {
        this.httpClient = httpClient;
        this.logger = logger;
        this.localStorage = localStorage;
    }

    private async Task AddAuthHeaderAsync()
    {
        var token = await this.localStorage.GetItemAsync("auth_token");
        if (!string.IsNullOrEmpty(token))
            this.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    /// <summary>Returns the API base URL for constructing hub URLs.</summary>
    public string GetApiBaseUrl() => this.httpClient.BaseAddress?.ToString().TrimEnd('/') ?? string.Empty;

    /// <summary>Gets a combat by its ID.</summary>
    public async Task<CombatDto?> GetCombatAsync(int combatId)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await this.httpClient.GetAsync($"api/combat/{combatId}");
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<CombatDto>();

            this.logger.LogWarning("Failed to fetch combat {CombatId}. Status: {StatusCode}", combatId, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error fetching combat {CombatId}", combatId);
            return null;
        }
    }

    /// <summary>Gets the active combat for a session, if any.</summary>
    public async Task<CombatDto?> GetActiveCombatForSessionAsync(int sessionId)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await this.httpClient.GetAsync($"api/combat/session/{sessionId}/active");
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<CombatDto>();

            return null;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error fetching active combat for session {SessionId}", sessionId);
            return null;
        }
    }

    /// <summary>Creates a new combat encounter.</summary>
    public async Task<CombatDto?> CreateCombatAsync(CreateCombatDto dto)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await this.httpClient.PostAsJsonAsync("api/combat", dto);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<CombatDto>();

            this.logger.LogWarning("Failed to create combat. Status: {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error creating combat for session {SessionId}", dto.SessionId);
            return null;
        }
    }

    /// <summary>Starts the initiative phase for a combat.</summary>
    public async Task<CombatDto?> StartInitiativePhaseAsync(int combatId, StartInitiativeDto dto)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await this.httpClient.PostAsJsonAsync($"api/combat/{combatId}/initiative/start", dto);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<CombatDto>();

            this.logger.LogWarning("Failed to start initiative for combat {CombatId}. Status: {StatusCode}", combatId, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error starting initiative phase for combat {CombatId}", combatId);
            return null;
        }
    }

    /// <summary>Sets the initiative value for a participant.</summary>
    public async Task<CombatDto?> SetInitiativeAsync(int combatId, int participantId, SetInitiativeDto dto)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await this.httpClient.PutAsJsonAsync($"api/combat/{combatId}/initiative/{participantId}", dto);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<CombatDto>();

            this.logger.LogWarning("Failed to set initiative for participant {ParticipantId}. Status: {StatusCode}", participantId, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error setting initiative for participant {ParticipantId}", participantId);
            return null;
        }
    }

    /// <summary>Starts the active combat phase with an optional explicit participant order.</summary>
    public async Task<CombatDto?> StartCombatAsync(int combatId, StartCombatDto? dto = null)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await this.httpClient.PostAsJsonAsync($"api/combat/{combatId}/start", dto ?? new StartCombatDto());
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<CombatDto>();

            this.logger.LogWarning("Failed to start combat {CombatId}. Status: {StatusCode}", combatId, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error starting combat {CombatId}", combatId);
            return null;
        }
    }

    /// <summary>Records a combat action.</summary>
    public async Task<CombatActionDto?> RecordActionAsync(int combatId, CreateCombatActionDto dto)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await this.httpClient.PostAsJsonAsync($"api/combat/{combatId}/action", dto);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<CombatActionDto>();

            this.logger.LogWarning("Failed to record action for combat {CombatId}. Status: {StatusCode}", combatId, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error recording action for combat {CombatId}", combatId);
            return null;
        }
    }

    /// <summary>Updates the HP of a participant.</summary>
    public async Task<CombatParticipantDto?> UpdateHpAsync(int combatId, int participantId, UpdateHpDto dto)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await this.httpClient.PutAsJsonAsync($"api/combat/{combatId}/hp/{participantId}", dto);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<CombatParticipantDto>();

            this.logger.LogWarning("Failed to update HP for participant {ParticipantId}. Status: {StatusCode}", participantId, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error updating HP for participant {ParticipantId}", participantId);
            return null;
        }
    }

    /// <summary>Advances the combat to the next turn.</summary>
    public async Task<CombatDto?> NextTurnAsync(int combatId)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await this.httpClient.PostAsync($"api/combat/{combatId}/next-turn", null);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<CombatDto>();

            this.logger.LogWarning("Failed to advance turn for combat {CombatId}. Status: {StatusCode}", combatId, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error advancing turn for combat {CombatId}", combatId);
            return null;
        }
    }

    /// <summary>Toggles a participant's active status (for generic mode — no HP tracking).</summary>
    public async Task<CombatDto?> ToggleParticipantActiveAsync(int combatId, int participantId, bool isActive)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await this.httpClient.PutAsJsonAsync(
                $"api/combat/{combatId}/participants/{participantId}/active",
                new SetActiveDto { IsActive = isActive });
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<CombatDto>();

            this.logger.LogWarning("Failed to toggle participant {ParticipantId} active. Status: {StatusCode}", participantId, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error toggling participant {ParticipantId} active status", participantId);
            return null;
        }
    }

    /// <summary>Ends a combat encounter.</summary>
    public async Task<CombatDto?> EndCombatAsync(int combatId, EndCombatDto dto)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await this.httpClient.PostAsJsonAsync($"api/combat/{combatId}/end", dto);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<CombatDto>();

            this.logger.LogWarning("Failed to end combat {CombatId}. Status: {StatusCode}", combatId, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error ending combat {CombatId}", combatId);
            return null;
        }
    }
}
