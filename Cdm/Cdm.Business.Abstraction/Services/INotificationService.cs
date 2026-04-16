// -----------------------------------------------------------------------
// <copyright file="INotificationService.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Abstraction.Services;

using System.Collections.Generic;
using System.Threading.Tasks;
using Cdm.Business.Abstraction.DTOs;

/// <summary>
/// Service for managing user notifications.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Gets all notifications for a user, ordered by creation date descending.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>A list of notifications for the user.</returns>
    Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(int userId);

    /// <summary>
    /// Gets the number of unread notifications for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>The count of unread notifications.</returns>
    Task<int> GetUnreadCountAsync(int userId);

    /// <summary>
    /// Marks a specific notification as read.
    /// </summary>
    /// <param name="notificationId">The notification identifier.</param>
    /// <param name="userId">The user identifier (must be the recipient).</param>
    /// <returns>True if marked successfully, false if not found or unauthorized.</returns>
    Task<bool> MarkAsReadAsync(int notificationId, int userId);

    /// <summary>
    /// Marks all notifications for a user as read.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>The number of notifications marked as read.</returns>
    Task<int> MarkAllAsReadAsync(int userId);

    /// <summary>
    /// Deletes a specific notification.
    /// </summary>
    /// <param name="notificationId">The notification identifier.</param>
    /// <param name="userId">The user identifier (must be the recipient).</param>
    /// <returns>True if deleted successfully, false if not found or unauthorized.</returns>
    Task<bool> DeleteNotificationAsync(int notificationId, int userId);

    /// <summary>
    /// Creates a notification. Intended for internal use by other services.
    /// </summary>
    /// <param name="request">The notification creation request.</param>
    /// <returns>The created notification.</returns>
    Task<NotificationDto?> CreateNotificationAsync(CreateNotificationDto request);
}
