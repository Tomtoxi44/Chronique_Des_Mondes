// -----------------------------------------------------------------------
// <copyright file="AchievementApiClient.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Web.Services.ApiClients;

using Cdm.Business.Abstraction.DTOs;
using Cdm.Web.Services.Storage;
using System.Net.Http.Headers;
using System.Net.Http.Json;

/// <summary>
/// API client for achievement operations.
/// </summary>
public class AchievementApiClient
{
    private readonly HttpClient httpClient;
    private readonly ILogger<AchievementApiClient> logger;
    private readonly ILocalStorageService localStorage;

    /// <summary>
    /// Initializes a new instance of the <see cref="AchievementApiClient"/> class.
    /// </summary>
    public AchievementApiClient(HttpClient httpClient, ILogger<AchievementApiClient> logger, ILocalStorageService localStorage)
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

    /// <summary>Gets all achievements for a world.</summary>
    public async Task<List<AchievementDto>> GetAchievementsByWorldAsync(int worldId)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await this.httpClient.GetAsync($"/api/achievements/world/{worldId}");
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<List<AchievementDto>>() ?? new();
            this.logger.LogWarning("Failed to get achievements for world {WorldId}. Status: {Status}", worldId, response.StatusCode);
            return new();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error getting achievements for world {WorldId}", worldId);
            return new();
        }
    }

    /// <summary>Gets all achievements for a campaign.</summary>
    public async Task<List<AchievementDto>> GetAchievementsByCampaignAsync(int campaignId)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await this.httpClient.GetAsync($"/api/achievements/campaign/{campaignId}");
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<List<AchievementDto>>() ?? new();
            this.logger.LogWarning("Failed to get achievements for campaign {CampaignId}. Status: {Status}", campaignId, response.StatusCode);
            return new();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error getting achievements for campaign {CampaignId}", campaignId);
            return new();
        }
    }

    /// <summary>Gets all achievements for a chapter.</summary>
    public async Task<List<AchievementDto>> GetAchievementsByChapterAsync(int chapterId)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await this.httpClient.GetAsync($"/api/achievements/chapter/{chapterId}");
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<List<AchievementDto>>() ?? new();
            this.logger.LogWarning("Failed to get achievements for chapter {ChapterId}. Status: {Status}", chapterId, response.StatusCode);
            return new();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error getting achievements for chapter {ChapterId}", chapterId);
            return new();
        }
    }

    /// <summary>Gets an achievement by ID.</summary>
    public async Task<AchievementDto?> GetAchievementByIdAsync(int id)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await this.httpClient.GetAsync($"/api/achievements/{id}");
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<AchievementDto>();
            this.logger.LogWarning("Achievement {AchievementId} not found. Status: {Status}", id, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error getting achievement {AchievementId}", id);
            return null;
        }
    }

    /// <summary>Creates a new achievement.</summary>
    public async Task<AchievementDto?> CreateAchievementAsync(CreateAchievementDto dto)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await this.httpClient.PostAsJsonAsync("/api/achievements", dto);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<AchievementDto>();
            this.logger.LogWarning("Failed to create achievement. Status: {Status}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error creating achievement '{Name}'", dto.Name);
            return null;
        }
    }

    /// <summary>Updates an existing achievement.</summary>
    public async Task<AchievementDto?> UpdateAchievementAsync(int id, CreateAchievementDto dto)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await this.httpClient.PutAsJsonAsync($"/api/achievements/{id}", dto);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<AchievementDto>();
            this.logger.LogWarning("Failed to update achievement {AchievementId}. Status: {Status}", id, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error updating achievement {AchievementId}", id);
            return null;
        }
    }

    /// <summary>Deletes an achievement.</summary>
    public async Task<bool> DeleteAchievementAsync(int id)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await this.httpClient.DeleteAsync($"/api/achievements/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error deleting achievement {AchievementId}", id);
            return false;
        }
    }

    /// <summary>Awards an achievement to a user (GM action).</summary>
    public async Task<UserAchievementDto?> AwardAchievementAsync(int achievementId, int targetUserId, string? message = null)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await this.httpClient.PostAsJsonAsync(
                $"/api/achievements/{achievementId}/award",
                new { TargetUserId = targetUserId, Message = message });
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<UserAchievementDto>();
            this.logger.LogWarning("Failed to award achievement {AchievementId}. Status: {Status}", achievementId, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error awarding achievement {AchievementId}", achievementId);
            return null;
        }
    }

    /// <summary>Gets all achievements of the current user.</summary>
    public async Task<List<UserAchievementDto>> GetMyAchievementsAsync()
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await this.httpClient.GetAsync("/api/achievements/user");
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<List<UserAchievementDto>>() ?? new();
            this.logger.LogWarning("Failed to get user achievements. Status: {Status}", response.StatusCode);
            return new();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error getting user achievements");
            return new();
        }
    }

    /// <summary>Gets achievements earned by a specific user in a world.</summary>
    public async Task<List<UserAchievementDto>> GetUserAchievementsInWorldAsync(int userId, int worldId)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await this.httpClient.GetAsync($"/api/achievements/user/{userId}/world/{worldId}");
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<List<UserAchievementDto>>() ?? new();
            this.logger.LogWarning("Failed to get achievements for user {UserId} in world {WorldId}. Status: {Status}", userId, worldId, response.StatusCode);
            return new();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error getting achievements for user {UserId} in world {WorldId}", userId, worldId);
            return new();
        }
    }

    /// <summary>Revokes a user achievement.</summary>
    public async Task<bool> RevokeAchievementAsync(int userAchievementId)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await this.httpClient.DeleteAsync($"/api/achievements/user/{userAchievementId}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error revoking user achievement {UserAchievementId}", userAchievementId);
            return false;
        }
    }
}
