// -----------------------------------------------------------------------
// <copyright file="NotificationService.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Common.Services;

using Cdm.Business.Abstraction.DTOs;
using Cdm.Business.Abstraction.Services;
using Cdm.Data.Common;
using Cdm.Data.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

/// <summary>
/// Service for managing notification operations.
/// </summary>
/// <param name="dbContext">Database context for notification data access.</param>
/// <param name="logger">Logger instance for structured logging.</param>
public class NotificationService(
    AppDbContext dbContext,
    ILogger<NotificationService> logger) : INotificationService
{
    private readonly AppDbContext dbContext = dbContext;
    private readonly ILogger<NotificationService> logger = logger;

    /// <inheritdoc/>
    public async Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(int userId)
    {
        try
        {
            this.logger.LogInformation("Fetching notifications for user {UserId}", userId);

            var notifications = await this.dbContext.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => MapToDto(n))
                .ToListAsync();

            return notifications;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error fetching notifications for user {UserId}", userId);
            return [];
        }
    }

    /// <inheritdoc/>
    public async Task<int> GetUnreadCountAsync(int userId)
    {
        try
        {
            return await this.dbContext.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error counting unread notifications for user {UserId}", userId);
            return 0;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> MarkAsReadAsync(int notificationId, int userId)
    {
        try
        {
            var notification = await this.dbContext.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification == null)
            {
                this.logger.LogWarning(
                    "Notification {NotificationId} not found for user {UserId}",
                    notificationId, userId);
                return false;
            }

            if (notification.IsRead)
            {
                return true;
            }

            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;

            await this.dbContext.SaveChangesAsync();

            this.logger.LogInformation(
                "Notification {NotificationId} marked as read for user {UserId}",
                notificationId, userId);

            return true;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error marking notification {NotificationId} as read", notificationId);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<int> MarkAllAsReadAsync(int userId)
    {
        try
        {
            var unread = await this.dbContext.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            if (unread.Count == 0)
            {
                return 0;
            }

            var now = DateTime.UtcNow;
            foreach (var notification in unread)
            {
                notification.IsRead = true;
                notification.ReadAt = now;
            }

            await this.dbContext.SaveChangesAsync();

            this.logger.LogInformation(
                "Marked {Count} notifications as read for user {UserId}",
                unread.Count, userId);

            return unread.Count;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error marking all notifications as read for user {UserId}", userId);
            return 0;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteNotificationAsync(int notificationId, int userId)
    {
        try
        {
            var notification = await this.dbContext.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification == null)
            {
                this.logger.LogWarning(
                    "Notification {NotificationId} not found for user {UserId}",
                    notificationId, userId);
                return false;
            }

            this.dbContext.Notifications.Remove(notification);
            await this.dbContext.SaveChangesAsync();

            this.logger.LogInformation(
                "Notification {NotificationId} deleted for user {UserId}",
                notificationId, userId);

            return true;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error deleting notification {NotificationId}", notificationId);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<NotificationDto?> CreateNotificationAsync(CreateNotificationDto request)
    {
        try
        {
            this.logger.LogInformation(
                "Creating notification of type {Type} for user {UserId}",
                request.Type, request.UserId);

            var notification = new Notification
            {
                UserId = request.UserId,
                Type = request.Type,
                Title = request.Title,
                Message = request.Message,
                RelatedEntityId = request.RelatedEntityId,
                RelatedEntityType = request.RelatedEntityType,
                ActionUrl = request.ActionUrl,
                SentBy = request.SentBy,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            this.dbContext.Notifications.Add(notification);
            await this.dbContext.SaveChangesAsync();

            this.logger.LogInformation(
                "Notification {NotificationId} created for user {UserId}",
                notification.Id, request.UserId);

            return MapToDto(notification);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error creating notification for user {UserId}", request.UserId);
            return null;
        }
    }

    private static NotificationDto MapToDto(Notification notification)
    {
        return new NotificationDto
        {
            Id = notification.Id,
            UserId = notification.UserId,
            Type = notification.Type,
            Title = notification.Title,
            Message = notification.Message,
            RelatedEntityId = notification.RelatedEntityId,
            RelatedEntityType = notification.RelatedEntityType,
            ActionUrl = notification.ActionUrl,
            IsRead = notification.IsRead,
            ReadAt = notification.ReadAt,
            CreatedAt = notification.CreatedAt,
            SentBy = notification.SentBy
        };
    }
}
