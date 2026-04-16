// -----------------------------------------------------------------------
// <copyright file="NotificationApiClient.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Web.Services.ApiClients;

using Cdm.Business.Abstraction.DTOs;
using System.Net.Http.Json;

/// <summary>
/// API client for notification operations.
/// </summary>
public class NotificationApiClient
{
    private readonly HttpClient httpClient;
    private readonly ILogger<NotificationApiClient> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationApiClient"/> class.
    /// </summary>
    public NotificationApiClient(HttpClient httpClient, ILogger<NotificationApiClient> logger)
    {
        this.httpClient = httpClient;
        this.logger = logger;
    }

    /// <summary>Gets the current user's notifications.</summary>
    public async Task<List<NotificationDto>> GetNotificationsAsync()
    {
        try
        {
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
            var response = await this.httpClient.DeleteAsync($"/api/notifications/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error deleting notification {Id}", id);
            return false;
        }
    }

    private record UnreadCountResponse(int Count);
}
