// -----------------------------------------------------------------------
// <copyright file="NotificationHub.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.ApiService.Hubs;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

/// <summary>
/// SignalR hub for real-time notifications.
/// Handles sending notifications to specific users or groups.
/// </summary>
[Authorize]
public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationHub"/> class.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    public NotificationHub(ILogger<NotificationHub> logger)
    {
        this.logger = logger;
    }

    /// <summary>
    /// Called when a user connects. Adds them to their personal notification group.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    public override async Task OnConnectedAsync()
    {
        var userId = this.GetUserId();
        var userGroup = $"user_{userId}";

        await this.Groups.AddToGroupAsync(this.Context.ConnectionId, userGroup);

        this.logger.LogInformation(
            "User {UserId} connected to notification hub",
            userId);

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Sends a notification to a specific user.
    /// </summary>
    /// <param name="targetUserId">The recipient user ID.</param>
    /// <param name="notification">The notification data.</param>
    /// <returns>A task representing the async operation.</returns>
    public async Task SendNotificationToUser(int targetUserId, NotificationData notification)
    {
        var senderId = this.GetUserId();
        var userGroup = $"user_{targetUserId}";

        this.logger.LogInformation(
            "User {SenderId} sending notification to user {TargetUserId}: {Type}",
            senderId,
            targetUserId,
            notification.Type);

        await this.Clients.Group(userGroup).SendAsync("ReceiveNotification", notification);
    }

    /// <summary>
    /// Broadcasts a notification to all members of a campaign.
    /// </summary>
    /// <param name="campaignId">The campaign identifier.</param>
    /// <param name="notification">The notification data.</param>
    /// <returns>A task representing the async operation.</returns>
    public async Task SendNotificationToCampaign(int campaignId, NotificationData notification)
    {
        var senderId = this.GetUserId();
        var campaignGroup = $"campaign_{campaignId}";

        this.logger.LogInformation(
            "User {SenderId} sending notification to campaign {CampaignId}: {Type}",
            senderId,
            campaignId,
            notification.Type);

        await this.Clients.Group(campaignGroup).SendAsync("ReceiveNotification", notification);
    }

    /// <summary>
    /// Joins a campaign notification group to receive campaign-wide notifications.
    /// </summary>
    /// <param name="campaignId">The campaign identifier.</param>
    /// <returns>A task representing the async operation.</returns>
    public async Task JoinCampaignNotifications(int campaignId)
    {
        var userId = this.GetUserId();
        var campaignGroup = $"campaign_{campaignId}";

        await this.Groups.AddToGroupAsync(this.Context.ConnectionId, campaignGroup);

        this.logger.LogInformation(
            "User {UserId} joined campaign {CampaignId} notifications",
            userId,
            campaignId);
    }

    /// <summary>
    /// Leaves a campaign notification group.
    /// </summary>
    /// <param name="campaignId">The campaign identifier.</param>
    /// <returns>A task representing the async operation.</returns>
    public async Task LeaveCampaignNotifications(int campaignId)
    {
        var userId = this.GetUserId();
        var campaignGroup = $"campaign_{campaignId}";

        await this.Groups.RemoveFromGroupAsync(this.Context.ConnectionId, campaignGroup);

        this.logger.LogInformation(
            "User {UserId} left campaign {CampaignId} notifications",
            userId,
            campaignId);
    }

    /// <summary>
    /// Marks a notification as read.
    /// </summary>
    /// <param name="notificationId">The notification identifier.</param>
    /// <returns>A task representing the async operation.</returns>
    public async Task MarkAsRead(int notificationId)
    {
        var userId = this.GetUserId();

        this.logger.LogInformation(
            "User {UserId} marked notification {NotificationId} as read",
            userId,
            notificationId);

        // This would typically update the database
        // For now, just acknowledge the action
        await this.Clients.Caller.SendAsync("NotificationMarkedAsRead", notificationId);
    }

    /// <summary>
    /// Marks all notifications as read for the current user.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    public async Task MarkAllAsRead()
    {
        var userId = this.GetUserId();

        this.logger.LogInformation(
            "User {UserId} marked all notifications as read",
            userId);

        await this.Clients.Caller.SendAsync("AllNotificationsMarkedAsRead");
    }

    /// <summary>
    /// Called when a user disconnects.
    /// </summary>
    /// <param name="exception">Exception if disconnection was caused by error.</param>
    /// <returns>A task representing the async operation.</returns>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = this.GetUserId();

        if (exception != null)
        {
            this.logger.LogWarning(
                exception,
                "User {UserId} disconnected from notification hub with error",
                userId);
        }
        else
        {
            this.logger.LogInformation(
                "User {UserId} disconnected from notification hub",
                userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Gets the current user ID from claims.
    /// </summary>
    /// <returns>The user ID.</returns>
    private int GetUserId()
    {
        var userIdClaim = this.Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out int userId) ? userId : 0;
    }
}

/// <summary>
/// Represents notification data sent through SignalR.
/// </summary>
public class NotificationData
{
    /// <summary>
    /// Gets or sets the notification ID (from database).
    /// </summary>
    public int? Id { get; set; }

    /// <summary>
    /// Gets or sets the notification type.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the notification title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the notification message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the action URL (where to navigate).
    /// </summary>
    public string? ActionUrl { get; set; }

    /// <summary>
    /// Gets or sets the timestamp.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets whether this notification is read.
    /// </summary>
    public bool IsRead { get; set; } = false;

    /// <summary>
    /// Gets or sets the sender user ID.
    /// </summary>
    public int? SenderId { get; set; }

    /// <summary>
    /// Gets or sets the sender name.
    /// </summary>
    public string? SenderName { get; set; }
}
