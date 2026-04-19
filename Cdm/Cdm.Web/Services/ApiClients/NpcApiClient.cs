// -----------------------------------------------------------------------
// <copyright file="NpcApiClient.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Web.Services.ApiClients;

using Cdm.Business.Abstraction.DTOs;
using Cdm.Web.Services.Storage;
using System.Net.Http.Headers;
using System.Net.Http.Json;

/// <summary>
/// API client for Non-Player Character (NPC) operations.
/// </summary>
public class NpcApiClient
{
    private readonly HttpClient httpClient;
    private readonly ILogger<NpcApiClient> logger;
    private readonly ILocalStorageService localStorage;

    /// <summary>
    /// Initializes a new instance of the <see cref="NpcApiClient"/> class.
    /// </summary>
    public NpcApiClient(HttpClient httpClient, ILogger<NpcApiClient> logger, ILocalStorageService localStorage)
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

    /// <summary>
    /// Gets all NPCs for a specific chapter.
    /// </summary>
    public async Task<List<NpcDto>> GetNpcsByChapterAsync(int chapterId)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await this.httpClient.GetAsync($"/api/npcs/chapter/{chapterId}");
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<List<NpcDto>>() ?? new();

            this.logger.LogWarning("Failed to fetch NPCs for chapter {ChapterId}. Status: {StatusCode}",
                chapterId, response.StatusCode);
            return new();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error fetching NPCs for chapter {ChapterId}", chapterId);
            return new();
        }
    }

    /// <summary>
    /// Creates a new NPC.
    /// </summary>
    public async Task<NpcDto?> CreateNpcAsync(CreateNpcDto createDto)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await this.httpClient.PostAsJsonAsync("/api/npcs", createDto);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<NpcDto>();

            this.logger.LogWarning("Failed to create NPC. Status: {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error creating NPC for chapter {ChapterId}", createDto.ChapterId);
            return null;
        }
    }

    /// <summary>
    /// Updates an existing NPC.
    /// </summary>
    public async Task<NpcDto?> UpdateNpcAsync(int id, CreateNpcDto updateDto)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await this.httpClient.PutAsJsonAsync($"/api/npcs/{id}", updateDto);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<NpcDto>();

            this.logger.LogWarning("Failed to update NPC {NpcId}. Status: {StatusCode}", id, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error updating NPC {NpcId}", id);
            return null;
        }
    }

    /// <summary>
    /// Deletes an NPC.
    /// </summary>
    public async Task<bool> DeleteNpcAsync(int id)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await this.httpClient.DeleteAsync($"/api/npcs/{id}");
            if (response.IsSuccessStatusCode) return true;

            this.logger.LogWarning("Failed to delete NPC {NpcId}. Status: {StatusCode}", id, response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error deleting NPC {NpcId}", id);
            return false;
        }
    }
}
