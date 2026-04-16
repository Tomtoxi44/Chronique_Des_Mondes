// -----------------------------------------------------------------------
// <copyright file="NotificationDto.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Abstraction.DTOs;

using Cdm.Common.Enums;

/// <summary>
/// Data transfer object for a notification.
/// </summary>
public class NotificationDto
{
    /// <summary>
    /// Gets or sets the notification identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the recipient user ID.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the notification type.
    /// </summary>
    public NotificationType Type { get; set; }

    /// <summary>
    /// Gets or sets the notification title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the notification message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the related entity ID.
    /// </summary>
    public int? RelatedEntityId { get; set; }

    /// <summary>
    /// Gets or sets the related entity type.
    /// </summary>
    public string? RelatedEntityType { get; set; }

    /// <summary>
    /// Gets or sets the action URL (where to navigate when clicking).
    /// </summary>
    public string? ActionUrl { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the notification has been read.
    /// </summary>
    public bool IsRead { get; set; }

    /// <summary>
    /// Gets or sets when the notification was read.
    /// </summary>
    public DateTime? ReadAt { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the sender user ID.
    /// </summary>
    public int? SentBy { get; set; }
}
