// -----------------------------------------------------------------------
// <copyright file="NotificationApiClient.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Web.Services.ApiClients;

using Cdm.Business.Abstraction.DTOs;
using Cdm.Web.Services.Storage;
using System.Net.Http.Headers;
using System.Net.Http.Json;

/// <summary>
/// API client for notification operations.
/// </summary>
public class NotificationApiClient
{
    private readonly HttpClient httpClient;
    private readonly ILogger<NotificationApiClient> logger;
    private readonly ILocalStorageService localStorage;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationApiClient"/> class.
    /// </summary>
    public NotificationApiClient(HttpClient httpClient, ILogger<NotificationApiClient> logger, ILocalStorageService localStorage)
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

    /// <summary>Gets the current user's notifications.</summary>
    public async Task<List<NotificationDto>> GetNotificationsAsync()
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await this.httpClient.GetAsync("/api/notifications");
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<List<NotificationDto>>() ?? new();
            this.logger.LogWarning("Failed to get notifications. Status: {Status}", response.StatusCode);
            return new();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error getting notifications");
            return new();
        }
    }

    /// <summary>Gets the count of unread notifications.</summary>
    public async Task<int> GetUnreadCountAsync()
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await this.httpClient.GetAsync("/api/notifications/unread-count");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<UnreadCountResponse>();
                return result?.Count ?? 0;
            }
            return 0;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error getting unread notification count");
            return 0;
        }
    }

    /// <summary>Marks a notification as read.</summary>
    public async Task<bool> MarkAsReadAsync(int id)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await this.httpClient.PutAsync($"/api/notifications/{id}/read", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error marking notification {Id} as read", id);
            return false;
        }
    }

    /// <summary>Marks all notifications as read.</summary>
    public async Task<bool> MarkAllAsReadAsync()
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await this.httpClient.PutAsync("/api/notifications/read-all", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error marking all notifications as read");
            return false;
        }
    }

    /// <summary>Deletes a notification.</summary>
    public async Task<bool> DeleteNotificationAsync(int id)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await this.httpClient.DeleteAsync($"/api/notifications/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error deleting notification {Id}", id);
            return false;
        }
    }

    /// <summary>Returns the API base URL for constructing hub URLs.</summary>
    public string GetApiBaseUrl() => this.httpClient.BaseAddress?.ToString().TrimEnd('/') ?? string.Empty;

    private record UnreadCountResponse(int Count);
}
