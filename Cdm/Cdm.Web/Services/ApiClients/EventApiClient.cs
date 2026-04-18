// -----------------------------------------------------------------------
// <copyright file="EventApiClient.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Web.Services.ApiClients;

using Cdm.Business.Abstraction.DTOs;
using Cdm.Web.Services.Storage;
using System.Net.Http.Headers;
using System.Net.Http.Json;

/// <summary>
/// API client for event operations.
/// </summary>
public class EventApiClient
{
    private readonly HttpClient httpClient;
    private readonly ILogger<EventApiClient> logger;
    private readonly ILocalStorageService localStorage;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventApiClient"/> class.
    /// </summary>
    public EventApiClient(HttpClient httpClient, ILogger<EventApiClient> logger, ILocalStorageService localStorage)
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

    /// <summary>Gets all events for a world.</summary>
    public async Task<List<EventDto>> GetEventsByWorldAsync(int worldId)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await this.httpClient.GetAsync($"/api/events/world/{worldId}");
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<List<EventDto>>() ?? new();
            this.logger.LogWarning("Failed to get events for world {WorldId}. Status: {Status}", worldId, response.StatusCode);
            return new();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error getting events for world {WorldId}", worldId);
            return new();
        }
    }

    /// <summary>Gets all events for a campaign.</summary>
    public async Task<List<EventDto>> GetEventsByCampaignAsync(int campaignId)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await this.httpClient.GetAsync($"/api/events/campaign/{campaignId}");
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<List<EventDto>>() ?? new();
            this.logger.LogWarning("Failed to get events for campaign {CampaignId}. Status: {Status}", campaignId, response.StatusCode);
            return new();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error getting events for campaign {CampaignId}", campaignId);
            return new();
        }
    }

    /// <summary>Gets all events for a chapter.</summary>
    public async Task<List<EventDto>> GetEventsByChapterAsync(int chapterId)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await this.httpClient.GetAsync($"/api/events/chapter/{chapterId}");
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<List<EventDto>>() ?? new();
            this.logger.LogWarning("Failed to get events for chapter {ChapterId}. Status: {Status}", chapterId, response.StatusCode);
            return new();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error getting events for chapter {ChapterId}", chapterId);
            return new();
        }
    }

    /// <summary>Gets an event by ID.</summary>
    public async Task<EventDto?> GetEventByIdAsync(int id)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await this.httpClient.GetAsync($"/api/events/{id}");
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<EventDto>();
            this.logger.LogWarning("Event {EventId} not found. Status: {Status}", id, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error getting event {EventId}", id);
            return null;
        }
    }

    /// <summary>Creates a new event.</summary>
    public async Task<EventDto?> CreateEventAsync(CreateEventDto dto)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await this.httpClient.PostAsJsonAsync("/api/events", dto);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<EventDto>();
            this.logger.LogWarning("Failed to create event. Status: {Status}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error creating event '{Name}'", dto.Name);
            return null;
        }
    }

    /// <summary>Updates an existing event.</summary>
    public async Task<EventDto?> UpdateEventAsync(int id, CreateEventDto dto)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await this.httpClient.PutAsJsonAsync($"/api/events/{id}", dto);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<EventDto>();
            this.logger.LogWarning("Failed to update event {EventId}. Status: {Status}", id, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error updating event {EventId}", id);
            return null;
        }
    }

    /// <summary>Deletes an event.</summary>
    public async Task<bool> DeleteEventAsync(int id)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await this.httpClient.DeleteAsync($"/api/events/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error deleting event {EventId}", id);
            return false;
        }
    }

    /// <summary>Toggles the active status of an event.</summary>
    public async Task<EventDto?> SetEventActiveAsync(int id, bool isActive)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await this.httpClient.PutAsJsonAsync($"/api/events/{id}/active", new { IsActive = isActive });
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<EventDto>();
            this.logger.LogWarning("Failed to set active status for event {EventId}. Status: {Status}", id, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error setting active status for event {EventId}", id);
            return null;
        }
    }
}
